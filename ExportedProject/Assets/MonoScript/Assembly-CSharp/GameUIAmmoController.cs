using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUIAmmoController : BraveBehaviour
{
	[SerializeField]
	[BetterList]
	public GameUIAmmoType[] ammoTypes;

	[FormerlySerializedAs("GunAmmoBottomCapSprite")]
	public dfSprite InitialBottomCapSprite;

	[FormerlySerializedAs("GunAmmoTopCapSprite")]
	public dfSprite InitialTopCapSprite;

	public dfSprite GunBoxSprite;

	public dfSprite GunQuickSwitchIcon;

	public GameObject ExtraGunCardPrefab;

	public List<dfControl> AdditionalGunBoxSprites = new List<dfControl>();

	public dfLabel GunClipCountLabel;

	public dfLabel GunAmmoCountLabel;

	public dfSprite GunCooldownForegroundSprite;

	public dfSprite GunCooldownFillSprite;

	public dfSpriteAnimation AmmoBurstVFX;

	[NonSerialized]
	public bool temporarilyPreventVisible;

	[NonSerialized]
	public bool forceInvisiblePermanent;

	private List<dfTiledSprite> fgSpritesForModules = new List<dfTiledSprite>();

	private List<dfTiledSprite> bgSpritesForModules = new List<dfTiledSprite>();

	private List<List<dfTiledSprite>> addlFgSpritesForModules = new List<List<dfTiledSprite>>();

	private List<List<dfTiledSprite>> addlBgSpritesForModules = new List<List<dfTiledSprite>>();

	private List<dfSprite> topCapsForModules = new List<dfSprite>();

	private List<dfSprite> bottomCapsForModules = new List<dfSprite>();

	private List<GameUIAmmoType.AmmoType> cachedAmmoTypesForModules = new List<GameUIAmmoType.AmmoType>();

	private List<string> cachedCustomAmmoTypesForModules = new List<string>();

	private dfPanel m_panel;

	private List<GameUIAmmoType> m_additionalAmmoTypeDefinitions = new List<GameUIAmmoType>();

	public tk2dClippedSprite[] gunSprites;

	public bool IsLeftAligned;

	private Gun m_cachedGun;

	private List<int> m_cachedModuleShotsRemaining = new List<int>();

	private int m_cachedMaxAmmo;

	private int m_cachedTotalAmmo;

	private int m_cachedNumberModules = 1;

	private bool m_cachedUndertaleness;

	private tk2dSprite[][] outlineSprites;

	private bool m_initialized;

	private Material m_ClippedMaterial;

	private Material m_ClippedZWriteOffMaterial;

	private float UI_OUTLINE_DEPTH = 1f;

	private static int NumberOfAdditionalGunCards = 3;

	private List<dfSprite> m_additionalRegisteredSprites = new List<dfSprite>();

	private tk2dSpriteDefinition m_cachedGunSpriteDefinition;

	private bool m_isCurrentlyFlipping;

	private bool m_currentFlipReverse;

	private float m_currentGunSpriteXOffset;

	private float m_currentGunSpriteZOffset;

	private bool m_deferCurrentGunSwap;

	private bool m_cardFlippedQueued;

	private const float FLIP_TIME = 0.15f;

	private tk2dSprite m_extantNoAmmoIcon;

	public bool SuppressNextGunFlip;

	private const int NUM_PIXELS_PER_MODULE = -10;

	public dfSprite DefaultAmmoFGSprite
	{
		get
		{
			if (fgSpritesForModules == null || fgSpritesForModules.Count == 0)
			{
				return null;
			}
			return fgSpritesForModules[0];
		}
	}

	public bool IsFlipping
	{
		get
		{
			return m_isCurrentlyFlipping;
		}
	}

	private int GUN_BOX_EXTRA_PX_OFFSET
	{
		get
		{
			return (!IsLeftAligned) ? 9 : (-9);
		}
	}

	private int AdditionalBoxOffsetPx
	{
		get
		{
			return (!IsLeftAligned) ? 2 : (-2);
		}
	}

	private void Initialize()
	{
		m_panel = GetComponent<dfPanel>();
		outlineSprites = new tk2dSprite[gunSprites.Length][];
		for (int i = 0; i < gunSprites.Length; i++)
		{
			tk2dClippedSprite tk2dClippedSprite2 = gunSprites[i];
			SpriteOutlineManager.AddOutlineToSprite(tk2dClippedSprite2, Color.white, 2f);
			outlineSprites[i] = SpriteOutlineManager.GetOutlineSprites(tk2dClippedSprite2);
			for (int j = 0; j < outlineSprites[i].Length; j++)
			{
				if (outlineSprites[i].Length > 1)
				{
					float num = ((j != 1) ? 0f : 0.0625f);
					num = ((j != 3) ? num : (-0.0625f));
					float num2 = ((j != 0) ? 0f : 0.0625f);
					num2 = ((j != 2) ? num2 : (-0.0625f));
					outlineSprites[i][j].transform.localPosition = (new Vector3(num, num2, 0f) * Pixelator.Instance.CurrentTileScale * 16f * GameUIRoot.Instance.PixelsToUnits()).WithZ(UI_OUTLINE_DEPTH);
				}
				outlineSprites[i][j].gameObject.layer = tk2dClippedSprite2.gameObject.layer;
			}
		}
		m_ClippedMaterial = new Material(ShaderCache.Acquire("Daikon Forge/Clipped UI Shader"));
		m_ClippedZWriteOffMaterial = new Material(ShaderCache.Acquire("Daikon Forge/Clipped UI Shader ZWriteOff"));
		topCapsForModules.Add(InitialTopCapSprite);
		bottomCapsForModules.Add(InitialBottomCapSprite);
		m_panel.SendToBack();
		m_initialized = true;
	}

	public dfSprite RegisterNewAdditionalSprite(string spriteName)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(InitialBottomCapSprite.gameObject);
		dfSprite component = gameObject.GetComponent<dfSprite>();
		InitialBottomCapSprite.Parent.AddControl(component);
		component.SpriteName = spriteName;
		component.Size = component.SpriteInfo.sizeInPixels * Pixelator.Instance.CurrentTileScale;
		m_additionalRegisteredSprites.Add(component);
		UpdateAdditionalSprites();
		return component;
	}

	public void DeregisterAdditionalSprite(dfSprite sprite)
	{
		if (m_additionalRegisteredSprites.Contains(sprite))
		{
			m_additionalRegisteredSprites.Remove(sprite);
			UnityEngine.Object.Destroy(sprite.gameObject);
			UpdateAdditionalSprites();
		}
	}

	private void UpdateAdditionalSprites()
	{
		float currentTileScale = Pixelator.Instance.CurrentTileScale;
		Vector3 position = GunAmmoCountLabel.Position;
		Vector3 zero = Vector3.zero;
		if (IsLeftAligned)
		{
			position += new Vector3(0f, 4f * currentTileScale, 0f);
		}
		else
		{
			position += new Vector3(GunAmmoCountLabel.Size.x, 4f * currentTileScale, 0f);
		}
		int num = (IsLeftAligned ? 1 : (-1));
		for (int i = 0; i < m_additionalRegisteredSprites.Count; i++)
		{
			Vector2 size = m_additionalRegisteredSprites[i].Size;
			if (IsLeftAligned)
			{
				m_additionalRegisteredSprites[i].Position = position + num * zero;
				zero += new Vector3(size.x + currentTileScale, 0f, 0f);
			}
			else
			{
				zero += new Vector3(size.x, 0f, 0f);
				m_additionalRegisteredSprites[i].Position = position + num * zero;
				zero += new Vector3(currentTileScale, 0f, 0f);
			}
		}
	}

	private void RepositionOutlines(dfControl arg1, Vector3 arg2, Vector3 arg3)
	{
		if (outlineSprites == null)
		{
			return;
		}
		for (int i = 0; i < gunSprites.Length; i++)
		{
			for (int j = 0; j < outlineSprites.Length; j++)
			{
				outlineSprites[i][j].gameObject.layer = gunSprites[i].gameObject.layer;
			}
		}
	}

	public void DimGunSprite()
	{
		for (int i = 0; i < gunSprites.Length; i++)
		{
			gunSprites[i].gameObject.SetActive(false);
		}
		if (m_extantNoAmmoIcon != null)
		{
			m_extantNoAmmoIcon.gameObject.SetActive(false);
		}
	}

	public void UndimGunSprite()
	{
		for (int i = 0; i < gunSprites.Length; i++)
		{
			gunSprites[i].gameObject.SetActive(true);
		}
		if (m_extantNoAmmoIcon != null)
		{
			m_extantNoAmmoIcon.gameObject.SetActive(true);
		}
	}

	private float ActualSign(float f)
	{
		if (f < 0f)
		{
			return -1f;
		}
		if (f > 0f)
		{
			return 1f;
		}
		return 0f;
	}

	public void UpdateScale()
	{
		float currentTileScale = Pixelator.Instance.CurrentTileScale;
		GunBoxSprite.Size = GunBoxSprite.SpriteInfo.sizeInPixels * currentTileScale;
		Vector2 tileScale = new Vector2(currentTileScale, currentTileScale);
		for (int i = 0; i < fgSpritesForModules.Count; i++)
		{
			if ((bool)fgSpritesForModules[i])
			{
				fgSpritesForModules[i].TileScale = tileScale;
				bgSpritesForModules[i].TileScale = tileScale;
			}
		}
		for (int j = 0; j < addlFgSpritesForModules.Count; j++)
		{
			List<dfTiledSprite> list = addlFgSpritesForModules[j];
			List<dfTiledSprite> list2 = addlBgSpritesForModules[j];
			for (int k = 0; k < list.Count; k++)
			{
				list[k].TileScale = tileScale;
				list2[k].TileScale = tileScale;
			}
		}
		for (int l = 0; l < topCapsForModules.Count; l++)
		{
			topCapsForModules[l].Size = topCapsForModules[l].SpriteInfo.sizeInPixels * currentTileScale;
			bottomCapsForModules[l].Size = bottomCapsForModules[l].SpriteInfo.sizeInPixels * currentTileScale;
		}
		if (GunClipCountLabel != null)
		{
			GunClipCountLabel.TextScale = currentTileScale;
		}
		if (GunAmmoCountLabel != null)
		{
			GunAmmoCountLabel.TextScale = currentTileScale;
		}
	}

	public void SetAmmoCountLabelColor(Color targetcolor)
	{
		GunAmmoCountLabel.Color = targetcolor;
		GunAmmoCountLabel.BottomColor = targetcolor;
	}

	public void ToggleRenderers(bool value)
	{
		if (!m_initialized)
		{
			return;
		}
		if (GunBoxSprite != null && GunBoxSprite.Parent != null)
		{
			GunBoxSprite.IsVisible = value;
		}
		if (GunBoxSprite != null)
		{
			GunBoxSprite.IsVisible = value;
		}
		if (GunQuickSwitchIcon != null && !value)
		{
			GunQuickSwitchIcon.IsVisible = value;
		}
		for (int i = 0; i < fgSpritesForModules.Count; i++)
		{
			if ((bool)fgSpritesForModules[i])
			{
				fgSpritesForModules[i].IsVisible = value;
				bgSpritesForModules[i].IsVisible = value;
			}
		}
		for (int j = 0; j < addlFgSpritesForModules.Count; j++)
		{
			List<dfTiledSprite> list = addlFgSpritesForModules[j];
			List<dfTiledSprite> list2 = addlBgSpritesForModules[j];
			for (int k = 0; k < list.Count; k++)
			{
				list[k].IsVisible = value;
				list2[k].IsVisible = value;
			}
		}
		if (m_extantNoAmmoIcon != null)
		{
			m_extantNoAmmoIcon.renderer.enabled = value;
		}
		if (GunAmmoCountLabel != null)
		{
			GunAmmoCountLabel.IsVisible = value;
		}
		for (int l = 0; l < topCapsForModules.Count; l++)
		{
			topCapsForModules[l].IsVisible = value;
			bottomCapsForModules[l].IsVisible = value;
		}
		if (GunClipCountLabel != null)
		{
			GunClipCountLabel.IsVisible = value;
		}
		for (int m = 0; m < gunSprites.Length; m++)
		{
			tk2dClippedSprite tk2dClippedSprite2 = gunSprites[m];
			if (tk2dClippedSprite2.renderer.enabled == value)
			{
				continue;
			}
			tk2dClippedSprite2.renderer.enabled = value;
			outlineSprites[m] = SpriteOutlineManager.GetOutlineSprites(tk2dClippedSprite2);
			for (int n = 0; n < outlineSprites[m].Length; n++)
			{
				if ((bool)outlineSprites[m][n] && (bool)outlineSprites[m][n].renderer)
				{
					outlineSprites[m][n].renderer.enabled = value;
				}
			}
		}
	}

	private void DoGunCardFlip(Gun newGun, int change)
	{
		if (AdditionalGunBoxSprites.Count == 0 || AdditionalGunBoxSprites.Count > 10)
		{
			return;
		}
		if (!m_isCurrentlyFlipping && GameUIRoot.Instance.GunventoryFolded)
		{
			if (change > 0)
			{
				StartCoroutine(HandleGunCardFlipReverse(newGun));
			}
			else
			{
				StartCoroutine(HandleGunCardFlip(newGun));
			}
		}
		else if (m_cardFlippedQueued)
		{
		}
	}

	private Transform GetChildestOfTransforms(Transform parent)
	{
		Transform transform = parent;
		while (transform != null && transform.childCount > 0)
		{
			transform = GetFirstValidChild(transform);
		}
		return transform;
	}

	private IEnumerator WaitForCurrentGunFlipToEnd(Gun newGun, int change)
	{
		m_cardFlippedQueued = true;
		while (m_isCurrentlyFlipping)
		{
			yield return null;
		}
		if (change > 0)
		{
			m_deferCurrentGunSwap = true;
		}
		m_isCurrentlyFlipping = true;
		yield return null;
		m_cardFlippedQueued = false;
		if (change > 0)
		{
			StartCoroutine(HandleGunCardFlipReverse(newGun));
		}
		else
		{
			StartCoroutine(HandleGunCardFlip(newGun));
		}
	}

	private IEnumerator HandleGunCardFlipReverse(Gun newGun)
	{
		m_deferCurrentGunSwap = true;
		m_isCurrentlyFlipping = true;
		m_currentFlipReverse = true;
		float elapsed = 0f;
		float p2u = GunBoxSprite.PixelsToUnits();
		Transform gbTransform = GunBoxSprite.transform;
		GameObject placeholderCardObject = UnityEngine.Object.Instantiate(ExtraGunCardPrefab);
		dfControl placeholderCard = placeholderCardObject.GetComponent<dfControl>();
		Transform placeholderTransform = placeholderCardObject.transform;
		placeholderCard.Pivot = (IsLeftAligned ? dfPivotPoint.TopRight : dfPivotPoint.TopLeft);
		placeholderTransform.parent = m_panel.transform;
		m_panel.AddControl(placeholderCard);
		placeholderCard.RelativePosition = GunBoxSprite.RelativePosition;
		m_cachedGunSpriteDefinition = m_cachedGun.GetSprite().Collection.spriteDefinitions[m_cachedGun.DefaultSpriteID];
		m_currentGunSpriteZOffset = -2f;
		for (int i = 0; i < AdditionalGunBoxSprites.Count; i++)
		{
			(AdditionalGunBoxSprites[i] as dfTextureSprite).Material = m_ClippedMaterial;
			AdditionalGunBoxSprites[i].Invalidate();
		}
		Vector3 cachedPosition = Vector3.zero;
		Transform firstExtraGunCardTransform = GetFirstValidChild(gbTransform);
		firstExtraGunCardTransform.parent = m_panel.transform;
		m_panel.AddControl(firstExtraGunCardTransform.GetComponent<dfControl>());
		cachedPosition = firstExtraGunCardTransform.position;
		(placeholderCard as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
		Transform leafExtraCardTransform = (placeholderTransform.parent = GetChildestOfTransforms(firstExtraGunCardTransform));
		leafExtraCardTransform.GetComponent<dfControl>().AddControl(placeholderCard);
		GunBoxSprite.enabled = false;
		tk2dClippedSprite[] newGunSprites = new tk2dClippedSprite[gunSprites.Length];
		tk2dSprite[][] oldGunSpritesAndOutlines = new tk2dSprite[gunSprites.Length][];
		for (int j = 0; j < gunSprites.Length; j++)
		{
			newGunSprites[j] = UnityEngine.Object.Instantiate(gunSprites[j].gameObject, gunSprites[j].transform.position, Quaternion.identity).GetComponent<tk2dClippedSprite>();
		}
		Vector3 startPosition = placeholderTransform.position + new Vector3(Pixelator.Instance.CurrentTileScale * (float)AdditionalBoxOffsetPx * (float)AdditionalGunBoxSprites.Count * p2u, 0f, 0f);
		for (int k = 0; k < newGunSprites.Length; k++)
		{
			tk2dClippedSprite tk2dClippedSprite2 = newGunSprites[k];
			SpriteOutlineManager.RemoveOutlineFromSprite(tk2dClippedSprite2, true);
			if (newGun.CurrentAmmo != 0)
			{
				SpriteOutlineManager.AddOutlineToSprite(tk2dClippedSprite2, Color.white, 2f);
			}
			tk2dClippedSprite2.transform.parent = gunSprites[k].transform.parent;
			tk2dClippedSprite2.transform.position = tk2dClippedSprite2.transform.position.WithZ(5f);
			tk2dClippedSprite2.renderer.material.SetFloat("_Saturation", (newGun.CurrentAmmo != 0) ? 1 : 0);
			tk2dBaseSprite tk2dBaseSprite2 = newGun.GetSprite();
			oldGunSpritesAndOutlines[k] = tk2dClippedSprite2.GetComponentsInChildren<tk2dSprite>();
			for (int l = 0; l < oldGunSpritesAndOutlines[k].Length; l++)
			{
				oldGunSpritesAndOutlines[k][l].scale = tk2dClippedSprite2.scale;
				oldGunSpritesAndOutlines[k][l].SetSprite(tk2dBaseSprite2.Collection, tk2dBaseSprite2.spriteId);
				SpriteOutlineManager.ForceUpdateOutlineMaterial(oldGunSpritesAndOutlines[k][l], tk2dBaseSprite2);
			}
		}
		bool hasDepthSwapped = false;
		float adjFlipTime = 0.15f * (float)((AdditionalGunBoxSprites.Count <= 20) ? 1 : (AdditionalGunBoxSprites.Count / 20));
		while (elapsed < adjFlipTime && GameUIRoot.Instance.GunventoryFolded)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / adjFlipTime;
			if (t >= 0.5f && !hasDepthSwapped)
			{
				m_cachedGunSpriteDefinition = null;
				hasDepthSwapped = true;
				if ((bool)placeholderTransform)
				{
					placeholderTransform.parent = m_panel.transform;
					m_panel.AddControl(placeholderCard);
					firstExtraGunCardTransform.parent = placeholderTransform;
					placeholderCard.AddControl(firstExtraGunCardTransform.GetComponent<dfControl>());
					(placeholderCard as dfTextureSprite).Material = m_ClippedMaterial;
				}
				for (int m = 0; m < AdditionalGunBoxSprites.Count; m++)
				{
					(AdditionalGunBoxSprites[m] as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
				}
				m_currentGunSpriteZOffset = 5f;
			}
			float xOffset = BraveMathCollege.DoubleLerp(0f, (float)(AdditionalGunBoxSprites.Count * AdditionalBoxOffsetPx + AdditionalBoxOffsetPx * 2) * Pixelator.Instance.CurrentTileScale, (float)(AdditionalGunBoxSprites.Count * -AdditionalBoxOffsetPx) * Pixelator.Instance.CurrentTileScale, t);
			float yOffset = BraveMathCollege.DoubleLerpSmooth(0f, 24f * Pixelator.Instance.CurrentTileScale, 0f, t);
			float zRotation = (float)((!IsLeftAligned) ? 1 : (-1)) * BraveMathCollege.DoubleLerp(0f, -20f, 0f, Mathf.Clamp01(t * 1.1f));
			if (placeholderTransform == null || !placeholderTransform)
			{
				break;
			}
			placeholderTransform.position = startPosition + new Vector3(xOffset * p2u, yOffset * p2u, 0f);
			placeholderTransform.rotation = Quaternion.Euler(0f, 0f, zRotation);
			for (int n = 0; n < newGunSprites.Length; n++)
			{
				Vector3 center = placeholderCard.GetCenter();
				tk2dClippedSprite tk2dClippedSprite3 = newGunSprites[n];
				tk2dClippedSprite3.SetSprite(newGun.GetSprite().Collection, newGun.DefaultSpriteID);
				tk2dBaseSprite[] array = SpriteOutlineManager.GetOutlineSprites<tk2dBaseSprite>(tk2dClippedSprite3);
				for (int num = 0; num < array.Length; num++)
				{
					SpriteOutlineManager.ForceUpdateOutlineMaterial(array[num], tk2dClippedSprite3);
				}
				Bounds untrimmedBounds = tk2dClippedSprite3.Collection.spriteDefinitions[newGun.DefaultSpriteID].GetUntrimmedBounds();
				Vector3 vector = Vector3.Scale(untrimmedBounds.min + untrimmedBounds.extents, tk2dClippedSprite3.scale);
				float z = (tk2dClippedSprite3.transform.rotation * new Vector3(0f - vector.x, vector.y * -1f, vector.z)).z;
				Vector3 vector2 = GetOffsetVectorForGun(newGun, true).WithZ(z);
				tk2dClippedSprite3.transform.position = center.WithZ((!hasDepthSwapped) ? 5f : (center.z - 2f)) + vector2;
				tk2dClippedSprite3.transform.position = tk2dClippedSprite3.transform.position.Quantize(GunBoxSprite.PixelsToUnits() * 3f);
				if (t >= 1f)
				{
					zRotation = 0f;
				}
				tk2dClippedSprite3.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
			}
			int hasAdditionalBGSprites = 0;
			if (addlBgSpritesForModules.Count > 0 && addlBgSpritesForModules[0].Count > 0)
			{
				hasAdditionalBGSprites = 1;
			}
			m_currentGunSpriteXOffset = (float)AdditionalBoxOffsetPx * Pixelator.Instance.CurrentTileScale * p2u * (float)hasAdditionalBGSprites * (float)(IsLeftAligned ? 1 : (-1));
			float extraCardXOffset = BraveMathCollege.SmoothLerp((float)(-AdditionalBoxOffsetPx) * Pixelator.Instance.CurrentTileScale * p2u, 0f, t);
			if ((bool)firstExtraGunCardTransform)
			{
				firstExtraGunCardTransform.position = cachedPosition + new Vector3(extraCardXOffset, 0f, 0f);
				firstExtraGunCardTransform.rotation = Quaternion.identity;
			}
			if (elapsed > adjFlipTime)
			{
				m_currentGunSpriteXOffset = 0f;
				m_cachedGunSpriteDefinition = null;
			}
			yield return null;
			for (int num2 = 0; num2 < newGunSprites.Length; num2++)
			{
				tk2dClippedSprite tk2dClippedSprite4 = newGunSprites[num2];
				if (!tk2dClippedSprite4.renderer.enabled)
				{
					for (int num3 = 0; num3 < oldGunSpritesAndOutlines[num2].Length; num3++)
					{
						oldGunSpritesAndOutlines[num2][num3].renderer.enabled = true;
					}
				}
			}
		}
		m_currentGunSpriteXOffset = 0f;
		m_cachedGunSpriteDefinition = null;
		m_deferCurrentGunSwap = false;
		yield return null;
		m_cachedGunSpriteDefinition = null;
		PostFlipReset(firstExtraGunCardTransform, gbTransform, placeholderCardObject, newGunSprites, newGun);
		m_isCurrentlyFlipping = false;
	}

	private Transform GetFirstValidChild(Transform source)
	{
		for (int i = 0; i < source.childCount; i++)
		{
			if ((bool)source.GetChild(i) && (!GunQuickSwitchIcon || !(source.GetChild(i) == GunQuickSwitchIcon.transform)))
			{
				return source.GetChild(i);
			}
		}
		return null;
	}

	public void UpdateNoAmmoIcon()
	{
		if (m_extantNoAmmoIcon != null)
		{
			m_extantNoAmmoIcon.scale = gunSprites[0].scale;
			m_extantNoAmmoIcon.transform.position = GunBoxSprite.GetCenter().Quantize(0.0625f * m_extantNoAmmoIcon.scale.x).WithZ(m_panel.transform.position.z - 3f);
		}
	}

	public void AddNoAmmoIcon()
	{
		if (m_extantNoAmmoIcon == null)
		{
			gunSprites[0].renderer.material.SetFloat("_Saturation", 0f);
			SpriteOutlineManager.ToggleOutlineRenderers(gunSprites[0], false);
			tk2dSprite component = ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/NoAmmoIcon"))).GetComponent<tk2dSprite>();
			component.transform.parent = m_panel.transform;
			component.HeightOffGround = 5f;
			component.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
			component.scale = gunSprites[0].scale;
			component.transform.position = GunBoxSprite.GetCenter().Quantize(0.0625f * component.scale.x);
			m_extantNoAmmoIcon = component;
		}
	}

	public void ClearNoAmmoIcon()
	{
		if (m_extantNoAmmoIcon != null)
		{
			if (m_isCurrentlyFlipping && m_currentFlipReverse)
			{
				m_extantNoAmmoIcon.renderer.enabled = false;
				return;
			}
			SpriteOutlineManager.ToggleOutlineRenderers(gunSprites[0], true);
			gunSprites[0].renderer.material.SetFloat("_Saturation", 1f);
			UnityEngine.Object.Destroy(m_extantNoAmmoIcon.gameObject);
			m_extantNoAmmoIcon = null;
		}
	}

	private IEnumerator HandleGunCardFlip(Gun newGun)
	{
		m_deferCurrentGunSwap = true;
		m_isCurrentlyFlipping = true;
		m_currentFlipReverse = false;
		float elapsed = 0f;
		float p2u = GunBoxSprite.PixelsToUnits();
		Transform gbTransform = GunBoxSprite.transform;
		GameObject placeholderCardObject = UnityEngine.Object.Instantiate(ExtraGunCardPrefab);
		dfControl placeholderCard = placeholderCardObject.GetComponent<dfControl>();
		Transform placeholderTransform = placeholderCardObject.transform;
		placeholderCard.Pivot = (IsLeftAligned ? dfPivotPoint.TopRight : dfPivotPoint.TopLeft);
		placeholderTransform.parent = m_panel.transform;
		m_panel.AddControl(placeholderCard);
		placeholderCard.RelativePosition = GunBoxSprite.RelativePosition;
		m_currentGunSpriteZOffset = 5f;
		for (int i = 0; i < AdditionalGunBoxSprites.Count; i++)
		{
			(AdditionalGunBoxSprites[i] as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
			AdditionalGunBoxSprites[i].Invalidate();
		}
		Vector3 cachedPosition = Vector3.zero;
		Transform newChild = GetFirstValidChild(gbTransform);
		newChild.parent = placeholderTransform;
		(placeholderCard as dfTextureSprite).Material = m_ClippedMaterial;
		placeholderCard.AddControl(newChild.GetComponent<dfControl>());
		cachedPosition = newChild.position;
		GunBoxSprite.enabled = false;
		Vector3 startPosition = placeholderTransform.position;
		tk2dClippedSprite[] oldGunSprites = new tk2dClippedSprite[gunSprites.Length];
		tk2dBaseSprite[][] gunSpritesAndOutlines = new tk2dBaseSprite[gunSprites.Length][];
		tk2dSpriteDefinition[] oldGunSpriteDefinitions = new tk2dSpriteDefinition[gunSprites.Length];
		tk2dSpriteCollectionData[] oldGunSpriteCollections = new tk2dSpriteCollectionData[gunSprites.Length];
		for (int j = 0; j < gunSprites.Length; j++)
		{
			tk2dClippedSprite tk2dClippedSprite2 = gunSprites[j];
			oldGunSprites[j] = UnityEngine.Object.Instantiate(tk2dClippedSprite2.gameObject, tk2dClippedSprite2.transform.position, Quaternion.identity).GetComponent<tk2dClippedSprite>();
			oldGunSpriteDefinitions[j] = m_cachedGun.GetSprite().Collection.spriteDefinitions[m_cachedGun.DefaultSpriteID];
			oldGunSpriteCollections[j] = m_cachedGun.GetSprite().Collection;
			oldGunSprites[j].transform.parent = tk2dClippedSprite2.transform.parent;
			oldGunSprites[j].transform.position = oldGunSprites[j].transform.position.WithZ(-2f);
			oldGunSprites[j].renderer.material.SetFloat("_Saturation", (m_cachedGun.CurrentAmmo != 0) ? 1 : 0);
			SpriteOutlineManager.RemoveOutlineFromSprite(oldGunSprites[j], true);
			if (m_cachedGun.CurrentAmmo != 0)
			{
				SpriteOutlineManager.AddOutlineToSprite(oldGunSprites[j], Color.white, 2f);
			}
			gunSpritesAndOutlines[j] = tk2dClippedSprite2.GetComponentsInChildren<tk2dBaseSprite>();
		}
		bool hasDepthSwapped = false;
		float adjFlipTime = 0.15f * (float)((AdditionalGunBoxSprites.Count <= 20) ? 1 : (AdditionalGunBoxSprites.Count / 20));
		while (elapsed < adjFlipTime && GameUIRoot.Instance.GunventoryFolded)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / adjFlipTime;
			if (t >= 0.5f && !hasDepthSwapped)
			{
				hasDepthSwapped = true;
				newChild.parent = m_panel.transform;
				m_panel.AddControl(newChild.GetComponent<dfControl>());
				(placeholderCard as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
				for (int k = 0; k < AdditionalGunBoxSprites.Count; k++)
				{
					(AdditionalGunBoxSprites[k] as dfTextureSprite).Material = m_ClippedMaterial;
				}
				m_currentGunSpriteZOffset = -2f;
				Transform childestOfTransforms = GetChildestOfTransforms(newChild);
				if ((bool)placeholderTransform)
				{
					placeholderTransform.parent = childestOfTransforms;
				}
				childestOfTransforms.GetComponent<dfControl>().AddControl(placeholderCard);
			}
			float xOffset = BraveMathCollege.DoubleLerp(0f, (float)(AdditionalGunBoxSprites.Count * AdditionalBoxOffsetPx + AdditionalBoxOffsetPx * 2) * Pixelator.Instance.CurrentTileScale, (float)(AdditionalGunBoxSprites.Count * AdditionalBoxOffsetPx) * Pixelator.Instance.CurrentTileScale, t);
			float yOffset = BraveMathCollege.DoubleLerpSmooth(0f, 24f * Pixelator.Instance.CurrentTileScale, 0f, t);
			float zRotation = (float)((!IsLeftAligned) ? 1 : (-1)) * BraveMathCollege.DoubleLerp(0f, -20f, 0f, t);
			if ((bool)placeholderTransform)
			{
				placeholderTransform.position = startPosition + new Vector3(xOffset * p2u, yOffset * p2u, 0f);
				placeholderTransform.rotation = Quaternion.Euler(0f, 0f, zRotation);
			}
			for (int l = 0; l < gunSprites.Length; l++)
			{
				tk2dClippedSprite tk2dClippedSprite3 = oldGunSprites[l];
				tk2dSprite[] array = SpriteOutlineManager.GetOutlineSprites<tk2dSprite>(tk2dClippedSprite3);
				if (array != null)
				{
					for (int m = 0; m < array.Length; m++)
					{
						if ((bool)array[m])
						{
							array[m].SetSprite(oldGunSpriteCollections[l], tk2dClippedSprite3.spriteId);
							array[m].ForceUpdateMaterial();
							SpriteOutlineManager.ForceRebuildMaterial(array[m], tk2dClippedSprite3, Color.white);
						}
					}
				}
				if ((bool)placeholderCard)
				{
					Vector3 center = placeholderCard.GetCenter();
					Vector3 vector = Vector3.Scale(oldGunSpriteDefinitions[l].GetUntrimmedBounds().extents, tk2dClippedSprite3.scale);
					Vector3 vector2 = tk2dClippedSprite3.transform.rotation * new Vector3(0f - vector.x, vector.y * -1f, vector.z);
					tk2dClippedSprite3.transform.position = center.WithZ((!hasDepthSwapped) ? (center.z - 2f) : 5f) + vector2;
					tk2dClippedSprite3.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
				}
			}
			float extraCardXOffset = BraveMathCollege.SmoothLerp(0f, (float)(-AdditionalBoxOffsetPx) * Pixelator.Instance.CurrentTileScale * p2u, t);
			m_currentGunSpriteXOffset = (float)AdditionalBoxOffsetPx * Pixelator.Instance.CurrentTileScale * p2u + extraCardXOffset;
			if ((bool)newChild)
			{
				newChild.position = cachedPosition + new Vector3(extraCardXOffset, 0f, 0f);
				newChild.rotation = Quaternion.identity;
			}
			yield return null;
			m_deferCurrentGunSwap = false;
			for (int n = 0; n < gunSprites.Length; n++)
			{
				if (!gunSprites[n].renderer.enabled)
				{
					for (int num = 0; num < gunSpritesAndOutlines[n].Length; num++)
					{
						gunSpritesAndOutlines[n][num].renderer.enabled = true;
					}
				}
			}
		}
		PostFlipReset(newChild, gbTransform, placeholderCardObject, oldGunSprites, newGun);
		m_isCurrentlyFlipping = false;
	}

	private void PostFlipReset(Transform newChild, Transform gbTransform, GameObject placeholderCardObject, tk2dClippedSprite[] oldGunSprites, Gun newGun)
	{
		for (int i = 0; i < AdditionalGunBoxSprites.Count; i++)
		{
			(AdditionalGunBoxSprites[i] as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
		}
		if ((bool)newChild)
		{
			newChild.parent = gbTransform;
			GunBoxSprite.AddControl(newChild.GetComponent<dfControl>());
			newChild.GetComponent<dfControl>().RelativePosition = new Vector3((float)AdditionalBoxOffsetPx * Pixelator.Instance.CurrentTileScale, 0f, 0f);
		}
		UnityEngine.Object.Destroy(placeholderCardObject);
		for (int j = 0; j < oldGunSprites.Length; j++)
		{
			UnityEngine.Object.Destroy(oldGunSprites[j].gameObject);
		}
		m_currentGunSpriteXOffset = 0f;
		m_currentGunSpriteZOffset = 0f;
		GunBoxSprite.enabled = true;
		UpdateGunSprite(newGun, 0);
	}

	private void UpdateGunSprite(Gun newGun, int change, Gun secondaryGun = null)
	{
		if (newGun != m_cachedGun && !SuppressNextGunFlip && m_cachedGun != null)
		{
			DoGunCardFlip(newGun, change);
		}
		SuppressNextGunFlip = false;
		if (newGun.CurrentAmmo == 0)
		{
			AddNoAmmoIcon();
			UpdateNoAmmoIcon();
		}
		else
		{
			ClearNoAmmoIcon();
		}
		for (int i = 0; i < gunSprites.Length; i++)
		{
			Gun gun = ((i <= 0 || !secondaryGun) ? newGun : secondaryGun);
			tk2dBaseSprite tk2dBaseSprite2 = gun.GetSprite();
			int num = tk2dBaseSprite2.spriteId;
			tk2dSpriteCollectionData collection = tk2dBaseSprite2.Collection;
			if (gun.OnlyUsesIdleInWeaponBox)
			{
				num = gun.DefaultSpriteID;
			}
			else if ((bool)gun.weaponPanelSpriteOverride)
			{
				num = gun.weaponPanelSpriteOverride.GetMatch(num);
			}
			tk2dClippedSprite tk2dClippedSprite2 = gunSprites[i];
			tk2dClippedSprite2.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
			for (int j = 0; j < outlineSprites[i].Length; j++)
			{
				if (outlineSprites[i].Length > 1)
				{
					float num2 = ((j != 1) ? 0f : 0.0625f);
					num2 = ((j != 3) ? num2 : (-0.0625f));
					float num3 = ((j != 0) ? 0f : 0.0625f);
					num3 = ((j != 2) ? num3 : (-0.0625f));
					outlineSprites[i][j].transform.localPosition = (new Vector3(num2, num3, 0f) * Pixelator.Instance.CurrentTileScale * 16f * GameUIRoot.Instance.PixelsToUnits()).WithZ(UI_OUTLINE_DEPTH);
				}
				outlineSprites[i][j].scale = tk2dClippedSprite2.scale;
			}
			if (!m_deferCurrentGunSwap)
			{
				if (!tk2dClippedSprite2.renderer.enabled)
				{
					ToggleRenderers(true);
				}
				if (tk2dClippedSprite2.spriteId != num || tk2dClippedSprite2.Collection != collection)
				{
					tk2dClippedSprite2.SetSprite(collection, num);
					if (tk2dClippedSprite2.OverrideMaterialMode != tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE)
					{
						tk2dClippedSprite2.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
						if ((bool)tk2dClippedSprite2.renderer && tk2dClippedSprite2.renderer.material.shader.name.Contains("Gonner"))
						{
							tk2dClippedSprite2.renderer.material.shader = Shader.Find("tk2d/CutoutVertexColorTilted");
						}
					}
					tk2dClippedSprite2.renderer.material.EnableKeyword("SATURATION_ON");
					for (int k = 0; k < outlineSprites[i].Length; k++)
					{
						outlineSprites[i][k].SetSprite(collection, num);
						SpriteOutlineManager.ForceUpdateOutlineMaterial(outlineSprites[i][k], tk2dBaseSprite2);
					}
				}
			}
			Vector3 center = GunBoxSprite.GetCenter();
			if ((bool)secondaryGun)
			{
				center += CalculateLocalOffsetForGunInDualWieldMode(newGun, secondaryGun, i);
			}
			tk2dClippedSprite2.transform.position = center + GetOffsetVectorForGun((i <= 0 || !secondaryGun) ? newGun : secondaryGun, m_isCurrentlyFlipping);
			tk2dClippedSprite2.transform.position = tk2dClippedSprite2.transform.position.Quantize(GunBoxSprite.PixelsToUnits() * 3f);
		}
		if (!newGun.UsesRechargeLikeActiveItem && !newGun.IsUndertaleGun)
		{
			GunCooldownFillSprite.IsVisible = false;
			GunCooldownForegroundSprite.IsVisible = false;
		}
		else
		{
			GunCooldownForegroundSprite.RelativePosition = GunBoxSprite.RelativePosition;
			GunCooldownFillSprite.RelativePosition = GunBoxSprite.RelativePosition + new Vector3(123f, 3f, 0f);
			GunCooldownFillSprite.ZOrder = GunBoxSprite.ZOrder + 1;
			GunCooldownForegroundSprite.ZOrder = GunCooldownFillSprite.ZOrder + 1;
			GunCooldownFillSprite.IsVisible = true;
			GunCooldownForegroundSprite.IsVisible = true;
		}
		if (newGun.UsesRechargeLikeActiveItem || newGun.IsUndertaleGun)
		{
			GunCooldownFillSprite.FillAmount = newGun.CurrentActiveItemChargeAmount;
		}
	}

	private Vector3 CalculateLocalOffsetForGunInDualWieldMode(Gun primary, Gun secondary, int currentIndex)
	{
		float num = GunBoxSprite.PixelsToUnits();
		Vector2 vector = GunBoxSprite.Size * 0.5f * num;
		Bounds bounds = primary.GetSprite().GetBounds();
		Bounds bounds2 = secondary.GetSprite().GetBounds();
		Vector3 vector2 = vector + new Vector2(-8f * num, -8f * num) - Vector2.Scale(((currentIndex != 0) ? bounds2 : bounds).extents.XY(), gunSprites[0].scale.XY());
		if (currentIndex == 0)
		{
			return new Vector3(vector2.x, 0f - vector2.y, 0f);
		}
		return new Vector3(0f - vector2.x, vector2.y, 0f);
	}

	public Vector2 GetOffsetVectorForSpecificSprite(tk2dBaseSprite targetSprite, bool isFlippingGun)
	{
		tk2dSpriteDefinition currentSpriteDef = targetSprite.GetCurrentSpriteDef();
		Vector3 vector = Vector3.Scale(-currentSpriteDef.GetBounds().min + -currentSpriteDef.GetBounds().extents, gunSprites[0].scale);
		if (isFlippingGun)
		{
			vector += new Vector3(m_currentGunSpriteXOffset, 0f, m_currentGunSpriteZOffset);
		}
		return vector;
	}

	public Vector3 GetOffsetVectorForGun(Gun newGun, bool isFlippingGun)
	{
		tk2dSpriteDefinition tk2dSpriteDefinition2 = null;
		tk2dSpriteDefinition2 = ((m_cachedGunSpriteDefinition == null || isFlippingGun) ? newGun.GetSprite().Collection.spriteDefinitions[newGun.DefaultSpriteID] : m_cachedGunSpriteDefinition);
		Vector3 result = Vector3.Scale(-tk2dSpriteDefinition2.GetBounds().min + -tk2dSpriteDefinition2.GetBounds().extents, gunSprites[0].scale);
		if (isFlippingGun)
		{
			result += new Vector3(m_currentGunSpriteXOffset, 0f, m_currentGunSpriteZOffset);
		}
		return result;
	}

	protected void RebuildExtraGunCards(GunInventory guns)
	{
		Debug.Log("REBUILDING EXTRA GUN CARDS");
		float num = m_panel.PixelsToUnits();
		for (int i = 0; i < AdditionalGunBoxSprites.Count; i++)
		{
			AdditionalGunBoxSprites[i].transform.parent = null;
			UnityEngine.Object.Destroy(AdditionalGunBoxSprites[i].gameObject);
		}
		AdditionalGunBoxSprites.Clear();
		dfControl dfControl2 = GunBoxSprite;
		Transform parent = GunBoxSprite.transform;
		int num2 = Mathf.Min(guns.AllGuns.Count - 1, NumberOfAdditionalGunCards);
		for (int j = 0; j < num2; j++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ExtraGunCardPrefab);
			gameObject.transform.parent = parent;
			dfControl component = gameObject.GetComponent<dfControl>();
			dfControl2.AddControl(component);
			component.RelativePosition = new Vector3((float)AdditionalBoxOffsetPx * Pixelator.Instance.CurrentTileScale, 0f, 0f);
			dfControl2 = component;
			parent = gameObject.transform;
			AdditionalGunBoxSprites.Add(component);
		}
		float num3 = (float)((!IsLeftAligned) ? 1 : (-1)) * Pixelator.Instance.CurrentTileScale * (float)(m_cachedNumberModules - 1) * -10f * num;
		float num4 = (float)(-AdditionalBoxOffsetPx * AdditionalGunBoxSprites.Count + -GUN_BOX_EXTRA_PX_OFFSET) * Pixelator.Instance.CurrentTileScale * num;
		if (IsLeftAligned)
		{
			GunBoxSprite.transform.position = GunBoxSprite.transform.position.WithX(m_panel.transform.position.x - m_panel.Width * num + num4 + num3);
		}
		else
		{
			GunBoxSprite.transform.position = m_panel.transform.position + new Vector3(num4 + num3, 0f, 0f);
		}
		GunBoxSprite.Invalidate();
	}

	private GameUIAmmoType GetUIAmmoType(GameUIAmmoType.AmmoType sourceType, string customType)
	{
		GameUIAmmoType[] array = ammoTypes;
		if (IsLeftAligned)
		{
			for (int i = 0; i < GameUIRoot.Instance.ammoControllers.Count; i++)
			{
				if (!GameUIRoot.Instance.ammoControllers[i].IsLeftAligned)
				{
					array = GameUIRoot.Instance.ammoControllers[i].ammoTypes;
					break;
				}
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (sourceType == GameUIAmmoType.AmmoType.CUSTOM)
			{
				if (array[j].ammoType == GameUIAmmoType.AmmoType.CUSTOM && array[j].customAmmoType == customType)
				{
					return array[j];
				}
			}
			else if (array[j].ammoType == sourceType)
			{
				return array[j];
			}
		}
		return array[0];
	}

	public void TriggerUIDisabled()
	{
		if (GameUIRoot.Instance.ForceHideGunPanel)
		{
			ToggleRenderers(false);
		}
	}

	private void CleanupLists(int numberModules)
	{
		for (int num = fgSpritesForModules.Count - 1; num >= numberModules; num--)
		{
			if ((bool)fgSpritesForModules[num])
			{
				UnityEngine.Object.Destroy(fgSpritesForModules[num].gameObject);
				fgSpritesForModules.RemoveAt(num);
			}
			if ((bool)bgSpritesForModules[num])
			{
				UnityEngine.Object.Destroy(bgSpritesForModules[num].gameObject);
				bgSpritesForModules.RemoveAt(num);
			}
			cachedAmmoTypesForModules.RemoveAt(num);
			cachedCustomAmmoTypesForModules.RemoveAt(num);
		}
		for (int num2 = addlFgSpritesForModules.Count - 1; num2 >= numberModules; num2--)
		{
			if (addlFgSpritesForModules[num2] != null)
			{
				for (int num3 = addlFgSpritesForModules[num2].Count - 1; num3 >= 0; num3--)
				{
					UnityEngine.Object.Destroy(addlFgSpritesForModules[num2][num3].gameObject);
					UnityEngine.Object.Destroy(addlBgSpritesForModules[num2][num3].gameObject);
				}
				addlFgSpritesForModules.RemoveAt(num2);
				addlBgSpritesForModules.RemoveAt(num2);
			}
		}
		for (int num4 = topCapsForModules.Count - 1; num4 >= numberModules; num4--)
		{
			if ((bool)topCapsForModules[num4])
			{
				UnityEngine.Object.Destroy(topCapsForModules[num4].gameObject);
				topCapsForModules.RemoveAt(num4);
			}
			if ((bool)bottomCapsForModules[num4])
			{
				UnityEngine.Object.Destroy(bottomCapsForModules[num4].gameObject);
				bottomCapsForModules.RemoveAt(num4);
			}
		}
		for (int num5 = m_cachedModuleShotsRemaining.Count - 1; num5 >= numberModules; num5--)
		{
			m_cachedModuleShotsRemaining.RemoveAt(num5);
		}
	}

	private void EnsureInitialization(int usedModuleIndex)
	{
		if (usedModuleIndex >= fgSpritesForModules.Count)
		{
			fgSpritesForModules.Add(null);
		}
		if (usedModuleIndex >= bgSpritesForModules.Count)
		{
			bgSpritesForModules.Add(null);
		}
		if (usedModuleIndex >= addlFgSpritesForModules.Count)
		{
			addlFgSpritesForModules.Add(new List<dfTiledSprite>());
		}
		if (usedModuleIndex >= addlBgSpritesForModules.Count)
		{
			addlBgSpritesForModules.Add(new List<dfTiledSprite>());
		}
		if (usedModuleIndex >= cachedAmmoTypesForModules.Count)
		{
			cachedAmmoTypesForModules.Add(GameUIAmmoType.AmmoType.SMALL_BULLET);
		}
		if (usedModuleIndex >= cachedCustomAmmoTypesForModules.Count)
		{
			cachedCustomAmmoTypesForModules.Add(string.Empty);
		}
		if (usedModuleIndex >= topCapsForModules.Count)
		{
			dfSprite item = topCapsForModules[0].Parent.AddPrefab(topCapsForModules[0].gameObject) as dfSprite;
			dfSprite item2 = bottomCapsForModules[0].Parent.AddPrefab(bottomCapsForModules[0].gameObject) as dfSprite;
			topCapsForModules.Add(item);
			bottomCapsForModules.Add(item2);
		}
		if (usedModuleIndex >= m_cachedModuleShotsRemaining.Count)
		{
			m_cachedModuleShotsRemaining.Add(0);
		}
	}

	public void UpdateUIGun(GunInventory guns, int inventoryShift)
	{
		if (!m_initialized)
		{
			Initialize();
		}
		if (guns.AllGuns.Count != 0 && guns.AllGuns.Count - 1 != AdditionalGunBoxSprites.Count && GameUIRoot.Instance.GunventoryFolded && !m_isCurrentlyFlipping && (guns.AllGuns.Count - 1 < AdditionalGunBoxSprites.Count || AdditionalGunBoxSprites.Count < NumberOfAdditionalGunCards))
		{
			RebuildExtraGunCards(guns);
		}
		Gun currentGun = guns.CurrentGun;
		Gun currentSecondaryGun = guns.CurrentSecondaryGun;
		if (currentGun == null || GameUIRoot.Instance.ForceHideGunPanel || temporarilyPreventVisible || forceInvisiblePermanent)
		{
			ToggleRenderers(false);
			return;
		}
		GunQuickSwitchIcon.IsVisible = false;
		int num = 0;
		for (int i = 0; i < currentGun.Volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = currentGun.Volley.projectiles[i];
			if (projectileModule == currentGun.DefaultModule || (projectileModule.IsDuctTapeModule && projectileModule.ammoCost > 0))
			{
				num++;
			}
		}
		if ((bool)currentSecondaryGun)
		{
			for (int j = 0; j < currentSecondaryGun.Volley.projectiles.Count; j++)
			{
				ProjectileModule projectileModule2 = currentSecondaryGun.Volley.projectiles[j];
				if (projectileModule2 == currentSecondaryGun.DefaultModule || (projectileModule2.IsDuctTapeModule && projectileModule2.ammoCost > 0))
				{
					num++;
				}
			}
		}
		bool didChangeGun = currentGun != m_cachedGun || currentGun.DidTransformGunThisFrame;
		currentGun.DidTransformGunThisFrame = false;
		UpdateGunSprite(currentGun, inventoryShift, currentSecondaryGun);
		float num2 = 0f;
		if (num != m_cachedNumberModules)
		{
			int num3 = num - m_cachedNumberModules;
			num2 = (float)((!IsLeftAligned) ? 1 : (-1)) * Pixelator.Instance.CurrentTileScale * (float)num3 * -10f;
			GunAmmoCountLabel.RelativePosition += new Vector3(num2, 0f, 0f);
			GunBoxSprite.RelativePosition += new Vector3(num2, 0f, 0f);
		}
		if (m_cachedTotalAmmo != currentGun.CurrentAmmo || m_cachedMaxAmmo != currentGun.AdjustedMaxAmmo || m_cachedUndertaleness != currentGun.IsUndertaleGun)
		{
			if (currentGun.IsUndertaleGun)
			{
				if (!IsLeftAligned && m_cachedMaxAmmo == int.MaxValue)
				{
					GunAmmoCountLabel.RelativePosition += new Vector3(3f, 0f, 0f);
				}
				GunAmmoCountLabel.Text = "0/0";
			}
			else if (currentGun.InfiniteAmmo)
			{
				if (!IsLeftAligned && (!m_cachedGun || !m_cachedGun.InfiniteAmmo))
				{
					GunAmmoCountLabel.RelativePosition += new Vector3(-3f, 0f, 0f);
				}
				GunAmmoCountLabel.ProcessMarkup = true;
				GunAmmoCountLabel.ColorizeSymbols = false;
				GunAmmoCountLabel.Text = "[sprite \"infinite-big\"]";
			}
			else if (currentGun.AdjustedMaxAmmo > 0)
			{
				if (!IsLeftAligned && m_cachedMaxAmmo == int.MaxValue)
				{
					GunAmmoCountLabel.RelativePosition += new Vector3(3f, 0f, 0f);
				}
				GunAmmoCountLabel.Text = currentGun.CurrentAmmo + "/" + currentGun.AdjustedMaxAmmo;
			}
			else
			{
				if (!IsLeftAligned && m_cachedMaxAmmo == int.MaxValue)
				{
					GunAmmoCountLabel.RelativePosition += new Vector3(3f, 0f, 0f);
				}
				GunAmmoCountLabel.Text = currentGun.CurrentAmmo.ToString();
			}
		}
		CleanupLists(num);
		int num4 = 0;
		int num5 = currentGun.Volley.projectiles.Count;
		if ((bool)currentSecondaryGun)
		{
			num5 += currentSecondaryGun.Volley.projectiles.Count;
		}
		for (int k = 0; k < num5; k++)
		{
			Gun gun = ((k < currentGun.Volley.projectiles.Count) ? currentGun : currentSecondaryGun);
			int index = ((!(gun == currentGun)) ? (k - currentGun.Volley.projectiles.Count) : k);
			ProjectileModule projectileModule3 = gun.Volley.projectiles[index];
			if (projectileModule3 == gun.DefaultModule || (projectileModule3.IsDuctTapeModule && projectileModule3.ammoCost > 0))
			{
				EnsureInitialization(num4);
				dfTiledSprite currentAmmoFGSprite = fgSpritesForModules[num4];
				dfTiledSprite currentAmmoBGSprite = bgSpritesForModules[num4];
				List<dfTiledSprite> addlModuleFGSprites = addlFgSpritesForModules[num4];
				List<dfTiledSprite> addlModuleBGSprites = addlBgSpritesForModules[num4];
				dfSprite moduleTopCap = topCapsForModules[num4];
				dfSprite moduleBottomCap = bottomCapsForModules[num4];
				GameUIAmmoType.AmmoType cachedAmmoTypeForModule = cachedAmmoTypesForModules[num4];
				string cachedCustomAmmoTypeForModule = cachedCustomAmmoTypesForModules[num4];
				int cachedShotsInClip = m_cachedModuleShotsRemaining[num4];
				UpdateAmmoUIForModule(ref currentAmmoFGSprite, ref currentAmmoBGSprite, addlModuleFGSprites, addlModuleBGSprites, moduleTopCap, moduleBottomCap, projectileModule3, gun, ref cachedAmmoTypeForModule, ref cachedCustomAmmoTypeForModule, ref cachedShotsInClip, didChangeGun, num - (num4 + 1));
				fgSpritesForModules[num4] = currentAmmoFGSprite;
				bgSpritesForModules[num4] = currentAmmoBGSprite;
				cachedAmmoTypesForModules[num4] = cachedAmmoTypeForModule;
				cachedCustomAmmoTypesForModules[num4] = cachedCustomAmmoTypeForModule;
				m_cachedModuleShotsRemaining[num4] = cachedShotsInClip;
				num4++;
			}
		}
		if (currentGun.IsHeroSword)
		{
			for (int l = 0; l < bgSpritesForModules.Count; l++)
			{
				fgSpritesForModules[l].IsVisible = false;
				bgSpritesForModules[l].IsVisible = false;
			}
			for (int m = 0; m < topCapsForModules.Count; m++)
			{
				topCapsForModules[m].IsVisible = false;
				bottomCapsForModules[m].IsVisible = false;
			}
		}
		else if (!bottomCapsForModules[0].IsVisible)
		{
			for (int n = 0; n < bgSpritesForModules.Count; n++)
			{
				fgSpritesForModules[n].IsVisible = true;
				bgSpritesForModules[n].IsVisible = true;
			}
			for (int num6 = 0; num6 < topCapsForModules.Count; num6++)
			{
				topCapsForModules[num6].IsVisible = true;
				bottomCapsForModules[num6].IsVisible = true;
			}
		}
		GunClipCountLabel.IsVisible = false;
		m_cachedGun = currentGun;
		m_cachedNumberModules = num;
		m_cachedTotalAmmo = currentGun.CurrentAmmo;
		m_cachedMaxAmmo = currentGun.AdjustedMaxAmmo;
		m_cachedUndertaleness = currentGun.IsUndertaleGun;
		UpdateAdditionalSprites();
	}

	private void UpdateAmmoUIForModule(ref dfTiledSprite currentAmmoFGSprite, ref dfTiledSprite currentAmmoBGSprite, List<dfTiledSprite> AddlModuleFGSprites, List<dfTiledSprite> AddlModuleBGSprites, dfSprite ModuleTopCap, dfSprite ModuleBottomCap, ProjectileModule module, Gun currentGun, ref GameUIAmmoType.AmmoType cachedAmmoTypeForModule, ref string cachedCustomAmmoTypeForModule, ref int cachedShotsInClip, bool didChangeGun, int numberRemaining)
	{
		int num = ((module.GetModNumberOfShotsInClip(currentGun.CurrentOwner) > 0) ? (module.GetModNumberOfShotsInClip(currentGun.CurrentOwner) - currentGun.RuntimeModuleData[module].numberShotsFired) : currentGun.ammo);
		if (num > currentGun.ammo)
		{
			num = currentGun.ammo;
		}
		int num2 = ((module.GetModNumberOfShotsInClip(currentGun.CurrentOwner) > 0) ? module.GetModNumberOfShotsInClip(currentGun.CurrentOwner) : currentGun.AdjustedMaxAmmo);
		if (currentGun.RequiresFundsToShoot)
		{
			num = Mathf.FloorToInt((float)(currentGun.CurrentOwner as PlayerController).carriedConsumables.Currency / (float)currentGun.CurrencyCostPerShot);
			num2 = Mathf.FloorToInt((float)(currentGun.CurrentOwner as PlayerController).carriedConsumables.Currency / (float)currentGun.CurrencyCostPerShot);
		}
		if (currentAmmoFGSprite == null || didChangeGun || module.ammoType != cachedAmmoTypeForModule || module.customAmmoType != cachedCustomAmmoTypeForModule)
		{
			m_additionalAmmoTypeDefinitions.Clear();
			if (currentAmmoFGSprite != null)
			{
				UnityEngine.Object.Destroy(currentAmmoFGSprite.gameObject);
			}
			if (currentAmmoBGSprite != null)
			{
				UnityEngine.Object.Destroy(currentAmmoBGSprite.gameObject);
			}
			for (int i = 0; i < AddlModuleBGSprites.Count; i++)
			{
				UnityEngine.Object.Destroy(AddlModuleBGSprites[i].gameObject);
				UnityEngine.Object.Destroy(AddlModuleFGSprites[i].gameObject);
			}
			AddlModuleBGSprites.Clear();
			AddlModuleFGSprites.Clear();
			GameUIAmmoType uIAmmoType = GetUIAmmoType(module.ammoType, module.customAmmoType);
			GameObject gameObject = UnityEngine.Object.Instantiate(uIAmmoType.ammoBarFG.gameObject);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(uIAmmoType.ammoBarBG.gameObject);
			gameObject.transform.parent = GunBoxSprite.transform.parent;
			gameObject2.transform.parent = GunBoxSprite.transform.parent;
			gameObject.name = uIAmmoType.ammoBarFG.name;
			gameObject2.name = uIAmmoType.ammoBarBG.name;
			currentAmmoFGSprite = gameObject.GetComponent<dfTiledSprite>();
			currentAmmoBGSprite = gameObject2.GetComponent<dfTiledSprite>();
			m_panel.AddControl(currentAmmoFGSprite);
			m_panel.AddControl(currentAmmoBGSprite);
			currentAmmoFGSprite.EnableBlackLineFix = module.shootStyle == ProjectileModule.ShootStyle.Beam;
			currentAmmoBGSprite.EnableBlackLineFix = currentAmmoFGSprite.EnableBlackLineFix;
			if (module.usesOptionalFinalProjectile)
			{
				GameUIAmmoType uIAmmoType2 = GetUIAmmoType(module.finalAmmoType, module.finalCustomAmmoType);
				m_additionalAmmoTypeDefinitions.Add(uIAmmoType2);
				gameObject = UnityEngine.Object.Instantiate(uIAmmoType2.ammoBarFG.gameObject);
				gameObject2 = UnityEngine.Object.Instantiate(uIAmmoType2.ammoBarBG.gameObject);
				gameObject.transform.parent = GunBoxSprite.transform.parent;
				gameObject2.transform.parent = GunBoxSprite.transform.parent;
				gameObject.name = uIAmmoType2.ammoBarFG.name;
				gameObject2.name = uIAmmoType2.ammoBarBG.name;
				AddlModuleFGSprites.Add(gameObject.GetComponent<dfTiledSprite>());
				AddlModuleBGSprites.Add(gameObject2.GetComponent<dfTiledSprite>());
				m_panel.AddControl(AddlModuleFGSprites[0]);
				m_panel.AddControl(AddlModuleBGSprites[0]);
			}
		}
		float currentTileScale = Pixelator.Instance.CurrentTileScale;
		int num3 = (module.usesOptionalFinalProjectile ? module.GetModifiedNumberOfFinalProjectiles(currentGun.CurrentOwner) : 0);
		int num4 = num2 - num3;
		int num5 = Mathf.Max(0, num - num3);
		int num6 = Mathf.Min(num3, num);
		int a = 125;
		if (module.shootStyle == ProjectileModule.ShootStyle.Beam)
		{
			a = 500;
			num3 = Mathf.CeilToInt((float)num3 / 2f);
			num4 = Mathf.CeilToInt((float)num4 / 2f);
			num5 = Mathf.CeilToInt((float)num5 / 2f);
			num6 = Mathf.CeilToInt((float)num6 / 2f);
		}
		num4 = Mathf.Min(a, num4);
		num5 = Mathf.Min(a, num5);
		currentAmmoBGSprite.Size = new Vector2(currentAmmoBGSprite.SpriteInfo.sizeInPixels.x * currentTileScale, currentAmmoBGSprite.SpriteInfo.sizeInPixels.y * currentTileScale * (float)num4);
		currentAmmoFGSprite.Size = new Vector2(currentAmmoFGSprite.SpriteInfo.sizeInPixels.x * currentTileScale, currentAmmoFGSprite.SpriteInfo.sizeInPixels.y * currentTileScale * (float)num5);
		for (int j = 0; j < AddlModuleBGSprites.Count; j++)
		{
			AddlModuleBGSprites[j].Size = new Vector2(AddlModuleBGSprites[j].SpriteInfo.sizeInPixels.x * currentTileScale, AddlModuleBGSprites[j].SpriteInfo.sizeInPixels.y * currentTileScale * (float)num3);
			AddlModuleFGSprites[j].Size = new Vector2(AddlModuleFGSprites[j].SpriteInfo.sizeInPixels.x * currentTileScale, AddlModuleFGSprites[j].SpriteInfo.sizeInPixels.y * currentTileScale * (float)num6);
		}
		if (!didChangeGun && AmmoBurstVFX != null && cachedShotsInClip > num && !currentGun.IsReloading)
		{
			int num7 = cachedShotsInClip - num;
			for (int k = 0; k < num7; k++)
			{
				GameObject gameObject3 = UnityEngine.Object.Instantiate(AmmoBurstVFX.gameObject);
				dfSprite component = gameObject3.GetComponent<dfSprite>();
				dfSpriteAnimation component2 = gameObject3.GetComponent<dfSpriteAnimation>();
				component.ZOrder = currentAmmoFGSprite.ZOrder + 1;
				float num8 = component.Size.y / 2f;
				currentAmmoFGSprite.AddControl(component);
				component.transform.position = currentAmmoFGSprite.GetCenter();
				component.RelativePosition = component.RelativePosition.WithY((float)k * currentAmmoFGSprite.SpriteInfo.sizeInPixels.y * currentTileScale - num8);
				if (num5 == 0 && num3 > 0)
				{
					component.RelativePosition += new Vector3(0f, AddlModuleFGSprites[0].SpriteInfo.sizeInPixels.y * currentTileScale * Mathf.Max(0f, (float)(num3 - num6) - 0.5f), 0f);
				}
				component2.Play();
			}
		}
		float num9 = currentTileScale * (float)numberRemaining * -10f;
		float num10 = 0f - Pixelator.Instance.CurrentTileScale + num9;
		float num11 = 0f;
		float num12 = ((AddlModuleBGSprites.Count <= 0) ? 0f : AddlModuleBGSprites[0].Size.y);
		if (IsLeftAligned)
		{
			ModuleBottomCap.RelativePosition = m_panel.Size.WithX(0f).ToVector3ZUp() - ModuleBottomCap.Size.WithX(0f).ToVector3ZUp() + new Vector3(0f - num10, num11, 0f);
		}
		else
		{
			ModuleBottomCap.RelativePosition = m_panel.Size.ToVector3ZUp() - ModuleBottomCap.Size.ToVector3ZUp() + new Vector3(num10, 0f - num11, 0f);
		}
		ModuleTopCap.RelativePosition = ModuleBottomCap.RelativePosition + new Vector3(0f, 0f - currentAmmoBGSprite.Size.y + (0f - num12) + (0f - ModuleTopCap.Size.y), 0f);
		float num13 = ModuleTopCap.Size.x / 2f;
		float num14 = BraveMathCollege.QuantizeFloat(currentAmmoBGSprite.Size.x / 2f - num13, currentTileScale);
		float num15 = currentAmmoFGSprite.Size.x / 2f - num13;
		currentAmmoBGSprite.RelativePosition = ModuleTopCap.RelativePosition + new Vector3(0f - num14, ModuleTopCap.Size.y, 0f);
		currentAmmoFGSprite.RelativePosition = ModuleTopCap.RelativePosition + new Vector3(0f - num15, ModuleTopCap.Size.y + currentAmmoFGSprite.SpriteInfo.sizeInPixels.y * (float)(num4 - num5) * currentTileScale, 0f);
		currentAmmoFGSprite.ZOrder = currentAmmoBGSprite.ZOrder + 1;
		if (AddlModuleBGSprites.Count > 0)
		{
			num14 = BraveMathCollege.QuantizeFloat(AddlModuleBGSprites[0].Size.x / 2f - num13, currentTileScale);
			AddlModuleBGSprites[0].RelativePosition = ModuleTopCap.RelativePosition + new Vector3(0f - num14, ModuleTopCap.Size.y + currentAmmoBGSprite.Size.y, 0f);
			num15 = AddlModuleFGSprites[0].Size.x / 2f - num13;
			AddlModuleFGSprites[0].RelativePosition = ModuleTopCap.RelativePosition + new Vector3(0f - num15, ModuleTopCap.Size.y + currentAmmoBGSprite.Size.y + AddlModuleFGSprites[0].SpriteInfo.sizeInPixels.y * (float)(num3 - num6) * currentTileScale, 0f);
		}
		cachedAmmoTypeForModule = module.ammoType;
		cachedCustomAmmoTypeForModule = module.customAmmoType;
		cachedShotsInClip = num;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
