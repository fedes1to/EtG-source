using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	public class RoomEventTriggerArea
	{
		public HashSet<IntVector2> triggerCells;

		public IntVector2 initialPosition;

		public List<IEventTriggerable> events;

		[NonSerialized]
		public GameObject tempDataObject;

		public RoomEventTriggerArea()
		{
			triggerCells = new HashSet<IntVector2>();
			events = new List<IEventTriggerable>();
		}

		public RoomEventTriggerArea(PrototypeEventTriggerArea prototype, IntVector2 basePosition)
		{
			triggerCells = new HashSet<IntVector2>();
			events = new List<IEventTriggerable>();
			for (int i = 0; i < prototype.triggerCells.Count; i++)
			{
				IntVector2 intVector = prototype.triggerCells[i].ToIntVector2() + basePosition;
				CellData cellData = GameManager.Instance.Dungeon.data[intVector];
				cellData.cellVisualData.containsObjectSpaceStamp = true;
				triggerCells.Add(intVector);
				if (i == 0)
				{
					initialPosition = intVector;
				}
			}
		}

		public void Trigger(int eventIndex)
		{
			for (int i = 0; i < events.Count; i++)
			{
				events[i].Trigger(eventIndex);
			}
		}

		public void AddGameObject(GameObject g)
		{
			IEventTriggerable eventTriggerable = g.GetComponentInChildren(typeof(IEventTriggerable)) as IEventTriggerable;
			if (eventTriggerable == null)
			{
				return;
			}
			events.Add(eventTriggerable);
			if (!(eventTriggerable is HangingObjectController))
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					IntVector2 key = initialPosition + new IntVector2(i, j);
					GameManager.Instance.Dungeon.data[key].cellVisualData.containsWallSpaceStamp = true;
					GameManager.Instance.Dungeon.data[key].cellVisualData.containsObjectSpaceStamp = true;
				}
			}
		}
	}
}
