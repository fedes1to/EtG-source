using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class WallMimicController : CustomEngageDoer, IPlaceConfigurable
{
	public GameObject WallDisappearVFX;

	protected bool m_playerTrueSight;

	private Vector3 m_startingPos;

	private IntVector2 pos1;

	private IntVector2 pos2;

	private DungeonData.Direction m_facingDirection;

	private GameObject m_fakeWall;

	private GameObject m_fakeCeiling;

	private GunHandController[] m_hands;

	private GoopDoer m_goopDoer;

	private bool m_isHidden = true;

	private bool m_isFinished;

	private float m_collisionKnockbackStrength;

	private bool m_configured;

	protected bool CanAwaken
	{
		get
		{
			return m_isHidden && !PassiveItem.IsFlagSetAtAll(typeof(MimicRingItem));
		}
	}

	public override bool IsFinished
	{
		get
		{
			return m_isFinished;
		}
	}

	public void Awake()
	{
		ObjectVisibilityManager objectVisibilityManager = base.visibilityManager;
		objectVisibilityManager.OnToggleRenderers = (Action)Delegate.Combine(objectVisibilityManager.OnToggleRenderers, new Action(OnToggleRenderers));
		base.aiActor.IsGone = true;
	}

	public void Start()
	{
		if (!m_configured)
		{
			ConfigureOnPlacement(GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Ceil)));
		}
		base.transform.position = m_startingPos;
		base.specRigidbody.Reinitialize();
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = DungeonData.GetAngleFromDirection(m_facingDirection);
		m_fakeWall = SecretRoomBuilder.GenerateWallMesh(m_facingDirection, pos1, "Mimic Wall", null, true);
		if (base.aiActor.ParentRoom != null)
		{
			m_fakeWall.transform.parent = base.aiActor.ParentRoom.hierarchyParent;
		}
		m_fakeWall.transform.position = pos1.ToVector3().WithZ(pos1.y - 2) + Vector3.down;
		if (m_facingDirection == DungeonData.Direction.SOUTH)
		{
			StaticReferenceManager.AllShadowSystemDepthHavers.Add(m_fakeWall.transform);
		}
		else if (m_facingDirection == DungeonData.Direction.WEST)
		{
			m_fakeWall.transform.position = m_fakeWall.transform.position + new Vector3(-0.1875f, 0f);
		}
		m_fakeCeiling = SecretRoomBuilder.GenerateRoomCeilingMesh(GetCeilingTileSet(pos1, pos2, m_facingDirection), "Mimic Ceiling", null, true);
		if (base.aiActor.ParentRoom != null)
		{
			m_fakeCeiling.transform.parent = base.aiActor.ParentRoom.hierarchyParent;
		}
		m_fakeCeiling.transform.position = pos1.ToVector3().WithZ(pos1.y - 4);
		if (m_facingDirection == DungeonData.Direction.NORTH)
		{
			m_fakeCeiling.transform.position += new Vector3(-1f, 0f);
		}
		else if (m_facingDirection == DungeonData.Direction.SOUTH)
		{
			m_fakeCeiling.transform.position += new Vector3(-1f, 2f);
		}
		else if (m_facingDirection == DungeonData.Direction.EAST)
		{
			m_fakeCeiling.transform.position += new Vector3(-1f, 0f);
		}
		m_fakeCeiling.transform.position = m_fakeCeiling.transform.position.WithZ(m_fakeCeiling.transform.position.y - 5f);
		for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
		{
			base.specRigidbody.PixelColliders[i].Enabled = false;
		}
		if (m_facingDirection == DungeonData.Direction.NORTH)
		{
			base.specRigidbody.PixelColliders.Add(PixelCollider.CreateRectangle(CollisionLayer.LowObstacle, 38, 38, 32, 8));
			base.specRigidbody.PixelColliders.Add(PixelCollider.CreateRectangle(CollisionLayer.HighObstacle, 38, 54, 32, 8));
		}
		else if (m_facingDirection == DungeonData.Direction.SOUTH)
		{
			base.specRigidbody.PixelColliders.Add(PixelCollider.CreateRectangle(CollisionLayer.LowObstacle, 38, 38, 32, 16));
			base.specRigidbody.PixelColliders.Add(PixelCollider.CreateRectangle(CollisionLayer.HighObstacle, 38, 54, 32, 16));
		}
		else if (m_facingDirection == DungeonData.Direction.WEST || m_facingDirection == DungeonData.Direction.EAST)
		{
			base.specRigidbody.PixelColliders.Add(PixelCollider.CreateRectangle(CollisionLayer.LowObstacle, 46, 38, 16, 32));
			base.specRigidbody.PixelColliders.Add(PixelCollider.CreateRectangle(CollisionLayer.HighObstacle, 46, 38, 16, 32));
		}
		base.specRigidbody.ForceRegenerate();
		base.aiActor.HasDonePlayerEnterCheck = true;
		m_collisionKnockbackStrength = base.aiActor.CollisionKnockbackStrength;
		base.aiActor.CollisionKnockbackStrength = 0f;
		base.aiActor.CollisionDamage = 0f;
		m_goopDoer = GetComponent<GoopDoer>();
	}

	public void Update()
	{
		if (!CanAwaken)
		{
			return;
		}
		Vector2 unitBottomLeft = base.specRigidbody.PixelColliders[2].UnitBottomLeft;
		Vector2 max = unitBottomLeft;
		if (m_facingDirection == DungeonData.Direction.SOUTH)
		{
			unitBottomLeft += new Vector2(0f, -1.5f);
			max += new Vector2(2f, 0f);
		}
		else if (m_facingDirection == DungeonData.Direction.NORTH)
		{
			unitBottomLeft += new Vector2(0f, 1f);
			max += new Vector2(2f, 3f);
		}
		else if (m_facingDirection == DungeonData.Direction.WEST)
		{
			unitBottomLeft += new Vector2(-1.5f, 0f);
			max += new Vector2(0f, 2f);
		}
		else if (m_facingDirection == DungeonData.Direction.EAST)
		{
			unitBottomLeft += new Vector2(1f, 0f);
			max += new Vector2(2.5f, 2f);
		}
		bool flag = false;
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			if (playerController.CanDetectHiddenEnemies)
			{
				flag = true;
				if (!m_playerTrueSight)
				{
					m_playerTrueSight = true;
					base.aiActor.ToggleRenderers(true);
				}
			}
			if (!playerController || !playerController.healthHaver.IsAlive || playerController.IsGhost)
			{
				continue;
			}
			Vector2 unitCenter = playerController.specRigidbody.GetUnitCenter(ColliderType.Ground);
			if (!unitCenter.IsWithin(unitBottomLeft, max))
			{
				continue;
			}
			if ((bool)m_goopDoer)
			{
				Vector2 unitCenter2 = base.specRigidbody.PixelColliders[2].UnitCenter;
				if (m_facingDirection == DungeonData.Direction.NORTH)
				{
					unitCenter2 += Vector2.up;
				}
				DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(m_goopDoer.goopDefinition);
				goopManagerForGoopType.TimedAddGoopArc(unitCenter2, 3f, 90f, DungeonData.GetIntVector2FromDirection(m_facingDirection).ToVector2(), 0.2f);
			}
			StartCoroutine(BecomeMimic());
		}
		if (!flag && m_playerTrueSight)
		{
			m_playerTrueSight = false;
			base.aiActor.ToggleRenderers(false);
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)base.visibilityManager)
		{
			ObjectVisibilityManager objectVisibilityManager = base.visibilityManager;
			objectVisibilityManager.OnToggleRenderers = (Action)Delegate.Remove(objectVisibilityManager.OnToggleRenderers, new Action(OnToggleRenderers));
		}
		base.OnDestroy();
	}

	public override void StartIntro()
	{
		StartCoroutine(DoIntro());
	}

	private IEnumerator DoIntro()
	{
		base.aiActor.enabled = false;
		base.behaviorSpeculator.enabled = false;
		base.aiActor.ToggleRenderers(false);
		base.aiActor.IsGone = true;
		base.healthHaver.IsVulnerable = false;
		base.knockbackDoer.SetImmobile(true, "WallMimicController");
		m_hands = GetComponentsInChildren<GunHandController>();
		for (int i = 0; i < m_hands.Length; i++)
		{
			m_hands[i].gameObject.SetActive(false);
		}
		yield return null;
		base.aiActor.ToggleRenderers(false);
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "WallMimicController");
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnBeamCollision = (SpeculativeRigidbody.OnBeamCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnBeamCollision, new SpeculativeRigidbody.OnBeamCollisionDelegate(HandleBeamCollision));
		for (int j = 0; j < m_hands.Length; j++)
		{
			m_hands[j].gameObject.SetActive(false);
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (CanAwaken && (bool)rigidbodyCollision.OtherRigidbody.projectile)
		{
			StartCoroutine(BecomeMimic());
		}
	}

	private void HandleBeamCollision(BeamController beamController)
	{
		if (CanAwaken)
		{
			StartCoroutine(BecomeMimic());
		}
	}

	private void OnToggleRenderers()
	{
		if (m_isHidden && (bool)base.aiActor)
		{
			if ((bool)base.aiActor.sprite)
			{
				base.aiActor.sprite.renderer.enabled = false;
			}
			if ((bool)base.aiActor.ShadowObject)
			{
				base.aiActor.ShadowObject.GetComponent<Renderer>().enabled = false;
			}
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		Vector2 vector = base.transform.position.XY() + new Vector2((float)base.specRigidbody.GroundPixelCollider.ManualOffsetX / 16f, (float)base.specRigidbody.GroundPixelCollider.ManualOffsetY / 16f);
		Vector2 vector2 = vector.ToIntVector2().ToVector2();
		base.transform.position += (Vector3)(vector2 - vector);
		pos1 = vector2.ToIntVector2(VectorConversions.Floor);
		pos2 = pos1 + IntVector2.Right;
		m_facingDirection = GetFacingDirection(pos1, pos2);
		if (m_facingDirection == DungeonData.Direction.WEST)
		{
			pos1 = pos2;
			m_startingPos = base.transform.position + new Vector3(1f, 0f);
		}
		else if (m_facingDirection == DungeonData.Direction.EAST)
		{
			pos2 = pos1;
			m_startingPos = base.transform.position;
		}
		else
		{
			m_startingPos = base.transform.position + new Vector3(0.5f, 0f);
		}
		CellData cellData = GameManager.Instance.Dungeon.data[pos1];
		CellData cellData2 = GameManager.Instance.Dungeon.data[pos2];
		cellData.isSecretRoomCell = true;
		cellData2.isSecretRoomCell = true;
		cellData.forceDisallowGoop = true;
		cellData2.forceDisallowGoop = true;
		cellData.cellVisualData.preventFloorStamping = true;
		cellData2.cellVisualData.preventFloorStamping = true;
		cellData.isWallMimicHideout = true;
		cellData2.isWallMimicHideout = true;
		if (m_facingDirection == DungeonData.Direction.WEST || m_facingDirection == DungeonData.Direction.EAST)
		{
			GameManager.Instance.Dungeon.data[pos1 + IntVector2.Up].isSecretRoomCell = true;
		}
		m_configured = true;
	}

	private IEnumerator BecomeMimic()
	{
		if (m_hands == null)
		{
			StartCoroutine(DoIntro());
		}
		m_isHidden = false;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnBeamCollision = (SpeculativeRigidbody.OnBeamCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnBeamCollision, new SpeculativeRigidbody.OnBeamCollisionDelegate(HandleBeamCollision));
		AIAnimator tongueAnimator = base.aiAnimator.ChildAnimator;
		tongueAnimator.renderer.enabled = true;
		tongueAnimator.spriteAnimator.enabled = true;
		AIAnimator spitAnimator = tongueAnimator.ChildAnimator;
		spitAnimator.renderer.enabled = true;
		spitAnimator.spriteAnimator.enabled = true;
		tongueAnimator.PlayUntilFinished("spawn");
		float delay = tongueAnimator.CurrentClipLength;
		float timer2 = 0f;
		bool hasPlayedVFX = false;
		while (timer2 < delay)
		{
			yield return null;
			timer2 += BraveTime.DeltaTime;
			if (hasPlayedVFX || !(delay - timer2 < 0.1f))
			{
				continue;
			}
			hasPlayedVFX = true;
			if (!WallDisappearVFX)
			{
				continue;
			}
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.zero;
			switch (m_facingDirection)
			{
			case DungeonData.Direction.SOUTH:
				vector = new Vector2(0f, -1f);
				vector2 = new Vector2(0f, 1f);
				break;
			case DungeonData.Direction.EAST:
				vector = new Vector2(0f, -1f);
				vector2 = new Vector2(0f, 1f);
				break;
			case DungeonData.Direction.WEST:
				vector = new Vector2(0f, -1f);
				vector2 = new Vector2(0f, 1f);
				break;
			}
			Vector2 min = Vector2.Min(pos1.ToVector2(), pos2.ToVector2()) + vector;
			Vector2 max = Vector2.Max(pos1.ToVector2(), pos2.ToVector2()) + new Vector2(1f, 1f) + vector2;
			for (int i = 0; i < 5; i++)
			{
				Vector2 vector3 = BraveUtility.RandomVector2(min, max, new Vector2(0.25f, 0.25f)) + new Vector2(0f, 1f);
				GameObject gameObject = SpawnManager.SpawnVFX(WallDisappearVFX, vector3, Quaternion.identity);
				tk2dBaseSprite tk2dBaseSprite2 = ((!gameObject) ? null : gameObject.GetComponent<tk2dBaseSprite>());
				if ((bool)tk2dBaseSprite2)
				{
					tk2dBaseSprite2.HeightOffGround = 8f;
					tk2dBaseSprite2.UpdateZDepth();
				}
			}
		}
		PickupObject.ItemQuality targetQuality = ((UnityEngine.Random.value < 0.2f) ? PickupObject.ItemQuality.B : (BraveUtility.RandomBool() ? PickupObject.ItemQuality.D : PickupObject.ItemQuality.C));
		GenericLootTable lootTable = ((!BraveUtility.RandomBool()) ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable);
		PickupObject item = LootEngine.GetItemOfTypeAndQuality<PickupObject>(targetQuality, lootTable);
		if ((bool)item)
		{
			base.aiActor.AdditionalSafeItemDrops.Add(item);
		}
		base.aiActor.enabled = true;
		base.behaviorSpeculator.enabled = true;
		if (base.aiActor.ParentRoom != null && base.aiActor.ParentRoom.IsSealed)
		{
			base.aiActor.IgnoreForRoomClear = false;
		}
		int count = base.specRigidbody.PixelColliders.Count;
		for (int j = 0; j < count - 2; j++)
		{
			base.specRigidbody.PixelColliders[j].Enabled = true;
		}
		base.specRigidbody.PixelColliders.RemoveAt(count - 1);
		base.specRigidbody.PixelColliders.RemoveAt(count - 2);
		StaticReferenceManager.AllShadowSystemDepthHavers.Remove(m_fakeWall.transform);
		UnityEngine.Object.Destroy(m_fakeWall);
		UnityEngine.Object.Destroy(m_fakeCeiling);
		for (int k = 0; k < m_hands.Length; k++)
		{
			m_hands[k].gameObject.SetActive(true);
		}
		base.aiActor.ToggleRenderers(true);
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(true, "WallMimicController");
		}
		base.aiActor.IsGone = false;
		base.healthHaver.IsVulnerable = true;
		base.aiActor.State = AIActor.ActorState.Normal;
		for (int l = 0; l < m_hands.Length; l++)
		{
			m_hands[l].gameObject.SetActive(false);
		}
		m_isFinished = true;
		delay = 0.58f;
		timer2 = 0f;
		Vector3 targetPos = m_startingPos + DungeonData.GetIntVector2FromDirection(m_facingDirection).ToVector3();
		while (timer2 < delay)
		{
			base.aiAnimator.LockFacingDirection = true;
			base.aiAnimator.FacingDirection = DungeonData.GetAngleFromDirection(m_facingDirection);
			yield return null;
			timer2 += BraveTime.DeltaTime;
			base.transform.position = Vector3.Lerp(m_startingPos, targetPos, Mathf.InverseLerp(0.42f, 0.58f, timer2));
			base.specRigidbody.Reinitialize();
		}
		base.aiAnimator.LockFacingDirection = false;
		base.knockbackDoer.SetImmobile(false, "WallMimicController");
		base.aiActor.CollisionDamage = 0.5f;
		base.aiActor.CollisionKnockbackStrength = m_collisionKnockbackStrength;
	}

	private DungeonData.Direction GetFacingDirection(IntVector2 pos1, IntVector2 pos2)
	{
		DungeonData data = GameManager.Instance.Dungeon.data;
		if (data.isWall(pos1 + IntVector2.Down) && data.isWall(pos1 + IntVector2.Up))
		{
			return DungeonData.Direction.EAST;
		}
		if (data.isWall(pos2 + IntVector2.Down) && data.isWall(pos2 + IntVector2.Up))
		{
			return DungeonData.Direction.WEST;
		}
		if (data.isWall(pos1 + IntVector2.Down) && data.isWall(pos2 + IntVector2.Down))
		{
			return DungeonData.Direction.NORTH;
		}
		if (data.isWall(pos1 + IntVector2.Up) && data.isWall(pos2 + IntVector2.Up))
		{
			return DungeonData.Direction.SOUTH;
		}
		Debug.LogError("Not able to determine the direction of a wall mimic!");
		return DungeonData.Direction.SOUTH;
	}

	private HashSet<IntVector2> GetCeilingTileSet(IntVector2 pos1, IntVector2 pos2, DungeonData.Direction facingDirection)
	{
		IntVector2 intVector;
		IntVector2 intVector2;
		switch (facingDirection)
		{
		case DungeonData.Direction.NORTH:
			intVector = pos1 + new IntVector2(-1, 0);
			intVector2 = pos2 + new IntVector2(1, 1);
			break;
		case DungeonData.Direction.SOUTH:
			intVector = pos1 + new IntVector2(-1, 2);
			intVector2 = pos2 + new IntVector2(1, 3);
			break;
		case DungeonData.Direction.EAST:
			intVector = pos1 + new IntVector2(-1, 0);
			intVector2 = pos2 + new IntVector2(0, 3);
			break;
		case DungeonData.Direction.WEST:
			intVector = pos1 + new IntVector2(0, 0);
			intVector2 = pos2 + new IntVector2(1, 3);
			break;
		default:
			return null;
		}
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				IntVector2 item = new IntVector2(i, j);
				hashSet.Add(item);
			}
		}
		return hashSet;
	}
}
