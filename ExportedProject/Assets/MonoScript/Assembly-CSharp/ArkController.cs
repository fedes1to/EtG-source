using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ArkController : BraveBehaviour, IPlayerInteractable
{
	public tk2dSpriteAnimator LidAnimator;

	public tk2dSpriteAnimator ChestAnimator;

	public tk2dSpriteAnimator PoofAnimator;

	public tk2dSprite LightSpriteBeam;

	public tk2dSprite HellCrackSprite;

	public Transform GunSpawnPoint;

	public GameObject GunPrefab;

	public GameObject HeldGunPrefab;

	public List<Transform> ParallaxTransforms;

	public List<float> ParallaxFractions;

	[NonSerialized]
	private List<Vector3> ParallaxStartingPositions = new List<Vector3>();

	[NonSerialized]
	private List<DFGentleBob> Bobbers = new List<DFGentleBob>();

	[NonSerialized]
	private RoomHandler m_parentRoom;

	[NonSerialized]
	private Transform m_heldPastGun;

	private bool m_hasBeenInteracted;

	protected bool m_isLocalPointing;

	public static bool IsResettingPlayers;

	private IEnumerator Start()
	{
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		for (int i = 0; i < ParallaxTransforms.Count; i++)
		{
			ParallaxStartingPositions.Add(ParallaxTransforms[i].position);
			Bobbers.Add(ParallaxTransforms[i].GetComponent<DFGentleBob>());
		}
		yield return null;
		RoomHandler.unassignedInteractableObjects.Add(this);
	}

	private void Update()
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			return;
		}
		float num = m_parentRoom.area.basePosition.y;
		float num2 = m_parentRoom.area.basePosition.y + m_parentRoom.area.dimensions.y;
		float num3 = num2 - num;
		float x = GameManager.Instance.MainCameraController.transform.position.x;
		float y = GameManager.Instance.MainCameraController.transform.position.y;
		for (int i = 0; i < ParallaxTransforms.Count; i++)
		{
			float num4 = num3 * ParallaxFractions[i];
			float num5 = y - ParallaxStartingPositions[i].y;
			float num6 = x - ParallaxStartingPositions[i].x;
			float num7 = Mathf.Clamp(num5 / num3, -1f, 1f);
			float num8 = Mathf.Clamp(num6 / num3, -1f, 1f);
			float y2 = ParallaxStartingPositions[i].y + num7 * num4;
			float x2 = ParallaxStartingPositions[i].x + num8 * num4;
			Vector3 vector = ParallaxStartingPositions[i].WithY(y2).WithX(x2);
			if (Bobbers[i] != null)
			{
				Bobbers[i].AbsoluteStartPosition = vector;
			}
			else
			{
				ParallaxTransforms[i].position = vector;
			}
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (m_hasBeenInteracted)
		{
			return 100000f;
		}
		return Vector2.Distance(point, base.specRigidbody.UnitCenter) / 2f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		SpriteOutlineManager.AddOutlineToSprite(LidAnimator.sprite, Color.white);
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		SpriteOutlineManager.RemoveOutlineFromSprite(LidAnimator.sprite, true);
	}

	public void Interact(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		SpriteOutlineManager.RemoveOutlineFromSprite(LidAnimator.sprite);
		if (!m_hasBeenInteracted)
		{
			m_hasBeenInteracted = true;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].RemoveBrokenInteractable(this);
		}
		BraveInput.DoVibrationForAllPlayers(Vibration.Time.Normal, Vibration.Strength.Medium);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(interactor);
			float num = Vector2.Distance(otherPlayer.CenterPosition, interactor.CenterPosition);
			if (num > 8f || num < 0.75f)
			{
				Vector2 vector = Vector2.right;
				if (interactor.CenterPosition.x < ChestAnimator.sprite.WorldCenter.x)
				{
					vector = Vector2.left;
				}
				otherPlayer.WarpToPoint(otherPlayer.transform.position.XY() + vector * 2f, true);
			}
		}
		StartCoroutine(Open(interactor));
	}

	private IEnumerator HandleLightSprite()
	{
		yield return new WaitForSeconds(0.5f);
		float elapsed = 0f;
		float duration = 1f;
		LightSpriteBeam.renderer.enabled = true;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			LightSpriteBeam.transform.localScale = new Vector3(1f, Mathf.Lerp(0f, 1f, t), 1f);
			LightSpriteBeam.transform.localPosition = new Vector3(0f, Mathf.Lerp(1.375f, 0f, t), 0f);
			LightSpriteBeam.UpdateZDepth();
			yield return null;
		}
	}

	private IEnumerator Open(PlayerController interactor)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (GameManager.Instance.AllPlayers[i].healthHaver.IsAlive)
			{
				GameManager.Instance.AllPlayers[i].SetInputOverride("ark");
			}
		}
		LidAnimator.Play();
		ChestAnimator.Play();
		PoofAnimator.PlayAndDisableObject(string.Empty);
		base.specRigidbody.Reinitialize();
		GameManager.Instance.MainCameraController.OverrideRecoverySpeed = 2f;
		GameManager.Instance.MainCameraController.OverridePosition = ChestAnimator.sprite.WorldCenter + new Vector2(0f, 2f);
		GameManager.Instance.MainCameraController.SetManualControl(true);
		StartCoroutine(HandleLightSprite());
		while (LidAnimator.IsPlaying(LidAnimator.CurrentClip))
		{
			yield return null;
		}
		yield return StartCoroutine(HandleGun(interactor));
		yield return new WaitForSeconds(0.5f);
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		yield return StartCoroutine(HandleClockhair(interactor));
		interactor.ClearInputOverride("ark");
	}

	private Vector2 GetTargetClockhairPosition(BraveInput input, Vector2 currentClockhairPosition)
	{
		Vector2 rhs2 = Vector2.Max(rhs: (!input.IsKeyboardAndMouse()) ? (currentClockhairPosition + input.ActiveActions.Aim.Vector * 10f * BraveTime.DeltaTime) : (GameManager.Instance.MainCameraController.Camera.ScreenToWorldPoint(Input.mousePosition).XY() + new Vector2(0.375f, -0.25f)), lhs: GameManager.Instance.MainCameraController.MinVisiblePoint);
		return Vector2.Min(GameManager.Instance.MainCameraController.MaxVisiblePoint, rhs2);
	}

	private void UpdateCameraPositionDuringClockhair(Vector2 targetPosition)
	{
		float num = Vector2.Distance(targetPosition, ChestAnimator.sprite.WorldCenter);
		if (num > 8f)
		{
			targetPosition = ChestAnimator.sprite.WorldCenter;
		}
		Vector2 vector = GameManager.Instance.MainCameraController.OverridePosition;
		if (Vector2.Distance(vector, targetPosition) > 10f)
		{
			vector = GameManager.Instance.MainCameraController.transform.position.XY();
		}
		GameManager.Instance.MainCameraController.OverridePosition = Vector3.MoveTowards(vector, targetPosition, BraveTime.DeltaTime);
	}

	private bool CheckPlayerTarget(PlayerController target, Transform clockhairTransform)
	{
		Vector2 a = clockhairTransform.position.XY() + new Vector2(-0.375f, 0.25f);
		return Vector2.Distance(a, target.CenterPosition) < 0.625f;
	}

	private bool CheckHellTarget(tk2dBaseSprite hellTarget, Transform clockhairTransform)
	{
		if (hellTarget == null)
		{
			return false;
		}
		Vector2 a = clockhairTransform.position.XY() + new Vector2(-0.375f, 0.25f);
		return Vector2.Distance(a, hellTarget.WorldCenter) < 0.625f;
	}

	public void HandleHeldGunSpriteFlip(bool flipped)
	{
		tk2dSprite component = m_heldPastGun.GetComponent<tk2dSprite>();
		if (flipped)
		{
			if (!component.FlipY)
			{
				component.FlipY = true;
			}
		}
		else if (component.FlipY)
		{
			component.FlipY = false;
		}
		Transform transform = m_heldPastGun.Find("PrimaryHand");
		m_heldPastGun.localPosition = -transform.localPosition;
		if (flipped)
		{
			m_heldPastGun.localPosition = Vector3.Scale(m_heldPastGun.localPosition, new Vector3(1f, -1f, 1f));
		}
		m_heldPastGun.localPosition = BraveUtility.QuantizeVector(m_heldPastGun.localPosition, 16f);
		component.ForceRotationRebuild();
		component.UpdateZDepth();
	}

	private void PointGunAtClockhair(PlayerController interactor, Transform clockhairTransform)
	{
		Vector2 centerPosition = interactor.CenterPosition;
		Vector2 vector = clockhairTransform.position.XY() - centerPosition;
		if (m_isLocalPointing && vector.sqrMagnitude > 9f)
		{
			m_isLocalPointing = false;
		}
		else if (m_isLocalPointing || vector.sqrMagnitude < 4f)
		{
			m_isLocalPointing = true;
			float t = vector.sqrMagnitude / 4f - 0.05f;
			vector = Vector2.Lerp(Vector2.right, vector, t);
		}
		float value = BraveMathCollege.Atan2Degrees(vector);
		value = value.Quantize(3f);
		interactor.GunPivot.rotation = Quaternion.Euler(0f, 0f, value);
		interactor.ForceIdleFacePoint(vector, false);
		HandleHeldGunSpriteFlip(interactor.SpriteFlipped);
	}

	private IEnumerator HandleClockhair(PlayerController interactor)
	{
		Transform clockhairTransform = ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Clockhair"))).transform;
		ClockhairController clockhair = clockhairTransform.GetComponent<ClockhairController>();
		float elapsed = 0f;
		float duration = clockhair.ClockhairInDuration;
		Vector3 clockhairTargetPosition = interactor.CenterPosition;
		Vector3 clockhairStartPosition = clockhairTargetPosition + new Vector3(-20f, 5f, 0f);
		clockhair.renderer.enabled = true;
		clockhair.spriteAnimator.alwaysUpdateOffscreen = true;
		clockhair.spriteAnimator.Play("clockhair_intro");
		clockhair.hourAnimator.Play("hour_hand_intro");
		clockhair.minuteAnimator.Play("minute_hand_intro");
		clockhair.secondAnimator.Play("second_hand_intro");
		BraveInput currentInput = BraveInput.GetInstanceForPlayer(interactor.PlayerIDX);
		while (elapsed < duration)
		{
			UpdateCameraPositionDuringClockhair(interactor.CenterPosition);
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / duration;
			float smoothT = Mathf.SmoothStep(0f, 1f, t);
			clockhairTargetPosition = GetTargetClockhairPosition(currentInput, clockhairTargetPosition);
			Vector3 currentPosition = Vector3.Slerp(clockhairStartPosition, clockhairTargetPosition, smoothT);
			clockhairTransform.position = currentPosition.WithZ(0f);
			if (t > 0.5f)
			{
				clockhair.renderer.enabled = true;
			}
			if (t > 0.75f)
			{
				clockhair.hourAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.minuteAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.secondAnimator.GetComponent<Renderer>().enabled = true;
				GameCursorController.CursorOverride.SetOverride("ark", true);
			}
			clockhair.sprite.UpdateZDepth();
			PointGunAtClockhair(interactor, clockhairTransform);
			yield return null;
		}
		clockhair.SetMotionType(1f);
		float shotTargetTime = 0f;
		float holdDuration = 4f;
		PlayerController shotPlayer = null;
		bool didShootHellTrigger = false;
		Vector3 lastJitterAmount = Vector3.zero;
		bool m_isPlayingChargeAudio = false;
		while (true)
		{
			UpdateCameraPositionDuringClockhair(interactor.CenterPosition);
			clockhair.transform.position = clockhair.transform.position - lastJitterAmount;
			clockhair.transform.position = GetTargetClockhairPosition(currentInput, clockhair.transform.position.XY());
			clockhair.sprite.UpdateZDepth();
			bool isTargetingValidTarget = CheckPlayerTarget(GameManager.Instance.PrimaryPlayer, clockhairTransform);
			shotPlayer = GameManager.Instance.PrimaryPlayer;
			if (!isTargetingValidTarget && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				isTargetingValidTarget = CheckPlayerTarget(GameManager.Instance.SecondaryPlayer, clockhairTransform);
				shotPlayer = GameManager.Instance.SecondaryPlayer;
			}
			if (!isTargetingValidTarget && GameStatsManager.Instance.AllCorePastsBeaten())
			{
				isTargetingValidTarget = CheckHellTarget(HellCrackSprite, clockhairTransform);
				didShootHellTrigger = isTargetingValidTarget;
			}
			if (isTargetingValidTarget)
			{
				clockhair.SetMotionType(-10f);
			}
			else
			{
				clockhair.SetMotionType(1f);
			}
			if ((currentInput.ActiveActions.ShootAction.IsPressed || currentInput.ActiveActions.InteractAction.IsPressed) && isTargetingValidTarget)
			{
				if (!m_isPlayingChargeAudio)
				{
					m_isPlayingChargeAudio = true;
					AkSoundEngine.PostEvent("Play_OBJ_pastkiller_charge_01", base.gameObject);
				}
				shotTargetTime += BraveTime.DeltaTime;
			}
			else
			{
				shotTargetTime = Mathf.Max(0f, shotTargetTime - BraveTime.DeltaTime * 3f);
				if (m_isPlayingChargeAudio)
				{
					m_isPlayingChargeAudio = false;
					AkSoundEngine.PostEvent("Stop_OBJ_pastkiller_charge_01", base.gameObject);
				}
			}
			if ((currentInput.ActiveActions.ShootAction.WasReleased || currentInput.ActiveActions.InteractAction.WasReleased) && isTargetingValidTarget && shotTargetTime > holdDuration && !GameManager.Instance.IsPaused)
			{
				break;
			}
			if (shotTargetTime > 0f)
			{
				float distortionPower = Mathf.Lerp(0f, 0.35f, shotTargetTime / holdDuration);
				float distortRadius = 0.5f;
				float edgeRadius = Mathf.Lerp(4f, 7f, shotTargetTime / holdDuration);
				clockhair.UpdateDistortion(distortionPower, distortRadius, edgeRadius);
				float desatRadiusUV = Mathf.Lerp(2f, 0.25f, shotTargetTime / holdDuration);
				clockhair.UpdateDesat(true, desatRadiusUV);
				shotTargetTime = Mathf.Min(holdDuration + 0.25f, shotTargetTime + BraveTime.DeltaTime);
				float num = Mathf.Lerp(0f, 0.5f, (shotTargetTime - 1f) / (holdDuration - 1f));
				Vector3 vector = (UnityEngine.Random.insideUnitCircle * num).ToVector3ZUp();
				BraveInput.DoSustainedScreenShakeVibration(shotTargetTime / holdDuration * 0.8f);
				clockhair.transform.position = clockhair.transform.position + vector;
				lastJitterAmount = vector;
				clockhair.SetMotionType(Mathf.Lerp(-10f, -2400f, shotTargetTime / holdDuration));
			}
			else
			{
				lastJitterAmount = Vector3.zero;
				clockhair.UpdateDistortion(0f, 0f, 0f);
				clockhair.UpdateDesat(false, 0f);
				shotTargetTime = 0f;
				BraveInput.DoSustainedScreenShakeVibration(0f);
			}
			PointGunAtClockhair(interactor, clockhairTransform);
			yield return null;
		}
		BraveInput.DoSustainedScreenShakeVibration(0f);
		BraveInput.DoVibrationForAllPlayers(Vibration.Time.Normal, Vibration.Strength.Hard);
		clockhair.StartCoroutine(clockhair.WipeoutDistortionAndFade(0.5f));
		clockhair.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
		Pixelator.Instance.FadeToColor(1f, Color.white, true, 0.2f);
		Pixelator.Instance.DoRenderGBuffer = false;
		clockhair.spriteAnimator.Play("clockhair_fire");
		clockhair.hourAnimator.GetComponent<Renderer>().enabled = false;
		clockhair.minuteAnimator.GetComponent<Renderer>().enabled = false;
		clockhair.secondAnimator.GetComponent<Renderer>().enabled = false;
		yield return null;
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		bool isShortTunnel = didShootHellTrigger || shotPlayer.characterIdentity == PlayableCharacters.CoopCultist || CharacterStoryComplete(shotPlayer.characterIdentity);
		UnityEngine.Object.Destroy(m_heldPastGun.gameObject);
		interactor.ToggleGunRenderers(true, "ark");
		GameCursorController.CursorOverride.RemoveOverride("ark");
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.25f);
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(clockhair.sprite.WorldCenter, isShortTunnel, clockhair.spriteAnimator, (!didShootHellTrigger) ? shotPlayer.PlayerIDX : 0));
		if (isShortTunnel)
		{
			Pixelator.Instance.FadeToBlack(1f);
			yield return new WaitForSeconds(1f);
		}
		if (didShootHellTrigger)
		{
			GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.HELLGEON);
			GameManager.Instance.LoadCustomLevel("tt_bullethell");
		}
		else if (shotPlayer.characterIdentity == PlayableCharacters.CoopCultist)
		{
			GameManager.IsCoopPast = true;
			ResetPlayers();
			GameManager.Instance.LoadCustomLevel("fs_coop");
		}
		else if (CharacterStoryComplete(shotPlayer.characterIdentity) && shotPlayer.characterIdentity == PlayableCharacters.Gunslinger)
		{
			GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.FINALGEON);
			GameManager.IsGunslingerPast = true;
			ResetPlayers(true);
			GameManager.Instance.LoadCustomLevel("tt_bullethell");
		}
		else if (CharacterStoryComplete(shotPlayer.characterIdentity))
		{
			bool flag = false;
			GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.FINALGEON);
			switch (shotPlayer.characterIdentity)
			{
			case PlayableCharacters.Convict:
				flag = true;
				ResetPlayers();
				GameManager.Instance.LoadCustomLevel("fs_convict");
				break;
			case PlayableCharacters.Pilot:
				flag = true;
				ResetPlayers();
				GameManager.Instance.LoadCustomLevel("fs_pilot");
				break;
			case PlayableCharacters.Guide:
				flag = true;
				ResetPlayers();
				GameManager.Instance.LoadCustomLevel("fs_guide");
				break;
			case PlayableCharacters.Soldier:
				flag = true;
				ResetPlayers();
				GameManager.Instance.LoadCustomLevel("fs_soldier");
				break;
			case PlayableCharacters.Robot:
				flag = true;
				ResetPlayers();
				GameManager.Instance.LoadCustomLevel("fs_robot");
				break;
			case PlayableCharacters.Bullet:
				flag = true;
				ResetPlayers();
				GameManager.Instance.LoadCustomLevel("fs_bullet");
				break;
			}
			if (!flag)
			{
				AmmonomiconController.Instance.OpenAmmonomicon(true, true);
			}
			else
			{
				GameUIRoot.Instance.ToggleUICamera(false);
			}
		}
		else
		{
			AmmonomiconController.Instance.OpenAmmonomicon(true, true);
		}
		while (true)
		{
			yield return null;
		}
	}

	private void ResetPlayers(bool isGunslingerPast = false)
	{
		IsResettingPlayers = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (GameManager.Instance.AllPlayers[i].healthHaver.IsAlive)
			{
				if (!isGunslingerPast)
				{
					GameManager.Instance.AllPlayers[i].ResetToFactorySettings(true, true);
				}
				if (!isGunslingerPast)
				{
					GameManager.Instance.AllPlayers[i].CharacterUsesRandomGuns = false;
				}
				GameManager.Instance.AllPlayers[i].IsVisible = true;
				GameManager.Instance.AllPlayers[i].ClearInputOverride("ark");
				GameManager.Instance.AllPlayers[i].ClearAllInputOverrides();
			}
		}
		IsResettingPlayers = false;
	}

	private void DestroyPlayers()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			UnityEngine.Object.Destroy(GameManager.Instance.AllPlayers[i].gameObject);
		}
	}

	private bool CharacterStoryComplete(PlayableCharacters shotCharacter)
	{
		return GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_BULLET_COMPLETE) && GameManager.Instance.PrimaryPlayer.PastAccessible;
	}

	private void SpawnVFX(string vfxResourcePath, Vector2 pos)
	{
		GameObject original = (GameObject)BraveResources.Load(vfxResourcePath, typeof(GameObject));
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.PlaceAtPositionByAnchor(pos, tk2dBaseSprite.Anchor.MiddleCenter);
		component.UpdateZDepth();
	}

	private IEnumerator HandleGun(PlayerController interactor)
	{
		interactor.ToggleGunRenderers(false, "ark");
		GameObject instanceGun = UnityEngine.Object.Instantiate(GunPrefab, GunSpawnPoint.position, Quaternion.identity);
		Material gunMaterial = instanceGun.transform.Find("GunThatCanKillThePast").GetComponent<MeshRenderer>().sharedMaterial;
		tk2dSprite instanceGunSprite = instanceGun.transform.Find("GunThatCanKillThePast").GetComponent<tk2dSprite>();
		instanceGunSprite.HeightOffGround = 5f;
		gunMaterial.SetColor("_OverrideColor", Color.white);
		float elapsed3 = 0f;
		float raiseTime = 4f;
		Vector3 targetMidHeightPosition = GunSpawnPoint.position + new Vector3(0f, 6.5f, 0f);
		interactor.ForceIdleFacePoint(new Vector2(1f, -1f), false);
		while (elapsed3 < raiseTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			float t2 = Mathf.Clamp01(elapsed3 / raiseTime);
			t2 = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t2);
			instanceGun.transform.position = Vector3.Lerp(GunSpawnPoint.position, targetMidHeightPosition, t2);
			instanceGun.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 2f, t2);
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		while (instanceGunSprite.spriteAnimator.CurrentFrame != 0)
		{
			yield return null;
		}
		instanceGunSprite.spriteAnimator.Pause();
		Pixelator.Instance.FadeToColor(0.2f, Color.white, true, 0.2f);
		yield return new WaitForSeconds(0.1f);
		Transform burstObject = instanceGun.transform.Find("GTCKTP_Burst");
		if (burstObject != null)
		{
			burstObject.gameObject.SetActive(true);
		}
		BraveInput.DoVibrationForAllPlayers(Vibration.Time.Slow, Vibration.Strength.Medium);
		yield return new WaitForSeconds(0.2f);
		instanceGunSprite.spriteAnimator.Resume();
		elapsed3 = 0f;
		float fadeTime = 1f;
		while (elapsed3 < fadeTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			gunMaterial.SetColor("_OverrideColor", Color.Lerp(t: Mathf.Clamp01(elapsed3 / fadeTime), a: Color.white, b: new Color(1f, 1f, 1f, 0f)));
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		elapsed3 = 0f;
		float reraiseTime = 2f;
		while (elapsed3 < reraiseTime)
		{
			elapsed3 += BraveTime.DeltaTime;
			float t4 = Mathf.Clamp01(elapsed3 / reraiseTime);
			t4 = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t4);
			instanceGun.transform.position = Vector3.Lerp(targetMidHeightPosition, interactor.CenterPosition.ToVector3ZUp(targetMidHeightPosition.z - 10f), t4);
			instanceGun.transform.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, t4);
			yield return null;
		}
		GameObject pickupVFXPrefab = ResourceCache.Acquire("Global VFX/VFX_Item_Pickup") as GameObject;
		interactor.PlayEffectOnActor(pickupVFXPrefab, Vector3.zero);
		GameObject instanceEquippedGun = UnityEngine.Object.Instantiate(HeldGunPrefab);
		AkSoundEngine.PostEvent("Play_OBJ_weapon_pickup_01", base.gameObject);
		tk2dSprite instanceEquippedSprite = instanceEquippedGun.GetComponent<tk2dSprite>();
		instanceEquippedSprite.HeightOffGround = 2f;
		instanceEquippedSprite.attachParent = interactor.sprite;
		m_heldPastGun = instanceEquippedGun.transform;
		m_heldPastGun.parent = interactor.GunPivot;
		Transform primaryHandXform = m_heldPastGun.Find("PrimaryHand");
		m_heldPastGun.localRotation = Quaternion.identity;
		m_heldPastGun.localPosition = -primaryHandXform.localPosition;
		instanceEquippedSprite.UpdateZDepth();
		UnityEngine.Object.Destroy(instanceGun);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
