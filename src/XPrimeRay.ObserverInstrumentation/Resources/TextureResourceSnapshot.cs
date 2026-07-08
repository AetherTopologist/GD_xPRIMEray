using System.Collections.Generic;

namespace XPrimeRay.ObserverInstrumentation.Resources;

public sealed class TextureResourceSnapshot
{
    private readonly Dictionary<string, TextureSnapshot> _textures;

    private TextureResourceSnapshot(Dictionary<string, TextureSnapshot> textures) =>
        _textures = textures;

    public static readonly TextureResourceSnapshot Empty =
        new(new Dictionary<string, TextureSnapshot>());

    public static bool TryBuild(
        IEnumerable<TextureSnapshot>? snapshots,
        out TextureResourceSnapshot? resource,
        out string? errorReason)
    {
        resource = null;
        errorReason = null;
        if (snapshots == null) { errorReason = "null snapshots enumerable"; return false; }

        var dict = new Dictionary<string, TextureSnapshot>(StringComparer.Ordinal);
        foreach (TextureSnapshot snap in snapshots)
        {
            if (snap == null)
            { errorReason = "null snapshot in enumerable"; return false; }
            if (!snap.AssetId.IsValid)
            { errorReason = "snapshot has invalid AssetId"; return false; }
            string tag = snap.AssetId.Tag;
            if (dict.ContainsKey(tag))
            { errorReason = $"duplicate AssetId tag '{tag}'"; return false; }
            dict[tag] = snap;
        }

        resource = new TextureResourceSnapshot(dict);
        return true;
    }

    public bool TryGetTexture(TextureAssetId assetId, out TextureSnapshot? texture)
    {
        texture = null;
        if (!assetId.IsValid) return false;
        return _textures.TryGetValue(assetId.Tag, out texture!);
    }

    public int Count => _textures.Count;
}
