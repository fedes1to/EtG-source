using System.Collections.Generic;
using Dungeonator;
using Pathfinding;

public class PlacedBlockerConfigurable : BraveBehaviour, IPlaceConfigurable
{
	public enum ColliderSelection
	{
		Single,
		All
	}

	public ColliderSelection colliderSelection;

	[ShowInInspectorIf("colliderSelection", 0, false)]
	public bool SpecifyPixelCollider;

	[ShowInInspectorIf("SpecifyPixelCollider", false)]
	public int SpecifiedPixelCollider;

	private bool m_initialized;

	private List<OccupiedCells> m_allOccupiedCells;

	public bool ShowSpecifiedPixelCollider()
	{
		return colliderSelection == ColliderSelection.Single && SpecifyPixelCollider;
	}

	public void Start()
	{
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		Initialize(room);
	}

	protected override void OnDestroy()
	{
		if (GameManager.HasInstance && Pathfinder.HasInstance && (bool)base.specRigidbody && m_allOccupiedCells != null)
		{
			for (int i = 0; i < m_allOccupiedCells.Count; i++)
			{
				OccupiedCells occupiedCells = m_allOccupiedCells[i];
				if (occupiedCells != null)
				{
					occupiedCells.Clear();
				}
			}
		}
		base.OnDestroy();
	}

	private void Initialize(RoomHandler room)
	{
		if (m_initialized)
		{
			return;
		}
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Initialize();
			if (colliderSelection == ColliderSelection.All)
			{
				m_allOccupiedCells = new List<OccupiedCells>(base.specRigidbody.PixelColliders.Count);
				for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
				{
					m_allOccupiedCells.Add(new OccupiedCells(base.specRigidbody, base.specRigidbody.PixelColliders[i], room));
				}
			}
			else
			{
				m_allOccupiedCells = new List<OccupiedCells>(1);
				if (SpecifyPixelCollider)
				{
					m_allOccupiedCells.Add(new OccupiedCells(base.specRigidbody, base.specRigidbody.PixelColliders[SpecifiedPixelCollider], room));
				}
				else
				{
					m_allOccupiedCells.Add(new OccupiedCells(base.specRigidbody, room));
				}
			}
		}
		m_initialized = true;
	}
}
