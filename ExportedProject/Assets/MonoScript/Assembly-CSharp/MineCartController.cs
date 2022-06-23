using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class MineCartController : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public enum CartOccupationState
	{
		EMPTY,
		PLAYER,
		ENEMY,
		CARGO
	}

	[NonSerialized]
	public bool ForceActive;

	[DwarfConfigurable]
	public bool IsOnlyPlayerMinecart;

	[DwarfConfigurable]
	public bool AlwaysMoving;

	public tk2dSpriteAnimator childAnimator;

	public Transform attachTransform;

	public SpeculativeRigidbody carriedCargo;

	public bool MoveCarriedCargoIntoCart = true;

	public string HorizontalAnimationName;

	public string VerticalAnimationName;

	public float KnockbackStrengthPlayer = 3f;

	public float KnockbackStrengthEnemy = 10f;

	[DwarfConfigurable]
	public float MaxSpeed = 7f;

	public float TimeToMaxSpeed = 1f;

	private const float UnoccupiedSpeedDecay = 4f;

	private CartTurretController m_turret;

	public Transform Sparks_A;

	public Transform Sparks_B;

	[NonSerialized]
	public CartOccupationState occupation;

	protected GameActor m_rider;

	protected GameActor m_secondaryRider;

	protected PathMover m_pathMover;

	protected float m_elapsedOccupied;

	protected float m_elapsedSecondary;

	protected bool m_handlingQueuedAnimation;

	protected RoomHandler m_room;

	protected List<MineCartController> m_minecartsInRoom;

	private float m_justRolledInTimer;

	private bool m_hasHandledCornerAnimation;

	private bool m_wasPushedThisFrame;

	private SpeculativeRigidbody m_pusher;

	private bool m_cartSoundActive;

	private List<CollisionData> m_cachedCollisionList = new List<CollisionData>();

	private float m_lastAccelVector;

	private Dictionary<string, string> m_animationMap = new Dictionary<string, string>();

	protected Coroutine m_primaryLerpCoroutine;

	protected Coroutine m_secondaryLerpCoroutine;

	public float MaxSpeedEnemy
	{
		get
		{
			return MaxSpeed;
		}
	}

	public float MaxSpeedPlayer
	{
		get
		{
			return MaxSpeed;
		}
	}

	public GameActor CurrentInhabitant
	{
		get
		{
			return m_rider;
		}
	}

	private void Awake()
	{
		if (carriedCargo != null && carriedCargo.specRigidbody != null)
		{
			base.specRigidbody.RegisterSpecificCollisionException(carriedCargo.specRigidbody);
			carriedCargo.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
			m_turret = carriedCargo.GetComponent<CartTurretController>();
		}
		m_pathMover = GetComponent<PathMover>();
		m_pathMover.ForceCornerDelayHack = true;
		if ((bool)base.majorBreakable)
		{
			MajorBreakable obj = base.majorBreakable;
			obj.OnBreak = (Action)Delegate.Combine(obj.OnBreak, new Action(DestroyMineCart));
		}
	}

	private IEnumerator Start()
	{
		m_room = GetAbsoluteParentRoom();
		ForceActive |= AlwaysMoving;
		m_pathMover.nodeOffset = new Vector2(-0.5f, 0f);
		PathMover pathMover = m_pathMover;
		pathMover.OnNodeReached = (Action<Vector2, Vector2, bool>)Delegate.Combine(pathMover.OnNodeReached, new Action<Vector2, Vector2, bool>(HandleCornerReached));
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreRigidbodyCollision));
		base.specRigidbody.CollideWithTileMap = false;
		base.specRigidbody.ForceCarriesRigidbodies = true;
		yield return null;
		if (m_room.GetRoomName().Contains("BulletComponent"))
		{
			RoomHandler room = m_room;
			room.OnPlayerReturnedFromPit = (Action<PlayerController>)Delegate.Combine(room.OnPlayerReturnedFromPit, new Action<PlayerController>(HandlePlayerPitRespawn));
		}
		m_minecartsInRoom = m_room.GetComponentsAbsoluteInRoom<MineCartController>();
		base.specRigidbody.PixelColliders[0].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.LowObstacle);
		if (carriedCargo != null)
		{
			occupation = CartOccupationState.CARGO;
			BecomeCargoOccupied();
		}
	}

	private bool IsOnlyMinecartInRoom()
	{
		return m_minecartsInRoom.Count == 1;
	}

	private bool IsReachableFromPosition(Vector2 targetPoint)
	{
		Path path = new Path();
		Pathfinder.Instance.GetPath(targetPoint.ToIntVector2(VectorConversions.Floor), base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), out path, IntVector2.One);
		if (path == null)
		{
			return false;
		}
		return path.WillReachFinalGoal;
	}

	private void HandlePlayerPitRespawn(PlayerController obj)
	{
		m_pathMover.WarpToStart();
	}

	private void WarpToNearestPointOnPath(Vector2 targetPoint)
	{
		m_pathMover.WarpToNearestPoint(targetPoint);
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
	}

	private void HandlePitFall(Vector2 lastVec)
	{
		Evacuate(false, true);
		EvacuateSecondary(false, true);
		m_pathMover.Paused = true;
		IntVector2 dir = lastVec.ToIntVector2(VectorConversions.Floor).MajorAxis * 2;
		StartCoroutine(StartFallAnimation(dir, base.specRigidbody));
	}

	private IEnumerator StartFallAnimation(IntVector2 dir, SpeculativeRigidbody targetRigidbody)
	{
		targetRigidbody.enabled = false;
		targetRigidbody.sprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
		float duration = 0.5f;
		float rotation = ((dir.x == 0) ? 0f : ((0f - Mathf.Sign(dir.x)) * 135f));
		Vector3 velocity = dir.ToVector3() * 1.25f / duration;
		Vector3 acceleration = new Vector3(0f, -10f, 0f);
		float timer = 0f;
		while (timer < duration)
		{
			targetRigidbody.transform.position += velocity * BraveTime.DeltaTime;
			targetRigidbody.transform.eulerAngles = targetRigidbody.transform.eulerAngles.WithZ(Mathf.Lerp(0f, rotation, timer / duration));
			targetRigidbody.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, timer / duration);
			yield return null;
			timer += BraveTime.DeltaTime;
			velocity += acceleration * BraveTime.DeltaTime;
		}
		GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(targetRigidbody.transform.position);
		yield return null;
		targetRigidbody.transform.rotation = Quaternion.identity;
		targetRigidbody.transform.localScale = Vector3.one;
		targetRigidbody.enabled = true;
		targetRigidbody.sprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
		m_pathMover.Paused = false;
		m_pathMover.WarpToNearestPoint(m_pathMover.Path.nodes[0].RoomPosition + m_pathMover.nodeOffset + m_pathMover.RoomHandler.area.basePosition.ToVector2());
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		m_pathMover.ForcePathToNextNode();
	}

	protected override void OnDestroy()
	{
		StopSound();
		base.OnDestroy();
	}

	private void HandlePreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody.minorBreakable && otherRigidbody.minorBreakable.isImpermeableToGameActors)
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (occupation != 0 && occupation != CartOccupationState.PLAYER)
		{
			return;
		}
		if (otherRigidbody.gameActor is PlayerController)
		{
			PlayerController playerController = otherRigidbody.gameActor as PlayerController;
			if (playerController.IsDodgeRolling && playerController.previousMineCart != null && playerController.previousMineCart != this)
			{
				playerController.ForceStopDodgeRoll();
				playerController.ToggleGunRenderers(true, string.Empty);
				m_justRolledInTimer = 0.5f;
				BecomeOccupied(playerController);
			}
			else if (occupation == CartOccupationState.EMPTY && !Mathf.Approximately(m_pathMover.PathSpeed, 0f))
			{
			}
		}
		else if (otherRigidbody.gameActor is AIActor)
		{
			AIActor aIActor = otherRigidbody.gameActor as AIActor;
			if (!aIActor.IsNormalEnemy)
			{
				PhysicsEngine.SkipCollision = true;
			}
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (m_pathMover.Paused)
		{
			return;
		}
		Vector2 direction = BraveMathCollege.VectorToCone(-rigidbodyCollision.Normal, 15f);
		AIActor aIActor = rigidbodyCollision.OtherRigidbody.aiActor;
		if ((bool)aIActor && (bool)aIActor.healthHaver && aIActor.healthHaver.IsAlive && CurrentInhabitant is PlayerController && base.specRigidbody.Velocity.magnitude > 2f)
		{
			aIActor.healthHaver.ApplyDamage(50f, direction, "Minecart Damage");
		}
		if (rigidbodyCollision.OtherRigidbody.knockbackDoer != null)
		{
			if ((bool)rigidbodyCollision.OtherRigidbody.gameActor && rigidbodyCollision.OtherRigidbody.gameActor is PlayerController)
			{
				rigidbodyCollision.OtherRigidbody.knockbackDoer.ApplySourcedKnockback(direction, KnockbackStrengthPlayer * Mathf.Abs(m_pathMover.PathSpeed), base.gameObject);
			}
			else
			{
				rigidbodyCollision.OtherRigidbody.knockbackDoer.ApplySourcedKnockback(direction, KnockbackStrengthEnemy * Mathf.Abs(m_pathMover.PathSpeed), base.gameObject);
			}
			if ((bool)m_rider)
			{
				m_rider.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 1f);
			}
			if ((bool)m_secondaryRider)
			{
				m_secondaryRider.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 1f);
			}
			base.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 1f);
		}
	}

	private void Update()
	{
		if (!m_pathMover)
		{
			return;
		}
		bool flag = GameManager.Instance.PlayerIsNearRoom(m_room);
		bool flag2 = m_turret != null && m_turret.Inactive;
		m_justRolledInTimer -= BraveTime.DeltaTime;
		if (flag2 || (occupation == CartOccupationState.EMPTY && !ForceActive))
		{
			m_elapsedOccupied = 0f;
			m_elapsedSecondary = 0f;
			if (m_pathMover.PathSpeed != 0f)
			{
				m_pathMover.Paused = false;
				if (!m_wasPushedThisFrame)
				{
					m_pathMover.PathSpeed = Mathf.MoveTowards(m_pathMover.PathSpeed, 0f, 4f * BraveTime.DeltaTime);
				}
			}
			else if (!m_pathMover.Paused)
			{
				CellData cellData = GameManager.Instance.Dungeon.data[base.transform.position.IntXY()];
				if (cellData == null || cellData.type == CellType.WALL)
				{
					m_pathMover.PathSpeed = Mathf.Sign(m_pathMover.PathSpeed) * MaxSpeedEnemy;
				}
				m_pathMover.Paused = true;
			}
		}
		else if (occupation == CartOccupationState.CARGO)
		{
			if (flag)
			{
				if (m_pathMover.Paused)
				{
					m_pathMover.Paused = false;
				}
				float maxSpeedEnemy = MaxSpeedEnemy;
				m_pathMover.PathSpeed = BraveMathCollege.SmoothLerp(0f, maxSpeedEnemy, Mathf.Clamp01(m_elapsedOccupied / TimeToMaxSpeed));
				m_elapsedOccupied += BraveTime.DeltaTime;
				if (!carriedCargo)
				{
					occupation = CartOccupationState.EMPTY;
				}
			}
			else
			{
				m_pathMover.Paused = true;
			}
		}
		else
		{
			if (m_pathMover.Paused)
			{
				m_pathMover.Paused = false;
			}
			if (occupation == CartOccupationState.PLAYER)
			{
				m_pathMover.PathSpeed = Mathf.Clamp(m_pathMover.PathSpeed, 0f - MaxSpeedPlayer, MaxSpeedPlayer);
			}
			else
			{
				float to = ((occupation != CartOccupationState.PLAYER) ? MaxSpeedEnemy : MaxSpeedPlayer);
				float num = Mathf.Max(m_elapsedOccupied, m_elapsedSecondary);
				m_pathMover.PathSpeed = BraveMathCollege.SmoothLerp(0f, to, Mathf.Clamp01(num / TimeToMaxSpeed));
				if (ForceActive)
				{
					m_pathMover.PathSpeed = MaxSpeedEnemy;
				}
			}
			if (m_rider != null)
			{
				m_elapsedOccupied += BraveTime.DeltaTime;
			}
			if (m_secondaryRider != null)
			{
				m_elapsedSecondary += BraveTime.DeltaTime;
			}
			if (!GameManager.Instance.IsPaused)
			{
				if (occupation == CartOccupationState.PLAYER)
				{
					HandlePlayerRiderInput(m_rider, m_elapsedOccupied);
					HandlePlayerRiderInput(m_secondaryRider, m_elapsedSecondary);
				}
				if (!m_rider || m_rider.healthHaver.IsDead)
				{
					Evacuate();
				}
				if (!m_secondaryRider || m_secondaryRider.healthHaver.IsDead)
				{
					EvacuateSecondary();
				}
			}
		}
		if (m_pathMover.AbsPathSpeed > 0f && flag)
		{
			StartSound();
		}
		else
		{
			StopSound();
		}
		if (m_cartSoundActive)
		{
			AkSoundEngine.SetRTPCValue("Pitch_Minecart", m_pathMover.AbsPathSpeed / MaxSpeed);
		}
		Vector2 directionFromPreviousNode = PhysicsEngine.PixelToUnit(base.specRigidbody.PathTarget) - base.specRigidbody.Position.UnitPosition;
		if (!m_hasHandledCornerAnimation && !m_handlingQueuedAnimation && directionFromPreviousNode.magnitude < 0.5f)
		{
			Vector2 directionToNextNode = m_pathMover.GetNextTargetPosition() - PhysicsEngine.PixelToUnit(base.specRigidbody.PathTarget);
			m_hasHandledCornerAnimation = true;
			HandleTurnAnimation(directionFromPreviousNode, directionToNextNode);
		}
		HandlePushCarts();
		EnsureRiderPosition();
		m_wasPushedThisFrame = false;
		m_pusher = null;
		UpdateSparksTransforms();
	}

	private void StartSound()
	{
		if (!m_cartSoundActive)
		{
			m_cartSoundActive = true;
			AkSoundEngine.PostEvent("Play_OBJ_minecart_loop_01", base.gameObject);
		}
	}

	private void StopSound()
	{
		if (m_cartSoundActive)
		{
			m_cartSoundActive = false;
			AkSoundEngine.PostEvent("Stop_OBJ_minecart_loop_01", base.gameObject);
		}
	}

	private void UpdateSparksTransforms()
	{
		if (Sparks_A == null)
		{
			return;
		}
		Vector2 velocity = base.specRigidbody.Velocity;
		if (velocity.magnitude < 2f)
		{
			Sparks_A.gameObject.SetActive(false);
			Sparks_B.gameObject.SetActive(false);
			return;
		}
		Sparks_A.GetComponent<Renderer>().enabled = true;
		Sparks_B.GetComponent<Renderer>().enabled = true;
		Sparks_A.gameObject.SetActive(true);
		Sparks_B.gameObject.SetActive(true);
		if (velocity.IsHorizontal())
		{
			ParticleSystem componentInChildren = Sparks_A.GetComponentInChildren<ParticleSystem>();
			ParticleSystem componentInChildren2 = Sparks_B.GetComponentInChildren<ParticleSystem>();
			Sparks_A.localPosition = new Vector3(1.4375f, 0.375f, -1.125f);
			componentInChildren.transform.localRotation = Quaternion.Euler(-30f, -125.25f, 55f);
			Sparks_B.localPosition = new Vector3(0.5f, 0.375f, -1.125f);
			componentInChildren2.transform.localRotation = Quaternion.Euler(-30f, -125.25f, 55f);
			if (velocity.x < 0f)
			{
				Sparks_B.localPosition = new Vector3(1.4375f, 1.0625f, -0.4375f);
				Sparks_B.GetComponent<Renderer>().enabled = false;
				if (componentInChildren.simulationSpace == ParticleSystemSimulationSpace.Local)
				{
					componentInChildren.transform.localRotation = Quaternion.Euler(-10f, 90f, 0f);
					componentInChildren2.transform.localRotation = Quaternion.Euler(-10f, 90f, 0f);
				}
			}
			else
			{
				Sparks_A.localPosition = new Vector3(0.5f, 1.0625f, -0.4375f);
				Sparks_A.GetComponent<Renderer>().enabled = false;
				if (componentInChildren.simulationSpace == ParticleSystemSimulationSpace.Local)
				{
					componentInChildren.transform.localRotation = Quaternion.Euler(-10f, -125.25f, 55f);
					componentInChildren2.transform.localRotation = Quaternion.Euler(-10f, -125.25f, 55f);
				}
			}
		}
		else
		{
			Sparks_A.localPosition = new Vector3(0.625f, 0.125f, -1.375f);
			Sparks_A.GetComponentInChildren<ParticleSystem>().transform.localRotation = Quaternion.Euler(-45f, 0f, -45f);
			Sparks_B.localPosition = new Vector3(1.3125f, 0.125f, -1.375f);
			Sparks_B.GetComponentInChildren<ParticleSystem>().transform.localRotation = Quaternion.Euler(-45f, 0f, -45f);
			if (!(velocity.y > 0f))
			{
				Sparks_A.GetComponent<Renderer>().enabled = false;
				Sparks_B.GetComponent<Renderer>().enabled = false;
			}
		}
	}

	public void ApplyVelocity(float speed)
	{
		if (m_pathMover == null)
		{
			m_pathMover = GetComponent<PathMover>();
		}
		m_pathMover.Paused = false;
		m_pathMover.PathSpeed = Mathf.Max(MaxSpeedEnemy, m_pathMover.PathSpeed + speed);
	}

	protected void HandlePushCarts()
	{
		if (m_pathMover.PathSpeed == 0f)
		{
			return;
		}
		MineCartController mineCartController = CheckWillHitMineCart();
		if (mineCartController != null && m_pathMover.AbsPathSpeed / MaxSpeed < 0.3f)
		{
			m_pathMover.PathSpeed = 0f;
			return;
		}
		float num = Mathf.Min(m_pathMover.AbsPathSpeed, MaxSpeedPlayer);
		float num2 = num + 1f;
		num2 *= Mathf.Sign(m_pathMover.PathSpeed);
		if (mineCartController != null && (Mathf.Abs(num2) > Mathf.Abs(mineCartController.m_pathMover.PathSpeed) || Mathf.Sign(num2) != Mathf.Sign(mineCartController.m_pathMover.PathSpeed)))
		{
			float parametrizedPathPosition = mineCartController.m_pathMover.GetParametrizedPathPosition();
			float parametrizedPathPosition2 = m_pathMover.GetParametrizedPathPosition();
			if ((m_pathMover.PathSpeed > 0f && parametrizedPathPosition > parametrizedPathPosition2) || (parametrizedPathPosition < 0.25f && parametrizedPathPosition2 > 0.75f) || (m_pathMover.PathSpeed < 0f && parametrizedPathPosition < parametrizedPathPosition2) || (parametrizedPathPosition > 0.75f && parametrizedPathPosition2 < 0.25f))
			{
				mineCartController.m_pathMover.Paused = false;
				mineCartController.m_pathMover.PathSpeed = num2;
				mineCartController.m_wasPushedThisFrame = true;
				mineCartController.m_pusher = base.specRigidbody;
				mineCartController.HandlePushCarts();
			}
		}
	}

	protected MineCartController CheckWillHitMineCart()
	{
		MineCartController mineCartController = null;
		m_cachedCollisionList.Clear();
		IntVector2 pixelsToMove = (PhysicsEngine.UnitToPixel((PhysicsEngine.PixelToUnit(base.specRigidbody.PathTarget) - base.specRigidbody.Position.UnitPosition).normalized * base.specRigidbody.PathSpeed).ToVector2() * BraveTime.DeltaTime).ToIntVector2(VectorConversions.Ceil);
		SpeculativeRigidbody speculativeRigidbody = null;
		SpeculativeRigidbody speculativeRigidbody2 = null;
		if (occupation == CartOccupationState.CARGO)
		{
			speculativeRigidbody = carriedCargo;
		}
		else if (occupation != 0)
		{
			if ((bool)m_rider)
			{
				speculativeRigidbody = m_rider.specRigidbody;
			}
			if ((bool)m_secondaryRider)
			{
				speculativeRigidbody2 = m_secondaryRider.specRigidbody;
			}
		}
		if (PhysicsEngine.Instance.OverlapCast(base.specRigidbody, m_cachedCollisionList, false, true, null, null, false, null, null, speculativeRigidbody, speculativeRigidbody2, m_pusher))
		{
			for (int i = 0; i < m_cachedCollisionList.Count; i++)
			{
				if (!m_cachedCollisionList[i].OtherRigidbody)
				{
					continue;
				}
				MineCartController component = m_cachedCollisionList[i].OtherRigidbody.GetComponent<MineCartController>();
				if (!(component == null))
				{
					float parametrizedPathPosition = component.m_pathMover.GetParametrizedPathPosition();
					float parametrizedPathPosition2 = m_pathMover.GetParametrizedPathPosition();
					if ((m_pathMover.PathSpeed > 0f && parametrizedPathPosition > parametrizedPathPosition2) || (parametrizedPathPosition < 0.25f && parametrizedPathPosition2 > 0.75f) || (m_pathMover.PathSpeed < 0f && parametrizedPathPosition < parametrizedPathPosition2) || (parametrizedPathPosition > 0.75f && parametrizedPathPosition2 < 0.25f))
					{
						return component;
					}
				}
			}
		}
		CollisionData result;
		if (PhysicsEngine.Instance.RigidbodyCastWithIgnores(base.specRigidbody, pixelsToMove, out result, false, true, null, true, speculativeRigidbody, speculativeRigidbody2, m_pusher))
		{
			mineCartController = result.OtherRigidbody.GetComponent<MineCartController>();
			if (mineCartController != null)
			{
				for (int j = 0; j < m_cachedCollisionList.Count; j++)
				{
					if (m_cachedCollisionList[j].OtherRigidbody == mineCartController.specRigidbody)
					{
						mineCartController = null;
						break;
					}
				}
			}
		}
		CollisionData.Pool.Free(ref result);
		return mineCartController;
	}

	protected void HandlePlayerRiderInput(GameActor targetRider, float targetElapsed)
	{
		if (targetRider == null)
		{
			return;
		}
		PlayerController playerController = targetRider as PlayerController;
		playerController.ZeroVelocityThisFrame = true;
		if (!(targetElapsed > BraveTime.DeltaTime))
		{
			return;
		}
		GungeonActions activeActions = BraveInput.GetInstanceForPlayer(playerController.PlayerIDX).ActiveActions;
		if (activeActions.InteractAction.WasPressed && m_justRolledInTimer <= 0f)
		{
			if (targetRider == m_rider)
			{
				Evacuate();
			}
			else if (targetRider == m_secondaryRider)
			{
				EvacuateSecondary();
			}
		}
		if (targetRider == m_rider || (targetRider == m_secondaryRider && m_rider == null))
		{
			Vector2 majorAxis = BraveUtility.GetMajorAxis(m_pathMover.GetPositionOfNode(m_pathMover.CurrentIndex) - base.transform.position.XY());
			float num = Vector2.Dot(majorAxis, activeActions.Move.Vector) * 15f * Mathf.Sign(m_pathMover.PathSpeed) * BraveTime.DeltaTime;
			if (m_pathMover.AbsPathSpeed / MaxSpeed > 0.1f && Mathf.Sign(num) != m_lastAccelVector && num != 0f && Mathf.Sign(num) != Mathf.Sign(m_pathMover.PathSpeed))
			{
				AkSoundEngine.PostEvent("Play_OBJ_minecart_brake_01", base.gameObject);
				m_lastAccelVector = Mathf.Sign(num);
			}
			m_pathMover.PathSpeed += num;
			if (num == 0f && m_pathMover.AbsPathSpeed / MaxSpeedPlayer < 0.3f)
			{
				m_pathMover.PathSpeed = Mathf.MoveTowards(m_pathMover.PathSpeed, 0f, 4f * BraveTime.DeltaTime);
			}
		}
		if (!activeActions.DodgeRollAction.WasPressed || playerController.WasPausedThisFrame)
		{
			return;
		}
		if (activeActions.Move.Vector.magnitude > 0.1f)
		{
			if (targetRider == m_rider)
			{
				Evacuate(true);
			}
			else if (targetRider == m_secondaryRider)
			{
				EvacuateSecondary(true);
			}
		}
		else
		{
			Vector2 normalized = base.specRigidbody.Velocity.normalized;
			string empty = string.Empty;
			empty = ((!(Mathf.Abs(normalized.x) < 0.1f)) ? (((!(normalized.y > 0.1f)) ? "dodge_left" : "dodge_left_bw") + ((!playerController.ArmorlessAnimations || playerController.healthHaver.Armor != 0f) ? string.Empty : "_armorless")) : (((!(normalized.y > 0.1f)) ? "dodge" : "dodge_bw") + ((!playerController.ArmorlessAnimations || playerController.healthHaver.Armor != 0f) ? string.Empty : "_armorless")));
			playerController.QueueSpecificAnimation(empty);
		}
	}

	protected void SetAnimation(string animationName, float clipFpsFraction)
	{
		if (!string.IsNullOrEmpty(animationName))
		{
			float num = 4f;
			tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(animationName);
			if (!base.spriteAnimator.IsPlaying(clipByName))
			{
				base.spriteAnimator.Play(clipByName);
			}
			base.spriteAnimator.ClipFps = Mathf.Max(num, BraveMathCollege.UnboundedLerp(num, clipByName.fps, clipFpsFraction));
			string empty = string.Empty;
			if (m_animationMap.ContainsKey(animationName))
			{
				empty = m_animationMap[animationName];
			}
			else
			{
				empty = animationName.Replace("_A", "_B");
				m_animationMap.Add(animationName, empty);
			}
			tk2dSpriteAnimationClip clipByName2 = childAnimator.GetClipByName(empty);
			if (!childAnimator.IsPlaying(clipByName2))
			{
				childAnimator.Play(clipByName2);
			}
			childAnimator.ClipFps = Mathf.Max(num, BraveMathCollege.UnboundedLerp(num, clipByName2.fps, clipFpsFraction));
		}
	}

	public void HandleTurnAnimation(Vector2 directionFromPreviousNode, Vector2 directionToNextNode)
	{
		IntVector2 intMajorAxis = BraveUtility.GetIntMajorAxis(directionFromPreviousNode);
		IntVector2 intMajorAxis2 = BraveUtility.GetIntMajorAxis(directionToNextNode);
		float clipFpsFraction = 2f;
		if (intMajorAxis == IntVector2.North)
		{
			if (intMajorAxis2 == IntVector2.East)
			{
				SetAnimation("minecart_turn_TL_VH_A", clipFpsFraction);
			}
			else if (intMajorAxis2 == IntVector2.West)
			{
				SetAnimation("minecart_turn_TR_VH_A", clipFpsFraction);
			}
		}
		else if (intMajorAxis == IntVector2.East)
		{
			if (intMajorAxis2 == IntVector2.North)
			{
				SetAnimation("minecart_turn_BR_HV_A", clipFpsFraction);
			}
			else if (intMajorAxis2 == IntVector2.South)
			{
				SetAnimation("minecart_turn_TR_HV_A", clipFpsFraction);
			}
		}
		else if (intMajorAxis == IntVector2.South)
		{
			if (intMajorAxis2 == IntVector2.East)
			{
				SetAnimation("minecart_turn_BL_VH_A", clipFpsFraction);
			}
			else if (intMajorAxis2 == IntVector2.West)
			{
				SetAnimation("minecart_turn_BR_VH_A", clipFpsFraction);
			}
		}
		else if (intMajorAxis == IntVector2.West)
		{
			if (intMajorAxis2 == IntVector2.North)
			{
				SetAnimation("minecart_turn_BL_HV_A", clipFpsFraction);
			}
			else if (intMajorAxis2 == IntVector2.South)
			{
				SetAnimation("minecart_turn_TL_HV_A", clipFpsFraction);
			}
		}
		m_handlingQueuedAnimation = true;
	}

	public void HandleCornerReached(Vector2 directionFromPreviousNode, Vector2 directionToNextNode, bool hasNextNode)
	{
		m_pathMover.PathSpeed = Mathf.Sign(m_pathMover.PathSpeed) * ((!(m_pathMover.AbsPathSpeed > MaxSpeedEnemy)) ? (m_pathMover.AbsPathSpeed + 1f) : m_pathMover.AbsPathSpeed);
		if (!m_hasHandledCornerAnimation)
		{
			HandleTurnAnimation(directionFromPreviousNode, directionToNextNode);
		}
		m_hasHandledCornerAnimation = false;
		if (!hasNextNode)
		{
			if (GameManager.Instance.Dungeon.CellSupportsFalling(base.specRigidbody.UnitCenter))
			{
				HandlePitFall(directionFromPreviousNode);
			}
			else
			{
				StartCoroutine(DelayedWarpToStart());
			}
		}
	}

	private IEnumerator DelayedWarpToStart()
	{
		yield return null;
		if (m_pathMover.PathSpeed < 0f)
		{
			m_pathMover.WarpToNearestPoint(m_pathMover.Path.nodes[m_pathMover.Path.nodes.Count - 1].RoomPosition + m_pathMover.nodeOffset + m_pathMover.RoomHandler.area.basePosition.ToVector2());
		}
		else
		{
			m_pathMover.WarpToNearestPoint(m_pathMover.Path.nodes[0].RoomPosition + m_pathMover.nodeOffset + m_pathMover.RoomHandler.area.basePosition.ToVector2());
		}
		if (occupation == CartOccupationState.CARGO)
		{
			Vector2 vector = carriedCargo.transform.position.XY() - carriedCargo.sprite.WorldBottomCenter;
			carriedCargo.transform.position = attachTransform.position + vector.ToVector3ZUp();
			carriedCargo.specRigidbody.Reinitialize();
		}
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		m_pathMover.ForcePathToNextNode();
	}

	private void UpdateAnimations()
	{
		Vector2 velocity = base.specRigidbody.Velocity;
		float num = ((occupation != CartOccupationState.PLAYER) ? MaxSpeedEnemy : MaxSpeedPlayer);
		float clipFpsFraction = m_pathMover.PathSpeed / num;
		if (m_handlingQueuedAnimation)
		{
			if (!base.spriteAnimator.IsPlaying(base.spriteAnimator.CurrentClip))
			{
				m_handlingQueuedAnimation = false;
			}
			if (base.spriteAnimator.CurrentClip == null || base.spriteAnimator.ClipFps <= 0f)
			{
				m_handlingQueuedAnimation = false;
			}
		}
		if (velocity.x == 0f && velocity.y == 0f)
		{
			base.spriteAnimator.Stop();
			childAnimator.Stop();
		}
		else if (Mathf.Abs(velocity.x) < Mathf.Abs(velocity.y))
		{
			if (!m_handlingQueuedAnimation)
			{
				SetAnimation(VerticalAnimationName, clipFpsFraction);
			}
		}
		else if (Mathf.Abs(velocity.y) < Mathf.Abs(velocity.x) && !m_handlingQueuedAnimation)
		{
			SetAnimation(HorizontalAnimationName, clipFpsFraction);
		}
	}

	private void LateUpdate()
	{
		if (!m_pathMover.Paused)
		{
			UpdateAnimations();
		}
		if (occupation == CartOccupationState.EMPTY)
		{
			if (base.sprite.HeightOffGround != -1f)
			{
				base.sprite.HeightOffGround = -1f;
				base.sprite.UpdateZDepth();
			}
			if (childAnimator.sprite.HeightOffGround != 0.125f)
			{
				childAnimator.sprite.IsPerpendicular = false;
				childAnimator.sprite.HeightOffGround = 0.125f;
				childAnimator.sprite.UpdateZDepth();
			}
			return;
		}
		if (Mathf.Abs(base.specRigidbody.Velocity.y) > Mathf.Abs(base.specRigidbody.Velocity.x))
		{
			if (base.sprite.HeightOffGround != -1.25f)
			{
				base.sprite.HeightOffGround = -1.25f;
				base.sprite.UpdateZDepth();
			}
		}
		else if (base.sprite.HeightOffGround != -0.6f)
		{
			base.sprite.HeightOffGround = -0.6f;
			base.sprite.UpdateZDepth();
		}
		if (childAnimator.sprite.HeightOffGround != -2.5f)
		{
			childAnimator.sprite.IsPerpendicular = true;
			childAnimator.sprite.HeightOffGround = -2.5f;
			childAnimator.sprite.UpdateZDepth();
		}
		childAnimator.sprite.UpdateZDepth();
	}

	public void BecomeCargoOccupied()
	{
		if (MoveCarriedCargoIntoCart)
		{
			Vector2 vector = carriedCargo.transform.position.XY() - carriedCargo.sprite.WorldBottomCenter;
			carriedCargo.transform.position = attachTransform.position + vector.ToVector3ZUp();
			carriedCargo.specRigidbody.Reinitialize();
		}
		carriedCargo.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
		base.specRigidbody.RegisterSpecificCollisionException(carriedCargo.specRigidbody);
		carriedCargo.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.LowObstacle));
		if ((bool)carriedCargo.knockbackDoer)
		{
			carriedCargo.knockbackDoer.knockbackMultiplier = 0f;
		}
		if ((bool)carriedCargo.minorBreakable && carriedCargo.minorBreakable.explodesOnBreak)
		{
			MinorBreakable obj = carriedCargo.minorBreakable;
			obj.OnBreak = (Action)Delegate.Combine(obj.OnBreak, (Action)delegate
			{
				DestroyMineCart();
			});
		}
		base.specRigidbody.RegisterCarriedRigidbody(carriedCargo.specRigidbody);
	}

	private void DestroyMineCart()
	{
		if ((bool)carriedCargo && (bool)carriedCargo.minorBreakable)
		{
			carriedCargo.transform.parent = null;
		}
		Evacuate();
		EvacuateSecondary();
		GetAbsoluteParentRoom().DeregisterInteractable(this);
		m_pathMover.Paused = true;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void BecomeOccupied(PlayerController player)
	{
		if (occupation == CartOccupationState.ENEMY)
		{
			return;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		SpriteOutlineManager.RemoveOutlineFromSprite(childAnimator.sprite);
		if (occupation == CartOccupationState.PLAYER)
		{
			if (!(player == m_rider))
			{
				player.currentMineCart = this;
				m_elapsedSecondary = 0f;
				if (player.IsDodgeRolling)
				{
					player.ForceStopDodgeRoll();
				}
				m_secondaryRider = player;
				player.CurrentInputState = PlayerInputState.NoMovement;
				player.ZeroVelocityThisFrame = true;
				AttachSecondaryRider();
				StaticReferenceManager.ActiveMineCarts.Add(player, this);
			}
		}
		else if (occupation == CartOccupationState.EMPTY)
		{
			m_elapsedOccupied = 0f;
			player.currentMineCart = this;
			if (player.IsDodgeRolling)
			{
				player.ForceStopDodgeRoll();
			}
			m_rider = player;
			occupation = CartOccupationState.PLAYER;
			base.specRigidbody.PixelColliders[0].CollisionLayerCollidableOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
			player.CurrentInputState = PlayerInputState.NoMovement;
			player.ZeroVelocityThisFrame = true;
			AttachRider();
			StaticReferenceManager.ActiveMineCarts.Add(player, this);
		}
	}

	public void BecomeOccupied(AIActor enemy)
	{
		if (occupation == CartOccupationState.EMPTY)
		{
			m_elapsedOccupied = 0f;
			m_rider = enemy;
			occupation = CartOccupationState.ENEMY;
			AttachRider();
		}
	}

	public void EvacuateSpecificPlayer(PlayerController p, bool usePitfallLogic = false)
	{
		if (m_rider == p)
		{
			Evacuate(false, usePitfallLogic);
		}
		if (m_secondaryRider == p)
		{
			EvacuateSecondary(false, usePitfallLogic);
		}
	}

	private void Evacuate(bool doRoll = false, bool isPitfalling = false)
	{
		if (occupation == CartOccupationState.EMPTY)
		{
			return;
		}
		if (occupation == CartOccupationState.CARGO)
		{
			if ((bool)carriedCargo.minorBreakable)
			{
				carriedCargo.minorBreakable.Break();
			}
			return;
		}
		if ((bool)m_rider)
		{
			base.specRigidbody.DeregisterCarriedRigidbody(m_rider.specRigidbody);
			if (occupation == CartOccupationState.PLAYER)
			{
				GameManager.Instance.MainCameraController.SetManualControl(false);
				PlayerController playerController = m_rider as PlayerController;
				playerController.currentMineCart = null;
				playerController.CurrentInputState = PlayerInputState.AllInput;
				StaticReferenceManager.ActiveMineCarts.Remove(playerController);
				if (doRoll)
				{
					playerController.ForceStartDodgeRoll();
					playerController.previousMineCart = this;
				}
				else if (isPitfalling)
				{
					playerController.previousMineCart = this;
				}
				else
				{
					Vector2 vector = m_pathMover.GetPositionOfNode(m_pathMover.CurrentIndex) - m_pathMover.transform.position.XY();
					Vector2 majorAxis = BraveUtility.GetMajorAxis(vector);
					if ((m_pathMover.GetPositionOfNode(m_pathMover.PreviousIndex) - m_pathMover.transform.position.XY()).magnitude < 1.5f)
					{
						majorAxis *= -1f;
					}
					Vector2 vector2 = majorAxis.normalized * -1f;
					if (m_primaryLerpCoroutine != null)
					{
						StopCoroutine(m_primaryLerpCoroutine);
					}
					m_primaryLerpCoroutine = StartCoroutine(HandleLerpCameraPlayerPosition(playerController, -vector2));
					playerController.transform.position = playerController.transform.position + (majorAxis.normalized * -1f).ToVector3ZUp();
					playerController.specRigidbody.Reinitialize();
				}
			}
			if ((bool)m_rider.knockbackDoer)
			{
				m_rider.knockbackDoer.knockbackMultiplier = 1f;
			}
			m_rider.FallingProhibited = false;
			m_rider.specRigidbody.DeregisterSpecificCollisionException(base.specRigidbody);
			base.specRigidbody.DeregisterSpecificCollisionException(m_rider.specRigidbody);
			m_rider.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.LowObstacle));
			m_rider.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
			base.specRigidbody.RegisterTemporaryCollisionException(m_rider.specRigidbody, 0.25f);
			m_rider = null;
		}
		if (m_secondaryRider == null)
		{
			occupation = CartOccupationState.EMPTY;
			base.specRigidbody.PixelColliders[0].CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
		}
	}

	private IEnumerator HandleLerpCameraPlayerPosition(PlayerController targetPlayer, Vector2 sourceOffset)
	{
		if (targetPlayer.IsPrimaryPlayer)
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = true;
			GameManager.Instance.MainCameraController.OverridePlayerOnePosition = targetPlayer.CenterPosition;
		}
		else
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = true;
			GameManager.Instance.MainCameraController.OverridePlayerTwoPosition = targetPlayer.CenterPosition;
		}
		yield return null;
		float elapsed = 0f;
		float duration = 0.2f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			Vector2 currentOffset = Vector2.Lerp(sourceOffset, Vector2.zero, elapsed / duration);
			if (targetPlayer.IsPrimaryPlayer)
			{
				GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = true;
				GameManager.Instance.MainCameraController.OverridePlayerOnePosition = targetPlayer.CenterPosition + currentOffset;
			}
			else
			{
				GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = true;
				GameManager.Instance.MainCameraController.OverridePlayerTwoPosition = targetPlayer.CenterPosition + currentOffset;
			}
			yield return null;
		}
		if (targetPlayer.IsPrimaryPlayer)
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = false;
		}
		else
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = false;
		}
	}

	private void EvacuateSecondary(bool doRoll = false, bool isPitfalling = false)
	{
		if (occupation != CartOccupationState.PLAYER || !(m_secondaryRider != null))
		{
			return;
		}
		GameManager.Instance.MainCameraController.SetManualControl(false);
		base.specRigidbody.DeregisterCarriedRigidbody(m_secondaryRider.specRigidbody);
		PlayerController playerController = m_secondaryRider as PlayerController;
		playerController.currentMineCart = null;
		playerController.CurrentInputState = PlayerInputState.AllInput;
		StaticReferenceManager.ActiveMineCarts.Remove(playerController);
		if (doRoll)
		{
			playerController.ForceStartDodgeRoll();
			playerController.previousMineCart = this;
		}
		else if (isPitfalling)
		{
			playerController.previousMineCart = this;
		}
		else
		{
			Vector2 majorAxis = BraveUtility.GetMajorAxis(m_pathMover.GetPositionOfNode(m_pathMover.CurrentIndex) - m_pathMover.transform.position.XY());
			Vector2 vector = majorAxis.normalized * -1f;
			if (m_secondaryLerpCoroutine != null)
			{
				StopCoroutine(m_secondaryLerpCoroutine);
			}
			m_secondaryLerpCoroutine = StartCoroutine(HandleLerpCameraPlayerPosition(playerController, -vector));
			playerController.transform.position = playerController.transform.position + (majorAxis.normalized * -1f).ToVector3ZUp();
			playerController.specRigidbody.Reinitialize();
		}
		if ((bool)m_secondaryRider.knockbackDoer)
		{
			m_secondaryRider.knockbackDoer.knockbackMultiplier = 1f;
		}
		m_secondaryRider.FallingProhibited = false;
		m_secondaryRider.specRigidbody.DeregisterSpecificCollisionException(base.specRigidbody);
		base.specRigidbody.DeregisterSpecificCollisionException(m_secondaryRider.specRigidbody);
		m_secondaryRider.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.LowObstacle));
		m_secondaryRider.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
		base.specRigidbody.RegisterTemporaryCollisionException(m_secondaryRider.specRigidbody, 0.25f);
		m_secondaryRider = null;
		if (m_rider == null)
		{
			occupation = CartOccupationState.EMPTY;
			base.specRigidbody.PixelColliders[0].CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
		}
	}

	protected void AttachSecondaryRider()
	{
		Vector2 vector = m_secondaryRider.transform.position.XY() - m_secondaryRider.specRigidbody.UnitBottomCenter;
		Vector2 vector2 = new Vector2(0.125f, 0.25f);
		vector += vector2;
		if (m_secondaryRider is PlayerController)
		{
			Vector2 vector3 = (attachTransform.position + vector.ToVector3ZUp()).XY() - m_secondaryRider.transform.position.XY();
			if (m_secondaryLerpCoroutine != null)
			{
				StopCoroutine(m_secondaryLerpCoroutine);
			}
			m_secondaryLerpCoroutine = StartCoroutine(HandleLerpCameraPlayerPosition(m_secondaryRider as PlayerController, -vector3));
		}
		m_secondaryRider.transform.position = attachTransform.position + vector.ToVector3ZUp();
		m_secondaryRider.specRigidbody.Reinitialize();
		m_secondaryRider.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
		base.specRigidbody.RegisterSpecificCollisionException(m_secondaryRider.specRigidbody);
		m_secondaryRider.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.LowObstacle));
		if ((bool)m_secondaryRider.knockbackDoer)
		{
			m_secondaryRider.knockbackDoer.knockbackMultiplier = 0f;
		}
		m_secondaryRider.FallingProhibited = true;
		base.specRigidbody.RegisterCarriedRigidbody(m_secondaryRider.specRigidbody);
	}

	public void ForceUpdatePositions()
	{
		EnsureRiderPosition();
	}

	protected void EnsureRiderPosition()
	{
		if (m_rider != null)
		{
			Vector2 vector = attachTransform.position.XY() + (m_rider.transform.position.XY() - m_rider.specRigidbody.UnitBottomCenter);
			float num = Vector2.Distance(vector, m_rider.transform.position);
			if (num > 0.0625f)
			{
				m_rider.transform.position = vector;
				m_rider.specRigidbody.Reinitialize();
			}
		}
		if (m_secondaryRider != null)
		{
			Vector2 vector2 = attachTransform.position.XY() + (m_secondaryRider.transform.position.XY() - m_secondaryRider.specRigidbody.UnitBottomCenter + new Vector2(0.125f, 0.25f));
			float num2 = Vector2.Distance(vector2, m_secondaryRider.transform.position);
			if (num2 > 0.0625f)
			{
				m_secondaryRider.transform.position = vector2;
				m_secondaryRider.specRigidbody.Reinitialize();
			}
		}
	}

	protected void AttachRider()
	{
		Vector2 vector = m_rider.transform.position.XY() - m_rider.specRigidbody.UnitBottomCenter;
		if (m_rider is PlayerController)
		{
			Vector2 vector2 = (attachTransform.position + vector.ToVector3ZUp()).XY() - m_rider.transform.position.XY();
			if (m_primaryLerpCoroutine != null)
			{
				StopCoroutine(m_primaryLerpCoroutine);
			}
			m_primaryLerpCoroutine = StartCoroutine(HandleLerpCameraPlayerPosition(m_rider as PlayerController, -vector2));
		}
		m_rider.transform.position = attachTransform.position + vector.ToVector3ZUp();
		m_rider.specRigidbody.Reinitialize();
		m_rider.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
		base.specRigidbody.RegisterSpecificCollisionException(m_rider.specRigidbody);
		m_rider.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.LowObstacle));
		if ((bool)m_rider.knockbackDoer)
		{
			m_rider.knockbackDoer.knockbackMultiplier = 0f;
		}
		m_rider.FallingProhibited = true;
		base.specRigidbody.RegisterCarriedRigidbody(m_rider.specRigidbody);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (occupation == CartOccupationState.ENEMY || occupation == CartOccupationState.CARGO)
		{
			return 1000f;
		}
		return Vector2.Distance(point, base.specRigidbody.UnitCenter) / 2f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (occupation != CartOccupationState.PLAYER || (!(interactor == m_rider) && !(interactor == m_secondaryRider)))
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 1.75f);
			SpriteOutlineManager.AddOutlineToSprite(childAnimator.sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		SpriteOutlineManager.RemoveOutlineFromSprite(childAnimator.sprite, true);
	}

	public void Interact(PlayerController interactor)
	{
		if (occupation != CartOccupationState.ENEMY && (occupation != CartOccupationState.PLAYER || !(m_rider == interactor)) && (occupation != CartOccupationState.PLAYER || !(m_secondaryRider == interactor)))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.RemoveOutlineFromSprite(childAnimator.sprite);
			BecomeOccupied(interactor);
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
