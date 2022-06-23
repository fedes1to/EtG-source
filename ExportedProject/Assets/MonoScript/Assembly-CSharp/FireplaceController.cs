using System;
using System.Collections;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class FireplaceController : BraveBehaviour, IPlayerInteractable
{
	public GoopDefinition oilGoop;

	public GameObject FireObject;

	public SpeculativeRigidbody GrateRigidbody;

	public Vector2 goopMinOffset;

	public Vector2 goopDimensions;

	public Transform InteractPoint;

	private bool m_flipped;

	private Action OnInteract;

	private IEnumerator Start()
	{
		yield return null;
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		yield return null;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
			if (roomHandler.IsSecretRoom && roomHandler.secretRoomManager != null && roomHandler.secretRoomManager.revealStyle == SecretRoomManager.SecretRoomRevealStyle.FireplacePuzzle)
			{
				FireplaceController fireplaceController = this;
				fireplaceController.OnInteract = (Action)Delegate.Combine(fireplaceController.OnInteract, new Action(roomHandler.secretRoomManager.OpenDoor));
			}
		}
		IntVector2 baseCellPosition2 = base.transform.position.IntXY(VectorConversions.Floor);
		for (int j = 0; j < 8; j++)
		{
			for (int k = 3; k < 7; k++)
			{
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(baseCellPosition2 + new IntVector2(j, k)))
				{
					GameManager.Instance.Dungeon.data[baseCellPosition2 + new IntVector2(j, k)].isOccupied = true;
				}
			}
			for (int l = 3; l < 6; l++)
			{
				if ((j != 3 || l != 2) && (j != 4 || l != 2))
				{
					GameManager.Instance.Dungeon.data[baseCellPosition2 + new IntVector2(j, l)].forceDisallowGoop = true;
				}
			}
			for (int m = 0; m < 5; m++)
			{
				GameManager.Instance.Dungeon.data[baseCellPosition2 + new IntVector2(j, m)].containsTrap = true;
			}
			if (j >= 2 && j <= 5)
			{
				for (int n = 2; n < 4; n++)
				{
					GameManager.Instance.Dungeon.data[baseCellPosition2 + new IntVector2(j, n)].IsFireplaceCell = true;
				}
			}
		}
		CellData cellData = GameManager.Instance.Dungeon.data[baseCellPosition2 + new IntVector2(3, 2)];
		cellData.OnCellGooped = (Action<CellData>)Delegate.Combine(cellData.OnCellGooped, new Action<CellData>(HandleGooped));
		CellData cellData2 = GameManager.Instance.Dungeon.data[baseCellPosition2 + new IntVector2(4, 2)];
		cellData2.OnCellGooped = (Action<CellData>)Delegate.Combine(cellData2.OnCellGooped, new Action<CellData>(HandleGooped));
		SpeculativeRigidbody speculativeRigidbody = GrateRigidbody.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleCollision));
		SpeculativeRigidbody speculativeRigidbody2 = GrateRigidbody.specRigidbody;
		speculativeRigidbody2.OnHitByBeam = (Action<BasicBeamController>)Delegate.Combine(speculativeRigidbody2.OnHitByBeam, new Action<BasicBeamController>(HandleBeamCollision));
		yield return new WaitForSeconds(0.25f);
		baseCellPosition2 = base.transform.position.IntXY(VectorConversions.Floor);
		OccupiedCells oCells = new OccupiedCells(baseCellPosition2 + new IntVector2(0, 3), new IntVector2(8, 4), base.transform.position.GetAbsoluteRoom());
		oCells.FlagCells();
		Pathfinder.Instance.FlagRoomAsDirty(base.transform.position.GetAbsoluteRoom());
	}

	private void HandleGooped(CellData obj)
	{
		if (obj == null)
		{
			return;
		}
		IntVector2 key = (obj.position.ToCenterVector2() / DeadlyDeadlyGoopManager.GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
		if (DeadlyDeadlyGoopManager.allGoopPositionMap.ContainsKey(key))
		{
			DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = DeadlyDeadlyGoopManager.allGoopPositionMap[key];
			GoopDefinition goopDefinition = deadlyDeadlyGoopManager.goopDefinition;
			if (!goopDefinition.CanBeIgnited)
			{
				OnFireExtinguished();
			}
		}
	}

	private void HandleBeamCollision(BasicBeamController obj)
	{
		GoopModifier component = obj.GetComponent<GoopModifier>();
		if ((bool)component && component.goopDefinition != null && !component.goopDefinition.CanBeIgnited)
		{
			OnFireExtinguished();
		}
	}

	private void HandleCollision(CollisionData rigidbodyCollision)
	{
		Projectile projectile = rigidbodyCollision.OtherRigidbody.projectile;
		if ((bool)projectile && (bool)projectile.GetComponent<GoopModifier>())
		{
			GoopModifier component = projectile.GetComponent<GoopModifier>();
			if (component.goopDefinition != null && !component.goopDefinition.CanBeIgnited)
			{
				OnFireExtinguished();
			}
		}
	}

	private void OnFireExtinguished()
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		CellData cellData = GameManager.Instance.Dungeon.data[intVector + new IntVector2(3, 2)];
		cellData.OnCellGooped = (Action<CellData>)Delegate.Remove(cellData.OnCellGooped, new Action<CellData>(HandleGooped));
		CellData cellData2 = GameManager.Instance.Dungeon.data[intVector + new IntVector2(4, 2)];
		cellData2.OnCellGooped = (Action<CellData>)Delegate.Remove(cellData2.OnCellGooped, new Action<CellData>(HandleGooped));
		SpeculativeRigidbody speculativeRigidbody = GrateRigidbody.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleCollision));
		SpeculativeRigidbody speculativeRigidbody2 = GrateRigidbody.specRigidbody;
		speculativeRigidbody2.OnHitByBeam = (Action<BasicBeamController>)Delegate.Remove(speculativeRigidbody2.OnHitByBeam, new Action<BasicBeamController>(HandleBeamCollision));
		GrateRigidbody.enabled = false;
		GrateRigidbody.spriteAnimator.Play();
		tk2dBaseSprite component = FireObject.GetComponent<tk2dBaseSprite>();
		GlobalSparksDoer.DoRandomParticleBurst(25, component.WorldBottomLeft, component.WorldTopRight, Vector3.up, 70f, 0.5f, null, 0.75f, new Color(4f, 0.3f, 0f), GlobalSparksDoer.SparksType.SOLID_SPARKLES);
		GlobalSparksDoer.DoRandomParticleBurst(25, component.WorldBottomLeft, component.WorldTopRight, Vector3.up, 70f, 0.5f, null, 1.5f, new Color(4f, 0.3f, 0f), GlobalSparksDoer.SparksType.SOLID_SPARKLES);
		GlobalSparksDoer.DoRandomParticleBurst(25, component.WorldBottomLeft, component.WorldTopRight, Vector3.up, 70f, 0.5f, null, 2.25f, new Color(4f, 0.3f, 0f), GlobalSparksDoer.SparksType.SOLID_SPARKLES);
		GlobalSparksDoer.DoRandomParticleBurst(25, component.WorldBottomLeft, component.WorldTopRight, Vector3.up, 70f, 0.5f, null, 3f, new Color(4f, 0.3f, 0f), GlobalSparksDoer.SparksType.SOLID_SPARKLES);
		FireObject.SetActive(false);
	}

	private void Update()
	{
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		return Vector2.Distance(point, InteractPoint.position.XY());
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_flipped)
		{
			m_flipped = true;
			AkSoundEngine.PostEvent("Play_OBJ_secret_switch_01", base.gameObject);
			AkSoundEngine.PostEvent("Play_OBJ_wall_reveal_01", base.gameObject);
			ScreenShakeSettings shakesettings = new ScreenShakeSettings(0.2f, 7f, 1f, 0f, Vector2.left);
			GameManager.Instance.MainCameraController.DoScreenShake(shakesettings, null);
			if (OnInteract != null)
			{
				OnInteract();
			}
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		Debug.Log("ENTERED RANGE");
	}

	public void OnExitRange(PlayerController interactor)
	{
		Debug.Log("EXITED RANGE");
	}
}
