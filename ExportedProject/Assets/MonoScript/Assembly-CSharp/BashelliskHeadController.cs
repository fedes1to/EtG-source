using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class BashelliskHeadController : BashelliskSegment
{
	[Header("Head-Specific Data")]
	public BashelliskBodyPickupController pickupPrefab;

	public List<BashelliskBodyController> segmentPrefabs;

	public List<int> segmentCounts;

	public int startingSegments;

	[Header("Spawn Pickup Data")]
	public float initialSpawnDelay = 20f;

	public float minSpawnDelay = 20f;

	public float maxSpawnDelay = 40f;

	public float pickupHealthScaler = 1f;

	public float pickupLurchSpeed = 13f;

	[NonSerialized]
	public LinkedList<BashelliskSegment> Body = new LinkedList<BashelliskSegment>();

	[NonSerialized]
	public readonly PooledLinkedList<BashelliskBodyPickupController> AvailablePickups = new PooledLinkedList<BashelliskBodyPickupController>();

	private readonly PooledLinkedList<Vector2> m_path = new PooledLinkedList<Vector2>();

	private float m_pathSegmentLength;

	private float m_spawnTimer;

	private float m_nextSpawnHealthThreshold = 0.8f;

	private List<Vector2> m_pickupLocations = new List<Vector2>();

	private List<Projectile> m_collidedProjectiles = new List<Projectile>();

	public bool CanPickup { get; set; }

	public bool ReinitMovementDirection { get; set; }

	public bool IsMidPickup { get; set; }

	public void Start()
	{
		Body.AddFirst(this);
		base.healthHaver.bodyRigidbodies = new List<SpeculativeRigidbody>();
		base.healthHaver.bodyRigidbodies.Add(base.specRigidbody);
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		PhysicsEngine.Instance.OnPostRigidbodyMovement += OnPostRigidbodyMovement;
		for (int i = 0; i < startingSegments; i++)
		{
			Grow();
		}
		m_path.AddLast(center.position);
		m_path.AddLast(center.position);
		m_pathSegmentLength = 0f;
		m_spawnTimer = initialSpawnDelay;
		List<BashelliskPickupSpawnPoint> componentsInRoom = base.aiActor.ParentRoom.GetComponentsInRoom<BashelliskPickupSpawnPoint>();
		for (int j = 0; j < componentsInRoom.Count; j++)
		{
			m_pickupLocations.Add(componentsInRoom[j].transform.position + new Vector3(-0.3125f, 0f));
		}
	}

	public void Update()
	{
		for (LinkedListNode<BashelliskSegment> first = Body.First; first != null; first = first.Next)
		{
			if (first.Value != null && (bool)first.Value.specRigidbody)
			{
				first.Value.specRigidbody.CollideWithOthers = base.specRigidbody.CollideWithOthers;
			}
		}
		if (base.aiActor.enabled)
		{
			bool flag = false;
			if (m_spawnTimer <= 0f)
			{
				flag = true;
			}
			if (base.healthHaver.GetCurrentHealthPercentage() < m_nextSpawnHealthThreshold)
			{
				flag = true;
			}
			if (flag)
			{
				SpawnBodyPickup();
				m_spawnTimer = UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay);
				m_nextSpawnHealthThreshold -= 0.2f;
			}
			if (AvailablePickups.Count == 0)
			{
				m_spawnTimer -= BraveTime.DeltaTime;
			}
		}
		for (int num = m_collidedProjectiles.Count - 1; num >= 0; num--)
		{
			Projectile projectile = m_collidedProjectiles[num];
			if (!projectile || !projectile.gameObject || !projectile.gameObject.activeSelf || projectile.specRigidbody.PrimaryPixelCollider == null)
			{
				m_collidedProjectiles.RemoveAt(num);
			}
			else
			{
				bool flag2 = false;
				LinkedListNode<BashelliskSegment> first = Body.First;
				PixelCollider primaryPixelCollider = projectile.specRigidbody.PrimaryPixelCollider;
				while (first != null && !flag2)
				{
					if (first.Value != null && first.Value.specRigidbody != null)
					{
						SpeculativeRigidbody speculativeRigidbody = first.Value.specRigidbody;
						for (int i = 0; i < speculativeRigidbody.PixelColliders.Count; i++)
						{
							PixelCollider pixelCollider = speculativeRigidbody.PixelColliders[i];
							if (pixelCollider.Enabled && pixelCollider.Overlaps(primaryPixelCollider))
							{
								flag2 = true;
								break;
							}
						}
					}
					first = first.Next;
				}
				if (!flag2)
				{
					m_collidedProjectiles.RemoveAt(num);
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		if (GameManager.HasInstance && (bool)this)
		{
			while (Body.Count > 0)
			{
				if ((bool)Body.First.Value)
				{
					UnityEngine.Object.Destroy(Body.First.Value.gameObject);
				}
				Body.RemoveFirst();
			}
			while (AvailablePickups.Count > 0)
			{
				if ((bool)AvailablePickups.First.Value)
				{
					UnityEngine.Object.Destroy(AvailablePickups.First.Value.gameObject);
				}
				AvailablePickups.RemoveFirst();
			}
		}
		m_path.ClearPool();
		if (PhysicsEngine.HasInstance)
		{
			PhysicsEngine.Instance.OnPostRigidbodyMovement -= OnPostRigidbodyMovement;
		}
		base.OnDestroy();
	}

	public void OnPostRigidbodyMovement()
	{
		if (!base.enabled || IsMidPickup)
		{
			return;
		}
		UpdatePath(center.position);
		if ((bool)next)
		{
			next.UpdatePosition(m_path, m_path.First, 0f, 0f);
		}
		base.aiAnimator.FacingDirection = (Body.First.Value.center.position - Body.First.Next.Value.center.position).XY().ToAngle();
		if (CanPickup)
		{
			PixelCollider hitboxPixelCollider = base.specRigidbody.HitboxPixelCollider;
			LinkedListNode<BashelliskBodyPickupController> first = AvailablePickups.First;
			while (first != null)
			{
				if (!first.Value)
				{
					LinkedListNode<BashelliskBodyPickupController> node = first;
					first = first.Next;
					AvailablePickups.Remove(node, true);
					continue;
				}
				PixelCollider hitboxPixelCollider2 = first.Value.specRigidbody.HitboxPixelCollider;
				if (hitboxPixelCollider2 != null && hitboxPixelCollider.Overlaps(hitboxPixelCollider2))
				{
					StartCoroutine(PickupCR(first.Value));
					AvailablePickups.Remove(first, true);
					break;
				}
				first = first.Next;
			}
		}
		else
		{
			if (AvailablePickups.Count <= 0)
			{
				return;
			}
			LinkedListNode<BashelliskBodyPickupController> first2 = AvailablePickups.First;
			while (first2 != null)
			{
				if (!first2.Value)
				{
					LinkedListNode<BashelliskBodyPickupController> node2 = first2;
					first2 = first2.Next;
					AvailablePickups.Remove(node2, true);
				}
				else
				{
					first2 = first2.Next;
				}
			}
		}
	}

	public void Grow()
	{
		Vector3 position = center.position;
		int num = Body.Count - 1;
		int b = segmentCounts.Count - 1;
		int num2 = segmentCounts.Count - 1;
		while (num2 >= 0 && num >= 0)
		{
			num -= segmentCounts[num2];
			b = num2;
			num2--;
		}
		b = Mathf.Max(0, b);
		BashelliskBodyController bashelliskBodyController = UnityEngine.Object.Instantiate(segmentPrefabs[b], center.position, Quaternion.identity);
		bashelliskBodyController.transform.position = position - bashelliskBodyController.center.localPosition;
		bashelliskBodyController.Init(this);
		if (Body.Count > 1)
		{
			bashelliskBodyController.next = Body.First.Next.Value;
			Body.First.Next.Value.previous = bashelliskBodyController;
		}
		next = bashelliskBodyController;
		bashelliskBodyController.previous = this;
		Body.AddAfter(Body.First, bashelliskBodyController);
		base.healthHaver.bodyRigidbodies.Add(bashelliskBodyController.specRigidbody);
		SpeculativeRigidbody speculativeRigidbody = bashelliskBodyController.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = bashelliskBodyController.specRigidbody;
		speculativeRigidbody2.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
	}

	private IEnumerator PickupCR(BashelliskBodyPickupController pickup)
	{
		Vector2 pickupCenter = pickup.center.position;
		IsMidPickup = true;
		base.aiActor.BehaviorVelocity = Vector2.zero;
		base.aiAnimator.PlayUntilCancelled("bite_open", true);
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = (pickup.center.position - center.position).XY().ToAngle();
		pickup.aiAnimator.PlayUntilCancelled("fear", true);
		pickup.behaviorSpeculator.InterruptAndDisable();
		while (base.aiAnimator.IsPlaying("bite_open"))
		{
			yield return null;
		}
		if (!pickup)
		{
			base.aiAnimator.PlayUntilFinished("bite_close");
			IsMidPickup = false;
			yield break;
		}
		pickup.healthHaver.minimumHealth = 1f;
		base.healthHaver.ApplyHealing(pickupHealthScaler * pickup.healthHaver.GetCurrentHealth());
		Grow();
		if ((bool)next)
		{
			next.aiAnimator.PlayUntilFinished("intro");
		}
		pickup.specRigidbody.enabled = false;
		base.sprite.AttachRenderer(pickup.sprite);
		pickup.sprite.HeightOffGround = -0.1f;
		pickup.sprite.UpdateZDepth();
		Vector2 baseOffset = pickupCenter - center.position.XY();
		Vector2 offset2 = baseOffset + baseOffset.normalized * 0.25f;
		base.specRigidbody.PathMode = true;
		base.specRigidbody.PathTarget = PhysicsEngine.UnitToPixel(base.specRigidbody.Position.UnitPosition + offset2);
		base.specRigidbody.PathSpeed = pickupLurchSpeed;
		base.aiAnimator.PlayUntilCancelled("bite_close", true);
		float timer2 = 0f;
		float maxSafeTime2 = offset2.magnitude / pickupLurchSpeed + 0.4f;
		while (base.specRigidbody.PathMode && timer2 < maxSafeTime2)
		{
			yield return null;
			timer2 += BraveTime.DeltaTime;
			if ((double)timer2 > ((double)maxSafeTime2 - 0.4) * 0.60000002384185791 && (bool)pickup)
			{
				pickup.transform.localScale = new Vector3(0.7f, 0.7f);
			}
		}
		base.specRigidbody.PathMode = false;
		base.aiActor.ApplyEffect(pickup.buffEffect);
		UnityEngine.Object.Destroy(pickup.gameObject);
		offset2 = baseOffset.normalized * -0.5f;
		base.specRigidbody.PathMode = true;
		base.specRigidbody.PathTarget = PhysicsEngine.UnitToPixel(base.specRigidbody.Position.UnitPosition + offset2);
		timer2 = 0f;
		maxSafeTime2 = offset2.magnitude / pickupLurchSpeed + 0.4f;
		while (base.specRigidbody.PathMode && timer2 < maxSafeTime2)
		{
			yield return null;
			timer2 += BraveTime.DeltaTime;
		}
		base.specRigidbody.PathMode = false;
		UpdatePath(center.position);
		if ((bool)next)
		{
			next.UpdatePosition(m_path, m_path.First, 0f, 0f);
		}
		while (base.aiAnimator.IsPlaying("bite_close"))
		{
			yield return null;
		}
		base.aiAnimator.EndAnimationIf("bite_close");
		IsMidPickup = false;
	}

	private void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		Projectile projectile = otherRigidbody.projectile;
		if ((bool)projectile && m_collidedProjectiles.Contains(projectile))
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		Projectile projectile = rigidbodyCollision.OtherRigidbody.projectile;
		if ((bool)projectile)
		{
			m_collidedProjectiles.Add(projectile);
		}
	}

	private void SpawnBodyPickup()
	{
		List<Vector2> list = new List<Vector2>();
		list.AddRange(m_pickupLocations.FindAll(delegate(Vector2 pos)
		{
			for (LinkedListNode<BashelliskBodyPickupController> first = AvailablePickups.First; first != null; first = first.Next)
			{
				if ((bool)first.Value && Vector2.Distance(pos, first.Value.center.transform.position.XY()) < 3f)
				{
					return false;
				}
			}
			return true;
		}));
		Vector2 position;
		if (list.Count > 0)
		{
			position = BraveUtility.RandomElement(list);
		}
		else
		{
			Vector2 vector = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
			Vector2 vector2 = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
			IntVector2 bottomLeft = vector.ToIntVector2(VectorConversions.Ceil);
			IntVector2 topRight = vector2.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
			CellValidator cellValidator = delegate(IntVector2 c)
			{
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
						{
							return false;
						}
					}
				}
				return (c.x >= bottomLeft.x && c.y >= bottomLeft.y && c.x + 1 <= topRight.x && c.y + 1 <= topRight.y) ? true : false;
			};
			IntVector2? randomAvailableCell = base.aiActor.ParentRoom.GetRandomAvailableCell(new IntVector2(2, 2), CellTypes.FLOOR, false, cellValidator);
			position = ((!randomAvailableCell.HasValue) ? ((Vector2)base.transform.position) : randomAvailableCell.Value.ToVector2());
		}
		AIActor aIActor = AIActor.Spawn(pickupPrefab.aiActor, position, base.aiActor.ParentRoom);
		aIActor.transform.position += new Vector3(1.25f, 0f);
		aIActor.specRigidbody.Reinitialize();
		AvailablePickups.AddLast(aIActor.GetComponent<BashelliskBodyPickupController>());
		base.specRigidbody.RegisterSpecificCollisionException(aIActor.specRigidbody);
	}

	private void SpawnBodyPickupAtMouse()
	{
		BashelliskBodyPickupController value = UnityEngine.Object.Instantiate(position: base.aiActor.ParentRoom.GetBestRewardLocation(new IntVector2(2, 2), BraveUtility.GetMousePosition(), false).ToVector2(), original: pickupPrefab, rotation: Quaternion.identity);
		AvailablePickups.AddLast(value);
	}

	private void UpdatePath(Vector2 newPosition)
	{
		float num = Vector2.Distance(newPosition, m_path.First.Value);
		for (LinkedListNode<BashelliskSegment> linkedListNode = Body.First.Next; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			linkedListNode.Value.PathDist += num;
		}
		if (m_pathSegmentLength > 0.5f)
		{
			m_path.AddFirst(newPosition);
		}
		else
		{
			m_path.First.Value = newPosition;
		}
		m_pathSegmentLength = Vector2.Distance(m_path.First.Value, m_path.First.Next.Value);
	}
}
