using UnityEngine;

public static class SpriteOutlineManager
{
	public enum OutlineType
	{
		NORMAL,
		EEVEE
	}

	private static string[] m_outlineNames = new string[4] { "OutlineSprite0", "OutlineSprite1", "OutlineSprite2", "OutlineSprite3" };

	private static int m_atlasDataID = -1;

	public static void AddSingleOutlineToSprite(tk2dBaseSprite targetSprite, IntVector2 pixelOffset, Color outlineColor)
	{
		AddSingleOutlineToSprite<tk2dSprite>(targetSprite, pixelOffset, outlineColor, 0.4f);
	}

	public static void AddOutlineToSprite(tk2dBaseSprite targetSprite, Color outlineColor)
	{
		AddOutlineToSprite(targetSprite, outlineColor, 0.4f);
	}

	public static void AddOutlineToSprite<T>(tk2dBaseSprite targetSprite, Color outlineColor) where T : tk2dBaseSprite
	{
		AddOutlineToSprite<T>(targetSprite, outlineColor, 0.4f);
	}

	public static void AddOutlineToSprite<T>(tk2dBaseSprite targetSprite, Color outlineColor, Material overrideOutlineMaterial) where T : tk2dBaseSprite
	{
		AddOutlineToSprite<T>(targetSprite, outlineColor, 0.4f, 0f, overrideOutlineMaterial);
	}

	public static bool HasOutline(tk2dBaseSprite targetSprite)
	{
		tk2dBaseSprite[] componentsInChildren = targetSprite.GetComponentsInChildren<tk2dBaseSprite>(true);
		foreach (tk2dBaseSprite tk2dBaseSprite2 in componentsInChildren)
		{
			if (!(tk2dBaseSprite2.transform.parent != targetSprite.transform) && tk2dBaseSprite2.IsOutlineSprite)
			{
				return true;
			}
		}
		return false;
	}

	public static Material GetOutlineMaterial(tk2dBaseSprite targetSprite)
	{
		if (targetSprite == null)
		{
			return null;
		}
		Transform transform = targetSprite.transform.Find("BraveOutlineSprite");
		if (transform != null)
		{
			return transform.GetComponent<tk2dBaseSprite>().renderer.sharedMaterial;
		}
		tk2dBaseSprite[] componentsInChildren = targetSprite.GetComponentsInChildren<tk2dBaseSprite>(true);
		foreach (tk2dBaseSprite tk2dBaseSprite2 in componentsInChildren)
		{
			if (!(tk2dBaseSprite2.transform.parent != targetSprite.transform) && tk2dBaseSprite2.IsOutlineSprite)
			{
				return tk2dBaseSprite2.renderer.sharedMaterial;
			}
		}
		return null;
	}

	public static tk2dSprite[] GetOutlineSprites(tk2dBaseSprite targetSprite)
	{
		return GetOutlineSprites<tk2dSprite>(targetSprite);
	}

	public static int ChangeOutlineLayer(tk2dBaseSprite targetSprite, int targetLayer)
	{
		tk2dSprite[] outlineSprites = GetOutlineSprites(targetSprite);
		int result = -1;
		if (outlineSprites != null)
		{
			for (int i = 0; i < outlineSprites.Length; i++)
			{
				if ((bool)outlineSprites[i])
				{
					result = outlineSprites[i].gameObject.layer;
					outlineSprites[i].gameObject.layer = targetLayer;
				}
			}
		}
		return result;
	}

	public static T[] GetOutlineSprites<T>(tk2dBaseSprite targetSprite) where T : tk2dBaseSprite
	{
		if (targetSprite == null)
		{
			return null;
		}
		Transform transform = targetSprite.transform.Find("BraveOutlineSprite");
		if (transform != null)
		{
			return new T[1] { transform.GetComponent<T>() };
		}
		T[] componentsInChildren = targetSprite.GetComponentsInChildren<T>(true);
		T[] array = new T[4];
		int num = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			T val = componentsInChildren[i];
			if (!(val.transform.parent != targetSprite.transform) && val.IsOutlineSprite)
			{
				array[num] = val;
				num++;
			}
		}
		return array;
	}

	public static void UpdateSingleOutlineSprite(tk2dBaseSprite targetSprite, IntVector2 newPixelOffset)
	{
		Transform transform = targetSprite.transform.Find("OutlineSprite0");
		if (transform != null)
		{
			transform.localPosition = newPixelOffset.ToVector3() * 0.0625f;
		}
	}

	public static void AddSingleOutlineToSprite<T>(tk2dBaseSprite targetSprite, IntVector2 pixelOffset, Color outlineColor, float zOffset, float luminanceCutoff = 0f) where T : tk2dBaseSprite
	{
		HandleSingleOutlineAddition<T>(targetSprite, null, pixelOffset, 0, outlineColor, zOffset, luminanceCutoff);
	}

	private static void HandleInitialLayer(tk2dBaseSprite sourceSprite, GameObject outlineObject)
	{
		int num = sourceSprite.gameObject.layer;
		if (num == 22)
		{
			num = 21;
		}
		outlineObject.layer = num;
	}

	private static void HandleBraveOutlineAddition<T>(tk2dBaseSprite targetSprite, Color outlineColor, float zOffset, float luminanceCutoff = 0f) where T : tk2dBaseSprite
	{
		Transform transform = targetSprite.transform;
		GameObject gameObject = new GameObject("BraveOutlineSprite");
		Transform transform2 = gameObject.transform;
		transform2.parent = transform;
		transform2.localPosition = Vector3.zero;
		transform2.localRotation = Quaternion.identity;
		if (targetSprite.ignoresTiltworldDepth)
		{
			transform2.localPosition = transform2.localPosition.WithZ(1f);
		}
		T val = gameObject.AddComponent<T>();
		val.IsOutlineSprite = true;
		val.IsBraveOutlineSprite = true;
		val.usesOverrideMaterial = true;
		val.depthUsesTrimmedBounds = targetSprite.depthUsesTrimmedBounds;
		val.SetSprite(targetSprite.Collection, targetSprite.spriteId);
		val.ignoresTiltworldDepth = targetSprite.ignoresTiltworldDepth;
		HandleInitialLayer(targetSprite, gameObject);
		val.scale = targetSprite.scale;
		Material material = new Material(ShaderCache.Acquire("Brave/Internal/SinglePassOutline"));
		material.mainTexture = targetSprite.renderer.sharedMaterial.mainTexture;
		material.SetColor("_OverrideColor", outlineColor);
		material.SetFloat("_LuminanceCutoff", luminanceCutoff);
		material.DisableKeyword("OUTLINE_OFF");
		material.EnableKeyword("OUTLINE_ON");
		val.renderer.material = material;
		val.HeightOffGround = 0f - zOffset;
		targetSprite.AttachRenderer(val);
		targetSprite.UpdateZDepth();
		HandleSpriteChanged(targetSprite);
	}

	private static Material HandleSingleOutlineAddition<T>(tk2dBaseSprite targetSprite, Material sharedMaterialToUse, IntVector2 pixelOffset, int outlineIndex, Color outlineColor, float zOffset, float luminanceCutoff = 0f) where T : tk2dBaseSprite
	{
		Transform transform = targetSprite.transform;
		Vector3 a = pixelOffset.ToVector3() * 0.0625f;
		GameObject gameObject = new GameObject(m_outlineNames[outlineIndex]);
		Transform transform2 = gameObject.transform;
		transform2.parent = transform;
		transform2.localPosition = Vector3.Scale(a, targetSprite.scale);
		transform2.localRotation = Quaternion.identity;
		if (targetSprite.ignoresTiltworldDepth)
		{
			transform2.localPosition = transform2.localPosition.WithZ(1f);
		}
		T val = gameObject.AddComponent<T>();
		val.IsOutlineSprite = true;
		val.usesOverrideMaterial = true;
		val.depthUsesTrimmedBounds = targetSprite.depthUsesTrimmedBounds;
		val.SetSprite(targetSprite.Collection, targetSprite.spriteId);
		val.ignoresTiltworldDepth = targetSprite.ignoresTiltworldDepth;
		HandleInitialLayer(targetSprite, gameObject);
		val.scale = targetSprite.scale;
		Material material = sharedMaterialToUse;
		if (material == null)
		{
			material = new Material(ShaderCache.Acquire("tk2d/SpriteOutlineCutout"));
			material.mainTexture = targetSprite.renderer.sharedMaterial.mainTexture;
			material.SetColor("_OverrideColor", outlineColor);
			material.SetFloat("_LuminanceCutoff", luminanceCutoff);
		}
		val.renderer.material = material;
		val.HeightOffGround = 0f - zOffset;
		targetSprite.AttachRenderer(val);
		targetSprite.UpdateZDepth();
		return material;
	}

	private static Material HandleSingleScaledOutlineAddition<T>(tk2dBaseSprite targetSprite, Material sharedMaterialToUse, IntVector2 pixelOffset, int outlineIndex, Color outlineColor, float zOffset, float luminanceCutoff = 0f) where T : tk2dBaseSprite
	{
		Transform transform = targetSprite.transform;
		Vector3 a = pixelOffset.ToVector3() * 0.0625f;
		bool flipX = targetSprite.FlipX;
		bool flipY = targetSprite.FlipY;
		Vector3 scale = Vector3.Scale(new Vector3((!flipX) ? 1 : (-1), (!flipY) ? 1 : (-1), 1f), targetSprite.scale);
		GameObject gameObject = new GameObject(m_outlineNames[outlineIndex]);
		Transform transform2 = gameObject.transform;
		transform2.parent = transform;
		transform2.localPosition = Vector3.Scale(a, targetSprite.scale);
		transform2.localRotation = Quaternion.identity;
		transform2.localScale = Vector3.one;
		if (targetSprite.ignoresTiltworldDepth)
		{
			transform2.localPosition = transform2.localPosition.WithZ(1f);
		}
		T val = gameObject.AddComponent<T>();
		val.IsOutlineSprite = true;
		val.usesOverrideMaterial = true;
		val.depthUsesTrimmedBounds = targetSprite.depthUsesTrimmedBounds;
		val.SetSprite(targetSprite.Collection, targetSprite.spriteId);
		val.ignoresTiltworldDepth = targetSprite.ignoresTiltworldDepth;
		HandleInitialLayer(targetSprite, gameObject);
		val.scale = scale;
		Material material = sharedMaterialToUse;
		if (material == null)
		{
			material = new Material(ShaderCache.Acquire("tk2d/SpriteOutlineCutout"));
			material.mainTexture = targetSprite.renderer.sharedMaterial.mainTexture;
			material.SetColor("_OverrideColor", outlineColor);
			material.SetFloat("_LuminanceCutoff", luminanceCutoff);
		}
		val.renderer.material = material;
		val.HeightOffGround = 0f - zOffset;
		targetSprite.AttachRenderer(val);
		targetSprite.UpdateZDepth();
		return material;
	}

	public static void ForceRebuildMaterial(tk2dBaseSprite outlineSprite, tk2dBaseSprite sourceSprite, Color c, float luminanceCutoff = 0f)
	{
		Material material = null;
		if (material == null)
		{
			material = new Material(ShaderCache.Acquire("tk2d/SpriteOutlineCutout"));
			material.mainTexture = sourceSprite.renderer.sharedMaterial.mainTexture;
			material.SetColor("_OverrideColor", c);
			material.SetFloat("_LuminanceCutoff", luminanceCutoff);
		}
		outlineSprite.renderer.material = material;
	}

	public static void ForceUpdateOutlineMaterial(tk2dBaseSprite outlineSprite, tk2dBaseSprite sourceSprite)
	{
		if ((bool)sourceSprite && (bool)outlineSprite)
		{
			Material sharedMaterial = outlineSprite.renderer.sharedMaterial;
			sharedMaterial.mainTexture = sourceSprite.renderer.sharedMaterial.mainTexture;
			outlineSprite.renderer.sharedMaterial = sharedMaterial;
		}
	}

	public static void AddScaledOutlineToSprite<T>(tk2dBaseSprite targetSprite, Color outlineColor, float zOffset, float luminanceCutoff) where T : tk2dBaseSprite
	{
		if (!(GameManager.Instance.Dungeon != null) || !GameManager.Instance.Dungeon.debugSettings.DISABLE_OUTLINES)
		{
			if (HasOutline(targetSprite))
			{
				RemoveOutlineFromSprite(targetSprite, true);
			}
			IntVector2[] cardinals = IntVector2.Cardinals;
			Material sharedMaterialToUse = null;
			for (int i = 0; i < 4; i++)
			{
				sharedMaterialToUse = HandleSingleScaledOutlineAddition<T>(targetSprite, sharedMaterialToUse, cardinals[i], i, outlineColor, zOffset, luminanceCutoff);
			}
			targetSprite.SpriteChanged += HandleSpriteChanged;
			targetSprite.UpdateZDepth();
			HandleSpriteChanged(targetSprite);
		}
	}

	public static void AddOutlineToSprite(tk2dBaseSprite targetSprite, Color outlineColor, float zOffset, float luminanceCutoff = 0f, OutlineType outlineType = OutlineType.NORMAL)
	{
		if (GameManager.Instance.Dungeon != null && GameManager.Instance.Dungeon.debugSettings.DISABLE_OUTLINES)
		{
			return;
		}
		if (HasOutline(targetSprite))
		{
			RemoveOutlineFromSprite(targetSprite, true);
		}
		switch (outlineType)
		{
		case OutlineType.NORMAL:
			HandleBraveOutlineAddition<tk2dSprite>(targetSprite, outlineColor, zOffset, luminanceCutoff);
			break;
		case OutlineType.EEVEE:
		{
			IntVector2[] cardinals = IntVector2.Cardinals;
			Material material = null;
			for (int i = 0; i < 4; i++)
			{
				material = HandleSingleOutlineAddition<tk2dSprite>(targetSprite, material, cardinals[i], i, outlineColor, zOffset, luminanceCutoff);
			}
			material.shader = Shader.Find("Brave/PlayerShaderEevee");
			material.SetTexture("_EeveeTex", targetSprite.transform.parent.GetComponent<CharacterAnimationRandomizer>().CosmicTex);
			break;
		}
		}
		targetSprite.SpriteChanged += HandleSpriteChanged;
		targetSprite.UpdateZDepth();
		HandleSpriteChanged(targetSprite);
		if (!targetSprite.renderer.enabled)
		{
			ToggleOutlineRenderers(targetSprite, false);
		}
	}

	public static void AddOutlineToSprite<T>(tk2dBaseSprite targetSprite, Color outlineColor, float zOffset, float luminanceCutoff = 0f, Material overrideOutlineMaterial = null) where T : tk2dBaseSprite
	{
		if (!(GameManager.Instance.Dungeon != null) || !GameManager.Instance.Dungeon.debugSettings.DISABLE_OUTLINES)
		{
			if (HasOutline(targetSprite))
			{
				RemoveOutlineFromSprite(targetSprite, true);
			}
			IntVector2[] cardinals = IntVector2.Cardinals;
			Material sharedMaterialToUse = overrideOutlineMaterial;
			for (int i = 0; i < 4; i++)
			{
				sharedMaterialToUse = HandleSingleOutlineAddition<T>(targetSprite, sharedMaterialToUse, cardinals[i], i, outlineColor, zOffset, luminanceCutoff);
			}
			targetSprite.SpriteChanged += HandleSpriteChanged;
			targetSprite.UpdateZDepth();
			HandleSpriteChanged(targetSprite);
		}
	}

	public static void HandleSpriteChanged(tk2dBaseSprite targetSprite)
	{
		if (m_atlasDataID == -1)
		{
			m_atlasDataID = Shader.PropertyToID("_AtlasData");
		}
		Transform transform = targetSprite.transform;
		Vector3 scale = targetSprite.scale;
		bool flag = false;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			tk2dBaseSprite component = child.GetComponent<tk2dBaseSprite>();
			if ((bool)component && component.IsBraveOutlineSprite)
			{
				flag = true;
				tk2dSpriteDefinition currentSpriteDef = targetSprite.GetCurrentSpriteDef();
				Vector4 value = new Vector4(1f, 1f, 0f, 0f);
				if (currentSpriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
				{
					value = new Vector4(value.x, value.y, 1f, 1f);
				}
				value.x *= targetSprite.scale.x;
				value.y *= targetSprite.scale.y;
				tk2dBaseSprite component2 = child.GetComponent<tk2dBaseSprite>();
				component2.scale = scale;
				component2.SetSprite(targetSprite.Collection, targetSprite.spriteId);
				component2.renderer.material.SetVector(m_atlasDataID, value);
			}
		}
		if (flag)
		{
			return;
		}
		for (int j = 0; j < 4; j++)
		{
			Transform transform2 = transform.Find(m_outlineNames[j]);
			if (transform2 != null)
			{
				tk2dBaseSprite component3 = transform2.GetComponent<tk2dBaseSprite>();
				component3.scale = scale;
				component3.SetSprite(targetSprite.Collection, targetSprite.spriteId);
			}
		}
	}

	public static void ToggleOutlineRenderers(tk2dBaseSprite targetSprite, bool value)
	{
		tk2dBaseSprite[] componentsInChildren = targetSprite.GetComponentsInChildren<tk2dBaseSprite>(true);
		foreach (tk2dBaseSprite tk2dBaseSprite2 in componentsInChildren)
		{
			if (tk2dBaseSprite2.IsOutlineSprite)
			{
				tk2dBaseSprite2.renderer.enabled = value;
			}
		}
	}

	public static void RemoveOutlineFromSprite(tk2dBaseSprite targetSprite, bool deparent = false)
	{
		Transform transform = targetSprite.transform;
		targetSprite.SpriteChanged -= HandleSpriteChanged;
		bool flag = false;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			tk2dBaseSprite component = child.GetComponent<tk2dBaseSprite>();
			if ((bool)component && component.IsBraveOutlineSprite)
			{
				flag = true;
				if (deparent)
				{
					child.parent = null;
				}
				Object.Destroy(child.gameObject);
			}
		}
		if (flag)
		{
			return;
		}
		for (int j = 0; j < 4; j++)
		{
			Transform transform2 = transform.Find(m_outlineNames[j]);
			if (transform2 != null)
			{
				if (deparent)
				{
					transform2.parent = null;
				}
				Object.Destroy(transform2.gameObject);
				continue;
			}
			break;
		}
	}
}
