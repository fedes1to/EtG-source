using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DungeonDoorController : PersistentVFXManagerBehaviour, IPlayerInteractable
{
	public enum DungeonDoorMode
	{
		COMPLEX,
		BOSS_DOOR_ONLY_UNSEALS,
		SINGLE_DOOR,
		ONE_WAY_DOOR_ONLY_UNSEALS,
		FINAL_BOSS_DOOR
	}

	[Serializable]
	public class DoorModule
	{
		public tk2dSpriteAnimator animator;

		public float openDepth;

		public float closedDepth;

		public bool openPerpendicular = true;

		public bool horizontalFlips = true;

		public string openAnimationName;

		public string closeAnimationName;

		public List<tk2dSpriteAnimator> AOAnimatorsToDisable;

		[HideInInspector]
		public tk2dSprite sprite;

		[HideInInspector]
		public SpeculativeRigidbody rigidbody;

		[NonSerialized]
		[HideInInspector]
		public bool isLerping;
	}

	[SerializeField]
	protected DungeonDoorMode doorMode;

	[SerializeField]
	protected bool doorClosesAfterEveryOpen;

	[NonSerialized]
	private bool hasEverBeenOpen;

	public DoorModule[] doorModules;

	public bool hideSealAnimators = true;

	public tk2dSpriteAnimator[] sealAnimators;

	public tk2dSpriteAnimator[] sealChainAnimators;

	public tk2dSpriteAnimator[] sealVFX;

	public float unsealDistanceMaximum = -1f;

	public GameObject unsealedVFXOverride;

	public string sealAnimationName;

	public string unsealAnimationName;

	public string playerNearSealedAnimationName;

	public bool SupportsSubsidiaryDoors = true;

	public bool northSouth = true;

	[NonSerialized]
	public RuntimeExitDefinition exitDefinition;

	[NonSerialized]
	public RoomHandler upstreamRoom;

	[NonSerialized]
	public RoomHandler downstreamRoom;

	[NonSerialized]
	public bool OneWayDoor;

	[HideInInspector]
	public DungeonDoorSubsidiaryBlocker subsidiaryBlocker;

	[HideInInspector]
	public DungeonDoorController subsidiaryDoor;

	[HideInInspector]
	public DungeonDoorController parentDoor;

	[NonSerialized]
	public Transform messageTransformPoint;

	[NonSerialized]
	public string messageToDisplay;

	public tk2dSpriteAnimator LockAnimator;

	public tk2dSpriteAnimator ChainsAnimator;

	private bool m_open;

	public bool isLocked;

	public bool lockIsBusted;

	[SerializeField]
	private bool isSealed;

	private bool m_openIsFlipped;

	public bool usesUnsealScreenShake;

	public ScreenShakeSettings unsealScreenShake;

	private bool m_isDestroyed;

	private bool m_wasOpenWhenSealed;

	private bool m_lockHasApproached;

	private bool m_lockHasLaughed;

	private bool m_lockHasSpit;

	private bool m_finalBossDoorHasOpened;

	private bool m_isCoopArrowing;

	private bool m_hasGC;

	public bool IsUniqueVisiblityDoor
	{
		get
		{
			return hasEverBeenOpen && doorClosesAfterEveryOpen;
		}
	}

	public DungeonDoorMode Mode
	{
		get
		{
			return doorMode;
		}
	}

	public bool IsOpen
	{
		get
		{
			if (doorMode == DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || doorMode == DungeonDoorMode.FINAL_BOSS_DOOR)
			{
				return true;
			}
			if (doorMode == DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
			{
				return true;
			}
			return m_open;
		}
	}

	public bool IsOpenForVisibilityTest
	{
		get
		{
			if (doorMode == DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || doorMode == DungeonDoorMode.FINAL_BOSS_DOOR)
			{
				return true;
			}
			if (doorMode == DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
			{
				return true;
			}
			if (IsSealed)
			{
				return false;
			}
			if (IsUniqueVisiblityDoor)
			{
				return true;
			}
			return m_open;
		}
	}

	public bool IsSealed
	{
		get
		{
			return isSealed;
		}
		set
		{
			if (value && m_open)
			{
				Close();
			}
			if (isSealed != value)
			{
				if (value)
				{
					SealInternal();
				}
				else
				{
					UnsealInternal();
				}
			}
		}
	}

	public bool KeepBossDoorSealed { get; set; }

	public void SetSealedSilently(bool v)
	{
		isSealed = v;
	}

	public void DoSeal(RoomHandler sourceRoom)
	{
		if (doorMode == DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
		{
			if (subsidiaryDoor != null)
			{
				subsidiaryDoor.SealInternal();
			}
			if (subsidiaryBlocker != null)
			{
				subsidiaryBlocker.Seal();
			}
		}
		else if (subsidiaryBlocker != null || subsidiaryDoor != null)
		{
			if (exitDefinition.upstreamExit.jointedExit)
			{
				if (((exitDefinition.upstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH || exitDefinition.upstreamExit.referencedExit.exitDirection == DungeonData.Direction.SOUTH) && exitDefinition.upstreamRoom == sourceRoom) || ((exitDefinition.downstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH || exitDefinition.downstreamExit.referencedExit.exitDirection == DungeonData.Direction.SOUTH) && exitDefinition.downstreamRoom == sourceRoom))
				{
					SealInternal();
					return;
				}
				if (subsidiaryDoor != null)
				{
					subsidiaryDoor.SealInternal();
				}
				if (subsidiaryBlocker != null)
				{
					subsidiaryBlocker.Seal();
				}
			}
			else if (sourceRoom == exitDefinition.upstreamRoom)
			{
				SealInternal();
			}
			else
			{
				if (subsidiaryDoor != null)
				{
					subsidiaryDoor.SealInternal();
				}
				if (subsidiaryBlocker != null)
				{
					subsidiaryBlocker.Seal();
				}
			}
		}
		else
		{
			SealInternal();
		}
	}

	public void AssignPressurePlate(PressurePlate source)
	{
		source.OnPressurePlateDepressed = (Action<PressurePlate>)Delegate.Combine(source.OnPressurePlateDepressed, new Action<PressurePlate>(OnPressurePlateTriggered));
	}

	private void OnPressurePlateTriggered(PressurePlate source)
	{
		source.OnPressurePlateDepressed = (Action<PressurePlate>)Delegate.Remove(source.OnPressurePlateDepressed, new Action<PressurePlate>(OnPressurePlateTriggered));
		DoUnseal(downstreamRoom);
	}

	public void DoUnseal(RoomHandler sourceRoom)
	{
		if (subsidiaryDoor != null && subsidiaryDoor.isSealed)
		{
			subsidiaryDoor.UnsealInternal();
		}
		if (subsidiaryBlocker != null && subsidiaryBlocker.isSealed)
		{
			subsidiaryBlocker.Unseal();
		}
		if (isSealed)
		{
			UnsealInternal();
		}
	}

	private void Start()
	{
		if (doorMode != DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS && doorMode != DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS && doorMode != DungeonDoorMode.FINAL_BOSS_DOOR)
		{
			for (int i = 0; i < doorModules.Length; i++)
			{
				tk2dSprite component = doorModules[i].animator.GetComponent<tk2dSprite>();
				component.depthUsesTrimmedBounds = true;
				SpeculativeRigidbody component2 = doorModules[i].animator.GetComponent<SpeculativeRigidbody>();
				component2.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(component2.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
				component2.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component2.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
				doorModules[i].sprite = component;
				doorModules[i].rigidbody = component2;
				tk2dSpriteAnimator animator = doorModules[i].animator;
				animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
			}
			UpdateDoorDepths();
		}
		if (doorMode == DungeonDoorMode.COMPLEX && !northSouth)
		{
			for (int j = 0; j < sealAnimators.Length; j++)
			{
				if (sealAnimators[j].sprite.attachParent != null)
				{
					sealAnimators[j].sprite.attachParent.DetachRenderer(sealAnimators[j].sprite);
				}
				sealAnimators[j].sprite.automaticallyManagesDepth = true;
				sealAnimators[j].sprite.attachParent = null;
				sealAnimators[j].sprite.depthUsesTrimmedBounds = false;
				sealAnimators[j].sprite.HeightOffGround = 0f;
			}
		}
		if (doorMode == DungeonDoorMode.FINAL_BOSS_DOOR)
		{
			IntVector2 intVector = base.transform.position.IntXY() + new IntVector2(-2, -1);
			for (int k = 0; k < 8; k++)
			{
				for (int l = 0; l < 6; l++)
				{
					IntVector2 item = intVector + new IntVector2(l, k);
					if (upstreamRoom != null)
					{
						upstreamRoom.FeatureCells.Add(item);
					}
				}
			}
		}
		if (sealAnimators != null)
		{
			for (int m = 0; m < sealAnimators.Length; m++)
			{
				sealAnimators[m].alwaysUpdateOffscreen = true;
			}
		}
		if (exitDefinition != null && exitDefinition.upstreamExit != null && exitDefinition.upstreamExit.isLockedDoor)
		{
			isLocked = true;
		}
		if (isLocked && parentDoor != null && parentDoor.subsidiaryDoor == this)
		{
			isLocked = false;
		}
		if (isLocked)
		{
			if (LockAnimator == null)
			{
				BecomeLockedDoor();
			}
			RoomHandler.unassignedInteractableObjects.Add(this);
		}
		SpeculativeRigidbody[] componentsInChildren = GetComponentsInChildren<SpeculativeRigidbody>();
		for (int n = 0; n < componentsInChildren.Length; n++)
		{
			componentsInChildren[n].PreventPiercing = true;
		}
	}

	public void ForceBecomeLockedDoor()
	{
		if (isLocked)
		{
			if (LockAnimator == null)
			{
				BecomeLockedDoor();
			}
			RoomHandler.unassignedInteractableObjects.Add(this);
		}
	}

	protected void BecomeLockedDoor()
	{
		if (doorMode != 0)
		{
			return;
		}
		if (!northSouth)
		{
			GameObject original = (GameObject)BraveResources.Load("Global Prefabs/DoorLockPrefab_Horizontal");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			float x = 0f;
			if ((bool)doorModules[0].animator && (bool)doorModules[0].animator.Sprite)
			{
				x = doorModules[0].animator.transform.localPosition.x + doorModules[0].animator.Sprite.GetBounds().max.x;
			}
			else if ((bool)doorModules[0].sprite)
			{
				x = doorModules[0].sprite.transform.localPosition.x + doorModules[0].sprite.GetBounds().max.x;
			}
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = new Vector3(x, 0f, 0f);
			LockAnimator = gameObject.GetComponent<tk2dSpriteAnimator>();
			ChainsAnimator = gameObject.transform.GetChild(0).GetComponent<tk2dSpriteAnimator>();
		}
		else
		{
			GameObject original2 = (GameObject)BraveResources.Load("Global Prefabs/DoorLockPrefab_Vertical");
			GameObject gameObject2 = UnityEngine.Object.Instantiate(original2);
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.localPosition = new Vector3(0f, -0.75f, 0f);
			LockAnimator = gameObject2.GetComponent<tk2dSpriteAnimator>();
			ChainsAnimator = gameObject2.transform.GetChild(0).GetComponent<tk2dSpriteAnimator>();
		}
		LockAnimator.sprite.UpdateZDepth();
		ChainsAnimator.sprite.UpdateZDepth();
		if (!northSouth)
		{
			LockAnimator.sprite.IsPerpendicular = true;
			ChainsAnimator.sprite.UpdateZDepth();
		}
		if (!northSouth && exitDefinition.upstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST)
		{
			FlipLockToOtherSide();
		}
	}

	protected void UpdateDoorDepths()
	{
		for (int i = 0; i < doorModules.Length; i++)
		{
			if (!doorModules[i].isLerping)
			{
				float num = ((!m_open) ? doorModules[i].closedDepth : doorModules[i].openDepth);
				if (!northSouth && !doorModules[i].sprite.depthUsesTrimmedBounds)
				{
					num = -5.25f;
				}
				if (doorModules[i].sprite.HeightOffGround != num)
				{
					AnimationDepthLerp(doorModules[i].sprite, num, null, doorModules[i], !northSouth && i == 0);
				}
			}
		}
	}

	private void Update()
	{
		if (isSealed && (doorMode == DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || (!m_finalBossDoorHasOpened && doorMode == DungeonDoorMode.FINAL_BOSS_DOOR)))
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (!GameManager.Instance.AllPlayers[i] || !GameManager.Instance.AllPlayers[i].healthHaver || GameManager.Instance.AllPlayers[i].healthHaver.IsDead || GameManager.Instance.PreventPausing || GameManager.Instance.AllPlayers[i].CurrentRoom == null || (GameManager.Instance.AllPlayers[i].CurrentRoom != upstreamRoom && GameManager.Instance.AllPlayers[i].CurrentRoom != downstreamRoom) || !GameManager.Instance.AllPlayers[i].CurrentRoom.UnsealConditionsMet() || (!(unsealDistanceMaximum <= 0f) && !(Vector2.Distance(sealAnimators[0].Sprite.WorldCenter, GameManager.Instance.AllPlayers[i].specRigidbody.UnitCenter) < unsealDistanceMaximum)))
				{
					continue;
				}
				if (doorMode == DungeonDoorMode.FINAL_BOSS_DOOR)
				{
					bool flag = false;
					if (GameManager.Instance.AllPlayers[i].CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
					{
						List<AIActor> activeEnemies = GameManager.Instance.AllPlayers[i].CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
						for (int j = 0; j < activeEnemies.Count; j++)
						{
							if ((bool)activeEnemies[j] && !activeEnemies[j].IgnoreForRoomClear && activeEnemies[j].HasBeenEngaged && activeEnemies[j].IsNormalEnemy)
							{
								flag = true;
							}
						}
					}
					if (!flag)
					{
						m_finalBossDoorHasOpened = true;
						UnsealInternal();
					}
				}
				else if (doorMode != DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || !KeepBossDoorSealed)
				{
					UnsealInternal();
				}
			}
		}
		if (isLocked && lockIsBusted)
		{
			string text = ((!northSouth) ? "lock_guy_side_busted" : "lock_guy_busted");
			if (!LockAnimator.IsPlaying(text))
			{
				LockAnimator.Play(text);
			}
		}
		else if (northSouth && isLocked)
		{
			Vector2 zero = Vector2.zero;
			for (int k = 0; k < doorModules.Length; k++)
			{
				zero += doorModules[k].rigidbody.UnitCenter;
			}
			zero /= (float)doorModules.Length;
			float num = Vector2.Distance(zero, GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter);
			if (!m_lockHasApproached && num < 2.5f)
			{
				LockAnimator.Play("lock_guy_approach");
				m_lockHasApproached = true;
			}
			else if (num > 2.5f)
			{
				if (m_lockHasLaughed)
				{
					LockAnimator.Play("lock_guy_spit");
				}
				m_lockHasLaughed = false;
				m_lockHasApproached = false;
			}
			if (!m_lockHasSpit && LockAnimator != null && LockAnimator.IsPlaying("lock_guy_spit") && LockAnimator.CurrentFrame == 3)
			{
				m_lockHasSpit = true;
				GameObject gameObject = SpawnManager.SpawnVFX(BraveResources.Load("Global VFX/VFX_Lock_Spit") as GameObject);
				tk2dSprite componentInChildren = gameObject.GetComponentInChildren<tk2dSprite>();
				componentInChildren.UpdateZDepth();
				componentInChildren.PlaceAtPositionByAnchor(LockAnimator.sprite.WorldCenter, tk2dBaseSprite.Anchor.UpperCenter);
			}
		}
		if (!northSouth || !isSealed)
		{
			return;
		}
		for (int l = 0; l < sealAnimators.Length; l++)
		{
			sealAnimators[l].sprite.UpdateZDepth();
		}
		if (string.IsNullOrEmpty(playerNearSealedAnimationName))
		{
			return;
		}
		Vector2 zero2 = Vector2.zero;
		for (int m = 0; m < doorModules.Length; m++)
		{
			zero2 += doorModules[m].rigidbody.UnitCenter;
		}
		zero2 /= (float)doorModules.Length;
		if (Vector2.Distance(zero2, GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter) < 4f)
		{
			for (int n = 0; n < sealAnimators.Length; n++)
			{
				if (!sealAnimators[n].IsPlaying(playerNearSealedAnimationName) && !sealAnimators[n].IsPlaying(unsealAnimationName) && !sealAnimators[n].IsPlaying(sealAnimationName))
				{
					sealAnimators[n].Play(playerNearSealedAnimationName);
				}
			}
			return;
		}
		for (int num2 = 0; num2 < sealAnimators.Length; num2++)
		{
			if (sealAnimators[num2].IsPlaying(playerNearSealedAnimationName))
			{
				sealAnimators[num2].Stop();
				tk2dSpriteAnimationClip clipByName = sealAnimators[num2].GetClipByName(sealAnimationName);
				sealAnimators[num2].Sprite.SetSprite(clipByName.frames[clipByName.frames.Length - 1].spriteId);
			}
		}
	}

	protected void AnimationDepthLerp(tk2dSprite targetSprite, float targetDepth, tk2dSpriteAnimationClip clip, DoorModule m = null, bool isSpecialHorizontalTopCase = false)
	{
		float duration = 1f;
		if (clip != null)
		{
			duration = (float)clip.frames.Length / clip.fps;
		}
		StartCoroutine(DepthLerp(targetSprite, targetDepth, duration, m, isSpecialHorizontalTopCase));
	}

	private IEnumerator DepthLerp(tk2dSprite targetSprite, float targetDepth, float duration, DoorModule m = null, bool isSpecialHorizontalTopCase = false)
	{
		if (m != null)
		{
			if (!m_open)
			{
				targetSprite.IsPerpendicular = true;
			}
			m.isLerping = true;
		}
		float elapsed = 0f;
		float startingDepth = targetSprite.HeightOffGround;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			targetSprite.HeightOffGround = Mathf.Lerp(startingDepth, targetDepth, t);
			if (m_open && isSpecialHorizontalTopCase)
			{
				targetSprite.depthUsesTrimmedBounds = false;
				targetSprite.HeightOffGround = -5.25f;
			}
			targetSprite.UpdateZDepth();
			yield return null;
		}
		targetSprite.HeightOffGround = (targetSprite.depthUsesTrimmedBounds ? targetDepth : (-5.25f));
		targetSprite.UpdateZDepth();
		if (m != null)
		{
			if (m_open)
			{
				targetSprite.IsPerpendicular = m.openPerpendicular;
			}
			m.isLerping = false;
		}
	}

	public void OnAnimationCompleted(tk2dSpriteAnimator a, tk2dSpriteAnimationClip c)
	{
		UpdateDoorDepths();
	}

	public void Open(bool flipped = false)
	{
		if (doorMode == DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || doorMode == DungeonDoorMode.FINAL_BOSS_DOOR || doorMode == DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS || IsSealed || isLocked || m_isDestroyed || m_open)
		{
			return;
		}
		if (!hasEverBeenOpen)
		{
			RoomHandler roomHandler = null;
			if (exitDefinition != null)
			{
				if (exitDefinition.upstreamRoom != null && exitDefinition.upstreamRoom.WillSealOnEntry())
				{
					roomHandler = exitDefinition.upstreamRoom;
				}
				else if (exitDefinition.downstreamRoom != null && exitDefinition.downstreamRoom.WillSealOnEntry())
				{
					roomHandler = exitDefinition.downstreamRoom;
				}
			}
			if (roomHandler != null && ((bool)subsidiaryDoor || (bool)parentDoor))
			{
				DungeonDoorController dungeonDoorController = ((!subsidiaryDoor) ? parentDoor : subsidiaryDoor);
				Vector2 center = roomHandler.area.Center;
				float num = Vector2.Distance(center, base.transform.position);
				float num2 = Vector2.Distance(center, dungeonDoorController.transform.position);
				if (num2 < num)
				{
					roomHandler = null;
				}
			}
			if (roomHandler != null)
			{
				BraveMemory.HandleRoomEntered(roomHandler.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All));
			}
		}
		AkSoundEngine.PostEvent("play_OBJ_door_open_01", base.gameObject);
		SetState(true, flipped);
		if (doorClosesAfterEveryOpen)
		{
			StartCoroutine(DelayedReclose());
		}
	}

	private IEnumerator DelayedReclose()
	{
		yield return new WaitForSeconds(0.1f);
		while (true)
		{
			bool containsPlayer = false;
			for (int i = 0; i < doorModules.Length; i++)
			{
				for (int j = 0; j < doorModules[i].rigidbody.PixelColliders.Count; j++)
				{
					List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(doorModules[i].rigidbody.PixelColliders[j]);
					for (int k = 0; k < overlappingRigidbodies.Count; k++)
					{
						if (overlappingRigidbodies[k].GetComponent<PlayerController>() != null)
						{
							containsPlayer = true;
							break;
						}
					}
					if (containsPlayer)
					{
						break;
					}
				}
				if (containsPlayer)
				{
					break;
				}
			}
			if (!containsPlayer)
			{
				break;
			}
			yield return null;
		}
		Close();
	}

	protected virtual void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		CheckForPlayerCollision(rigidbodyCollision.OtherRigidbody, rigidbodyCollision.Normal);
		if (IsSealed || !isLocked || !rigidbodyCollision.OtherRigidbody)
		{
			return;
		}
		if ((bool)rigidbodyCollision.OtherRigidbody.GetComponent<KeyProjModifier>())
		{
			Unlock();
		}
		if (rigidbodyCollision.OtherRigidbody.GetComponent<KeyBullet>() != null)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.DOORS_UNLOCKED_WITH_KEY_BULLETS, 1f);
			isLocked = false;
			bool flipped = false;
			if (rigidbodyCollision.Normal.y < 0f && northSouth)
			{
				flipped = true;
			}
			if (rigidbodyCollision.Normal.x < 0f && !northSouth)
			{
				flipped = true;
			}
			Open(flipped);
			m_isDestroyed = true;
		}
	}

	private void OnEnterTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		CheckForPlayerCollision(specRigidbody, sourceSpecRigidbody.UnitCenter - specRigidbody.UnitCenter);
	}

	private void CheckForPlayerCollision(SpeculativeRigidbody otherRigidbody, Vector2 normal)
	{
		if (isSealed || isLocked)
		{
			return;
		}
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if (!(component != null) || m_open)
		{
			return;
		}
		bool flipped = false;
		if (normal.y < 0f && northSouth)
		{
			flipped = true;
		}
		if (normal.x < 0f && !northSouth)
		{
			flipped = true;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
		{
			Open(flipped);
			BraveInput.DoVibrationForAllPlayers(Vibration.Time.Quick, Vibration.Strength.Light);
			return;
		}
		bool flag = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i].IsGhost && (!GameManager.Instance.AllPlayers[i].healthHaver.IsDead || GameManager.Instance.AllPlayers[i].IsGhost))
			{
				float distanceToPlayer = GetDistanceToPlayer(GameManager.Instance.AllPlayers[i].specRigidbody);
				if (distanceToPlayer > 0.3f)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			Open(flipped);
			BraveInput.DoVibrationForAllPlayers(Vibration.Time.Quick, Vibration.Strength.Light);
			if (exitDefinition != null && exitDefinition.downstreamRoom != null && ((exitDefinition.upstreamRoom != null && exitDefinition.upstreamRoom.WillSealOnEntry()) || (exitDefinition.downstreamRoom != null && exitDefinition.downstreamRoom.WillSealOnEntry())))
			{
				HandleCoopPlayers(flipped);
			}
		}
		else if (!m_isCoopArrowing)
		{
			StartCoroutine(DoCoopArrowWhilePlayerIsNear(component));
		}
	}

	private IEnumerator DoCoopArrowWhilePlayerIsNear(PlayerController nearPlayer)
	{
		if (m_isCoopArrowing)
		{
			yield break;
		}
		m_isCoopArrowing = true;
		while (!IsSealed && !IsOpen && !isLocked)
		{
			float playerDist = GetDistanceToPlayer(nearPlayer.specRigidbody);
			if (playerDist > 1f)
			{
				break;
			}
			if (nearPlayer.IsPrimaryPlayer)
			{
				GameManager.Instance.SecondaryPlayer.DoCoopArrow();
			}
			else
			{
				GameManager.Instance.PrimaryPlayer.DoCoopArrow();
			}
			yield return null;
		}
		m_isCoopArrowing = false;
	}

	private void HandleCoopPlayers(bool flipped)
	{
		if (GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero = (northSouth ? ((!flipped) ? (Vector2.up * 2.75f) : (-Vector2.up * 1.25f)) : ((!flipped) ? (Vector2.right * 1.25f) : (-Vector2.right * 1.25f)));
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if ((bool)GameManager.Instance.AllPlayers[i] && !GameManager.Instance.AllPlayers[i].IsGhost)
			{
				Vector2 vector = GetSRBAveragePosition() + zero;
				float initialDelay = ((!northSouth) ? 0.1f : 0.2f);
				List<SpeculativeRigidbody> list = new List<SpeculativeRigidbody>();
				for (int j = 0; j < doorModules.Length; j++)
				{
					list.Add(doorModules[j].sprite.specRigidbody);
				}
				for (int k = 0; k < sealAnimators.Length; k++)
				{
					list.Add(sealAnimators[k].sprite.specRigidbody);
				}
				GameManager.Instance.AllPlayers[i].ForceMoveInDirectionUntilThreshold(zero.normalized, (!northSouth) ? vector.x : vector.y, initialDelay, 1f, list);
			}
		}
	}

	public void Close()
	{
		if (doorMode != DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS && doorMode != DungeonDoorMode.FINAL_BOSS_DOOR && doorMode != DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS && !m_isDestroyed && m_open)
		{
			SetState(false);
		}
	}

	private IEnumerator MoveTransformSmoothly(Transform target, Vector3 delta, float animationTime, Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip> action)
	{
		float elapsed = 0f;
		Vector3 startPosition = target.position;
		Vector3 endPosition = target.position + delta;
		tk2dSprite targetSprite = target.GetComponent<tk2dSprite>();
		float startHeightOffGround = targetSprite.HeightOffGround;
		float endHeightOffGround = targetSprite.HeightOffGround + delta.y + delta.z;
		while (elapsed < animationTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / animationTime));
			Vector3 currentPosition = BraveUtility.QuantizeVector(Vector3.Lerp(startPosition, endPosition, t));
			targetSprite.HeightOffGround = Mathf.Lerp(startHeightOffGround, endHeightOffGround, t);
			target.position = currentPosition;
			targetSprite.UpdateZDepth();
			yield return null;
		}
		targetSprite.HeightOffGround = endHeightOffGround;
		target.position = endPosition;
		targetSprite.UpdateZDepth();
		if (action != null)
		{
			action(target.GetComponent<tk2dSpriteAnimator>(), null);
		}
	}

	private void SealInternal()
	{
		m_wasOpenWhenSealed = m_open;
		if (m_open)
		{
			Close();
		}
		if (Mode == DungeonDoorMode.FINAL_BOSS_DOOR)
		{
			sealAnimators[0].Play(sealAnimationName);
			sealAnimators[0].specRigidbody.PrimaryPixelCollider.Enabled = true;
		}
		else if (!string.IsNullOrEmpty(sealAnimationName))
		{
			for (int i = 0; i < sealAnimators.Length; i++)
			{
				sealAnimators[i].Sprite.UpdateZDepth();
				sealAnimators[i].AnimationCompleted = null;
				tk2dSpriteAnimator obj = sealAnimators[i];
				obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(OnSealAnimationEvent));
				sealAnimators[i].gameObject.SetActive(true);
				sealAnimators[i].Play(sealAnimationName);
				if (sealAnimators[i].Sprite.specRigidbody != null)
				{
					sealAnimators[i].Sprite.specRigidbody.enabled = true;
					sealAnimators[i].Sprite.specRigidbody.Initialize();
					for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
					{
						sealAnimators[i].Sprite.specRigidbody.RegisterGhostCollisionException(GameManager.Instance.AllPlayers[j].specRigidbody);
					}
				}
			}
			AkSoundEngine.PostEvent("Play_OBJ_gate_slam_01", base.gameObject);
			for (int k = 0; k < sealChainAnimators.Length; k++)
			{
				if (sealChainAnimators[k].GetClipByName(sealAnimationName + "_chain") != null)
				{
					sealChainAnimators[k].AnimationCompleted = null;
					sealChainAnimators[k].gameObject.SetActive(true);
					sealChainAnimators[k].Play(sealAnimationName + "_chain");
				}
			}
		}
		else if (Mode != DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS && Mode == DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS)
		{
			for (int l = 0; l < sealAnimators.Length; l++)
			{
				StartCoroutine(MoveTransformSmoothly(sealAnimators[l].transform, new Vector3(0f, -1.5625f, 0f), 1.5f, null));
			}
		}
		if (doorMode == DungeonDoorMode.SINGLE_DOOR)
		{
			for (int m = 0; m < doorModules.Length; m++)
			{
				doorModules[m].rigidbody.enabled = true;
			}
		}
		for (int n = 0; n < sealAnimators.Length; n++)
		{
			tk2dSpriteAnimator tk2dSpriteAnimator2 = sealAnimators[n];
			if (tk2dSpriteAnimator2.GetComponent<SpeculativeRigidbody>() != null)
			{
				tk2dSpriteAnimator2.GetComponent<SpeculativeRigidbody>().enabled = true;
			}
		}
		isSealed = true;
	}

	private GameObject SpawnVFXAtPoint(GameObject vfx, Vector3 position)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(vfx, position, Quaternion.identity, true);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.HeightOffGround = 0.25f;
		component.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.MiddleCenter);
		component.IsPerpendicular = false;
		component.UpdateZDepth();
		return gameObject;
	}

	private IEnumerator DelayedLayerChange(GameObject targetObject, string layer, float delay)
	{
		yield return new WaitForSeconds(delay);
		targetObject.layer = LayerMask.NameToLayer(layer);
	}

	private void UnsealInternal()
	{
		if (Mode == DungeonDoorMode.FINAL_BOSS_DOOR)
		{
			if (!IsSealed)
			{
				return;
			}
			sealAnimators[0].Play(unsealAnimationName);
			sealVFX[0].gameObject.SetActive(true);
			sealVFX[0].PlayAndDisableObject(string.Empty);
		}
		else if (!string.IsNullOrEmpty(unsealAnimationName))
		{
			for (int i = 0; i < sealAnimators.Length; i++)
			{
				sealAnimators[i].Sprite.UpdateZDepth();
				sealAnimators[i].Play(unsealAnimationName);
				tk2dSpriteAnimator obj = sealAnimators[i];
				obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnUnsealAnimationCompleted));
				sealAnimators[i].AnimationEventTriggered = null;
				if (sealAnimators[i].Sprite.specRigidbody != null)
				{
					sealAnimators[i].Sprite.specRigidbody.enabled = false;
				}
			}
			AkSoundEngine.PostEvent("Play_OBJ_gate_open_01", base.gameObject);
			for (int j = 0; j < sealChainAnimators.Length; j++)
			{
				if (sealChainAnimators[j].GetClipByName(unsealAnimationName + "_chain") != null)
				{
					sealChainAnimators[j].Play(unsealAnimationName + "_chain");
					tk2dSpriteAnimator obj2 = sealChainAnimators[j];
					obj2.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj2.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnUnsealAnimationCompleted));
					sealChainAnimators[j].AnimationEventTriggered = null;
				}
			}
		}
		else if (Mode == DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
		{
			if (northSouth)
			{
				for (int k = 0; k < sealAnimators.Length; k++)
				{
					sealAnimators[k].gameObject.layer = LayerMask.NameToLayer("BG_Critical");
					StartCoroutine(MoveTransformSmoothly(sealAnimators[k].transform, new Vector3(0f, -2f, -2.25f), 1.5f, OnUnsealAnimationCompleted));
				}
				StartCoroutine(DelayedLayerChange(sealAnimators[0].transform.GetChild(0).gameObject, "BG_Critical", 1.5f));
				if (GameManager.Instance.Dungeon.SecretRoomHorizontalPoofVFX != null)
				{
					GameObject gameObject = SpawnVFXAtPoint(GameManager.Instance.Dungeon.SecretRoomHorizontalPoofVFX, base.transform.position + new Vector3(0f, -0.25f, 0f));
					tk2dSpriteAnimator component = gameObject.GetComponent<tk2dSpriteAnimator>();
					component.PlayAndDestroyObject(string.Empty);
				}
			}
			else
			{
				for (int l = 0; l < sealAnimators.Length; l++)
				{
					sealAnimators[l].gameObject.layer = LayerMask.NameToLayer("BG_Critical");
					StartCoroutine(MoveTransformSmoothly(sealAnimators[l].transform, new Vector3(0f, -2.5f, -3.25f), 1.5f, OnUnsealAnimationCompleted));
				}
				StartCoroutine(DelayedLayerChange(sealAnimators[0].transform.GetChild(0).gameObject, "BG_Critical", 1.35f));
				if (GameManager.Instance.Dungeon.SecretRoomVerticalPoofVFX != null)
				{
					GameObject gameObject2 = SpawnVFXAtPoint(GameManager.Instance.Dungeon.SecretRoomVerticalPoofVFX, base.transform.position + Vector3.up);
					gameObject2.transform.position = base.transform.position + new Vector3(-1.25f, 0.75f, 0f);
					tk2dSpriteAnimator component2 = gameObject2.GetComponent<tk2dSpriteAnimator>();
					component2.PlayAndDestroyObject(string.Empty);
				}
			}
			AkSoundEngine.PostEvent("Play_OBJ_secret_door_01", base.gameObject);
		}
		else if (Mode == DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS)
		{
			for (int m = 0; m < sealAnimators.Length; m++)
			{
				StartCoroutine(MoveTransformSmoothly(sealAnimators[m].transform, new Vector3(0f, 1.5625f, 0f), 1.5f, OnUnsealAnimationCompleted));
			}
			AkSoundEngine.PostEvent("Play_OBJ_bossdoor_open_01", base.gameObject);
		}
		if (doorMode == DungeonDoorMode.SINGLE_DOOR && m_open && !isLocked)
		{
			for (int n = 0; n < doorModules.Length; n++)
			{
				doorModules[n].rigidbody.enabled = false;
			}
		}
		if (usesUnsealScreenShake)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(unsealScreenShake, base.transform.position);
		}
		isSealed = false;
		if (m_wasOpenWhenSealed)
		{
			Open();
		}
	}

	public void OnSealAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		if (clip.GetFrame(frameNo).eventInfo == "SealVFX" && sealVFX.Length > 0)
		{
			for (int i = 0; i < sealVFX.Length; i++)
			{
				sealVFX[i].gameObject.SetActive(true);
				sealVFX[i].Play();
			}
			animator.Sprite.UpdateZDepth();
		}
	}

	private IEnumerator HandleFrameDelayedUnsealedVFXOverride()
	{
		if (!m_hasGC)
		{
			m_hasGC = true;
			yield return null;
		}
		unsealedVFXOverride.SetActive(true);
		BraveInput.DoVibrationForAllPlayers(Vibration.Time.Normal, Vibration.Strength.Medium);
	}

	public void OnUnsealAnimationCompleted(tk2dSpriteAnimator a, tk2dSpriteAnimationClip c)
	{
		if (hideSealAnimators)
		{
			a.gameObject.SetActive(false);
		}
		if (a.GetComponent<SpeculativeRigidbody>() != null)
		{
			a.GetComponent<SpeculativeRigidbody>().enabled = false;
		}
		if (unsealedVFXOverride != null)
		{
			StartCoroutine(HandleFrameDelayedUnsealedVFXOverride());
		}
	}

	public void OnCloseAnimationCompleted(tk2dSpriteAnimator a, tk2dSpriteAnimationClip c)
	{
		a.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(a.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnCloseAnimationCompleted));
		if (a.Sprite.FlipX)
		{
			a.Sprite.FlipX = false;
		}
	}

	private void SetState(bool openState, bool flipped = false)
	{
		if (openState)
		{
			hasEverBeenOpen = true;
		}
		TriggerPersistentVFXClear();
		m_open = openState;
		if (!northSouth)
		{
			for (int i = 0; i < doorModules.Length; i++)
			{
				if (doorModules[i].horizontalFlips)
				{
					doorModules[i].sprite.FlipX = ((!openState) ? m_openIsFlipped : flipped);
				}
			}
		}
		if (openState)
		{
			for (int j = 0; j < doorModules.Length; j++)
			{
				m_openIsFlipped = flipped;
				DoorModule doorModule = doorModules[j];
				string text = doorModule.openAnimationName;
				tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
				if (!string.IsNullOrEmpty(text))
				{
					if (flipped && northSouth)
					{
						text = text.Replace("_north", "_south");
					}
					tk2dSpriteAnimationClip2 = doorModule.animator.GetClipByName(text);
				}
				if (tk2dSpriteAnimationClip2 != null)
				{
					doorModule.animator.Play(tk2dSpriteAnimationClip2);
				}
				for (int k = 0; k < doorModule.AOAnimatorsToDisable.Count; k++)
				{
					doorModule.AOAnimatorsToDisable[k].PlayAndDisableObject(string.Empty);
				}
				doorModule.rigidbody.enabled = false;
				AnimationDepthLerp(doorModule.sprite, doorModule.openDepth, tk2dSpriteAnimationClip2, doorModule, !northSouth && j == 0);
			}
		}
		else
		{
			for (int l = 0; l < doorModules.Length; l++)
			{
				DoorModule doorModule2 = doorModules[l];
				string text2 = doorModule2.closeAnimationName;
				tk2dSpriteAnimationClip tk2dSpriteAnimationClip3 = null;
				if (!string.IsNullOrEmpty(text2))
				{
					if (m_openIsFlipped && northSouth)
					{
						text2 = text2.Replace("_north", "_south");
					}
					tk2dSpriteAnimationClip3 = doorModule2.animator.GetClipByName(text2);
				}
				if (tk2dSpriteAnimationClip3 != null)
				{
					doorModule2.animator.Play(tk2dSpriteAnimationClip3);
					tk2dSpriteAnimator animator = doorModule2.animator;
					animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnCloseAnimationCompleted));
				}
				else
				{
					doorModule2.animator.StopAndResetFrame();
				}
				for (int m = 0; m < doorModule2.AOAnimatorsToDisable.Count; m++)
				{
					doorModule2.AOAnimatorsToDisable[m].gameObject.SetActive(true);
					doorModule2.AOAnimatorsToDisable[m].StopAndResetFrame();
				}
				doorModule2.rigidbody.enabled = true;
				AnimationDepthLerp(doorModule2.sprite, doorModule2.closedDepth, tk2dSpriteAnimationClip3, doorModule2);
			}
		}
		IntVector2 startingPosition = base.transform.position.IntXY(VectorConversions.Floor);
		if (upstreamRoom != null && upstreamRoom.visibility != 0)
		{
			Pixelator.Instance.ProcessRoomAdditionalExits(startingPosition, upstreamRoom, false);
		}
		if (downstreamRoom != null && downstreamRoom.visibility != 0)
		{
			Pixelator.Instance.ProcessRoomAdditionalExits(startingPosition, downstreamRoom, false);
		}
	}

	public float GetDistanceToPlayer(SpeculativeRigidbody playerRigidbody)
	{
		Vector4 sRBBoundingBox = GetSRBBoundingBox();
		return BraveMathCollege.DistBetweenRectangles(new Vector2(sRBBoundingBox.x, sRBBoundingBox.y), new Vector2(sRBBoundingBox.z - sRBBoundingBox.x, sRBBoundingBox.w - sRBBoundingBox.y), playerRigidbody.UnitBottomLeft, playerRigidbody.UnitDimensions);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private Vector2 GetModuleAveragePosition()
	{
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < doorModules.Length; i++)
		{
			zero += doorModules[i].animator.Sprite.WorldCenter;
		}
		return zero / doorModules.Length;
	}

	private Vector4 GetSRBBoundingBox()
	{
		Vector2 lhs = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 lhs2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < doorModules.Length; i++)
		{
			if (doorModules[i].rigidbody != null)
			{
				lhs = Vector2.Min(lhs, doorModules[i].rigidbody.UnitBottomLeft);
				lhs2 = Vector2.Max(lhs2, doorModules[i].rigidbody.UnitTopRight);
			}
		}
		return new Vector4(lhs.x, lhs.y, lhs2.x, lhs2.y);
	}

	private Vector2 GetSRBAveragePosition()
	{
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < doorModules.Length; i++)
		{
			zero += doorModules[i].animator.GetComponent<SpeculativeRigidbody>().UnitCenter;
			if (!northSouth)
			{
				zero += new Vector2(0f, -0.375f);
			}
		}
		return zero / doorModules.Length;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!isLocked || lockIsBusted)
		{
			return 1000f;
		}
		return Vector2.Distance(point, GetModuleAveragePosition()) / 3f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && (!isLocked || interactor.carriedConsumables.KeyBullets > 0 || interactor.carriedConsumables.InfiniteKeys) && !IsSealed)
		{
			SpriteOutlineManager.AddOutlineToSprite(LockAnimator.Sprite, Color.white, 0.05f);
			LockAnimator.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this && !IsSealed)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(LockAnimator.Sprite);
		}
	}

	public void FlipLockToOtherSide()
	{
		LockAnimator.Sprite.FlipX = true;
		LockAnimator.Sprite.transform.position += new Vector3(-0.375f, 0f, 0f);
		ChainsAnimator.Sprite.FlipX = true;
		if (ChainsAnimator.transform.parent != LockAnimator.transform)
		{
			ChainsAnimator.Sprite.transform.position += new Vector3(-0.375f, 0f, 0f);
		}
	}

	public void Unlock()
	{
		if (!isLocked)
		{
			return;
		}
		isLocked = false;
		if (LockAnimator != null)
		{
			if (northSouth)
			{
				LockAnimator.PlayAndDestroyObject("look_guy_unlock");
				ChainsAnimator.PlayAndDestroyObject("lock_guy_chain_north_unlock");
			}
			else
			{
				LockAnimator.PlayAndDestroyObject("lock_guy_side_unlock");
				ChainsAnimator.PlayAndDestroyObject("lock_guy_chain_side_unlock");
			}
		}
		RoomHandler.unassignedInteractableObjects.Remove(this);
	}

	public void BreakLock()
	{
		if (isLocked && !lockIsBusted)
		{
			lockIsBusted = true;
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (IsSealed || lockIsBusted || !isLocked)
		{
			return;
		}
		if (interactor.carriedConsumables.KeyBullets <= 0 && !interactor.carriedConsumables.InfiniteKeys)
		{
			if (northSouth && LockAnimator != null)
			{
				LockAnimator.Play("lock_guy_laugh");
				m_lockHasLaughed = true;
				m_lockHasSpit = false;
			}
		}
		else
		{
			if (!interactor.carriedConsumables.InfiniteKeys)
			{
				interactor.carriedConsumables.KeyBullets--;
			}
			Unlock();
		}
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
}
