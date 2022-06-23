using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class GatlingGullIntroDoer : TimeInvariantMonoBehaviour, IPlaceConfigurable
{
	public float initialDelay = 1f;

	public float cameraMoveSpeed = 5f;

	public PortraitSlideSettings portraitSlideSettings;

	public ScreenShakeSettings landingShakeSettings;

	public ScreenShakeSettings featherShakeSettings;

	[HideInInspector]
	public tk2dSpriteAnimator gullAnimator;

	public GameObject feathersVFX;

	public GameObject feathersDebris;

	public int numFeathersToSpawn = 15;

	protected bool m_isRunning;

	protected List<tk2dSpriteAnimator> m_animators = new List<tk2dSpriteAnimator>();

	protected float m_elapsedFrameTime;

	protected CameraController m_camera;

	protected Transform m_cameraTransform;

	protected List<CutsceneMotion> activeMotions = new List<CutsceneMotion>();

	protected RoomHandler m_room;

	protected Transform m_shadowTransform;

	protected tk2dSpriteAnimator m_shadowAnimator;

	protected int m_currentPhase;

	protected bool m_phaseComplete = true;

	protected bool m_hasSkipped;

	protected float m_phaseCountdown;

	protected GameObject gunObject;

	protected ParticleSystem feathersSystem;

	protected GameUIBossHealthController bossUI;

	private bool m_hasTriggeredWalkIn;

	private bool m_waitingForBossCard;

	private Vector2[] m_idealStartingPositions;

	private bool m_hasCoopTeleported;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		m_room.Entered += TriggerSequence;
	}

	private void HandlePlayerWalkIn(PlayerController leadPlayer)
	{
		if (m_hasTriggeredWalkIn)
		{
			return;
		}
		m_hasTriggeredWalkIn = true;
		m_hasCoopTeleported = false;
		RoomHandler roomHandler = null;
		for (int i = 0; i < m_room.connectedRooms.Count; i++)
		{
			if (m_room.connectedRooms[i].distanceFromEntrance <= m_room.distanceFromEntrance)
			{
				roomHandler = m_room.connectedRooms[i];
				break;
			}
		}
		if (roomHandler == null)
		{
			return;
		}
		RuntimeExitDefinition exitDefinitionForConnectedRoom = m_room.GetExitDefinitionForConnectedRoom(roomHandler);
		DungeonData.Direction exitDirection = exitDefinitionForConnectedRoom.upstreamExit.referencedExit.exitDirection;
		IntVector2 intVector = exitDefinitionForConnectedRoom.upstreamExit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exitDefinitionForConnectedRoom.upstreamExit.referencedExit.exitDirection);
		intVector += exitDefinitionForConnectedRoom.upstreamRoom.area.basePosition;
		float num = ((exitDirection != 0 && exitDirection != DungeonData.Direction.SOUTH) ? intVector.x : intVector.y);
		num = ((exitDirection != DungeonData.Direction.EAST && exitDirection != 0) ? (num - (float)(exitDefinitionForConnectedRoom.upstreamExit.TotalExitLength + exitDefinitionForConnectedRoom.downstreamExit.TotalExitLength)) : (num + (float)(exitDefinitionForConnectedRoom.upstreamExit.TotalExitLength + exitDefinitionForConnectedRoom.downstreamExit.TotalExitLength)));
		leadPlayer.ForceWalkInDirectionWhilePaused(exitDirection, num);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(leadPlayer);
			float num2 = ((exitDirection != 0 && exitDirection != DungeonData.Direction.SOUTH) ? Mathf.Abs(num - leadPlayer.CenterPosition.x) : Mathf.Abs(num - leadPlayer.CenterPosition.y));
			IntVector2 pixelsToMove = IntVector2.Zero;
			int num3 = Mathf.RoundToInt(num2 * 16f);
			switch (exitDirection)
			{
			case DungeonData.Direction.NORTH:
				pixelsToMove = new IntVector2(0, num3);
				break;
			case DungeonData.Direction.EAST:
				pixelsToMove = new IntVector2(num3, 0);
				break;
			case DungeonData.Direction.SOUTH:
				pixelsToMove = new IntVector2(0, -num3);
				break;
			case DungeonData.Direction.WEST:
				pixelsToMove = new IntVector2(-num3, 0);
				break;
			}
			CollisionData result;
			if (PhysicsEngine.Instance.RigidbodyCast(otherPlayer.specRigidbody, pixelsToMove, out result))
			{
				num2 = PhysicsEngine.PixelToUnit(result.NewPixelsToMove).magnitude;
			}
			CollisionData.Pool.Free(ref result);
			switch (exitDirection)
			{
			case DungeonData.Direction.NORTH:
				num = otherPlayer.CenterPosition.y + num2;
				break;
			case DungeonData.Direction.EAST:
				num = otherPlayer.CenterPosition.x + num2;
				break;
			case DungeonData.Direction.SOUTH:
				num = otherPlayer.CenterPosition.y - num2;
				break;
			case DungeonData.Direction.WEST:
				num = otherPlayer.CenterPosition.x - num2;
				break;
			}
			otherPlayer.ForceWalkInDirectionWhilePaused(exitDirection, num);
			m_idealStartingPositions = new Vector2[2];
			IntVector2 intVector2 = ((exitDirection != 0 && exitDirection != DungeonData.Direction.SOUTH) ? (intVector + IntVector2.Up) : (intVector + IntVector2.Right));
			float num4 = exitDefinitionForConnectedRoom.upstreamExit.TotalExitLength + exitDefinitionForConnectedRoom.downstreamExit.TotalExitLength;
			switch (exitDirection)
			{
			case DungeonData.Direction.NORTH:
				m_idealStartingPositions[0] = intVector2.ToVector2() + new Vector2(-0.5f, 0f) + new Vector2(0f, num4 + 0.5f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(0.25f, -0.25f) + new Vector2(0f, num4 - 0.25f);
				break;
			case DungeonData.Direction.EAST:
				m_idealStartingPositions[0] = intVector2.ToVector2() + new Vector2(num4 + 0.5f, 0f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(-0.25f, -1f) + new Vector2(num4 - 0.25f, 0f);
				break;
			case DungeonData.Direction.SOUTH:
				m_idealStartingPositions[0] = intVector2.ToVector2() + new Vector2(-0.5f, 0f) - new Vector2(0f, num4 + 0.5f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(0.25f, 0.25f) - new Vector2(0f, num4 - 0.25f);
				break;
			case DungeonData.Direction.WEST:
				m_idealStartingPositions[0] = intVector2.ToVector2() - new Vector2(num4 + 0.5f, 0f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(0.25f, -1f) - new Vector2(num4 - 0.25f, 0f);
				break;
			case DungeonData.Direction.NORTHEAST:
			case DungeonData.Direction.SOUTHEAST:
			case DungeonData.Direction.SOUTHWEST:
				break;
			}
		}
	}

	public void TriggerSequence(PlayerController enterer)
	{
		GameManager.Instance.StartCoroutine(FrameDelayedTriggerSequence(enterer));
	}

	public IEnumerator FrameDelayedTriggerSequence(PlayerController enterer)
	{
		if (!base.enabled || GameManager.Instance.PreventPausing)
		{
			yield break;
		}
		m_room.Entered -= TriggerSequence;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			if (enterer.IsPrimaryPlayer)
			{
				if (!GameManager.Instance.SecondaryPlayer.healthHaver.IsDead)
				{
					GameManager.Instance.SecondaryPlayer.ReuniteWithOtherPlayer(enterer);
				}
			}
			else if (!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead)
			{
				GameManager.Instance.PrimaryPlayer.ReuniteWithOtherPlayer(enterer);
			}
		}
		GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_Boss_Theme_Gull", base.gameObject);
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		GameManager.IsBossIntro = true;
		GameManager.Instance.PreventPausing = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		bossUI = GameUIRoot.Instance.bossController;
		base.aiAnimator.enabled = false;
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
		base.renderer.enabled = false;
		StaticReferenceManager.DestroyAllProjectiles();
		HandlePlayerWalkIn(enterer);
		m_camera = GameManager.Instance.MainCameraController;
		m_camera.StopTrackingPlayer();
		m_camera.SetManualControl(true, false);
		m_camera.OverridePosition = m_camera.transform.position;
		m_cameraTransform = m_camera.transform;
		if (gullAnimator == null)
		{
			GameObject gameObject = new GameObject("gull_placeholder");
			gameObject.transform.position = base.transform.position;
			tk2dSprite tk2dSprite2 = tk2dSprite.AddComponent(gameObject, base.sprite.Collection, base.sprite.spriteId);
			gullAnimator = tk2dSpriteAnimator.AddComponent(gameObject, base.spriteAnimator.Library, base.spriteAnimator.DefaultClipId);
			SpriteOutlineManager.AddOutlineToSprite(tk2dSprite2, Color.black, 0.5f);
			DepthLookupManager.ProcessRenderer(tk2dSprite2.renderer);
			tk2dSpriteAnimator obj = gullAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
			tk2dSprite2.UpdateZDepth();
			gunObject = new GameObject("gull_gun_placeholder");
			gunObject.transform.position = base.transform.position;
			tk2dSprite.AddComponent(gunObject, base.sprite.Collection, base.sprite.Collection.GetSpriteIdByName("gatling_gun_stationary"));
			DepthLookupManager.ProcessRenderer(gunObject.GetComponent<Renderer>());
			gullAnimator.GetComponent<Renderer>().enabled = false;
		}
		yield return null;
		yield return null;
		Minimap.Instance.TemporarilyPreventMinimap = true;
		m_isRunning = true;
	}

	protected void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		for (int i = 0; i < base.aiActor.animationAudioEvents.Count; i++)
		{
			if (base.aiActor.animationAudioEvents[i].eventTag == frame.eventInfo && GameManager.AUDIO_ENABLED)
			{
				AkSoundEngine.PostEvent(base.aiActor.animationAudioEvents[i].eventName, base.gameObject);
			}
		}
	}

	protected void EndSequence()
	{
		bossUI.EndBossPortraitEarly();
		m_camera.StartTrackingPlayer();
		m_camera.SetManualControl(false);
		base.aiAnimator.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
		base.renderer.enabled = true;
		if (m_shadowTransform != null)
		{
			m_shadowTransform.position = base.specRigidbody.UnitCenter;
		}
		if (m_shadowAnimator != null)
		{
			m_shadowAnimator.Play("shadow_static");
			m_shadowAnimator.Sprite.independentOrientation = true;
			m_shadowAnimator.Sprite.IsPerpendicular = false;
			m_shadowAnimator.Sprite.HeightOffGround = -1f;
		}
		base.specRigidbody.CollideWithOthers = true;
		if ((bool)base.aiActor)
		{
			base.aiActor.IsGone = false;
			base.aiActor.State = AIActor.ActorState.Normal;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i].healthHaver.IsDead)
			{
				GameManager.Instance.AllPlayers[i].ToggleGunRenderers(true, string.Empty);
			}
		}
		if (feathersSystem != null)
		{
			UnityEngine.Object.Destroy(feathersSystem.gameObject);
		}
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(true, false, string.Empty);
		GameManager.Instance.PreventPausing = false;
		BraveTime.ClearMultiplier(base.gameObject);
		GameManager.IsBossIntro = false;
		tk2dSpriteAnimator[] componentsInChildren = GetComponentsInChildren<tk2dSpriteAnimator>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if ((bool)componentsInChildren[j])
			{
				componentsInChildren[j].alwaysUpdateOffscreen = true;
			}
		}
		Minimap.Instance.TemporarilyPreventMinimap = false;
		m_isRunning = false;
	}

	private IEnumerator DelayedTriggerAnimation(tk2dSpriteAnimator anim, string animName, float delay)
	{
		float elapsed = 0f;
		while (elapsed < delay)
		{
			elapsed += m_deltaTime;
			yield return null;
		}
		anim.Play(animName);
	}

	private IEnumerator WaitForBossCard()
	{
		m_waitingForBossCard = true;
		yield return StartCoroutine(bossUI.TriggerBossPortraitCR(portraitSlideSettings));
		m_waitingForBossCard = false;
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (!m_isRunning || !base.enabled)
		{
			return;
		}
		if (GenericIntroDoer.SkipFrame)
		{
			GenericIntroDoer.SkipFrame = false;
			return;
		}
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
		if (m_shadowTransform == null && (bool)base.aiActor.ShadowObject)
		{
			m_shadowTransform = base.aiActor.ShadowObject.transform;
		}
		for (int i = 0; i < m_animators.Count; i++)
		{
			m_animators[i].UpdateAnimation(realDeltaTime);
		}
		for (int j = 0; j < activeMotions.Count; j++)
		{
			CutsceneMotion cutsceneMotion = activeMotions[j];
			Vector2? lerpEnd = cutsceneMotion.lerpEnd;
			Vector2 vector = ((!lerpEnd.HasValue) ? GameManager.Instance.MainCameraController.GetIdealCameraPosition() : cutsceneMotion.lerpEnd.Value);
			float num = Vector2.Distance(vector, cutsceneMotion.lerpStart);
			float num2 = cutsceneMotion.speed * realDeltaTime;
			float num3 = num2 / num;
			cutsceneMotion.lerpProgress = Mathf.Clamp01(cutsceneMotion.lerpProgress + num3);
			float t = cutsceneMotion.lerpProgress;
			if (cutsceneMotion.isSmoothStepped)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			Vector2 vector2 = Vector2.Lerp(cutsceneMotion.lerpStart, vector, t);
			if (cutsceneMotion.camera != null)
			{
				cutsceneMotion.camera.OverridePosition = vector2.ToVector3ZUp(cutsceneMotion.zOffset);
			}
			else
			{
				cutsceneMotion.transform.position = BraveUtility.QuantizeVector(vector2.ToVector3ZUp(cutsceneMotion.zOffset), PhysicsEngine.Instance.PixelsPerUnit);
			}
			if (cutsceneMotion.lerpProgress == 1f)
			{
				activeMotions.RemoveAt(j);
				j--;
				m_currentPhase++;
				m_phaseComplete = true;
			}
		}
		bool flag = BraveInput.PrimaryPlayerInstance.MenuInteractPressed;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			flag = flag || BraveInput.SecondaryPlayerInstance.MenuInteractPressed;
		}
		if (flag && !m_hasSkipped)
		{
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !m_hasCoopTeleported)
			{
				TeleportCoopPlayers();
			}
			m_currentPhase = 13;
			m_phaseComplete = true;
			activeMotions.Clear();
			m_hasSkipped = false;
			base.specRigidbody.CollideWithOthers = true;
			if ((bool)base.aiActor)
			{
				base.aiActor.IsGone = false;
				base.aiActor.State = AIActor.ActorState.Normal;
			}
			tk2dSpriteAnimator[] componentsInChildren = GetComponentsInChildren<tk2dSpriteAnimator>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				if ((bool)componentsInChildren[k])
				{
					componentsInChildren[k].alwaysUpdateOffscreen = true;
				}
			}
		}
		Vector3 vector3 = new Vector3(0f, 1f, 0f);
		if (m_phaseComplete)
		{
			switch (m_currentPhase)
			{
			case 0:
			{
				gullAnimator.transform.position = new Vector3(30f, 5f, 0f) + base.transform.position + vector3;
				SpriteOutlineManager.ToggleOutlineRenderers(gullAnimator.GetComponent<tk2dSprite>(), false);
				CutsceneMotion cutsceneMotion3 = new CutsceneMotion(m_cameraTransform, base.specRigidbody.UnitCenter, cameraMoveSpeed);
				cutsceneMotion3.camera = m_camera;
				activeMotions.Add(cutsceneMotion3);
				m_phaseComplete = false;
				break;
			}
			case 1:
				m_phaseCountdown = initialDelay;
				m_phaseComplete = false;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !m_hasCoopTeleported)
				{
					TeleportCoopPlayers();
				}
				break;
			case 2:
			{
				m_shadowAnimator = base.aiActor.ShadowObject.GetComponent<tk2dSpriteAnimator>();
				m_animators.Add(m_shadowAnimator);
				m_animators.Add(gullAnimator);
				CutsceneMotion item = new CutsceneMotion(gullAnimator.transform, new Vector2(-60f, 0f) + gullAnimator.transform.position.XY(), 27f);
				activeMotions.Add(item);
				AkSoundEngine.PostEvent("Play_ANM_Gull_Shadow_01", base.gameObject);
				m_phaseComplete = false;
				break;
			}
			case 3:
				m_phaseCountdown = initialDelay;
				m_phaseComplete = false;
				break;
			case 4:
			{
				gullAnimator.GetComponent<Renderer>().enabled = true;
				SpriteOutlineManager.ToggleOutlineRenderers(gullAnimator.GetComponent<tk2dSprite>(), true);
				m_shadowAnimator.Play("shadow_out");
				AkSoundEngine.PostEvent("Play_ANM_Gull_Intro_01", base.gameObject);
				gullAnimator.enabled = false;
				gullAnimator.Play(gullAnimator.GetClipByName("fly"));
				CutsceneMotion cutsceneMotion5 = new CutsceneMotion(gullAnimator.transform, base.transform.position + vector3, 20f);
				cutsceneMotion5.isSmoothStepped = false;
				activeMotions.Add(cutsceneMotion5);
				m_phaseComplete = false;
				break;
			}
			case 5:
				UnityEngine.Object.Destroy(gunObject);
				gullAnimator.transform.position -= vector3;
				gullAnimator.Play(gullAnimator.GetClipByName("pick_up"));
				AkSoundEngine.PostEvent("Play_ANM_Gull_Lift_01", base.gameObject);
				m_phaseCountdown = (float)gullAnimator.CurrentClip.frames.Length / gullAnimator.CurrentClip.fps;
				m_phaseComplete = false;
				break;
			case 6:
			{
				m_shadowAnimator.Play("shadow_in");
				gullAnimator.Play(gullAnimator.GetClipByName("fly_pick_up"));
				gullAnimator.Sprite.HeightOffGround += 3f;
				CutsceneMotion cutsceneMotion4 = new CutsceneMotion(gullAnimator.transform, base.transform.position + new Vector3(20f, 20f, 0f), 15f);
				cutsceneMotion4.isSmoothStepped = false;
				activeMotions.Add(cutsceneMotion4);
				m_phaseComplete = false;
				break;
			}
			case 7:
				m_phaseCountdown = 1f;
				m_phaseComplete = false;
				gullAnimator.Sprite.HeightOffGround -= 3f;
				gullAnimator.Play("land_feathered");
				gullAnimator.Stop();
				break;
			case 8:
			{
				base.aiActor.ToggleShadowVisiblity(true);
				m_shadowAnimator.Play("shadow_out");
				AkSoundEngine.PostEvent("Play_ANM_Gull_Descend_01", base.gameObject);
				StartCoroutine(DelayedTriggerAnimation(gullAnimator, "land_feathered", 0.8f));
				gullAnimator.transform.position = base.transform.position + new Vector3(0f, 50f, 0f);
				CutsceneMotion item2 = new CutsceneMotion(gullAnimator.transform, base.transform.position, 50f);
				activeMotions.Add(item2);
				m_phaseComplete = false;
				break;
			}
			case 9:
				m_camera.DoScreenShake(landingShakeSettings, null);
				m_phaseCountdown = 1.5f;
				m_phaseComplete = false;
				break;
			case 10:
				m_animators.Remove(m_shadowAnimator);
				gullAnimator.Play(gullAnimator.GetClipByName("awaken_feathered"));
				AkSoundEngine.PostEvent("Play_ANM_Gull_Flex_01", base.gameObject);
				m_phaseCountdown = (float)gullAnimator.CurrentClip.frames.Length / gullAnimator.CurrentClip.fps;
				m_phaseComplete = false;
				break;
			case 11:
			{
				Vector3 position = base.transform.position + base.sprite.GetBounds().center + new Vector3(0f, 0f, -5f);
				feathersSystem = SpawnManager.SpawnVFX(feathersVFX, position, feathersVFX.transform.rotation).GetComponent<ParticleSystem>();
				feathersSystem.Play();
				for (int l = 0; l < numFeathersToSpawn; l++)
				{
					float z = (float)l * (360f / (float)numFeathersToSpawn);
					DebrisObject component = SpawnManager.SpawnDebris(feathersDebris, position, Quaternion.identity).GetComponent<DebrisObject>();
					float num4 = UnityEngine.Random.Range(4f, 10f);
					float z2 = UnityEngine.Random.Range(2f, 5f);
					float startingHeight = UnityEngine.Random.Range(0.5f, 2f);
					component.Trigger((Quaternion.Euler(0f, 0f, z) * Vector2.right * num4).WithZ(z2), startingHeight);
				}
				m_camera.DoScreenShake(featherShakeSettings, null);
				gullAnimator.Play(gullAnimator.GetClipByName("awaken_plucked"));
				m_phaseCountdown = (float)gullAnimator.CurrentClip.frames.Length / gullAnimator.CurrentClip.fps;
				m_phaseComplete = false;
				break;
			}
			case 12:
				AkSoundEngine.PostEvent("Play_UI_boss_intro_01", base.gameObject);
				StartCoroutine(WaitForBossCard());
				m_phaseCountdown = 1E+10f;
				m_phaseComplete = false;
				break;
			case 13:
			{
				gullAnimator.enabled = true;
				m_animators.Remove(gullAnimator);
				GameManager.Instance.MainCameraController.ForceUpdateControllerCameraState(CameraController.ControllerCameraState.RoomLock);
				CutsceneMotion cutsceneMotion2 = new CutsceneMotion(m_cameraTransform, null, cameraMoveSpeed);
				cutsceneMotion2.camera = m_camera;
				activeMotions.Add(cutsceneMotion2);
				m_phaseComplete = false;
				break;
			}
			case 14:
				if ((bool)gunObject)
				{
					UnityEngine.Object.Destroy(gunObject);
				}
				UnityEngine.Object.Destroy(gullAnimator.gameObject);
				EndSequence();
				return;
			}
		}
		if (m_currentPhase > 14)
		{
			m_currentPhase = 14;
		}
		Bounds untrimmedBounds = gullAnimator.Sprite.GetUntrimmedBounds();
		if (m_shadowTransform != null)
		{
			m_shadowTransform.position = m_shadowTransform.position.WithX(gullAnimator.transform.position.x + untrimmedBounds.extents.x);
		}
		if (m_currentPhase == 12 && !m_waitingForBossCard)
		{
			m_phaseCountdown = 0f;
			m_currentPhase++;
			m_phaseComplete = true;
		}
		if (feathersSystem != null)
		{
			feathersSystem.Simulate(realDeltaTime, true, false);
		}
		if (m_phaseCountdown > 0f)
		{
			m_phaseCountdown -= realDeltaTime;
			if (m_phaseCountdown <= 0f)
			{
				m_phaseCountdown = 0f;
				m_currentPhase++;
				m_phaseComplete = true;
			}
		}
		gullAnimator.GetComponent<tk2dSprite>().UpdateZDepth();
	}

	protected override void OnDestroy()
	{
		if (m_room != null)
		{
			m_room.Entered -= TriggerSequence;
		}
		base.OnDestroy();
	}

	private void TeleportCoopPlayers()
	{
		if (!m_hasCoopTeleported)
		{
			Vector2 centerPosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
			Vector2 centerPosition2 = GameManager.Instance.SecondaryPlayer.CenterPosition;
			if (Vector2.Distance(centerPosition2, m_idealStartingPositions[0]) < Vector2.Distance(centerPosition, m_idealStartingPositions[0]))
			{
				Vector2 vector = m_idealStartingPositions[0];
				m_idealStartingPositions[0] = m_idealStartingPositions[1];
				m_idealStartingPositions[1] = vector;
			}
			if (Vector3.Distance(centerPosition, m_idealStartingPositions[0]) > 2f)
			{
				GameManager.Instance.PrimaryPlayer.WarpToPoint(m_idealStartingPositions[0], true);
			}
			if (Vector3.Distance(centerPosition2, m_idealStartingPositions[1]) > 2f)
			{
				GameManager.Instance.SecondaryPlayer.WarpToPoint(m_idealStartingPositions[1], true);
			}
		}
	}
}
