using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace Pathfinding
{
	public class Path
	{
		public LinkedList<IntVector2> Positions;

		public LinkedList<IntVector2> PreSmoothedPositions = new LinkedList<IntVector2>();

		public IntVector2 Clearance = IntVector2.One;

		public int Count
		{
			get
			{
				return (Positions != null) ? Positions.Count : 0;
			}
		}

		public IntVector2 First
		{
			get
			{
				return Positions.First.Value;
			}
		}

		public bool WillReachFinalGoal { get; set; }

		public float InaccurateLength
		{
			get
			{
				if (Positions.Count == 0)
				{
					return 0f;
				}
				float num = 0f;
				LinkedListNode<IntVector2> linkedListNode = Positions.First;
				LinkedListNode<IntVector2> next = linkedListNode.Next;
				while (linkedListNode != null && next != null)
				{
					num += (float)IntVector2.ManhattanDistance(linkedListNode.Value, next.Value);
					linkedListNode = next;
					next = next.Next;
				}
				return num;
			}
		}

		public Path()
		{
			Positions = new LinkedList<IntVector2>();
			WillReachFinalGoal = true;
		}

		public Path(LinkedList<IntVector2> positions, IntVector2 clearance)
		{
			Positions = positions;
			Clearance = clearance;
			WillReachFinalGoal = true;
		}

		public Vector2 GetFirstCenterVector2()
		{
			return Pathfinder.GetClearanceOffset(Positions.First.Value, Clearance);
		}

		public Vector2 GetSecondCenterVector2()
		{
			return Pathfinder.GetClearanceOffset(Positions.First.Next.Value, Clearance);
		}

		public void RemoveFirst()
		{
			Positions.RemoveFirst();
		}

		public void Smooth(Vector2 startPos, Vector2 extents, CellTypes passableCellTypes, bool canPassOccupied, IntVector2 clearance)
		{
			Pathfinder.Instance.Smooth(this, startPos, extents, passableCellTypes, canPassOccupied, clearance);
		}
	}
}
