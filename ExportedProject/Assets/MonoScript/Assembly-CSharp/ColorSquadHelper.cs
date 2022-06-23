using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ColorSquadHelper : MonoBehaviour, IPlaceConfigurable
{
	private RoomHandler m_room;

	private IEnumerator Start()
	{
		yield return null;
		List<FlippableCover> covers = m_room.GetComponentsInRoom<FlippableCover>();
		for (int i = 0; i < covers.Count; i++)
		{
			covers[i].Flip(DungeonData.GetDirectionFromVector2(BraveUtility.GetMajorAxis(covers[i].transform.position.XY() - base.transform.position.XY())));
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
	}
}
