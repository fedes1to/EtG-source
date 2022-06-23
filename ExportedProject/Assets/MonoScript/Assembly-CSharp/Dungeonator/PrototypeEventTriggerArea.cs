using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class PrototypeEventTriggerArea
	{
		[SerializeField]
		public List<Vector2> triggerCells;

		public PrototypeEventTriggerArea()
		{
			triggerCells = new List<Vector2>();
		}

		public PrototypeEventTriggerArea(IEnumerable<Vector2> cells)
		{
			triggerCells = new List<Vector2>(cells);
		}

		public PrototypeEventTriggerArea(IEnumerable<IntVector2> cells)
		{
			triggerCells = new List<Vector2>();
			foreach (IntVector2 cell in cells)
			{
				triggerCells.Add(cell.ToVector2());
			}
		}

		public PrototypeEventTriggerArea CreateMirror(IntVector2 roomDimensions)
		{
			PrototypeEventTriggerArea prototypeEventTriggerArea = new PrototypeEventTriggerArea();
			for (int i = 0; i < triggerCells.Count; i++)
			{
				Vector2 item = triggerCells[i];
				item.x = (float)roomDimensions.x - (item.x + 1f);
				prototypeEventTriggerArea.triggerCells.Add(item);
			}
			return prototypeEventTriggerArea;
		}
	}
}
