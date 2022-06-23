using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectController : MonoBehaviour
{
	public static bool HasSelected;

	public int startCharacter = 1;

	public GameObject[] playerArrows;

	public string[] playerPrefabPaths;

	public Camera uiCamera;

	public int currentSelected;

	public Transform pterodactylVFX;

	public dfSprite[] groundWinds;

	public dfSprite[] skyWinds;

	public Image FadeImage;

	protected int m_queuedChange;

	protected bool m_isTransitioning;

	protected bool m_isInitialized;

	protected Dictionary<GameObject, dfPanel> arrowToTextPanelMap = new Dictionary<GameObject, dfPanel>();

	protected GungeonActions activeActions;

	private Vector2 m_lastMousePosition = Vector2.zero;

	private int m_lastMouseSelected = -1;

	public static string GetCharacterPathFromIdentity(PlayableCharacters id)
	{
		switch (id)
		{
		case PlayableCharacters.Bullet:
			return "PlayerBullet";
		case PlayableCharacters.Convict:
			return "PlayerConvict";
		case PlayableCharacters.Guide:
			return "PlayerGuide";
		case PlayableCharacters.Pilot:
			return "PlayerRogue";
		case PlayableCharacters.Robot:
			return "PlayerRobot";
		case PlayableCharacters.Soldier:
			return "PlayerMarine";
		case PlayableCharacters.Eevee:
			return "PlayerEevee";
		case PlayableCharacters.Gunslinger:
			return "PlayerGunslinger";
		case PlayableCharacters.CoopCultist:
			return "PlayerCoopCultist";
		default:
			return "PlayerRogue";
		}
	}

	public static string GetCharacterPathFromQuickStart()
	{
		GameOptions.QuickstartCharacter quickstartCharacter = GameManager.Options.PreferredQuickstartCharacter;
		if (quickstartCharacter == GameOptions.QuickstartCharacter.LAST_USED)
		{
			switch (GameManager.Options.LastPlayedCharacter)
			{
			case PlayableCharacters.Pilot:
				quickstartCharacter = GameOptions.QuickstartCharacter.PILOT;
				break;
			case PlayableCharacters.Convict:
				quickstartCharacter = GameOptions.QuickstartCharacter.CONVICT;
				break;
			case PlayableCharacters.Guide:
				quickstartCharacter = GameOptions.QuickstartCharacter.GUIDE;
				break;
			case PlayableCharacters.Soldier:
				quickstartCharacter = GameOptions.QuickstartCharacter.SOLDIER;
				break;
			case PlayableCharacters.Bullet:
				quickstartCharacter = GameOptions.QuickstartCharacter.BULLET;
				break;
			case PlayableCharacters.Robot:
				quickstartCharacter = GameOptions.QuickstartCharacter.ROBOT;
				break;
			default:
				quickstartCharacter = GameOptions.QuickstartCharacter.PILOT;
				break;
			}
		}
		if (quickstartCharacter == GameOptions.QuickstartCharacter.BULLET && !GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05))
		{
			quickstartCharacter = GameOptions.QuickstartCharacter.PILOT;
		}
		if (quickstartCharacter == GameOptions.QuickstartCharacter.ROBOT && !GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_RECEIVED_BUSTED_TELEVISION))
		{
			quickstartCharacter = GameOptions.QuickstartCharacter.PILOT;
		}
		switch (quickstartCharacter)
		{
		case GameOptions.QuickstartCharacter.PILOT:
			return "PlayerRogue";
		case GameOptions.QuickstartCharacter.SOLDIER:
			return "PlayerMarine";
		case GameOptions.QuickstartCharacter.CONVICT:
			return "PlayerConvict";
		case GameOptions.QuickstartCharacter.GUIDE:
			return "PlayerGuide";
		case GameOptions.QuickstartCharacter.BULLET:
			return "PlayerBullet";
		case GameOptions.QuickstartCharacter.ROBOT:
			return "PlayerRobot";
		default:
			return "PlayerRogue";
		}
	}

	private void Start()
	{
		FadeImage.color = new Color(0f, 0f, 0f, 1f);
		StartCoroutine(LerpFadeAlpha(1f, 0f, 0.3f));
		HasSelected = false;
		currentSelected = startCharacter;
		m_lastMouseSelected = currentSelected;
		for (int i = 0; i < playerArrows.Length; i++)
		{
			arrowToTextPanelMap.Add(playerArrows[i], playerArrows[i].transform.parent.parent.Find("TextPanel").GetComponent<dfPanel>());
			if (currentSelected != i)
			{
				playerArrows[i].SetActive(false);
				continue;
			}
			playerArrows[i].GetComponent<tk2dSpriteAnimator>().Play();
			dfPanel dfPanel2 = arrowToTextPanelMap[playerArrows[i]];
			dfPanel2.Width = 500f;
			dfPanel2.ResolutionChangedPostLayout = (Action<dfControl, Vector3, Vector3>)Delegate.Combine(dfPanel2.ResolutionChangedPostLayout, new Action<dfControl, Vector3, Vector3>(ResolutionChangedPanel));
			ResolutionChangedPanel(dfPanel2, Vector3.zero, Vector3.zero);
		}
		StartCoroutine(HandleGroundWinds());
		StartCoroutine(HandleSkyWinds());
		StartCoroutine(HandlePterodactyl());
	}

	public void OnDestroy()
	{
		if (activeActions != null)
		{
			activeActions.Destroy();
			activeActions = null;
		}
	}

	private IEnumerator HandleSkyWinds()
	{
		while (true)
		{
			int randomWindex = UnityEngine.Random.Range(0, skyWinds.Length);
			dfSprite windSprite = skyWinds[randomWindex];
			dfSpriteAnimation windAnimation = windSprite.GetComponent<dfSpriteAnimation>();
			windSprite.IsVisible = true;
			windAnimation.Play();
			yield return new WaitForSeconds(windAnimation.Length);
			windSprite.IsVisible = false;
			windAnimation.Reset();
			yield return new WaitForSeconds(UnityEngine.Random.Range(2, 6));
		}
	}

	private IEnumerator HandleGroundWinds()
	{
		while (true)
		{
			int randomWindex = UnityEngine.Random.Range(0, groundWinds.Length);
			dfSprite windSprite = groundWinds[randomWindex];
			dfSpriteAnimation windAnimation = windSprite.GetComponent<dfSpriteAnimation>();
			windSprite.IsVisible = true;
			windAnimation.Play();
			yield return new WaitForSeconds(windAnimation.Length);
			windSprite.IsVisible = false;
			windAnimation.Reset();
			yield return new WaitForSeconds(UnityEngine.Random.Range(3, 8));
		}
	}

	private IEnumerator HandlePterodactyl()
	{
		dfSpriteAnimation animator = pterodactylVFX.GetComponent<dfSpriteAnimation>();
		dfSprite sprite = animator.GetComponent<dfSprite>();
		Vector2 startRelativePositionBase = sprite.RelativePosition;
		dfGUIManager manager = sprite.GetManager();
		yield return null;
		while (true)
		{
			sprite.IsVisible = true;
			float scaleFactor = (float)Screen.height * manager.RenderCamera.rect.height / (float)manager.FixedHeight;
			float xOffset1 = 800f * scaleFactor;
			float xOffset2 = 1200f * scaleFactor;
			float yOffset = 20f * scaleFactor;
			animator.Play();
			Vector2 startRelativePosition = startRelativePositionBase + new Vector2(0f, UnityEngine.Random.Range(0f - yOffset, yOffset));
			Vector2 targetRelativePosition = new Vector2(startRelativePosition.x + UnityEngine.Random.Range(xOffset1, xOffset2), startRelativePosition.y + UnityEngine.Random.Range(0f - yOffset, yOffset));
			float elapsed = 0f;
			float duration = animator.Length;
			while (elapsed < duration)
			{
				elapsed += BraveTime.DeltaTime;
				sprite.RelativePosition = Vector2.Lerp(startRelativePosition, targetRelativePosition, Mathf.SmoothStep(0f, 1f, elapsed / duration));
				yield return null;
			}
			sprite.IsVisible = false;
			animator.Reset();
			sprite.RelativePosition = startRelativePositionBase;
			yield return new WaitForSeconds(UnityEngine.Random.Range(6, 20));
		}
	}

	private void Initialize()
	{
		m_isInitialized = true;
		uint out_bankID = 1u;
		DebugTime.RecordStartTime();
		AkSoundEngine.LoadBank("SFX.bnk", -1, out out_bankID);
		DebugTime.Log("CharacterSelectController.Initialize.LoadBank()");
		AkSoundEngine.PostEvent("Play_AMB_night_loop_01", base.gameObject);
	}

	private void Do()
	{
		HasSelected = true;
		GameObject gameObject = playerArrows[currentSelected];
		CharacterSelectIdleDoer componentInParent = gameObject.GetComponentInParent<CharacterSelectIdleDoer>();
		componentInParent.enabled = false;
		float delayTime = 0.25f;
		if (componentInParent != null && !string.IsNullOrEmpty(componentInParent.onSelectedAnimation))
		{
			tk2dSpriteAnimationClip clipByName = componentInParent.spriteAnimator.GetClipByName(componentInParent.onSelectedAnimation);
			delayTime = (float)clipByName.frames.Length / clipByName.fps;
			componentInParent.spriteAnimator.Play(clipByName);
		}
		StartCoroutine(OnSelectedCharacter(delayTime, playerPrefabPaths[currentSelected]));
	}

	private IEnumerator OnSelectedCharacter(float delayTime, string playerPrefabPath)
	{
		yield return new WaitForSeconds(delayTime);
		StartCoroutine(LerpFadeAlpha(0f, 1f, 0.15f));
		yield return new WaitForSeconds(0.15f);
		GameManager.PlayerPrefabForNewGame = (GameObject)BraveResources.Load(playerPrefabPaths[currentSelected]);
		PlayerController playerController = GameManager.PlayerPrefabForNewGame.GetComponent<PlayerController>();
		GameStatsManager.Instance.BeginNewSession(playerController);
		GameManager.Instance.DelayedLoadNextLevel(0.25f);
	}

	private IEnumerator HandleTransition(GameObject arrowToSlide, GameObject targetArrow)
	{
		m_isTransitioning = true;
		dfPanel currentTextPanel = arrowToTextPanelMap[arrowToSlide];
		dfPanel newTextPanel = arrowToTextPanelMap[targetArrow];
		Vector3 initialPosition = arrowToSlide.transform.position;
		Vector3 targetPosition = targetArrow.transform.position;
		float elapsed2 = 0f;
		float duration2 = 0.15f;
		currentTextPanel.IsVisible = false;
		while (elapsed2 < duration2)
		{
			elapsed2 += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed2 / duration2);
			Vector3 currentPosition = Vector3.Lerp(initialPosition, targetPosition, t);
			arrowToSlide.transform.position = currentPosition;
			currentTextPanel.IsVisible = false;
			yield return null;
		}
		int targetReticleFrame = arrowToSlide.GetComponent<tk2dSpriteAnimator>().CurrentFrame + 1;
		arrowToSlide.SetActive(false);
		arrowToSlide.transform.position = initialPosition;
		currentTextPanel.IsVisible = false;
		targetArrow.SetActive(true);
		targetArrow.GetComponent<tk2dSpriteAnimator>().Play();
		targetArrow.GetComponent<tk2dSpriteAnimator>().SetFrame(targetReticleFrame);
		m_isTransitioning = false;
		elapsed2 = 0f;
		duration2 = 0.5f;
		newTextPanel.Width = 1f;
		newTextPanel.IsVisible = true;
		newTextPanel.ResolutionChangedPostLayout = null;
		newTextPanel.ResolutionChangedPostLayout = (Action<dfControl, Vector3, Vector3>)Delegate.Combine(newTextPanel.ResolutionChangedPostLayout, new Action<dfControl, Vector3, Vector3>(ResolutionChangedPanel));
		yield return new WaitForSeconds(0.45f);
		ResolutionChangedPanel(newTextPanel, Vector3.zero, Vector3.zero);
		while (elapsed2 < duration2)
		{
			elapsed2 += BraveTime.DeltaTime;
			float t2 = Mathf.SmoothStep(0f, 1f, elapsed2 / duration2);
			newTextPanel.Width = (int)Mathf.Lerp(1f, 450f, t2);
			yield return null;
		}
	}

	private void ResolutionChangedPanel(dfControl newTextPanel, Vector3 previousRelativePosition, Vector3 newRelativePosition)
	{
		dfLabel component = newTextPanel.transform.Find("NameLabel").GetComponent<dfLabel>();
		dfLabel component2 = newTextPanel.transform.Find("DescLabel").GetComponent<dfLabel>();
		dfLabel component3 = newTextPanel.transform.Find("GunLabel").GetComponent<dfLabel>();
		float num = (float)Screen.height * component.GetManager().RenderCamera.rect.height / 1080f * 4f;
		int num2 = Mathf.FloorToInt(num);
		tk2dBaseSprite sprite = newTextPanel.Parent.GetComponentsInChildren<CharacterSelectFacecardIdleDoer>(true)[0].sprite;
		newTextPanel.transform.position = sprite.transform.position + new Vector3(18f * num * component.PixelsToUnits(), 41f * num * component.PixelsToUnits(), 0f);
		component.TextScale = num;
		component2.TextScale = num;
		component3.TextScale = num;
		component.Padding = new RectOffset(2 * num2, 2 * num2, -2 * num2, num2);
		component2.Padding = new RectOffset(2 * num2, 2 * num2, -2 * num2, num2);
		component3.Padding = new RectOffset(2 * num2, 2 * num2, -2 * num2, num2);
		component.RelativePosition = new Vector3(num * 2f, num, 0f);
		component2.RelativePosition = new Vector3(0f, num + component.Size.y, 0f) + component.RelativePosition;
		component3.RelativePosition = new Vector3(0f, num + component2.Size.y, 0f) + component2.RelativePosition;
	}

	private void HandleShiftLeft()
	{
		if (m_isTransitioning)
		{
			m_queuedChange = -1;
			return;
		}
		currentSelected = (currentSelected - 1 + playerArrows.Length) % playerArrows.Length;
		AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
	}

	private void HandleShiftRight()
	{
		if (m_isTransitioning)
		{
			m_queuedChange = 1;
			return;
		}
		currentSelected = (currentSelected + 1 + playerArrows.Length) % playerArrows.Length;
		AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
	}

	private void ForceSelect(int index)
	{
		if (!m_isTransitioning)
		{
			currentSelected = index;
			AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
		}
	}

	private IEnumerator LerpFadeAlpha(float startAlpha, float targetAlpha, float duration)
	{
		yield return null;
		float elapsed = 0f;
		Color startColor = new Color(0f, 0f, 0f, startAlpha);
		Color endColor = new Color(0f, 0f, 0f, targetAlpha);
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			FadeImage.color = Color.Lerp(startColor, endColor, t);
			yield return null;
		}
	}

	public void HandleSelect()
	{
		if (!GameManager.Instance.IsLoadingLevel)
		{
			Do();
			AkSoundEngine.PostEvent("Play_UI_menu_characterselect_01", base.gameObject);
			AkSoundEngine.PostEvent("Stop_AMB_night_loop_01", base.gameObject);
		}
	}

	private void Update()
	{
		if (!m_isInitialized)
		{
			Initialize();
		}
		if (HasSelected)
		{
			return;
		}
		GameObject gameObject = playerArrows[currentSelected];
		ResolutionChangedPanel(arrowToTextPanelMap[playerArrows[currentSelected]], Vector3.zero, Vector3.zero);
		if ((Input.mousePosition.XY() - m_lastMousePosition).magnitude > 2f)
		{
			int num = -1;
			float num2 = float.MaxValue;
			Vector2 a = uiCamera.ScreenToWorldPoint(Input.mousePosition).XY();
			for (int i = 0; i < playerArrows.Length; i++)
			{
				tk2dBaseSprite component = playerArrows[i].transform.parent.GetComponent<tk2dBaseSprite>();
				Vector2 b = component.transform.position.XY() + Vector2.Scale(component.transform.localScale.XY(), Vector2.Scale(component.scale.XY(), component.GetUntrimmedBounds().extents.XY()));
				float num3 = Vector2.Distance(a, b);
				if (num3 < num2 && num3 < 0.1f)
				{
					num2 = num3;
					num = i;
				}
			}
			if (!m_isTransitioning)
			{
				if (num != -1 && num != currentSelected)
				{
					ForceSelect(num);
					currentSelected = num;
				}
				m_lastMouseSelected = num;
			}
		}
		if (activeActions.SelectLeft.WasPressedAsDpadRepeating)
		{
			HandleShiftLeft();
		}
		if (activeActions.SelectRight.WasPressedAsDpadRepeating)
		{
			HandleShiftRight();
		}
		if (activeActions.MenuSelectAction.WasPressed || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
		{
			HandleSelect();
		}
		if (Input.GetMouseButtonDown(0) && m_lastMouseSelected != -1)
		{
			currentSelected = m_lastMouseSelected;
			HandleSelect();
		}
		if (m_queuedChange != 0 && !m_isTransitioning)
		{
			if (gameObject == playerArrows[currentSelected])
			{
				currentSelected = (currentSelected + m_queuedChange + playerArrows.Length) % playerArrows.Length;
				AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
			}
			m_queuedChange = 0;
		}
		GameObject gameObject2 = playerArrows[currentSelected];
		if (gameObject != gameObject2)
		{
			StartCoroutine(HandleTransition(gameObject, gameObject2));
		}
		m_lastMousePosition = Input.mousePosition;
	}
}
