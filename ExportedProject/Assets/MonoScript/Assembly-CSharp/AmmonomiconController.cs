using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmonomiconController : MonoBehaviour
{
	public static string AmmonomiconErrorSprite = "zombullet_idle_front_001";

	private static AmmonomiconController m_instance;

	public string AmmonomiconEquipmentLeftPagePath;

	public string AmmonomiconEquipmentRightPagePath;

	public List<AmmonomiconFrameDefinition> OpenAnimationFrames;

	public List<AmmonomiconFrameDefinition> TurnPageRightAnimationFrames;

	public List<AmmonomiconFrameDefinition> TurnPageLeftAnimationFrames;

	public List<AmmonomiconFrameDefinition> CloseAnimationFrames;

	public AnimationCurve DepartureYCurve;

	public float TotalDepartureTime = 0.5f;

	public float DepartureYTotalDistance = -5f;

	public tk2dSpriteCollectionData EncounterIconCollection;

	private AmmonomiconFrameDefinition m_currentFrameDefinition;

	public GameObject AmmonomiconBasePrefab;

	[SerializeField]
	private float GLOBAL_ANIMATION_SCALE = 1f;

	private GameObject m_AmmonomiconBase;

	private AmmonomiconInstanceManager m_AmmonomiconInstance;

	private MeshRenderer m_LowerRenderTargetPrefab;

	private MeshRenderer m_UpperRenderTargetPrefab;

	private dfTextureSprite m_AmmonomiconLowerImage;

	private dfTextureSprite m_AmmonomiconUpperImage;

	private dfTextureSprite m_AmmonomiconOptionalThirdImage;

	private dfTextureSprite m_CurrentLeft_RenderTarget;

	private dfTextureSprite m_CurrentRight_RenderTarget;

	private AmmonomiconPageRenderer m_CurrentLeftPageManager;

	private AmmonomiconPageRenderer m_CurrentRightPageManager;

	private AmmonomiconPageRenderer m_ImpendingLeftPageManager;

	private AmmonomiconPageRenderer m_ImpendingRightPageManager;

	private bool m_isOpening;

	private bool m_isOpen;

	private bool m_isPageTransitioning;

	private List<bool> m_offsetInUse = new List<bool>();

	private List<Vector3> m_offsets = new List<Vector3>();

	private const float m_PAGE_DEPTH = -0.5f;

	private bool m_applicationFocus = true;

	public bool HandlingQueuedUnlocks;

	private float m_cachedCorePanelY = -1f;

	private Dictionary<AmmonomiconPageRenderer.PageType, AmmonomiconPageRenderer> m_extantPageMap = new Dictionary<AmmonomiconPageRenderer.PageType, AmmonomiconPageRenderer>();

	private bool m_transitionIsQueued;

	private string m_queuedLeftPath;

	private AmmonomiconPageRenderer.PageType m_queuedLeftType;

	private string m_queuedRightPath;

	private AmmonomiconPageRenderer.PageType m_queuedRightType;

	private bool m_queuedNextPage;

	private bool m_isClosing;

	public static AmmonomiconController Instance
	{
		get
		{
			if (BraveUtility.isLoadingLevel)
			{
				return null;
			}
			if (GameManager.Instance.Dungeon == null)
			{
				return null;
			}
			if (m_instance == null)
			{
				AmmonomiconController ammonomiconController = Object.FindObjectOfType<AmmonomiconController>();
				if (ammonomiconController == null)
				{
					Debug.LogError("INSTANTIATING AMMONOMICON ???");
					GameObject gameObject = (GameObject)Object.Instantiate(BraveResources.Load("Ammonomicon Controller"));
					ammonomiconController = gameObject.GetComponent<AmmonomiconController>();
				}
				m_instance = ammonomiconController;
			}
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance;
		}
	}

	public static AmmonomiconController ForceInstance
	{
		get
		{
			if (m_instance == null)
			{
				AmmonomiconController ammonomiconController = Object.FindObjectOfType<AmmonomiconController>();
				if (ammonomiconController == null)
				{
					Debug.LogError("INSTANTIATING AMMONOMICON ???");
					GameObject gameObject = (GameObject)Object.Instantiate(BraveResources.Load("Ammonomicon Controller"));
					ammonomiconController = gameObject.GetComponent<AmmonomiconController>();
				}
				m_instance = ammonomiconController;
			}
			return m_instance;
		}
	}

	public bool IsOpening
	{
		get
		{
			return m_isOpening;
		}
	}

	public bool IsClosing
	{
		get
		{
			return m_isClosing;
		}
	}

	public bool IsOpen
	{
		get
		{
			return m_isOpen;
		}
	}

	public bool BookmarkHasFocus
	{
		get
		{
			if (!m_isOpen)
			{
				return false;
			}
			return m_AmmonomiconInstance.BookmarkHasFocus;
		}
	}

	public AmmonomiconInstanceManager Ammonomicon
	{
		get
		{
			return m_AmmonomiconInstance;
		}
	}

	public AmmonomiconPageRenderer BestInteractingLeftPageRenderer
	{
		get
		{
			if (IsTurningPage && ImpendingLeftPageRenderer != null)
			{
				return ImpendingLeftPageRenderer;
			}
			return CurrentLeftPageRenderer;
		}
	}

	public AmmonomiconPageRenderer BestInteractingRightPageRenderer
	{
		get
		{
			if (IsTurningPage && ImpendingRightPageRenderer != null)
			{
				return ImpendingRightPageRenderer;
			}
			return CurrentRightPageRenderer;
		}
	}

	public AmmonomiconPageRenderer CurrentLeftPageRenderer
	{
		get
		{
			return m_CurrentLeftPageManager;
		}
	}

	public AmmonomiconPageRenderer CurrentRightPageRenderer
	{
		get
		{
			return m_CurrentRightPageManager;
		}
	}

	public AmmonomiconPageRenderer ImpendingLeftPageRenderer
	{
		get
		{
			return m_ImpendingLeftPageManager;
		}
	}

	public AmmonomiconPageRenderer ImpendingRightPageRenderer
	{
		get
		{
			return m_ImpendingRightPageManager;
		}
	}

	public bool IsTurningPage
	{
		get
		{
			return m_isPageTransitioning;
		}
	}

	public static bool GuiManagerIsPageRenderer(dfGUIManager manager)
	{
		if (m_instance != null && m_instance.IsOpen && m_instance.m_AmmonomiconInstance != null && m_instance.m_AmmonomiconInstance.GetComponent<dfGUIManager>() == manager)
		{
			return true;
		}
		return false;
	}

	public static void EnsureExistence()
	{
		if (!(GameManager.Instance.Dungeon == null) && m_instance == null)
		{
			AmmonomiconController ammonomiconController = Object.FindObjectOfType<AmmonomiconController>();
			if (ammonomiconController == null)
			{
				Debug.LogError("INSTANTIATING AMMONOMICON ???");
				GameObject gameObject = (GameObject)Object.Instantiate(BraveResources.Load("Ammonomicon Controller"));
				ammonomiconController = gameObject.GetComponent<AmmonomiconController>();
			}
			m_instance = ammonomiconController;
		}
	}

	public void ReturnFocusToBookmark()
	{
		m_AmmonomiconInstance.bookmarks[m_AmmonomiconInstance.CurrentlySelectedTabIndex].ForceFocus();
	}

	private void Awake()
	{
		for (int i = 0; i < 12; i++)
		{
			m_offsetInUse.Add(false);
			m_offsets.Add(new Vector3(-200 + -20 * i, -200 + -20 * i, 0f));
		}
	}

	private void Start()
	{
		PrecacheAllData();
	}

	public void PrecacheAllData()
	{
		m_AmmonomiconBase = Object.Instantiate(AmmonomiconBasePrefab, new Vector3(-500f, -500f, 0f), Quaternion.identity);
		m_AmmonomiconInstance = m_AmmonomiconBase.GetComponent<AmmonomiconInstanceManager>();
		Transform transform = m_AmmonomiconBase.transform.Find("Core");
		m_AmmonomiconLowerImage = transform.Find("Ammonomicon Bottom").GetComponent<dfTextureSprite>();
		m_AmmonomiconUpperImage = transform.Find("Ammonomicon Top").GetComponent<dfTextureSprite>();
		m_AmmonomiconOptionalThirdImage = transform.Find("Ammonomicon Toppest").GetComponent<dfTextureSprite>();
		m_AmmonomiconOptionalThirdImage.Material = new Material(ShaderCache.Acquire("Daikon Forge/Default UI Shader Highest Queue"));
		m_AmmonomiconOptionalThirdImage.IsVisible = false;
		m_AmmonomiconUpperImage.Material = new Material(ShaderCache.Acquire("Daikon Forge/Default UI Shader High Queue"));
		m_LowerRenderTargetPrefab = transform.Find("Ammonomicon Page Renderer Lower").GetComponent<MeshRenderer>();
		m_LowerRenderTargetPrefab.enabled = false;
		m_UpperRenderTargetPrefab = transform.Find("Ammonomicon Page Renderer Upper").GetComponent<MeshRenderer>();
		m_UpperRenderTargetPrefab.enabled = false;
		m_AmmonomiconInstance.GuiManager.RenderCamera.enabled = false;
		m_AmmonomiconInstance.GuiManager.enabled = false;
		AmmonomiconInstanceManager component = AmmonomiconBasePrefab.GetComponent<AmmonomiconInstanceManager>();
		Transform transform2 = new GameObject("_Ammonomicon").transform;
		m_AmmonomiconBase.transform.parent = transform2;
		base.transform.parent = transform2;
		for (int i = 0; i < component.bookmarks.Length - 1; i++)
		{
			AmmonomiconPageRenderer ammonomiconPageRenderer = LoadPageUIAtPath(component.bookmarks[i].TargetNewPageLeft, component.bookmarks[i].LeftPageType, true);
			AmmonomiconPageRenderer ammonomiconPageRenderer2 = LoadPageUIAtPath(component.bookmarks[i].TargetNewPageRight, component.bookmarks[i].RightPageType, true);
			ammonomiconPageRenderer.transform.parent.parent = transform2;
			ammonomiconPageRenderer2.transform.parent.parent = transform2;
		}
		Object.DontDestroyOnLoad(transform2.gameObject);
	}

	private void OpenInternal(bool isDeath, bool isVictory, EncounterTrackable targetTrackable = null)
	{
		m_isOpening = true;
		while (dfGUIManager.GetModalControl() != null)
		{
			Debug.LogError(dfGUIManager.GetModalControl().name + " was modal, popping...");
			dfGUIManager.PopModal();
		}
		m_isPageTransitioning = true;
		m_AmmonomiconInstance.GuiManager.enabled = true;
		m_AmmonomiconInstance.GuiManager.RenderCamera.enabled = true;
		int num = (isDeath ? (m_AmmonomiconInstance.bookmarks.Length - 1) : 0);
		m_CurrentLeftPageManager = LoadPageUIAtPath(m_AmmonomiconInstance.bookmarks[num].TargetNewPageLeft, (!isDeath) ? AmmonomiconPageRenderer.PageType.EQUIPMENT_LEFT : AmmonomiconPageRenderer.PageType.DEATH_LEFT, false, isVictory);
		m_CurrentRightPageManager = LoadPageUIAtPath(m_AmmonomiconInstance.bookmarks[num].TargetNewPageRight, (!isDeath) ? AmmonomiconPageRenderer.PageType.EQUIPMENT_RIGHT : AmmonomiconPageRenderer.PageType.DEATH_RIGHT, false, isVictory);
		m_CurrentLeftPageManager.ForceUpdateLanguageFonts();
		m_CurrentRightPageManager.ForceUpdateLanguageFonts();
		if (m_CurrentRightPageManager.pageType == AmmonomiconPageRenderer.PageType.EQUIPMENT_RIGHT && m_CurrentLeftPageManager.LastFocusTarget != null)
		{
			AmmonomiconPokedexEntry component = (m_CurrentLeftPageManager.LastFocusTarget as dfButton).GetComponent<AmmonomiconPokedexEntry>();
			m_CurrentRightPageManager.SetRightDataPageTexts(component.ChildSprite, component.linkedEncounterTrackable);
		}
		else if (m_CurrentRightPageManager.pageType == AmmonomiconPageRenderer.PageType.EQUIPMENT_RIGHT)
		{
			m_CurrentRightPageManager.SetRightDataPageUnknown();
		}
		m_CurrentRightPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
		StartCoroutine(HandleOpenAmmonomicon(isDeath, GameManager.Options.HasEverSeenAmmonomicon, targetTrackable));
		GameManager.Options.HasEverSeenAmmonomicon = true;
	}

	public void OpenAmmonomiconToTrackable(EncounterTrackable targetTrackable)
	{
		if (!m_isOpen && !m_isOpening)
		{
			m_isOpen = true;
			OpenInternal(false, false, targetTrackable);
		}
	}

	public void OpenAmmonomicon(bool isDeath, bool isVictory)
	{
		if (!m_isOpen && !m_isOpening)
		{
			m_isOpen = true;
			OpenInternal(isDeath, isVictory);
		}
	}

	private void LateUpdate()
	{
		if (Pixelator.Instance == null || !(m_AmmonomiconBase != null) || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		dfGUIManager component = m_AmmonomiconBase.GetComponent<dfGUIManager>();
		component.UIScale = Pixelator.Instance.ScaleTileScale / 3f;
		Vector2 screenSize = component.GetScreenSize();
		Vector2 vector = new Vector2(screenSize.x / 1920f, screenSize.y / 1080f);
		float num = Pixelator.Instance.ScaleTileScale / 3f;
		if (m_CurrentLeftPageManager != null)
		{
			m_CurrentLeftPageManager.targetRenderer.transform.localScale = new Vector3(1.77777779f * vector.x, 2f * vector.x, 1f) * num;
			m_CurrentLeftPageManager.targetRenderer.transform.localPosition = new Vector3(-0.5f * m_CurrentLeftPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			if (m_currentFrameDefinition != null)
			{
				m_CurrentLeftPageManager.targetRenderer.transform.localPosition += Vector3.Scale(m_currentFrameDefinition.CurrentLeftOffset, m_CurrentLeftPageManager.targetRenderer.transform.localScale);
			}
		}
		if (m_CurrentRightPageManager != null)
		{
			m_CurrentRightPageManager.targetRenderer.transform.localScale = new Vector3(1.77777779f * vector.x, 2f * vector.x, 1f) * num;
			m_CurrentRightPageManager.targetRenderer.transform.localPosition = new Vector3(0.5f * m_CurrentRightPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			if (m_currentFrameDefinition != null)
			{
				m_CurrentRightPageManager.targetRenderer.transform.localPosition += Vector3.Scale(m_currentFrameDefinition.CurrentRightOffset, m_CurrentRightPageManager.targetRenderer.transform.localScale);
			}
		}
		if (m_ImpendingLeftPageManager != null)
		{
			m_ImpendingLeftPageManager.targetRenderer.transform.localScale = new Vector3(1.77777779f * vector.x, 2f * vector.x, 1f) * num;
			m_ImpendingLeftPageManager.targetRenderer.transform.localPosition = new Vector3(-0.5f * m_ImpendingLeftPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			if (m_currentFrameDefinition != null)
			{
				m_ImpendingLeftPageManager.targetRenderer.transform.localPosition += Vector3.Scale(m_currentFrameDefinition.ImpendingLeftOffset, m_ImpendingLeftPageManager.targetRenderer.transform.localScale);
			}
		}
		if (m_ImpendingRightPageManager != null)
		{
			m_ImpendingRightPageManager.targetRenderer.transform.localScale = new Vector3(1.77777779f * vector.x, 2f * vector.x, 1f) * num;
			m_ImpendingRightPageManager.targetRenderer.transform.localPosition = new Vector3(0.5f * m_ImpendingRightPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			if (m_currentFrameDefinition != null)
			{
				m_ImpendingRightPageManager.targetRenderer.transform.localPosition += Vector3.Scale(m_currentFrameDefinition.ImpendingRightOffset, m_ImpendingRightPageManager.targetRenderer.transform.localScale);
			}
		}
		if (!(m_CurrentLeftPageManager != null) || !(m_CurrentRightPageManager != null))
		{
			return;
		}
		if (Input.mousePosition.x > (float)Screen.width / 2f)
		{
			if (m_CurrentRightPageManager.guiManager.RenderCamera.depth <= m_CurrentLeftPageManager.guiManager.RenderCamera.depth)
			{
				m_CurrentRightPageManager.guiManager.RenderCamera.depth = 4f;
			}
		}
		else if (m_CurrentLeftPageManager.guiManager.RenderCamera.depth <= m_CurrentRightPageManager.guiManager.RenderCamera.depth)
		{
			m_CurrentRightPageManager.guiManager.RenderCamera.depth = 1f;
		}
	}

	public void OnApplicationFocus(bool focusStatus)
	{
		m_applicationFocus = focusStatus;
	}

	private IEnumerator HandleOpenAmmonomicon(bool isDeath, bool isShortAnimation, EncounterTrackable targetTrackable = null)
	{
		List<AmmonomiconFrameDefinition> TargetAnimationFrames = OpenAnimationFrames;
		if (isShortAnimation)
		{
			AkSoundEngine.PostEvent("Play_UI_ammonomicon_open_01", base.gameObject);
			TargetAnimationFrames = new List<AmmonomiconFrameDefinition>();
			for (int i = 0; i < 9; i++)
			{
				TargetAnimationFrames.Add(OpenAnimationFrames[i]);
			}
			for (int j = 23; j < OpenAnimationFrames.Count; j++)
			{
				TargetAnimationFrames.Add(OpenAnimationFrames[j]);
			}
		}
		else
		{
			AkSoundEngine.PostEvent("Play_UI_ammonomicon_intro_01", base.gameObject);
		}
		float animationTime = GetAnimationLength(TargetAnimationFrames);
		float elapsed = 0f;
		int currentFrameIndex = 0;
		float nextFrameTime = TargetAnimationFrames[0].frameTime * GLOBAL_ANIMATION_SCALE;
		SetFrame(TargetAnimationFrames[0]);
		while (elapsed < animationTime)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			if (elapsed >= animationTime)
			{
				break;
			}
			if (elapsed >= nextFrameTime)
			{
				currentFrameIndex++;
				nextFrameTime += TargetAnimationFrames[currentFrameIndex].frameTime * GLOBAL_ANIMATION_SCALE;
				SetFrame(TargetAnimationFrames[currentFrameIndex]);
			}
			while (!m_applicationFocus)
			{
				yield return null;
			}
			yield return null;
		}
		SetFrame(TargetAnimationFrames[TargetAnimationFrames.Count - 1]);
		if (isDeath)
		{
			m_AmmonomiconInstance.OpenDeath();
		}
		else
		{
			m_AmmonomiconInstance.Open();
		}
		if (targetTrackable != null)
		{
			AmmonomiconPokedexEntry pokedexEntry = CurrentLeftPageRenderer.GetPokedexEntry(targetTrackable);
			if (pokedexEntry != null)
			{
				Debug.Log("GET INFO SUCCESS");
				pokedexEntry.ForceFocus();
			}
		}
		m_isPageTransitioning = false;
		HandleQueuedUnlocks();
	}

	private void HandleQueuedUnlocks()
	{
		List<EncounterDatabaseEntry> queuedTrackables = GameManager.Instance.GetQueuedTrackables();
		if (queuedTrackables.Count > 0)
		{
			StartCoroutine(HandleQueuedUnlocks_CR(queuedTrackables));
		}
		else
		{
			m_isOpening = false;
		}
	}

	private IEnumerator HandleQueuedUnlocks_CR(List<EncounterDatabaseEntry> trackableData)
	{
		HandlingQueuedUnlocks = true;
		for (int i = 0; i < trackableData.Count; i++)
		{
			yield return null;
			EncounterDatabaseEntry trackable = trackableData[i];
			GameObject hasAppearedInstance = (GameObject)Object.Instantiate(BraveResources.Load("Global Prefabs/AppearedInTheGungeonRoot"), new Vector3(-1200f, 300f, 0f), Quaternion.identity);
			dfPanel hasAppearedPanel = hasAppearedInstance.GetComponentInChildren<dfPanel>();
			hasAppearedPanel.BringToFront();
			AppearedInTheGungeonController apparator = hasAppearedPanel.GetComponent<AppearedInTheGungeonController>();
			apparator.Appear(trackable);
			while ((!(BraveInput.PrimaryPlayerInstance != null) || !BraveInput.PrimaryPlayerInstance.ActiveActions.AnyActionPressed()) && (GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER || !(BraveInput.SecondaryPlayerInstance != null) || !BraveInput.SecondaryPlayerInstance.ActiveActions.AnyActionPressed()))
			{
				yield return null;
			}
			apparator.ShwoopClosed();
			GameManager.Instance.AcknowledgeKnownTrackable(trackable);
			float ela = 0f;
			while (ela < 0.2f)
			{
				ela += GameManager.INVARIANT_DELTA_TIME;
				yield return null;
			}
		}
		HandlingQueuedUnlocks = false;
		m_isOpening = false;
	}

	private void SetFrame(AmmonomiconFrameDefinition def)
	{
		m_currentFrameDefinition = def;
		m_AmmonomiconLowerImage.IsVisible = def.AmmonomiconBottomLayerTexture != null;
		if (m_AmmonomiconLowerImage.IsVisible)
		{
			m_AmmonomiconLowerImage.Texture = def.AmmonomiconBottomLayerTexture;
		}
		m_AmmonomiconUpperImage.IsVisible = def.AmmonomiconTopLayerTexture != null;
		if (m_AmmonomiconUpperImage.IsVisible)
		{
			m_AmmonomiconUpperImage.Texture = def.AmmonomiconTopLayerTexture;
		}
		if (def.AmmonomiconToppestLayerTexture != null)
		{
			m_AmmonomiconOptionalThirdImage.IsVisible = true;
			m_AmmonomiconOptionalThirdImage.Texture = def.AmmonomiconToppestLayerTexture;
		}
		else
		{
			m_AmmonomiconOptionalThirdImage.IsVisible = false;
		}
		if (m_CurrentLeftPageManager != null)
		{
			m_CurrentLeftPageManager.targetRenderer.transform.localPosition = new Vector3(-0.5f * m_CurrentLeftPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			m_CurrentLeftPageManager.targetRenderer.transform.localPosition += Vector3.Scale(def.CurrentLeftOffset, m_CurrentLeftPageManager.targetRenderer.transform.localScale);
			m_CurrentLeftPageManager.targetRenderer.enabled = def.CurrentLeftVisible;
			if (def.CurrentLeftVisible)
			{
				m_CurrentLeftPageManager.SetMatrix(def.CurrentLeftMatrix);
			}
		}
		if (m_CurrentRightPageManager != null)
		{
			m_CurrentRightPageManager.targetRenderer.transform.localPosition = new Vector3(0.5f * m_CurrentRightPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			m_CurrentRightPageManager.targetRenderer.transform.localPosition += Vector3.Scale(def.CurrentRightOffset, m_CurrentRightPageManager.targetRenderer.transform.localScale);
			m_CurrentRightPageManager.targetRenderer.enabled = def.CurrentRightVisible;
			if (def.CurrentRightVisible)
			{
				m_CurrentRightPageManager.SetMatrix(def.CurrentRightMatrix);
			}
		}
		if (m_ImpendingLeftPageManager != null)
		{
			m_ImpendingLeftPageManager.targetRenderer.transform.localPosition = new Vector3(-0.5f * m_ImpendingLeftPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			m_ImpendingLeftPageManager.targetRenderer.transform.localPosition += Vector3.Scale(def.ImpendingLeftOffset, m_ImpendingLeftPageManager.targetRenderer.transform.localScale);
			m_ImpendingLeftPageManager.targetRenderer.enabled = def.ImpendingLeftVisible;
			if (def.ImpendingLeftVisible)
			{
				m_ImpendingLeftPageManager.SetMatrix(def.ImpendingLeftMatrix);
			}
		}
		if (m_ImpendingRightPageManager != null)
		{
			m_ImpendingRightPageManager.targetRenderer.transform.localPosition = new Vector3(0.5f * m_ImpendingRightPageManager.targetRenderer.transform.localScale.x, 0f, -0.5f);
			m_ImpendingRightPageManager.targetRenderer.transform.localPosition += Vector3.Scale(def.ImpendingRightOffset, m_ImpendingRightPageManager.targetRenderer.transform.localScale);
			m_ImpendingRightPageManager.targetRenderer.enabled = def.ImpendingRightVisible;
			if (def.ImpendingRightVisible)
			{
				m_ImpendingRightPageManager.SetMatrix(def.ImpendingRightMatrix);
			}
		}
	}

	public void CloseAmmonomicon(bool doDestroy = false)
	{
		if (!m_isClosing && !m_isOpening)
		{
			AkSoundEngine.PostEvent("Stop_UI_ammonomicon_open_01", base.gameObject);
			AkSoundEngine.PostEvent("Play_UI_menu_back_01", base.gameObject);
			m_isClosing = true;
			m_isPageTransitioning = true;
			StartCoroutine(HandleCloseAmmonomicon(doDestroy));
		}
	}

	private void ForceTerminateClosing()
	{
		m_isClosing = false;
	}

	private IEnumerator HandleCloseMotion()
	{
		float elapsed = 0f;
		dfPanel targetPanel = m_AmmonomiconBase.transform.Find("Core").GetComponent<dfPanel>();
		if (m_cachedCorePanelY == -1f)
		{
			m_cachedCorePanelY = targetPanel.RelativePosition.y;
		}
		targetPanel.RelativePosition = targetPanel.RelativePosition.WithY(m_cachedCorePanelY);
		float startRelativeY = targetPanel.RelativePosition.y;
		while (elapsed < TotalDepartureTime && m_isClosing)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / TotalDepartureTime;
			targetPanel.RelativePosition = Vector3Extensions.WithY(y: startRelativeY + DepartureYCurve.Evaluate(t) * DepartureYTotalDistance, vector: targetPanel.RelativePosition);
			yield return null;
		}
		while (m_isOpen && m_isClosing)
		{
			yield return null;
		}
		targetPanel.RelativePosition = targetPanel.RelativePosition.WithY(startRelativeY);
	}

	private IEnumerator HandleCloseAmmonomicon(bool doDestroy = false)
	{
		List<AmmonomiconFrameDefinition> TargetAnimationFrames = CloseAnimationFrames;
		float animationTime = GetAnimationLength(TargetAnimationFrames);
		float elapsed = 0f;
		SetFrame(TargetAnimationFrames[0]);
		StartCoroutine(HandleCloseMotion());
		while (elapsed < animationTime && m_isClosing)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			if (elapsed >= animationTime)
			{
				break;
			}
			yield return null;
		}
		if (m_CurrentLeftPageManager != null)
		{
			m_CurrentLeftPageManager.Disable();
		}
		if (m_CurrentRightPageManager != null)
		{
			m_CurrentRightPageManager.Disable();
		}
		if (m_ImpendingLeftPageManager != null)
		{
			m_ImpendingLeftPageManager.Disable();
		}
		if (m_ImpendingRightPageManager != null)
		{
			m_ImpendingRightPageManager.Disable();
		}
		m_CurrentLeftPageManager = null;
		m_CurrentRightPageManager = null;
		m_AmmonomiconInstance.Close();
		m_AmmonomiconInstance.GuiManager.RenderCamera.enabled = false;
		m_AmmonomiconInstance.GuiManager.enabled = false;
		m_isPageTransitioning = false;
		m_isClosing = false;
		m_isOpen = false;
	}

	private float GetAnimationLength(List<AmmonomiconFrameDefinition> frames)
	{
		float num = 0f;
		for (int i = 0; i < frames.Count; i++)
		{
			num += frames[i].frameTime * GLOBAL_ANIMATION_SCALE;
		}
		return num;
	}

	private AmmonomiconPageRenderer LoadPageUIAtPath(string path, AmmonomiconPageRenderer.PageType pageType, bool isPreCache = false, bool isVictory = false)
	{
		AmmonomiconPageRenderer ammonomiconPageRenderer = null;
		if (m_extantPageMap.ContainsKey(pageType))
		{
			ammonomiconPageRenderer = m_extantPageMap[pageType];
			if (pageType == AmmonomiconPageRenderer.PageType.DEATH_LEFT || pageType == AmmonomiconPageRenderer.PageType.DEATH_RIGHT)
			{
				AmmonomiconDeathPageController component = ammonomiconPageRenderer.transform.parent.GetComponent<AmmonomiconDeathPageController>();
				component.isVictoryPage = isVictory;
			}
			ammonomiconPageRenderer.EnableRendering();
			ammonomiconPageRenderer.DoRefreshData();
		}
		else
		{
			GameObject gameObject = (GameObject)Object.Instantiate(BraveResources.Load(path));
			ammonomiconPageRenderer = gameObject.GetComponentInChildren<AmmonomiconPageRenderer>();
			dfGUIManager component2 = m_AmmonomiconBase.GetComponent<dfGUIManager>();
			GameObject gameObject2 = Object.Instantiate(m_LowerRenderTargetPrefab.gameObject);
			gameObject2.transform.parent = component2.transform.Find("Core");
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.layer = LayerMask.NameToLayer("SecondaryGUI");
			MeshRenderer component3 = gameObject2.GetComponent<MeshRenderer>();
			if (isVictory)
			{
				AmmonomiconDeathPageController component4 = ammonomiconPageRenderer.transform.parent.GetComponent<AmmonomiconDeathPageController>();
				component4.isVictoryPage = true;
			}
			ammonomiconPageRenderer.Initialize(component3);
			ammonomiconPageRenderer.EnableRendering();
			for (int i = 0; i < m_offsets.Count; i++)
			{
				if (!m_offsetInUse[i])
				{
					m_offsetInUse[i] = true;
					gameObject.transform.position = m_offsets[i];
					ammonomiconPageRenderer.offsetIndex = i;
					break;
				}
			}
			m_extantPageMap.Add(pageType, ammonomiconPageRenderer);
			if (isPreCache)
			{
				ammonomiconPageRenderer.Disable(isPreCache);
			}
			else
			{
				ammonomiconPageRenderer.transform.parent.parent = m_AmmonomiconBase.transform.parent;
			}
		}
		return ammonomiconPageRenderer;
	}

	private void MakeImpendingCurrent()
	{
		if (m_isOpen)
		{
			m_CurrentLeftPageManager.Disable();
			m_CurrentRightPageManager.Disable();
			m_CurrentLeftPageManager = m_ImpendingLeftPageManager;
			m_CurrentRightPageManager = m_ImpendingRightPageManager;
			m_CurrentLeftPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
			m_CurrentRightPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
			m_ImpendingLeftPageManager = null;
			m_ImpendingRightPageManager = null;
		}
	}

	public void TurnToPreviousPage(string pathToNextLeftPage, AmmonomiconPageRenderer.PageType leftPageType, string pathToNextRightPage, AmmonomiconPageRenderer.PageType rightPageType)
	{
		if (m_isPageTransitioning)
		{
			SetQueuedTransition(false, pathToNextLeftPage, leftPageType, pathToNextRightPage, rightPageType);
			return;
		}
		m_isPageTransitioning = true;
		m_ImpendingLeftPageManager = LoadPageUIAtPath(pathToNextLeftPage, leftPageType);
		m_ImpendingRightPageManager = LoadPageUIAtPath(pathToNextRightPage, rightPageType);
		m_ImpendingLeftPageManager.UpdateOnBecameActive();
		m_ImpendingRightPageManager.UpdateOnBecameActive();
		m_ImpendingRightPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconTransitionPageShader");
		m_CurrentLeftPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconTransitionPageShader");
		m_CurrentRightPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
		m_ImpendingLeftPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
		StartCoroutine(HandleTurnToPreviousPage());
	}

	private IEnumerator HandleTurnToPreviousPage()
	{
		AkSoundEngine.PostEvent("Play_UI_page_turn_01", base.gameObject);
		float animationTime = GetAnimationLength(TurnPageLeftAnimationFrames);
		float elapsed = 0f;
		int currentFrameIndex = 0;
		float nextFrameTime = TurnPageLeftAnimationFrames[0].frameTime * GLOBAL_ANIMATION_SCALE;
		SetFrame(TurnPageLeftAnimationFrames[0]);
		while (elapsed < animationTime)
		{
			if (!m_isOpen)
			{
				yield break;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			if (elapsed >= animationTime)
			{
				break;
			}
			if (elapsed >= nextFrameTime)
			{
				currentFrameIndex++;
				nextFrameTime += TurnPageLeftAnimationFrames[currentFrameIndex].frameTime * GLOBAL_ANIMATION_SCALE;
				SetFrame(TurnPageLeftAnimationFrames[currentFrameIndex]);
			}
			yield return null;
		}
		if (!m_isOpen)
		{
			yield break;
		}
		MakeImpendingCurrent();
		SetFrame(OpenAnimationFrames[OpenAnimationFrames.Count - 1]);
		m_isPageTransitioning = false;
		if (m_transitionIsQueued)
		{
			if (m_queuedNextPage)
			{
				TurnToNextPage(m_queuedLeftPath, m_queuedLeftType, m_queuedRightPath, m_queuedRightType);
			}
			else
			{
				TurnToPreviousPage(m_queuedLeftPath, m_queuedLeftType, m_queuedRightPath, m_queuedRightType);
			}
			m_transitionIsQueued = false;
		}
	}

	private void SetQueuedTransition(bool nextPage, string pathToNextLeftPage, AmmonomiconPageRenderer.PageType leftPageType, string pathToNextRightPage, AmmonomiconPageRenderer.PageType rightPageType)
	{
		if (!m_isClosing)
		{
			if (m_isPageTransitioning && ImpendingLeftPageRenderer.pageType == leftPageType)
			{
				m_transitionIsQueued = false;
				return;
			}
			m_transitionIsQueued = true;
			m_queuedLeftPath = pathToNextLeftPage;
			m_queuedLeftType = leftPageType;
			m_queuedRightPath = pathToNextRightPage;
			m_queuedRightType = rightPageType;
			m_queuedNextPage = nextPage;
		}
	}

	public void TurnToNextPage(string pathToNextLeftPage, AmmonomiconPageRenderer.PageType leftPageType, string pathToNextRightPage, AmmonomiconPageRenderer.PageType rightPageType)
	{
		if (m_isPageTransitioning)
		{
			SetQueuedTransition(true, pathToNextLeftPage, leftPageType, pathToNextRightPage, rightPageType);
			return;
		}
		m_isPageTransitioning = true;
		m_ImpendingLeftPageManager = LoadPageUIAtPath(pathToNextLeftPage, leftPageType);
		m_ImpendingRightPageManager = LoadPageUIAtPath(pathToNextRightPage, rightPageType);
		m_ImpendingLeftPageManager.UpdateOnBecameActive();
		m_ImpendingRightPageManager.UpdateOnBecameActive();
		m_ImpendingLeftPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconTransitionPageShader");
		m_CurrentRightPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconTransitionPageShader");
		m_CurrentLeftPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
		m_ImpendingRightPageManager.targetRenderer.sharedMaterial.shader = ShaderCache.Acquire("Custom/AmmonomiconPageShader");
		StartCoroutine(HandleTurnToNextPage());
	}

	private IEnumerator HandleTurnToNextPage()
	{
		AkSoundEngine.PostEvent("Play_UI_page_turn_01", base.gameObject);
		float animationTime = GetAnimationLength(TurnPageRightAnimationFrames);
		float elapsed = 0f;
		int currentFrameIndex = 0;
		float nextFrameTime = TurnPageRightAnimationFrames[0].frameTime * GLOBAL_ANIMATION_SCALE;
		SetFrame(TurnPageRightAnimationFrames[0]);
		while (elapsed < animationTime)
		{
			if (!m_isOpen)
			{
				yield break;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			if (elapsed >= animationTime)
			{
				break;
			}
			if (elapsed >= nextFrameTime)
			{
				currentFrameIndex++;
				nextFrameTime += TurnPageRightAnimationFrames[currentFrameIndex].frameTime * GLOBAL_ANIMATION_SCALE;
				SetFrame(TurnPageRightAnimationFrames[currentFrameIndex]);
			}
			yield return null;
		}
		if (!m_isOpen)
		{
			yield break;
		}
		MakeImpendingCurrent();
		SetFrame(OpenAnimationFrames[OpenAnimationFrames.Count - 1]);
		m_isPageTransitioning = false;
		if (m_transitionIsQueued)
		{
			if (m_queuedNextPage)
			{
				TurnToNextPage(m_queuedLeftPath, m_queuedLeftType, m_queuedRightPath, m_queuedRightType);
			}
			else
			{
				TurnToPreviousPage(m_queuedLeftPath, m_queuedLeftType, m_queuedRightPath, m_queuedRightType);
			}
			m_transitionIsQueued = false;
		}
	}
}
