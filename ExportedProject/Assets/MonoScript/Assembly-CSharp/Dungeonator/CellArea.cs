using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	public class CellArea
	{
		public IntVector2 basePosition;

		public IntVector2 dimensions;

		public Vector2 weightedOverlapMovementVector;

		public int variableBorderSizeX;

		public int variableBorderSizeY;

		public bool IsProceduralRoom = true;

		public string PrototypeRoomName;

		public PrototypeDungeonRoom.RoomCategory PrototypeRoomCategory = PrototypeDungeonRoom.RoomCategory.NORMAL;

		public PrototypeDungeonRoom.RoomNormalSubCategory PrototypeRoomNormalSubcategory;

		public PrototypeDungeonRoom.RoomSpecialSubCategory PrototypeRoomSpecialSubcategory;

		public PrototypeDungeonRoom.RoomBossSubCategory PrototypeRoomBossSubcategory;

		public bool PrototypeLostWoodsRoom;

		public RuntimePrototypeRoomData runtimePrototypeData;

		private PrototypeDungeonRoom m_prototypeRoom;

		public List<PrototypeRoomExit> instanceUsedExits;

		public Dictionary<PrototypeRoomExit, RuntimeRoomExitData> exitToLocalDataMap;

		public List<IntVector2> proceduralCells;

		private int borderDistance;

		public PrototypeDungeonRoom prototypeRoom
		{
			get
			{
				return m_prototypeRoom;
			}
			set
			{
				IsProceduralRoom = value == null && IsProceduralRoom;
				PrototypeRoomName = ((!(value == null)) ? value.name : PrototypeRoomName);
				PrototypeRoomCategory = ((!(value == null)) ? value.category : PrototypeRoomCategory);
				PrototypeRoomNormalSubcategory = ((!(value == null)) ? value.subCategoryNormal : PrototypeRoomNormalSubcategory);
				PrototypeRoomSpecialSubcategory = ((!(value == null)) ? value.subCategorySpecial : PrototypeRoomSpecialSubcategory);
				PrototypeRoomBossSubcategory = ((!(value == null)) ? value.subCategoryBoss : PrototypeRoomBossSubcategory);
				PrototypeLostWoodsRoom = ((!(value == null)) ? value.IsLostWoodsRoom : PrototypeLostWoodsRoom);
				m_prototypeRoom = value;
			}
		}

		public Vector2 UnitBottomLeft
		{
			get
			{
				return basePosition.ToVector2();
			}
		}

		public Vector2 UnitCenter
		{
			get
			{
				return basePosition.ToVector2() + dimensions.ToVector2() * 0.5f;
			}
		}

		public Vector2 UnitTopRight
		{
			get
			{
				return (basePosition + dimensions).ToVector2();
			}
		}

		public float UnitLeft
		{
			get
			{
				return basePosition.x;
			}
		}

		public float UnitRight
		{
			get
			{
				return basePosition.x + dimensions.x;
			}
		}

		public float UnitBottom
		{
			get
			{
				return basePosition.y;
			}
		}

		public float UnitTop
		{
			get
			{
				return basePosition.y + dimensions.y;
			}
		}

		public Vector2 Center
		{
			get
			{
				return new Vector2((float)basePosition.x + (float)dimensions.x / 2f, (float)basePosition.y + (float)dimensions.y / 2f);
			}
		}

		public IntVector2 IntCenter
		{
			get
			{
				return new IntVector2((int)Center.x, (int)Center.y);
			}
		}

		public CellArea(IntVector2 p, IntVector2 d, int borderOffset = 0)
		{
			basePosition = p;
			dimensions = d;
			borderDistance = borderOffset;
			instanceUsedExits = new List<PrototypeRoomExit>();
			exitToLocalDataMap = new Dictionary<PrototypeRoomExit, RuntimeRoomExitData>();
		}

		public bool Overlaps(CellArea other)
		{
			IntVector2 intVector = basePosition + dimensions;
			IntVector2 intVector2 = other.basePosition + other.dimensions;
			if (basePosition.x < intVector2.x && intVector.x > other.basePosition.x && basePosition.y < intVector2.y && intVector.y > other.basePosition.y)
			{
				return true;
			}
			return false;
		}

		public bool OverlapsWithUnitBorder(CellArea other)
		{
			IntVector2 intVector = basePosition + IntVector2.NegOne;
			IntVector2 intVector2 = basePosition + dimensions + IntVector2.One * 2;
			IntVector2 intVector3 = other.basePosition + IntVector2.NegOne;
			IntVector2 intVector4 = other.basePosition + other.dimensions + IntVector2.One * 2;
			if (intVector.x < intVector4.x && intVector2.x > intVector3.x && intVector.y < intVector4.y && intVector2.y > intVector3.y)
			{
				return true;
			}
			return false;
		}

		public bool ContainsWithUnitBorder(IntVector2 point)
		{
			if (point.x >= basePosition.x - 1 && point.x <= basePosition.x + 1 && point.y >= basePosition.y - 1 && point.y <= basePosition.y + 1)
			{
				return true;
			}
			return false;
		}

		public bool Contains(IntVector2 point)
		{
			if (point.x >= basePosition.x + borderDistance && point.x <= basePosition.x + dimensions.x - borderDistance && point.y >= basePosition.y + borderDistance && point.y <= basePosition.y + dimensions.y - borderDistance)
			{
				return true;
			}
			return false;
		}

		public bool CellOnBorder(IntVector2 pos)
		{
			bool result = false;
			if ((pos.x < basePosition.x + 1 && Contains(pos)) || (pos.x >= basePosition.x + dimensions.x - 1 && Contains(pos)) || (pos.y < basePosition.y + 1 && Contains(pos)) || (pos.y >= basePosition.y + dimensions.y - 1 && Contains(pos)))
			{
				result = true;
			}
			return result;
		}

		public int CheckSharedEdge(CellArea other, int lengthOfSharedEdge, out IntVector2 position, out DungeonData.Direction dir)
		{
			int num = Math.Max(basePosition.x, other.basePosition.x);
			int num2 = Math.Min(basePosition.x + dimensions.x, other.basePosition.x + other.dimensions.x);
			int num3 = num2 - num;
			if (num3 >= lengthOfSharedEdge)
			{
				if (other.basePosition.y > basePosition.y)
				{
					dir = DungeonData.Direction.NORTH;
					position = new IntVector2(num, basePosition.y + dimensions.y);
				}
				else
				{
					dir = DungeonData.Direction.SOUTH;
					position = new IntVector2(num, basePosition.y);
				}
				return num3;
			}
			num = Math.Max(basePosition.y, other.basePosition.y);
			num2 = Math.Min(basePosition.y + dimensions.y, other.basePosition.y + other.dimensions.y);
			int num4 = num2 - num;
			if (num4 >= lengthOfSharedEdge)
			{
				if (other.basePosition.x > basePosition.x)
				{
					dir = DungeonData.Direction.EAST;
					position = new IntVector2(basePosition.x + dimensions.x, num);
				}
				else
				{
					dir = DungeonData.Direction.WEST;
					position = new IntVector2(basePosition.x, num);
				}
				return num4;
			}
			dir = DungeonData.Direction.NORTHWEST;
			position = IntVector2.Zero;
			return -1;
		}

		public bool LineIntersect(IntVector2 p1, IntVector2 p2)
		{
			return LineIntersectsLine(p1, p2, basePosition, basePosition + new IntVector2(0, dimensions.y)) || LineIntersectsLine(p1, p2, basePosition, basePosition + new IntVector2(dimensions.x, 0)) || LineIntersectsLine(p1, p2, basePosition + new IntVector2(0, dimensions.y), basePosition + dimensions) || LineIntersectsLine(p1, p2, basePosition + new IntVector2(dimensions.x, 0), basePosition + dimensions) || (Contains(p1) && Contains(p2));
		}

		private bool LineIntersectsLine(IntVector2 l1p1, IntVector2 l1p2, IntVector2 l2p1, IntVector2 l2p2)
		{
			float num = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			float num2 = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);
			if (num2 == 0f)
			{
				return false;
			}
			float num3 = num / num2;
			num = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
			float num4 = num / num2;
			if (num3 < 0f || num3 > 1f || num4 < 0f || num4 > 1f)
			{
				return false;
			}
			return true;
		}
	}
}
