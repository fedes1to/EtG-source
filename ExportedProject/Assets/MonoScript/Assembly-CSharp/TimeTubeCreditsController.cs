using System;
using System.Collections;
using Reaktion;
using UnityEngine;

public class TimeTubeCreditsController
{
	public static bool IsTimeTubing;

	protected Material m_decayMaterial;

	public static GameObject PreAcquiredTunnelInstance;

	public static GameObject PreAcquiredPastDiorama;

	[NonSerialized]
	public bool ForceNoTimefallForCoop;

	private bool m_shouldTimefall;

	private float m_timefallJitterMultiplier = 1f;

	protected Vector4 GetCenterPointInScreenUV(Vector2 centerPoint, float dIntensity, float dRadius)
	{
		Vector3 vector = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(centerPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, dRadius, dIntensity);
	}

	public void Cleanup()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].ClearInputOverride("time tube");
		}
		Pixelator.Instance.AdditionalCoreStackRenderPass = null;
		UnityEngine.Object.Destroy(m_decayMaterial);
	}

	public void CleanupLich()
	{
		Pixelator.Instance.AdditionalCoreStackRenderPass = null;
		UnityEngine.Object.Destroy(m_decayMaterial);
	}

	public static void AcquireTunnelInstanceInAdvance()
	{
		PreAcquiredTunnelInstance = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/ModTunnel_02"), new Vector3(-100f, -700f, 0f), Quaternion.identity);
		PreAcquiredTunnelInstance.SetActive(false);
	}

	public static void AcquirePastDioramaInAdvance()
	{
		string path = ((!GameManager.IsGunslingerPast) ? "GungeonPastDiorama" : "GungeonTruePastDiorama");
		GameObject gameObject = BraveResources.Load(path) as GameObject;
		TitleDioramaController component = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, Quaternion.identity).GetComponent<TitleDioramaController>();
		PreAcquiredPastDiorama = component.gameObject;
		PreAcquiredPastDiorama.SetActive(false);
	}

	public static void ClearPerLevelData()
	{
		PreAcquiredTunnelInstance = null;
		PreAcquiredPastDiorama = null;
	}

	public void ClearDebris()
	{
		for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
		{
			if ((bool)StaticReferenceManager.AllDebris[i])
			{
				Vector2 flatPoint = StaticReferenceManager.AllDebris[i].transform.position.XY();
				if (GameManager.Instance.MainCameraController.PointIsVisible(flatPoint))
				{
					StaticReferenceManager.AllDebris[i].TriggerDestruction();
				}
			}
		}
	}

	public IEnumerator HandleTimeTubeLightFX()
	{
		float hValue = 0f;
		while (m_shouldTimefall)
		{
			hValue = (hValue + GameManager.INVARIANT_DELTA_TIME / 4f) % 1f;
			PlatformInterface.SetAlienFXAmbientColor(new HSBColor(hValue, 1f, 1f).ToColor());
			yield return null;
		}
	}

	public IEnumerator HandleTimeTubeCredits(Vector2 decayCenter, bool skipCredits, tk2dSpriteAnimator optionalAnimatorToDisable, int shotPlayerID, bool quickEndShatter = false)
	{
		IsTimeTubing = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		GameManager.Instance.PreventPausing = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("time tube");
		}
		Shader.SetGlobalFloat("_Tubiness", 1f);
		Pixelator.Instance.DoOcclusionLayer = false;
		m_decayMaterial = new Material(ShaderCache.Acquire("Brave/Internal/WorldDecay"));
		Vector4 decaySettings = GetCenterPointInScreenUV(decayCenter, 0f, -1f);
		m_decayMaterial.SetVector("_WaveCenter", decaySettings);
		m_decayMaterial.SetTexture("_NoiseTex", BraveResources.Load("Global VFX/shatter_mask") as Texture2D);
		Pixelator.Instance.AdditionalCoreStackRenderPass = m_decayMaterial;
		float DecayDuration = 3f;
		GameObject TunnelInstance = null;
		if ((bool)PreAcquiredTunnelInstance)
		{
			TunnelInstance = PreAcquiredTunnelInstance;
			TunnelInstance.SetActive(true);
		}
		else
		{
			TunnelInstance = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/ModTunnel_02"), new Vector3(-100f, -700f, 0f), Quaternion.identity);
		}
		TunnelThatCanKillThePast TunnelController = TunnelInstance.GetComponent<TunnelThatCanKillThePast>();
		Camera TunnelCamera = TunnelController.GetComponentInChildren<Camera>();
		Pixelator.Instance.AdditionalPreBGCamera = TunnelCamera;
		BraveCameraUtility.MaintainCameraAspect(TunnelCamera);
		yield return null;
		GameManager.Instance.FlushMusicAudio();
		AkSoundEngine.PostEvent("Play_MUS_Anthem_Winner_Short_01", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent("Play_ENV_time_shatter_01", GameManager.Instance.gameObject);
		yield return null;
		m_shouldTimefall = true;
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			if (!ForceNoTimefallForCoop || GameManager.Instance.AllPlayers[j].IsPrimaryPlayer)
			{
				GameManager.Instance.StartCoroutine(HandleTimefallCorpse(GameManager.Instance.AllPlayers[j], GameManager.Instance.AllPlayers[j].PlayerIDX == shotPlayerID, TunnelCamera, TunnelController.transform));
			}
		}
		GameManager.Instance.StartCoroutine(HandleTimeTubeLightFX());
		float elapsed4 = 0f;
		float duration3 = 1f;
		float maxDecayPower = ((GameManager.Instance.PrimaryPlayer.characterIdentity != PlayableCharacters.Convict || GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.CHARACTER_PAST) ? 2.5f : 3.5f);
		bool BGCameraIsActive = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON;
		if (skipCredits)
		{
			do
			{
				BraveCameraUtility.MaintainCameraAspect(TunnelCamera);
				elapsed4 += BraveTime.DeltaTime;
				if (optionalAnimatorToDisable != null && optionalAnimatorToDisable.CurrentFrame == optionalAnimatorToDisable.CurrentClip.frames.Length - 1)
				{
					optionalAnimatorToDisable.renderer.enabled = false;
				}
				decaySettings = GetCenterPointInScreenUV(decayCenter, 0f, BraveMathCollege.LinearToSmoothStepInterpolate(-1f, maxDecayPower, elapsed4 / DecayDuration));
				m_decayMaterial.SetVector("_WaveCenter", decaySettings);
				if (BGCameraIsActive && elapsed4 > DecayDuration)
				{
					BGCameraIsActive = false;
					EndTimesNebulaController endTimesNebulaController = UnityEngine.Object.FindObjectOfType<EndTimesNebulaController>();
					if ((bool)endTimesNebulaController)
					{
						endTimesNebulaController.BecomeInactive();
					}
				}
				yield return null;
			}
			while (!(elapsed4 > 2f));
		}
		else
		{
			dfSprite creditsTopBar = GameUIRoot.Instance.Manager.transform.Find("CreditsTopBar").GetComponent<dfSprite>();
			dfSprite creditsBottomBar = GameUIRoot.Instance.Manager.transform.Find("CreditsBottomBar").GetComponent<dfSprite>();
			creditsTopBar.IsVisible = true;
			creditsBottomBar.IsVisible = true;
			creditsTopBar.BringToFront();
			creditsBottomBar.BringToFront();
			GameUIRoot.Instance.notificationController.ForceToFront();
			dfScrollPanel creditsPanel = GameUIRoot.Instance.Manager.transform.Find("CreditsPanel").GetComponent<dfScrollPanel>();
			creditsPanel.IsVisible = true;
			creditsPanel.RelativePosition = new Vector3(60f, 0f, 0f);
			int modPad = Mathf.FloorToInt((float)GameUIRoot.Instance.Manager.RenderCamera.pixelHeight / GameUIRoot.Instance.Manager.UIScale);
			creditsPanel.ScrollPadding = new RectOffset(0, 0, modPad, modPad + 90);
			creditsPanel.IsInteractive = false;
			creditsPanel.SendToBack();
			Vector2 maxScrollSize = creditsPanel.GetMaxScrollPositionDimensions();
			float accelCounter2 = 0f;
			float m_accumScrollAmount = 0f;
			do
			{
				float modDeltaTime2 = BraveTime.DeltaTime;
				GungeonActions activeActions = BraveInput.GetInstanceForPlayer(GameManager.Instance.PrimaryPlayer.PlayerIDX).ActiveActions;
				GungeonActions secondaryActions = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? null : BraveInput.GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX).ActiveActions);
				accelCounter2 = ((!activeActions.ShootAction.IsPressed && !activeActions.InteractAction.IsPressed && (secondaryActions == null || (!secondaryActions.ShootAction.IsPressed && !secondaryActions.InteractAction.IsPressed))) ? 0f : (accelCounter2 + BraveTime.DeltaTime));
				accelCounter2 = Mathf.Clamp01(accelCounter2);
				modDeltaTime2 *= Mathf.Lerp(1f, 50f, accelCounter2);
				BraveCameraUtility.MaintainCameraAspect(TunnelCamera);
				elapsed4 += modDeltaTime2;
				if (optionalAnimatorToDisable != null && optionalAnimatorToDisable.CurrentFrame == optionalAnimatorToDisable.CurrentClip.frames.Length - 1)
				{
					optionalAnimatorToDisable.renderer.enabled = false;
				}
				decaySettings = GetCenterPointInScreenUV(decayCenter, 0f, BraveMathCollege.LinearToSmoothStepInterpolate(-1f, maxDecayPower, elapsed4 / DecayDuration));
				m_decayMaterial.SetVector("_WaveCenter", decaySettings);
				if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
				{
					float t = Mathf.Clamp01(1f - (elapsed4 - DecayDuration) / DecayDuration);
					Pixelator.Instance.SetFreezeFramePower(t);
				}
				if (BGCameraIsActive && elapsed4 > DecayDuration)
				{
					BGCameraIsActive = false;
					EndTimesNebulaController endTimesNebulaController2 = UnityEngine.Object.FindObjectOfType<EndTimesNebulaController>();
					if ((bool)endTimesNebulaController2)
					{
						endTimesNebulaController2.BecomeInactive();
					}
				}
				if (elapsed4 > DecayDuration + 1f)
				{
					m_accumScrollAmount += modDeltaTime2 * 35f;
					if (m_accumScrollAmount > 1f)
					{
						float num = Mathf.Floor(m_accumScrollAmount);
						m_accumScrollAmount -= num;
						creditsPanel.ScrollPosition += new Vector2(0f, num);
					}
				}
				yield return null;
			}
			while (!(creditsPanel.ScrollPosition.y >= maxScrollSize.y - 60f));
			creditsTopBar.IsVisible = false;
			creditsBottomBar.IsVisible = false;
			UnityEngine.Object.Destroy(creditsPanel.gameObject);
		}
		Pixelator.Instance.ClearFreezeFrame();
		if (quickEndShatter)
		{
			TunnelController.shattering = true;
			elapsed4 = 0f;
			duration3 = 4f;
			Vector3 lastScreenShakeAmount = Vector3.zero;
			while (elapsed4 < duration3)
			{
				elapsed4 += BraveTime.DeltaTime;
				float targetValue2 = 0f;
				targetValue2 = ((!(elapsed4 < 1f)) ? (0.2f + Mathf.PingPong(elapsed4 - 1f, 1f) / 30f) : BraveMathCollege.LinearToSmoothStepInterpolate(0f, 0.2f, elapsed4 / 1f));
				lastScreenShakeAmount = GameManager.Instance.MainCameraController.DoFrameScreenShake(Mathf.Lerp(0f, 0.3f, elapsed4 / duration3), Mathf.Lerp(3f, 8f, elapsed4 / duration3), Vector2.zero, lastScreenShakeAmount, Time.realtimeSinceStartup);
				TunnelController.ManuallySetShatterAmount(targetValue2);
				yield return null;
			}
			Shader.SetGlobalFloat("_Tubiness", 0f);
			elapsed4 = 0f;
			float shatterDuration2 = 1f;
			m_shouldTimefall = false;
			TitleDioramaController tdc2 = null;
			if ((bool)PreAcquiredPastDiorama)
			{
				PreAcquiredPastDiorama.SetActive(true);
				tdc2 = PreAcquiredPastDiorama.GetComponent<TitleDioramaController>();
			}
			else
			{
				string path = ((!GameManager.IsGunslingerPast) ? "GungeonPastDiorama" : "GungeonTruePastDiorama");
				GameObject gameObject = BraveResources.Load(path) as GameObject;
				tdc2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, Quaternion.identity).GetComponent<TitleDioramaController>();
			}
			tdc2.ManualTrigger();
			while (elapsed4 < shatterDuration2)
			{
				elapsed4 += BraveTime.DeltaTime;
				TunnelController.ManuallySetShatterAmount(Mathf.Lerp(0.2f, -100f, elapsed4 / shatterDuration2));
				lastScreenShakeAmount = GameManager.Instance.MainCameraController.DoFrameScreenShake(Mathf.Lerp(0.5f, 0.05f, elapsed4 / shatterDuration2), Mathf.Lerp(8f, 3f, elapsed4 / shatterDuration2), Vector3.right, lastScreenShakeAmount, Time.realtimeSinceStartup);
				BraveCameraUtility.MaintainCameraAspect(TunnelCamera);
				m_timefallJitterMultiplier = Mathf.Lerp(1f, 0f, elapsed4 / duration3);
				decaySettings = new Vector4(0.5f, 0.5f, BraveMathCollege.LinearToSmoothStepInterpolate(2.5f, -1f, elapsed4 / shatterDuration2), 0f);
				m_decayMaterial.SetVector("_WaveCenter", decaySettings);
				yield return null;
			}
			GameManager.Instance.MainCameraController.ClearFrameScreenShake(lastScreenShakeAmount);
		}
		else
		{
			elapsed4 = 0f;
			duration3 = 6f;
			TunnelController.Shatter();
			dfPanel thanksPanel = null;
			if (!skipCredits)
			{
				if (GameStatsManager.Instance.GetCharacterSpecificFlag(GameManager.Instance.PrimaryPlayer.characterIdentity, CharacterSpecificGungeonFlags.KILLED_PAST))
				{
					thanksPanel = GameUIRoot.Instance.Manager.AddPrefab(BraveResources.Load("Global Prefabs/PastGameCompletePanel") as GameObject) as dfPanel;
					if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.GERMAN)
					{
						Transform transform = thanksPanel.transform.Find("Label (1)");
						if ((bool)transform)
						{
							dfLabel component = transform.GetComponent<dfLabel>();
							if ((bool)component)
							{
								component.AutoSize = true;
								component.Anchor = dfAnchorStyle.Bottom | dfAnchorStyle.CenterHorizontal;
							}
						}
					}
				}
				else
				{
					thanksPanel = GameUIRoot.Instance.Manager.AddPrefab(BraveResources.Load("Global Prefabs/StandardGameCompletePanel") as GameObject) as dfPanel;
				}
				thanksPanel.BringToFront();
				thanksPanel.PerformLayout();
			}
			Shader.SetGlobalFloat("_Tubiness", 0f);
			float shatterDuration = ((!skipCredits) ? 10f : 4f);
			while (elapsed4 < shatterDuration)
			{
				elapsed4 += BraveTime.DeltaTime;
				BraveCameraUtility.MaintainCameraAspect(TunnelCamera);
				m_timefallJitterMultiplier = Mathf.Lerp(1f, 0f, elapsed4 / duration3);
				if (thanksPanel != null)
				{
					thanksPanel.Opacity = Mathf.Lerp(0f, 1f, elapsed4 / duration3);
				}
				if (elapsed4 > shatterDuration - 1f)
				{
					m_shouldTimefall = false;
				}
				yield return null;
			}
		}
		IsTimeTubing = false;
	}

	private IEnumerator HandleTimefallCorpse(PlayerController sourcePlayer, bool isShotPlayer, Camera TunnelCamera, Transform TunnelTransform)
	{
		if (sourcePlayer.healthHaver.IsDead)
		{
			yield break;
		}
		sourcePlayer.IsVisible = false;
		sourcePlayer.IsOnFire = false;
		sourcePlayer.CurrentPoisonMeterValue = 0f;
		sourcePlayer.ToggleFollowerRenderers(false);
		GameObject timefallCorpseInstance = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/TimefallCorpse"), sourcePlayer.sprite.transform.position, Quaternion.identity);
		timefallCorpseInstance.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
		tk2dSpriteAnimator targetTimefallAnimator = timefallCorpseInstance.GetComponent<tk2dSpriteAnimator>();
		SpriteOutlineManager.AddOutlineToSprite(targetTimefallAnimator.Sprite, Color.black);
		tk2dSpriteAnimation timefallGenericLibrary = targetTimefallAnimator.Library;
		tk2dSpriteAnimation timefallSpecificLibrary = (targetTimefallAnimator.Library = ((!(sourcePlayer is PlayerSpaceshipController)) ? sourcePlayer.sprite.spriteAnimator.Library : (sourcePlayer as PlayerSpaceshipController).TimefallCorpseLibrary));
		if (isShotPlayer)
		{
			if (sourcePlayer.ArmorlessAnimations && sourcePlayer.healthHaver.Armor == 0f)
			{
				targetTimefallAnimator.Play("death_shot_armorless");
			}
			else
			{
				targetTimefallAnimator.Play("death_shot");
			}
		}
		int iterator = 0;
		tk2dSpriteAnimationClip clip7 = null;
		float timePosition = 0f;
		Vector2[] noiseVectors = new Vector2[3];
		for (int i = 0; i < 3; i++)
		{
			float f = UnityEngine.Random.value * (float)Math.PI * 2f;
			noiseVectors[i].Set(Mathf.Cos(f), Mathf.Sin(f));
		}
		Vector3 FallCenterPosOffset = Vector3.zero;
		if (!isShotPlayer)
		{
			FallCenterPosOffset = new Vector3(0.25f, -1.25f, 3f);
		}
		if (!sourcePlayer.IsPrimaryPlayer)
		{
			FallCenterPosOffset += new Vector3(0f, 0f, 1f);
		}
		Vector3 initialPosition = sourcePlayer.transform.position;
		float timefallElapsed = 0f;
		while (m_shouldTimefall)
		{
			timefallElapsed += GameManager.INVARIANT_DELTA_TIME;
			float positionOffsetStrength = 3f * m_timefallJitterMultiplier;
			float positionOffsetSpeed = 0.25f;
			timePosition += BraveTime.DeltaTime * positionOffsetSpeed;
			Vector3 p2 = new Vector3(JitterMotion.Fbm(noiseVectors[0] * timePosition, 2), JitterMotion.Fbm(noiseVectors[1] * timePosition, 2), 0f);
			p2 = p2 * positionOffsetStrength * 2f;
			Vector3 screenPoint = TunnelCamera.WorldToViewportPoint(TunnelTransform.position);
			Vector3 worldPoint = GameManager.Instance.MainCameraController.Camera.ViewportToWorldPoint(screenPoint);
			targetTimefallAnimator.transform.position = Vector3.Lerp(initialPosition, worldPoint + FallCenterPosOffset + p2, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timefallElapsed)));
			if (!targetTimefallAnimator.IsPlaying(targetTimefallAnimator.CurrentClip))
			{
				targetTimefallAnimator.ForceClearCurrentClip();
				float num = 0.5f;
				switch (iterator)
				{
				case 0:
					targetTimefallAnimator.Library = timefallSpecificLibrary;
					clip7 = targetTimefallAnimator.GetClipByName("timefall");
					if (clip7 != null)
					{
						targetTimefallAnimator.PlayForDuration("timefall", clip7.BaseClipLength);
					}
					break;
				case 1:
					targetTimefallAnimator.Library = timefallSpecificLibrary;
					clip7 = targetTimefallAnimator.GetClipByName("timefall");
					if (clip7 != null)
					{
						targetTimefallAnimator.PlayForDuration("timefall", clip7.BaseClipLength);
					}
					break;
				case 2:
					targetTimefallAnimator.Library = timefallGenericLibrary;
					clip7 = targetTimefallAnimator.GetClipByName("timefall_skull");
					if (clip7 != null)
					{
						targetTimefallAnimator.PlayForDuration("timefall_skull", clip7.BaseClipLength * 2f);
					}
					num = 1f;
					break;
				case 3:
					targetTimefallAnimator.Library = timefallGenericLibrary;
					clip7 = targetTimefallAnimator.GetClipByName("timefall_meat");
					if (clip7 != null)
					{
						targetTimefallAnimator.PlayForDuration("timefall_meat", clip7.BaseClipLength * 2f);
					}
					num = 1f;
					break;
				case 4:
					targetTimefallAnimator.Library = timefallGenericLibrary;
					clip7 = targetTimefallAnimator.GetClipByName("timefall_nerve");
					if (clip7 != null)
					{
						targetTimefallAnimator.PlayForDuration("timefall_nerve", clip7.BaseClipLength * 2f);
					}
					num = 1f;
					break;
				default:
					targetTimefallAnimator.Library = timefallSpecificLibrary;
					clip7 = targetTimefallAnimator.GetClipByName("timefall");
					if (clip7 != null)
					{
						targetTimefallAnimator.PlayForDuration("timefall", clip7.BaseClipLength);
					}
					break;
				}
				iterator = (iterator + ((UnityEngine.Random.value < num) ? 1 : 0)) % 5;
			}
			yield return null;
		}
		float elapsed = 0f;
		float duration = 1f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			timefallCorpseInstance.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
			yield return null;
		}
		UnityEngine.Object.Destroy(timefallCorpseInstance);
	}
}
