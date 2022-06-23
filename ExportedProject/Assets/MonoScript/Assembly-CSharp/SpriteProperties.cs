using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Sprites/Sprite Properties")]
public class SpriteProperties : MonoBehaviour
{
	public Texture2D blankTexture;

	public dfSprite sprite;

	public bool ShowBorders { get; set; }

	public float PatternScaleX
	{
		get
		{
			return ((dfTiledSprite)sprite).TileScale.x;
		}
		set
		{
			dfTiledSprite dfTiledSprite2 = sprite as dfTiledSprite;
			dfTiledSprite2.TileScale = new Vector2(value, dfTiledSprite2.TileScale.y);
		}
	}

	public float PatternScaleY
	{
		get
		{
			return ((dfTiledSprite)sprite).TileScale.y;
		}
		set
		{
			dfTiledSprite dfTiledSprite2 = sprite as dfTiledSprite;
			dfTiledSprite2.TileScale = new Vector2(dfTiledSprite2.TileScale.x, value);
		}
	}

	public float PatternOffsetX
	{
		get
		{
			return ((dfTiledSprite)sprite).TileScroll.x;
		}
		set
		{
			dfTiledSprite dfTiledSprite2 = sprite as dfTiledSprite;
			dfTiledSprite2.TileScroll = new Vector2(value, dfTiledSprite2.TileScroll.y);
		}
	}

	public float PatternOffsetY
	{
		get
		{
			return ((dfTiledSprite)sprite).TileScroll.y;
		}
		set
		{
			dfTiledSprite dfTiledSprite2 = sprite as dfTiledSprite;
			dfTiledSprite2.TileScroll = new Vector2(dfTiledSprite2.TileScroll.x, value);
		}
	}

	public int FillOrigin
	{
		get
		{
			return (int)((dfRadialSprite)sprite).FillOrigin;
		}
		set
		{
			((dfRadialSprite)sprite).FillOrigin = (dfPivotPoint)value;
		}
	}

	public bool FillVertical
	{
		get
		{
			return sprite.FillDirection == dfFillDirection.Vertical;
		}
		set
		{
			if (value)
			{
				sprite.FillDirection = dfFillDirection.Vertical;
			}
			else
			{
				sprite.FillDirection = dfFillDirection.Horizontal;
			}
		}
	}

	public bool FlipHorizontal
	{
		get
		{
			return sprite.Flip.IsSet(dfSpriteFlip.FlipHorizontal);
		}
		set
		{
			sprite.Flip = sprite.Flip.SetFlag(dfSpriteFlip.FlipHorizontal, value);
		}
	}

	public bool FlipVertical
	{
		get
		{
			return sprite.Flip.IsSet(dfSpriteFlip.FlipVertical);
		}
		set
		{
			sprite.Flip = sprite.Flip.SetFlag(dfSpriteFlip.FlipVertical, value);
		}
	}

	private void Awake()
	{
		if (sprite == null)
		{
			sprite = GetComponent<dfSprite>();
		}
		ShowBorders = true;
		base.useGUILayout = false;
	}

	private void Start()
	{
	}

	private void OnGUI()
	{
		if (ShowBorders && !(blankTexture == null))
		{
			Rect screenRect = sprite.GetScreenRect();
			RectOffset border = sprite.SpriteInfo.border;
			float x = screenRect.x;
			float y = screenRect.y;
			float width = screenRect.width;
			float height = screenRect.height;
			int num = border.left;
			int num2 = border.right;
			int num3 = border.top;
			int num4 = border.bottom;
			if (sprite.Flip.IsSet(dfSpriteFlip.FlipHorizontal))
			{
				num = border.right;
				num2 = border.left;
			}
			if (sprite.Flip.IsSet(dfSpriteFlip.FlipVertical))
			{
				num3 = border.bottom;
				num4 = border.top;
			}
			GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			GUI.DrawTexture(new Rect(x - 10f, y + (float)num3, width + 20f, 1f), blankTexture);
			GUI.DrawTexture(new Rect(x - 10f, y + height - (float)num4, width + 20f, 1f), blankTexture);
			GUI.DrawTexture(new Rect(x + (float)num, y - 10f, 1f, height + 20f), blankTexture);
			GUI.DrawTexture(new Rect(x + width - (float)num2, y - 10f, 1f, height + 20f), blankTexture);
		}
	}
}
