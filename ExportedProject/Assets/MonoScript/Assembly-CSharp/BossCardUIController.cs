using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCardUIController : TimeInvariantMonoBehaviour
{
	[Header("Dave Stuff")]
	public float FLASH_DURATION = 0.1f;

	public float FLASHBAR_CROSS_DURATION = 0.2f;

	public float FLASHBAR_WAIT_DURATION = 0.1f;

	public float FLASHBAR_EXPAND_DURATION = 0.2f;

	public float TEXT_IN_DURATION = 0.5f;

	public float CHARACTER_INITIAL_MOVE_DURATION = 0.5f;

	public float CHARACTER_SLIDE_DURATION = 2f;

	public float CHARACTER_SLIDE_SPEED = 0.05f;

	public float BOSS_SLIDE_SPEED = 0.05f;

	public float PARALLAX_QUANTIZATION_STEP = 0.1f;

	[Header("Not for Daves")]
	public Camera uiCamera;

	public dfTextureSprite topTriangle;

	public dfTextureSprite bottomTriangle;

	public dfTextureSprite womboBar;

	public dfTextureSprite womboBG;

	public dfTextureSprite bossSprite;

	public Transform bossStart;

	public Transform bossTarget;

	public dfTextureSprite playerSprite;

	public Transform playerStart;

	public Transform playerTarget;

	public dfTextureSprite coopSprite;

	[Header("Parallax Bros")]
	public List<dfTextureSprite> parallaxSprites;

	public List<float> parallaxSpeeds;

	public List<Transform> parallaxStarts;

	public List<Transform> parallaxEnds;

	[Header("Light Streaks")]
	public dfSprite lightStreaksSprite;

	public List<string> lightStreakSpriteNames;

	[Header("Texts")]
	public List<Transform> floatingTexts;

	public List<Transform> floatingTextStarts;

	public List<Transform> floatingTextTargets;

	public dfLabel nameLabel;

	public dfLabel subtitleLabel;

	public dfLabel quoteLabel;

	private string m_charSpriteName;

	private Texture m_bossSprite;

	private Pixelator_Simple m_pix;

	private IntVector2 m_bossSpritePxOffset;

	private IntVector2 m_topLeftTextPxOffset;

	private IntVector2 m_bottomRightTextPxOffset;

	private bool m_isPlaying;

	private float m_cachedNameLabelTextScale = -1f;

	private bool m_doLightStreaks;

	private void Initialize()
	{
		m_pix = uiCamera.GetComponent<Pixelator_Simple>();
		m_pix.Initialize();
		ToggleCoreVisiblity(false);
		ResetTextsToStart();
	}

	private void InitializeTextsShared()
	{
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN)
		{
			nameLabel.PerCharacterOffset = Vector3.zero;
			subtitleLabel.PerCharacterOffset = Vector3.zero;
			quoteLabel.PerCharacterOffset = Vector3.zero;
			nameLabel.transform.rotation = Quaternion.identity;
			subtitleLabel.transform.rotation = Quaternion.identity;
			quoteLabel.transform.rotation = Quaternion.identity;
			dfLabel obj = nameLabel;
			bool flag = false;
			nameLabel.Outline = flag;
			obj.Shadow = flag;
			dfLabel obj2 = subtitleLabel;
			flag = false;
			subtitleLabel.Outline = flag;
			obj2.Shadow = flag;
			dfLabel obj3 = quoteLabel;
			flag = false;
			quoteLabel.Outline = flag;
			obj3.Shadow = flag;
		}
	}

	public void InitializeTexts(PortraitSlideSettings pss)
	{
		m_topLeftTextPxOffset = pss.topLeftTextPxOffset;
		m_bottomRightTextPxOffset = pss.bottomRightTextPxOffset;
		if ((bool)GameManager.Instance.Dungeon)
		{
			nameLabel.Glitchy = GameManager.Instance.Dungeon.IsGlitchDungeon;
			subtitleLabel.Glitchy = GameManager.Instance.Dungeon.IsGlitchDungeon;
		}
		if (m_cachedNameLabelTextScale == -1f)
		{
			m_cachedNameLabelTextScale = nameLabel.TextScale;
		}
		nameLabel.Text = StringTableManager.GetEnemiesString(pss.bossNameString);
		float autosizeWidth = nameLabel.GetAutosizeWidth();
		if (autosizeWidth > 800f)
		{
			nameLabel.PerCharacterOffset = new Vector3(0f, 2f, 0f);
			nameLabel.TextScale = 1000f / autosizeWidth * m_cachedNameLabelTextScale;
			m_topLeftTextPxOffset += new IntVector2(0, -6);
			m_topLeftTextPxOffset += new IntVector2(0, -6);
		}
		else
		{
			nameLabel.PerCharacterOffset = new Vector3(0f, 3f, 0f);
			nameLabel.TextScale = m_cachedNameLabelTextScale;
		}
		InitializeTextsShared();
		subtitleLabel.Text = StringTableManager.GetEnemiesString(pss.bossSubtitleString);
		quoteLabel.Text = StringTableManager.GetEnemiesString(pss.bossQuoteString);
		m_bossSprite = pss.bossArtSprite;
		m_bossSpritePxOffset = pss.bossSpritePxOffset;
	}

	private IEnumerator InvariantWaitForSeconds(float seconds)
	{
		float elapsed = 0f;
		while (elapsed < seconds)
		{
			elapsed += m_deltaTime;
			yield return null;
		}
	}

	public void TriggerSequence()
	{
		for (int i = 0; i < parallaxSprites.Count; i++)
		{
			if (!parallaxSprites[i])
			{
				parallaxSprites.RemoveAt(i);
				parallaxEnds.RemoveAt(i);
				parallaxStarts.RemoveAt(i);
				parallaxSpeeds.RemoveAt(i);
				i--;
			}
		}
		StartCoroutine(CoreSequence(null));
	}

	private void ToggleCoreVisiblity(bool visible)
	{
		for (int i = 0; i < parallaxSprites.Count; i++)
		{
			if (!parallaxSprites[i])
			{
				parallaxSprites.RemoveAt(i);
				parallaxEnds.RemoveAt(i);
				parallaxStarts.RemoveAt(i);
				parallaxSpeeds.RemoveAt(i);
				i--;
			}
		}
		if (!m_bossSprite)
		{
			bossSprite.IsVisible = false;
			playerSprite.IsVisible = false;
			coopSprite.IsVisible = false;
		}
		else
		{
			bossSprite.IsVisible = visible;
			bossSprite.Texture = m_bossSprite;
			playerSprite.IsVisible = !GameManager.Instance.PrimaryPlayer.healthHaver.IsDead;
			coopSprite.IsVisible = GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !GameManager.Instance.SecondaryPlayer.healthHaver.IsDead;
			if (coopSprite.IsVisible)
			{
				coopSprite.ZOrder = bossSprite.ZOrder - 1;
			}
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			playerSprite.Texture = primaryPlayer.BosscardSprites[0];
			if (primaryPlayer.characterIdentity == PlayableCharacters.Eevee)
			{
				Shader shader = Shader.Find("Brave/Internal/GlitchEevee");
				Material material = new Material(shader);
				material.name = "Default Texture Shader";
				material.hideFlags = HideFlags.DontSave;
				material.mainTexture = primaryPlayer.BosscardSprites[0];
				Material material2 = material;
				material2.SetTexture("_EeveeTex", primaryPlayer.GetComponent<CharacterAnimationRandomizer>().CosmicTex);
				material2.SetFloat("_WaveIntensity", 0.1f);
				material2.SetFloat("_ColorIntensity", 0.015f);
				playerSprite.OverrideMaterial = material2;
			}
			else if (playerSprite.OverrideMaterial != null)
			{
				playerSprite.OverrideMaterial = null;
			}
			BraveDFTextureAnimator component = playerSprite.GetComponent<BraveDFTextureAnimator>();
			component.timeless = true;
			component.textures = GameManager.Instance.PrimaryPlayer.BosscardSprites.ToArray();
			component.fps = GameManager.Instance.PrimaryPlayer.BosscardSpriteFPS;
			RecalculateScales();
		}
		topTriangle.IsVisible = visible;
		bottomTriangle.IsVisible = visible;
		if (!visible)
		{
			womboBG.IsVisible = false;
			womboBar.IsVisible = false;
			lightStreaksSprite.IsVisible = false;
			for (int j = 0; j < parallaxSprites.Count; j++)
			{
				parallaxSprites[j].IsVisible = false;
			}
		}
	}

	private void RecalculateScales()
	{
		dfGUIManager manager = coopSprite.GetManager();
		Vector2 screenSize = manager.GetScreenSize();
		playerSprite.Size = manager.GetScreenSize();
		if (coopSprite != null)
		{
			float num = 1.7791667f;
			coopSprite.Size = new Vector2(screenSize.y * num, screenSize.y);
		}
		float num2 = (float)bossSprite.Texture.width / (float)bossSprite.Texture.height;
		bossSprite.Size = new Vector2(screenSize.y * num2, screenSize.y);
	}

	private Vector3 GetCoopOffset()
	{
		Vector2 a = playerTarget.localPosition.XY();
		if (playerSprite.IsVisible)
		{
			PerCharacterCoopPositionData coopBosscardOffset = GameManager.Instance.PrimaryPlayer.CoopBosscardOffset;
			if (coopBosscardOffset.flipCoopCultist && coopSprite.transform.localScale.x != -1f)
			{
				coopSprite.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
			Vector2 b = new Vector2(coopBosscardOffset.percentOffset.x * -1f, coopBosscardOffset.percentOffset.y);
			Vector2 vector = Vector2.Scale(a, b);
			return vector.ToVector3ZUp();
		}
		return Vector3.zero;
	}

	public void ToggleBoxing(bool enable)
	{
		if (!enable)
		{
			Pixelator.Instance.LerpToLetterbox(1f, 0f);
			Pixelator.Instance.SetWindowbox(1f);
			return;
		}
		float num = 1.77777779f;
		float aSPECT = BraveCameraUtility.ASPECT;
		if (num < aSPECT)
		{
			Pixelator.Instance.SetWindowbox(num / aSPECT * 0.5f);
		}
		else if (num > aSPECT)
		{
			Pixelator.Instance.LerpToLetterbox(num / aSPECT * 0.5f, 0f);
		}
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)playerSprite && playerSprite.OverrideMaterial != null)
		{
			playerSprite.OverrideMaterial.SetFloat("_AdditionalTime", playerSprite.OverrideMaterial.GetFloat("_AdditionalTime") + GameManager.INVARIANT_DELTA_TIME / 4f);
		}
	}

	public IEnumerator CoreSequence(PortraitSlideSettings pss)
	{
		if (!m_isPlaying)
		{
			m_isPlaying = true;
			Initialize();
			ToggleBoxing(true);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.ToggleUICamera(false);
			RecalculateScales();
			lightStreaksSprite.IsVisible = false;
			for (int i = 0; i < parallaxSprites.Count; i++)
			{
				parallaxSprites[i].IsVisible = false;
			}
			if ((bool)playerSprite)
			{
				playerSprite.IsVisible = false;
			}
			if ((bool)coopSprite)
			{
				coopSprite.IsVisible = false;
			}
			Material targetMaterial = m_pix.RenderMaterial;
			StartCoroutine(FlashWhiteToBlack(targetMaterial, false));
			BraveMemory.HandleBossCardFlashAnticipation();
			yield return StartCoroutine(InvariantWaitForSeconds(FLASH_DURATION));
			bossSprite.transform.position = bossStart.position;
			playerSprite.transform.position = playerStart.position;
			if (coopSprite.IsVisible)
			{
				coopSprite.transform.position = playerSprite.transform.position + GetCoopOffset();
			}
			ToggleCoreVisiblity(true);
			if ((bool)playerSprite)
			{
				playerSprite.IsVisible = false;
			}
			if ((bool)coopSprite)
			{
				coopSprite.IsVisible = false;
			}
			StartCoroutine(HandleLightStreaks());
			yield return StartCoroutine(InvariantWaitForSeconds(FLASH_DURATION));
			targetMaterial.SetColor("_OverrideColor", Color.clear);
			StartCoroutine(WomboCombo(pss));
			yield return StartCoroutine(InvariantWaitForSeconds(FLASHBAR_CROSS_DURATION));
			StartCoroutine(LerpTextsToTargets());
			float waitDuration = Mathf.Max(FLASHBAR_WAIT_DURATION + FLASHBAR_EXPAND_DURATION, TEXT_IN_DURATION);
			yield return StartCoroutine(InvariantWaitForSeconds(waitDuration));
			yield return StartCoroutine(InvariantWaitForSeconds(0.1f));
			yield return StartCoroutine(HandleCharacterSlides());
			StartCoroutine(FlashWhiteToBlack(targetMaterial, true));
			yield return StartCoroutine(InvariantWaitForSeconds(FLASH_DURATION));
			ToggleCoreVisiblity(false);
			m_doLightStreaks = false;
			ResetTextsToStart();
			GameUIRoot.Instance.ToggleUICamera(true);
			GameUIRoot.Instance.ShowCoreUI(string.Empty);
			ToggleBoxing(false);
			m_isPlaying = false;
		}
	}

	public void BreakSequence()
	{
		GameUIRoot.Instance.ToggleUICamera(true);
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		ToggleBoxing(false);
		m_isPlaying = false;
	}

	private IEnumerator HandleLightStreaks()
	{
		lightStreaksSprite.IsVisible = true;
		List<float> parallaxTs = new List<float>();
		for (int i = 0; i < parallaxSprites.Count; i++)
		{
			dfTextureSprite dfTextureSprite2 = parallaxSprites[i];
			dfTextureSprite2.IsVisible = true;
			dfTextureSprite2.transform.position = parallaxStarts[i].position;
			parallaxTs.Add(0f);
		}
		m_doLightStreaks = true;
		float elapsed = 0f;
		float individualSpriteDuration = 0.1f;
		int currentLightStreakSprite = 0;
		while (m_doLightStreaks)
		{
			elapsed += m_deltaTime;
			if (elapsed > individualSpriteDuration)
			{
				elapsed -= individualSpriteDuration;
				currentLightStreakSprite = (currentLightStreakSprite + 1) % lightStreakSpriteNames.Count;
				lightStreaksSprite.SpriteName = lightStreakSpriteNames[currentLightStreakSprite];
			}
			for (int j = 0; j < parallaxSprites.Count; j++)
			{
				parallaxTs[j] = (parallaxTs[j] + parallaxSpeeds[j] * m_deltaTime) % 1f;
				float t = parallaxTs[j].Quantize(PARALLAX_QUANTIZATION_STEP);
				parallaxSprites[j].transform.position = Vector3.Lerp(parallaxStarts[j].position, parallaxEnds[j].position, t);
			}
			yield return null;
		}
		lightStreaksSprite.IsVisible = false;
		for (int k = 0; k < parallaxSprites.Count; k++)
		{
			parallaxSprites[k].IsVisible = false;
		}
	}

	private IEnumerator WomboCombo(PortraitSlideSettings pss)
	{
		womboBG.IsVisible = true;
		womboBG.Color = Color.black;
		womboBG.Opacity = 1f;
		womboBG.ZOrder = 1;
		womboBar.ZOrder = 2;
		lightStreaksSprite.ZOrder = 0;
		for (int i = 0; i < parallaxSprites.Count; i++)
		{
			parallaxSprites[i].ZOrder = 0;
		}
		womboBar.IsVisible = true;
		womboBar.Opacity = 1f;
		float crossDuration = FLASHBAR_CROSS_DURATION;
		float waitDuration = FLASHBAR_WAIT_DURATION;
		float expandDuration = FLASHBAR_EXPAND_DURATION;
		dfGUIManager manager = womboBar.GetManager();
		Vector2 screenSize = manager.GetScreenSize();
		float elapsed2 = 0f;
		while (elapsed2 < crossDuration)
		{
			elapsed2 += m_deltaTime;
			float t = elapsed2 / crossDuration;
			womboBar.Width = Mathf.Lerp(0f, screenSize.x * 1.5f, t);
			yield return null;
		}
		yield return StartCoroutine(InvariantWaitForSeconds(waitDuration));
		elapsed2 = 0f;
		while (elapsed2 < expandDuration)
		{
			elapsed2 += m_deltaTime;
			float t2 = elapsed2 / expandDuration;
			womboBar.Height = Mathf.Lerp(10f, screenSize.y * 1.5f, t2);
			float quadT = t2 * t2 * t2 * t2;
			womboBar.Opacity = 1f - quadT;
			womboBG.Opacity = Mathf.Lerp(1f, 0.8f, quadT);
			womboBG.Color = Color.Lerp(Color.black, (pss == null) ? Color.blue : pss.bgColor, quadT);
			yield return null;
		}
		womboBG.ZOrder = 0;
		lightStreaksSprite.ZOrder = 1;
		womboBar.IsVisible = false;
		womboBar.Size = new Vector2(0f, 10f);
	}

	private IEnumerator HandleDelayedCoopCharacterSlide()
	{
		if (GameManager.Instance.CurrentGameType != 0)
		{
			float initialMoveDuration = CHARACTER_INITIAL_MOVE_DURATION;
			float slideDuration = CHARACTER_SLIDE_DURATION - CHARACTER_INITIAL_MOVE_DURATION;
			float elapsed2 = 0f;
			Vector3 playerVec = playerTarget.position - playerStart.position;
			coopSprite.transform.position = playerStart.position + GetCoopOffset();
			float p2u = playerSprite.PixelsToUnits();
			while (elapsed2 < initialMoveDuration)
			{
				elapsed2 += m_deltaTime;
				Vector3 calcedPlayerPos = Vector3.Lerp(t: elapsed2 / initialMoveDuration, a: playerStart.position, b: playerTarget.position);
				coopSprite.transform.position = calcedPlayerPos + GetCoopOffset();
				yield return null;
			}
			elapsed2 = 0f;
			Vector3 currentRealPlayerPosition = playerTarget.position;
			while (elapsed2 < slideDuration)
			{
				elapsed2 += m_deltaTime;
				currentRealPlayerPosition += playerVec.normalized * m_deltaTime * BOSS_SLIDE_SPEED;
				coopSprite.transform.position = currentRealPlayerPosition.Quantize(p2u) + GetCoopOffset();
				yield return null;
			}
		}
	}

	private IEnumerator HandleCharacterSlides()
	{
		playerSprite.IsVisible = !GameManager.Instance.PrimaryPlayer.healthHaver.IsDead;
		coopSprite.IsVisible = GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !GameManager.Instance.SecondaryPlayer.healthHaver.IsDead;
		float initialMoveDuration = CHARACTER_INITIAL_MOVE_DURATION;
		float slideDuration = CHARACTER_SLIDE_DURATION;
		float elapsed2 = 0f;
		Vector3 playerVec = playerTarget.position - playerStart.position;
		Vector3 bossVec = bossTarget.position - bossStart.position;
		bossSprite.transform.position = bossStart.position;
		playerSprite.transform.position = playerStart.position;
		float p2u = bossSprite.PixelsToUnits();
		Vector3 bossOffset = m_bossSpritePxOffset.ToVector3() * p2u;
		while (elapsed2 < initialMoveDuration)
		{
			elapsed2 += m_deltaTime;
			float t = elapsed2 / initialMoveDuration;
			bossSprite.transform.position = Vector3.Lerp(bossStart.position + bossOffset, bossTarget.position + bossOffset, t);
			playerSprite.transform.position = Vector3.Lerp(playerStart.position, playerTarget.position, t);
			yield return null;
		}
		StartCoroutine(HandleDelayedCoopCharacterSlide());
		elapsed2 = 0f;
		Vector3 currentRealBossPosition = bossSprite.transform.position;
		Vector3 currentRealPlayerPosition = playerSprite.transform.position;
		while (elapsed2 < slideDuration)
		{
			elapsed2 += m_deltaTime;
			currentRealBossPosition += bossVec.normalized * m_deltaTime * CHARACTER_SLIDE_SPEED;
			currentRealPlayerPosition += playerVec.normalized * m_deltaTime * BOSS_SLIDE_SPEED;
			bossSprite.transform.position = currentRealBossPosition.Quantize(p2u);
			playerSprite.transform.position = currentRealPlayerPosition.Quantize(p2u);
			yield return null;
		}
	}

	private IEnumerator LerpTextsToTargets()
	{
		float lerpDuration = TEXT_IN_DURATION;
		float elapsed = 0f;
		float p2u = bossSprite.PixelsToUnits();
		Vector3 topLeftTextOffset = m_topLeftTextPxOffset.ToVector3() * p2u;
		Vector3 bottomRightTextOffset = m_bottomRightTextPxOffset.ToVector3() * p2u;
		while (elapsed < lerpDuration)
		{
			elapsed += m_deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / lerpDuration);
			for (int i = 0; i < floatingTexts.Count; i++)
			{
				Vector3 zero = Vector3.zero;
				dfControl component = floatingTexts[i].GetComponent<dfControl>();
				if (component.Pivot == dfPivotPoint.BottomRight)
				{
					Vector3 vector = component.Size.ToVector3ZUp() * component.PixelsToUnits();
					vector.y *= -1f;
				}
				Vector3 vector2 = ((i != 1) ? topLeftTextOffset : bottomRightTextOffset);
				floatingTexts[i].position = Vector3.Lerp(floatingTextStarts[i].position + vector2, floatingTextTargets[i].position + vector2, t);
			}
			yield return null;
		}
	}

	private void ResetTextsToStart()
	{
		for (int i = 0; i < floatingTexts.Count; i++)
		{
			floatingTexts[i].position = floatingTextStarts[i].position;
		}
		playerSprite.transform.position = playerStart.position;
		if (coopSprite.IsVisible)
		{
			coopSprite.transform.position = playerSprite.transform.position + GetCoopOffset();
		}
		bossSprite.transform.position = bossStart.position;
	}

	private IEnumerator FlashColorToColor(Color startColor, Color targetColor, float fadeDuration, Material targetMaterial)
	{
		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += m_deltaTime;
			float t = elapsed / fadeDuration;
			Color c = Color.Lerp(startColor, targetColor, t);
			targetMaterial.SetColor("_OverrideColor", c);
			yield return null;
		}
	}

	private IEnumerator FlashWhiteToBlack(Material targetMaterial, bool backToClear)
	{
		float fadeDuration = FLASH_DURATION;
		PlatformInterface.SetAlienFXColor(new Color(1f, 1f, 1f, 1f), 0.5f);
		yield return StartCoroutine(FlashColorToColor(Color.clear, Color.white, fadeDuration, targetMaterial));
		yield return StartCoroutine(FlashColorToColor(Color.white, Color.black, fadeDuration, targetMaterial));
		if (backToClear)
		{
			yield return StartCoroutine(FlashColorToColor(Color.black, Color.clear, fadeDuration, targetMaterial));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
