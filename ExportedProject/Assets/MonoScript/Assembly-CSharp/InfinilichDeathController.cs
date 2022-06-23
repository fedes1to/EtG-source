using System.Collections;
using UnityEngine;

public class InfinilichDeathController : BraveBehaviour
{
	public GameObject bigExplosionVfx;

	public GameObject finalExplosionVfx;

	public GameObject eyePos;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.SuppressContinuousKillCamBulletDestruction = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.ForceStop();
		}
		base.aiAnimator.PlayUntilCancelled("death", true);
		GameManager.Instance.StartCoroutine(OnDeathExplosionsCR());
	}

	protected Vector2 GetTargetClockhairPosition(Vector2 currentClockhairPosition)
	{
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(GameManager.Instance.PrimaryPlayer.PlayerIDX);
		Vector2 rhs2 = Vector2.Max(rhs: (!instanceForPlayer.IsKeyboardAndMouse()) ? (currentClockhairPosition + instanceForPlayer.ActiveActions.Aim.Vector * 10f * BraveTime.DeltaTime) : (GameManager.Instance.MainCameraController.Camera.ScreenToWorldPoint(Input.mousePosition).XY() + new Vector2(0.375f, -0.25f)), lhs: GameManager.Instance.MainCameraController.MinVisiblePoint);
		return Vector2.Min(GameManager.Instance.MainCameraController.MaxVisiblePoint, rhs2);
	}

	private bool CheckTarget(GameActor target, Transform clockhairTransform)
	{
		Vector2 a = clockhairTransform.position.XY() + new Vector2(-0.375f, 0.25f);
		return Vector2.Distance(a, target.CenterPosition + new Vector2(0f, -1.25f)) < 0.875f;
	}

	private IEnumerator HandleClockhair(PlayerController interactor)
	{
		GameManager.Instance.PrimaryPlayer.SetInputOverride("past");
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.SecondaryPlayer.SetInputOverride("past");
		}
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		CameraController camera = GameManager.Instance.MainCameraController;
		Vector2 currentCamCenter = camera.OverridePosition;
		Vector2 desiredCamCenter = base.aiAnimator.sprite.WorldCenter;
		camera.SetManualControl(true);
		if (Vector2.Distance(currentCamCenter, desiredCamCenter) > 2.5f)
		{
			camera.OverridePosition = desiredCamCenter;
		}
		Transform clockhairTransform = ((GameObject)Object.Instantiate(BraveResources.Load("Clockhair"))).transform;
		ClockhairController clockhair = clockhairTransform.GetComponent<ClockhairController>();
		float elapsed = 0f;
		float duration = clockhair.ClockhairInDuration;
		Vector3 clockhairTargetPosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
		Vector3 clockhairStartPosition = clockhairTargetPosition + new Vector3(-20f, 5f, 0f);
		clockhair.renderer.enabled = true;
		clockhair.spriteAnimator.alwaysUpdateOffscreen = true;
		clockhair.spriteAnimator.Play("clockhair_intro");
		clockhair.hourAnimator.Play("hour_hand_intro");
		clockhair.minuteAnimator.Play("minute_hand_intro");
		clockhair.secondAnimator.Play("second_hand_intro");
		while (elapsed < duration)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / duration;
			float smoothT = Mathf.SmoothStep(0f, 1f, t);
			clockhairTargetPosition = GetTargetClockhairPosition(clockhairTargetPosition);
			Vector3 currentPosition = Vector3.Slerp(clockhairStartPosition, clockhairTargetPosition, smoothT);
			clockhairTransform.position = currentPosition.WithZ(0f);
			UpdateEyes(clockhairTransform.position, false);
			if (t > 0.5f)
			{
				clockhair.renderer.enabled = true;
			}
			if (t > 0.75f)
			{
				clockhair.hourAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.minuteAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.secondAnimator.GetComponent<Renderer>().enabled = true;
			}
			clockhair.sprite.UpdateZDepth();
			yield return null;
		}
		clockhair.SetMotionType(1f);
		BraveInput currentInput = BraveInput.GetInstanceForPlayer(GameManager.Instance.PrimaryPlayer.PlayerIDX);
		float shotTargetTime2 = 0f;
		float holdDuration = 4f;
		Vector3 lastScreenShakeAmount2 = Vector3.zero;
		Vector3 lastJitterAmount = Vector3.zero;
		while (true)
		{
			clockhair.transform.position = clockhair.transform.position - lastJitterAmount;
			clockhair.transform.position = GetTargetClockhairPosition(clockhair.transform.position.XY());
			clockhair.sprite.UpdateZDepth();
			bool isTargetingValidTarget = CheckTarget(base.aiActor, clockhairTransform);
			if (isTargetingValidTarget)
			{
				clockhair.SetMotionType(-10f);
			}
			else
			{
				clockhair.SetMotionType(1f);
			}
			shotTargetTime2 = (((!currentInput.ActiveActions.ShootAction.IsPressed && !currentInput.ActiveActions.InteractAction.IsPressed) || !isTargetingValidTarget) ? Mathf.Max(0f, shotTargetTime2 - BraveTime.DeltaTime * 3f) : (shotTargetTime2 + BraveTime.DeltaTime));
			UpdateEyes(clockhair.transform.position, currentInput.ActiveActions.ShootAction.IsPressed || currentInput.ActiveActions.InteractAction.IsPressed);
			if ((currentInput.ActiveActions.ShootAction.WasReleased || currentInput.ActiveActions.InteractAction.WasReleased) && isTargetingValidTarget && shotTargetTime2 > holdDuration && !GameManager.Instance.IsPaused)
			{
				break;
			}
			if (shotTargetTime2 > 0f)
			{
				lastScreenShakeAmount2 = camera.DoFrameScreenShake(Mathf.Lerp(0f, 0.5f, shotTargetTime2 / holdDuration), Mathf.Lerp(3f, 8f, shotTargetTime2 / holdDuration), Vector3.right, lastScreenShakeAmount2, Time.realtimeSinceStartup);
				float distortionPower = Mathf.Lerp(0f, 0.35f, shotTargetTime2 / holdDuration);
				float distortRadius = 0.5f;
				float edgeRadius = Mathf.Lerp(4f, 7f, shotTargetTime2 / holdDuration);
				clockhair.UpdateDistortion(distortionPower, distortRadius, edgeRadius);
				float desatRadiusUV = Mathf.Lerp(2f, 0.25f, shotTargetTime2 / holdDuration);
				clockhair.UpdateDesat(true, desatRadiusUV);
				shotTargetTime2 = Mathf.Min(holdDuration + 0.25f, shotTargetTime2 + BraveTime.DeltaTime);
				float num = Mathf.Lerp(0f, 0.5f, (shotTargetTime2 - 1f) / (holdDuration - 1f));
				Vector3 vector = (Random.insideUnitCircle * num).ToVector3ZUp();
				BraveInput.DoSustainedScreenShakeVibration(shotTargetTime2 / holdDuration * 0.8f);
				clockhair.transform.position = clockhair.transform.position + vector;
				lastJitterAmount = vector;
				clockhair.SetMotionType(Mathf.Lerp(-10f, -2400f, shotTargetTime2 / holdDuration));
			}
			else
			{
				lastJitterAmount = Vector3.zero;
				camera.ClearFrameScreenShake(lastScreenShakeAmount2);
				lastScreenShakeAmount2 = Vector3.zero;
				clockhair.UpdateDistortion(0f, 0f, 0f);
				clockhair.UpdateDesat(false, 0f);
				shotTargetTime2 = 0f;
				BraveInput.DoSustainedScreenShakeVibration(0f);
			}
			yield return null;
		}
		camera.ClearFrameScreenShake(lastScreenShakeAmount2);
		lastScreenShakeAmount2 = Vector3.zero;
		BraveInput.DoSustainedScreenShakeVibration(0f);
		BraveInput.DoVibrationForAllPlayers(Vibration.Time.Quick, Vibration.Strength.Hard);
		clockhair.StartCoroutine(clockhair.WipeoutDistortionAndFade(0.5f));
		clockhair.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
		Pixelator.Instance.FadeToColor(0.2f, Color.white, true, 0.2f);
		Pixelator.Instance.DoRenderGBuffer = false;
		clockhair.spriteAnimator.PlayAndDisableRenderer("clockhair_fire");
		clockhair.hourAnimator.GetComponent<Renderer>().enabled = false;
		clockhair.minuteAnimator.GetComponent<Renderer>().enabled = false;
		clockhair.secondAnimator.GetComponent<Renderer>().enabled = false;
		yield return null;
	}

	private void UpdateEyes(Vector2 clockhairPosition, bool isInDanger)
	{
		Vector2 vector = clockhairPosition - eyePos.transform.position.XY();
		if (isInDanger && vector.magnitude < 7f)
		{
			if (!base.aiAnimator.IsPlaying("clockhair_target"))
			{
				base.aiAnimator.PlayUntilCancelled("clockhair_target");
			}
			return;
		}
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = vector.ToAngle();
		if (Mathf.Abs(vector.x) < 4f && Mathf.Abs(vector.y) < 5f)
		{
			if (vector.y > 2f)
			{
				if (!base.aiAnimator.IsPlaying("clockhair_up"))
				{
					base.aiAnimator.PlayUntilCancelled("clockhair_up");
				}
			}
			else if (vector.y < -2f)
			{
				if (!base.aiAnimator.IsPlaying("clockhair_down"))
				{
					base.aiAnimator.PlayUntilCancelled("clockhair_down");
				}
			}
			else if (!base.aiAnimator.IsPlaying("clockhair_mid"))
			{
				base.aiAnimator.PlayUntilCancelled("clockhair_mid");
			}
		}
		else if (!base.aiAnimator.IsPlaying("clockhair"))
		{
			base.aiAnimator.PlayUntilCancelled("clockhair");
		}
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		SuperReaperController.PreventShooting = true;
		while (base.aiAnimator.IsPlaying("death"))
		{
			yield return null;
		}
		Pixelator.Instance.DoMinimap = false;
		BossKillCam extantCam = Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		Vector2 lichCenter = base.aiAnimator.sprite.WorldCenter;
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		yield return StartCoroutine(HandleClockhair(GameManager.Instance.BestActivePlayer));
		if (GameManager.Instance.PrimaryPlayer != null)
		{
			GameStatsManager.Instance.SetCharacterSpecificFlag(CharacterSpecificGungeonFlags.CLEARED_BULLET_HELL, true);
			if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Eevee && !GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED))
			{
				GameManager.LastUsedPlayerPrefab = (GameObject)BraveResources.Load("PlayerGunslinger");
				QuickRestartOptions opts = new QuickRestartOptions
				{
					ChallengeMode = ChallengeModeType.None,
					GunGame = false,
					BossRush = false,
					NumMetas = 0
				};
				Material glitchPass = new Material(Shader.Find("Brave/Internal/GlitchUnlit"));
				Pixelator.Instance.RegisterAdditionalRenderPass(glitchPass);
				yield return new WaitForSeconds(4f);
				Pixelator.Instance.FadeToBlack(1f);
				yield return new WaitForSeconds(1.25f);
				GameManager.Instance.QuickRestart(opts);
				yield break;
			}
		}
		GameManager.Instance.MainCameraController.DoScreenShake(1.25f, 8f, 0.5f, 0.75f, null);
		GameObject spawnedExplosion = SpawnManager.SpawnVFX(finalExplosionVfx, base.specRigidbody.HitboxPixelCollider.UnitCenter + new Vector2(-0.25f, -0.25f), Quaternion.identity);
		tk2dBaseSprite explosionSprite = spawnedExplosion.GetComponent<tk2dSprite>();
		explosionSprite.HeightOffGround = 0.8f;
		base.sprite.AttachRenderer(explosionSprite);
		base.sprite.UpdateZDepth();
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH)
		{
			yield return null;
			Object.Destroy(base.gameObject);
			GameManager.Instance.MainCameraController.SetManualControl(true);
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHERPA_SUPERBOSSRUSH_COMPLETE, true);
			GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_SUPERBOSSRUSH, true);
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY, 10f);
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameManager.Instance.AllPlayers[i].SetInputOverride("game complete");
			}
			Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
			AmmonomiconController.Instance.OpenAmmonomicon(true, true);
			yield break;
		}
		Object.Destroy(base.gameObject);
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		if (GameManager.Instance.PrimaryPlayer.IsTemporaryEeveeForUnlock)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.FLAG_EEVEE_UNLOCKED, true);
		}
		for (int j = 0; j < StaticReferenceManager.AllDebris.Count; j++)
		{
			if ((bool)StaticReferenceManager.AllDebris[j])
			{
				Vector2 flatPoint = StaticReferenceManager.AllDebris[j].transform.position.XY();
				if (GameManager.Instance.MainCameraController.PointIsVisible(flatPoint))
				{
					StaticReferenceManager.AllDebris[j].TriggerDestruction();
				}
			}
		}
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.25f);
		yield return new WaitForSeconds(0.4f);
		yield return GameManager.Instance.StartCoroutine(ttcc.HandleTimeTubeCredits(lichCenter, false, null, -1, true));
		ttcc.CleanupLich();
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		BraveCameraUtility.OverrideAspect = 1.77777779f;
		yield return GameManager.Instance.StartCoroutine(HandlePastBeingShot());
	}

	private IEnumerator HandlePastBeingShot()
	{
		Minimap.Instance.TemporarilyPreventMinimap = true;
		Pixelator.Instance.LerpToLetterbox(1f, 2.5f);
		yield return new WaitForSeconds(3f);
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true);
		AkSoundEngine.PostEvent("Play_ENV_final_flash_01", GameManager.Instance.gameObject);
		yield return new WaitForSeconds(0.15f);
		yield return null;
		Pixelator.Instance.FadeToColor(0.25f, Color.white, true, 0.25f);
		TitleDioramaController tdc = Object.FindObjectOfType<TitleDioramaController>();
		yield return new WaitForSeconds(1.25f);
		if (tdc.VFX_BulletImpact != null)
		{
			tdc.VFX_BulletImpact.SetActive(true);
			tdc.VFX_BulletImpact.GetComponent<tk2dSpriteAnimator>().PlayAndDisableObject(string.Empty);
			tdc.VFX_BulletImpact.GetComponent<tk2dSprite>().UpdateZDepth();
		}
		if (tdc.VFX_TrailParticles != null)
		{
			tdc.VFX_TrailParticles.SetActive(true);
			BraveUtility.EnableEmission(tdc.VFX_TrailParticles.GetComponent<ParticleSystem>(), true);
		}
		AkSoundEngine.PostEvent("Play_ENV_final_shot_01", GameManager.Instance.gameObject);
		tdc.PastIslandSprite.SetSprite("marsh_of_gungeon_past_hit_001");
		yield return new WaitForSeconds(1.25f);
		if (tdc.VFX_Splash != null)
		{
			yield return GameManager.Instance.StartCoroutine(HandleSplashBody(GameManager.Instance.PrimaryPlayer, true, tdc));
		}
		yield return new WaitForSeconds(2f);
		float elapsed = 0f;
		float duration = 10f;
		Transform targetXform = tdc.PastIslandSprite.transform.parent;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			tdc.SkyRenderer.material.SetFloat("_SkyBoost", Mathf.Lerp(0.88f, 0f, t));
			tdc.SkyRenderer.material.SetColor("_OverrideColor", Color.Lerp(new Color(1f, 0.55f, 0.2f, 1f), new Color(0.05f, 0.08f, 0.15f, 1f), t));
			tdc.SkyRenderer.material.SetFloat("_CurvePower", Mathf.Lerp(0.3f, -0.25f, t));
			tdc.SkyRenderer.material.SetFloat("_DitherCohesionFactor", Mathf.Lerp(0.3f, 1f, t));
			tdc.SkyRenderer.material.SetFloat("_StepValue", Mathf.Lerp(0.2f, 0.01f, t));
			targetXform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0f, -60f, 0f), BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t));
			yield return null;
		}
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private IEnumerator HandleSplashBody(PlayerController sourcePlayer, bool isPrimaryPlayer, TitleDioramaController diorama)
	{
		AkSoundEngine.PostEvent("Play_CHR_forever_fall_01", GameManager.Instance.gameObject);
		if (!sourcePlayer.healthHaver.IsDead)
		{
			GameObject timefallCorpseInstance = (GameObject)Object.Instantiate(BraveResources.Load("Global Prefabs/TimefallCorpse"), sourcePlayer.sprite.transform.position, Quaternion.identity);
			timefallCorpseInstance.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
			tk2dSpriteAnimator targetTimefallAnimator = timefallCorpseInstance.GetComponent<tk2dSpriteAnimator>();
			SpriteOutlineManager.AddOutlineToSprite(targetTimefallAnimator.Sprite, Color.black);
			tk2dSpriteAnimation timefallSpecificLibrary = (targetTimefallAnimator.Library = (targetTimefallAnimator.Library = ((!(sourcePlayer is PlayerSpaceshipController)) ? sourcePlayer.sprite.spriteAnimator.Library : (sourcePlayer as PlayerSpaceshipController).TimefallCorpseLibrary)));
			tk2dSpriteAnimationClip clip = targetTimefallAnimator.GetClipByName("timefall");
			if (clip != null)
			{
				targetTimefallAnimator.Play("timefall");
			}
			float elapsed = 0f;
			float duration = 3f;
			while (elapsed < duration)
			{
				elapsed += BraveTime.DeltaTime;
				Vector3 startPoint = diorama.VFX_Splash.transform.position + new Vector3(-8f, 40f, 0f);
				Vector3 endPoint = diorama.VFX_Splash.GetComponent<tk2dBaseSprite>().WorldCenter.ToVector3ZUp(startPoint.z);
				targetTimefallAnimator.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(elapsed / duration));
				timefallCorpseInstance.transform.localScale = Vector3.Lerp(Vector3.one * 1.25f, new Vector3(0.125f, 0.125f, 0.125f), Mathf.Clamp01(elapsed / duration));
				yield return null;
			}
			AkSoundEngine.PostEvent("Play_CHR_final_splash_01", GameManager.Instance.gameObject);
			diorama.VFX_Splash.SetActive(true);
			diorama.VFX_Splash.GetComponent<tk2dSpriteAnimator>().PlayAndDisableObject(string.Empty);
			diorama.VFX_Splash.GetComponent<tk2dSprite>().UpdateZDepth();
			Object.Destroy(timefallCorpseInstance);
		}
	}
}
