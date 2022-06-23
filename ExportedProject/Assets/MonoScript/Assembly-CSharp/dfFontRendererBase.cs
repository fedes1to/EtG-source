using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class dfFontRendererBase : IDisposable
{
	public dfFontBase Font { get; protected set; }

	public Vector2 MaxSize { get; set; }

	public float PixelRatio { get; set; }

	public float TextScale { get; set; }

	public int CharacterSpacing { get; set; }

	public Vector3 VectorOffset { get; set; }

	public Vector3 PerCharacterAccumulatedOffset { get; set; }

	public bool ProcessMarkup { get; set; }

	public bool WordWrap { get; set; }

	public bool MultiLine { get; set; }

	public bool OverrideMarkupColors { get; set; }

	public bool ColorizeSymbols { get; set; }

	public TextAlignment TextAlign { get; set; }

	public Color32 DefaultColor { get; set; }

	public Color32? BottomColor { get; set; }

	public float Opacity { get; set; }

	public bool Outline { get; set; }

	public int OutlineSize { get; set; }

	public Color32 OutlineColor { get; set; }

	public bool Shadow { get; set; }

	public Color32 ShadowColor { get; set; }

	public Vector2 ShadowOffset { get; set; }

	public int TabSize { get; set; }

	public List<int> TabStops { get; set; }

	public Vector2 RenderedSize { get; internal set; }

	public int LinesRendered { get; internal set; }

	public abstract void Release();

	public abstract float[] GetCharacterWidths(string text);

	public abstract Vector2 MeasureString(string text);

	public abstract void Render(string text, dfRenderData destination);

	protected virtual void Reset()
	{
		Font = null;
		PixelRatio = 0f;
		TextScale = 1f;
		CharacterSpacing = 0;
		VectorOffset = Vector3.zero;
		PerCharacterAccumulatedOffset = Vector3.zero;
		ProcessMarkup = false;
		WordWrap = false;
		MultiLine = false;
		OverrideMarkupColors = false;
		ColorizeSymbols = false;
		TextAlign = TextAlignment.Left;
		DefaultColor = Color.white;
		BottomColor = null;
		Opacity = 1f;
		Outline = false;
		Shadow = false;
	}

	public void Dispose()
	{
		Release();
	}
}
