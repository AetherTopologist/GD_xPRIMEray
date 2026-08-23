#!/usr/bin/env bash
set -u

# External watchdog: it owns no measurement data and remains alive while the
# child process fails. Usage: runtime_health_watchdog.sh --output DIR -- command args...
out="${TMPDIR:-/tmp}/xprimeray-runtime-health"
interval=1
while [[ $# -gt 0 ]]; do
  case "$1" in
    --output) out="$2"; shift 2;;
    --interval) interval="$2"; shift 2;;
    --) shift; break;;
    *) echo "usage: $0 [--output DIR] [--interval SEC] -- command" >&2; exit 2;;
  esac
done
[[ $# -gt 0 ]] || { echo "missing command" >&2; exit 2; }
mkdir -p "$out"
log="$out/host_health.ndjson"
run_id="$(date -u +%Y%m%dT%H%M%S)-$$"
now(){ date -u +%Y-%m-%dT%H:%M:%S.%3NZ; }
json(){ printf '%s\n' "$1" >> "$log"; }
json "{\"timestamp_utc\":\"$(now)\",\"event\":\"RUN_START\",\"run_id\":\"$run_id\",\"status\":\"started\"}"
"$@" & child=$!
while kill -0 "$child" 2>/dev/null; do
  rss=""; cpu=""; cpu_pct=""; avail=""; commit=""; gpu="null"
  if [[ -r "/proc/$child/status" ]]; then
    rss="$(awk '/VmRSS:/ {print $2*1024; exit}' "/proc/$child/status" 2>/dev/null || true)"
    cpu="$(awk '{print $14+$15}' "/proc/$child/stat" 2>/dev/null || true)"
    cpu_pct="$(ps -p "$child" -o %cpu= 2>/dev/null | awk '{print $1}' || true)"
  fi
  avail="$(awk '/MemAvailable:/ {print $2*1024; exit}' /proc/meminfo 2>/dev/null || true)"
  commit="$(awk '/CommitLimit:/ {limit=$2} /Committed_AS:/ {used=$2} END {print used*1024 ":" limit*1024}' /proc/meminfo 2>/dev/null || true)"
  if command -v powershell.exe >/dev/null 2>&1; then
    gpu="$(powershell.exe -NoProfile -Command "\$u=(Get-Counter '\\GPU Engine(*)\\Utilization Percentage' -ErrorAction SilentlyContinue).CounterSamples | Measure-Object -Property CookedValue -Sum; \$m=(Get-Counter '\\GPU Process Memory(*)\\Local Usage' -ErrorAction SilentlyContinue).CounterSamples | Measure-Object -Property CookedValue -Sum; [pscustomobject]@{utilization_percent=\$u.Sum; memory_bytes=\$m.Sum} | ConvertTo-Json -Compress" 2>/dev/null | tr -d '\r\n' || true)"
    [[ -z "$gpu" ]] && gpu="null"
  fi
  json "{\"timestamp_utc\":\"$(now)\",\"event\":\"heartbeat\",\"run_id\":\"$run_id\",\"pid\":$child,\"process_rss_bytes\":${rss:-null},\"process_cpu_ticks\":${cpu:-null},\"process_cpu_percent\":${cpu_pct:-null},\"host_load_1m\":\"$(awk '{print $1}' /proc/loadavg 2>/dev/null || true)\",\"available_memory_bytes\":${avail:-null},\"committed_memory\":\"${commit}\",\"gpu\":$gpu}"
  sleep "$interval"
done
wait "$child"; rc=$?
if [[ $rc -eq 0 ]]; then
  json "{\"timestamp_utc\":\"$(now)\",\"event\":\"RUN_COMPLETE\",\"run_id\":\"$run_id\",\"status\":\"complete\",\"exit_code\":0}"
else
  json "{\"timestamp_utc\":\"$(now)\",\"event\":\"RUN_ABORT\",\"run_id\":\"$run_id\",\"status\":\"failed\",\"exit_code\":$rc}"
fi
exit "$rc"
