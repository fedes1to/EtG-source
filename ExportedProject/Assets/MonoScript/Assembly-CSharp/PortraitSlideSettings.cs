using System;
using UnityEngine;

[Serializable]
public class PortraitSlideSettings
{
	[StringTableString("enemies")]
	public string bossNameString;

	[StringTableString("enemies")]
	public string bossSubtitleString;

	[StringTableString("enemies")]
	public string bossQuoteString;

	public Texture bossArtSprite;

	public IntVector2 bossSpritePxOffset;

	public IntVector2 topLeftTextPxOffset;

	public IntVector2 bottomRightTextPxOffset;

	public Color bgColor = Color.blue;
}
