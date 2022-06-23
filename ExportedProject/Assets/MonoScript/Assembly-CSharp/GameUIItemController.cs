using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIItemController : MonoBehaviour
{
	public dfSprite ItemBoxSprite;

	public dfSprite ItemBoxFillSprite;

	public dfSprite ItemBoxFGSprite;

	public tk2dClippedSprite itemSprite;

	public GameObject ExtraItemCardPrefab;

	public List<dfControl> AdditionalItemBoxSprites = new List<dfControl>();

	public dfLabel ItemCountLabel;

	[NonSerialized]
	public bool temporarilyPreventVisible;

	public bool IsRightAligned;

	private Material itemSpriteMaterial;

	private tk2dSprite[] outlineSprites;

	private PlayerItem m_cachedItem;

	private bool m_initialized;

	private Material m_ClippedMaterial;

	private Material m_ClippedZWriteOffMaterial;

	private dfPanel m_panel;

	private float UI_OUTLINE_DEPTH = 1f;

	private tk2dSpriteDefinition m_cachedItemSpriteDefinition;

	private bool m_isCurrentlyFlipping;

	private float m_currentItemSpriteXOffset;

	private float m_currentItemSpriteZOffset;

	private bool m_deferCurrentItemSwap;

	private bool m_cardFlippedQueued;

	private const float FLIP_TIME = 0.15f;

	private int AdditionalBoxOffsetPX
	{
		get
		{
			return (!IsRightAligned) ? 2 : (-2);
		}
	}

	private void Update()
	{
		if (temporarilyPreventVisible && (bool)itemSprite && itemSprite.renderer.enabled)
		{
			ToggleRenderers(false);
		}
		if (!GameManager.Instance.IsLoadingLevel && Minimap.Instance.IsFullscreen && (bool)itemSprite && (bool)itemSprite.renderer && itemSprite.renderer.enabled)
		{
			itemSprite.renderer.enabled = false;
		}
	}

	private void Initialize()
	{
		m_panel = GetComponent<dfPanel>();
		itemSprite.usesOverrideMaterial = true;
		itemSpriteMaterial = itemSprite.renderer.material;
		SpriteOutlineManager.AddOutlineToSprite(itemSprite, Color.white);
		outlineSprites = SpriteOutlineManager.GetOutlineSprites(itemSprite);
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			if (outlineSprites.Length > 1)
			{
				float num = ((i != 1) ? 0f : 0.0625f);
				num = ((i != 3) ? num : (-0.0625f));
				float num2 = ((i != 0) ? 0f : 0.0625f);
				num2 = ((i != 2) ? num2 : (-0.0625f));
				outlineSprites[i].transform.localPosition = (new Vector3(num, num2, 0f) * Pixelator.Instance.CurrentTileScale * 16f * GameUIRoot.Instance.PixelsToUnits()).WithZ(UI_OUTLINE_DEPTH);
			}
			outlineSprites[i].gameObject.layer = itemSprite.gameObject.layer;
		}
		m_ClippedMaterial = new Material(ShaderCache.Acquire("Daikon Forge/Clipped UI Shader"));
		m_ClippedZWriteOffMaterial = new Material(ShaderCache.Acquire("Daikon Forge/Clipped UI Shader ZWriteOff"));
		m_initialized = true;
	}

	public void ToggleRenderersOld(bool value)
	{
		if ((bool)ItemBoxSprite)
		{
			ItemBoxSprite.IsVisible = value;
		}
		if ((bool)ItemCountLabel)
		{
			SetItemCountVisible(value);
		}
		if (!(itemSprite != null))
		{
			return;
		}
		itemSprite.renderer.enabled = value;
		outlineSprites = SpriteOutlineManager.GetOutlineSprites(itemSprite);
		if (outlineSprites != null)
		{
			for (int i = 0; i < outlineSprites.Length; i++)
			{
				outlineSprites[i].renderer.enabled = value;
			}
		}
	}

	public void ToggleRenderers(bool value)
	{
		itemSprite.renderer.enabled = value;
		if (ItemBoxSprite != null && ItemBoxSprite.Parent != null)
		{
			ItemBoxSprite.IsVisible = value;
		}
		if (ItemBoxSprite != null)
		{
			ItemBoxSprite.IsVisible = value;
		}
		if (ItemCountLabel != null && !value)
		{
			SetItemCountVisible(value);
		}
		for (int i = 0; i < AdditionalItemBoxSprites.Count; i++)
		{
			AdditionalItemBoxSprites[i].IsVisible = value;
			AdditionalItemBoxSprites[i].IsVisible = value;
		}
		outlineSprites = SpriteOutlineManager.GetOutlineSprites(itemSprite);
		for (int j = 0; j < outlineSprites.Length; j++)
		{
			if ((bool)outlineSprites[j])
			{
				outlineSprites[j].renderer.enabled = value;
			}
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

	private void DoItemCardFlip(PlayerItem newItem, int change)
	{
		if (AdditionalItemBoxSprites.Count == 0)
		{
			return;
		}
		if (!m_isCurrentlyFlipping)
		{
			if (change > 0)
			{
				StartCoroutine(HandleItemCardFlipReverse(newItem));
			}
			else
			{
				StartCoroutine(HandleItemCardFlip(newItem));
			}
		}
		else if (!m_cardFlippedQueued)
		{
			StartCoroutine(WaitForCurrentItemFlipToEnd(newItem, change));
		}
	}

	private IEnumerator WaitForCurrentItemFlipToEnd(PlayerItem newItem, int change)
	{
		m_cardFlippedQueued = true;
		while (m_isCurrentlyFlipping)
		{
			yield return null;
		}
		if (change > 0)
		{
			m_deferCurrentItemSwap = true;
		}
		m_isCurrentlyFlipping = true;
		yield return null;
		m_cardFlippedQueued = false;
		if (change > 0)
		{
			StartCoroutine(HandleItemCardFlipReverse(newItem));
		}
		else
		{
			StartCoroutine(HandleItemCardFlip(newItem));
		}
	}

	private IEnumerator HandleItemCardFlipReverse(PlayerItem newGun)
	{
		m_deferCurrentItemSwap = true;
		m_isCurrentlyFlipping = true;
		yield return null;
		float elapsed = 0f;
		float p2u = ItemBoxSprite.PixelsToUnits();
		Transform gbTransform = ItemBoxSprite.transform;
		GameObject placeholderCardObject = UnityEngine.Object.Instantiate(ExtraItemCardPrefab);
		dfControl placeholderCard = placeholderCardObject.GetComponent<dfControl>();
		Transform placeholderTransform = placeholderCardObject.transform;
		placeholderTransform.parent = m_panel.transform;
		m_panel.AddControl(placeholderCard);
		placeholderCard.RelativePosition = ItemBoxSprite.RelativePosition;
		m_cachedItemSpriteDefinition = m_cachedItem.sprite.Collection.spriteDefinitions[m_cachedItem.sprite.spriteId];
		m_currentItemSpriteZOffset = -2f;
		for (int i = 0; i < AdditionalItemBoxSprites.Count; i++)
		{
			(AdditionalItemBoxSprites[i] as dfTextureSprite).Material = m_ClippedMaterial;
			AdditionalItemBoxSprites[i].Invalidate();
		}
		Vector3 cachedPosition2 = Vector3.zero;
		Transform firstExtraGunCardTransform = gbTransform.GetChild(0);
		firstExtraGunCardTransform.parent = m_panel.transform;
		m_panel.AddControl(firstExtraGunCardTransform.GetComponent<dfControl>());
		cachedPosition2 = firstExtraGunCardTransform.position;
		(placeholderCard as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
		Transform leafExtraCardTransform = (placeholderTransform.parent = firstExtraGunCardTransform.GetFirstLeafChild());
		leafExtraCardTransform.GetComponent<dfControl>().AddControl(placeholderCard);
		ItemBoxSprite.IsVisible = false;
		tk2dClippedSprite newGunSprite = UnityEngine.Object.Instantiate(itemSprite.gameObject, itemSprite.transform.position, Quaternion.identity).GetComponent<tk2dClippedSprite>();
		newGunSprite.transform.parent = itemSprite.transform.parent;
		newGunSprite.transform.position = newGunSprite.transform.position.WithZ(5f);
		Vector3 startPosition = placeholderTransform.position + new Vector3(Pixelator.Instance.CurrentTileScale * (float)(-AdditionalBoxOffsetPX) * (float)AdditionalItemBoxSprites.Count * p2u, 0f, 0f);
		tk2dBaseSprite weapSprite = newGun.sprite;
		UpdateItemSpriteScale();
		tk2dBaseSprite[] oldGunSpriteAndOutlines = newGunSprite.GetComponentsInChildren<tk2dBaseSprite>();
		for (int j = 0; j < oldGunSpriteAndOutlines.Length; j++)
		{
			oldGunSpriteAndOutlines[j].scale = newGunSprite.scale;
			oldGunSpriteAndOutlines[j].SetSprite(weapSprite.Collection, weapSprite.spriteId);
			SpriteOutlineManager.ForceUpdateOutlineMaterial(oldGunSpriteAndOutlines[j], weapSprite);
		}
		Bounds itemBounds = newGunSprite.GetUntrimmedBounds();
		bool hasDepthSwapped = false;
		float adjFlipTime = 0.15f * (float)((AdditionalItemBoxSprites.Count <= 20) ? 1 : (AdditionalItemBoxSprites.Count / 20));
		while (elapsed < adjFlipTime && GameUIRoot.Instance.GunventoryFolded)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / adjFlipTime;
			if (t >= 0.5f && !hasDepthSwapped)
			{
				hasDepthSwapped = true;
				placeholderTransform.parent = m_panel.transform;
				m_panel.AddControl(placeholderCard);
				firstExtraGunCardTransform.parent = placeholderTransform;
				placeholderCard.AddControl(firstExtraGunCardTransform.GetComponent<dfControl>());
				(placeholderCard as dfTextureSprite).Material = m_ClippedMaterial;
				for (int k = 0; k < AdditionalItemBoxSprites.Count; k++)
				{
					(AdditionalItemBoxSprites[k] as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
				}
				m_currentItemSpriteZOffset = 5f;
			}
			float xOffset = BraveMathCollege.DoubleLerp(0f, (float)(AdditionalItemBoxSprites.Count * AdditionalBoxOffsetPX + AdditionalBoxOffsetPX * 2) * Pixelator.Instance.CurrentTileScale * -1f, (float)(AdditionalItemBoxSprites.Count * AdditionalBoxOffsetPX) * Pixelator.Instance.CurrentTileScale, t);
			float yOffset = BraveMathCollege.DoubleLerpSmooth(0f, 9f * Pixelator.Instance.CurrentTileScale, 0f, t);
			float zRotation = BraveMathCollege.DoubleLerp(0f, 20f, 0f, Mathf.Clamp01(t * 1.1f));
			placeholderTransform.position = startPosition + new Vector3(xOffset * p2u, yOffset * p2u, 0f);
			placeholderTransform.rotation = Quaternion.Euler(0f, 0f, zRotation);
			Vector3 center = placeholderCard.GetCenter();
			Vector3 vector = newGunSprite.transform.rotation * new Vector3(0f - itemBounds.extents.x, itemBounds.extents.y * -1f, itemBounds.extents.z);
			newGunSprite.transform.position = center.WithZ((!hasDepthSwapped) ? 5f : (center.z - 2f)) + vector;
			newGunSprite.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
			float extraCardXOffset = BraveMathCollege.SmoothLerp((float)AdditionalBoxOffsetPX * Pixelator.Instance.CurrentTileScale * p2u, 0f, t);
			m_currentItemSpriteXOffset = (float)(-AdditionalBoxOffsetPX) * Pixelator.Instance.CurrentTileScale * p2u + extraCardXOffset;
			firstExtraGunCardTransform.position = cachedPosition2 + new Vector3(extraCardXOffset, 0f, 0f);
			firstExtraGunCardTransform.rotation = Quaternion.identity;
			yield return null;
			if (!newGunSprite.renderer.enabled)
			{
				for (int l = 0; l < oldGunSpriteAndOutlines.Length; l++)
				{
					oldGunSpriteAndOutlines[l].renderer.enabled = true;
				}
			}
		}
		m_cachedItemSpriteDefinition = null;
		m_deferCurrentItemSwap = false;
		yield return null;
		PostFlipReset(firstExtraGunCardTransform, gbTransform, placeholderCardObject, newGunSprite);
		m_isCurrentlyFlipping = false;
	}

	private IEnumerator HandleItemCardFlip(PlayerItem newItem)
	{
		m_deferCurrentItemSwap = true;
		m_isCurrentlyFlipping = true;
		float elapsed = 0f;
		float p2u = ItemBoxSprite.PixelsToUnits();
		Transform gbTransform = ItemBoxSprite.transform;
		GameObject placeholderCardObject = UnityEngine.Object.Instantiate(ExtraItemCardPrefab);
		dfControl placeholderCard = placeholderCardObject.GetComponent<dfControl>();
		Transform placeholderTransform = placeholderCardObject.transform;
		placeholderTransform.parent = m_panel.transform;
		m_panel.AddControl(placeholderCard);
		placeholderCard.RelativePosition = ItemBoxSprite.RelativePosition;
		m_currentItemSpriteZOffset = 5f;
		for (int i = 0; i < AdditionalItemBoxSprites.Count; i++)
		{
			(AdditionalItemBoxSprites[i] as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
			AdditionalItemBoxSprites[i].Invalidate();
		}
		Vector3 cachedPosition2 = Vector3.zero;
		Transform newChild = gbTransform.GetChild(0);
		newChild.parent = placeholderTransform;
		(placeholderCard as dfTextureSprite).Material = m_ClippedMaterial;
		placeholderCard.AddControl(newChild.GetComponent<dfControl>());
		cachedPosition2 = newChild.position;
		ItemBoxSprite.IsVisible = false;
		tk2dClippedSprite previousItemSprite = UnityEngine.Object.Instantiate(itemSprite.gameObject, itemSprite.transform.position, Quaternion.identity).GetComponent<tk2dClippedSprite>();
		tk2dSpriteCollectionData previousItemSpriteCollection = previousItemSprite.Collection;
		previousItemSprite.transform.parent = itemSprite.transform.parent;
		previousItemSprite.transform.position = previousItemSprite.transform.position.WithZ(-2f);
		Vector3 startPosition = placeholderTransform.position;
		tk2dBaseSprite[] gunSpriteAndOutlines = itemSprite.GetComponentsInChildren<tk2dBaseSprite>();
		UpdateItemSpriteScale();
		for (int j = 0; j < gunSpriteAndOutlines.Length; j++)
		{
		}
		bool hasDepthSwapped = false;
		float adjFlipTime = 0.15f * (float)((AdditionalItemBoxSprites.Count <= 20) ? 1 : (AdditionalItemBoxSprites.Count / 20));
		while (elapsed < adjFlipTime && GameUIRoot.Instance.GunventoryFolded && (bool)newChild && (bool)m_panel)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / adjFlipTime;
			if (t >= 0.5f && !hasDepthSwapped)
			{
				Debug.Log("doing depth swap");
				hasDepthSwapped = true;
				newChild.parent = m_panel.transform;
				m_panel.AddControl(newChild.GetComponent<dfControl>());
				(placeholderCard as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
				for (int k = 0; k < AdditionalItemBoxSprites.Count; k++)
				{
					(AdditionalItemBoxSprites[k] as dfTextureSprite).Material = m_ClippedMaterial;
				}
				m_currentItemSpriteZOffset = -2f;
				Transform transform = (placeholderTransform.parent = newChild.GetFirstLeafChild());
				transform.GetComponent<dfControl>().AddControl(placeholderCard);
			}
			float xOffset = BraveMathCollege.DoubleLerp(0f, (float)(AdditionalItemBoxSprites.Count * AdditionalBoxOffsetPX + AdditionalBoxOffsetPX * 2) * Pixelator.Instance.CurrentTileScale * -1f, (float)(AdditionalItemBoxSprites.Count * AdditionalBoxOffsetPX) * Pixelator.Instance.CurrentTileScale * -1f, t);
			float yOffset = BraveMathCollege.DoubleLerpSmooth(0f, 9f * Pixelator.Instance.CurrentTileScale, 0f, t);
			float zRotation = BraveMathCollege.DoubleLerp(0f, 20f, 0f, t);
			if ((bool)placeholderTransform)
			{
				placeholderTransform.position = startPosition + new Vector3(xOffset * p2u, yOffset * p2u, 0f);
				placeholderTransform.rotation = Quaternion.Euler(0f, 0f, zRotation);
			}
			tk2dSprite[] array = SpriteOutlineManager.GetOutlineSprites<tk2dSprite>(previousItemSprite);
			if (array != null)
			{
				for (int l = 0; l < array.Length; l++)
				{
					if ((bool)array[l])
					{
						array[l].SetSprite(previousItemSpriteCollection, previousItemSprite.spriteId);
						array[l].ForceUpdateMaterial();
						SpriteOutlineManager.ForceRebuildMaterial(array[l], previousItemSprite, Color.white);
					}
				}
			}
			if ((bool)placeholderCard)
			{
				Vector3 center = placeholderCard.GetCenter();
				previousItemSprite.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
				previousItemSprite.PlaceAtPositionByAnchor(center, tk2dBaseSprite.Anchor.MiddleCenter);
			}
			float extraCardXOffset = BraveMathCollege.SmoothLerp(0f, (float)AdditionalBoxOffsetPX * Pixelator.Instance.CurrentTileScale * p2u, t);
			m_currentItemSpriteXOffset = (float)(-AdditionalBoxOffsetPX) * Pixelator.Instance.CurrentTileScale * p2u + extraCardXOffset;
			if ((bool)newChild)
			{
				newChild.position = cachedPosition2 + new Vector3(extraCardXOffset, 0f, 0f);
				newChild.rotation = Quaternion.identity;
			}
			yield return null;
			m_deferCurrentItemSwap = false;
			if (!itemSprite.renderer.enabled)
			{
				for (int m = 0; m < gunSpriteAndOutlines.Length; m++)
				{
					gunSpriteAndOutlines[m].renderer.enabled = true;
				}
			}
		}
		PostFlipReset(newChild, gbTransform, placeholderCardObject, previousItemSprite);
		m_isCurrentlyFlipping = false;
	}

	private void PostFlipReset(Transform newChild, Transform gbTransform, GameObject placeholderCardObject, tk2dClippedSprite oldGunSprite)
	{
		for (int i = 0; i < AdditionalItemBoxSprites.Count; i++)
		{
			(AdditionalItemBoxSprites[i] as dfTextureSprite).Material = m_ClippedZWriteOffMaterial;
		}
		if ((bool)newChild)
		{
			if ((bool)gbTransform)
			{
				newChild.parent = gbTransform;
			}
			ItemBoxSprite.AddControl(newChild.GetComponent<dfControl>());
			newChild.GetComponent<dfControl>().RelativePosition = new Vector3((float)(-AdditionalBoxOffsetPX) * Pixelator.Instance.CurrentTileScale, 0f, 0f);
		}
		if ((bool)placeholderCardObject)
		{
			UnityEngine.Object.Destroy(placeholderCardObject);
		}
		if ((bool)oldGunSprite)
		{
			UnityEngine.Object.Destroy(oldGunSprite.gameObject);
		}
		m_currentItemSpriteXOffset = 0f;
		m_currentItemSpriteZOffset = 0f;
		ItemBoxSprite.IsVisible = true;
	}

	public void UpdateScale()
	{
		ItemBoxSprite.Size = ItemBoxSprite.SpriteInfo.sizeInPixels * Pixelator.Instance.CurrentTileScale;
		ItemBoxFillSprite.Size = new Vector2(3f, 26f) * Pixelator.Instance.CurrentTileScale;
		ItemBoxFGSprite.Size = ItemBoxFGSprite.SpriteInfo.sizeInPixels * Pixelator.Instance.CurrentTileScale;
		if (!m_isCurrentlyFlipping)
		{
			ItemBoxFGSprite.RelativePosition = ItemBoxSprite.RelativePosition;
			ItemBoxFillSprite.RelativePosition = ItemBoxSprite.RelativePosition + new Vector3(123f, 3f, 0f);
		}
	}

	protected void RebuildExtraItemCards(PlayerItem current, List<PlayerItem> items)
	{
		float num = m_panel.PixelsToUnits();
		for (int i = 0; i < AdditionalItemBoxSprites.Count; i++)
		{
			UnityEngine.Object.Destroy(AdditionalItemBoxSprites[i].gameObject);
		}
		AdditionalItemBoxSprites.Clear();
		dfControl dfControl2 = ItemBoxSprite;
		Transform parent = ItemBoxSprite.transform;
		for (int j = 0; j < items.Count - 1; j++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ExtraItemCardPrefab);
			gameObject.transform.parent = parent;
			dfControl component = gameObject.GetComponent<dfControl>();
			dfControl2.AddControl(component);
			component.RelativePosition = new Vector3((float)(-AdditionalBoxOffsetPX) * Pixelator.Instance.CurrentTileScale, 0f, 0f);
			dfControl2 = component;
			parent = gameObject.transform;
			AdditionalItemBoxSprites.Add(component);
		}
		float num2 = (float)(AdditionalBoxOffsetPX * AdditionalItemBoxSprites.Count) * Pixelator.Instance.CurrentTileScale * num;
		if (IsRightAligned)
		{
			ItemBoxSprite.transform.position = ItemBoxSprite.transform.position.WithX(m_panel.transform.position.x + (0f - ItemBoxSprite.Width * num) + num2);
		}
		else
		{
			ItemBoxSprite.transform.position = m_panel.transform.position + new Vector3(num2, 0f, 0f);
		}
		ItemBoxSprite.Invalidate();
	}

	public void DimItemSprite()
	{
		if (!(m_cachedItem == null))
		{
			itemSprite.gameObject.SetActive(false);
		}
	}

	public void UndimItemSprite()
	{
		if (!(m_cachedItem == null))
		{
			itemSprite.gameObject.SetActive(true);
		}
	}

	private void UpdateItemSpriteScale()
	{
		itemSprite.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			if (outlineSprites.Length > 1)
			{
				float num = ((i != 1) ? 0f : 0.0625f);
				num = ((i != 3) ? num : (-0.0625f));
				float num2 = ((i != 0) ? 0f : 0.0625f);
				num2 = ((i != 2) ? num2 : (-0.0625f));
				outlineSprites[i].transform.localPosition = (new Vector3(num, num2, 0f) * Pixelator.Instance.CurrentTileScale * 16f * GameUIRoot.Instance.PixelsToUnits()).WithZ(UI_OUTLINE_DEPTH);
			}
			outlineSprites[i].scale = itemSprite.scale;
		}
	}

	public Vector3 GetOffsetVectorForItem(PlayerItem newItem, bool isFlippingGun)
	{
		tk2dSpriteDefinition tk2dSpriteDefinition2 = null;
		tk2dSpriteDefinition2 = ((m_cachedItemSpriteDefinition == null) ? newItem.sprite.Collection.spriteDefinitions[newItem.sprite.spriteId] : m_cachedItemSpriteDefinition);
		Vector3 result = Vector3.Scale((-tk2dSpriteDefinition2.GetBounds().min + -tk2dSpriteDefinition2.GetBounds().extents).Quantize(0.0625f), itemSprite.scale);
		if (isFlippingGun)
		{
			result += new Vector3(m_currentItemSpriteXOffset, 0f, m_currentItemSpriteZOffset);
		}
		return result;
	}

	private void UpdateItemSprite(PlayerItem newItem, int itemShift)
	{
		tk2dSprite component = newItem.GetComponent<tk2dSprite>();
		if (newItem != m_cachedItem)
		{
			DoItemCardFlip(newItem, itemShift);
		}
		UpdateItemSpriteScale();
		if (!m_deferCurrentItemSwap)
		{
			if (!itemSprite.renderer.enabled)
			{
				ToggleRenderers(true);
			}
			if (itemSprite.spriteId != component.spriteId || itemSprite.Collection != component.Collection)
			{
				itemSprite.SetSprite(component.Collection, component.spriteId);
				for (int i = 0; i < outlineSprites.Length; i++)
				{
					outlineSprites[i].SetSprite(component.Collection, component.spriteId);
					SpriteOutlineManager.ForceUpdateOutlineMaterial(outlineSprites[i], component);
				}
			}
		}
		Vector3 center = ItemBoxSprite.GetCenter();
		itemSprite.transform.position = center + GetOffsetVectorForItem(newItem, m_isCurrentlyFlipping);
		itemSprite.transform.position = itemSprite.transform.position.Quantize(ItemBoxSprite.PixelsToUnits() * 3f);
		if (newItem.PreventCooldownBar || (!newItem.IsActive && !newItem.IsOnCooldown) || m_isCurrentlyFlipping)
		{
			ItemBoxFillSprite.IsVisible = false;
			ItemBoxFGSprite.IsVisible = false;
			ItemBoxSprite.SpriteName = "weapon_box_02";
		}
		else
		{
			ItemBoxFillSprite.IsVisible = true;
			ItemBoxFGSprite.IsVisible = true;
			ItemBoxSprite.SpriteName = "weapon_box_02_cd";
		}
		if (newItem.IsActive)
		{
			ItemBoxFillSprite.FillAmount = 1f - newItem.ActivePercentage;
		}
		else
		{
			ItemBoxFillSprite.FillAmount = 1f - newItem.CooldownPercentage;
		}
		PlayerController user = GameManager.Instance.PrimaryPlayer;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && IsRightAligned)
		{
			user = GameManager.Instance.SecondaryPlayer;
		}
		if (newItem.IsOnCooldown || !newItem.CanBeUsed(user))
		{
			Color color = itemSpriteMaterial.GetColor("_OverrideColor");
			Color color2 = new Color(0f, 0f, 0f, 0.8f);
			if (color != color2)
			{
				itemSpriteMaterial.SetColor("_OverrideColor", color2);
				tk2dSprite[] array = SpriteOutlineManager.GetOutlineSprites(itemSprite);
				Color value = new Color(0.4f, 0.4f, 0.4f, 1f);
				for (int j = 0; j < array.Length; j++)
				{
					array[j].renderer.material.SetColor("_OverrideColor", value);
				}
			}
			return;
		}
		Color color3 = itemSpriteMaterial.GetColor("_OverrideColor");
		Color color4 = new Color(0f, 0f, 0f, 0f);
		if (color3 != color4)
		{
			itemSpriteMaterial.SetColor("_OverrideColor", color4);
			tk2dSprite[] array2 = SpriteOutlineManager.GetOutlineSprites(itemSprite);
			Color white = Color.white;
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k].renderer.material.SetColor("_OverrideColor", white);
			}
		}
	}

	public void TriggerUIDisabled()
	{
		if (GameUIRoot.Instance.ForceHideItemPanel)
		{
			itemSprite.renderer.enabled = false;
			for (int i = 0; i < outlineSprites.Length; i++)
			{
				outlineSprites[i].renderer.enabled = false;
			}
			ItemBoxSprite.IsVisible = false;
			ItemBoxFillSprite.IsVisible = false;
			ItemBoxFGSprite.IsVisible = false;
		}
	}

	private void SetItemCountVisible(bool val)
	{
		ItemCountLabel.IsVisible = val;
	}

	public void UpdateItem(PlayerItem current, List<PlayerItem> items)
	{
		if (!m_initialized)
		{
			Initialize();
		}
		if (GameUIRoot.Instance.ForceHideItemPanel || temporarilyPreventVisible)
		{
			ToggleRenderers(false);
			return;
		}
		if (items.Count != 0 && items.Count - 1 != AdditionalItemBoxSprites.Count && GameUIRoot.Instance.GunventoryFolded)
		{
			RebuildExtraItemCards(current, items);
		}
		if (current == null || GameUIRoot.Instance.ForceHideItemPanel || temporarilyPreventVisible)
		{
			if (ItemBoxSprite.IsVisible)
			{
				itemSprite.renderer.enabled = false;
				for (int i = 0; i < outlineSprites.Length; i++)
				{
					outlineSprites[i].renderer.enabled = false;
				}
				ItemBoxSprite.IsVisible = false;
				ItemBoxFillSprite.IsVisible = false;
				ItemBoxFGSprite.IsVisible = false;
			}
			SetItemCountVisible(false);
		}
		else
		{
			if ((!ItemBoxSprite.IsVisible || !itemSprite.renderer.enabled) && !m_isCurrentlyFlipping && !m_deferCurrentItemSwap)
			{
				itemSprite.renderer.enabled = true;
				for (int j = 0; j < outlineSprites.Length; j++)
				{
					outlineSprites[j].renderer.enabled = true;
				}
				ItemBoxSprite.IsVisible = true;
			}
			if ((current.canStack && current.numberOfUses > 1 && current.consumable) || (current.numberOfUses > 1 && current.UsesNumberOfUsesBeforeCooldown && !current.IsOnCooldown))
			{
				SetItemCountVisible(true);
				ItemCountLabel.Text = current.numberOfUses.ToString();
			}
			else if (current is EstusFlaskItem)
			{
				EstusFlaskItem estusFlaskItem = current as EstusFlaskItem;
				SetItemCountVisible(true);
				ItemCountLabel.Text = estusFlaskItem.RemainingDrinks.ToString();
			}
			else if (current is RatPackItem && !current.IsOnCooldown)
			{
				RatPackItem ratPackItem = current as RatPackItem;
				SetItemCountVisible(true);
				ItemCountLabel.Text = ratPackItem.ContainedBullets.ToString();
			}
			else
			{
				SetItemCountVisible(false);
			}
			int itemShift = 0;
			if (current != m_cachedItem && items.Contains(m_cachedItem))
			{
				int num = items.IndexOf(m_cachedItem);
				int num2 = items.IndexOf(current);
				itemShift = ((items.Count == 2) ? (-1) : ((num2 == 0 && num == items.Count - 1) ? 1 : ((num2 != items.Count - 1 || num != 0) ? (num2 - num) : (-1))));
			}
			else if (current != m_cachedItem)
			{
				itemShift = -1;
			}
			UpdateItemSprite(current, itemShift);
		}
		if (itemSprite.renderer.enabled && !ItemBoxSprite.IsVisible)
		{
			ToggleRenderers(true);
		}
		m_cachedItem = current;
	}
}
