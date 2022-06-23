using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class WarpWingPortalController : BraveBehaviour, IPlayerInteractable
{
	public bool UsesTriggerZone;

	[NonSerialized]
	public WarpWingPortalController pairedPortal;

	[NonSerialized]
	public RoomHandler parentRoom;

	[NonSerialized]
	public RuntimeRoomExitData parentExit;

	[NonSerialized]
	private float FailChance;

	[NonSerialized]
	public WarpWingPortalController failPortal;

	private bool m_justUsed;

	private IEnumerator Start()
	{
		yield return null;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.RESOURCEFUL_RAT)
		{
			HandleResourcefulRatFlowSetup();
		}
		base.sprite.UpdateZDepth();
		if (UsesTriggerZone)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEntered));
		}
	}

	private void Update()
	{
		m_justUsed = false;
	}

	private void HandleTriggerEntered(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!m_justUsed)
		{
			PlayerController component = specRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				DoTeleport(component);
			}
		}
	}

	private void HandleResourcefulRatFlowSetup()
	{
		DungeonData.Direction[] resourcefulRatSolution = GameManager.GetResourcefulRatSolution();
		if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_ChainRoom_01")
		{
			if (resourcefulRatSolution[0] == parentExit.referencedExit.exitDirection)
			{
				pairedPortal = GetDirectionalPortalFromRoom(2, DungeonData.Direction.WEST);
				return;
			}
			AttachResourcefulRatFailRoom();
			pairedPortal = GetRatFirstRoomEntrancePortal();
		}
		else if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_ChainRoom_02")
		{
			if (resourcefulRatSolution[1] == parentExit.referencedExit.exitDirection)
			{
				pairedPortal = GetDirectionalPortalFromRoom(3, DungeonData.Direction.WEST);
				return;
			}
			AttachResourcefulRatFailRoom();
			pairedPortal = GetRatFirstRoomEntrancePortal();
		}
		else if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_ChainRoom_03")
		{
			if (resourcefulRatSolution[2] == parentExit.referencedExit.exitDirection)
			{
				pairedPortal = GetDirectionalPortalFromRoom(4, DungeonData.Direction.WEST);
				return;
			}
			AttachResourcefulRatFailRoom();
			pairedPortal = GetRatFirstRoomEntrancePortal();
		}
		else if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_ChainRoom_04")
		{
			if (resourcefulRatSolution[3] == parentExit.referencedExit.exitDirection)
			{
				pairedPortal = GetDirectionalPortalFromRoom(5, DungeonData.Direction.WEST);
				return;
			}
			AttachResourcefulRatFailRoom();
			pairedPortal = GetRatFirstRoomEntrancePortal();
		}
		else if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_ChainRoom_05")
		{
			if (resourcefulRatSolution[4] == parentExit.referencedExit.exitDirection)
			{
				pairedPortal = GetDirectionalPortalFromRoom(6, DungeonData.Direction.WEST);
				return;
			}
			AttachResourcefulRatFailRoom();
			pairedPortal = GetRatFirstRoomEntrancePortal();
		}
		else if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_ChainRoom_06")
		{
			if (resourcefulRatSolution[5] == parentExit.referencedExit.exitDirection)
			{
				pairedPortal = GetDirectionalPortalFromRoom(7, DungeonData.Direction.WEST);
				return;
			}
			AttachResourcefulRatFailRoom();
			pairedPortal = GetRatFirstRoomEntrancePortal();
		}
		else if (parentRoom.area.PrototypeRoomName == "ResourcefulRat_FailRoom")
		{
			AttachResourcefulRatFailRoom();
			WarpWingPortalController ratFirstRoomEntrancePortal = GetRatFirstRoomEntrancePortal();
			if (ratFirstRoomEntrancePortal != null)
			{
				pairedPortal = ratFirstRoomEntrancePortal;
			}
		}
	}

	private WarpWingPortalController GetDirectionalPortalFromRoom(int roomIndex, DungeonData.Direction dir)
	{
		WarpWingPortalController result = null;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			string text = "ResourcefulRat_ChainRoom_0" + roomIndex;
			if (roomIndex > 6)
			{
				text = "Boss Foyer";
			}
			if (!(GameManager.Instance.Dungeon.data.rooms[i].area.PrototypeRoomName == text))
			{
				continue;
			}
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
			for (int j = 0; j < roomHandler.area.instanceUsedExits.Count; j++)
			{
				if (roomHandler.area.instanceUsedExits[j].exitDirection == dir)
				{
					result = roomHandler.area.exitToLocalDataMap[roomHandler.area.instanceUsedExits[j]].warpWingPortal;
				}
			}
		}
		return result;
	}

	private WarpWingPortalController GetRatFirstRoomEntrancePortal()
	{
		return GetDirectionalPortalFromRoom(2, DungeonData.Direction.WEST);
	}

	private void AttachResourcefulRatFailRoom()
	{
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			if (!(GameManager.Instance.Dungeon.data.rooms[i].area.PrototypeRoomName == "ResourcefulRat_FailExit"))
			{
				continue;
			}
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
			for (int j = 0; j < roomHandler.area.instanceUsedExits.Count; j++)
			{
				WarpWingPortalController warpWingPortal = roomHandler.area.exitToLocalDataMap[roomHandler.area.instanceUsedExits[j]].warpWingPortal;
				if (warpWingPortal != null)
				{
					warpWingPortal.pairedPortal = warpWingPortal;
					failPortal = warpWingPortal;
					FailChance = 0.25f;
					break;
				}
			}
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (UsesTriggerZone)
		{
			return 1000f;
		}
		if (pairedPortal == this || pairedPortal == null)
		{
			return 1000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	private IEnumerator HandleDelayedAnimationTrigger(tk2dSpriteAnimator target)
	{
		yield return new WaitForSeconds(0.6f);
		target.Play("resourceful_rat_teleport_in");
	}

	public void Interact(PlayerController player)
	{
		DoTeleport(player);
	}

	private IEnumerator MarkUsed(WarpWingPortalController targetPortal)
	{
		float elapsed = 0f;
		while (elapsed < 2f)
		{
			elapsed += BraveTime.DeltaTime;
			targetPortal.m_justUsed = true;
			yield return null;
		}
	}

	private void DoTeleport(PlayerController player)
	{
		if (!(pairedPortal == this) && !(pairedPortal == null))
		{
			if (failPortal != null && UnityEngine.Random.value < FailChance)
			{
				base.spriteAnimator.Play("resourceful_rat_teleport_out");
				StartCoroutine(HandleDelayedAnimationTrigger(failPortal.spriteAnimator));
				StartCoroutine(MarkUsed(this));
				StartCoroutine(MarkUsed(pairedPortal));
				player.TeleportToPoint(failPortal.sprite.WorldCenter, true);
			}
			else
			{
				base.spriteAnimator.Play("resourceful_rat_teleport_out");
				StartCoroutine(HandleDelayedAnimationTrigger(pairedPortal.spriteAnimator));
				StartCoroutine(MarkUsed(this));
				StartCoroutine(MarkUsed(pairedPortal));
				player.TeleportToPoint(pairedPortal.sprite.WorldCenter, true);
			}
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
