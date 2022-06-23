using System.Collections.Generic;
using UnityEngine;

public class TrashLauncher : BraveBehaviour
{
	public enum TrashManipulateMode
	{
		GATHER_AND_TOSS,
		DRAGONBALL_Z
	}

	public TrashManipulateMode mode;

	private HashSet<DebrisObject> m_debris = new HashSet<DebrisObject>();

	private PlayerController m_player;

	public float liftIntensity = 2f;

	private void Start()
	{
		m_player = GetComponentInParent<PlayerController>();
	}

	private void Update()
	{
		Vector2 worldCenter = base.sprite.WorldCenter;
		for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
		{
			DebrisObject debrisObject = StaticReferenceManager.AllDebris[i];
			if (!debrisObject || debrisObject.IsPickupObject || debrisObject.Priority == EphemeralObject.EphemeralPriority.Critical)
			{
				continue;
			}
			Vector2 vector = ((!debrisObject.sprite) ? debrisObject.transform.position.XY() : debrisObject.sprite.WorldCenter);
			Vector2 vector2 = worldCenter - vector;
			float sqrMagnitude = vector2.sqrMagnitude;
			switch (mode)
			{
			case TrashManipulateMode.GATHER_AND_TOSS:
				if (sqrMagnitude < 100f)
				{
					if (!m_debris.Contains(debrisObject))
					{
						m_debris.Add(debrisObject);
					}
					if (debrisObject.HasBeenTriggered)
					{
						debrisObject.ApplyVelocity(vector2.normalized * 25f * debrisObject.inertialMass * BraveTime.DeltaTime);
						debrisObject.PreventFallingInPits = true;
					}
				}
				break;
			case TrashManipulateMode.DRAGONBALL_Z:
				if (sqrMagnitude < 100f)
				{
					if (!m_debris.Contains(debrisObject))
					{
						m_debris.Add(debrisObject);
					}
					if (debrisObject.HasBeenTriggered && debrisObject.UnadjustedDebrisPosition.z < 0.75f)
					{
						debrisObject.IncrementZHeight(liftIntensity * BraveTime.DeltaTime);
					}
				}
				break;
			}
		}
	}

	public void OnDespawned()
	{
		if (mode == TrashManipulateMode.GATHER_AND_TOSS)
		{
			Vector2 vector = Random.insideUnitCircle;
			if ((bool)m_player)
			{
				vector = m_player.unadjustedAimPoint.XY() - m_player.CenterPosition;
			}
			vector = vector.normalized;
			foreach (DebrisObject debri in m_debris)
			{
				if ((bool)debri)
				{
					Vector2 vector2 = Quaternion.Euler(0f, 0f, Random.Range(-15, 15)) * vector;
					debri.ApplyVelocity(vector2 * Random.Range(45, 55));
				}
			}
		}
		base.OnDestroy();
	}
}
