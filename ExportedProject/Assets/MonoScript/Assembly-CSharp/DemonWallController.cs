using System;
using Dungeonator;
using UnityEngine;

public class DemonWallController : BraveBehaviour
{
	private enum State
	{
		Intro,
		LockCamera,
		Dead
	}

	private State m_state;

	private bool m_isMotionRestricted;

	private int m_cachedCameraMinY;

	private RoomHandler m_room;

	private int m_leftId;

	private int m_rightId;

	public bool IsCameraLocked
	{
		get
		{
			return m_state == State.LockCamera;
		}
	}

	public Vector2 CameraPos { get; set; }

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		base.aiActor.ManualKnockbackHandling = true;
		base.aiActor.ParentRoom.Entered += PlayerEnteredRoom;
		base.aiActor.healthHaver.OnPreDeath += OnPreDeath;
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		Vector2 vector = new Vector2(0f, base.specRigidbody.HitboxPixelCollider.UnitDimensions.y - mainCameraController.Camera.orthographicSize + 0.5f);
		CameraPos = base.specRigidbody.UnitCenter + vector;
		m_room = base.aiActor.ParentRoom;
	}

	public void Update()
	{
		if (m_state == State.Intro)
		{
			if (base.specRigidbody.CollideWithOthers)
			{
				m_state = State.LockCamera;
				CameraController mainCameraController = GameManager.Instance.MainCameraController;
				mainCameraController.SetManualControl(true);
				mainCameraController.OverridePosition = CameraPos;
				Vector2 unitBottomCenter = base.specRigidbody.UnitBottomCenter;
				m_leftId = DeadlyDeadlyGoopManager.RegisterUngoopableCircle(unitBottomCenter + new Vector2(-1.5f, 1.5f), 1.5f);
				m_rightId = DeadlyDeadlyGoopManager.RegisterUngoopableCircle(unitBottomCenter + new Vector2(1.5f, 1.5f), 1.5f);
			}
		}
		else if (m_state == State.LockCamera)
		{
			Vector2 unitBottomCenter2 = base.specRigidbody.UnitBottomCenter;
			DeadlyDeadlyGoopManager.UpdateUngoopableCircle(m_leftId, unitBottomCenter2 + new Vector2(-1.5f, 1.5f), 1.5f);
			DeadlyDeadlyGoopManager.UpdateUngoopableCircle(m_rightId, unitBottomCenter2 + new Vector2(1.5f, 1.5f), 1.5f);
			MarkInaccessible(true);
		}
		m_cachedCameraMinY = PhysicsEngine.UnitToPixel(BraveUtility.ViewportToWorldpoint(new Vector2(0.5f, 0f), ViewportType.Camera).y);
	}

	protected override void OnDestroy()
	{
		RestrictMotion(false);
		ModifyCamera(false);
		MarkInaccessible(false);
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		}
		if ((bool)base.aiActor && base.aiActor.ParentRoom != null)
		{
			base.aiActor.ParentRoom.Entered -= PlayerEnteredRoom;
		}
		if ((bool)base.aiActor && (bool)base.aiActor.healthHaver)
		{
			base.aiActor.healthHaver.OnPreDeath -= OnPreDeath;
		}
		base.OnDestroy();
	}

	private void OnCollision(CollisionData rigidbodyCollision)
	{
		if (rigidbodyCollision.collisionType != 0 || base.aiActor.IsFrozen)
		{
			return;
		}
		PlayerController component = rigidbodyCollision.OtherRigidbody.GetComponent<PlayerController>();
		MajorBreakable majorBreakable = rigidbodyCollision.OtherRigidbody.majorBreakable;
		AIActor aIActor = rigidbodyCollision.OtherRigidbody.aiActor;
		if (!base.healthHaver.IsDead && component != null)
		{
			Vector2 vector = -Vector2.up;
			IntVector2 intVector = component.specRigidbody.UnitBottomCenter.ToIntVector2(VectorConversions.Floor);
			if (GameManager.Instance.Dungeon.data.isTopWall(intVector.x, intVector.y))
			{
				component.healthHaver.ApplyDamage(1000f, vector, base.aiActor.GetActorName(), CoreDamageTypes.None, DamageCategory.Collision, true);
			}
			component.healthHaver.ApplyDamage(base.aiActor.CollisionDamage, vector, base.aiActor.GetActorName(), CoreDamageTypes.None, DamageCategory.Collision);
			component.knockbackDoer.ApplySourcedKnockback(vector, base.aiActor.CollisionKnockbackStrength, base.gameObject, true);
			if ((bool)base.knockbackDoer)
			{
				base.knockbackDoer.ApplySourcedKnockback(-vector, component.collisionKnockbackStrength, base.gameObject);
			}
			base.aiActor.CollisionVFX.SpawnAtPosition(rigidbodyCollision.Contact, 0f, null, Vector2.zero, Vector2.zero, 2f);
			component.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
		}
		if ((bool)aIActor && (bool)aIActor.CompanionOwner)
		{
			Debug.LogError("knocking back companion");
			aIActor.knockbackDoer.ApplySourcedKnockback(Vector2.down, 50f, base.gameObject, true);
		}
		if ((bool)majorBreakable)
		{
			majorBreakable.ApplyDamage(1000f, Vector2.down, true, false, true);
		}
	}

	private void PlayerEnteredRoom(PlayerController playerController)
	{
		RestrictMotion(true);
	}

	private void OnPreDeath(Vector2 finalDirection)
	{
		RestrictMotion(false);
		MarkInaccessible(false);
		base.aiActor.ParentRoom.Entered -= PlayerEnteredRoom;
		if (m_state == State.LockCamera)
		{
			m_state = State.Dead;
		}
	}

	private void PlayerMovementRestrictor(SpeculativeRigidbody playerSpecRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		int maxY = playerSpecRigidbody.PixelColliders[1].MaxY;
		int minY = base.specRigidbody.PixelColliders[1].MinY;
		if (maxY + pixelOffset.y >= minY && pixelOffset.y > prevPixelOffset.y)
		{
			validLocation = false;
			return;
		}
		IntVector2 intVector = pixelOffset - prevPixelOffset;
		CellArea area = base.aiActor.ParentRoom.area;
		if (intVector.x < 0)
		{
			int num = playerSpecRigidbody.PixelColliders[0].MinX + pixelOffset.x;
			int num2 = area.basePosition.x * 16;
			if (num < num2)
			{
				validLocation = false;
				return;
			}
		}
		else if (intVector.x > 0)
		{
			int num3 = playerSpecRigidbody.PixelColliders[0].MaxX + pixelOffset.x;
			int num4 = (area.basePosition.x + area.dimensions.x) * 16 - 1;
			if (num3 > num4)
			{
				validLocation = false;
				return;
			}
		}
		if (intVector.y < 0)
		{
			int num5 = playerSpecRigidbody.PixelColliders[0].MinY + pixelOffset.y;
			if (num5 < m_cachedCameraMinY)
			{
				validLocation = false;
			}
		}
	}

	private void RestrictMotion(bool value)
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

	public void ModifyCamera(bool value)
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach)
		{
			if (value)
			{
				GameManager.Instance.MainCameraController.SetManualControl(true, false);
			}
			else
			{
				GameManager.Instance.MainCameraController.SetManualControl(false);
			}
		}
	}

	private void MarkInaccessible(bool inaccessible)
	{
		if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach || !base.aiActor || base.aiActor.ParentRoom == null)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		IntVector2 basePosition = m_room.area.basePosition;
		IntVector2 intVector = m_room.area.basePosition + base.aiActor.ParentRoom.area.dimensions - IntVector2.One;
		if (inaccessible && (bool)base.specRigidbody)
		{
			basePosition.y = (int)base.specRigidbody.UnitBottomCenter.y - 3;
		}
		for (int i = basePosition.x; i <= intVector.x; i++)
		{
			for (int j = basePosition.y; j <= intVector.y; j++)
			{
				if (data.CheckInBoundsAndValid(i, j))
				{
					data[i, j].IsPlayerInaccessible = inaccessible;
				}
			}
		}
	}
}
