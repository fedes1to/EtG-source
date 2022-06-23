using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Pathfinding;
using UnityEngine;

public class TalkDoerLite : DungeonPlaceableBehaviour, IPlayerInteractable
{
	[Serializable]
	public class TeleportSettings
	{
		public string anim;

		public float animDelay;

		public GameObject vfx;

		public float vfxDelay;

		public Teleport.Timing timing;

		public GameObject vfxAnchor;
	}

	public enum TalkingState
	{
		None,
		Passive,
		Conversation
	}

	private const float c_reinteractDelay = 0.5f;

	[Header("Interactable Region")]
	public bool usesOverrideInteractionRegion;

	[ShowInInspectorIf("usesOverrideInteractionRegion", false)]
	public Vector2 overrideRegionOffset = Vector2.zero;

	[ShowInInspectorIf("usesOverrideInteractionRegion", false)]
	public Vector2 overrideRegionDimensions = Vector2.zero;

	public float overrideInteractionRadius = -1f;

	public bool PreventInteraction;

	public bool AllowPlayerToPassEventually = true;

	[Header("Speech Options")]
	public Transform speakPoint;

	public bool SpeaksGleepGlorpenese;

	public string audioCharacterSpeechTag = string.Empty;

	public float playerApproachRadius = 5f;

	public float conversationBreakRadius = 5f;

	public TalkDoerLite echo1;

	public TalkDoerLite echo2;

	[Header("Other Options")]
	public bool PreventCoopInteraction;

	public bool IsPaletteSwapped;

	public Texture2D PaletteTexture;

	public TeleportSettings teleportInSettings;

	public TeleportSettings teleportOutSettings;

	public List<GameObject> itemsToLeaveBehind;

	public GameObject shadow;

	public bool DisableOnShortcutRun;

	public GameObject OptionalMinimapIcon;

	public float OverheadUIElementDelay = 1f;

	private float m_overheadUIElementDelay;

	public GameObject OverheadUIElementOnPreInteract;

	[NonSerialized]
	private dfControl m_extantOverheadUIElement;

	public tk2dSprite OptionalCustomNotificationSprite;

	public float OutlineDepth = 0.4f;

	public float OutlineLuminanceCutoff = 0.05f;

	public List<GameObject> ReassignPrefabReferences;

	[NonSerialized]
	public Action OnGenericFSMActionA;

	[NonSerialized]
	public Action OnGenericFSMActionB;

	[NonSerialized]
	public Action OnGenericFSMActionC;

	[NonSerialized]
	public Action OnGenericFSMActionD;

	[NonSerialized]
	public bool ForcePlayerLookAt;

	[NonSerialized]
	public bool ForceNonInteractable;

	private PlayerController m_talkingPlayer;

	[NonSerialized]
	public Tribool ShopStockStatus = Tribool.Unready;

	private TalkingState m_talkingState;

	private bool m_isPlayerInRange;

	private bool m_isInteractable = true;

	private bool m_showOutlines = true;

	private bool m_allowWalkAways = true;

	private bool m_hasPlayerLocked;

	private float m_setInteractableTimer;

	private bool m_isHighlighted;

	private bool m_currentlyHasOutlines = true;

	private bool m_playerFacingNPC;

	private bool m_playerInsideApproachDistance;

	private bool m_coopPlayerInsideApproachDistance;

	private int m_numTimesSpokenTo;

	private RoomHandler m_parentRoom;

	private bool m_hasZombieTextBox;

	private float m_zombieBoxTimer;

	private string m_zombieBoxTalkAnim;

	[NonSerialized]
	public bool SuppressClear;

	private int m_clearTextFrameCountdown = -1;

	private bool m_collidedWithPlayerLastFrame;

	private float m_collidedWithPlayerTimer;

	public float MovementSpeed = 3f;

	[EnumFlags]
	public CellTypes PathableTiles = CellTypes.FLOOR;

	[NonSerialized]
	public bool m_isReadyForRepath = true;

	[NonSerialized]
	private Path m_currentPath;

	[NonSerialized]
	private Vector2? m_overridePathEnd;

	private IntVector2? m_clearance;

	public bool IsDoingForcedSpeech;

	public TalkingState State
	{
		get
		{
			return m_talkingState;
		}
		set
		{
			if (!SuppressReinteractDelay && m_talkingState != 0 && value == TalkingState.None && IsInteractable)
			{
				IsInteractable = false;
				m_setInteractableTimer = 0.5f;
			}
			m_talkingState = value;
			UpdateOutlines();
		}
	}

	public bool IsTalking
	{
		get
		{
			return State != TalkingState.None;
		}
		set
		{
			State = (value ? TalkingState.Conversation : TalkingState.None);
		}
	}

	public bool IsPlayerInRange
	{
		get
		{
			return m_isPlayerInRange;
		}
		set
		{
			m_isPlayerInRange = value;
			UpdateOutlines();
		}
	}

	public bool ShowOutlines
	{
		get
		{
			return m_showOutlines;
		}
		set
		{
			m_showOutlines = value;
			UpdateOutlines();
		}
	}

	public bool AllowWalkAways
	{
		get
		{
			return m_allowWalkAways;
		}
		set
		{
			m_allowWalkAways = value;
		}
	}

	public bool IsInteractable
	{
		get
		{
			return m_isInteractable;
		}
		set
		{
			m_isInteractable = value;
			UpdateOutlines();
		}
	}

	public bool HasPlayerLocked
	{
		get
		{
			return m_hasPlayerLocked;
		}
		set
		{
			m_hasPlayerLocked = value;
			UpdateOutlines();
		}
	}

	public bool SuppressReinteractDelay { get; set; }

	public bool SuppressRoomEnterExitEvents { get; set; }

	public PlayerInputState CachedPlayerInput { get; set; }

	public PlayerController TalkingPlayer
	{
		get
		{
			return m_talkingPlayer;
		}
		set
		{
			if (value == null && m_talkingPlayer != null)
			{
				m_talkingPlayer.TalkPartner = null;
				m_talkingPlayer.IsTalking = false;
			}
			if (value != null && m_talkingPlayer == null)
			{
				value.TalkPartner = this;
				value.IsTalking = true;
			}
			if (value != null && m_talkingPlayer != null)
			{
				m_talkingPlayer.IsTalking = false;
				m_talkingPlayer.TalkPartner = null;
				value.IsTalking = true;
				value.TalkPartner = this;
			}
			m_talkingPlayer = value;
		}
	}

	public PlayerController CompletedTalkingPlayer { get; set; }

	public int NumTimesSpokenTo
	{
		get
		{
			return m_numTimesSpokenTo;
		}
	}

	public RoomHandler ParentRoom
	{
		get
		{
			if (m_parentRoom == null)
			{
				m_parentRoom = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			}
			return m_parentRoom;
		}
	}

	public AIActor HostileObject { get; set; }

	public bool IsPlayingZombieAnimation
	{
		get
		{
			return m_hasZombieTextBox && m_zombieBoxTimer > 0f && (bool)base.aiAnimator && base.aiAnimator.IsPlaying(m_zombieBoxTalkAnim);
		}
	}

	public Path CurrentPath
	{
		get
		{
			return m_currentPath;
		}
		set
		{
			m_currentPath = value;
		}
	}

	public IntVector2 PathTile
	{
		get
		{
			return base.specRigidbody.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		}
	}

	public IntVector2 Clearance
	{
		get
		{
			IntVector2? clearance = m_clearance;
			if (!clearance.HasValue)
			{
				m_clearance = base.specRigidbody.UnitDimensions.ToIntVector2(VectorConversions.Ceil);
			}
			return m_clearance.Value;
		}
	}

	public static void ClearPerLevelData()
	{
		StaticReferenceManager.AllNpcs.Clear();
	}

	private void Start()
	{
		if (shadow != null)
		{
			tk2dBaseSprite component = shadow.GetComponent<tk2dBaseSprite>();
			if ((bool)component && component.HeightOffGround >= -1f && component.GetCurrentSpriteDef().name == "rogue_shadow" && shadow.layer == LayerMask.NameToLayer("FG_Critical"))
			{
				component.HeightOffGround = -5f;
				component.UpdateZDepth();
			}
		}
		m_overheadUIElementDelay = OverheadUIElementDelay;
		StaticReferenceManager.AllNpcs.Add(this);
		if (base.aiActor != null && !base.aiActor.IsNormalEnemy && !RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Add(this);
		}
		if (base.specRigidbody != null)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, OutlineDepth, OutlineLuminanceCutoff);
		m_parentRoom = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		if (AllowPlayerToPassEventually && base.specRigidbody != null && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandlePlayerTemporaryIncorporeality));
		}
		if (m_parentRoom != null)
		{
			m_parentRoom.Entered += PlayerEnteredRoom;
			m_parentRoom.Exited += PlayerExitedRoom;
			if ((bool)OptionalMinimapIcon)
			{
				Minimap.Instance.RegisterRoomIcon(m_parentRoom, OptionalMinimapIcon);
			}
		}
		if (IsPaletteSwapped)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.SetTexture("_PaletteTex", PaletteTexture);
		}
		if (DisableOnShortcutRun && GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
		{
			IsInteractable = false;
			SetNpcVisibility.SetVisible(this, false);
			ShowOutlines = false;
		}
		if ((bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		}
		for (int i = 0; i < base.playmakerFsms.Length; i++)
		{
			PlayMakerFSM playMakerFSM = base.playmakerFsms[i];
			if (!playMakerFSM || playMakerFSM.Fsm == null)
			{
				continue;
			}
			for (int j = 0; j < playMakerFSM.Fsm.States.Length; j++)
			{
				FsmState fsmState = playMakerFSM.Fsm.States[j];
				for (int k = 0; k < fsmState.Actions.Length; k++)
				{
					FsmStateAction fsmStateAction = fsmState.Actions[k];
					if (!(fsmStateAction is DialogueBox))
					{
						continue;
					}
					DialogueBox dialogueBox = fsmStateAction as DialogueBox;
					if (dialogueBox.AlternativeTalker != null)
					{
						TalkDoerLite instanceReference = GetInstanceReference(dialogueBox.AlternativeTalker);
						if ((bool)instanceReference)
						{
							dialogueBox.AlternativeTalker = instanceReference;
						}
					}
				}
			}
		}
	}

	private TalkDoerLite GetInstanceReference(TalkDoerLite prefab)
	{
		if (!prefab)
		{
			return null;
		}
		for (int i = 0; i < ReassignPrefabReferences.Count; i++)
		{
			GameObject gameObject = ReassignPrefabReferences[i];
			if (gameObject.name.StartsWith(prefab.name))
			{
				TalkDoerLite component = gameObject.GetComponent<TalkDoerLite>();
				if ((bool)component)
				{
					return component;
				}
			}
		}
		return null;
	}

	private void OnEnable()
	{
		if ((bool)this && (bool)speakPoint)
		{
			TextBoxManager.ClearTextBoxImmediate(speakPoint);
		}
	}

	private void HandlePlayerTemporaryIncorporeality(CollisionData rigidbodyCollision)
	{
		if (rigidbodyCollision.OtherRigidbody.GetComponent<PlayerController>() != null)
		{
			m_collidedWithPlayerLastFrame = true;
			m_collidedWithPlayerTimer += BraveTime.DeltaTime;
			if (m_collidedWithPlayerTimer > 1f)
			{
				base.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 0.25f);
			}
		}
	}

	public void ConvertToGhost()
	{
		if ((bool)base.sprite && (bool)base.sprite.renderer)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = ShaderCache.Acquire(PlayerController.DefaultShaderName);
			base.sprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
			base.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
			base.sprite.renderer.material.SetColor("_FlatColor", new Color(0.2f, 0.25f, 0.67f, 1f));
			base.sprite.renderer.material.SetVector("_SpecialFlags", new Vector4(1f, 0f, 0f, 0f));
		}
	}

	private void Update()
	{
		if (IsTalking && GameManager.Instance.IsLoadingLevel)
		{
			EndConversation.ForceEndConversation(this);
		}
		m_collidedWithPlayerLastFrame = false;
		if (m_setInteractableTimer > 0f)
		{
			if (IsInteractable)
			{
				m_setInteractableTimer = -1f;
			}
			else
			{
				m_setInteractableTimer -= BraveTime.DeltaTime;
				if (m_setInteractableTimer <= 0f)
				{
					IsInteractable = true;
				}
			}
		}
		if (AllowWalkAways && m_talkingState == TalkingState.Conversation && Vector2.Distance(TalkingPlayer.sprite.WorldCenter, base.sprite.WorldCenter) > conversationBreakRadius)
		{
			SendPlaymakerEvent("playerWalkedAway");
		}
		if (CompletedTalkingPlayer != null && !IsTalking && Vector2.Distance(CompletedTalkingPlayer.sprite.WorldCenter, base.sprite.WorldCenter) > conversationBreakRadius)
		{
			SendPlaymakerEvent("playerWalkedAwayPolitely");
			CompletedTalkingPlayer = null;
		}
		if (m_hasZombieTextBox && m_zombieBoxTimer > 0f)
		{
			m_zombieBoxTimer -= BraveTime.DeltaTime;
			if (m_zombieBoxTimer <= 0f)
			{
				CloseTextBox(true);
			}
		}
		if (IsPlayerInRange)
		{
			if (m_overheadUIElementDelay > 0f)
			{
				m_overheadUIElementDelay -= BraveTime.DeltaTime;
				if (m_overheadUIElementDelay <= 0f)
				{
					CreateOverheadUI();
				}
			}
		}
		else if (m_overheadUIElementDelay < OverheadUIElementDelay)
		{
			m_overheadUIElementDelay += BraveTime.DeltaTime;
		}
		if (GameManager.Instance.IsPaused && m_extantOverheadUIElement != null)
		{
			if (m_extantOverheadUIElement.IsVisible)
			{
				m_extantOverheadUIElement.IsVisible = false;
				tk2dBaseSprite[] componentsInChildren = m_extantOverheadUIElement.GetComponentsInChildren<tk2dBaseSprite>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].renderer.enabled = false;
				}
			}
		}
		else if (m_extantOverheadUIElement != null && !m_extantOverheadUIElement.IsVisible)
		{
			m_extantOverheadUIElement.IsVisible = true;
			tk2dBaseSprite[] componentsInChildren2 = m_extantOverheadUIElement.GetComponentsInChildren<tk2dBaseSprite>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].renderer.enabled = true;
			}
		}
		if ((bool)GameManager.Instance && (bool)GameManager.Instance.PrimaryPlayer && !GameManager.Instance.PrimaryPlayer.IsStealthed)
		{
			bool flag = Vector2.Distance(base.specRigidbody.UnitCenter, GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter) < playerApproachRadius;
			if (!m_playerInsideApproachDistance && flag)
			{
				SendPlaymakerEvent("playerApproached");
			}
			else if (m_playerInsideApproachDistance && !flag)
			{
				SendPlaymakerEvent("playerUnapproached");
			}
			m_playerInsideApproachDistance = flag;
		}
		if ((bool)GameManager.Instance && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer && !GameManager.Instance.SecondaryPlayer.IsStealthed)
		{
			bool flag2 = Vector2.Distance(base.specRigidbody.UnitCenter, GameManager.Instance.SecondaryPlayer.specRigidbody.UnitCenter) < playerApproachRadius;
			if (!m_coopPlayerInsideApproachDistance && flag2)
			{
				SendPlaymakerEvent("coopPlayerApproached");
			}
			else if (m_coopPlayerInsideApproachDistance && !flag2)
			{
				SendPlaymakerEvent("coopPlayerUnapproached");
			}
			m_coopPlayerInsideApproachDistance = flag2;
		}
		if ((bool)GameManager.Instance && (bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.CurrentRoom == m_parentRoom)
		{
			Vector2 zero = Vector2.zero;
			zero = ((!(GameManager.Instance.PrimaryPlayer.CurrentGun == null) && !GameManager.Instance.PrimaryPlayer.inventory.ForceNoGun) ? ((Vector2)(GameManager.Instance.PrimaryPlayer.unadjustedAimPoint - GameManager.Instance.PrimaryPlayer.LockedApproximateSpriteCenter)) : GameManager.Instance.PrimaryPlayer.NonZeroLastCommandedDirection);
			float num = Vector2.Dot(rhs: (base.specRigidbody.UnitCenter - GameManager.Instance.PrimaryPlayer.LockedApproximateSpriteCenter.XY()).normalized, lhs: zero.normalized);
			bool flag3 = num > -0.25f || ForcePlayerLookAt;
			if (!m_playerFacingNPC && flag3)
			{
				SendPlaymakerEvent("playerStartedFacing");
			}
			else if (m_playerFacingNPC && !flag3)
			{
				SendPlaymakerEvent("playerStoppedFacing");
			}
			m_playerFacingNPC = flag3;
		}
	}

	protected void LateUpdate()
	{
		if (!m_collidedWithPlayerLastFrame)
		{
			m_collidedWithPlayerTimer = 0f;
		}
		if (!SuppressClear && m_clearTextFrameCountdown > 0)
		{
			m_clearTextFrameCountdown--;
			if (m_clearTextFrameCountdown <= 0)
			{
				TextBoxManager.ClearTextBox(speakPoint);
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)this && (bool)speakPoint)
		{
			TextBoxManager.ClearTextBoxImmediate(speakPoint);
		}
		if (m_extantOverheadUIElement != null)
		{
			UnityEngine.Object.Destroy(m_extantOverheadUIElement.gameObject);
			m_extantOverheadUIElement = null;
		}
	}

	protected override void OnDestroy()
	{
		if (m_parentRoom != null)
		{
			m_parentRoom.Entered -= PlayerEnteredRoom;
			m_parentRoom.Exited -= PlayerExitedRoom;
		}
		StaticReferenceManager.AllNpcs.Remove(this);
		base.OnDestroy();
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!this || !IsInteractable)
		{
			return 1000f;
		}
		if (ForceNonInteractable)
		{
			return 1000f;
		}
		if (PreventInteraction)
		{
			return 1000f;
		}
		if (!base.gameObject.activeSelf)
		{
			return 1000f;
		}
		if ((bool)base.aiActor && GameManager.Instance.BestActivePlayer.IsInCombat)
		{
			return 1000f;
		}
		if (usesOverrideInteractionRegion)
		{
			return BraveMathCollege.DistToRectangle(point, base.transform.position.XY() + overrideRegionOffset * 0.0625f, overrideRegionDimensions * 0.0625f);
		}
		float num = 1000f;
		if ((bool)base.specRigidbody)
		{
			PixelCollider primaryPixelCollider = base.specRigidbody.PrimaryPixelCollider;
			return BraveMathCollege.DistToRectangle(point, primaryPixelCollider.UnitBottomLeft, primaryPixelCollider.UnitDimensions);
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.center += base.sprite.transform.position;
		return BraveMathCollege.DistToRectangle(point, bounds.min, bounds.size);
	}

	public float GetOverrideMaxDistance()
	{
		return overrideInteractionRadius;
	}

	private void CreateOverheadUI()
	{
		if (IsTalking || !(OverheadUIElementOnPreInteract != null) || !(m_extantOverheadUIElement == null))
		{
			return;
		}
		m_extantOverheadUIElement = GameUIRoot.Instance.Manager.AddPrefab(OverheadUIElementOnPreInteract);
		FoyerInfoPanelController component = m_extantOverheadUIElement.GetComponent<FoyerInfoPanelController>();
		if ((bool)component)
		{
			component.followTransform = base.transform;
			if (component.characterIdentity != PlayableCharacters.CoopCultist)
			{
				component.offset = new Vector3(0.75f, 1.625f, 0f);
			}
			else
			{
				component.offset = new Vector3(0.75f, 2.25f, 0f);
			}
		}
	}

	private void DestroyOverheadUI()
	{
		if (OverheadUIElementOnPreInteract != null && m_extantOverheadUIElement != null)
		{
			UnityEngine.Object.Destroy(m_extantOverheadUIElement.gameObject);
			m_extantOverheadUIElement = null;
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		IsPlayerInRange = true;
		UpdateOutlines();
	}

	public void OnExitRange(PlayerController interactor)
	{
		DestroyOverheadUI();
		IsPlayerInRange = false;
		UpdateOutlines();
	}

	public void Interact(PlayerController interactor)
	{
		if ((!interactor.IsPrimaryPlayer && PreventCoopInteraction) || !IsInteractable || m_talkingState == TalkingState.Conversation || (GameManager.Instance.IsFoyer && interactor.WasTalkingThisFrame))
		{
			return;
		}
		if (GameManager.Instance.IsFoyer)
		{
			FoyerCharacterSelectFlag component = GetComponent<FoyerCharacterSelectFlag>();
			if ((bool)component && !component.CanBeSelected())
			{
				AkSoundEngine.PostEvent("Play_UI_menu_cancel_01", base.gameObject);
				return;
			}
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			GameManager.Instance.LastUsedInputDeviceForConversation = BraveInput.GetInstanceForPlayer(interactor.PlayerIDX).ActiveActions.Device;
		}
		if (m_extantOverheadUIElement != null)
		{
			UnityEngine.Object.Destroy(m_extantOverheadUIElement.gameObject);
			m_extantOverheadUIElement = null;
		}
		TalkingPlayer = interactor;
		EncounterTrackable component2 = GetComponent<EncounterTrackable>();
		if (m_numTimesSpokenTo == 0 && component2 != null)
		{
			GameStatsManager.Instance.HandleEncounteredObject(component2);
		}
		m_numTimesSpokenTo++;
		SendPlaymakerEvent("playerInteract");
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	private void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (m_talkingState != TalkingState.Conversation && !IsTalking)
		{
			SpeculativeRigidbody otherRigidbody = rigidbodyCollision.OtherRigidbody;
			if ((bool)otherRigidbody.projectile && otherRigidbody.projectile.Owner is PlayerController)
			{
				SendPlaymakerEvent("takePlayerDamage");
			}
		}
	}

	private void PlayerEnteredRoom(PlayerController p)
	{
		if (!p.IsStealthed && !SuppressRoomEnterExitEvents)
		{
			SendPlaymakerEvent("playerEnteredRoom");
		}
	}

	private void PlayerExitedRoom()
	{
		if ((!GameManager.Instance.PrimaryPlayer || !GameManager.Instance.PrimaryPlayer.IsStealthed) && !SuppressRoomEnterExitEvents)
		{
			SendPlaymakerEvent("playerExitedRoom");
		}
	}

	protected void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (frame.eventOutline != 0)
		{
			if (frame.eventOutline == tk2dSpriteAnimationFrame.OutlineModifier.TurnOn)
			{
				ShowOutlines = true;
			}
			else if (frame.eventOutline == tk2dSpriteAnimationFrame.OutlineModifier.TurnOff)
			{
				ShowOutlines = false;
			}
		}
	}

	public void SetZombieBoxTimer(float timer, string talkAnim)
	{
		m_hasZombieTextBox = true;
		m_zombieBoxTimer = timer;
		m_zombieBoxTalkAnim = talkAnim;
	}

	public void ShowText(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, TextBoxManager.BoxSlideOrientation slideOrientation = TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, bool showContinueText = false, bool isThoughtBox = false, string overrideSpeechAudioTag = null)
	{
		m_hasZombieTextBox = false;
		m_zombieBoxTimer = 0f;
		m_clearTextFrameCountdown = -1;
		if (isThoughtBox)
		{
			string overrideAudioTag = ((overrideSpeechAudioTag == null) ? (audioCharacterSpeechTag ?? string.Empty) : overrideSpeechAudioTag);
			TextBoxManager.ShowThoughtBubble(worldPosition, parent, duration, text, instant, showContinueText, overrideAudioTag);
		}
		else
		{
			string audioTag = ((overrideSpeechAudioTag == null) ? (audioCharacterSpeechTag ?? string.Empty) : overrideSpeechAudioTag);
			TextBoxManager.ShowTextBox(worldPosition, parent, duration, text, audioTag, instant, slideOrientation, showContinueText, SpeaksGleepGlorpenese);
		}
	}

	public void CloseTextBox(bool overrideZombieBoxes)
	{
		if (overrideZombieBoxes)
		{
			m_hasZombieTextBox = false;
			m_zombieBoxTimer = 0f;
			if ((bool)base.aiAnimator)
			{
				base.aiAnimator.EndAnimationIf(m_zombieBoxTalkAnim);
			}
		}
		if (!m_hasZombieTextBox)
		{
			m_clearTextFrameCountdown = 2;
		}
	}

	private void UpdateOutlines()
	{
		bool flag = IsInteractable && State != TalkingState.Conversation && IsPlayerInRange && !HasPlayerLocked;
		if (flag != m_isHighlighted || m_currentlyHasOutlines != ShowOutlines)
		{
			m_isHighlighted = flag;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			if (ShowOutlines)
			{
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, (!m_isHighlighted) ? Color.black : Color.white, OutlineDepth, (!m_isHighlighted) ? OutlineLuminanceCutoff : 0f);
			}
			base.sprite.UpdateZDepth();
			m_currentlyHasOutlines = ShowOutlines;
		}
	}

	public Vector2 GetPathVelocityContribution(Vector2 lastPosition, int distanceThresholdPixels = 1)
	{
		if (m_currentPath == null || m_currentPath.Count == 0)
		{
			Vector2? overridePathEnd = m_overridePathEnd;
			if (!overridePathEnd.HasValue)
			{
				return Vector2.zero;
			}
		}
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 vector = ((m_currentPath == null) ? m_overridePathEnd.Value : m_currentPath.GetFirstCenterVector2());
		int num = ((m_currentPath != null) ? m_currentPath.Count : 0);
		Vector2? overridePathEnd2 = m_overridePathEnd;
		bool flag = num + (overridePathEnd2.HasValue ? 1 : 0) == 1;
		bool flag2 = false;
		int pixel = ((!flag) ? 1 : distanceThresholdPixels);
		if (Vector2.Distance(unitCenter, vector) < PhysicsEngine.PixelToUnit(pixel))
		{
			flag2 = true;
		}
		else if (!flag)
		{
			Vector2 b = BraveMathCollege.ClosestPointOnLineSegment(vector, lastPosition, unitCenter);
			if (Vector2.Distance(vector, b) < PhysicsEngine.PixelToUnit(pixel))
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			if (m_currentPath != null && m_currentPath.Count > 0)
			{
				m_currentPath.RemoveFirst();
				if (m_currentPath.Count == 0)
				{
					m_currentPath = null;
					return Vector2.zero;
				}
			}
			else
			{
				Vector2? overridePathEnd3 = m_overridePathEnd;
				if (overridePathEnd3.HasValue)
				{
					m_overridePathEnd = null;
				}
			}
		}
		Vector2 result = vector - unitCenter;
		if (flag && MovementSpeed > result.magnitude)
		{
			return result;
		}
		return MovementSpeed * result.normalized;
	}

	public void PathfindToPosition(Vector2 targetPosition, Vector2? overridePathEnd = null, CellValidator cellValidator = null)
	{
		Path path = null;
		if (Pathfinder.Instance.GetPath(PathTile, targetPosition.ToIntVector2(VectorConversions.Floor), out path, Clearance, PathableTiles, cellValidator))
		{
			m_currentPath = path;
			m_overridePathEnd = overridePathEnd;
			if (m_currentPath.Count == 0)
			{
				m_currentPath = null;
			}
			else
			{
				path.Smooth(base.specRigidbody.UnitCenter, base.specRigidbody.UnitDimensions / 2f, PathableTiles, false, Clearance);
			}
		}
	}

	public void ForceTimedSpeech(string words, float initialDelay, float duration, TextBoxManager.BoxSlideOrientation slideOrientation)
	{
		Debug.Log("starting forced timed speech: " + words);
		StartCoroutine(HandleForcedTimedSpeech(words, initialDelay, duration, slideOrientation));
	}

	private IEnumerator HandleForcedTimedSpeech(string words, float initialDelay, float duration, TextBoxManager.BoxSlideOrientation slideOrientation)
	{
		IsDoingForcedSpeech = true;
		while (initialDelay > 0f)
		{
			initialDelay -= BraveTime.DeltaTime;
			if (!IsDoingForcedSpeech)
			{
				Debug.Log("breaking forced timed speech: " + words);
				yield break;
			}
			yield return null;
		}
		TextBoxManager.ClearTextBox(speakPoint);
		base.aiAnimator.PlayUntilCancelled("talk");
		if (string.IsNullOrEmpty(audioCharacterSpeechTag))
		{
			TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -4f), speakPoint, -1f, words, string.Empty, true, slideOrientation, false, SpeaksGleepGlorpenese);
		}
		else
		{
			TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -4f), speakPoint, -1f, words, audioCharacterSpeechTag, false, slideOrientation, false, SpeaksGleepGlorpenese);
		}
		if (duration > 0f)
		{
			while (duration > 0f && IsDoingForcedSpeech)
			{
				duration -= BraveTime.DeltaTime;
				yield return null;
			}
		}
		else
		{
			while (IsDoingForcedSpeech)
			{
				yield return null;
			}
		}
		Debug.Log("ending forced timed speech: " + words);
		TextBoxManager.ClearTextBox(speakPoint);
		base.aiAnimator.EndAnimation();
		IsDoingForcedSpeech = false;
	}
}
