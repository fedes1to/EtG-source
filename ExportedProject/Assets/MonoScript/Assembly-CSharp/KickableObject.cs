using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class KickableObject : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public float rollSpeed = 3f;

	[CheckAnimation(null)]
	public string[] rollAnimations;

	[CheckAnimation(null)]
	public string[] impactAnimations;

	public bool leavesGoopTrail;

	[ShowInInspectorIf("leavesGoopTrail", false)]
	public GoopDefinition goopType;

	[ShowInInspectorIf("leavesGoopTrail", false)]
	public float goopFrequency = 0.05f;

	[ShowInInspectorIf("leavesGoopTrail", false)]
	public float goopRadius = 1f;

	public bool triggersBreakTimer;

	[ShowInInspectorIf("triggersBreakTimer", false)]
	public float breakTimerLength = 3f;

	[ShowInInspectorIf("triggersBreakTimer", false)]
	public GameObject timerVFX;

	public bool RollingDestroysSafely = true;

	public string RollingBreakAnim = "red_barrel_break";

	private float m_goopElapsed;

	private DeadlyDeadlyGoopManager m_goopManager;

	private RoomHandler m_room;

	private bool m_isBouncingBack;

	private bool m_timerIsActive;

	[NonSerialized]
	public bool AllowTopWallTraversal;

	public IntVector2? m_lastDirectionKicked;

	private bool m_shouldDisplayOutline;

	private PlayerController m_lastInteractingPlayer;

	private DungeonData.Direction m_lastOutlineDirection = (DungeonData.Direction)(-1);

	private int m_lastSpriteId;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnPlayerCollision));
	}

	public void Update()
	{
		if (m_shouldDisplayOutline)
		{
			int quadrant;
			DungeonData.Direction inverseDirection = DungeonData.GetInverseDirection(DungeonData.GetDirectionFromIntVector2(GetFlipDirection(m_lastInteractingPlayer.specRigidbody, out quadrant)));
			if (inverseDirection != m_lastOutlineDirection || base.sprite.spriteId != m_lastSpriteId)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
				SpriteOutlineManager.AddSingleOutlineToSprite<tk2dSprite>(base.sprite, DungeonData.GetIntVector2FromDirection(inverseDirection), Color.white, 0.25f);
			}
			m_lastOutlineDirection = inverseDirection;
			m_lastSpriteId = base.sprite.spriteId;
		}
		if (!leavesGoopTrail || !(base.specRigidbody.Velocity.magnitude > 0.1f))
		{
			return;
		}
		m_goopElapsed += BraveTime.DeltaTime;
		if (m_goopElapsed > goopFrequency)
		{
			m_goopElapsed -= BraveTime.DeltaTime;
			if (m_goopManager == null)
			{
				m_goopManager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopType);
			}
			m_goopManager.AddGoopCircle(base.sprite.WorldCenter, goopRadius + 0.1f);
		}
		if (AllowTopWallTraversal && GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(base.sprite.WorldCenter.ToIntVector2(VectorConversions.Floor)) && GameManager.Instance.Dungeon.data[base.sprite.WorldCenter.ToIntVector2(VectorConversions.Floor)].IsFireplaceCell)
		{
			MinorBreakable component = GetComponent<MinorBreakable>();
			if ((bool)component && !component.IsBroken)
			{
				component.Break(Vector2.zero);
				GameStatsManager.Instance.SetFlag(GungeonFlags.FLAG_ROLLED_BARREL_INTO_FIREPLACE, true);
			}
		}
	}

	public void ForceDeregister()
	{
		if (m_room != null)
		{
			m_room.DeregisterInteractable(this);
		}
		RoomHandler.unassignedInteractableObjects.Remove(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		Vector2 inVec = interactor.CenterPosition - base.specRigidbody.UnitCenter;
		switch (BraveMathCollege.VectorToQuadrant(inVec))
		{
		case 0:
			return "tablekick_down";
		case 1:
			shouldBeFlipped = true;
			return "tablekick_right";
		case 2:
			return "tablekick_up";
		case 3:
			return "tablekick_right";
		default:
			Debug.Log("fail");
			return "tablekick_up";
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			m_lastInteractingPlayer = interactor;
			m_shouldDisplayOutline = true;
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			ClearOutlines();
			m_shouldDisplayOutline = false;
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.sprite.transform.position, bounds.max + base.sprite.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void Interact(PlayerController player)
	{
		GameManager.Instance.Dungeon.GetRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2()).DeregisterInteractable(this);
		RoomHandler.unassignedInteractableObjects.Remove(this);
		Kick(player.specRigidbody);
		AkSoundEngine.PostEvent("Play_OBJ_table_flip_01", player.gameObject);
		ClearOutlines();
		m_shouldDisplayOutline = false;
		if (GameManager.Instance.InTutorial)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerRolledBarrel");
		}
	}

	private void NoPits(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		Func<IntVector2, bool> func = delegate(IntVector2 pixel)
		{
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(pixel);
			if (!GameManager.Instance.Dungeon.CellSupportsFalling(vector))
			{
				return false;
			}
			List<SpeculativeRigidbody> platformsAt = GameManager.Instance.Dungeon.GetPlatformsAt(vector);
			if (platformsAt != null)
			{
				for (int i = 0; i < platformsAt.Count; i++)
				{
					if (platformsAt[i].PrimaryPixelCollider.ContainsPixel(pixel))
					{
						return false;
					}
				}
			}
			return true;
		};
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider != null)
		{
			IntVector2 intVector = pixelOffset - prevPixelOffset;
			if (intVector == IntVector2.Down && func(primaryPixelCollider.LowerLeft + pixelOffset) && func(primaryPixelCollider.LowerRight + pixelOffset) && (!func(primaryPixelCollider.UpperRight + prevPixelOffset) || !func(primaryPixelCollider.UpperLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Right && func(primaryPixelCollider.LowerRight + pixelOffset) && func(primaryPixelCollider.UpperRight + pixelOffset) && (!func(primaryPixelCollider.UpperLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Up && func(primaryPixelCollider.UpperRight + pixelOffset) && func(primaryPixelCollider.UpperLeft + pixelOffset) && (!func(primaryPixelCollider.LowerLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerRight + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Left && func(primaryPixelCollider.UpperLeft + pixelOffset) && func(primaryPixelCollider.LowerLeft + pixelOffset) && (!func(primaryPixelCollider.LowerRight + prevPixelOffset) || !func(primaryPixelCollider.UpperRight + prevPixelOffset)))
			{
				validLocation = false;
			}
		}
		if (!validLocation)
		{
			StopRolling(true);
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
	}

	private void OnPlayerCollision(CollisionData rigidbodyCollision)
	{
		PlayerController component = rigidbodyCollision.OtherRigidbody.GetComponent<PlayerController>();
		if (RollingDestroysSafely && component != null && component.IsDodgeRolling)
		{
			MinorBreakable component2 = GetComponent<MinorBreakable>();
			component2.destroyOnBreak = true;
			component2.makeParallelOnBreak = false;
			component2.breakAnimName = RollingBreakAnim;
			component2.explodesOnBreak = false;
			component2.Break(-rigidbodyCollision.Normal);
		}
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		MinorBreakable component = otherRigidbody.GetComponent<MinorBreakable>();
		if ((bool)component && !component.onlyVulnerableToGunfire && !component.IsBig)
		{
			component.Break(base.specRigidbody.Velocity);
			PhysicsEngine.SkipCollision = true;
		}
		if ((bool)otherRigidbody && (bool)otherRigidbody.aiActor && !otherRigidbody.aiActor.IsNormalEnemy)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void OnCollision(CollisionData collision)
	{
		if ((collision.collisionType != 0 || !(collision.OtherRigidbody.projectile != null)) && ((BraveMathCollege.ActualSign(base.specRigidbody.Velocity.x) != 0f && Mathf.Sign(collision.Normal.x) != Mathf.Sign(base.specRigidbody.Velocity.x)) || (BraveMathCollege.ActualSign(base.specRigidbody.Velocity.y) != 0f && Mathf.Sign(collision.Normal.y) != Mathf.Sign(base.specRigidbody.Velocity.y))) && ((BraveMathCollege.ActualSign(base.specRigidbody.Velocity.x) != 0f && Mathf.Sign(collision.Contact.x - base.specRigidbody.UnitCenter.x) == Mathf.Sign(base.specRigidbody.Velocity.x)) || (BraveMathCollege.ActualSign(base.specRigidbody.Velocity.y) != 0f && Mathf.Sign(collision.Contact.y - base.specRigidbody.UnitCenter.y) == Mathf.Sign(base.specRigidbody.Velocity.y))))
		{
			StopRolling(collision.collisionType == CollisionData.CollisionType.TileMap);
		}
	}

	private bool IsRollAnimation()
	{
		for (int i = 0; i < rollAnimations.Length; i++)
		{
			if (base.spriteAnimator.CurrentClip.name == rollAnimations[i])
			{
				return true;
			}
		}
		return false;
	}

	private void StopRolling(bool bounceBack)
	{
		if (bounceBack && !m_isBouncingBack)
		{
			StartCoroutine(HandleBounceback());
			return;
		}
		base.spriteAnimator.Stop();
		if (IsRollAnimation())
		{
			tk2dSpriteAnimationClip currentClip = base.spriteAnimator.CurrentClip;
			base.spriteAnimator.Stop();
			base.spriteAnimator.Sprite.SetSprite(currentClip.frames[currentClip.frames.Length - 1].spriteId);
		}
		base.specRigidbody.Velocity = Vector2.zero;
		MinorBreakable component = GetComponent<MinorBreakable>();
		if (component != null)
		{
			component.onlyVulnerableToGunfire = false;
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(NoPits));
		RoomHandler.unassignedInteractableObjects.Add(this);
		m_isBouncingBack = false;
	}

	private IEnumerator HandleBounceback()
	{
		if (m_lastDirectionKicked.HasValue)
		{
			m_isBouncingBack = true;
			Vector2 dirToMove = m_lastDirectionKicked.Value.ToVector2().normalized * -1f;
			int quadrant = BraveMathCollege.VectorToQuadrant(dirToMove);
			base.specRigidbody.Velocity = rollSpeed * dirToMove;
			m_lastDirectionKicked *= -1;
			tk2dSpriteAnimationClip rollClip = base.spriteAnimator.GetClipByName(rollAnimations[quadrant]);
			if (rollClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection)
			{
				base.spriteAnimator.PlayFromFrame(rollClip, rollClip.loopStart);
			}
			else
			{
				base.spriteAnimator.Play(rollClip);
			}
			float ela = 0f;
			float dura = 1.5f / base.specRigidbody.Velocity.magnitude;
			while (ela < dura && m_isBouncingBack)
			{
				ela += BraveTime.DeltaTime;
				base.specRigidbody.Velocity = rollSpeed * dirToMove;
				yield return null;
			}
			if (m_isBouncingBack)
			{
				StopRolling(false);
			}
		}
	}

	private void ClearOutlines()
	{
		m_lastOutlineDirection = (DungeonData.Direction)(-1);
		m_lastSpriteId = -1;
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
	}

	private IEnumerator HandleBreakTimer()
	{
		m_timerIsActive = true;
		if (timerVFX != null)
		{
			timerVFX.SetActive(true);
		}
		yield return new WaitForSeconds(breakTimerLength);
		base.minorBreakable.Break();
	}

	private void RemoveFromRoomHierarchy()
	{
		Transform hierarchyParent = base.transform.position.GetAbsoluteRoom().hierarchyParent;
		Transform parent = base.transform;
		while (parent.parent != null)
		{
			if (parent.parent == hierarchyParent)
			{
				parent.parent = null;
				break;
			}
			parent = parent.parent;
		}
	}

	private void Kick(SpeculativeRigidbody kickerRigidbody)
	{
		if ((bool)base.specRigidbody && !base.specRigidbody.enabled)
		{
			return;
		}
		RemoveFromRoomHierarchy();
		List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody.PrimaryPixelCollider);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			if ((bool)overlappingRigidbodies[i] && (bool)overlappingRigidbodies[i].minorBreakable && !overlappingRigidbodies[i].minorBreakable.IsBroken && !overlappingRigidbodies[i].minorBreakable.onlyVulnerableToGunfire && !overlappingRigidbodies[i].minorBreakable.OnlyBrokenByCode)
			{
				overlappingRigidbodies[i].minorBreakable.Break();
			}
		}
		int value = ~CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox);
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody, value);
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(NoPits));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnCollision = (Action<CollisionData>)Delegate.Combine(speculativeRigidbody2.OnCollision, new Action<CollisionData>(OnCollision));
		SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
		speculativeRigidbody3.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody3.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		int quadrant;
		IntVector2 flipDirection = GetFlipDirection(kickerRigidbody, out quadrant);
		if (AllowTopWallTraversal)
		{
			base.specRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyBlocker));
		}
		base.specRigidbody.Velocity = rollSpeed * flipDirection.ToVector2();
		tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(rollAnimations[quadrant]);
		bool flag = false;
		if (m_lastDirectionKicked.HasValue)
		{
			if (m_lastDirectionKicked.Value.y == 0 && flipDirection.y == 0)
			{
				flag = true;
			}
			if (m_lastDirectionKicked.Value.x == 0 && flipDirection.x == 0)
			{
				flag = true;
			}
		}
		if (clipByName != null && clipByName.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection && flag)
		{
			base.spriteAnimator.PlayFromFrame(clipByName, clipByName.loopStart);
		}
		else
		{
			base.spriteAnimator.Play(clipByName);
		}
		if (triggersBreakTimer && !m_timerIsActive)
		{
			StartCoroutine(HandleBreakTimer());
		}
		MinorBreakable component = GetComponent<MinorBreakable>();
		if (component != null)
		{
			component.breakAnimName = impactAnimations[quadrant];
			component.onlyVulnerableToGunfire = true;
		}
		IntVector2 key = base.transform.PositionVector2().ToIntVector2();
		GameManager.Instance.Dungeon.data[key].isOccupied = false;
		m_lastDirectionKicked = flipDirection;
	}

	public IntVector2 GetFlipDirection(SpeculativeRigidbody kickerRigidbody, out int quadrant)
	{
		Vector2 inVec = base.specRigidbody.UnitCenter - kickerRigidbody.UnitCenter;
		quadrant = BraveMathCollege.VectorToQuadrant(inVec);
		return IntVector2.Cardinals[quadrant];
	}
}
