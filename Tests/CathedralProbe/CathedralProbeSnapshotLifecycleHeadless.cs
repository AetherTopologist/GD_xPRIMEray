using Godot;

public partial class CathedralProbeSnapshotLifecycleHeadless : Node
{
	private const int TimeoutFrames = 360;
	private GrinFilmCamera _film;
	private int _frames;
	private bool _requested;
	private bool _finished;

	public override void _Ready()
	{
		_film = GetNodeOrNull<GrinFilmCamera>("GrinFilmCamera");
		if (_film == null)
		{
			Finish(false, "missing GrinFilmCamera");
			return;
		}

		if (!_film.TryRequestCathedralProbeSnapshot(240, out ProbeSnapshotLifecycleResult request))
		{
			Finish(false, $"request rejected state={request.State} reason={request.Reason}");
			return;
		}
		_requested = true;
	}

	public override void _Process(double delta)
	{
		if (_finished || !_requested || _film == null)
		{
			return;
		}

		_frames++;
		if (!_film.TryGetCathedralProbeSnapshotLifecycleResult(out ProbeSnapshotLifecycleResult result))
		{
			if (_frames >= TimeoutFrames)
			{
				Finish(false, "timeout without lifecycle result");
			}
			return;
		}

		if (!result.IsTerminal)
		{
			if (_frames >= TimeoutFrames)
			{
				Finish(false, $"timeout state={result.State} reason={result.Reason}");
			}
			return;
		}

		bool pass = result.State == ProbeSnapshotLifecycleState.Complete &&
			result.UnprocessedPixelCount == 0 &&
			result.ProcessedPixelCount == result.TotalPixelCount &&
			result.HistogramReconciles() &&
			result.RegionAnalysisAvailable;
		Finish(
			pass,
			$"state={result.State} reason={result.Reason} processed={result.ProcessedPixelCount}/{result.TotalPixelCount} " +
			$"unprocessed={result.UnprocessedPixelCount} regions={result.RegionCount} selectable={result.SelectableRegionCount}");
	}

	private void Finish(bool pass, string message)
	{
		_finished = true;
		GD.Print($"CATHEDRAL SNAPSHOT HARNESS {(pass ? "PASS" : "FAIL")} {message}");
		GetTree().Quit(pass ? 0 : 1);
	}
}
