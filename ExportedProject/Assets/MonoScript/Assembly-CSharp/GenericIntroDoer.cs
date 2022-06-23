using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class GenericIntroDoer : TimeInvariantMonoBehaviour, IPlaceConfigurable
{
	public enum TriggerType
	{
		PlayerEnteredRoom = 10,
		BossTriggerZone = 20
	}

	private enum Phase
	{
		CameraIntro,
		InitialDelay,
		PreIntroAnim,
		BossCard,
		CameraOutro,
		Cleanup
	}

	public TriggerType triggerType = TriggerType.PlayerEnteredRoom;

	public float initialDelay = 1f;

	public float cameraMoveSpeed = 5f;

	public AIAnimator specifyIntroAiAnimator;

	public string BossMusicEvent = "Play_MUS_Boss_Theme_Beholster";

	public bool PreventBossMusic;

	public bool InvisibleBeforeIntroAnim;

	[CheckAnimation(null)]
	public string preIntroAnim = string.Empty;

	[CheckDirectionalAnimation(null)]
	public string preIntroDirectionalAnim = string.Empty;

	[FormerlySerializedAs("preIntroAnimationName")]
	[CheckAnimation(null)]
	public string introAnim = string.Empty;

	[FormerlySerializedAs("preIntroDirectionalAnimation")]
	[CheckDirectionalAnimation(null)]
	public string introDirectionalAnim = string.Empty;

	public bool continueAnimDuringOutro;

	public GameObject cameraFocus;

	public Vector2 roomPositionCameraFocus;

	public bool restrictPlayerMotionToRoom;

	public bool fusebombLock;

	public float AdditionalHeightOffset;

	public bool SkipBossCard;

	[HideInInspectorIf("SkipBossCard", false)]
	public PortraitSlideSettings portraitSlideSettings;

	public bool HideGunAndHand;

	public Action OnIntroFinished;

	private Tribool m_singleFrameSkipDelay = Tribool.Unready;

	private bool m_isRunning;

	private bool m_waitingForBossCard;

	private bool m_hasTriggeredWalkIn;

	private Phase m_currentPhase;

	private bool m_phaseComplete = true;

	private float m_phaseCountdown;

	private CameraController m_camera;

	private Transform m_cameraTransform;

	private RoomHandler m_room;

	private GameUIBossHealthController bossUI;

	private List<tk2dSpriteAnimator> m_animators = new List<tk2dSpriteAnimator>();

	private List<CutsceneMotion> activeMotions = new List<CutsceneMotion>();

	private SpecificIntroDoer m_specificIntroDoer;

	private bool m_waitingForSpecificIntroCompletion;

	private GameObject m_roomCameraFocus;

	private bool m_isCameraModified;

	private bool m_isMotionRestricted;

	private int m_minPlayerX;

	private int m_minPlayerY;

	private int m_maxPlayerX;

	private int m_maxPlayerY;

	private Vector2[] m_idealStartingPositions;

	private bool m_hasCoopTeleported;

	public Vector2 BossCenter
	{
		get
		{
			if (m_specificIntroDoer != null)
			{
				Vector2? overrideIntroPosition = m_specificIntroDoer.OverrideIntroPosition;
				if (overrideIntroPosition.HasValue)
				{
					return overrideIntroPosition.Value;
				}
			}
			if ((bool)base.specRigidbody)
			{
				return base.specRigidbody.UnitCenter;
			}
			if ((bool)base.dungeonPlaceable)
			{
				return base.transform.position.XY() + new Vector2((float)base.dungeonPlaceable.placeableWidth / 2f, (float)base.dungeonPlaceable.placeableHeight / 2f);
			}
			return base.transform.position;
		}
	}

	public bool SkipFinalizeAnimation { get; set; }

	public bool SuppressSkipping { get; set; }

	public static bool SkipFrame { get; set; }

	private void Awake()
	{
		m_specificIntroDoer = GetComponent<SpecificIntroDoer>();
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (!m_isRunning || !base.enabled)
		{
			return;
		}
		if (SkipFrame)
		{
			SkipFrame = false;
			return;
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
			if (cutsceneMotion.lerpProgress >= 1f)
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
		if (SuppressSkipping)
		{
			flag = false;
		}
		if (m_singleFrameSkipDelay != Tribool.Unready)
		{
			flag = false;
		}
		if (flag)
		{
			BraveMemory.HandleBossCardSkip();
			m_singleFrameSkipDelay = Tribool.Ready;
		}
		else if (m_singleFrameSkipDelay == Tribool.Ready)
		{
			m_singleFrameSkipDelay = Tribool.Complete;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !m_hasCoopTeleported)
			{
				TeleportCoopPlayers();
			}
			AkSoundEngine.PostEvent("STOP_SND_Diagetic", base.gameObject);
			m_currentPhase = Phase.CameraOutro;
			bossUI.EndBossPortraitEarly();
			m_phaseComplete = true;
			activeMotions.Clear();
			SpeculativeRigidbody[] componentsInChildren = GetComponentsInChildren<SpeculativeRigidbody>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				componentsInChildren[k].CollideWithOthers = true;
			}
			AIActor[] componentsInChildren2 = GetComponentsInChildren<AIActor>();
			for (int l = 0; l < componentsInChildren2.Length; l++)
			{
				componentsInChildren2[l].IsGone = false;
			}
			if ((bool)base.aiActor)
			{
				base.aiActor.State = AIActor.ActorState.Normal;
			}
			if (InvisibleBeforeIntroAnim)
			{
				base.aiActor.ToggleRenderers(true);
			}
			if (!string.IsNullOrEmpty(preIntroDirectionalAnim))
			{
				((!specifyIntroAiAnimator) ? base.aiAnimator : specifyIntroAiAnimator).EndAnimationIf(preIntroDirectionalAnim);
			}
			if (m_specificIntroDoer != null)
			{
				m_specificIntroDoer.EndIntro();
			}
			tk2dSpriteAnimator[] componentsInChildren3 = GetComponentsInChildren<tk2dSpriteAnimator>();
			for (int m = 0; m < componentsInChildren3.Length; m++)
			{
				if ((bool)componentsInChildren3[m])
				{
					componentsInChildren3[m].alwaysUpdateOffscreen = true;
				}
			}
		}
		if (m_phaseComplete)
		{
			DirectionalAnimation directionalAnimation = null;
			if ((bool)base.aiAnimator)
			{
				if (base.aiAnimator.IdleAnimation.HasAnimation)
				{
					directionalAnimation = base.aiAnimator.IdleAnimation;
				}
				else if (base.aiAnimator.MoveAnimation.HasAnimation)
				{
					directionalAnimation = base.aiAnimator.MoveAnimation;
				}
			}
			switch (m_currentPhase)
			{
			case Phase.CameraIntro:
			{
				CutsceneMotion cutsceneMotion2 = new CutsceneMotion(m_cameraTransform, BossCenter, cameraMoveSpeed);
				cutsceneMotion2.camera = m_camera;
				activeMotions.Add(cutsceneMotion2);
				m_phaseComplete = false;
				if ((bool)base.spriteAnimator)
				{
					m_animators.Add(base.spriteAnimator);
					base.spriteAnimator.enabled = false;
				}
				if ((bool)base.aiAnimator && (bool)base.aiAnimator.ChildAnimator)
				{
					m_animators.Add(base.aiAnimator.ChildAnimator.spriteAnimator);
					base.aiAnimator.ChildAnimator.spriteAnimator.enabled = false;
				}
				if (m_specificIntroDoer != null)
				{
					m_specificIntroDoer.OnCameraIntro();
				}
				break;
			}
			case Phase.InitialDelay:
				m_phaseCountdown = initialDelay;
				m_phaseComplete = false;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !m_hasCoopTeleported)
				{
					TeleportCoopPlayers();
				}
				break;
			case Phase.PreIntroAnim:
				if (InvisibleBeforeIntroAnim)
				{
					base.aiActor.ToggleRenderers(true);
				}
				if (m_specificIntroDoer != null)
				{
					m_specificIntroDoer.StartIntro(m_animators);
					m_phaseCountdown = float.MaxValue;
					m_phaseComplete = false;
					m_waitingForSpecificIntroCompletion = !m_specificIntroDoer.IsIntroFinished;
					if (m_waitingForSpecificIntroCompletion)
					{
						break;
					}
				}
				if (!string.IsNullOrEmpty(introAnim))
				{
					base.spriteAnimator.Play(base.spriteAnimator.GetClipByName(introAnim));
					m_phaseCountdown = (float)base.spriteAnimator.CurrentClip.frames.Length / base.spriteAnimator.CurrentClip.fps;
					m_phaseCountdown += 0.25f;
					m_phaseComplete = false;
				}
				else if (!string.IsNullOrEmpty(introDirectionalAnim))
				{
					AIAnimator aIAnimator3 = ((!specifyIntroAiAnimator) ? base.aiAnimator : specifyIntroAiAnimator);
					aIAnimator3.PlayUntilFinished(introDirectionalAnim);
					tk2dSpriteAnimator tk2dSpriteAnimator2 = aIAnimator3.spriteAnimator;
					if ((bool)aIAnimator3.ChildAnimator && aIAnimator3.ChildAnimator.HasDirectionalAnimation(introDirectionalAnim))
					{
						tk2dSpriteAnimator2 = aIAnimator3.ChildAnimator.spriteAnimator;
					}
					m_phaseCountdown = (float)tk2dSpriteAnimator2.CurrentClip.frames.Length / tk2dSpriteAnimator2.CurrentClip.fps;
					m_phaseCountdown += 0.25f;
					m_phaseComplete = false;
				}
				else
				{
					m_phaseCountdown = 0f;
					m_phaseComplete = false;
				}
				break;
			case Phase.BossCard:
				if (!SkipBossCard)
				{
					AkSoundEngine.PostEvent("Play_UI_boss_intro_01", base.gameObject);
					StartCoroutine(WaitForBossCard());
					m_phaseCountdown = float.MaxValue;
					m_phaseComplete = false;
					if (m_specificIntroDoer != null)
					{
						m_specificIntroDoer.OnBossCard();
					}
				}
				break;
			case Phase.CameraOutro:
			{
				if ((bool)cameraFocus || roomPositionCameraFocus != Vector2.zero || fusebombLock)
				{
					ModifyCamera(true);
				}
				if (restrictPlayerMotionToRoom)
				{
					RestrictMotion(true);
				}
				Vector2? targetPosition = null;
				if ((bool)m_specificIntroDoer)
				{
					Vector2? overrideOutroPosition = m_specificIntroDoer.OverrideOutroPosition;
					if (overrideOutroPosition.HasValue)
					{
						targetPosition = overrideOutroPosition.Value;
					}
				}
				GameManager.Instance.MainCameraController.ForceUpdateControllerCameraState(CameraController.ControllerCameraState.RoomLock);
				CutsceneMotion cutsceneMotion3 = new CutsceneMotion(m_cameraTransform, targetPosition, cameraMoveSpeed);
				cutsceneMotion3.camera = m_camera;
				activeMotions.Add(cutsceneMotion3);
				m_phaseComplete = false;
				if (!continueAnimDuringOutro)
				{
					if (AdditionalHeightOffset != 0f)
					{
						tk2dBaseSprite[] componentsInChildren5 = GetComponentsInChildren<tk2dBaseSprite>();
						for (int num4 = 0; num4 < componentsInChildren5.Length; num4++)
						{
							componentsInChildren5[num4].HeightOffGround -= AdditionalHeightOffset;
						}
						base.sprite.UpdateZDepth();
					}
					if (!string.IsNullOrEmpty(introDirectionalAnim))
					{
						AIAnimator aIAnimator2 = ((!specifyIntroAiAnimator) ? base.aiAnimator : specifyIntroAiAnimator);
						aIAnimator2.EndAnimationIf(introDirectionalAnim);
					}
					if (directionalAnimation != null && !SkipFinalizeAnimation)
					{
						base.spriteAnimator.Play(directionalAnimation.GetInfo(-90f).name);
					}
				}
				if (m_specificIntroDoer != null)
				{
					m_specificIntroDoer.OnCameraOutro();
				}
				break;
			}
			case Phase.Cleanup:
				if (continueAnimDuringOutro)
				{
					if (AdditionalHeightOffset != 0f)
					{
						tk2dBaseSprite[] componentsInChildren4 = GetComponentsInChildren<tk2dBaseSprite>();
						for (int n = 0; n < componentsInChildren4.Length; n++)
						{
							componentsInChildren4[n].HeightOffGround -= AdditionalHeightOffset;
						}
						base.sprite.UpdateZDepth();
					}
					if (!string.IsNullOrEmpty(introDirectionalAnim))
					{
						AIAnimator aIAnimator = ((!specifyIntroAiAnimator) ? base.aiAnimator : specifyIntroAiAnimator);
						aIAnimator.EndAnimationIf(introDirectionalAnim);
					}
					if (directionalAnimation != null && !SkipFinalizeAnimation)
					{
						base.spriteAnimator.Play(directionalAnimation.GetInfo(-90f).name);
					}
				}
				if ((bool)base.spriteAnimator)
				{
					m_animators.Remove(base.spriteAnimator);
					base.spriteAnimator.enabled = true;
				}
				if ((bool)base.aiAnimator && (bool)base.aiAnimator.ChildAnimator)
				{
					base.aiAnimator.ChildAnimator.spriteAnimator.enabled = true;
				}
				if (m_specificIntroDoer != null)
				{
					m_specificIntroDoer.OnCleanup();
				}
				EndSequence();
				return;
			}
		}
		if (m_currentPhase > Phase.Cleanup)
		{
			m_currentPhase = Phase.Cleanup;
		}
		if (m_currentPhase == Phase.PreIntroAnim)
		{
			if (m_waitingForSpecificIntroCompletion && m_specificIntroDoer.IsIntroFinished)
			{
				m_phaseCountdown = 0f;
				m_currentPhase++;
				m_phaseComplete = true;
			}
			if (!string.IsNullOrEmpty(preIntroDirectionalAnim))
			{
				((!specifyIntroAiAnimator) ? base.aiAnimator : specifyIntroAiAnimator).EndAnimationIf(preIntroDirectionalAnim);
			}
		}
		else if (m_currentPhase == Phase.BossCard && !m_waitingForBossCard)
		{
			m_phaseCountdown = 0f;
			m_currentPhase++;
			m_phaseComplete = true;
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
	}

	protected override void OnDestroy()
	{
		if (m_room != null)
		{
			m_room.Entered -= PlayerEntered;
		}
		if (m_isCameraModified)
		{
			ModifyCamera(false);
		}
		if (m_isMotionRestricted)
		{
			RestrictMotion(false);
		}
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		m_room.Entered += PlayerEntered;
	}

	public void PlayerEntered(PlayerController player)
	{
		if (GameManager.HasInstance && GameManager.Instance.RunData.SpawnAngryToadie && (bool)base.healthHaver && !base.healthHaver.IsSubboss && GameManager.Instance.CurrentFloor < 5 && !base.name.StartsWith("BossStatues", StringComparison.Ordinal) && !base.name.StartsWith("DemonWall", StringComparison.Ordinal))
		{
			Vector2 position = base.specRigidbody.UnitBottomRight + new Vector2(2.5f, 0.25f);
			AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(GlobalEnemyGuids.BulletKingToadieAngry), position, base.aiActor.ParentRoom, true);
			GameManager.Instance.RunData.SpawnAngryToadie = false;
		}
		if (triggerType == TriggerType.PlayerEnteredRoom)
		{
			TriggerSequence(player);
		}
	}

	public void TriggerSequence(PlayerController enterer)
	{
		StartCoroutine(FrameDelayedTriggerSequence(enterer));
	}

	public IEnumerator FrameDelayedTriggerSequence(PlayerController enterer)
	{
		if (GameManager.Instance.PreventPausing || !base.enabled || ((bool)base.aiActor && !base.aiActor.enabled))
		{
			yield break;
		}
		m_room.Entered -= PlayerEntered;
		List<AIActor> enemiesInRoom = m_room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < enemiesInRoom.Count; i++)
		{
			if (enemiesInRoom[i].gameObject != base.gameObject)
			{
				GenericIntroDoer component = enemiesInRoom[i].gameObject.GetComponent<GenericIntroDoer>();
				if ((bool)component && component.m_isRunning)
				{
					yield break;
				}
			}
		}
		if (!PreventBossMusic)
		{
			string text = BossMusicEvent;
			if ((bool)m_specificIntroDoer && m_specificIntroDoer.OverrideBossMusicEvent != null)
			{
				text = m_specificIntroDoer.OverrideBossMusicEvent;
			}
			GameManager.Instance.DungeonMusicController.SwitchToBossMusic((!string.IsNullOrEmpty(text)) ? text : "Play_MUS_Boss_Theme_Beholster", base.gameObject);
		}
		Minimap.Instance.ToggleMinimap(false);
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		GameManager.IsBossIntro = true;
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			if ((bool)GameManager.Instance.AllPlayers[j])
			{
				GameManager.Instance.AllPlayers[j].SetInputOverride("BossIntro");
			}
		}
		GameManager.Instance.PreventPausing = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		if ((bool)base.healthHaver && base.healthHaver.UsesVerticalBossBar)
		{
			bossUI = GameUIRoot.Instance.bossControllerSide;
		}
		else if ((bool)base.healthHaver && base.healthHaver.UsesSecondaryBossBar)
		{
			bossUI = GameUIRoot.Instance.bossController2;
		}
		else
		{
			bossUI = GameUIRoot.Instance.bossController;
		}
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.enabled = false;
		}
		if ((bool)base.renderer)
		{
			base.renderer.enabled = true;
		}
		if (HideGunAndHand && (bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "genericIntro");
		}
		StaticReferenceManager.DestroyAllProjectiles();
		HandlePlayerWalkIn(enterer);
		m_camera = GameManager.Instance.MainCameraController;
		m_camera.StopTrackingPlayer();
		m_camera.SetManualControl(true, false);
		m_camera.OverridePosition = m_camera.transform.position;
		m_cameraTransform = m_camera.transform;
		if (AdditionalHeightOffset != 0f)
		{
			tk2dBaseSprite[] componentsInChildren = GetComponentsInChildren<tk2dBaseSprite>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				componentsInChildren[k].HeightOffGround += AdditionalHeightOffset;
			}
			base.sprite.UpdateZDepth();
		}
		if (InvisibleBeforeIntroAnim)
		{
			base.aiActor.ToggleRenderers(false);
		}
		if (!string.IsNullOrEmpty(preIntroAnim))
		{
			base.spriteAnimator.Play(preIntroAnim);
		}
		if (!string.IsNullOrEmpty(preIntroDirectionalAnim))
		{
			((!specifyIntroAiAnimator) ? base.aiAnimator : specifyIntroAiAnimator).PlayUntilFinished(preIntroDirectionalAnim);
		}
		if ((bool)m_specificIntroDoer)
		{
			m_specificIntroDoer.PlayerWalkedIn(enterer, m_animators);
		}
		yield return null;
		yield return null;
		Minimap.Instance.TemporarilyPreventMinimap = true;
		m_isRunning = true;
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
		float num = float.MaxValue;
		RuntimeExitDefinition runtimeExitDefinition = null;
		for (int j = 0; j < m_room.area.instanceUsedExits.Count; j++)
		{
			PrototypeRoomExit key = m_room.area.instanceUsedExits[j];
			if (!m_room.area.exitToLocalDataMap.ContainsKey(key))
			{
				continue;
			}
			RuntimeRoomExitData key2 = m_room.area.exitToLocalDataMap[key];
			if (m_room.exitDefinitionsByExit.ContainsKey(key2))
			{
				RuntimeExitDefinition runtimeExitDefinition2 = m_room.exitDefinitionsByExit[key2];
				float num2 = Vector2.Distance(b: ((runtimeExitDefinition2.upstreamRoom != m_room) ? runtimeExitDefinition2.GetDownstreamNearDoorPosition() : runtimeExitDefinition2.GetUpstreamNearDoorPosition()).ToCenterVector2(), a: leadPlayer.CenterPosition);
				if (num2 < num)
				{
					num = num2;
					runtimeExitDefinition = runtimeExitDefinition2;
				}
			}
		}
		if (runtimeExitDefinition == null || num > 10f)
		{
			runtimeExitDefinition = m_room.GetExitDefinitionForConnectedRoom(roomHandler);
		}
		DungeonData.Direction direction = DungeonData.InvertDirection(runtimeExitDefinition.GetDirectionFromRoom(m_room));
		IntVector2 intVector = ((runtimeExitDefinition.upstreamRoom != m_room) ? runtimeExitDefinition.GetDownstreamNearDoorPosition() : runtimeExitDefinition.GetUpstreamNearDoorPosition());
		if ((bool)m_specificIntroDoer)
		{
			intVector = m_specificIntroDoer.OverrideExitBasePosition(direction, intVector);
		}
		float num3 = ((direction != 0 && direction != DungeonData.Direction.SOUTH) ? intVector.x : intVector.y);
		num3 = ((direction != DungeonData.Direction.EAST && direction != 0) ? (num3 - 3f) : (num3 + 3f));
		Debug.LogError(string.Concat(direction, "|", num3));
		leadPlayer.ForceWalkInDirectionWhilePaused(direction, num3);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(leadPlayer);
			float num4 = ((direction != 0 && direction != DungeonData.Direction.SOUTH) ? Mathf.Abs(num3 - leadPlayer.CenterPosition.x) : Mathf.Abs(num3 - leadPlayer.CenterPosition.y));
			IntVector2 pixelsToMove = IntVector2.Zero;
			int num5 = Mathf.RoundToInt(num4 * 16f);
			switch (direction)
			{
			case DungeonData.Direction.NORTH:
				pixelsToMove = new IntVector2(0, num5);
				break;
			case DungeonData.Direction.EAST:
				pixelsToMove = new IntVector2(num5, 0);
				break;
			case DungeonData.Direction.SOUTH:
				pixelsToMove = new IntVector2(0, -num5);
				break;
			case DungeonData.Direction.WEST:
				pixelsToMove = new IntVector2(-num5, 0);
				break;
			}
			CollisionData result;
			if (PhysicsEngine.Instance.RigidbodyCast(otherPlayer.specRigidbody, pixelsToMove, out result))
			{
				num4 = PhysicsEngine.PixelToUnit(result.NewPixelsToMove).magnitude;
			}
			CollisionData.Pool.Free(ref result);
			switch (direction)
			{
			case DungeonData.Direction.NORTH:
				num3 = otherPlayer.CenterPosition.y + num4;
				break;
			case DungeonData.Direction.EAST:
				num3 = otherPlayer.CenterPosition.x + num4;
				break;
			case DungeonData.Direction.SOUTH:
				num3 = otherPlayer.CenterPosition.y - num4;
				break;
			case DungeonData.Direction.WEST:
				num3 = otherPlayer.CenterPosition.x - num4;
				break;
			}
			otherPlayer.ForceWalkInDirectionWhilePaused(direction, num3);
			m_idealStartingPositions = new Vector2[2];
			IntVector2 intVector2 = ((direction != 0 && direction != DungeonData.Direction.SOUTH) ? (intVector + IntVector2.Up) : (intVector + IntVector2.Right));
			float num6 = 3f;
			switch (direction)
			{
			case DungeonData.Direction.NORTH:
				m_idealStartingPositions[0] = intVector2.ToVector2() + new Vector2(-0.5f, 0f) + new Vector2(0f, num6 + 0.5f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(0.25f, -0.25f) + new Vector2(0f, num6 - 0.25f);
				break;
			case DungeonData.Direction.EAST:
				m_idealStartingPositions[0] = intVector2.ToVector2() + new Vector2(num6 + 0.5f, 0f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(-0.25f, -1f) + new Vector2(num6 - 0.25f, 0f);
				break;
			case DungeonData.Direction.SOUTH:
				m_idealStartingPositions[0] = intVector2.ToVector2() + new Vector2(-0.5f, 0f) - new Vector2(0f, num6 + 0.5f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(0.25f, 0.25f) - new Vector2(0f, num6 - 0.25f);
				break;
			case DungeonData.Direction.WEST:
				m_idealStartingPositions[0] = intVector2.ToVector2() - new Vector2(num6 + 0.5f, 0f);
				m_idealStartingPositions[1] = intVector2.ToVector2() + new Vector2(0.25f, -1f) - new Vector2(num6 - 0.25f, 0f);
				break;
			case DungeonData.Direction.NORTHEAST:
			case DungeonData.Direction.SOUTHEAST:
			case DungeonData.Direction.SOUTHWEST:
				break;
			}
		}
	}

	private void EndSequence(bool isChildSequence = false)
	{
		if (!isChildSequence)
		{
			List<AIActor> activeEnemies = m_room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (activeEnemies[i].gameObject != base.gameObject)
				{
					GenericIntroDoer component = activeEnemies[i].gameObject.GetComponent<GenericIntroDoer>();
					if ((bool)component)
					{
						component.EndSequence(true);
					}
				}
			}
			bossUI.EndBossPortraitEarly();
			m_camera.StartTrackingPlayer();
			m_camera.SetManualControl(false);
		}
		if (HideGunAndHand && (bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(true, "genericIntro");
		}
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.enabled = true;
		}
		if ((bool)base.renderer)
		{
			base.renderer.enabled = true;
		}
		if ((bool)base.spriteAnimator)
		{
			base.spriteAnimator.enabled = true;
		}
		SpeculativeRigidbody[] componentsInChildren = GetComponentsInChildren<SpeculativeRigidbody>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].CollideWithOthers = true;
		}
		AIActor[] componentsInChildren2 = GetComponentsInChildren<AIActor>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			componentsInChildren2[k].IsGone = false;
		}
		if (m_specificIntroDoer != null)
		{
			m_specificIntroDoer.EndIntro();
		}
		if ((bool)base.aiActor)
		{
			base.aiActor.State = AIActor.ActorState.Normal;
		}
		if (InvisibleBeforeIntroAnim)
		{
			base.aiActor.ToggleRenderers(true);
		}
		if (m_room != null)
		{
			Minimap.Instance.RevealMinimapRoom(m_room, true);
		}
		for (int l = 0; l < GameManager.Instance.AllPlayers.Length; l++)
		{
			if (!GameManager.Instance.AllPlayers[l].healthHaver.IsDead)
			{
				GameManager.Instance.AllPlayers[l].ToggleGunRenderers(true, string.Empty);
			}
		}
		GameManager.Instance.PreventPausing = false;
		for (int m = 0; m < GameManager.Instance.AllPlayers.Length; m++)
		{
			if ((bool)GameManager.Instance.AllPlayers[m])
			{
				GameManager.Instance.AllPlayers[m].ClearInputOverride("BossIntro");
			}
		}
		GameUIRoot.Instance.ToggleLowerPanels(true, false, string.Empty);
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		tk2dSpriteAnimator[] componentsInChildren3 = GetComponentsInChildren<tk2dSpriteAnimator>();
		for (int n = 0; n < componentsInChildren3.Length; n++)
		{
			if ((bool)componentsInChildren3[n])
			{
				componentsInChildren3[n].alwaysUpdateOffscreen = true;
			}
		}
		BraveTime.ClearMultiplier(base.gameObject);
		GameManager.IsBossIntro = false;
		SuperReaperController.PreventShooting = false;
		Minimap.Instance.TemporarilyPreventMinimap = false;
		m_isRunning = false;
		if (OnIntroFinished != null)
		{
			OnIntroFinished();
		}
	}

	private IEnumerator WaitForBossCard()
	{
		m_waitingForBossCard = true;
		yield return StartCoroutine(bossUI.TriggerBossPortraitCR(portraitSlideSettings));
		m_waitingForBossCard = false;
	}

	private void TeleportCoopPlayers()
	{
		if (!m_hasCoopTeleported && m_idealStartingPositions != null && m_idealStartingPositions.Length >= 1)
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

	public void SkipWalkIn()
	{
		m_hasTriggeredWalkIn = true;
	}

	private void ModifyCamera(bool value)
	{
		if (m_isCameraModified == value || !GameManager.HasInstance)
		{
			return;
		}
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		if (!mainCameraController)
		{
			return;
		}
		if (value)
		{
			if ((bool)cameraFocus)
			{
				mainCameraController.LockToRoom = true;
				mainCameraController.AddFocusPoint(cameraFocus);
			}
			if (roomPositionCameraFocus != Vector2.zero)
			{
				m_roomCameraFocus = new GameObject("room camera focus");
				m_roomCameraFocus.transform.position = base.aiActor.ParentRoom.area.basePosition.ToVector2() + roomPositionCameraFocus;
				m_roomCameraFocus.transform.parent = base.aiActor.ParentRoom.hierarchyParent;
				mainCameraController.LockToRoom = true;
				mainCameraController.AddFocusPoint(m_roomCameraFocus);
			}
			if (fusebombLock)
			{
				mainCameraController.PreventFuseBombAimOffset = true;
			}
			mainCameraController.LockToRoom = true;
			m_isCameraModified = true;
			if ((bool)base.aiActor && (bool)base.aiActor.healthHaver)
			{
				base.aiActor.healthHaver.OnDeath += OnDeath;
			}
		}
		else
		{
			if ((bool)cameraFocus)
			{
				mainCameraController.LockToRoom = false;
				mainCameraController.RemoveFocusPoint(cameraFocus);
			}
			if (roomPositionCameraFocus != Vector2.zero && (bool)m_roomCameraFocus)
			{
				mainCameraController.LockToRoom = false;
				mainCameraController.RemoveFocusPoint(m_roomCameraFocus);
			}
			if (fusebombLock)
			{
				mainCameraController.PreventFuseBombAimOffset = false;
			}
			mainCameraController.LockToRoom = false;
			m_isCameraModified = false;
			if ((bool)base.aiActor && (bool)base.aiActor.healthHaver)
			{
				base.aiActor.healthHaver.OnDeath -= OnDeath;
			}
		}
	}

	public void RestrictMotion(bool value)
	{
		if (m_isMotionRestricted == value)
		{
			return;
		}
		if (value)
		{
			if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach)
			{
				return;
			}
			CellArea area = base.aiActor.ParentRoom.area;
			m_minPlayerX = area.basePosition.x * 16;
			m_minPlayerY = area.basePosition.y * 16 + 8;
			m_maxPlayerX = (area.basePosition.x + area.dimensions.x) * 16;
			m_maxPlayerY = (area.basePosition.y + area.dimensions.y - 1) * 16;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				SpeculativeRigidbody speculativeRigidbody = GameManager.Instance.AllPlayers[i].specRigidbody;
				speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
			}
		}
		else
		{
			if (!GameManager.HasInstance || GameManager.IsReturningToBreach)
			{
				return;
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController)
				{
					SpeculativeRigidbody speculativeRigidbody2 = playerController.specRigidbody;
					speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
				}
			}
		}
		m_isMotionRestricted = value;
	}

	private void PlayerMovementRestrictor(SpeculativeRigidbody playerSpecRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (validLocation)
		{
			if (pixelOffset.y < prevPixelOffset.y && playerSpecRigidbody.PixelColliders[0].MinY + pixelOffset.y < m_minPlayerY)
			{
				validLocation = false;
			}
			if (pixelOffset.y > prevPixelOffset.y && playerSpecRigidbody.PixelColliders[0].MaxY + pixelOffset.y >= m_maxPlayerY)
			{
				validLocation = false;
			}
			if (pixelOffset.x < prevPixelOffset.x && playerSpecRigidbody.PixelColliders[0].MinX + pixelOffset.x < m_minPlayerX)
			{
				validLocation = false;
			}
			if (pixelOffset.x > prevPixelOffset.x && playerSpecRigidbody.PixelColliders[0].MaxX + pixelOffset.x >= m_maxPlayerX)
			{
				validLocation = false;
			}
		}
	}

	private void OnDeath(Vector2 deathDir)
	{
		if (m_isCameraModified)
		{
			ModifyCamera(false);
		}
		if (m_isMotionRestricted)
		{
			RestrictMotion(false);
		}
	}
}
