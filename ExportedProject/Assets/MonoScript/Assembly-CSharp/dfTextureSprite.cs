using System;
using UnityEngine;

[Serializable]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_texture_sprite.html")]
[dfCategory("Basic Controls")]
[dfTooltip("Implements a Sprite that allows the user to use any Texture and Material they wish without having to use a Texture Atlas")]
[AddComponentMenu("Daikon Forge/User Interface/Sprite/Texture")]
[ExecuteInEditMode]
public class dfTextureSprite : dfControl
{
	private static int[] TRIANGLE_INDICES = new int[6] { 0, 1, 3, 3, 1, 2 };

	[SerializeField]
	protected Texture texture;

	[SerializeField]
	protected Material material;

	[SerializeField]
	protected dfSpriteFlip flip;

	[SerializeField]
	protected dfFillDirection fillDirection;

	[SerializeField]
	protected float fillAmount = 1f;

	[SerializeField]
	protected bool invertFill;

	[SerializeField]
	protected Rect cropRect = new Rect(0f, 0f, 1f, 1f);

	[SerializeField]
	protected bool cropImage;

	private bool createdRuntimeMaterial;

	private Material renderMaterial;

	[NonSerialized]
	public Material OverrideMaterial;

	public bool CropTexture
	{
		get
		{
			return cropImage;
		}
		set
		{
			if (value != cropImage)
			{
				cropImage = value;
				Invalidate();
			}
		}
	}

	public Rect CropRect
	{
		get
		{
			return cropRect;
		}
		set
		{
			value = validateCropRect(value);
			if (value != cropRect)
			{
				cropRect = value;
				Invalidate();
			}
		}
	}

	public Texture Texture
	{
		get
		{
			return texture;
		}
		set
		{
			if (value != texture)
			{
				texture = value;
				Invalidate();
				if (value != null && size.sqrMagnitude <= float.Epsilon)
				{
					size = new Vector2(value.width, value.height);
				}
				OnTextureChanged(value);
			}
		}
	}

	public Material Material
	{
		get
		{
			return material;
		}
		set
		{
			if (value != material)
			{
				disposeCreatedMaterial();
				renderMaterial = null;
				material = value;
				Invalidate();
			}
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

	public Material RenderMaterial
	{
		get
		{
			return renderMaterial;
		}
	}

	public event PropertyChangedEventHandler<Texture> TextureChanged;

	public override void OnEnable()
	{
		base.OnEnable();
		renderMaterial = null;
	}

	public override void OnDestroy()
	{
		disposeCreatedMaterial();
		base.OnDestroy();
		if (renderMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(renderMaterial);
			renderMaterial = null;
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying && renderMaterial != null)
		{
			disposeCreatedMaterial();
			UnityEngine.Object.DestroyImmediate(renderMaterial);
			renderMaterial = null;
		}
	}

	protected override void OnRebuildRenderData()
	{
		base.OnRebuildRenderData();
		if (texture == null)
		{
			return;
		}
		ensureMaterial();
		if (!(material == null))
		{
			if (renderMaterial == null)
			{
				renderMaterial = new Material(material)
				{
					hideFlags = HideFlags.DontSave,
					name = material.name + " (copy)"
				};
			}
			renderMaterial.mainTexture = texture;
			renderData.Material = OverrideMaterial ?? renderMaterial;
			float num = PixelsToUnits();
			float x = 0f;
			float y = 0f;
			float x2 = size.x * num;
			float y2 = (0f - size.y) * num;
			Vector3 vector = pivot.TransformToUpperLeft(size).RoundToInt() * num;
			renderData.Vertices.Add(new Vector3(x, y, 0f) + vector);
			renderData.Vertices.Add(new Vector3(x2, y, 0f) + vector);
			renderData.Vertices.Add(new Vector3(x2, y2, 0f) + vector);
			renderData.Vertices.Add(new Vector3(x, y2, 0f) + vector);
			renderData.Triangles.AddRange(TRIANGLE_INDICES);
			rebuildUV(renderData);
			Color32 item = ApplyOpacity(color);
			renderData.Colors.Add(item);
			renderData.Colors.Add(item);
			renderData.Colors.Add(item);
			renderData.Colors.Add(item);
			if (fillAmount < 1f)
			{
				doFill(renderData);
			}
		}
	}

	protected virtual void disposeCreatedMaterial()
	{
		if (createdRuntimeMaterial)
		{
			UnityEngine.Object.DestroyImmediate(material);
			material = null;
			createdRuntimeMaterial = false;
		}
	}

	protected virtual void rebuildUV(dfRenderData renderBuffer)
	{
		dfList<Vector2> uV = renderBuffer.UV;
		if (cropImage)
		{
			int width = texture.width;
			int height = texture.height;
			float num = Mathf.Max(0f, Mathf.Min(cropRect.x, width));
			float num2 = Mathf.Max(0f, Mathf.Min(cropRect.xMax, width));
			float num3 = Mathf.Max(0f, Mathf.Min(cropRect.y, height));
			float num4 = Mathf.Max(0f, Mathf.Min(cropRect.yMax, height));
			uV.Add(new Vector2(num / (float)width, num4 / (float)height));
			uV.Add(new Vector2(num2 / (float)width, num4 / (float)height));
			uV.Add(new Vector2(num2 / (float)width, num3 / (float)height));
			uV.Add(new Vector2(num / (float)width, num3 / (float)height));
		}
		else
		{
			uV.Add(new Vector2(0f, 1f));
			uV.Add(new Vector2(1f, 1f));
			uV.Add(new Vector2(1f, 0f));
			uV.Add(new Vector2(0f, 0f));
		}
		Vector2 zero = Vector2.zero;
		if (flip.IsSet(dfSpriteFlip.FlipHorizontal))
		{
			zero = uV[1];
			uV[1] = uV[0];
			uV[0] = zero;
			zero = uV[3];
			uV[3] = uV[2];
			uV[2] = zero;
		}
		if (flip.IsSet(dfSpriteFlip.FlipVertical))
		{
			zero = uV[0];
			uV[0] = uV[3];
			uV[3] = zero;
			zero = uV[1];
			uV[1] = uV[2];
			uV[2] = zero;
		}
	}

	protected virtual void doFill(dfRenderData renderData)
	{
		dfList<Vector3> vertices = renderData.Vertices;
		dfList<Vector2> uV = renderData.UV;
		int index = 0;
		int index2 = 1;
		int index3 = 3;
		int index4 = 2;
		if (invertFill)
		{
			if (fillDirection == dfFillDirection.Horizontal)
			{
				index = 1;
				index2 = 0;
				index3 = 2;
				index4 = 3;
			}
			else
			{
				index = 3;
				index2 = 2;
				index3 = 0;
				index4 = 1;
			}
		}
		if (fillDirection == dfFillDirection.Horizontal)
		{
			vertices[index2] = Vector3.Lerp(vertices[index2], vertices[index], 1f - fillAmount);
			vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index3], 1f - fillAmount);
			uV[index2] = Vector2.Lerp(uV[index2], uV[index], 1f - fillAmount);
			uV[index4] = Vector2.Lerp(uV[index4], uV[index3], 1f - fillAmount);
		}
		else
		{
			vertices[index3] = Vector3.Lerp(vertices[index3], vertices[index], 1f - fillAmount);
			vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index2], 1f - fillAmount);
			uV[index3] = Vector2.Lerp(uV[index3], uV[index], 1f - fillAmount);
			uV[index4] = Vector2.Lerp(uV[index4], uV[index2], 1f - fillAmount);
		}
	}

	private Rect validateCropRect(Rect rect)
	{
		if (texture == null)
		{
			return default(Rect);
		}
		int width = texture.width;
		int height = texture.height;
		float x = Mathf.Max(0f, Mathf.Min(rect.x, width));
		float y = Mathf.Max(0f, Mathf.Min(rect.y, height));
		float width2 = Mathf.Max(0f, Mathf.Min(rect.width, width));
		float height2 = Mathf.Max(0f, Mathf.Min(rect.height, height));
		return new Rect(x, y, width2, height2);
	}

	protected internal virtual void OnTextureChanged(Texture value)
	{
		SignalHierarchy("OnTextureChanged", this, value);
		if (this.TextureChanged != null)
		{
			this.TextureChanged(this, value);
		}
	}

	private void ensureMaterial()
	{
		if (!(material != null) && !(texture == null))
		{
			Shader shader = Shader.Find("Daikon Forge/Default UI Shader");
			if (shader == null)
			{
				Debug.LogError("Failed to find default shader");
				return;
			}
			material = new Material(shader)
			{
				name = "Default Texture Shader",
				hideFlags = HideFlags.DontSave,
				mainTexture = texture
			};
			createdRuntimeMaterial = true;
		}
	}
}
