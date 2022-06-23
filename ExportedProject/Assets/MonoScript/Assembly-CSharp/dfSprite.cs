using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[AddComponentMenu("Daikon Forge/User Interface/Sprite/Basic")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_sprite.html")]
[dfTooltip("Used to render a sprite from a Texture Atlas on the screen")]
public class dfSprite : dfControl
{
	internal struct RenderOptions
	{
		public dfAtlas atlas;

		public dfAtlas.ItemInfo spriteInfo;

		public Color32 color;

		public float pixelsToUnits;

		public Vector2 size;

		public dfSpriteFlip flip;

		public bool invertFill;

		public dfFillDirection fillDirection;

		public float fillAmount;

		public Vector3 offset;

		public int baseIndex;
	}

	private static int[] TRIANGLE_INDICES = new int[6] { 0, 1, 3, 3, 1, 2 };

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string spriteName;

	[SerializeField]
	protected dfSpriteFlip flip;

	[SerializeField]
	protected dfFillDirection fillDirection;

	[SerializeField]
	protected float fillAmount = 1f;

	[SerializeField]
	protected bool invertFill;

	[NonSerialized]
	public Material OverrideMaterial;

	public dfAtlas Atlas
	{
		get
		{
			if (atlas == null)
			{
				dfGUIManager manager = GetManager();
				if (manager != null)
				{
					return atlas = manager.DefaultAtlas;
				}
			}
			return atlas;
		}
		set
		{
			if (!dfAtlas.Equals(value, atlas))
			{
				atlas = value;
				Invalidate();
			}
		}
	}

	public string SpriteName
	{
		get
		{
			return spriteName;
		}
		set
		{
			value = getLocalizedValue(value);
			if (value != spriteName)
			{
				spriteName = value;
				dfAtlas.ItemInfo spriteInfo = SpriteInfo;
				if (size == Vector2.zero && spriteInfo != null)
				{
					size = spriteInfo.sizeInPixels;
					updateCollider();
				}
				Invalidate();
				OnSpriteNameChanged(value);
			}
		}
	}

	public dfAtlas.ItemInfo SpriteInfo
	{
		get
		{
			dfAtlas.ItemInfo itemInfo = null;
			if (Atlas == null)
			{
				return null;
			}
			return Atlas[spriteName];
		}
	}

	public dfSpriteFlip Flip
	{
		get
		{
			return flip;
		}
		set
		{
			if (value != flip)
			{
				flip = value;
				Invalidate();
			}
		}
	}

	public dfFillDirection FillDirection
	{
		get
		{
			return fillDirection;
		}
		set
		{
			if (value != fillDirection)
			{
				fillDirection = value;
				Invalidate();
			}
		}
	}

	public float FillAmount
	{
		get
		{
			return fillAmount;
		}
		set
		{
			if (!Mathf.Approximately(value, fillAmount))
			{
				fillAmount = Mathf.Max(0f, Mathf.Min(1f, value));
				Invalidate();
			}
		}
	}

	public bool InvertFill
	{
		get
		{
			return invertFill;
		}
		set
		{
			if (value != invertFill)
			{
				invertFill = value;
				Invalidate();
			}
		}
	}

	public event PropertyChangedEventHandler<string> SpriteNameChanged;

	protected internal override void OnLocalize()
	{
		base.OnLocalize();
		SpriteName = getLocalizedValue(spriteName);
	}

	protected internal virtual void OnSpriteNameChanged(string value)
	{
		Signal("OnSpriteNameChanged", this, value);
		if (this.SpriteNameChanged != null)
		{
			this.SpriteNameChanged(this, value);
		}
	}

	public override Vector2 CalculateMinimumSize()
	{
		dfAtlas.ItemInfo spriteInfo = SpriteInfo;
		if (spriteInfo == null)
		{
			return Vector2.zero;
		}
		RectOffset border = spriteInfo.border;
		if (border != null && border.horizontal > 0 && border.vertical > 0)
		{
			return Vector2.Max(base.CalculateMinimumSize(), new Vector2(border.horizontal, border.vertical));
		}
		return base.CalculateMinimumSize();
	}

	protected override void OnRebuildRenderData()
	{
		if (Atlas != null && Atlas.Material != null && !(SpriteInfo == null))
		{
			renderData.Material = OverrideMaterial ?? Atlas.Material;
			Color32 color = ApplyOpacity((!base.IsEnabled) ? disabledColor : base.color);
			RenderOptions renderOptions = default(RenderOptions);
			renderOptions.atlas = Atlas;
			renderOptions.color = color;
			renderOptions.fillAmount = fillAmount;
			renderOptions.fillDirection = fillDirection;
			renderOptions.flip = flip;
			renderOptions.invertFill = invertFill;
			renderOptions.offset = pivot.TransformToUpperLeft(base.Size);
			renderOptions.pixelsToUnits = PixelsToUnits();
			renderOptions.size = base.Size;
			renderOptions.spriteInfo = SpriteInfo;
			RenderOptions options = renderOptions;
			renderSprite(renderData, options);
		}
	}

	internal static void renderSprite(dfRenderData data, RenderOptions options)
	{
		if (!(options.fillAmount <= float.Epsilon))
		{
			options.baseIndex = data.Vertices.Count;
			rebuildTriangles(data, options);
			rebuildVertices(data, options);
			rebuildUV(data, options);
			rebuildColors(data, options);
			if (options.fillAmount < 1f)
			{
				doFill(data, options);
			}
		}
	}

	private static void rebuildTriangles(dfRenderData renderData, RenderOptions options)
	{
		int baseIndex = options.baseIndex;
		dfList<int> triangles = renderData.Triangles;
		triangles.EnsureCapacity(triangles.Count + TRIANGLE_INDICES.Length);
		for (int i = 0; i < TRIANGLE_INDICES.Length; i++)
		{
			triangles.Add(baseIndex + TRIANGLE_INDICES[i]);
		}
	}

	private static void rebuildVertices(dfRenderData renderData, RenderOptions options)
	{
		dfList<Vector3> vertices = renderData.Vertices;
		int baseIndex = options.baseIndex;
		float x = 0f;
		float y = 0f;
		float x2 = Mathf.Ceil(options.size.x);
		float y2 = Mathf.Ceil(0f - options.size.y);
		vertices.Add(new Vector3(x, y, 0f) * options.pixelsToUnits);
		vertices.Add(new Vector3(x2, y, 0f) * options.pixelsToUnits);
		vertices.Add(new Vector3(x2, y2, 0f) * options.pixelsToUnits);
		vertices.Add(new Vector3(x, y2, 0f) * options.pixelsToUnits);
		Vector3 vector = options.offset.RoundToInt() * options.pixelsToUnits;
		Vector3[] items = vertices.Items;
		for (int i = baseIndex; i < baseIndex + 4; i++)
		{
			items[i] = (items[i] + vector).Quantize(options.pixelsToUnits);
		}
	}

	private static void rebuildColors(dfRenderData renderData, RenderOptions options)
	{
		dfList<Color32> colors = renderData.Colors;
		colors.Add(options.color);
		colors.Add(options.color);
		colors.Add(options.color);
		colors.Add(options.color);
	}

	private static void rebuildUV(dfRenderData renderData, RenderOptions options)
	{
		Rect region = options.spriteInfo.region;
		dfList<Vector2> uV = renderData.UV;
		uV.Add(new Vector2(region.x, region.yMax));
		uV.Add(new Vector2(region.xMax, region.yMax));
		uV.Add(new Vector2(region.xMax, region.y));
		uV.Add(new Vector2(region.x, region.y));
		Vector2 zero = Vector2.zero;
		if (options.flip.IsSet(dfSpriteFlip.FlipHorizontal))
		{
			zero = uV[1];
			uV[1] = uV[0];
			uV[0] = zero;
			zero = uV[3];
			uV[3] = uV[2];
			uV[2] = zero;
		}
		if (options.flip.IsSet(dfSpriteFlip.FlipVertical))
		{
			zero = uV[0];
			uV[0] = uV[3];
			uV[3] = zero;
			zero = uV[1];
			uV[1] = uV[2];
			uV[2] = zero;
		}
	}

	private static void doFill(dfRenderData renderData, RenderOptions options)
	{
		int baseIndex = options.baseIndex;
		dfList<Vector3> vertices = renderData.Vertices;
		dfList<Vector2> uV = renderData.UV;
		int index = baseIndex;
		int index2 = baseIndex + 1;
		int index3 = baseIndex + 3;
		int index4 = baseIndex + 2;
		if (options.invertFill)
		{
			if (options.fillDirection == dfFillDirection.Horizontal)
			{
				index = baseIndex + 1;
				index2 = baseIndex;
				index3 = baseIndex + 2;
				index4 = baseIndex + 3;
			}
			else
			{
				index = baseIndex + 3;
				index2 = baseIndex + 2;
				index3 = baseIndex;
				index4 = baseIndex + 1;
			}
		}
		if (options.fillDirection == dfFillDirection.Horizontal)
		{
			vertices[index2] = Vector3.Lerp(vertices[index2], vertices[index], 1f - options.fillAmount);
			vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index3], 1f - options.fillAmount);
			uV[index2] = Vector2.Lerp(uV[index2], uV[index], 1f - options.fillAmount);
			uV[index4] = Vector2.Lerp(uV[index4], uV[index3], 1f - options.fillAmount);
		}
		else
		{
			vertices[index3] = Vector3.Lerp(vertices[index3], vertices[index], 1f - options.fillAmount);
			vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index2], 1f - options.fillAmount);
			uV[index3] = Vector2.Lerp(uV[index3], uV[index], 1f - options.fillAmount);
			uV[index4] = Vector2.Lerp(uV[index4], uV[index2], 1f - options.fillAmount);
		}
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(spriteName))
		{
			return string.Format("{0} ({1})", base.name, spriteName);
		}
		return base.ToString();
	}
}
