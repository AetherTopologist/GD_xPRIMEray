using Godot;
using System;

/// <summary>
/// Canvas-layer HUD that cycles researcher quotes as the transport demo runs.
/// Place as a CanvasLayer child in the fixture scene; it self-creates its label.
/// </summary>
public partial class WormholeTransportHUD : CanvasLayer
{
	[Export] public float SecondsPerQuote = 5.0f;
	[Export] public float FadeDuration = 0.8f;
	[Export] public bool AutoCycle = true;

	private static readonly (string speaker, string text)[] Quotes =
	{
		("Puthoff",
			"The vacuum is not empty — it is the seat of\nthe most violent physics imaginable."),
		("Davis & Froning",
			"Warp bubble topology requires a negative energy\ndensity throat to remain traversable."),
		("Cramer",
			"The transaction handshake is non-local;\nthe offer wave shortcuts Euclidean path."),
		("Miley",
			"Multiple plasma toroids provide mutual phase\ncoherence for stable transport."),
	};

	private RichTextLabel _label;
	private int _quoteIndex;
	private double _timer;
	private double _fadeTimer;
	private bool _fadingIn = true;

	public override void _Ready()
	{
		_label = new RichTextLabel();
		_label.BbcodeEnabled = true;
		_label.FitContent = true;
		_label.MouseFilter = Control.MouseFilterEnum.Ignore;
		_label.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
		_label.Position = new Vector2(28, -120);
		_label.Size = new Vector2(520, 100);
		_label.Modulate = new Color(1, 1, 1, 0);
		AddChild(_label);

		ShowQuote(_quoteIndex);
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (!AutoCycle)
		{
			return;
		}

		_fadeTimer += delta;
		float alpha = _label.Modulate.A;

		if (_fadingIn)
		{
			alpha = Mathf.Clamp((float)(_fadeTimer / FadeDuration), 0f, 1f);
			_label.Modulate = new Color(1, 1, 1, alpha);
			if (_fadeTimer >= FadeDuration)
			{
				_fadeTimer = 0;
				_fadingIn = false;
			}
			return;
		}

		_timer += delta;
		if (_timer < SecondsPerQuote)
		{
			return;
		}

		// Start fade-out then advance quote
		alpha = Mathf.Clamp(1f - (float)((_timer - SecondsPerQuote) / FadeDuration), 0f, 1f);
		_label.Modulate = new Color(1, 1, 1, alpha);

		if (_timer >= SecondsPerQuote + FadeDuration)
		{
			_timer = 0;
			_fadeTimer = 0;
			_fadingIn = true;
			_quoteIndex = (_quoteIndex + 1) % Quotes.Length;
			ShowQuote(_quoteIndex);
		}
	}

	// Allow external code (e.g. demo controller) to jump to a specific quote index.
	public void SetQuoteByIndex(int index)
	{
		_quoteIndex = ((index % Quotes.Length) + Quotes.Length) % Quotes.Length;
		_timer = 0;
		_fadeTimer = 0;
		_fadingIn = true;
		ShowQuote(_quoteIndex);
	}

	private void ShowQuote(int idx)
	{
		if (_label == null)
		{
			return;
		}

		(string speaker, string text) = Quotes[idx];
		_label.Text = $"[color=#aaddff][b]— {speaker}[/b][/color]\n[color=#ddeeff][i]{text}[/i][/color]";
	}
}
