using Dungeonator;
using UnityEngine;

public class SimpleRoomActivation : BraveBehaviour, IPlaceConfigurable
{
	public GameObject[] objectsToActivate;

	private bool m_active;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		room.BecameVisible += Activate;
	}

	protected void Activate(float delay)
	{
		if (!m_active)
		{
			m_active = true;
			for (int i = 0; i < objectsToActivate.Length; i++)
			{
				objectsToActivate[i].SetActive(true);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
