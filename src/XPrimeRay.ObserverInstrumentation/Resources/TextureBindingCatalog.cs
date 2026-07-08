using System.Collections.Generic;

namespace XPrimeRay.ObserverInstrumentation.Resources;

// Keyed by (colliderName, slotId.Name) to support multiple texture slots per collider.
public sealed class TextureBindingCatalog
{
    private readonly Dictionary<(string, string), TextureResourceBinding> _bindings;

    private TextureBindingCatalog(Dictionary<(string, string), TextureResourceBinding> bindings) =>
        _bindings = bindings;

    public static bool TryCreate(
        IEnumerable<(string colliderName, TextureResourceBinding binding)>? bindings,
        out TextureBindingCatalog? catalog,
        out string? errorReason)
    {
        catalog = null;
        errorReason = null;
        if (bindings == null) { errorReason = "null bindings enumerable"; return false; }

        var dict = new Dictionary<(string, string), TextureResourceBinding>();
        foreach ((string name, TextureResourceBinding binding) in bindings)
        {
            if (string.IsNullOrEmpty(name))
            { errorReason = "blank collider name"; return false; }
            if (binding == null || !binding.IsValid)
            { errorReason = $"invalid binding for collider '{name}'"; return false; }
            var key = (name, binding.SlotId.Name);
            if (dict.ContainsKey(key))
            { errorReason = $"duplicate (colliderName='{name}', slotId='{binding.SlotId.Name}')"; return false; }
            dict[key] = binding;
        }

        catalog = new TextureBindingCatalog(dict);
        return true;
    }

    public bool TryGetBinding(string colliderName, TextureSlotId slotId, out TextureResourceBinding? binding) =>
        _bindings.TryGetValue((colliderName, slotId.Name), out binding!);

    public int Count => _bindings.Count;
}
