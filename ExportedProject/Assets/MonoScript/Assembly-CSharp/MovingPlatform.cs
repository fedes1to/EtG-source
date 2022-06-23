using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class MovingPlatform : DungeonPlaceableBehaviour, IDwarfDrawable, IPlaceConfigurable
{
	public IntVector2 Size;

	[DwarfConfigurable]
	public bool UsesDwarfConfigurableSize;

	[DwarfConfigurable]
	public float DwarfConfigurableWidth = 3f;

	[DwarfConfigurable]
	public float DwarfConfigurableHeight = 3f;

	public Transform StencilQuad;

	public bool StaticForPitfall;

	public bool AllowsForGoop;

	private tk2dSlicedSprite m_sprite;

	private RoomHandler m_room;

	public IEnumerator Start()
	{
		if (UsesDwarfConfigurableSize)
		{
			Size = new IntVector2((int)DwarfConfigurableWidth, (int)DwarfConfigurableHeight);
		}
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
		speculativeRigidbody3.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody3.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(OnExitTrigger));
		SpeculativeRigidbody speculativeRigidbody4 = base.specRigidbody;
		speculativeRigidbody4.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(speculativeRigidbody4.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(OnPostRigidbodyMovement));
		if (UsesDwarfConfigurableSize)
		{
			ForceUpdateSize();
		}
		m_sprite = GetComponent<tk2dSlicedSprite>();
		MarkCells();
	}

	public void ForceUpdateSize()
	{
		tk2dSlicedSprite component = GetComponent<tk2dSlicedSprite>();
		if (component != null)
		{
			component.dimensions = new Vector2(16 * Size.x, 16 * Size.y);
		}
		PixelCollider pixelCollider = base.specRigidbody.PixelColliders.Find((PixelCollider c) => c.CollisionLayer == CollisionLayer.MovingPlatform);
		if (pixelCollider == null)
		{
			pixelCollider = new PixelCollider();
			pixelCollider.CollisionLayer = CollisionLayer.MovingPlatform;
			base.specRigidbody.PixelColliders.Add(pixelCollider);
		}
		pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
		pixelCollider.ManualOffsetX = 0;
		pixelCollider.ManualOffsetY = 0;
		pixelCollider.ManualWidth = Size.x * 16;
		pixelCollider.ManualHeight = Size.y * 16;
		pixelCollider.Regenerate(base.specRigidbody.transform);
		base.specRigidbody.Reinitialize();
		if (StencilQuad != null)
		{
			float num = component.dimensions.x / 16f;
			float num2 = (component.dimensions.y + 7f) / 16f;
			float num3 = 0.4375f;
			StencilQuad.localScale = new Vector3(num, num2, 1f);
			StencilQuad.transform.localPosition = new Vector3(num / 2f, num2 / 2f - num3, 0f);
		}
	}

	protected override void OnDestroy()
	{
		if (m_room != null && (bool)base.specRigidbody && m_room.RoomMovingPlatforms != null)
		{
			m_room.RoomMovingPlatforms.Remove(base.specRigidbody);
		}
		base.OnDestroy();
	}

	private void OnEnterTrigger(SpeculativeRigidbody obj, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if ((bool)obj.gameActor && !obj.gameActor.SupportingPlatforms.Contains(this))
		{
			obj.gameActor.SupportingPlatforms.Add(this);
		}
		base.specRigidbody.RegisterCarriedRigidbody(obj);
	}

	private void OnTriggerCollision(SpeculativeRigidbody obj, SpeculativeRigidbody source, CollisionData collisionData)
	{
	}

	private void OnExitTrigger(SpeculativeRigidbody obj, SpeculativeRigidbody source)
	{
		if ((bool)obj && (bool)obj.gameActor)
		{
			obj.gameActor.SupportingPlatforms.Remove(this);
		}
		if ((bool)this)
		{
			base.specRigidbody.DeregisterCarriedRigidbody(obj);
		}
	}

	public void ClearCells()
	{
		PixelCollider primaryPixelCollider = base.specRigidbody.PrimaryPixelCollider;
		IntVector2 intVector = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.LowerLeft).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.UpperRight).ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				List<SpeculativeRigidbody> platforms = GameManager.Instance.Dungeon.data.cellData[i][j].platforms;
				if (platforms != null)
				{
					platforms.Remove(base.specRigidbody);
				}
				if (AllowsForGoop)
				{
					DeadlyDeadlyGoopManager.ForceClearGoopsInCell(new IntVector2(i, j));
					GameManager.Instance.Dungeon.data.cellData[i][j].forceAllowGoop = false;
				}
			}
		}
	}

	public void MarkCells()
	{
		PixelCollider primaryPixelCollider = base.specRigidbody.PrimaryPixelCollider;
		IntVector2 intVector = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.LowerLeft).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.UpperRight).ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				if (GameManager.Instance.Dungeon.data.cellData[i][j] != null)
				{
					List<SpeculativeRigidbody> list = GameManager.Instance.Dungeon.data.cellData[i][j].platforms;
					if (list == null)
					{
						list = new List<SpeculativeRigidbody>();
						GameManager.Instance.Dungeon.data.cellData[i][j].platforms = list;
					}
					if (!list.Contains(base.specRigidbody))
					{
						list.Add(base.specRigidbody);
					}
					if (AllowsForGoop)
					{
						GameManager.Instance.Dungeon.data.cellData[i][j].forceAllowGoop = true;
					}
				}
			}
		}
	}

	private void OnPostRigidbodyMovement(SpeculativeRigidbody specRigidbody, Vector2 unitDelta, IntVector2 pixelDelta)
	{
		if (pixelDelta == IntVector2.Zero)
		{
			return;
		}
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		IntVector2 intVector = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.LowerLeft - pixelDelta).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.LowerLeft).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector3 = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.UpperRight - pixelDelta).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector4 = PhysicsEngine.PixelToUnitMidpoint(primaryPixelCollider.UpperRight).ToIntVector2(VectorConversions.Floor);
		Dungeon dungeon = GameManager.Instance.Dungeon;
		if (intVector != intVector2 || intVector3 != intVector4)
		{
			for (int i = intVector.x; i <= intVector3.x; i++)
			{
				for (int j = intVector.y; j <= intVector3.y; j++)
				{
					if (dungeon.CellExists(i, j) && dungeon.data[i, j] != null)
					{
						List<SpeculativeRigidbody> platforms = dungeon.data[i, j].platforms;
						if (platforms != null)
						{
							platforms.Remove(specRigidbody);
						}
					}
				}
			}
			for (int k = intVector2.x; k <= intVector4.x; k++)
			{
				for (int l = intVector2.y; l <= intVector4.y; l++)
				{
					if (dungeon.CellExists(k, l) && dungeon.data[k, l] != null)
					{
						List<SpeculativeRigidbody> list = dungeon.data[k, l].platforms;
						if (list == null)
						{
							list = new List<SpeculativeRigidbody>();
							GameManager.Instance.Dungeon.data.cellData[k][l].platforms = list;
						}
						if (!list.Contains(specRigidbody))
						{
							list.Add(specRigidbody);
						}
					}
				}
			}
		}
		if (m_sprite != null)
		{
			m_sprite.UpdateZDepth();
		}
	}

	public IntVector2 GetOverrideDwarfDimensions(PrototypePlacedObjectData objectData)
	{
		if (objectData.GetBoolFieldValueByName("UsesDwarfConfigurableSize"))
		{
			return new IntVector2((int)objectData.GetFieldValueByName("DwarfConfigurableWidth"), (int)objectData.GetFieldValueByName("DwarfConfigurableHeight"));
		}
		return new IntVector2(placeableWidth, placeableHeight);
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		if (room.RoomMovingPlatforms != null && (bool)base.specRigidbody)
		{
			room.RoomMovingPlatforms.Add(base.specRigidbody);
		}
	}
}
