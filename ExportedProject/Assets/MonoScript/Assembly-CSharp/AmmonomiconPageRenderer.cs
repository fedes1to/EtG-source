using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmmonomiconPageRenderer : MonoBehaviour
{
	public enum PageType
	{
		NONE,
		EQUIPMENT_LEFT,
		EQUIPMENT_RIGHT,
		GUNS_LEFT,
		GUNS_RIGHT,
		ITEMS_LEFT,
		ITEMS_RIGHT,
		ENEMIES_LEFT,
		ENEMIES_RIGHT,
		BOSSES_LEFT,
		BOSSES_RIGHT,
		DEATH_LEFT,
		DEATH_RIGHT
	}

	private struct RectangleLineInfo
	{
		public int numberOfElements;

		public float lineHeightUnits;

		public float initialXOffset;
	}

	public PageType pageType;

	public MeshRenderer targetRenderer;

	public dfGUIManager guiManager;

	public int offsetIndex = -1;

	public dfSprite HeaderBGSprite;

	private IAmmonomiconFocusable m_lastFocusTarget;

	[NonSerialized]
	public dfControl PrimaryClipPanel;

	private Camera m_camera;

	private Material renderMaterial;

	private int topBezierPropID;

	private int leftBezierPropID;

	private int rightBezierPropID;

	private int bottomBezierPropID;

	private List<AmmonomiconPokedexEntry> m_pokedexEntries = new List<AmmonomiconPokedexEntry>();

	private RenderTexture m_renderBuffer;

	private dfFontBase EnglishFont;

	private dfFontBase OtherLanguageFont;

	private dfFontBase BaseAlagardFont;

	private dfFontBase OtherAlagardFont;

	private float? OriginalHeaderRelativeY;

	private bool m_hasAdjustedForChinese;

	private StringTableManager.GungeonSupportedLanguages m_cachedLanguage;

	private List<dfButton> m_prevLineButtons;

	public IAmmonomiconFocusable LastFocusTarget
	{
		get
		{
			return m_lastFocusTarget;
		}
		set
		{
			m_lastFocusTarget = value;
		}
	}

	public void Awake()
	{
		if (pageType == PageType.EQUIPMENT_LEFT)
		{
			dfScrollPanel component = base.transform.parent.Find("Scroll Panel").GetComponent<dfScrollPanel>();
			Transform transform = component.transform.Find("Header");
			dfLabel[] componentsInChildren = transform.GetComponentsInChildren<dfLabel>();
			BaseAlagardFont = componentsInChildren[0].Font;
			OtherAlagardFont = (BraveResources.Load("Alternate Fonts/AlagardExtended22") as GameObject).GetComponent<dfFont>();
		}
		else if (pageType == PageType.EQUIPMENT_RIGHT)
		{
			dfScrollPanel component2 = base.transform.parent.Find("Scroll Panel").GetComponent<dfScrollPanel>();
			dfLabel component3 = component2.transform.Find("Scroll Panel").Find("Panel").Find("Label")
				.GetComponent<dfLabel>();
			if ((bool)component3)
			{
				EnglishFont = component3.Font;
				OtherLanguageFont = GameUIRoot.Instance.Manager.DefaultFont;
			}
		}
	}

	public List<AmmonomiconPokedexEntry> GetPokedexEntries()
	{
		return m_pokedexEntries;
	}

	public AmmonomiconPokedexEntry GetPokedexEntry(EncounterTrackable targetTrackable)
	{
		for (int i = 0; i < m_pokedexEntries.Count; i++)
		{
			if (m_pokedexEntries[i].linkedEncounterTrackable.myGuid == targetTrackable.EncounterGuid)
			{
				return m_pokedexEntries[i];
			}
		}
		return null;
	}

	protected void ToggleHeaderImage()
	{
		if (pageType == PageType.EQUIPMENT_LEFT || pageType == PageType.GUNS_LEFT || pageType == PageType.ITEMS_LEFT || pageType == PageType.ENEMIES_LEFT || pageType == PageType.BOSSES_LEFT)
		{
			if (GameManager.Options.CurrentLanguage != 0 && HeaderBGSprite != null)
			{
				HeaderBGSprite.IsVisible = false;
			}
			else if (HeaderBGSprite != null)
			{
				HeaderBGSprite.IsVisible = true;
			}
		}
	}

	public void ForceUpdateLanguageFonts()
	{
		AmmonomiconPageRenderer ammonomiconPageRenderer = ((!(AmmonomiconController.Instance.ImpendingRightPageRenderer != null)) ? AmmonomiconController.Instance.CurrentRightPageRenderer : AmmonomiconController.Instance.ImpendingRightPageRenderer);
		if (ammonomiconPageRenderer.pageType != PageType.DEATH_RIGHT)
		{
			dfScrollPanel component = ammonomiconPageRenderer.guiManager.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
			dfLabel component2 = component.transform.Find("Scroll Panel").Find("Panel").Find("Label")
				.GetComponent<dfLabel>();
			CheckLanguageFonts(component2);
			component2.Localize();
		}
		ForceUpdateHeaderFonts();
	}

	private void ForceUpdateHeaderFonts()
	{
		AmmonomiconPageRenderer ammonomiconPageRenderer = ((!(AmmonomiconController.Instance.ImpendingLeftPageRenderer != null)) ? AmmonomiconController.Instance.CurrentLeftPageRenderer : AmmonomiconController.Instance.ImpendingLeftPageRenderer);
		if (this != ammonomiconPageRenderer)
		{
			return;
		}
		if (pageType == PageType.EQUIPMENT_LEFT)
		{
			ToggleHeaderImage();
		}
		dfScrollPanel component = ammonomiconPageRenderer.guiManager.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
		Transform transform = component.transform.Find("Header");
		dfLabel[] componentsInChildren = transform.GetComponentsInChildren<dfLabel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!OriginalHeaderRelativeY.HasValue && componentsInChildren[i].transform.parent == transform)
			{
				OriginalHeaderRelativeY = componentsInChildren[i].RelativePosition.y;
			}
			CheckHeaderFonts(componentsInChildren[i], componentsInChildren[i].transform.parent == transform);
		}
	}

	public void UpdateOnBecameActive()
	{
		ForceUpdateLanguageFonts();
		if (AmmonomiconController.Instance.ImpendingLeftPageRenderer == null || AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget == null)
		{
			switch (pageType)
			{
			case PageType.GUNS_RIGHT:
				SetFirstVisibleTexts();
				break;
			case PageType.ITEMS_RIGHT:
				SetFirstVisibleTexts();
				break;
			case PageType.ENEMIES_RIGHT:
				SetFirstVisibleTexts();
				break;
			case PageType.BOSSES_RIGHT:
				SetFirstVisibleTexts();
				break;
			case PageType.ITEMS_LEFT:
			case PageType.ENEMIES_LEFT:
			case PageType.BOSSES_LEFT:
				break;
			}
		}
	}

	private void SetFirstVisibleTexts()
	{
		if (AmmonomiconController.Instance.ImpendingLeftPageRenderer != null)
		{
			for (int i = 0; i < AmmonomiconController.Instance.ImpendingLeftPageRenderer.m_pokedexEntries.Count; i++)
			{
				AmmonomiconPokedexEntry ammonomiconPokedexEntry = AmmonomiconController.Instance.ImpendingLeftPageRenderer.m_pokedexEntries[i];
				if (ammonomiconPokedexEntry.encounterState == AmmonomiconPokedexEntry.EncounterState.ENCOUNTERED)
				{
					SetRightDataPageTexts(ammonomiconPokedexEntry.ChildSprite, ammonomiconPokedexEntry.linkedEncounterTrackable);
					if (AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget == null)
					{
						AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget = ammonomiconPokedexEntry.GetComponent<dfControl>();
					}
					return;
				}
				if (ammonomiconPokedexEntry.encounterState == AmmonomiconPokedexEntry.EncounterState.KNOWN)
				{
					SetPageDataUnknown(this);
					SetRightDataPageName(ammonomiconPokedexEntry.ChildSprite, ammonomiconPokedexEntry.linkedEncounterTrackable);
					if (AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget == null)
					{
						AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget = ammonomiconPokedexEntry.GetComponent<dfControl>();
					}
					return;
				}
			}
		}
		SetPageDataUnknown(this);
	}

	public void Initialize(MeshRenderer ts)
	{
		targetRenderer = ts;
		m_camera = GetComponent<Camera>();
		m_camera.aspect = 8f / 9f;
		guiManager = base.transform.parent.GetComponent<dfGUIManager>();
		guiManager.UIScale = 1f;
		Transform transform = guiManager.transform.Find("Scroll Panel");
		if (transform != null)
		{
			transform.GetComponent<dfScrollPanel>().LockScrollPanelToZero = true;
		}
		RebuildRenderData();
		topBezierPropID = Shader.PropertyToID("_TopBezier");
		leftBezierPropID = Shader.PropertyToID("_LeftBezier");
		rightBezierPropID = Shader.PropertyToID("_RightBezier");
		bottomBezierPropID = Shader.PropertyToID("_BottomBezier");
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetRow(0, new Vector4(0f, 0f, 0f, 0f));
		matrix.SetRow(1, new Vector4(0f, 0f, 0f, 0f));
		matrix.SetRow(2, new Vector4(1f, 1f, 1f, 1f));
		matrix.SetRow(3, new Vector4(1f, 1f, 1f, 1f));
		SetMatrix(matrix);
		StartCoroutine(DelayedBuildPage());
	}

	private void RebuildRenderData()
	{
		if (m_renderBuffer != null)
		{
			RenderTexture.ReleaseTemporary(m_renderBuffer);
			m_renderBuffer = null;
		}
		Debug.LogWarning("Reacquiring Page Buffer 960x1080");
		m_renderBuffer = RenderTexture.GetTemporary(960, 1080, 0, RenderTextureFormat.Default);
		m_renderBuffer.name = "temporary ammonomicon render buffer";
		m_renderBuffer.filterMode = FilterMode.Point;
		m_renderBuffer.DiscardContents();
		m_camera.targetTexture = m_renderBuffer;
		renderMaterial = new Material(ShaderCache.Acquire("Custom/AmmonomiconPageShader"));
		renderMaterial.SetTexture("_MainTex", m_renderBuffer);
		targetRenderer.material = renderMaterial;
	}

	private IEnumerator DelayedBuildPage()
	{
		if (pageType == PageType.EQUIPMENT_LEFT)
		{
			while (GameManager.Instance.IsSelectingCharacter)
			{
				yield return null;
			}
		}
		switch (pageType)
		{
		case PageType.EQUIPMENT_LEFT:
			InitializeEquipmentPageLeft();
			break;
		case PageType.EQUIPMENT_RIGHT:
			InitializeEquipmentPageRight();
			break;
		case PageType.GUNS_LEFT:
			InitializeGunsPageLeft();
			break;
		case PageType.ITEMS_LEFT:
			InitializeItemsPageLeft();
			break;
		case PageType.ENEMIES_LEFT:
			InitializeEnemiesPageLeft();
			break;
		case PageType.BOSSES_LEFT:
			InitializeBossesPageLeft();
			break;
		case PageType.GUNS_RIGHT:
			SetPageDataUnknown(this);
			break;
		case PageType.ITEMS_RIGHT:
			SetPageDataUnknown(this);
			break;
		case PageType.ENEMIES_RIGHT:
			SetPageDataUnknown(this);
			break;
		case PageType.BOSSES_RIGHT:
			SetPageDataUnknown(this);
			break;
		case PageType.DEATH_LEFT:
			InitializeDeathPageLeft();
			break;
		case PageType.DEATH_RIGHT:
			InitializeDeathPageRight();
			break;
		}
	}

	private void InitializeDeathPageLeft()
	{
		AmmonomiconDeathPageController component = guiManager.GetComponent<AmmonomiconDeathPageController>();
		component.DoInitialize();
	}

	private void InitializeDeathPageRight()
	{
		AmmonomiconDeathPageController component = guiManager.GetComponent<AmmonomiconDeathPageController>();
		component.DoInitialize();
		dfScrollPanel component2 = component.transform.Find("Scroll Panel").Find("Footer").Find("ScrollItemsPanel")
			.GetComponent<dfScrollPanel>();
		dfPanel component3 = component2.transform.Find("AllItemsPanel").GetComponent<dfPanel>();
		for (int i = 0; i < component3.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(component3.transform.GetChild(i).gameObject);
		}
		List<tk2dBaseSprite> list = new List<tk2dBaseSprite>();
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[j];
			for (int k = 0; k < playerController.inventory.AllGuns.Count; k++)
			{
				Gun gun = playerController.inventory.AllGuns[k];
				tk2dClippedSprite tk2dClippedSprite2 = AddSpriteToPage<tk2dClippedSprite>(gun.GetSprite().Collection, gun.DefaultSpriteID);
				SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite2, Color.black, 0.1f, 0.01f);
				tk2dClippedSprite2.transform.parent = component3.transform;
				tk2dClippedSprite2.transform.position = component3.GetCenter();
				list.Add(tk2dClippedSprite2);
			}
			for (int l = 0; l < playerController.activeItems.Count; l++)
			{
				tk2dClippedSprite tk2dClippedSprite3 = AddSpriteToPage<tk2dClippedSprite>(playerController.activeItems[l].sprite.Collection, playerController.activeItems[l].sprite.spriteId);
				SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite3, Color.black, 0.1f, 0.01f);
				tk2dClippedSprite3.transform.parent = component3.transform;
				tk2dClippedSprite3.transform.position = component3.GetCenter();
				list.Add(tk2dClippedSprite3);
			}
			for (int m = 0; m < playerController.passiveItems.Count; m++)
			{
				tk2dClippedSprite tk2dClippedSprite4 = AddSpriteToPage<tk2dClippedSprite>(playerController.passiveItems[m].sprite.Collection, playerController.passiveItems[m].sprite.spriteId);
				SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite4, Color.black, 0.1f, 0.01f);
				tk2dClippedSprite4.transform.parent = component3.transform;
				tk2dClippedSprite4.transform.position = component3.GetCenter();
				list.Add(tk2dClippedSprite4);
			}
		}
		list = list.ttOrderBy((tk2dBaseSprite a) => a.GetBounds().size.y);
		List<tk2dBaseSprite> previousLineSprites = new List<tk2dBaseSprite>();
		BoxArrangeItems(component3, list, new Vector2(0f, 6f), new Vector2(6f, 3f), ref previousLineSprites);
		StartCoroutine(HandleDeathItemsClipping(component3, list));
	}

	private IEnumerator HandleDeathItemsClipping(dfPanel parentPanel, List<tk2dBaseSprite> itemSprites)
	{
		while (!GameManager.Instance.IsLoadingLevel)
		{
			for (int i = 0; i < itemSprites.Count; i++)
			{
				tk2dClippedSprite tk2dClippedSprite2 = itemSprites[i] as tk2dClippedSprite;
				Vector3[] corners = parentPanel.Parent.GetCorners();
				float x = corners[0].x;
				float y = corners[0].y;
				float x2 = corners[3].x;
				float y2 = corners[3].y;
				Bounds untrimmedBounds = tk2dClippedSprite2.GetUntrimmedBounds();
				untrimmedBounds.center += tk2dClippedSprite2.transform.position;
				float x3 = Mathf.Clamp01((x - untrimmedBounds.min.x) / untrimmedBounds.size.x);
				float y3 = Mathf.Clamp01((y2 - untrimmedBounds.min.y) / untrimmedBounds.size.y);
				float x4 = Mathf.Clamp01((x2 - untrimmedBounds.min.x) / untrimmedBounds.size.x);
				float y4 = Mathf.Clamp01((y - untrimmedBounds.min.y) / untrimmedBounds.size.y);
				tk2dClippedSprite2.clipBottomLeft = new Vector2(x3, y3);
				tk2dClippedSprite2.clipTopRight = new Vector2(x4, y4);
				if (SpriteOutlineManager.HasOutline(tk2dClippedSprite2))
				{
					tk2dClippedSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(tk2dClippedSprite2);
					for (int j = 0; j < outlineSprites.Length; j++)
					{
						outlineSprites[j].clipBottomLeft = tk2dClippedSprite2.clipBottomLeft;
						outlineSprites[j].clipTopRight = tk2dClippedSprite2.clipTopRight;
					}
				}
			}
			yield return null;
		}
	}

	public void ReturnFocusToBookmarks()
	{
		LastFocusTarget = dfGUIManager.ActiveControl;
		for (int i = 0; i < AmmonomiconController.Instance.Ammonomicon.bookmarks.Length; i++)
		{
			if (AmmonomiconController.Instance.Ammonomicon.bookmarks[i].IsCurrentPage)
			{
				AmmonomiconController.Instance.Ammonomicon.bookmarks[i].ForceFocus();
				break;
			}
		}
	}

	public void LateUpdate()
	{
		if (m_camera.enabled && (!m_renderBuffer || m_renderBuffer == null || !m_renderBuffer.IsCreated()))
		{
			RebuildRenderData();
		}
		m_camera.transform.localPosition = new Vector3(0f, 0.001f, m_camera.transform.localPosition.z);
	}

	public void DoRefreshData()
	{
		if (pageType == PageType.EQUIPMENT_LEFT)
		{
			for (int i = 0; i < m_pokedexEntries.Count; i++)
			{
				UnityEngine.Object.Destroy(m_pokedexEntries[i].gameObject);
			}
			LastFocusTarget = null;
			m_pokedexEntries.Clear();
			InitializeEquipmentPageLeft();
			if (m_pokedexEntries.Count > 0)
			{
				LastFocusTarget = m_pokedexEntries[0].GetComponent<dfButton>();
			}
			guiManager.UIScaleLegacyMode = true;
			guiManager.UIScaleLegacyMode = false;
		}
		else if (pageType == PageType.DEATH_LEFT)
		{
			InitializeDeathPageLeft();
		}
		else if (pageType == PageType.DEATH_RIGHT)
		{
			InitializeDeathPageRight();
		}
		else
		{
			for (int j = 0; j < m_pokedexEntries.Count; j++)
			{
				m_pokedexEntries[j].UpdateEncounterState();
			}
		}
	}

	public void BoxArrangeItems(dfPanel sourcePanel, List<tk2dBaseSprite> sourceElements, Vector2 panelPaddingPx, Vector2 elementPaddingPx, ref List<tk2dBaseSprite> previousLineSprites)
	{
		if (previousLineSprites == null)
		{
			previousLineSprites = new List<tk2dBaseSprite>();
		}
		List<tk2dBaseSprite> list = new List<tk2dBaseSprite>(sourceElements);
		float num = guiManager.PixelsToUnits();
		float num2 = (sourcePanel.Width - panelPaddingPx.x * 2f) * num;
		float num3 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			num3 = Mathf.Max(num3, list[i].GetBounds().size.y + 2f * elementPaddingPx.y * num);
		}
		float num4 = num2;
		float num5 = 0f;
		float num6 = -1f * panelPaddingPx.y * num;
		int num7 = 1;
		List<tk2dBaseSprite> list2 = new List<tk2dBaseSprite>();
		float num8 = panelPaddingPx.y * num;
		float num9 = 0f;
		while (list.Count > 0)
		{
			tk2dBaseSprite tk2dBaseSprite2 = list[0];
			list.RemoveAt(0);
			Bounds bounds = tk2dBaseSprite2.GetBounds();
			Bounds untrimmedBounds = tk2dBaseSprite2.GetUntrimmedBounds();
			Vector3 size = bounds.size;
			Vector3 size2 = untrimmedBounds.size;
			bool flag = size.x > num4;
			size.x = Mathf.Min(size.x, num2);
			size2.x = Mathf.Min(size2.x, num2);
			if (!flag)
			{
				num4 -= size.x + 2f * elementPaddingPx.x * num;
				float y = num6 - num3 + (num3 - size.y) / 2f;
				Vector3 position = new Vector3(num5 + panelPaddingPx.x * num + elementPaddingPx.x * num, y, 0f);
				tk2dBaseSprite2.transform.parent = sourcePanel.transform;
				tk2dBaseSprite2.PlaceAtLocalPositionByAnchor(position, tk2dBaseSprite.Anchor.LowerLeft);
				num5 += size.x + 2f * elementPaddingPx.x * num;
				list2.Add(tk2dBaseSprite2);
			}
			if (!flag && list.Count != 0)
			{
				continue;
			}
			float num10 = num4;
			for (int j = 0; j < list2.Count; j++)
			{
				list2[j].transform.localPosition += new Vector3(num10 / 2f, 0f, 0f);
			}
			num8 += num3;
			if (previousLineSprites.Count > 0)
			{
				float num11 = 0f;
				for (int k = 0; k < list2.Count; k++)
				{
					num11 = Mathf.Max(num11, list2[k].GetBounds().size.y + 2f * elementPaddingPx.y * num);
				}
				float num12 = num3 - num11;
				if (list.Count == 0)
				{
					num12 = 0.5f * num9 + elementPaddingPx.y * num;
				}
				if (num12 > 0f)
				{
					for (int l = 0; l < list2.Count; l++)
					{
						list2[l].transform.localPosition = list2[l].transform.localPosition + new Vector3(0f, num12, 0f);
					}
					num6 += num12;
				}
				num8 -= num12;
				num9 = num12;
			}
			if (flag || list.Count != 0)
			{
				num6 -= num3;
				num5 = 0f;
				num4 = num2;
				num7++;
				list.Insert(0, tk2dBaseSprite2);
				previousLineSprites = list2;
				list2 = new List<tk2dBaseSprite>();
			}
		}
		previousLineSprites = list2;
		sourcePanel.Height = num8 / num + panelPaddingPx.y;
	}

	private void SetPageDataUnknown(AmmonomiconPageRenderer rightPage)
	{
		if (rightPage == null)
		{
			return;
		}
		dfScrollPanel component = rightPage.guiManager.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
		Transform transform = component.transform.Find("Header");
		if ((bool)transform)
		{
			dfLabel component2 = transform.Find("Label").GetComponent<dfLabel>();
			component2.Text = component2.ForceGetLocalizedValue("#AMMONOMICON_UNKNOWN");
			component2.PerformLayout();
			dfSprite component3 = transform.Find("Sprite").GetComponent<dfSprite>();
			if ((bool)component3)
			{
				component3.FillDirection = dfFillDirection.Vertical;
				component3.FillAmount = ((GameManager.Options.CurrentLanguage != 0) ? 0.8f : 1f);
				component3.InvertFill = true;
			}
		}
		dfLabel component4 = component.transform.Find("Tape Line One").Find("Label").GetComponent<dfLabel>();
		component4.Text = component4.ForceGetLocalizedValue("#AMMONOMICON_QUESTIONS");
		component4.PerformLayout();
		dfSlicedSprite componentInChildren = component.transform.Find("Tape Line One").GetComponentInChildren<dfSlicedSprite>();
		componentInChildren.Width = component4.GetAutosizeWidth() / 4f + 12f;
		dfLabel component5 = component.transform.Find("Tape Line Two").Find("Label").GetComponent<dfLabel>();
		component5.Text = component4.ForceGetLocalizedValue("#AMMONOMICON_QUESTIONS");
		component5.PerformLayout();
		dfSlicedSprite componentInChildren2 = component.transform.Find("Tape Line Two").GetComponentInChildren<dfSlicedSprite>();
		componentInChildren2.Width = component5.GetAutosizeWidth() / 4f + 12f;
		dfPanel component6 = component.transform.Find("ThePhoto").Find("Photo").Find("tk2dSpriteHolder")
			.GetComponent<dfPanel>();
		dfSprite component7 = component.transform.Find("ThePhoto").Find("Photo").Find("ItemShadow")
			.GetComponent<dfSprite>();
		component7.IsVisible = false;
		tk2dSprite componentInChildren3 = component6.GetComponentInChildren<tk2dSprite>();
		dfTextureSprite componentInChildren4 = component.transform.Find("ThePhoto").GetComponentInChildren<dfTextureSprite>();
		if (componentInChildren4 != null)
		{
			componentInChildren4.IsVisible = false;
		}
		if (!(componentInChildren3 == null))
		{
			if (SpriteOutlineManager.HasOutline(componentInChildren3))
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(componentInChildren3, true);
			}
			componentInChildren3.renderer.enabled = false;
		}
		dfLabel component8 = component.transform.Find("Scroll Panel").Find("Panel").Find("Label")
			.GetComponent<dfLabel>();
		CheckLanguageFonts(component8);
		component8.Text = component8.ForceGetLocalizedValue("#AMMONOMICON_MYSTERIOUS");
		component8.transform.parent.GetComponent<dfPanel>().Height = component8.Height;
		component.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>().ScrollPosition = Vector2.zero;
	}

	public void SetRightDataPageUnknown(bool impending = false)
	{
		AmmonomiconPageRenderer pageDataUnknown = ((!impending) ? AmmonomiconController.Instance.CurrentRightPageRenderer : AmmonomiconController.Instance.ImpendingRightPageRenderer);
		SetPageDataUnknown(pageDataUnknown);
	}

	private void CheckHeaderFonts(dfLabel headerLabel, bool isPrimaryLabel)
	{
		if (BaseAlagardFont == null)
		{
			BaseAlagardFont = headerLabel.Font;
			OtherAlagardFont = (BraveResources.Load("Alternate Fonts/AlagardExtended22") as GameObject).GetComponent<dfFont>();
		}
		if (isPrimaryLabel)
		{
			headerLabel.BringToFront();
		}
		if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			if (headerLabel.Font != BaseAlagardFont || headerLabel.TextScale != 2f)
			{
				headerLabel.Font = BaseAlagardFont;
				headerLabel.TextScale = 2f;
				headerLabel.PerformLayout();
			}
			if (isPrimaryLabel && headerLabel.RelativePosition.y != OriginalHeaderRelativeY.Value)
			{
				headerLabel.RelativePosition = headerLabel.RelativePosition.WithY(OriginalHeaderRelativeY.Value);
				headerLabel.PerformLayout();
			}
		}
		else if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN || StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
		{
			if (isPrimaryLabel && headerLabel.RelativePosition.y != OriginalHeaderRelativeY.Value)
			{
				headerLabel.RelativePosition = headerLabel.RelativePosition.WithY(OriginalHeaderRelativeY.Value);
				headerLabel.PerformLayout();
			}
		}
		else if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN)
		{
			if (isPrimaryLabel && headerLabel.RelativePosition.y != OriginalHeaderRelativeY.Value - 16f)
			{
				headerLabel.RelativePosition = headerLabel.RelativePosition.WithY(OriginalHeaderRelativeY.Value - 16f);
				headerLabel.PerformLayout();
			}
		}
		else
		{
			if (headerLabel != OtherAlagardFont || headerLabel.TextScale != 4f)
			{
				headerLabel.Font = OtherAlagardFont;
				headerLabel.TextScale = 4f;
				headerLabel.PerformLayout();
			}
			if (isPrimaryLabel && headerLabel.RelativePosition.y != OriginalHeaderRelativeY.Value - 24f)
			{
				headerLabel.RelativePosition = headerLabel.RelativePosition.WithY(OriginalHeaderRelativeY.Value - 24f);
				headerLabel.PerformLayout();
			}
		}
	}

	private void AdjustForChinese()
	{
		if (!(AmmonomiconController.Instance != null) || ((!m_hasAdjustedForChinese || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE) && (m_hasAdjustedForChinese || GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.CHINESE)))
		{
			return;
		}
		AmmonomiconPageRenderer ammonomiconPageRenderer = ((!(AmmonomiconController.Instance.ImpendingRightPageRenderer != null)) ? AmmonomiconController.Instance.CurrentRightPageRenderer : AmmonomiconController.Instance.ImpendingRightPageRenderer);
		if (ammonomiconPageRenderer != null)
		{
			dfScrollPanel component = ammonomiconPageRenderer.guiManager.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
			dfLabel component2 = component.transform.Find("Tape Line One").Find("Label").GetComponent<dfLabel>();
			dfLabel component3 = component.transform.Find("Tape Line Two").Find("Label").GetComponent<dfLabel>();
			if (m_hasAdjustedForChinese && GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.CHINESE)
			{
				component2.RelativePosition += new Vector3(0f, -8f, 0f);
				component3.RelativePosition += new Vector3(0f, -8f, 0f);
				m_hasAdjustedForChinese = false;
			}
			else if (!m_hasAdjustedForChinese && GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
			{
				component2.RelativePosition += new Vector3(0f, 8f, 0f);
				component3.RelativePosition += new Vector3(0f, 8f, 0f);
				m_hasAdjustedForChinese = true;
			}
		}
	}

	private void CheckLanguageFonts(dfLabel mainText)
	{
		if (EnglishFont == null)
		{
			EnglishFont = mainText.Font;
			OtherLanguageFont = GameUIRoot.Instance.Manager.DefaultFont;
		}
		AdjustForChinese();
		if (m_cachedLanguage != GameManager.Options.CurrentLanguage)
		{
			m_cachedLanguage = GameManager.Options.CurrentLanguage;
			switch (pageType)
			{
			case PageType.GUNS_RIGHT:
				SetPageDataUnknown(this);
				break;
			case PageType.ITEMS_RIGHT:
				SetPageDataUnknown(this);
				break;
			case PageType.ENEMIES_RIGHT:
				SetPageDataUnknown(this);
				break;
			case PageType.BOSSES_RIGHT:
				SetPageDataUnknown(this);
				break;
			}
		}
		if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			if (mainText.Font != EnglishFont)
			{
				mainText.Atlas = guiManager.DefaultAtlas;
				mainText.Font = EnglishFont;
			}
		}
		else if (StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.JAPANESE && StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.KOREAN && StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.CHINESE && StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.RUSSIAN && mainText.Font != OtherLanguageFont)
		{
			mainText.Atlas = GameUIRoot.Instance.Manager.DefaultAtlas;
			mainText.Font = OtherLanguageFont;
		}
	}

	public void SetRightDataPageName(tk2dBaseSprite sourceSprite, EncounterDatabaseEntry linkedTrackable)
	{
		JournalEntry journalData = linkedTrackable.journalData;
		AmmonomiconPageRenderer ammonomiconPageRenderer = ((!(AmmonomiconController.Instance.ImpendingRightPageRenderer != null)) ? AmmonomiconController.Instance.CurrentRightPageRenderer : AmmonomiconController.Instance.ImpendingRightPageRenderer);
		dfScrollPanel component = ammonomiconPageRenderer.guiManager.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
		Transform transform = component.transform.Find("Header");
		if ((bool)transform)
		{
			dfLabel component2 = transform.Find("Label").GetComponent<dfLabel>();
			component2.Text = journalData.GetPrimaryDisplayName();
			if (linkedTrackable.ForceEncounterState)
			{
				component2.Text = component2.ForceGetLocalizedValue("#AMMONOMICON_UNKNOWN");
			}
			component2.PerformLayout();
			dfSprite component3 = transform.Find("Sprite").GetComponent<dfSprite>();
			if ((bool)component3)
			{
				component3.FillDirection = dfFillDirection.Vertical;
				component3.FillAmount = ((GameManager.Options.CurrentLanguage != 0) ? 0.8f : 1f);
				component3.InvertFill = true;
			}
		}
	}

	public void SetRightDataPageTexts(tk2dBaseSprite sourceSprite, EncounterDatabaseEntry linkedTrackable)
	{
		JournalEntry journalData = linkedTrackable.journalData;
		AmmonomiconPageRenderer ammonomiconPageRenderer = ((!(AmmonomiconController.Instance.ImpendingRightPageRenderer != null)) ? AmmonomiconController.Instance.CurrentRightPageRenderer : AmmonomiconController.Instance.ImpendingRightPageRenderer);
		dfScrollPanel component = ammonomiconPageRenderer.guiManager.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
		Transform transform = component.transform.Find("Header");
		if ((bool)transform)
		{
			dfLabel component2 = transform.Find("Label").GetComponent<dfLabel>();
			component2.Text = journalData.GetPrimaryDisplayName();
			if (linkedTrackable.ForceEncounterState)
			{
				component2.Text = component2.ForceGetLocalizedValue("#AMMONOMICON_UNKNOWN");
			}
			component2.PerformLayout();
			dfSprite component3 = transform.Find("Sprite").GetComponent<dfSprite>();
			if ((bool)component3)
			{
				component3.FillDirection = dfFillDirection.Vertical;
				component3.FillAmount = ((GameManager.Options.CurrentLanguage != 0) ? 0.8f : 1f);
				component3.InvertFill = true;
			}
		}
		dfLabel component4 = component.transform.Find("Tape Line One").Find("Label").GetComponent<dfLabel>();
		component4.Text = journalData.GetNotificationPanelDescription();
		component4.PerformLayout();
		dfSlicedSprite componentInChildren = component.transform.Find("Tape Line One").GetComponentInChildren<dfSlicedSprite>();
		componentInChildren.Width = component4.GetAutosizeWidth() / 4f + 12f;
		dfLabel component5 = component.transform.Find("Tape Line Two").Find("Label").GetComponent<dfLabel>();
		component5.Text = linkedTrackable.GetSecondTapeDescriptor();
		component5.PerformLayout();
		dfSlicedSprite componentInChildren2 = component.transform.Find("Tape Line Two").GetComponentInChildren<dfSlicedSprite>();
		componentInChildren2.Width = component5.GetAutosizeWidth() / 4f + 12f;
		dfPanel component6 = component.transform.Find("ThePhoto").Find("Photo").Find("tk2dSpriteHolder")
			.GetComponent<dfPanel>();
		dfSprite component7 = component.transform.Find("ThePhoto").Find("Photo").Find("ItemShadow")
			.GetComponent<dfSprite>();
		component7.IsVisible = !journalData.IsEnemy;
		tk2dSprite tk2dSprite2 = component6.GetComponentInChildren<tk2dSprite>();
		dfTextureSprite componentInChildren3 = component.transform.Find("ThePhoto").GetComponentInChildren<dfTextureSprite>();
		if (journalData.IsEnemy && journalData.enemyPortraitSprite != null)
		{
			if (tk2dSprite2 != null)
			{
				if (SpriteOutlineManager.HasOutline(tk2dSprite2))
				{
					SpriteOutlineManager.RemoveOutlineFromSprite(tk2dSprite2, true);
				}
				tk2dSprite2.renderer.enabled = false;
			}
			componentInChildren3.IsVisible = true;
			componentInChildren3.Texture = journalData.enemyPortraitSprite;
		}
		else
		{
			if (componentInChildren3 != null)
			{
				componentInChildren3.IsVisible = false;
			}
			if (tk2dSprite2 == null)
			{
				tk2dSprite2 = AddSpriteToPage(sourceSprite);
				if (!journalData.IsEnemy)
				{
					tk2dSprite2.scale *= 2f;
				}
				tk2dSprite2.transform.parent = component6.transform;
			}
			else
			{
				tk2dSprite2.renderer.enabled = true;
				tk2dSprite2.SetSprite(sourceSprite.Collection, sourceSprite.spriteId);
			}
			if (SpriteOutlineManager.HasOutline(tk2dSprite2))
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(tk2dSprite2, true);
			}
			SpriteOutlineManager.AddScaledOutlineToSprite<tk2dSprite>(tk2dSprite2, Color.black, 0.1f, 0.05f);
			if (journalData.IsEnemy)
			{
				tk2dSprite2.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
			}
			else
			{
				tk2dSprite2.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.LowerCenter);
			}
			if (Mathf.RoundToInt(sourceSprite.GetCurrentSpriteDef().GetBounds().size.x / 0.0625f) % 2 == 1)
			{
				tk2dSprite2.transform.position = tk2dSprite2.transform.position.WithX(tk2dSprite2.transform.position.x - 1f / 32f * tk2dSprite2.scale.x);
			}
			tk2dSprite2.usesOverrideMaterial = true;
			tk2dSprite2.renderer.material.shader = ShaderCache.Acquire("tk2d/CutoutVertexColorTilted");
		}
		dfLabel component8 = component.transform.Find("Scroll Panel").Find("Panel").Find("Label")
			.GetComponent<dfLabel>();
		CheckLanguageFonts(component8);
		component8.Text = linkedTrackable.GetModifiedLongDescription();
		component8.transform.parent.GetComponent<dfPanel>().Height = component8.Height;
		component8.PerformLayout();
		component8.Update();
		dfScrollPanel component9 = component.transform.Find("Scroll Panel").GetComponent<dfScrollPanel>();
		component9.ScrollPosition = Vector2.zero;
		component.PerformLayout();
		component.Update();
	}

	private IEnumerator ConstructRectanglePageLayout(dfPanel sourcePanel, List<EncounterDatabaseEntry> journalEntries, Vector2 panelPaddingPx, Vector2 elementPaddingPx, bool hideButtons = false, List<AdvancedSynergyEntry> activeSynergies = null)
	{
		float boxyBox = ((!hideButtons) ? 20 : 8);
		float p2u = guiManager.PixelsToUnits();
		float panelWidthUnits = (sourcePanel.Width - panelPaddingPx.x * 2f) * p2u;
		float remainingLineWidth = panelWidthUnits;
		List<RectangleLineInfo> lineInfos = new List<RectangleLineInfo>();
		RectangleLineInfo currentLineInfo = default(RectangleLineInfo);
		float totalUnitHeight2 = 0f;
		tk2dSpriteCollectionData iconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
		for (int i = 0; i < journalEntries.Count; i++)
		{
			if (journalEntries[i] == null)
			{
				continue;
			}
			string text = journalEntries[i].journalData.AmmonomiconSprite;
			if (text.StartsWith("gunderfury_LV"))
			{
				text = "gunderfury_LV" + (GunderfuryController.GetCurrentTier() + 1) + "0_idle_001";
			}
			tk2dSpriteDefinition tk2dSpriteDefinition2 = null;
			if (!string.IsNullOrEmpty(text))
			{
				tk2dSpriteDefinition2 = iconCollection.GetSpriteDefinition(text);
			}
			Vector2 vector = ((tk2dSpriteDefinition2 == null) ? iconCollection.GetSpriteDefinition(AmmonomiconController.AmmonomiconErrorSprite).GetBounds() : tk2dSpriteDefinition2.GetBounds()).size * 16f;
			Vector2 vector2 = (vector * 4f + elementPaddingPx * 2f) * p2u;
			if (remainingLineWidth < vector2.x)
			{
				totalUnitHeight2 += currentLineInfo.lineHeightUnits;
				if (pageType == PageType.EQUIPMENT_LEFT)
				{
					remainingLineWidth += elementPaddingPx.x * p2u * 2f + 4f * p2u;
				}
				currentLineInfo.initialXOffset = (remainingLineWidth / 2f).Quantize(p2u * 4f);
				lineInfos.Add(currentLineInfo);
				currentLineInfo = default(RectangleLineInfo);
				remainingLineWidth = panelWidthUnits;
			}
			currentLineInfo.numberOfElements++;
			currentLineInfo.lineHeightUnits = Mathf.Max(currentLineInfo.lineHeightUnits, vector2.y);
			remainingLineWidth -= vector2.x;
		}
		if (pageType == PageType.EQUIPMENT_LEFT)
		{
			remainingLineWidth += elementPaddingPx.x * p2u * 2f + 4f * p2u;
		}
		totalUnitHeight2 += currentLineInfo.lineHeightUnits;
		currentLineInfo.initialXOffset = (remainingLineWidth / 2f).Quantize(p2u * 4f);
		lineInfos.Add(currentLineInfo);
		int accumulatedSpriteIndex = 0;
		float currentYLineTop = 0f - panelPaddingPx.y * p2u;
		dfButton prevButton = null;
		if (m_prevLineButtons == null)
		{
			m_prevLineButtons = new List<dfButton>();
		}
		GameObject pokedexBox = (GameObject)BraveResources.Load("Global Prefabs/Pokedex Box");
		for (int j = 0; j < lineInfos.Count; j++)
		{
			currentLineInfo = lineInfos[j];
			List<dfButton> list = new List<dfButton>();
			for (int k = 0; k < currentLineInfo.numberOfElements; k++)
			{
				EncounterDatabaseEntry encounterDatabaseEntry = journalEntries[accumulatedSpriteIndex];
				string text2 = encounterDatabaseEntry.journalData.AmmonomiconSprite;
				if (text2.StartsWith("gunderfury_LV"))
				{
					text2 = "gunderfury_LV60_idle_001";
				}
				int spriteIdByName = iconCollection.GetSpriteIdByName(text2, -1);
				if (spriteIdByName < 0)
				{
					Debug.LogWarning("Missing sprite " + text2 + "; add this to the Ammonomicon Icon Collection.");
					spriteIdByName = iconCollection.GetSpriteIdByName(AmmonomiconController.AmmonomiconErrorSprite);
				}
				dfButton dfButton2 = sourcePanel.AddPrefab(pokedexBox) as dfButton;
				dfButton2.MakePixelPerfect();
				dfButton2.PerformLayout();
				tk2dClippedSprite tk2dClippedSprite2 = AddSpriteToPage<tk2dClippedSprite>(iconCollection, spriteIdByName);
				if (journalEntries[accumulatedSpriteIndex].path.Contains("ResourcefulRatNote"))
				{
					tk2dClippedSprite2.SetSprite("resourcefulrat_note_base_001");
				}
				float num = ((Vector2)(tk2dClippedSprite2.GetBounds().size * 16f * 4f * p2u)).x / tk2dClippedSprite2.scale.x;
				dfButton2.Size = new Vector2(num / p2u + boxyBox * 2f, currentLineInfo.lineHeightUnits / p2u - (elementPaddingPx.y * 2f - boxyBox * 2f));
				if (text2.StartsWith("gunderfury_LV"))
				{
					text2 = "gunderfury_LV" + (GunderfuryController.GetCurrentTier() + 1) + "0_idle_001";
					spriteIdByName = iconCollection.GetSpriteIdByName(text2, -1);
					tk2dClippedSprite2.SetSprite(spriteIdByName);
				}
				tk2dClippedSprite2.transform.parent = dfButton2.transform.Find("CenterPoint");
				tk2dClippedSprite2.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
				tk2dClippedSprite2.transform.position = tk2dClippedSprite2.transform.position.Quantize(4f * p2u);
				if (hideButtons)
				{
					SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite2, Color.black, 0.1f, 0.05f);
				}
				if (j == 0 && k == 0)
				{
					dfButton2.RelativePosition = new Vector3(currentLineInfo.initialXOffset / p2u + (elementPaddingPx.x - boxyBox), (0f - currentYLineTop) / p2u + (elementPaddingPx.y - boxyBox), 0f);
				}
				else if (k == 0)
				{
					dfButton2.RelativePosition = new Vector3((currentLineInfo.initialXOffset / p2u).Quantize(4f), prevButton.RelativePosition.y + prevButton.Height + 4f, 0f);
				}
				if (k > 0)
				{
					dfButton2.RelativePosition = prevButton.RelativePosition + new Vector3(prevButton.Width + 4f, 0f, 0f);
				}
				dfButton2.RelativePosition = dfButton2.RelativePosition.Quantize(4f);
				dfButton2.PerformLayout();
				AmmonomiconPokedexEntry component = dfButton2.GetComponent<AmmonomiconPokedexEntry>();
				component.IsEquipmentPage = hideButtons;
				component.IsGunderfury = text2.StartsWith("gunderfury_LV");
				component.AssignSprite(tk2dClippedSprite2);
				component.linkedEncounterTrackable = journalEntries[accumulatedSpriteIndex];
				if (hideButtons)
				{
					component.pickupID = component.linkedEncounterTrackable.pickupObjectId;
					component.activeSynergies = activeSynergies;
				}
				if (journalEntries[accumulatedSpriteIndex].ForceEncounterState)
				{
					component.encounterState = AmmonomiconPokedexEntry.EncounterState.KNOWN;
					component.ForceEncounterState = true;
				}
				component.UpdateEncounterState();
				m_pokedexEntries.Add(component);
				UIKeyControls uIKeyControls = component.gameObject.AddComponent<UIKeyControls>();
				uIKeyControls.selectOnAction = true;
				if (k > 0)
				{
					uIKeyControls.left = prevButton;
					prevButton.GetComponent<UIKeyControls>().right = dfButton2;
				}
				else
				{
					uIKeyControls.OnLeftDown = (Action)Delegate.Combine(uIKeyControls.OnLeftDown, new Action(ReturnFocusToBookmarks));
				}
				if (hideButtons)
				{
					dfButton2.Opacity = 0.01f;
				}
				list.Add(dfButton2);
				prevButton = dfButton2;
				accumulatedSpriteIndex++;
			}
			if (j == 0)
			{
				Debug.Log(string.Concat(m_prevLineButtons, "|", (m_prevLineButtons != null) ? m_prevLineButtons.Count.ToString() : "null"));
			}
			if (m_prevLineButtons != null && m_prevLineButtons.Count > 0)
			{
				for (int l = 0; l < m_prevLineButtons.Count; l++)
				{
					int value = Mathf.RoundToInt((float)l / ((float)(m_prevLineButtons.Count - 1) * 1f) * (float)(list.Count - 1));
					value = Mathf.Clamp(value, 0, list.Count - 1);
					UIKeyControls component2 = m_prevLineButtons[l].GetComponent<UIKeyControls>();
					if (component2 != null && value >= 0 && value < list.Count)
					{
						component2.down = list[value];
					}
				}
				for (int m = 0; m < list.Count; m++)
				{
					int value2 = Mathf.RoundToInt((float)m / ((float)(list.Count - 1) * 1f) * (float)(m_prevLineButtons.Count - 1));
					value2 = Mathf.Clamp(value2, 0, m_prevLineButtons.Count - 1);
					UIKeyControls component3 = list[m].GetComponent<UIKeyControls>();
					if (component3 != null && value2 >= 0 && value2 < m_prevLineButtons.Count)
					{
						component3.up = m_prevLineButtons[value2];
					}
				}
			}
			m_prevLineButtons = list;
		}
		sourcePanel.Height = totalUnitHeight2 / p2u + 2f * panelPaddingPx.y + (float)(8 * lineInfos.Count);
		yield return null;
		if (!hideButtons)
		{
			SetRightDataPageUnknown(AmmonomiconController.Instance.IsTurningPage);
		}
	}

	private void InternalInitializeEnemiesPage(bool isBosses)
	{
		Transform transform = guiManager.transform.Find("Scroll Panel").Find("Scroll Panel");
		dfPanel component = transform.Find("Guns Panel").GetComponent<dfPanel>();
		List<KeyValuePair<int, EncounterDatabaseEntry>> list = new List<KeyValuePair<int, EncounterDatabaseEntry>>();
		for (int i = 0; i < EnemyDatabase.Instance.Entries.Count; i++)
		{
			EnemyDatabaseEntry enemyDatabaseEntry = EnemyDatabase.Instance.Entries[i];
			if (enemyDatabaseEntry != null && enemyDatabaseEntry.isInBossTab == isBosses && !string.IsNullOrEmpty(enemyDatabaseEntry.encounterGuid) && !EncounterDatabase.IsProxy(enemyDatabaseEntry.encounterGuid))
			{
				int key = ((enemyDatabaseEntry.ForcedPositionInAmmonomicon >= 0) ? enemyDatabaseEntry.ForcedPositionInAmmonomicon : 1000000000);
				list.Add(new KeyValuePair<int, EncounterDatabaseEntry>(key, EncounterDatabase.GetEntry(enemyDatabaseEntry.encounterGuid)));
			}
		}
		list = list.OrderBy((KeyValuePair<int, EncounterDatabaseEntry> e) => e.Key).ToList();
		List<EncounterDatabaseEntry> list2 = new List<EncounterDatabaseEntry>();
		dfPanel component2 = component.transform.GetChild(0).GetComponent<dfPanel>();
		for (int j = 0; j < list.Count; j++)
		{
			KeyValuePair<int, EncounterDatabaseEntry> keyValuePair = list[j];
			if (keyValuePair.Value != null && !keyValuePair.Value.journalData.SuppressInAmmonomicon)
			{
				list2.Add(keyValuePair.Value);
			}
		}
		StartCoroutine(ConstructRectanglePageLayout(component2, list2, new Vector2(12f, 20f), new Vector2(20f, 20f)));
		component2.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
		component.Height = component2.Height;
		component2.Height = component.Height;
	}

	public void InitializeBossesPageLeft()
	{
		InternalInitializeEnemiesPage(true);
	}

	public void InitializeEnemiesPageLeft()
	{
		InternalInitializeEnemiesPage(false);
	}

	public void InitializeItemsPageLeft()
	{
		Transform transform = guiManager.transform.Find("Scroll Panel").Find("Scroll Panel");
		dfPanel component = transform.Find("Guns Panel").GetComponent<dfPanel>();
		List<KeyValuePair<int, PickupObject>> list = new List<KeyValuePair<int, PickupObject>>();
		for (int i = 0; i < PickupObjectDatabase.Instance.Objects.Count; i++)
		{
			PickupObject pickupObject = PickupObjectDatabase.Instance.Objects[i];
			if (!(pickupObject is Gun) && pickupObject != null)
			{
				EncounterTrackable component2 = pickupObject.GetComponent<EncounterTrackable>();
				if (!(component2 == null) && string.IsNullOrEmpty(component2.ProxyEncounterGuid) && pickupObject.quality != PickupObject.ItemQuality.EXCLUDED)
				{
					int key = ((pickupObject.ForcedPositionInAmmonomicon >= 0) ? pickupObject.ForcedPositionInAmmonomicon : 1000000000);
					list.Add(new KeyValuePair<int, PickupObject>(key, pickupObject));
				}
			}
		}
		list = list.OrderBy((KeyValuePair<int, PickupObject> e) => e.Key).ToList();
		List<EncounterDatabaseEntry> list2 = new List<EncounterDatabaseEntry>();
		dfPanel component3 = component.transform.GetChild(0).GetComponent<dfPanel>();
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Value.quality == PickupObject.ItemQuality.EXCLUDED)
			{
				continue;
			}
			EncounterTrackable component4 = list[j].Value.GetComponent<EncounterTrackable>();
			if (!component4.journalData.SuppressInAmmonomicon)
			{
				EncounterDatabaseEntry entry = EncounterDatabase.GetEntry(component4.EncounterGuid);
				if (list[j].Value is ContentTeaserItem || list[j].Value is ContentTeaserGun)
				{
					entry.ForceEncounterState = true;
				}
				if (entry != null)
				{
					list2.Add(entry);
				}
			}
		}
		StartCoroutine(ConstructRectanglePageLayout(component3, list2, new Vector2(12f, 20f), new Vector2(20f, 20f)));
		component3.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
		component.Height = component3.Height;
		component3.Height = component.Height;
	}

	public void InitializeGunsPageLeft()
	{
		Transform transform = guiManager.transform.Find("Scroll Panel").Find("Scroll Panel");
		dfPanel component = transform.Find("Guns Panel").GetComponent<dfPanel>();
		List<Gun> list = new List<Gun>();
		bool flag = GameStatsManager.HasInstance && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE);
		for (int i = 0; i < PickupObjectDatabase.Instance.Objects.Count; i++)
		{
			if (PickupObjectDatabase.Instance.Objects[i] is Gun)
			{
				Gun gun = PickupObjectDatabase.Instance.Objects[i] as Gun;
				EncounterTrackable component2 = gun.GetComponent<EncounterTrackable>();
				if (!(component2 == null) && string.IsNullOrEmpty(component2.ProxyEncounterGuid) && gun.quality != PickupObject.ItemQuality.EXCLUDED && (!flag || gun.PickupObjectId != GlobalItemIds.UnfinishedGun) && (flag || gun.PickupObjectId != GlobalItemIds.FinishedGun))
				{
					list.Add(gun);
				}
			}
		}
		list = list.OrderBy((Gun g) => (g.ForcedPositionInAmmonomicon >= 0) ? g.ForcedPositionInAmmonomicon : 1000000000).ToList();
		List<EncounterDatabaseEntry> list2 = new List<EncounterDatabaseEntry>();
		dfPanel component3 = component.transform.GetChild(0).GetComponent<dfPanel>();
		for (int j = 0; j < list.Count; j++)
		{
			EncounterTrackable component4 = list[j].GetComponent<EncounterTrackable>();
			if (component4.journalData.SuppressInAmmonomicon)
			{
				continue;
			}
			EncounterDatabaseEntry entry = EncounterDatabase.GetEntry(component4.EncounterGuid);
			if (entry != null)
			{
				if (list[j] is ContentTeaserGun)
				{
					entry.ForceEncounterState = true;
				}
				list2.Add(entry);
			}
		}
		StartCoroutine(ConstructRectanglePageLayout(component3, list2, new Vector2(12f, 20f), new Vector2(20f, 20f)));
		component3.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
		component.Height = component3.Height;
		component3.Height = component.Height;
	}

	public void InitializeEquipmentPageRight()
	{
	}

	public void InitializeEquipmentPageLeft()
	{
		Debug.Log("INITIALIZING EQUIPMENT PAGE LEFT");
		Transform transform = guiManager.transform.Find("Scroll Panel").Find("Scroll Panel");
		PrimaryClipPanel = transform.GetComponent<dfControl>();
		dfPanel component = transform.Find("Guns Panel").GetComponent<dfPanel>();
		List<EncounterDatabaseEntry> list = new List<EncounterDatabaseEntry>();
		PlayerController playerController = null;
		bool flag = false;
		if (GameManager.Instance.IsSelectingCharacter)
		{
			flag = true;
			if ((bool)Foyer.Instance.CurrentSelectedCharacterFlag)
			{
				playerController = ((GameObject)BraveResources.Load(Foyer.Instance.CurrentSelectedCharacterFlag.CharacterPrefabPath)).GetComponent<PlayerController>();
			}
		}
		else
		{
			playerController = GameManager.Instance.PrimaryPlayer;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].PlayerIDX == GameManager.Instance.LastPausingPlayerID)
				{
					playerController = GameManager.Instance.AllPlayers[i];
				}
			}
		}
		if (!(playerController != null))
		{
			return;
		}
		List<AdvancedSynergyEntry> list2 = new List<AdvancedSynergyEntry>();
		if (flag)
		{
			for (int j = 0; j < playerController.startingGunIds.Count; j++)
			{
				Gun gun = PickupObjectDatabase.GetById(playerController.startingGunIds[j]) as Gun;
				EncounterTrackable component2 = gun.GetComponent<EncounterTrackable>();
				if ((bool)component2 && !string.IsNullOrEmpty(component2.EncounterGuid))
				{
					list.Add(EncounterDatabase.GetEntry(component2.EncounterGuid));
				}
			}
		}
		else
		{
			for (int k = 0; k < playerController.ActiveExtraSynergies.Count; k++)
			{
				list2.Add(GameManager.Instance.SynergyManager.synergies[playerController.ActiveExtraSynergies[k]]);
			}
			if (playerController.inventory != null && playerController.inventory.AllGuns != null)
			{
				for (int l = 0; l < playerController.inventory.AllGuns.Count; l++)
				{
					Gun gun2 = playerController.inventory.AllGuns[l];
					if (!gun2)
					{
						continue;
					}
					MimicGunController component3 = gun2.GetComponent<MimicGunController>();
					if (!component3)
					{
						EncounterTrackable component4 = gun2.GetComponent<EncounterTrackable>();
						if ((bool)component4 && !component4.SuppressInInventory && !string.IsNullOrEmpty(component4.EncounterGuid))
						{
							list.Add(EncounterDatabase.GetEntry(component4.EncounterGuid));
						}
					}
				}
			}
		}
		dfPanel component5 = component.transform.GetChild(0).GetComponent<dfPanel>();
		StartCoroutine(ConstructRectanglePageLayout(component5, list, new Vector2(12f, 16f), new Vector2(8f, 8f), true, list2));
		component5.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
		component.Height = Mathf.Max(100f, component5.Height);
		component5.Height = component.Height;
		List<PlayerItem> list3 = playerController.activeItems;
		if (flag)
		{
			list3 = new List<PlayerItem>(playerController.startingActiveItemIds.Count);
			for (int m = 0; m < playerController.startingActiveItemIds.Count; m++)
			{
				list3.Add(PickupObjectDatabase.GetById(playerController.startingActiveItemIds[m]) as PlayerItem);
			}
		}
		if (list3 != null && list3.Count > 0)
		{
			dfPanel component6 = transform.Find("Active Items Panel").GetComponent<dfPanel>();
			list.Clear();
			for (int n = 0; n < list3.Count; n++)
			{
				PlayerItem playerItem = list3[n];
				if ((bool)playerItem)
				{
					EncounterTrackable component7 = playerItem.GetComponent<EncounterTrackable>();
					if ((bool)component7 && !component7.SuppressInInventory)
					{
						list.Add(EncounterDatabase.GetEntry(component7.EncounterGuid));
					}
				}
			}
			component5 = component6.transform.GetChild(0).GetComponent<dfPanel>();
			StartCoroutine(ConstructRectanglePageLayout(component5, list, new Vector2(12f, 16f), new Vector2(8f, 8f), true, list2));
			component5.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
			component6.Height = Mathf.Max(100f, component5.Height);
			component5.Height = component6.Height;
		}
		List<PassiveItem> list4 = playerController.passiveItems;
		if (flag)
		{
			list4 = new List<PassiveItem>(playerController.startingPassiveItemIds.Count);
			for (int num = 0; num < playerController.startingPassiveItemIds.Count; num++)
			{
				list4.Add(PickupObjectDatabase.GetById(playerController.startingPassiveItemIds[num]) as PassiveItem);
			}
		}
		if (list4 == null || list4.Count <= 0)
		{
			return;
		}
		dfPanel component8 = transform.Find("Passive Items Panel").GetComponent<dfPanel>();
		list.Clear();
		for (int num2 = 0; num2 < list4.Count; num2++)
		{
			PassiveItem passiveItem = list4[num2];
			if ((bool)passiveItem)
			{
				EncounterTrackable component9 = passiveItem.GetComponent<EncounterTrackable>();
				if ((bool)component9 && !component9.SuppressInInventory)
				{
					list.Add(EncounterDatabase.GetEntry(component9.EncounterGuid));
				}
			}
		}
		component5 = component8.transform.GetChild(0).GetComponent<dfPanel>();
		StartCoroutine(ConstructRectanglePageLayout(component5, list, new Vector2(12f, 16f), new Vector2(8f, 8f), true, list2));
		component5.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
		component8.Height = Mathf.Max(100f, component5.Height);
		component5.Height = component8.Height;
	}

	public T AddSpriteToPage<T>(tk2dSpriteCollectionData collection, int spriteID) where T : tk2dBaseSprite
	{
		GameObject gameObject = new GameObject("ammonomicon tk2d sprite");
		gameObject.transform.parent = base.transform.parent;
		T val = tk2dBaseSprite.AddComponent<T>(gameObject, collection, spriteID);
		Bounds untrimmedBounds = val.GetUntrimmedBounds();
		Vector2 vector = GameUIUtility.TK2DtoDF(untrimmedBounds.size.XY(), guiManager.PixelsToUnits());
		val.scale = new Vector3(vector.x / untrimmedBounds.size.x, vector.y / untrimmedBounds.size.y, untrimmedBounds.size.z);
		val.ignoresTiltworldDepth = true;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.layer = guiManager.gameObject.layer;
		return val;
	}

	public tk2dSprite AddSpriteToPage(tk2dBaseSprite sourceSprite)
	{
		return AddSpriteToPage<tk2dSprite>(sourceSprite.Collection, sourceSprite.spriteId);
	}

	public void SetMatrix(Matrix4x4 matrix)
	{
		renderMaterial.SetVector(topBezierPropID, matrix.GetRow(0));
		renderMaterial.SetVector(leftBezierPropID, matrix.GetRow(1));
		renderMaterial.SetVector(bottomBezierPropID, matrix.GetRow(2));
		renderMaterial.SetVector(rightBezierPropID, matrix.GetRow(3));
	}

	public void EnableRendering()
	{
		if (m_renderBuffer != null)
		{
			m_renderBuffer.DiscardContents();
		}
		guiManager.gameObject.SetActive(true);
		m_camera.depth += 1f;
		m_camera.enabled = true;
		targetRenderer.enabled = true;
		guiManager.GetComponent<dfInputManager>().enabled = true;
		Color backgroundColor = m_camera.backgroundColor;
		CameraClearFlags clearFlags = m_camera.clearFlags;
		m_camera.clearFlags = CameraClearFlags.Color;
		m_camera.backgroundColor = Color.black;
		m_camera.Render();
		m_camera.clearFlags = clearFlags;
		m_camera.backgroundColor = backgroundColor;
	}

	public void Disable(bool isPrecache = false)
	{
		m_camera.depth -= 1f;
		m_camera.enabled = false;
		targetRenderer.enabled = false;
		guiManager.GetComponent<dfInputManager>().enabled = false;
		if (isPrecache)
		{
			StartCoroutine(HandleFrameDelayedInactivation());
		}
		else
		{
			guiManager.gameObject.SetActive(false);
		}
	}

	private IEnumerator HandleFrameDelayedInactivation()
	{
		yield return null;
		guiManager.gameObject.SetActive(false);
	}

	public void Dispose()
	{
		if ((bool)m_camera)
		{
			m_camera.RemoveAllCommandBuffers();
		}
		if (m_renderBuffer != null)
		{
			RenderTexture.ReleaseTemporary(m_renderBuffer);
			m_renderBuffer = null;
		}
		if ((bool)targetRenderer)
		{
			UnityEngine.Object.Destroy(targetRenderer.gameObject);
		}
		if ((bool)guiManager)
		{
			UnityEngine.Object.Destroy(guiManager.gameObject);
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}
}
