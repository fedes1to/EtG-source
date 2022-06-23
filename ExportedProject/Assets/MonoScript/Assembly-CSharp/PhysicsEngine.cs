using System;
using System.Collections.Generic;
using System.Diagnostics;
using BraveDynamicTree;
using Dungeonator;
using UnityEngine;
using UnityEngine.Profiling;

public class PhysicsEngine : MonoBehaviour
{
	public enum DebugDrawType
	{
		None,
		Boundaries,
		FullPixels
	}

	private class Raycaster
	{
		private PhysicsEngine physicsEngine;

		private DungeonData dungeonData;

		private Position origin;

		private Vector2 direction;

		private float dist;

		private bool collideWithTiles;

		private bool collideWithRigidbodies;

		private int rayMask;

		private CollisionLayer? sourceLayer;

		private bool collideWithTriggers;

		private Func<SpeculativeRigidbody, bool> rigidbodyExcluder;

		private ICollection<SpeculativeRigidbody> ignoreList;

		private RaycastResult nearestRigidbodyHit;

		private Vector2 p1;

		private float p1p2Dist;

		private Func<b2RayCastInput, SpeculativeRigidbody, float> queryPointer;

		public Raycaster()
		{
			queryPointer = RaycastAtRigidbodiesQuery;
		}

		public void SetAll(PhysicsEngine physicsEngine, DungeonData dungeonData, Position origin, Vector2 direction, float dist, bool collideWithTiles, bool collideWithRigidbodies, int rayMask, CollisionLayer? sourceLayer, bool collideWithTriggers, Func<SpeculativeRigidbody, bool> rigidbodyExcluder, ICollection<SpeculativeRigidbody> ignoreList)
		{
			this.physicsEngine = physicsEngine;
			this.dungeonData = dungeonData;
			this.origin = origin;
			this.direction = direction;
			this.dist = dist;
			this.collideWithTiles = collideWithTiles;
			this.collideWithRigidbodies = collideWithRigidbodies;
			this.rayMask = rayMask;
			this.sourceLayer = sourceLayer;
			this.collideWithTriggers = collideWithTriggers;
			this.rigidbodyExcluder = rigidbodyExcluder;
			this.ignoreList = ignoreList;
		}

		public void Clear()
		{
			physicsEngine = null;
			rigidbodyExcluder = null;
			ignoreList = null;
		}

		public bool DoRaycast(out RaycastResult result)
		{
			result = null;
			direction.Normalize();
			if (collideWithTiles && (bool)physicsEngine.TileMap)
			{
				string text = "Collision Layer";
				int tileMapLayerByName = BraveUtility.GetTileMapLayerByName(text, physicsEngine.TileMap);
				IntVector2 intVector = UnitToPixel(origin.UnitPosition);
				IntVector2 intVector2 = UnitToPixel(direction.normalized * dist);
				IntVector2 zero = IntVector2.Zero;
				IntVector2 intVector3 = intVector / physicsEngine.PixelsPerUnit;
				RaycastResult obj = RaycastAtTile(intVector3, tileMapLayerByName, text, rayMask, sourceLayer, origin, direction, dist, dungeonData);
				if (obj != null && (result == null || obj.Distance < result.Distance))
				{
					RaycastResult.Pool.Free(ref result);
					result = obj;
				}
				else
				{
					RaycastResult.Pool.Free(ref obj);
				}
				IntVector2 intVector4 = intVector3;
				while (zero.x != intVector2.x || zero.y != intVector2.y)
				{
					IntVector2 intVector5;
					if (zero.x == intVector2.x)
					{
						intVector5 = new IntVector2(0, Math.Sign(intVector2.y));
					}
					else if (zero.y == intVector2.y)
					{
						intVector5 = new IntVector2(Math.Sign(intVector2.x), 0);
					}
					else
					{
						float num = Mathf.Abs((float)zero.x / (float)intVector2.x);
						float num2 = Mathf.Abs((float)zero.y / (float)intVector2.y);
						intVector5 = ((!(num < num2)) ? new IntVector2(0, Math.Sign(intVector2.y)) : new IntVector2(Math.Sign(intVector2.x), 0));
					}
					zero += intVector5;
					IntVector2 intVector6 = (intVector + zero) / physicsEngine.PixelsPerUnit;
					if (!(intVector6 != intVector4))
					{
						continue;
					}
					RaycastResult obj2 = RaycastAtTile(intVector6, tileMapLayerByName, text, rayMask, sourceLayer, origin, direction, dist, dungeonData);
					if (obj2 != null && (result == null || obj2.Distance < result.Distance))
					{
						if (obj2.OtherPixelCollider.NormalModifier == null)
						{
							obj2.Normal = -intVector5.ToVector2().normalized;
						}
						RaycastResult.Pool.Free(ref result);
						result = obj2;
					}
					else
					{
						RaycastResult.Pool.Free(ref obj2);
					}
					intVector4 = intVector6;
				}
			}
			if (collideWithRigidbodies)
			{
				nearestRigidbodyHit = null;
				p1 = origin.UnitPosition;
				Vector2 vector = p1 + direction * dist;
				p1p2Dist = Vector2.Distance(p1, vector);
				physicsEngine.m_rigidbodyTree.RayCast(new b2RayCastInput(p1, vector), queryPointer);
				if (physicsEngine.CollidesWithProjectiles(rayMask, sourceLayer))
				{
					physicsEngine.m_projectileTree.RayCast(new b2RayCastInput(p1, vector), queryPointer);
				}
				if (nearestRigidbodyHit != null)
				{
					if (result == null || nearestRigidbodyHit.Distance < result.Distance)
					{
						RaycastResult.Pool.Free(ref result);
						result = nearestRigidbodyHit;
					}
					else
					{
						RaycastResult.Pool.Free(ref nearestRigidbodyHit);
					}
				}
			}
			return result != null;
		}

		private RaycastResult RaycastAtTile(IntVector2 pos, int layer, string layerName, int rayMask, CollisionLayer? sourceLayer, Position origin, Vector2 direction, float dist, DungeonData dungeonData)
		{
			Tile tile = physicsEngine.GetTile(pos.x, pos.y, physicsEngine.TileMap, layer, layerName, dungeonData);
			RaycastResult obj = null;
			if (tile == null || tile.PixelColliders == null || tile.PixelColliders.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < tile.PixelColliders.Count; i++)
			{
				PixelCollider pixelCollider = tile.PixelColliders[i];
				if (!pixelCollider.CanCollideWith(rayMask, sourceLayer))
				{
					continue;
				}
				RaycastResult result;
				if (pixelCollider.Raycast(origin.UnitPosition, direction, dist, out result))
				{
					if (obj == null || result.Distance < obj.Distance)
					{
						RaycastResult.Pool.Free(ref obj);
						obj = result;
					}
					else
					{
						RaycastResult.Pool.Free(ref result);
					}
				}
				else
				{
					RaycastResult.Pool.Free(ref result);
				}
			}
			return obj;
		}

		private float RaycastAtRigidbodiesQuery(b2RayCastInput rayCastInput, SpeculativeRigidbody rigidbody)
		{
			float result = rayCastInput.maxFraction;
			if ((bool)rigidbody && rigidbody.enabled && rigidbody.CollideWithOthers && (ignoreList == null || !ignoreList.Contains(rigidbody)))
			{
				if (rigidbodyExcluder != null && rigidbodyExcluder(rigidbody))
				{
					return result;
				}
				for (int i = 0; i < rigidbody.PixelColliders.Count; i++)
				{
					PixelCollider pixelCollider = rigidbody.PixelColliders[i];
					if ((!collideWithTriggers && pixelCollider.IsTrigger) || !pixelCollider.CanCollideWith(rayMask, sourceLayer))
					{
						continue;
					}
					RaycastResult result2;
					if (pixelCollider.Raycast(origin.UnitPosition, direction, dist, out result2))
					{
						if (nearestRigidbodyHit == null || result2.Distance < nearestRigidbodyHit.Distance)
						{
							RaycastResult.Pool.Free(ref nearestRigidbodyHit);
							nearestRigidbodyHit = result2;
							nearestRigidbodyHit.SpeculativeRigidbody = rigidbody;
							result = Vector2.Distance(p1, nearestRigidbodyHit.Contact) / p1p2Dist;
						}
						else
						{
							RaycastResult.Pool.Free(ref result2);
						}
					}
					else
					{
						RaycastResult.Pool.Free(ref result2);
					}
				}
			}
			return result;
		}
	}

	public enum PointCollisionState
	{
		Clean,
		HitBeforeNext,
		Hit
	}

	private class RigidbodyCaster
	{
		private PhysicsEngine physicsEngine;

		private DungeonData dungeonData;

		private SpeculativeRigidbody rigidbody;

		private IntVector2 pixelsToMove;

		private bool collideWithTiles;

		private bool collideWithRigidbodies;

		private int? overrideCollisionMask;

		private bool collideWithTriggers;

		private SpeculativeRigidbody[] ignoreList;

		private CollisionData tempResult;

		private List<PixelCollider.StepData> stepList;

		private Func<SpeculativeRigidbody, bool> callbackPointer;

		public RigidbodyCaster()
		{
			callbackPointer = RigidbodyCollisionCallback;
		}

		public void SetAll(PhysicsEngine physicsEngine, DungeonData dungeonData, SpeculativeRigidbody rigidbody, IntVector2 pixelsToMove, bool collideWithTiles, bool collideWithRigidbodies, int? overrideCollisionMask, bool collideWithTriggers, SpeculativeRigidbody[] ignoreList)
		{
			this.physicsEngine = physicsEngine;
			this.dungeonData = dungeonData;
			this.rigidbody = rigidbody;
			this.pixelsToMove = pixelsToMove;
			this.collideWithTiles = collideWithTiles;
			this.collideWithRigidbodies = collideWithRigidbodies;
			this.overrideCollisionMask = overrideCollisionMask;
			this.collideWithTriggers = collideWithTriggers;
			this.ignoreList = ignoreList;
		}

		public void Clear()
		{
			physicsEngine = null;
			rigidbody = null;
			ignoreList = null;
		}

		public bool DoRigidbodyCast(out CollisionData result)
		{
			tempResult = null;
			if (!rigidbody || rigidbody.PixelColliders.Count == 0)
			{
				result = null;
				return false;
			}
			stepList = PixelCollider.m_stepList;
			PixelMovementGenerator(pixelsToMove, stepList);
			IntVector2 intVector = IntVector2.MaxValue;
			IntVector2 intVector2 = IntVector2.MinValue;
			for (int i = 0; i < rigidbody.PixelColliders.Count; i++)
			{
				PixelCollider pixelCollider = rigidbody.PixelColliders[i];
				intVector = IntVector2.Min(intVector, pixelCollider.Min);
				intVector2 = IntVector2.Max(intVector2, pixelCollider.Max);
			}
			IntVector2 intVector3 = IntVector2.Min(intVector, intVector + pixelsToMove);
			IntVector2 intVector4 = IntVector2.Max(intVector2, intVector2 + pixelsToMove);
			if (collideWithTiles && (bool)physicsEngine.TileMap)
			{
				physicsEngine.InitNearbyTileCheck(PixelToUnit(intVector3 - IntVector2.One), PixelToUnit(intVector4 + IntVector2.One), physicsEngine.TileMap);
				for (Tile nextNearbyTile = physicsEngine.GetNextNearbyTile(dungeonData); nextNearbyTile != null; nextNearbyTile = physicsEngine.GetNextNearbyTile(dungeonData))
				{
					for (int j = 0; j < rigidbody.PixelColliders.Count; j++)
					{
						PixelCollider pixelCollider2 = rigidbody.PixelColliders[j];
						if (!collideWithTriggers && pixelCollider2.IsTrigger)
						{
							continue;
						}
						for (int k = 0; k < nextNearbyTile.PixelColliders.Count; k++)
						{
							PixelCollider otherCollider = nextNearbyTile.PixelColliders[k];
							LinearCastResult obj;
							if (!pixelCollider2.CanCollideWith(otherCollider) || !pixelCollider2.AABBOverlaps(otherCollider, pixelsToMove) || !pixelCollider2.LinearCast(otherCollider, rigidbody.PixelsToMove, stepList, out obj))
							{
								continue;
							}
							if (tempResult == null || obj.TimeUsed < tempResult.TimeUsed)
							{
								if (tempResult == null)
								{
									tempResult = CollisionData.Pool.Allocate();
								}
								tempResult.SetAll(obj);
								tempResult.collisionType = CollisionData.CollisionType.TileMap;
								tempResult.MyRigidbody = rigidbody;
								tempResult.TileLayerName = "Collision Layer";
								tempResult.TilePosition = nextNearbyTile.Position;
							}
							LinearCastResult.Pool.Free(ref obj);
						}
					}
				}
			}
			if (collideWithRigidbodies)
			{
				physicsEngine.m_rigidbodyTree.Query(GetSafeB2AABB(intVector3, intVector4), callbackPointer);
				if (overrideCollisionMask.HasValue)
				{
					if ((overrideCollisionMask.Value & physicsEngine.m_cachedProjectileMask) == physicsEngine.m_cachedProjectileMask)
					{
						physicsEngine.m_projectileTree.Query(GetSafeB2AABB(intVector3, intVector4), callbackPointer);
					}
				}
				else if (physicsEngine.CollidesWithProjectiles(rigidbody))
				{
					physicsEngine.m_projectileTree.Query(GetSafeB2AABB(intVector3, intVector4), callbackPointer);
				}
			}
			result = tempResult;
			return result != null;
		}

		private bool RigidbodyCollisionCallback(SpeculativeRigidbody otherRigidbody)
		{
			if ((bool)otherRigidbody && otherRigidbody != rigidbody && otherRigidbody.enabled && otherRigidbody.CollideWithOthers && Array.IndexOf(ignoreList, otherRigidbody) < 0)
			{
				for (int i = 0; i < rigidbody.PixelColliders.Count; i++)
				{
					PixelCollider pixelCollider = rigidbody.PixelColliders[i];
					if (!collideWithTriggers && pixelCollider.IsTrigger)
					{
						continue;
					}
					for (int j = 0; j < otherRigidbody.PixelColliders.Count; j++)
					{
						PixelCollider pixelCollider2 = otherRigidbody.PixelColliders[j];
						if (!collideWithTriggers && pixelCollider2.IsTrigger)
						{
							continue;
						}
						bool flag = ((!overrideCollisionMask.HasValue) ? pixelCollider.CanCollideWith(pixelCollider2) : pixelCollider2.CanCollideWith(overrideCollisionMask.Value));
						LinearCastResult obj;
						if (!flag || !pixelCollider.AABBOverlaps(pixelCollider2, pixelsToMove) || !pixelCollider.LinearCast(pixelCollider2, rigidbody.PixelsToMove, stepList, out obj))
						{
							continue;
						}
						if (tempResult == null || obj.TimeUsed < tempResult.TimeUsed)
						{
							if (tempResult == null)
							{
								tempResult = CollisionData.Pool.Allocate();
							}
							tempResult.SetAll(obj);
							tempResult.collisionType = CollisionData.CollisionType.Rigidbody;
							tempResult.MyRigidbody = rigidbody;
							tempResult.OtherRigidbody = otherRigidbody;
						}
						LinearCastResult.Pool.Free(ref obj);
					}
				}
			}
			return true;
		}
	}

	private struct NearbyTileData
	{
		private enum Type
		{
			FullRect,
			FullRectPrecalc,
			BresenhamVertical,
			BresenhamHorizontal,
			BresenhamShallow,
			BresenhamSteep
		}

		public tk2dTileMap tileMap;

		public int layer;

		public string layerName;

		private Type type;

		private int minPlotX;

		private int minPlotY;

		private int maxPlotX;

		private int maxPlotY;

		private int baseX;

		private int baseY;

		private int width;

		private int i;

		private int imax;

		private bool finished;

		private int x;

		private int y;

		private int extentsX;

		private int extentsY;

		private int endX;

		private int endY;

		private int deltaX;

		private int deltaY;

		private int xStep;

		private int yStep;

		private float deltaError;

		private float error;

		private static List<Tile> m_tiles = new List<Tile>();

		public void Init(float worldMinX, float worldMinY, float worldMaxX, float worldMaxY)
		{
			type = Type.FullRect;
			baseX = (int)worldMinX;
			baseY = (int)worldMinY;
			width = (int)worldMaxX - baseX + 1;
			int num = (int)worldMaxY - baseY + 1;
			i = 0;
			imax = width * num;
		}

		public void Init(float worldMinX, float worldMinY, float worldMaxX, float worldMaxY, IntVector2 pixelColliderDimensions, float positionX, float positionY, IntVector2 pixelsToMove, DungeonData dungeonData)
		{
			finished = false;
			minPlotX = (int)worldMinX;
			minPlotY = (int)worldMinY;
			maxPlotX = (int)worldMaxX;
			maxPlotY = (int)worldMaxY;
			int num = ((int)worldMaxX - (int)worldMinX + 1) * ((int)worldMaxY - (int)worldMinY + 1);
			if (num <= 6)
			{
				type = Type.FullRectPrecalc;
				GetAllNearbyTiles(worldMinX, worldMinY, worldMaxX, worldMaxY, dungeonData);
				return;
			}
			float num2 = (float)pixelColliderDimensions.x * 0.0625f * 0.5f;
			float num3 = (float)pixelColliderDimensions.y * 0.0625f * 0.5f;
			float num4 = positionX + num2;
			float num5 = positionY + num3;
			x = (int)num4;
			y = (int)num5;
			endX = (int)(num4 + (float)pixelsToMove.x * 0.0625f);
			endY = (int)(num5 + (float)pixelsToMove.y * 0.0625f);
			deltaX = endX - x;
			deltaY = endY - y;
			extentsX = Mathf.CeilToInt(num2 + 0.25f);
			extentsY = Mathf.CeilToInt(num3 + 0.25f);
			if (deltaX == 0)
			{
				type = Type.BresenhamVertical;
				yStep = (int)Mathf.Sign(deltaY);
				for (int i = -extentsY; i < 0; i++)
				{
					for (int j = -extentsX; j <= extentsX; j++)
					{
						Plot(x + j, y + yStep * i, Color.blue, dungeonData);
					}
				}
				return;
			}
			if (deltaY == 0)
			{
				type = Type.BresenhamHorizontal;
				xStep = (int)Mathf.Sign(deltaX);
				for (int k = -extentsX; k < 0; k++)
				{
					for (int l = -extentsY; l <= extentsY; l++)
					{
						Plot(x + xStep * k, y + l, Color.blue, dungeonData);
					}
				}
				return;
			}
			if (Mathf.Abs(deltaX) >= Mathf.Abs(deltaY))
			{
				type = Type.BresenhamShallow;
				xStep = (int)Mathf.Sign(deltaX);
				yStep = (int)Mathf.Sign(deltaY);
				for (int m = -extentsX; m < 0; m++)
				{
					for (int n = -extentsY; n <= extentsY; n++)
					{
						Plot(x + xStep * m, y + n, Color.blue, dungeonData);
					}
				}
				deltaError = Mathf.Abs((float)deltaY / (float)deltaX);
				error = 0f;
				return;
			}
			type = Type.BresenhamSteep;
			xStep = (int)Mathf.Sign(deltaX);
			yStep = (int)Mathf.Sign(deltaY);
			for (int num6 = -extentsY; num6 < 0; num6++)
			{
				for (int num7 = -extentsX; num7 <= extentsX; num7++)
				{
					Plot(x + num7, y + yStep * num6, Color.blue, dungeonData);
				}
			}
			deltaError = Mathf.Abs((float)deltaX / (float)deltaY);
			error = 0f;
		}

		private void Plot(int x, int y, Color color, DungeonData dungeonData, bool core = false)
		{
			CellData cellData;
			if (x < 0 || x < minPlotX || x >= m_instance.m_cachedDungeonWidth || y < 0 || y >= m_instance.m_cachedDungeonHeight || x < minPlotX || x > maxPlotX || y < minPlotY || y > maxPlotY || (cellData = dungeonData.cellData[x][y]) == null || (cellData.HasCachedPhysicsTile && cellData.CachedPhysicsTile == null))
			{
				return;
			}
			if (cellData.HasCachedPhysicsTile)
			{
				m_tiles.Add(cellData.CachedPhysicsTile);
				return;
			}
			Tile tile = Instance.GetTile(x, y, tileMap, layer, layerName, dungeonData);
			if (tile != null)
			{
				m_tiles.Add(tile);
			}
		}

		public void Finish(DungeonData dungeonData, bool preMove = false)
		{
			if (finished)
			{
				return;
			}
			int num = x;
			int num2 = y;
			switch (type)
			{
			case Type.BresenhamVertical:
			{
				if (!preMove)
				{
					num2 -= yStep;
				}
				for (int num3 = 1; num3 <= extentsY; num3++)
				{
					for (int num4 = -extentsX; num4 <= extentsX; num4++)
					{
						Plot(num + num4, num2 + yStep * num3, Color.blue, dungeonData);
					}
				}
				break;
			}
			case Type.BresenhamHorizontal:
			{
				if (!preMove)
				{
					num -= xStep;
				}
				for (int k = 1; k <= extentsX; k++)
				{
					for (int l = -extentsY; l <= extentsY; l++)
					{
						Plot(num + xStep * k, num2 + l, Color.blue, dungeonData);
					}
				}
				break;
			}
			case Type.BresenhamShallow:
			{
				if (!preMove)
				{
					num -= xStep;
				}
				for (int m = 1; m <= extentsX; m++)
				{
					for (int n = -extentsY; n <= extentsY; n++)
					{
						Plot(num + xStep * m, num2 + n, Color.blue, dungeonData);
					}
				}
				break;
			}
			case Type.BresenhamSteep:
			{
				if (!preMove)
				{
					num2 -= yStep;
				}
				for (int i = 1; i <= extentsY; i++)
				{
					for (int j = -extentsX; j <= extentsX; j++)
					{
						Plot(num + j, num2 + yStep * i, Color.blue, dungeonData);
					}
				}
				break;
			}
			}
			finished = true;
		}

		public Tile GetNextNearbyTile(DungeonData dungeonData)
		{
			switch (type)
			{
			case Type.FullRect:
				while (this.i < imax)
				{
					Tile tile = Instance.GetTile(baseX + this.i % width, baseY + this.i / width, tileMap, layer, layerName, dungeonData);
					this.i++;
					if (tile != null)
					{
						return tile;
					}
				}
				return null;
			case Type.FullRectPrecalc:
				if (m_tiles.Count > 0)
				{
					int index2 = m_tiles.Count - 1;
					Tile result2 = m_tiles[index2];
					m_tiles.RemoveAt(index2);
					return result2;
				}
				return null;
			case Type.BresenhamVertical:
				while (!finished && m_tiles.Count == 0)
				{
					for (int n = -extentsX; n <= extentsX; n++)
					{
						Plot(x + n, y, Color.yellow, dungeonData, n == 0);
					}
					if (y == endY)
					{
						Finish(dungeonData, true);
					}
					y += yStep;
				}
				if (m_tiles.Count > 0)
				{
					int index5 = m_tiles.Count - 1;
					Tile result5 = m_tiles[index5];
					m_tiles.RemoveAt(index5);
					return result5;
				}
				return null;
			case Type.BresenhamHorizontal:
				while (!finished && m_tiles.Count == 0)
				{
					for (int k = -extentsY; k <= extentsY; k++)
					{
						Plot(x, y + k, Color.yellow, dungeonData, k == 0);
					}
					if (x == endX)
					{
						Finish(dungeonData, true);
					}
					x += xStep;
				}
				if (m_tiles.Count > 0)
				{
					int index3 = m_tiles.Count - 1;
					Tile result3 = m_tiles[index3];
					m_tiles.RemoveAt(index3);
					return result3;
				}
				return null;
			case Type.BresenhamShallow:
				while (!finished && m_tiles.Count == 0)
				{
					if (error >= 0.5f)
					{
						for (int l = 1; l <= extentsX; l++)
						{
							Plot(x - xStep + xStep * l, y - yStep * extentsY, Color.blue, dungeonData);
						}
						y += yStep;
						error -= 1f;
						for (int num2 = -1; num2 >= -extentsX; num2--)
						{
							Plot(x + xStep * num2, y + yStep * extentsY, Color.blue, dungeonData);
						}
					}
					for (int m = -extentsY; m <= extentsY; m++)
					{
						Plot(x, y + m, Color.green, dungeonData, m == 0);
					}
					error += deltaError;
					if (x == endX)
					{
						Finish(dungeonData, true);
					}
					x += xStep;
				}
				if (m_tiles.Count > 0)
				{
					int index4 = m_tiles.Count - 1;
					Tile result4 = m_tiles[index4];
					m_tiles.RemoveAt(index4);
					return result4;
				}
				return null;
			case Type.BresenhamSteep:
				while (!finished && m_tiles.Count == 0)
				{
					if (error >= 0.5f)
					{
						for (int i = 1; i <= extentsY; i++)
						{
							Plot(x - xStep * extentsX, y - yStep + yStep * i, Color.blue, dungeonData);
						}
						x += xStep;
						error -= 1f;
						for (int num = -1; num >= -extentsY; num--)
						{
							Plot(x + xStep * extentsX, y + yStep * num, Color.blue, dungeonData);
						}
					}
					for (int j = -extentsX; j <= extentsX; j++)
					{
						Plot(x + j, y, Color.green, dungeonData, j == 0);
					}
					error += deltaError;
					if (y == endY)
					{
						Finish(dungeonData, true);
					}
					y += yStep;
				}
				if (m_tiles.Count > 0)
				{
					int index = m_tiles.Count - 1;
					Tile result = m_tiles[index];
					m_tiles.RemoveAt(index);
					return result;
				}
				return null;
			default:
				return null;
			}
		}

		private void GetAllNearbyTiles(float worldMinX, float worldMinY, float worldMaxX, float worldMaxY, DungeonData dungeonData)
		{
			baseX = (int)worldMinX;
			baseY = (int)worldMinY;
			width = (int)worldMaxX - baseX + 1;
			imax = width * ((int)worldMaxY - baseY + 1);
			for (i = 0; i < imax; i++)
			{
				int num = baseX + i % width;
				int num2 = baseY + i / width;
				CellData cellData;
				if (num >= 0 && num < m_instance.m_cachedDungeonWidth && num2 >= 0 && num2 < m_instance.m_cachedDungeonHeight && (cellData = dungeonData.cellData[num][num2]) != null && (!cellData.HasCachedPhysicsTile || cellData.CachedPhysicsTile != null))
				{
					if (cellData.HasCachedPhysicsTile)
					{
						m_tiles.Add(cellData.CachedPhysicsTile);
					}
					else
					{
						Tile tile = Instance.GetTile(num, num2, tileMap, layer, layerName, dungeonData);
						if (tile != null)
						{
							m_tiles.Add(tile);
						}
					}
				}
			}
		}

		public void Cleanup()
		{
			m_tiles.Clear();
		}
	}

	public class Tile : ICollidableObject
	{
		public int X;

		public int Y;

		public string LayerName;

		public List<PixelCollider> PixelColliders = new List<PixelCollider>();

		public IntVector2 Position
		{
			get
			{
				return new IntVector2(X, Y);
			}
		}

		public PixelCollider PrimaryPixelCollider
		{
			get
			{
				if (PixelColliders == null || PixelColliders.Count == 0)
				{
					return null;
				}
				if (PixelColliders[0].CollisionLayer == CollisionLayer.EnemyBlocker)
				{
					return null;
				}
				return PixelColliders[0];
			}
		}

		public Tile()
		{
		}

		public Tile(List<PixelCollider> pixelColliders, int x, int y, string layerName)
		{
			PixelColliders = pixelColliders;
			X = x;
			Y = y;
			LayerName = layerName;
		}

		public bool CanCollideWith(SpeculativeRigidbody rigidbody)
		{
			return true;
		}

		public List<PixelCollider> GetPixelColliders()
		{
			return PixelColliders;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return LayerName.GetHashCode() & X.GetHashCode() & Y.GetHashCode();
		}
	}

	public static CustomSampler csSortRigidbodies;

	public static CustomSampler csPreRigidbodyMovement;

	public static CustomSampler csPreprocessing;

	public static CustomSampler csInitialRigidbodyUpdates;

	public static CustomSampler csRigidbodyCollisions;

	public static CustomSampler csInitCollisions;

	public static CustomSampler csBuildStepList;

	public static CustomSampler csMovementRestrictions;

	public static CustomSampler csCollideWithOthers;

	public static CustomSampler csCollideWithTilemap;

	public static CustomSampler csCollideWithTilemapInner;

	public static CustomSampler csCanCollideWith;

	public static CustomSampler csRigidbodyPushing;

	public static CustomSampler csResolveCollision;

	public static CustomSampler csUpdatePositionVelocity;

	public static CustomSampler csHandleCarriedObjects;

	public static CustomSampler csEndChecks;

	public static CustomSampler csEndCleanup;

	public static CustomSampler csUpdateZDepth;

	public static CustomSampler csPostRigidbodyMovement;

	public static CustomSampler csHandleTriggerCollisions;

	public static CustomSampler csClearGhostCollisions;

	public static CustomSampler csRaycastTiles;

	public static CustomSampler csRaycastRigidbodies;

	public static CustomSampler csTreeCollisions;

	public static CustomSampler csRigidbodyTreeSearch;

	public static CustomSampler csProjectileTreeSearch;

	public static CustomSampler csUpdatePosition;

	public static CustomSampler csGetNextNearbyTile;

	public static CustomSampler csCollideWithTilemapSetup;

	public static CustomSampler csGetTiles;

	public static CustomSampler csCollideWithTilemapSingle;

	public tk2dTileMap TileMap;

	public int PixelsPerUnit = 16;

	private const int c_warnIterations = 5;

	private const int c_maxIterations = 50;

	public DebugDrawType DebugDraw;

	[HideInInspector]
	public Color[] DebugColors = new Color[3]
	{
		Color.green,
		Color.magenta,
		Color.cyan
	};

	private List<SpeculativeRigidbody> m_rigidbodies = new List<SpeculativeRigidbody>();

	private b2DynamicTree m_rigidbodyTree = new b2DynamicTree();

	private b2DynamicTree m_projectileTree = new b2DynamicTree();

	private HashSet<IntVector2> m_debugTilesDrawnThisFrame = new HashSet<IntVector2>();

	private static List<SpeculativeRigidbody> c_boundedRigidbodies = new List<SpeculativeRigidbody>();

	private List<SpeculativeRigidbody> m_deregisterRigidBodies = new List<SpeculativeRigidbody>();

	private int m_frameCount;

	private int m_cachedProjectileMask;

	private static PhysicsEngine m_instance;

	public static LinearCastResult PendingCastResult;

	private SpeculativeRigidbody[] m_emptyIgnoreList = new SpeculativeRigidbody[0];

	private SpeculativeRigidbody[] m_singleIgnoreList = new SpeculativeRigidbody[1];

	private static Raycaster m_raycaster = new Raycaster();

	private SpeculativeRigidbody[] emptyIgnoreList = new SpeculativeRigidbody[0];

	private static RigidbodyCaster m_rigidbodyCaster = new RigidbodyCaster();

	private static SpeculativeRigidbody m_cwrqRigidbody;

	private static List<PixelCollider.StepData> m_cwrqStepList;

	private static CollisionData m_cwrqCollisionData;

	private int m_cachedDungeonWidth;

	private int m_cachedDungeonHeight;

	private NearbyTileData m_nbt;

	public List<SpeculativeRigidbody> AllRigidbodies
	{
		get
		{
			return m_rigidbodies;
		}
	}

	public static PhysicsEngine Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}

	public static bool SkipCollision { get; set; }

	public static bool? CollisionHaltsVelocity { get; set; }

	public static bool HaltRemainingMovement { get; set; }

	public static Vector2? PostSliceVelocity { get; set; }

	public float PixelUnitWidth
	{
		get
		{
			return 1f / (float)PixelsPerUnit;
		}
	}

	public float HalfPixelUnitWidth
	{
		get
		{
			return 0.5f / (float)PixelsPerUnit;
		}
	}

	public event Action OnPreRigidbodyMovement;

	public event Action OnPostRigidbodyMovement;

	private void Awake()
	{
		m_instance = this;
		if (TileMap == null)
		{
			TileMap = UnityEngine.Object.FindObjectOfType<tk2dTileMap>();
		}
		m_cachedProjectileMask = CollisionMask.LayerToMask(CollisionLayer.Projectile);
	}

	[Conditional("PROFILE_PHYSICS")]
	public static void ProfileBegin(CustomSampler sampler)
	{
	}

	[Conditional("PROFILE_PHYSICS")]
	public static void ProfileEnd(CustomSampler sampler)
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
			PendingCastResult = null;
			m_deregisterRigidBodies.Clear();
			c_boundedRigidbodies.Clear();
			m_cwrqRigidbody = null;
			m_cwrqStepList = null;
			m_cwrqCollisionData = null;
		}
	}

	private void LateUpdate()
	{
		if (Time.timeScale == 0f || BraveTime.DeltaTime == 0f)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		m_cachedDungeonWidth = data.Width;
		m_cachedDungeonHeight = data.Height;
		m_frameCount++;
		if (m_frameCount > 5)
		{
			SortRigidbodies();
			if (this.OnPreRigidbodyMovement != null)
			{
				this.OnPreRigidbodyMovement();
			}
			Dungeon dungeon = GameManager.Instance.Dungeon;
			for (int i = 0; i < m_rigidbodies.Count; i++)
			{
				SpeculativeRigidbody speculativeRigidbody = m_rigidbodies[i];
				if (!speculativeRigidbody.isActiveAndEnabled)
				{
					continue;
				}
				List<PixelCollider> pixelColliders = speculativeRigidbody.PixelColliders;
				int count = pixelColliders.Count;
				Transform transform = speculativeRigidbody.transform;
				if (speculativeRigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.Unknown)
				{
					InferRegistration(speculativeRigidbody);
				}
				if (speculativeRigidbody.RegenerateColliders)
				{
					speculativeRigidbody.ForceRegenerate();
				}
				if ((speculativeRigidbody.UpdateCollidersOnScale && transform.hasChanged) || speculativeRigidbody.UpdateCollidersOnRotation)
				{
					float num = 0f;
					if (speculativeRigidbody.UpdateCollidersOnRotation)
					{
						num = transform.eulerAngles.z;
					}
					Vector2 vector;
					if (speculativeRigidbody.UpdateCollidersOnScale)
					{
						Vector3 localScale = transform.localScale;
						vector = new Vector2(speculativeRigidbody.AxialScale.x * localScale.x, speculativeRigidbody.AxialScale.y * localScale.y);
					}
					else
					{
						vector = Vector2.one;
					}
					tk2dBaseSprite sprite = speculativeRigidbody.sprite;
					if ((bool)sprite)
					{
						Vector2 vector2 = sprite.scale;
						vector = new Vector2(vector.x * Mathf.Abs(vector2.x), vector.y * Mathf.Abs(vector2.y));
					}
					if ((speculativeRigidbody.UpdateCollidersOnRotation && num != speculativeRigidbody.LastRotation) || (speculativeRigidbody.UpdateCollidersOnScale && vector != speculativeRigidbody.LastScale))
					{
						speculativeRigidbody.LastRotation = num;
						speculativeRigidbody.LastScale = vector;
						for (int j = 0; j < count; j++)
						{
							pixelColliders[j].SetRotationAndScale(num, vector);
						}
						speculativeRigidbody.UpdateColliderPositions();
					}
					transform.hasChanged = false;
				}
				List<SpeculativeRigidbody.TemporaryException> temporaryCollisionExceptions = speculativeRigidbody.m_temporaryCollisionExceptions;
				if (temporaryCollisionExceptions != null)
				{
					for (int num2 = temporaryCollisionExceptions.Count - 1; num2 >= 0; num2--)
					{
						SpeculativeRigidbody.TemporaryException value = temporaryCollisionExceptions[num2];
						if (value.HasEnded(speculativeRigidbody))
						{
							speculativeRigidbody.DeregisterTemporaryCollisionException(temporaryCollisionExceptions[num2].SpecRigidbody);
						}
						else
						{
							temporaryCollisionExceptions[num2] = value;
						}
					}
				}
				bool flag = false;
				for (int k = 0; k < count; k++)
				{
					PixelCollider pixelCollider = pixelColliders[k];
					if (pixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.BagelCollider && !pixelCollider.BagleUseFirstFrameOnly && pixelCollider.m_lastSpriteDef != pixelCollider.Sprite.GetTrueCurrentSpriteDef())
					{
						pixelCollider.RegenerateFromBagelCollider(pixelCollider.Sprite, transform, pixelCollider.m_rotation);
						flag = true;
					}
				}
				if (flag)
				{
					UpdatePosition(speculativeRigidbody);
				}
				if (speculativeRigidbody.HasTriggerCollisions)
				{
					speculativeRigidbody.ResetTriggerCollisionData();
				}
				if (speculativeRigidbody.HasFrameSpecificCollisionExceptions)
				{
					speculativeRigidbody.ClearFrameSpecificCollisionExceptions();
				}
				if (speculativeRigidbody.OnPreMovement != null)
				{
					speculativeRigidbody.OnPreMovement(speculativeRigidbody);
				}
			}
			if (m_nbt.tileMap == null)
			{
				m_nbt.tileMap = TileMap;
				m_nbt.layerName = "Collision Layer";
				m_nbt.layer = BraveUtility.GetTileMapLayerByName("Collision Layer", TileMap);
			}
			float deltaTime = BraveTime.DeltaTime;
			for (int l = 0; l < m_rigidbodies.Count; l++)
			{
				SpeculativeRigidbody speculativeRigidbody2 = m_rigidbodies[l];
				if (!speculativeRigidbody2 || !speculativeRigidbody2.isActiveAndEnabled)
				{
					continue;
				}
				Position position = speculativeRigidbody2.m_position;
				Vector2 vector3 = new Vector2((float)position.m_position.x * 0.0625f + position.m_remainder.x, (float)position.m_position.y * 0.0625f + position.m_remainder.y);
				IntVector2 position2 = position.m_position;
				if (speculativeRigidbody2.CapVelocity)
				{
					Vector2 maxVelocity = speculativeRigidbody2.MaxVelocity;
					if (Mathf.Abs(speculativeRigidbody2.Velocity.x) > maxVelocity.x)
					{
						speculativeRigidbody2.Velocity.x = Mathf.Sign(speculativeRigidbody2.Velocity.x) * maxVelocity.x;
					}
					if (Mathf.Abs(speculativeRigidbody2.Velocity.y) > maxVelocity.y)
					{
						speculativeRigidbody2.Velocity.y = Mathf.Sign(speculativeRigidbody2.Velocity.y) * maxVelocity.y;
					}
				}
				if (float.IsNaN(speculativeRigidbody2.Velocity.x))
				{
					speculativeRigidbody2.Velocity.x = 0f;
				}
				else
				{
					speculativeRigidbody2.Velocity.x = Mathf.Clamp(speculativeRigidbody2.Velocity.x, -1000f, 1000f);
				}
				if (float.IsNaN(speculativeRigidbody2.Velocity.y))
				{
					speculativeRigidbody2.Velocity.y = 0f;
				}
				else
				{
					speculativeRigidbody2.Velocity.y = Mathf.Clamp(speculativeRigidbody2.Velocity.y, -1000f, 1000f);
				}
				speculativeRigidbody2.TimeRemaining = deltaTime;
				if (speculativeRigidbody2.Velocity == Vector2.zero && speculativeRigidbody2.ImpartedPixelsToMove == IntVector2.Zero && !speculativeRigidbody2.ForceAlwaysUpdate && (!speculativeRigidbody2.PathMode || speculativeRigidbody2.PathSpeed == 0f))
				{
					speculativeRigidbody2.TimeRemaining = 0f;
				}
				Vector2? vector4 = null;
				List<SpeculativeRigidbody.PushedRigidbodyData> pushedRigidbodies = speculativeRigidbody2.m_pushedRigidbodies;
				int count2 = pushedRigidbodies.Count;
				for (int m = 0; m < count2; m++)
				{
					SpeculativeRigidbody.PushedRigidbodyData value2 = pushedRigidbodies[m];
					value2.PushedThisFrame = false;
					pushedRigidbodies[m] = value2;
				}
				if (speculativeRigidbody2.PathMode)
				{
					speculativeRigidbody2.Velocity = (PixelToUnit(speculativeRigidbody2.PathTarget) - speculativeRigidbody2.Position.UnitPosition).normalized * speculativeRigidbody2.PathSpeed;
				}
				int num3 = 0;
				while (speculativeRigidbody2.TimeRemaining > 0f && num3 < 50)
				{
					if (vector4.HasValue)
					{
						speculativeRigidbody2.Velocity = vector4.Value;
						vector4 = null;
					}
					float timeRemaining = speculativeRigidbody2.TimeRemaining;
					float timeUsed = timeRemaining;
					bool flag2 = false;
					IntVector2 impartedPixelsToMove = speculativeRigidbody2.ImpartedPixelsToMove;
					if (impartedPixelsToMove.x != 0 || impartedPixelsToMove.y != 0)
					{
						flag2 = true;
						timeUsed = 0f;
						speculativeRigidbody2.PixelsToMove = impartedPixelsToMove;
					}
					else
					{
						Vector2 velocity = speculativeRigidbody2.Velocity;
						Vector2 vector5 = new Vector2(velocity.x * timeRemaining, velocity.y * timeRemaining);
						IntVector2 position3 = speculativeRigidbody2.m_position.m_position;
						Vector2 remainder = speculativeRigidbody2.m_position.m_remainder;
						speculativeRigidbody2.PixelsToMove = new IntVector2(Mathf.RoundToInt(((float)position3.x * 0.0625f + remainder.x + vector5.x) * 16f) - position3.x, Mathf.RoundToInt(((float)position3.y * 0.0625f + remainder.y + vector5.y) * 16f) - position3.y);
					}
					speculativeRigidbody2.CollidedX = false;
					speculativeRigidbody2.CollidedY = false;
					CollisionData obj = null;
					bool flag3 = true;
					bool flag4 = true;
					List<PixelCollider.StepData> stepList = PixelCollider.m_stepList;
					if (flag2)
					{
						PixelMovementGenerator(speculativeRigidbody2.PixelsToMove, stepList);
					}
					else
					{
						PixelMovementGenerator(speculativeRigidbody2.m_position.m_remainder, speculativeRigidbody2.Velocity, speculativeRigidbody2.PixelsToMove, stepList);
					}
					if (speculativeRigidbody2.PathMode)
					{
						float num4 = Vector2.Distance(PixelToUnit(speculativeRigidbody2.PathTarget), speculativeRigidbody2.m_position.UnitPosition) / speculativeRigidbody2.PathSpeed;
						if (num4 <= timeRemaining && (obj == null || num4 < obj.TimeUsed))
						{
							CollisionData.Pool.Free(ref obj);
							if (obj == null)
							{
								obj = CollisionData.Pool.Allocate();
							}
							obj.collisionType = CollisionData.CollisionType.PathEnd;
							obj.NewPixelsToMove = speculativeRigidbody2.PathTarget - speculativeRigidbody2.m_position.PixelPosition;
							obj.CollidedX = speculativeRigidbody2.Velocity.x != 0f;
							obj.CollidedY = speculativeRigidbody2.Velocity.y != 0f;
							obj.MyRigidbody = speculativeRigidbody2;
							obj.MyPixelCollider = speculativeRigidbody2.PrimaryPixelCollider;
							obj.TimeUsed = num4;
						}
					}
					if (speculativeRigidbody2.MovementRestrictor != null)
					{
						IntVector2 prevPixelOffset = IntVector2.Zero;
						IntVector2 zero = IntVector2.Zero;
						for (int n = 0; n < stepList.Count; n++)
						{
							bool validLocation = true;
							zero += stepList[n].deltaPos;
							speculativeRigidbody2.MovementRestrictor(speculativeRigidbody2, prevPixelOffset, zero, ref validLocation);
							if (!validLocation)
							{
								float num5 = 0f;
								for (int num6 = 0; num6 <= n; num6++)
								{
									num5 += stepList[num6].deltaTime;
								}
								if (obj == null || num5 < obj.TimeUsed)
								{
									CollisionData.Pool.Free(ref obj);
									if (obj == null)
									{
										obj = CollisionData.Pool.Allocate();
									}
									obj.collisionType = CollisionData.CollisionType.MovementRestriction;
									obj.NewPixelsToMove = zero - stepList[n].deltaPos;
									obj.CollidedX = stepList[n].deltaPos.x != 0;
									obj.CollidedY = stepList[n].deltaPos.y != 0;
									obj.MyRigidbody = speculativeRigidbody2;
									obj.MyPixelCollider = speculativeRigidbody2.PrimaryPixelCollider;
									obj.TimeUsed = num5;
								}
								break;
							}
							prevPixelOffset = zero;
						}
					}
					if (speculativeRigidbody2.CanPush && speculativeRigidbody2.PushedRigidbodies.Count > 0)
					{
						for (int num7 = 0; num7 < speculativeRigidbody2.PushedRigidbodies.Count; num7++)
						{
							SpeculativeRigidbody.PushedRigidbodyData pushedRigidbodyData = speculativeRigidbody2.PushedRigidbodies[num7];
							if (!pushedRigidbodyData.PushedThisFrame)
							{
								continue;
							}
							IntVector2 pushedPixelsToMove = pushedRigidbodyData.GetPushedPixelsToMove(speculativeRigidbody2.PixelsToMove);
							CollisionData result;
							if (!RigidbodyCast(pushedRigidbodyData.SpecRigidbody, pushedPixelsToMove, out result))
							{
								continue;
							}
							result.collisionType = CollisionData.CollisionType.Pushable;
							result.CollidedX = pushedRigidbodyData.CollidedX;
							result.CollidedY = pushedRigidbodyData.CollidedY;
							result.TimeUsed = 0f;
							result.NewPixelsToMove = IntVector2.Zero;
							for (int num8 = 0; num8 < stepList.Count; num8++)
							{
								if (pushedRigidbodyData.GetPushedPixelsToMove(result.NewPixelsToMove) == result.NewPixelsToMove)
								{
									break;
								}
								result.NewPixelsToMove += stepList[num8].deltaPos;
								result.TimeUsed += stepList[num8].deltaTime;
							}
							result.IsPushCollision = true;
							if (obj == null || result.TimeUsed < obj.TimeUsed)
							{
								obj = result;
							}
						}
						if (flag2)
						{
							PixelMovementGenerator(speculativeRigidbody2.PixelsToMove, stepList);
						}
						else
						{
							PixelMovementGenerator(speculativeRigidbody2, stepList);
						}
					}
					if (speculativeRigidbody2.CollideWithOthers)
					{
						CollideWithRigidbodies(speculativeRigidbody2, stepList, ref obj);
					}
					if (speculativeRigidbody2.CollideWithTileMap && dungeon != null && TileMap != null)
					{
						List<PixelCollider> pixelColliders2 = speculativeRigidbody2.PixelColliders;
						for (int num9 = 0; num9 < pixelColliders2.Count; num9++)
						{
							PixelCollider pixelCollider2 = pixelColliders2[num9];
							if (pixelCollider2.Enabled && (pixelCollider2.CollisionLayer == CollisionLayer.TileBlocker || (CollisionLayerMatrix.GetMask(pixelCollider2.CollisionLayer) & 0x40) == 64))
							{
								CollideWithTilemap(speculativeRigidbody2, pixelCollider2, stepList, ref timeUsed, data, ref obj);
							}
						}
					}
					if (speculativeRigidbody2.CanPush && speculativeRigidbody2.PushedRigidbodies.Count > 0)
					{
						IntVector2 pixelsToMove = ((obj == null) ? speculativeRigidbody2.PixelsToMove : obj.NewPixelsToMove);
						for (int num10 = 0; num10 < speculativeRigidbody2.PushedRigidbodies.Count; num10++)
						{
							SpeculativeRigidbody.PushedRigidbodyData pushedRigidbodyData2 = speculativeRigidbody2.PushedRigidbodies[num10];
							if (pushedRigidbodyData2.PushedThisFrame)
							{
								SpeculativeRigidbody specRigidbody = pushedRigidbodyData2.SpecRigidbody;
								IntVector2 pushedPixelsToMove2 = pushedRigidbodyData2.GetPushedPixelsToMove(pixelsToMove);
								Position position4 = specRigidbody.Position;
								position4.PixelPosition += pushedPixelsToMove2;
								specRigidbody.Position = position4;
								specRigidbody.transform.position = specRigidbody.Position.GetPixelVector2().ToVector3ZUp(specRigidbody.transform.position.z);
								if (specRigidbody.OnPostRigidbodyMovement != null)
								{
									specRigidbody.OnPostRigidbodyMovement(specRigidbody, PixelToUnit(pushedPixelsToMove2), pushedPixelsToMove2);
								}
							}
						}
						if (obj != null && obj.IsPushCollision && (bool)obj.OtherRigidbody)
						{
							MinorBreakable minorBreakable = obj.OtherRigidbody.minorBreakable;
							if ((bool)minorBreakable && !minorBreakable.isInvulnerableToGameActors)
							{
								minorBreakable.Break(-obj.Normal);
							}
						}
					}
					if (obj != null)
					{
						if (!obj.Overlap && speculativeRigidbody2.CanPush && obj.OtherRigidbody != null && obj.OtherRigidbody.CanBePushed)
						{
							int num11 = -1;
							for (int num12 = 0; num12 < speculativeRigidbody2.PushedRigidbodies.Count; num12++)
							{
								if (speculativeRigidbody2.PushedRigidbodies[num12].SpecRigidbody == obj.OtherRigidbody)
								{
									num11 = num12;
									break;
								}
							}
							if (num11 < 0)
							{
								num11 = speculativeRigidbody2.PushedRigidbodies.Count;
								speculativeRigidbody2.PushedRigidbodies.Add(new SpeculativeRigidbody.PushedRigidbodyData(obj.OtherRigidbody));
							}
							else
							{
								obj.TimeUsed = 0f;
							}
							SpeculativeRigidbody.PushedRigidbodyData value3 = speculativeRigidbody2.PushedRigidbodies[num11];
							value3.Direction = ((!obj.CollidedX) ? IntVector2.Up : IntVector2.Right);
							value3.PushedThisFrame = true;
							speculativeRigidbody2.PushedRigidbodies[num11] = value3;
							obj.MyPixelCollider.RegisterFrameSpecificCollisionException(obj.MyRigidbody, obj.OtherPixelCollider);
							flag3 = false;
							flag4 = false;
							vector4 = speculativeRigidbody2.Velocity;
							if (obj.CollidedX)
							{
								vector4 = vector4.Value.WithX(vector4.Value.x * speculativeRigidbody2.PushSpeedModifier);
							}
							if (obj.CollidedY)
							{
								vector4 = vector4.Value.WithY(vector4.Value.y * speculativeRigidbody2.PushSpeedModifier);
							}
						}
						CollisionHaltsVelocity = null;
						HaltRemainingMovement = false;
						PostSliceVelocity = null;
						CollisionData obj2 = null;
						if (!obj.IsTriggerCollision)
						{
							if (speculativeRigidbody2.OnCollision != null)
							{
								speculativeRigidbody2.OnCollision(obj);
							}
							if (obj.OtherRigidbody != null && obj.OtherRigidbody.OnCollision != null)
							{
								if (obj2 == null)
								{
									obj2 = obj.GetInverse();
								}
								obj.OtherRigidbody.OnCollision(obj.GetInverse());
							}
						}
						if (obj.OtherRigidbody != null)
						{
							if (!obj.IsTriggerCollision)
							{
								if (speculativeRigidbody2.OnRigidbodyCollision != null)
								{
									speculativeRigidbody2.OnRigidbodyCollision(obj);
								}
								if (obj.OtherRigidbody.OnRigidbodyCollision != null)
								{
									if (obj2 == null)
									{
										obj2 = obj.GetInverse();
									}
									obj.OtherRigidbody.OnRigidbodyCollision(obj.GetInverse());
								}
							}
						}
						else if (obj.TileLayerName != null && speculativeRigidbody2.OnTileCollision != null)
						{
							speculativeRigidbody2.OnTileCollision(obj);
						}
						if (CollisionHaltsVelocity.HasValue)
						{
							flag3 = CollisionHaltsVelocity.Value;
						}
						if (obj.OtherRigidbody != null && obj.IsTriggerCollision)
						{
							SpeculativeRigidbody otherRigidbody = obj.OtherRigidbody;
							speculativeRigidbody2.PixelsToMove = obj.NewPixelsToMove;
							speculativeRigidbody2.CollidedX = obj.CollidedX;
							speculativeRigidbody2.CollidedY = obj.CollidedY;
							timeUsed = obj.TimeUsed;
							flag3 = false;
							flag4 = false;
							obj.MyPixelCollider.RegisterFrameSpecificCollisionException(speculativeRigidbody2, obj.OtherPixelCollider);
							TriggerCollisionData triggerCollisionData = obj.MyPixelCollider.RegisterTriggerCollision(obj.MyRigidbody, obj.OtherRigidbody, obj.OtherPixelCollider);
							TriggerCollisionData triggerCollisionData2 = obj.OtherPixelCollider.RegisterTriggerCollision(obj.MyRigidbody, obj.MyRigidbody, obj.MyPixelCollider);
							if (triggerCollisionData.FirstFrame)
							{
								if (speculativeRigidbody2.OnEnterTrigger != null)
								{
									speculativeRigidbody2.OnEnterTrigger(otherRigidbody, speculativeRigidbody2, obj);
								}
								if (otherRigidbody.OnEnterTrigger != null)
								{
									if (obj2 == null)
									{
										obj2 = obj.GetInverse();
									}
									otherRigidbody.OnEnterTrigger(speculativeRigidbody2, otherRigidbody, obj.GetInverse());
								}
							}
							if (triggerCollisionData.FirstFrame || triggerCollisionData.ContinuedCollision)
							{
								if (speculativeRigidbody2.OnTriggerCollision != null)
								{
									speculativeRigidbody2.OnTriggerCollision(otherRigidbody, speculativeRigidbody2, obj);
								}
								if (otherRigidbody.OnTriggerCollision != null)
								{
									if (obj2 == null)
									{
										obj2 = obj.GetInverse();
									}
									otherRigidbody.OnTriggerCollision(speculativeRigidbody2, otherRigidbody, obj.GetInverse());
								}
							}
							triggerCollisionData.Notified = true;
							triggerCollisionData2.Notified = true;
						}
						else if ((bool)obj.OtherRigidbody && (speculativeRigidbody2.IsGhostCollisionException(obj.OtherRigidbody) || obj.OtherRigidbody.IsGhostCollisionException(speculativeRigidbody2)))
						{
							if (!obj.Overlap)
							{
								speculativeRigidbody2.PixelsToMove = obj.NewPixelsToMove;
								timeUsed = obj.TimeUsed;
							}
							else
							{
								speculativeRigidbody2.PixelsToMove = IntVector2.Zero;
								timeUsed = 0f;
							}
							obj.MyPixelCollider.RegisterFrameSpecificCollisionException(speculativeRigidbody2, obj.OtherPixelCollider);
						}
						else
						{
							speculativeRigidbody2.CollidedX = obj.CollidedX;
							speculativeRigidbody2.CollidedY = obj.CollidedY;
							speculativeRigidbody2.PixelsToMove = obj.NewPixelsToMove;
							timeUsed = obj.TimeUsed;
						}
						if (obj2 != null)
						{
							CollisionData.Pool.Free(ref obj2);
						}
						if (!flag2 && obj.collisionType != CollisionData.CollisionType.PathEnd)
						{
							float num13 = PixelToUnit(1) / 2f;
							if (speculativeRigidbody2.CollidedX && !speculativeRigidbody2.CollidedY)
							{
								timeUsed = Mathf.Max(0f, timeUsed - Mathf.Abs(num13 / speculativeRigidbody2.Velocity.x));
							}
							else if (speculativeRigidbody2.CollidedY && !speculativeRigidbody2.CollidedX)
							{
								timeUsed = Mathf.Max(0f, timeUsed - Mathf.Abs(num13 / speculativeRigidbody2.Velocity.y));
							}
						}
					}
					if (flag2)
					{
						timeUsed = 0f;
						speculativeRigidbody2.Position = new Position(speculativeRigidbody2.Position.PixelPosition + speculativeRigidbody2.PixelsToMove, speculativeRigidbody2.Position.Remainder);
						speculativeRigidbody2.ImpartedPixelsToMove -= speculativeRigidbody2.PixelsToMove;
						if (obj == null || !obj.IsTriggerCollision)
						{
							if (speculativeRigidbody2.CollidedX)
							{
								speculativeRigidbody2.ImpartedPixelsToMove = speculativeRigidbody2.ImpartedPixelsToMove.WithX(0);
							}
							if (speculativeRigidbody2.CollidedY)
							{
								speculativeRigidbody2.ImpartedPixelsToMove = speculativeRigidbody2.ImpartedPixelsToMove.WithY(0);
							}
						}
					}
					else
					{
						Position position5 = speculativeRigidbody2.Position;
						if (speculativeRigidbody2.CollidedX && flag4)
						{
							position5.X += speculativeRigidbody2.PixelsToMove.x;
						}
						else
						{
							position5.UnitX += speculativeRigidbody2.Velocity.x * timeUsed;
						}
						if (speculativeRigidbody2.CollidedY && flag4)
						{
							position5.Y += speculativeRigidbody2.PixelsToMove.y;
						}
						else
						{
							position5.UnitY += speculativeRigidbody2.Velocity.y * timeUsed;
						}
						if (flag3)
						{
							if (speculativeRigidbody2.CollidedX)
							{
								speculativeRigidbody2.Velocity.x = 0f;
							}
							if (speculativeRigidbody2.CollidedY)
							{
								speculativeRigidbody2.Velocity.y = 0f;
							}
						}
						if (PostSliceVelocity.HasValue)
						{
							speculativeRigidbody2.Velocity = PostSliceVelocity.Value;
							PostSliceVelocity = null;
						}
						speculativeRigidbody2.Position = position5;
					}
					if (speculativeRigidbody2.CarriedRigidbodies != null)
					{
						for (int num14 = 0; num14 < speculativeRigidbody2.CarriedRigidbodies.Count; num14++)
						{
							SpeculativeRigidbody speculativeRigidbody3 = speculativeRigidbody2.CarriedRigidbodies[num14];
							if (speculativeRigidbody3.CanBeCarried || speculativeRigidbody2.ForceCarriesRigidbodies)
							{
								speculativeRigidbody3.ImpartedPixelsToMove += speculativeRigidbody2.PixelsToMove;
							}
						}
					}
					if (speculativeRigidbody2.IgnorePixelGrid)
					{
						IntVector2 position6 = speculativeRigidbody2.m_position.m_position;
						Vector2 remainder2 = speculativeRigidbody2.m_position.m_remainder;
						Transform transform2 = speculativeRigidbody2.transform;
						transform2.position = new Vector3((float)position6.x * 0.0625f + remainder2.x, (float)position6.y * 0.0625f + remainder2.y, transform2.position.z);
					}
					else
					{
						IntVector2 position7 = speculativeRigidbody2.Position.m_position;
						Transform transform3 = speculativeRigidbody2.transform;
						transform3.position = new Vector3((float)position7.x * 0.0625f, (float)position7.y * 0.0625f, transform3.position.z);
					}
					speculativeRigidbody2.TimeRemaining -= timeUsed;
					if (speculativeRigidbody2.PathMode && obj != null && obj.collisionType == CollisionData.CollisionType.PathEnd)
					{
						if (speculativeRigidbody2.OnPathTargetReached != null)
						{
							speculativeRigidbody2.OnPathTargetReached();
							if (speculativeRigidbody2.PathMode)
							{
								speculativeRigidbody2.Velocity = (PixelToUnit(speculativeRigidbody2.PathTarget) - speculativeRigidbody2.Position.UnitPosition).normalized * speculativeRigidbody2.PathSpeed;
							}
						}
						else
						{
							speculativeRigidbody2.PathMode = false;
						}
					}
					if (HaltRemainingMovement || (speculativeRigidbody2.Velocity == Vector2.zero && speculativeRigidbody2.ImpartedPixelsToMove == IntVector2.Zero && !speculativeRigidbody2.PathMode && !speculativeRigidbody2.HasUnresolvedTriggerCollisions))
					{
						speculativeRigidbody2.TimeRemaining = 0f;
					}
					num3++;
					if (obj != null)
					{
						CollisionData.Pool.Free(ref obj);
					}
				}
				List<SpeculativeRigidbody.PushedRigidbodyData> pushedRigidbodies2 = speculativeRigidbody2.m_pushedRigidbodies;
				for (int num15 = pushedRigidbodies2.Count - 1; num15 >= 0; num15--)
				{
					if (!pushedRigidbodies2[num15].PushedThisFrame)
					{
						pushedRigidbodies2.RemoveAt(num15);
					}
				}
				IntVector2 arg = speculativeRigidbody2.Position.m_position - position2;
				if (speculativeRigidbody2.OnPostRigidbodyMovement != null)
				{
					speculativeRigidbody2.OnPostRigidbodyMovement(speculativeRigidbody2, speculativeRigidbody2.m_position.UnitPosition - vector3, arg);
				}
				if (speculativeRigidbody2.TK2DSprite != null && (speculativeRigidbody2.TK2DSprite.IsZDepthDirty || arg.x != 0 || arg.y != 0))
				{
					speculativeRigidbody2.TK2DSprite.UpdateZDepth();
				}
				speculativeRigidbody2.RecheckTriggers = false;
			}
			if (this.OnPostRigidbodyMovement != null)
			{
				this.OnPostRigidbodyMovement();
			}
			for (int num16 = 0; num16 < m_rigidbodies.Count; num16++)
			{
				SpeculativeRigidbody speculativeRigidbody4 = m_rigidbodies[num16];
				if (!speculativeRigidbody4 || !speculativeRigidbody4.HasTriggerCollisions)
				{
					continue;
				}
				for (int num17 = 0; num17 < speculativeRigidbody4.PixelColliders.Count; num17++)
				{
					PixelCollider pixelCollider3 = speculativeRigidbody4.PixelColliders[num17];
					for (int num18 = pixelCollider3.TriggerCollisions.Count - 1; num18 >= 0; num18--)
					{
						TriggerCollisionData triggerCollisionData3 = pixelCollider3.TriggerCollisions[num18];
						PixelCollider pixelCollider4 = triggerCollisionData3.PixelCollider;
						SpeculativeRigidbody specRigidbody2 = triggerCollisionData3.SpecRigidbody;
						if (!triggerCollisionData3.Notified)
						{
							if (!triggerCollisionData3.FirstFrame && !triggerCollisionData3.ContinuedCollision)
							{
								if (speculativeRigidbody4.OnExitTrigger != null)
								{
									speculativeRigidbody4.OnExitTrigger(specRigidbody2, speculativeRigidbody4);
								}
								if (specRigidbody2.OnExitTrigger != null)
								{
									specRigidbody2.OnExitTrigger(speculativeRigidbody4, specRigidbody2);
								}
							}
							triggerCollisionData3.Notified = true;
							for (int num19 = 0; num19 < pixelCollider4.TriggerCollisions.Count; num19++)
							{
								if (pixelCollider4.TriggerCollisions[num19].PixelCollider == pixelCollider3)
								{
									pixelCollider4.TriggerCollisions[num19].Notified = true;
									break;
								}
							}
						}
						if (!triggerCollisionData3.FirstFrame && !triggerCollisionData3.ContinuedCollision)
						{
							pixelCollider3.TriggerCollisions.RemoveAt(num18);
							for (int num20 = 0; num20 < pixelCollider4.TriggerCollisions.Count; num20++)
							{
								if (pixelCollider4.TriggerCollisions[num20].PixelCollider == pixelCollider3)
								{
									pixelCollider4.TriggerCollisions.RemoveAt(num20);
									num20--;
								}
							}
						}
					}
				}
			}
			for (int num21 = 0; num21 < m_rigidbodies.Count; num21++)
			{
				SpeculativeRigidbody speculativeRigidbody5 = m_rigidbodies[num21];
				if (!speculativeRigidbody5.isActiveAndEnabled)
				{
					continue;
				}
				List<SpeculativeRigidbody> ghostCollisionExceptions = speculativeRigidbody5.GhostCollisionExceptions;
				if (ghostCollisionExceptions == null)
				{
					continue;
				}
				for (int num22 = 0; num22 < ghostCollisionExceptions.Count; num22++)
				{
					SpeculativeRigidbody speculativeRigidbody6 = ghostCollisionExceptions[num22];
					bool flag5 = false;
					if ((bool)speculativeRigidbody6)
					{
						for (int num23 = 0; num23 < speculativeRigidbody5.PixelColliders.Count; num23++)
						{
							if (flag5)
							{
								break;
							}
							PixelCollider pixelCollider5 = speculativeRigidbody5.PixelColliders[num23];
							for (int num24 = 0; num24 < speculativeRigidbody6.PixelColliders.Count; num24++)
							{
								if (flag5)
								{
									break;
								}
								PixelCollider otherCollider = speculativeRigidbody6.PixelColliders[num24];
								if (pixelCollider5.CanCollideWith(otherCollider, true))
								{
									flag5 |= pixelCollider5.Overlaps(otherCollider);
								}
							}
						}
					}
					if (!flag5)
					{
						speculativeRigidbody5.DeregisterGhostCollisionException(num22);
						num22--;
					}
				}
			}
			if (DebugDraw != 0)
			{
				m_debugTilesDrawnThisFrame.Clear();
			}
		}
		for (int num25 = 0; num25 < m_deregisterRigidBodies.Count; num25++)
		{
			Deregister(m_deregisterRigidBodies[num25]);
		}
		m_deregisterRigidBodies.Clear();
	}

	public void Query(Vector2 worldMin, Vector2 worldMax, Func<SpeculativeRigidbody, bool> callback)
	{
		m_rigidbodyTree.Query(GetSafeB2AABB(UnitToPixel(worldMin), UnitToPixel(worldMax)), callback);
	}

	public bool Raycast(Vector2 unitOrigin, Vector2 direction, float dist, out RaycastResult result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int rayMask = int.MaxValue, CollisionLayer? sourceLayer = null, bool collideWithTriggers = false, Func<SpeculativeRigidbody, bool> rigidbodyExcluder = null, SpeculativeRigidbody ignoreRigidbody = null)
	{
		bool result2;
		if (ignoreRigidbody == null)
		{
			result2 = RaycastWithIgnores(new Position(unitOrigin), direction, dist, out result, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, rigidbodyExcluder, m_emptyIgnoreList);
		}
		else
		{
			m_singleIgnoreList[0] = ignoreRigidbody;
			result2 = RaycastWithIgnores(new Position(unitOrigin), direction, dist, out result, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, rigidbodyExcluder, m_singleIgnoreList);
			m_singleIgnoreList[0] = null;
		}
		return result2;
	}

	public bool RaycastWithIgnores(Vector2 unitOrigin, Vector2 direction, float dist, out RaycastResult result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int rayMask = int.MaxValue, CollisionLayer? sourceLayer = null, bool collideWithTriggers = false, Func<SpeculativeRigidbody, bool> rigidbodyExcluder = null, ICollection<SpeculativeRigidbody> ignoreList = null)
	{
		return RaycastWithIgnores(new Position(unitOrigin), direction, dist, out result, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, rigidbodyExcluder, ignoreList);
	}

	public bool RaycastWithIgnores(Position origin, Vector2 direction, float dist, out RaycastResult result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int rayMask = int.MaxValue, CollisionLayer? sourceLayer = null, bool collideWithTriggers = false, Func<SpeculativeRigidbody, bool> rigidbodyExcluder = null, ICollection<SpeculativeRigidbody> ignoreList = null)
	{
		m_raycaster.SetAll(this, GameManager.Instance.Dungeon.data, origin, direction, dist, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, rigidbodyExcluder, ignoreList);
		bool result2 = m_raycaster.DoRaycast(out result);
		m_raycaster.Clear();
		return result2;
	}

	public bool Pointcast(Vector2 point, out SpeculativeRigidbody result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int rayMask = int.MaxValue, CollisionLayer? sourceLayer = null, bool collideWithTriggers = false, params SpeculativeRigidbody[] ignoreList)
	{
		return Pointcast(UnitToPixel(point), out result, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, ignoreList);
	}

	public bool Pointcast(IntVector2 point, out SpeculativeRigidbody result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int rayMask = int.MaxValue, CollisionLayer? sourceLayer = null, bool collideWithTriggers = false, params SpeculativeRigidbody[] ignoreList)
	{
		ICollidableObject tempResult = null;
		Func<ICollidableObject, IntVector2, ICollidableObject> collideWithCollidable = delegate(ICollidableObject collidable, IntVector2 p)
		{
			SpeculativeRigidbody speculativeRigidbody = collidable as SpeculativeRigidbody;
			if ((bool)speculativeRigidbody && !speculativeRigidbody.enabled)
			{
				return null;
			}
			for (int i = 0; i < collidable.GetPixelColliders().Count; i++)
			{
				PixelCollider pixelCollider = collidable.GetPixelColliders()[i];
				if ((collideWithTriggers || !pixelCollider.IsTrigger) && pixelCollider.CanCollideWith(rayMask, sourceLayer) && pixelCollider.ContainsPixel(p))
				{
					return collidable;
				}
			}
			return null;
		};
		if (collideWithTiles && (bool)TileMap)
		{
			int x;
			int y;
			TileMap.GetTileAtPosition(PixelToUnit(point), out x, out y);
			int tileMapLayerByName = BraveUtility.GetTileMapLayerByName("Collision Layer", TileMap);
			Tile tile = GetTile(x, y, TileMap, tileMapLayerByName, "Collision Layer", GameManager.Instance.Dungeon.data);
			if (tile != null)
			{
				tempResult = collideWithCollidable(tile, point);
				if (tempResult != null)
				{
					result = tempResult as SpeculativeRigidbody;
					return true;
				}
			}
		}
		if (collideWithRigidbodies)
		{
			Func<SpeculativeRigidbody, bool> callback = delegate(SpeculativeRigidbody rigidbody)
			{
				tempResult = collideWithCollidable(rigidbody, point);
				return tempResult == null;
			};
			m_rigidbodyTree.Query(GetSafeB2AABB(point, point), callback);
			if (CollidesWithProjectiles(rayMask, sourceLayer))
			{
				m_projectileTree.Query(GetSafeB2AABB(point, point), callback);
			}
		}
		result = tempResult as SpeculativeRigidbody;
		return result != null;
	}

	private static b2AABB GetSafeB2AABB(IntVector2 lowerBounds, IntVector2 upperBounds)
	{
		return new b2AABB(PixelToUnit(lowerBounds - IntVector2.One), PixelToUnit(upperBounds + 2 * IntVector2.One));
	}

	public bool Pointcast(List<IntVector2> points, List<IntVector2> lastFramePoints, int pointsWidth, out List<PointcastResult> pointResults, bool collideWithTiles = true, bool collideWithRigidbodies = true, int rayMask = int.MaxValue, CollisionLayer? sourceLayer = null, bool collideWithTriggers = false, Func<SpeculativeRigidbody, bool> rigidbodyExcluder = null, int ignoreTileBoneCount = 0, params SpeculativeRigidbody[] ignoreList)
	{
		int tileMapLayerByName = BraveUtility.GetTileMapLayerByName("Collision Layer", TileMap);
		pointResults = new List<PointcastResult>();
		c_boundedRigidbodies.Clear();
		if (collideWithRigidbodies)
		{
			IntVector2 pointMin = IntVector2.MaxValue;
			IntVector2 pointMax = IntVector2.MinValue;
			for (int i = 0; i < points.Count; i++)
			{
				pointMin = IntVector2.Min(pointMin, points[i]);
				pointMax = IntVector2.Max(pointMax, points[i]);
			}
			Func<SpeculativeRigidbody, bool> callback = delegate(SpeculativeRigidbody rigidbody)
			{
				if (!rigidbody || !rigidbody.enabled || !rigidbody.CollideWithOthers)
				{
					return true;
				}
				if (rigidbodyExcluder != null && rigidbodyExcluder(rigidbody))
				{
					return true;
				}
				if (Array.IndexOf(ignoreList, rigidbody) >= 0)
				{
					return true;
				}
				for (int num10 = 0; num10 < rigidbody.PixelColliders.Count; num10++)
				{
					PixelCollider pixelCollider = rigidbody.PixelColliders[num10];
					if ((collideWithTriggers || !pixelCollider.IsTrigger) && pixelCollider.CanCollideWith(rayMask, sourceLayer) && pixelCollider.AABBOverlaps(pointMin, pointMax - pointMin + IntVector2.One))
					{
						c_boundedRigidbodies.Add(rigidbody);
						break;
					}
				}
				return true;
			};
			m_rigidbodyTree.Query(GetSafeB2AABB(pointMin, pointMax), callback);
			if (CollidesWithProjectiles(rayMask, sourceLayer))
			{
				m_projectileTree.Query(GetSafeB2AABB(pointMin, pointMax), callback);
			}
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		HitDirection[] array = new HitDirection[pointsWidth];
		for (int j = 0; j < pointsWidth; j++)
		{
			array[j] = HitDirection.Forward;
		}
		for (int k = 0; k < points.Count - pointsWidth; k++)
		{
			Vector2 a = PixelToUnit(points[k]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
			Vector2 b = PixelToUnit(points[k + pointsWidth]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
			int num = k % pointsWidth;
			for (int l = 0; (k < points.Count - 2 * pointsWidth) ? (l < 2) : (l <= 2); l++)
			{
				bool flag = false;
				IntVector2 intVector = UnitToPixel(Vector2.Lerp(a, b, (float)l / 2f));
				if (collideWithTiles && (bool)TileMap && k >= ignoreTileBoneCount)
				{
					int x;
					int y;
					TileMap.GetTileAtPosition(PixelToUnit(intVector), out x, out y);
					Tile tile = GetTile(x, y, TileMap, tileMapLayerByName, "Collision Layer", data);
					if (tile != null && Pointcast_CoarsePass(tile, intVector, collideWithTriggers, rayMask, sourceLayer))
					{
						flag = true;
					}
				}
				if (collideWithRigidbodies && !flag)
				{
					for (int m = 0; m < c_boundedRigidbodies.Count; m++)
					{
						if (Pointcast_CoarsePass(c_boundedRigidbodies[m], intVector, collideWithTriggers, rayMask, sourceLayer))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag && array[num] == HitDirection.Backward)
				{
					Vector2 a2 = PixelToUnit(lastFramePoints[k]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					Vector2 b2 = PixelToUnit(lastFramePoints[k + pointsWidth]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					IntVector2 pixel = UnitToPixel(Vector2.Lerp(a2, b2, (float)l / 2f));
					Vector2 vector = PixelToUnit(pixel) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					Vector2 vector2 = PixelToUnit(intVector) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					Vector2 normalized = (vector2 - vector).normalized;
					float dist = (vector2 - vector).magnitude + 1.41421354f * PixelUnitWidth;
					RaycastResult result;
					flag = RaycastWithIgnores(vector, normalized, dist, out result, collideWithTiles, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, null, ignoreList);
					RaycastResult.Pool.Free(ref result);
				}
				if (flag && array[num] == HitDirection.Forward)
				{
					int num2;
					int num3;
					Vector2 vector3;
					Vector2 vector4;
					if (k < pointsWidth && l == 0)
					{
						num2 = 0;
						num3 = 0;
						vector3 = PixelToUnit(lastFramePoints[0]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
						vector4 = PixelToUnit(points[0]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					}
					else
					{
						num2 = ((l != 0) ? k : (k - pointsWidth));
						num3 = num2 + pointsWidth;
						vector3 = PixelToUnit(points[num2]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
						vector4 = PixelToUnit(points[num3]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					}
					Vector2 normalized2 = (vector4 - vector3).normalized;
					float dist2 = (vector4 - vector3).magnitude + 1.41421354f * PixelUnitWidth;
					RaycastResult result2;
					if (RaycastWithIgnores(vector3, normalized2, dist2, out result2, collideWithTiles && num3 >= ignoreTileBoneCount, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, null, ignoreList))
					{
						PointcastResult pointcastResult = PointcastResult.Pool.Allocate();
						pointcastResult.SetAll(HitDirection.Forward, num2, num2 / pointsWidth, result2);
						pointResults.Add(pointcastResult);
						array[num] = HitDirection.Backward;
					}
				}
				else if (!flag && array[num] == HitDirection.Backward)
				{
					int num4 = ((l != 0) ? k : (k - pointsWidth));
					int num5 = num4 + pointsWidth;
					Vector2 vector5 = PixelToUnit(points[num4]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					Vector2 vector6 = PixelToUnit(points[num5]) + new Vector2(HalfPixelUnitWidth, HalfPixelUnitWidth);
					Vector2 normalized3 = (vector5 - vector6).normalized;
					float num6 = (vector5 - vector6).magnitude + 1.41421354f * PixelUnitWidth;
					num6 *= 3f;
					RaycastResult result3;
					if (RaycastWithIgnores(vector6, normalized3, num6 * 3f, out result3, collideWithTiles && num4 >= ignoreTileBoneCount, collideWithRigidbodies, rayMask, sourceLayer, collideWithTriggers, null, ignoreList))
					{
						PointcastResult pointcastResult2 = PointcastResult.Pool.Allocate();
						pointcastResult2.SetAll(HitDirection.Backward, num5, num5 / pointsWidth, result3);
						pointResults.Add(pointcastResult2);
						array[num] = HitDirection.Forward;
					}
				}
			}
		}
		if (pointsWidth > 1)
		{
			pointResults.Sort();
			List<PointcastResult> list = new List<PointcastResult>();
			int num7 = 0;
			int num8 = 0;
			while (num7 < pointResults.Count)
			{
				int num9 = num8;
				int n;
				for (n = num7; n < pointResults.Count && pointResults[num7].boneIndex == pointResults[n].boneIndex; n++)
				{
					if (pointResults[n].hitDirection == HitDirection.Forward)
					{
						num8++;
					}
					else if (pointResults[n].hitDirection == HitDirection.Backward)
					{
						num8--;
					}
				}
				if (num7 == 0 && num8 > 0)
				{
					list.Add(pointResults[num7]);
				}
				else if (num9 == 0 && num8 > 0)
				{
					list.Add(pointResults[num7]);
				}
				else if (num9 >= 0 && num8 == 0)
				{
					list.Add(pointResults[num7]);
				}
				num7 = n;
			}
			pointResults = list;
		}
		return pointResults.Count > 0;
	}

	private bool Pointcast_CoarsePass(ICollidableObject collidable, IntVector2 point, bool collideWithTriggers, int rayMask, CollisionLayer? sourceLayer)
	{
		for (int i = 0; i < collidable.GetPixelColliders().Count; i++)
		{
			PixelCollider pixelCollider = collidable.GetPixelColliders()[i];
			if ((collideWithTriggers || !pixelCollider.IsTrigger) && pixelCollider.CanCollideWith(rayMask, sourceLayer) && pixelCollider.ContainsPixel(point))
			{
				return true;
			}
		}
		return false;
	}

	public bool RigidbodyCast(SpeculativeRigidbody rigidbody, IntVector2 pixelsToMove, out CollisionData result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int? overrideCollisionMask = null, bool collideWithTriggers = false)
	{
		return RigidbodyCastWithIgnores(rigidbody, pixelsToMove, out result, collideWithTiles, collideWithRigidbodies, overrideCollisionMask, collideWithTriggers, emptyIgnoreList);
	}

	public bool RigidbodyCastWithIgnores(SpeculativeRigidbody rigidbody, IntVector2 pixelsToMove, out CollisionData result, bool collideWithTiles = true, bool collideWithRigidbodies = true, int? overrideCollisionMask = null, bool collideWithTriggers = false, params SpeculativeRigidbody[] ignoreList)
	{
		m_rigidbodyCaster.SetAll(this, GameManager.Instance.Dungeon.data, rigidbody, pixelsToMove, collideWithTiles, collideWithRigidbodies, overrideCollisionMask, collideWithTriggers, ignoreList);
		bool result2 = m_rigidbodyCaster.DoRigidbodyCast(out result);
		m_rigidbodyCaster.Clear();
		return result2;
	}

	public bool OverlapCast(SpeculativeRigidbody rigidbody, List<CollisionData> overlappingCollisions = null, bool collideWithTiles = true, bool collideWithRigidbodies = true, int? overrideCollisionMask = null, int? ignoreCollisionMask = null, bool collideWithTriggers = false, Vector2? overridePosition = null, Func<SpeculativeRigidbody, bool> rigidbodyExcluder = null, params SpeculativeRigidbody[] ignoreList)
	{
		List<CollisionData> tempOverlappingCollisions = new List<CollisionData>();
		if (!rigidbody || rigidbody.PixelColliders.Count == 0)
		{
			if (overlappingCollisions != null)
			{
				overlappingCollisions.Clear();
			}
			return false;
		}
		IntVector2 intVector = IntVector2.Zero;
		if (overridePosition.HasValue)
		{
			intVector = new Position(overridePosition.Value).PixelPosition - rigidbody.Position.PixelPosition;
			for (int i = 0; i < rigidbody.PixelColliders.Count; i++)
			{
				rigidbody.PixelColliders[i].Position += intVector;
			}
		}
		IntVector2 intVector2 = IntVector2.MaxValue;
		IntVector2 intVector3 = IntVector2.MinValue;
		for (int j = 0; j < rigidbody.PixelColliders.Count; j++)
		{
			PixelCollider pixelCollider = rigidbody.PixelColliders[j];
			intVector2 = IntVector2.Min(intVector2, pixelCollider.Min);
			intVector3 = IntVector2.Max(intVector3, pixelCollider.Max);
		}
		if (collideWithTiles && (bool)TileMap)
		{
			IntVector2 pixel = intVector2 - IntVector2.One;
			IntVector2 pixel2 = intVector3 + IntVector2.One;
			InitNearbyTileCheck(PixelToUnit(pixel), PixelToUnit(pixel2), TileMap);
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (Tile nextNearbyTile = GetNextNearbyTile(data); nextNearbyTile != null; nextNearbyTile = GetNextNearbyTile(data))
			{
				for (int k = 0; k < rigidbody.PixelColliders.Count; k++)
				{
					PixelCollider pixelCollider2 = rigidbody.PixelColliders[k];
					for (int l = 0; l < nextNearbyTile.PixelColliders.Count; l++)
					{
						PixelCollider otherCollider = nextNearbyTile.PixelColliders[l];
						if (pixelCollider2.CanCollideWith(otherCollider) && pixelCollider2.AABBOverlaps(otherCollider) && pixelCollider2.Overlaps(otherCollider))
						{
							CollisionData collisionData = SingleCollision(rigidbody, pixelCollider2, nextNearbyTile, otherCollider, null, false);
							if (collisionData != null)
							{
								tempOverlappingCollisions.Add(collisionData);
							}
						}
					}
				}
			}
		}
		if (collideWithRigidbodies)
		{
			Func<SpeculativeRigidbody, bool> callback = delegate(SpeculativeRigidbody otherRigidbody)
			{
				if ((bool)otherRigidbody && otherRigidbody != rigidbody && otherRigidbody.enabled && otherRigidbody.CollideWithOthers && Array.IndexOf(ignoreList, otherRigidbody) < 0)
				{
					if (rigidbodyExcluder != null && rigidbodyExcluder(otherRigidbody))
					{
						return true;
					}
					for (int num = 0; num < rigidbody.PixelColliders.Count; num++)
					{
						PixelCollider pixelCollider3 = rigidbody.PixelColliders[num];
						for (int num2 = 0; num2 < otherRigidbody.PixelColliders.Count; num2++)
						{
							PixelCollider pixelCollider4 = otherRigidbody.PixelColliders[num2];
							if (collideWithTriggers || !pixelCollider4.IsTrigger)
							{
								bool flag;
								if (overrideCollisionMask.HasValue || ignoreCollisionMask.HasValue)
								{
									int num3 = ((!overrideCollisionMask.HasValue) ? CollisionLayerMatrix.GetMask(pixelCollider3.CollisionLayer) : overrideCollisionMask.Value);
									if (ignoreCollisionMask.HasValue)
									{
										num3 &= ~ignoreCollisionMask.Value;
									}
									flag = pixelCollider4.CanCollideWith(num3);
								}
								else
								{
									flag = pixelCollider3.CanCollideWith(pixelCollider4);
								}
								if (flag && pixelCollider3.AABBOverlaps(pixelCollider4) && pixelCollider3.Overlaps(pixelCollider4))
								{
									CollisionData collisionData2 = SingleCollision(rigidbody, pixelCollider3, otherRigidbody, pixelCollider4, null, false);
									if (collisionData2 != null)
									{
										tempOverlappingCollisions.Add(collisionData2);
									}
								}
							}
						}
					}
				}
				return true;
			};
			m_rigidbodyTree.Query(GetSafeB2AABB(intVector2, intVector3), callback);
			if (CollidesWithProjectiles(rigidbody))
			{
				m_projectileTree.Query(GetSafeB2AABB(intVector2, intVector3), callback);
			}
		}
		if (overridePosition.HasValue)
		{
			for (int m = 0; m < rigidbody.PixelColliders.Count; m++)
			{
				rigidbody.PixelColliders[m].Position -= intVector;
			}
		}
		bool result = tempOverlappingCollisions.Count > 0;
		if (overlappingCollisions == null)
		{
			for (int n = 0; n < tempOverlappingCollisions.Count; n++)
			{
				CollisionData obj = tempOverlappingCollisions[n];
				CollisionData.Pool.Free(ref obj);
			}
		}
		else
		{
			overlappingCollisions.Clear();
			overlappingCollisions.AddRange(tempOverlappingCollisions);
		}
		return result;
	}

	public void RegisterOverlappingGhostCollisionExceptions(SpeculativeRigidbody specRigidbody, int? overrideLayerMask = null, bool includeTriggers = false)
	{
		if (!m_rigidbodies.Contains(specRigidbody))
		{
			specRigidbody.Reinitialize();
		}
		List<SpeculativeRigidbody> overlappingRigidbodies = GetOverlappingRigidbodies(specRigidbody, overrideLayerMask, includeTriggers);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			specRigidbody.RegisterGhostCollisionException(overlappingRigidbodies[i]);
			overlappingRigidbodies[i].RegisterGhostCollisionException(specRigidbody);
		}
	}

	public List<SpeculativeRigidbody> GetOverlappingRigidbodies(SpeculativeRigidbody specRigidbody, int? overrideLayerMask = null, bool includeTriggers = false)
	{
		List<SpeculativeRigidbody> list = new List<SpeculativeRigidbody>();
		for (int i = 0; i < specRigidbody.PixelColliders.Count; i++)
		{
			list.AddRange(GetOverlappingRigidbodies(specRigidbody.PixelColliders[i], overrideLayerMask, includeTriggers));
		}
		for (int j = 0; j < list.Count - 1; j++)
		{
			for (int num = list.Count - 1; num > j; num--)
			{
				if (list[j] == list[num])
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public List<SpeculativeRigidbody> GetOverlappingRigidbodies(PixelCollider pixelCollider, int? overrideLayerMask = null, bool includeTriggers = false)
	{
		List<SpeculativeRigidbody> overlappingRigidbodies = new List<SpeculativeRigidbody>();
		Func<SpeculativeRigidbody, bool> callback = delegate(SpeculativeRigidbody rigidbody)
		{
			if (rigidbody.PixelColliders.Contains(pixelCollider))
			{
				return true;
			}
			if (!includeTriggers && pixelCollider.IsTrigger)
			{
				return true;
			}
			for (int i = 0; i < rigidbody.PixelColliders.Count; i++)
			{
				PixelCollider pixelCollider2 = rigidbody.PixelColliders[i];
				if (includeTriggers || !pixelCollider2.IsTrigger)
				{
					if (overrideLayerMask.HasValue)
					{
						int num = CollisionMask.LayerToMask(pixelCollider2.CollisionLayer);
						if ((overrideLayerMask.Value & num) != num)
						{
							continue;
						}
					}
					else if (!pixelCollider.CanCollideWith(pixelCollider2))
					{
						continue;
					}
					if (pixelCollider.AABBOverlaps(pixelCollider2))
					{
						overlappingRigidbodies.Add(rigidbody);
					}
				}
			}
			return true;
		};
		m_rigidbodyTree.Query(GetSafeB2AABB(pixelCollider.Min, pixelCollider.Max), callback);
		if (CollidesWithProjectiles(pixelCollider))
		{
			m_projectileTree.Query(GetSafeB2AABB(pixelCollider.Min, pixelCollider.Max), callback);
		}
		return overlappingRigidbodies;
	}

	public List<ICollidableObject> GetOverlappingCollidableObjects(Vector2 min, Vector2 max, bool collideWithTiles = true, bool collideWithRigidbodies = true, int? layerMask = null, bool includeTriggers = false)
	{
		List<ICollidableObject> overlappingRigidbodies = new List<ICollidableObject>();
		PixelCollider aabbCollider = new PixelCollider();
		aabbCollider.RegenerateFromManual(min, IntVector2.Zero, new IntVector2(Mathf.CeilToInt(16f * (max.x - min.x)), Mathf.CeilToInt(16f * (max.y - min.y))));
		if (collideWithTiles && (bool)TileMap)
		{
			IntVector2 pixel = aabbCollider.Min - IntVector2.One;
			IntVector2 pixel2 = aabbCollider.Max + IntVector2.One;
			InitNearbyTileCheck(PixelToUnit(pixel), PixelToUnit(pixel2), TileMap);
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (Tile nextNearbyTile = GetNextNearbyTile(data); nextNearbyTile != null; nextNearbyTile = GetNextNearbyTile(data))
			{
				for (int i = 0; i < nextNearbyTile.PixelColliders.Count; i++)
				{
					PixelCollider pixelCollider = nextNearbyTile.PixelColliders[i];
					if ((!layerMask.HasValue || pixelCollider.CanCollideWith(layerMask.Value)) && aabbCollider.AABBOverlaps(pixelCollider) && aabbCollider.Overlaps(pixelCollider))
					{
						overlappingRigidbodies.Add(nextNearbyTile);
					}
				}
			}
		}
		if (collideWithRigidbodies)
		{
			Func<SpeculativeRigidbody, bool> callback = delegate(SpeculativeRigidbody rigidbody)
			{
				for (int j = 0; j < rigidbody.PixelColliders.Count; j++)
				{
					PixelCollider pixelCollider2 = rigidbody.PixelColliders[j];
					if (includeTriggers || !pixelCollider2.IsTrigger)
					{
						if (layerMask.HasValue)
						{
							int num2 = CollisionMask.LayerToMask(pixelCollider2.CollisionLayer);
							if ((layerMask.Value & num2) != num2)
							{
								continue;
							}
						}
						if (aabbCollider.AABBOverlaps(pixelCollider2) && aabbCollider.Overlaps(pixelCollider2))
						{
							overlappingRigidbodies.Add(rigidbody);
						}
					}
				}
				return true;
			};
			m_rigidbodyTree.Query(GetSafeB2AABB(aabbCollider.Min, aabbCollider.Max), callback);
			int num = CollisionMask.LayerToMask(CollisionLayer.Projectile);
			if (!layerMask.HasValue || (layerMask.Value & num) == num)
			{
				m_projectileTree.Query(GetSafeB2AABB(aabbCollider.Min, aabbCollider.Max), callback);
			}
		}
		return overlappingRigidbodies;
	}

	public void Register(SpeculativeRigidbody rigidbody)
	{
		if (rigidbody == null)
		{
			return;
		}
		if (rigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.Unknown)
		{
			InferRegistration(rigidbody);
		}
		switch (rigidbody.PhysicsRegistration)
		{
		case SpeculativeRigidbody.RegistrationState.DeregisterScheduled:
			m_deregisterRigidBodies.Remove(rigidbody);
			rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Registered;
			break;
		case SpeculativeRigidbody.RegistrationState.Deregistered:
			m_rigidbodies.Add(rigidbody);
			if (rigidbody.IsSimpleProjectile)
			{
				rigidbody.proxyId = m_projectileTree.CreateProxy(rigidbody.b2AABB, rigidbody);
			}
			else
			{
				rigidbody.proxyId = m_rigidbodyTree.CreateProxy(rigidbody.b2AABB, rigidbody);
			}
			rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Registered;
			break;
		}
	}

	public void Deregister(SpeculativeRigidbody rigidbody)
	{
		if (rigidbody == null)
		{
			return;
		}
		if (rigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.Unknown)
		{
			InferRegistration(rigidbody);
		}
		if (rigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.Deregistered)
		{
			return;
		}
		if (rigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.DeregisterScheduled)
		{
			m_deregisterRigidBodies.Remove(rigidbody);
		}
		m_rigidbodies.Remove(rigidbody);
		if (rigidbody.proxyId >= 0)
		{
			if (rigidbody.IsSimpleProjectile)
			{
				m_projectileTree.DestroyProxy(rigidbody.proxyId);
			}
			else
			{
				m_rigidbodyTree.DestroyProxy(rigidbody.proxyId);
			}
			rigidbody.proxyId = -1;
		}
		rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Deregistered;
	}

	public void DeregisterWhenAvailable(SpeculativeRigidbody rigidbody)
	{
		if (!(rigidbody == null))
		{
			if (rigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.Unknown)
			{
				InferRegistration(rigidbody);
			}
			if (rigidbody.PhysicsRegistration == SpeculativeRigidbody.RegistrationState.Registered)
			{
				m_deregisterRigidBodies.Add(rigidbody);
				rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.DeregisterScheduled;
			}
		}
	}

	private void InferRegistration(SpeculativeRigidbody rigidbody)
	{
		if (m_deregisterRigidBodies.Contains(rigidbody))
		{
			rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.DeregisterScheduled;
		}
		else if (m_rigidbodies.Contains(rigidbody))
		{
			rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Registered;
		}
		else
		{
			rigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Deregistered;
		}
	}

	private void CollideWithRigidbodies(SpeculativeRigidbody rigidbody, List<PixelCollider.StepData> stepList, ref CollisionData nearestCollision)
	{
		if ((bool)rigidbody && rigidbody.enabled)
		{
			b2AABB b2AABB = rigidbody.b2AABB;
			IntVector2 pixelsToMove = rigidbody.PixelsToMove;
			if (pixelsToMove.x < 0)
			{
				b2AABB.lowerBound.x += (float)pixelsToMove.x * 0.0625f - 0.0625f;
				b2AABB.upperBound.x += 0.0625f;
			}
			else
			{
				b2AABB.lowerBound.x -= 0.0625f;
				b2AABB.upperBound.x += (float)pixelsToMove.x * 0.0625f + 0.0625f;
			}
			if (pixelsToMove.y < 0)
			{
				b2AABB.lowerBound.y += (float)pixelsToMove.Y * 0.0625f - 0.0625f;
				b2AABB.upperBound.y += 0.0625f;
			}
			else
			{
				b2AABB.lowerBound.y -= 0.0625f;
				b2AABB.upperBound.y += (float)pixelsToMove.y * 0.0625f + 0.0625f;
			}
			m_cwrqRigidbody = rigidbody;
			m_cwrqStepList = stepList;
			m_cwrqCollisionData = nearestCollision;
			m_rigidbodyTree.Query(b2AABB, CollideWithRigidbodiesQuery);
			if (CollidesWithProjectiles(rigidbody))
			{
				m_projectileTree.Query(b2AABB, CollideWithRigidbodiesQuery);
			}
			nearestCollision = m_cwrqCollisionData;
			m_cwrqRigidbody = null;
			m_cwrqStepList = null;
			m_cwrqCollisionData = null;
		}
	}

	private static bool CollideWithRigidbodiesQuery(SpeculativeRigidbody otherRigidbody)
	{
		for (int i = 0; i < m_cwrqRigidbody.PixelColliders.Count; i++)
		{
			PixelCollider collider = m_cwrqRigidbody.PixelColliders[i];
			for (int j = 0; j < otherRigidbody.PixelColliders.Count; j++)
			{
				PixelCollider otherCollider = otherRigidbody.PixelColliders[j];
				CollisionData obj = SingleCollision(m_cwrqRigidbody, collider, otherRigidbody, otherCollider, m_cwrqStepList, true);
				if (obj != null)
				{
					if (m_cwrqCollisionData == null || obj.TimeUsed < m_cwrqCollisionData.TimeUsed)
					{
						CollisionData.Pool.Free(ref m_cwrqCollisionData);
						m_cwrqCollisionData = obj;
					}
					else
					{
						CollisionData.Pool.Free(ref obj);
					}
				}
			}
		}
		return true;
	}

	private void CollideWithTilemap(SpeculativeRigidbody rigidbody, PixelCollider pixelCollider, List<PixelCollider.StepData> stepList, ref float timeUsed, DungeonData dungeonData, ref CollisionData nearestCollision)
	{
		Position position = rigidbody.m_position;
		IntVector2 intVector = pixelCollider.m_offset + pixelCollider.m_transformOffset;
		float num = (float)position.m_position.x * 0.0625f + position.m_remainder.x + (float)intVector.x * 0.0625f;
		float num2 = (float)position.m_position.y * 0.0625f + position.m_remainder.y + (float)intVector.y * 0.0625f;
		IntVector2 pixelsToMove = rigidbody.PixelsToMove;
		float num3 = num + (float)pixelsToMove.x * 0.0625f;
		float num4 = num2 + (float)pixelsToMove.y * 0.0625f;
		IntVector2 dimensions = pixelCollider.m_dimensions;
		float worldMinX;
		float worldMaxX;
		if (num < num3)
		{
			worldMinX = num - 0.25f;
			worldMaxX = num3 + 0.25f + (float)dimensions.x * 0.0625f;
		}
		else
		{
			worldMinX = num3 - 0.25f;
			worldMaxX = num + 0.25f + (float)dimensions.x * 0.0625f;
		}
		float worldMinY;
		float worldMaxY;
		if (num2 < num4)
		{
			worldMinY = num2 - 0.25f;
			worldMaxY = num4 + 0.25f + (float)dimensions.y * 0.0625f;
		}
		else
		{
			worldMinY = num4 - 0.25f;
			worldMaxY = num2 + 0.25f + (float)dimensions.y * 0.0625f;
		}
		InitNearbyTileCheck(worldMinX, worldMinY, worldMaxX, worldMaxY, TileMap, dimensions, num, num2, pixelsToMove, dungeonData);
		for (Tile nextNearbyTile = GetNextNearbyTile(dungeonData); nextNearbyTile != null; nextNearbyTile = GetNextNearbyTile(dungeonData))
		{
			for (int i = 0; i < nextNearbyTile.PixelColliders.Count; i++)
			{
				CollisionData obj = SingleCollision(rigidbody, pixelCollider, nextNearbyTile, nextNearbyTile.PixelColliders[i], stepList, true);
				if (obj != null)
				{
					if (nearestCollision == null || obj.TimeUsed < nearestCollision.TimeUsed)
					{
						CollisionData.Pool.Free(ref nearestCollision);
						nearestCollision = obj;
					}
					else
					{
						CollisionData.Pool.Free(ref obj);
					}
					m_nbt.Finish(dungeonData);
				}
			}
		}
		CleanupNearbyTileCheck();
	}

	private static CollisionData SingleCollision(SpeculativeRigidbody rigidbody, PixelCollider collider, ICollidableObject otherCollidable, PixelCollider otherCollider, List<PixelCollider.StepData> stepList, bool doPreCollision)
	{
		if (collider == null || otherCollider == null)
		{
			return null;
		}
		if (!collider.AABBOverlaps(otherCollider, rigidbody.PixelsToMove))
		{
			return null;
		}
		if (!otherCollidable.CanCollideWith(rigidbody))
		{
			return null;
		}
		if (!otherCollider.CanCollideWith(collider))
		{
			return null;
		}
		LinearCastResult result = null;
		CollisionData collisionData = null;
		if (otherCollider.DirectionIgnorer != null || !collider.Overlaps(otherCollider))
		{
			if (!collider.LinearCast(otherCollider, rigidbody.PixelsToMove, stepList, out result))
			{
				result = null;
			}
		}
		else if (collider.IsTrigger || otherCollider.IsTrigger)
		{
			result = LinearCastResult.Pool.Allocate();
			result.Contact = rigidbody.UnitCenter;
			result.Normal = Vector2.up;
			result.MyPixelCollider = collider;
			result.OtherPixelCollider = otherCollider;
			result.TimeUsed = 0f;
			result.CollidedX = true;
			result.CollidedY = true;
			result.NewPixelsToMove = IntVector2.Zero;
			result.Overlap = true;
		}
		else
		{
			IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
			int num = 0;
			int num2 = 1;
			int i;
			while (true)
			{
				for (i = 0; i < cardinalsAndOrdinals.Length; i++)
				{
					if (!collider.Overlaps(otherCollider, cardinalsAndOrdinals[i] * num2))
					{
						goto end_IL_00fe;
					}
				}
				num2++;
				num++;
				if (num > 100)
				{
					UnityEngine.Debug.LogError(string.Format("FREEZE AVERTED!  TELL RUBEL!  (you're welcome) [{0}] & [{1}]", rigidbody.name, (!(otherCollidable is SpeculativeRigidbody)) ? "tile" : ((SpeculativeRigidbody)otherCollidable).name));
				}
				continue;
				end_IL_00fe:
				break;
			}
			result = LinearCastResult.Pool.Allocate();
			result.Contact = rigidbody.UnitCenter;
			result.Normal = cardinalsAndOrdinals[i].ToVector2().normalized;
			result.MyPixelCollider = collider;
			result.OtherPixelCollider = otherCollider;
			result.TimeUsed = 0f;
			result.CollidedX = true;
			result.CollidedY = true;
			result.NewPixelsToMove = IntVector2.Zero;
			result.Overlap = true;
		}
		if (result != null)
		{
			if (doPreCollision)
			{
				if (otherCollidable is SpeculativeRigidbody)
				{
					SpeculativeRigidbody speculativeRigidbody = otherCollidable as SpeculativeRigidbody;
					SkipCollision = false;
					PendingCastResult = result;
					if (rigidbody.OnPreRigidbodyCollision != null)
					{
						rigidbody.OnPreRigidbodyCollision(rigidbody, collider, speculativeRigidbody, otherCollider);
					}
					if (speculativeRigidbody.OnPreRigidbodyCollision != null)
					{
						speculativeRigidbody.OnPreRigidbodyCollision(speculativeRigidbody, otherCollider, rigidbody, collider);
					}
					if (SkipCollision)
					{
						LinearCastResult.Pool.Free(ref result);
						return null;
					}
				}
				else if (otherCollidable is Tile)
				{
					Tile tile = otherCollidable as Tile;
					SkipCollision = false;
					PendingCastResult = result;
					if (rigidbody.OnPreTileCollision != null)
					{
						rigidbody.OnPreTileCollision(rigidbody, collider, tile, otherCollider);
					}
					if (SkipCollision)
					{
						LinearCastResult.Pool.Free(ref result);
						return null;
					}
				}
			}
			collisionData = CollisionData.Pool.Allocate();
			collisionData.SetAll(result);
			collisionData.MyRigidbody = rigidbody;
			if (otherCollidable is SpeculativeRigidbody)
			{
				collisionData.collisionType = CollisionData.CollisionType.Rigidbody;
				collisionData.OtherRigidbody = (SpeculativeRigidbody)otherCollidable;
			}
			else if (otherCollidable is Tile)
			{
				collisionData.collisionType = CollisionData.CollisionType.TileMap;
				collisionData.TileLayerName = ((Tile)otherCollidable).LayerName;
				collisionData.TilePosition = ((Tile)otherCollidable).Position;
			}
			LinearCastResult.Pool.Free(ref result);
		}
		return collisionData;
	}

	private bool CollidesWithProjectiles(int mask, CollisionLayer? sourceLayer)
	{
		if ((mask & m_cachedProjectileMask) != m_cachedProjectileMask)
		{
			return false;
		}
		if (sourceLayer.HasValue)
		{
			return (CollisionLayerMatrix.GetMask(sourceLayer.Value) & m_cachedProjectileMask) == m_cachedProjectileMask;
		}
		return true;
	}

	private bool CollidesWithProjectiles(SpeculativeRigidbody specRigidbody)
	{
		List<PixelCollider> pixelColliders = specRigidbody.PixelColliders;
		for (int i = 0; i < pixelColliders.Count; i++)
		{
			PixelCollider pixelCollider = pixelColliders[i];
			if ((CollisionLayerMatrix.GetMask(pixelCollider.CollisionLayer) & m_cachedProjectileMask) == m_cachedProjectileMask)
			{
				return true;
			}
			if ((pixelCollider.CollisionLayerCollidableOverride & m_cachedProjectileMask) == m_cachedProjectileMask)
			{
				return true;
			}
		}
		return false;
	}

	private bool CollidesWithProjectiles(PixelCollider pixelCollider)
	{
		return (CollisionLayerMatrix.GetMask(pixelCollider.CollisionLayer) & m_cachedProjectileMask) == m_cachedProjectileMask;
	}

	public void ClearAllCachedTiles()
	{
		Dungeon dungeon = GameManager.Instance.Dungeon;
		DungeonData data = dungeon.data;
		for (int i = 0; i < dungeon.Width; i++)
		{
			for (int j = 0; j < dungeon.Height; j++)
			{
				CellData cellData = data[i, j];
				if (cellData != null)
				{
					cellData.HasCachedPhysicsTile = false;
					cellData.CachedPhysicsTile = null;
				}
			}
		}
	}

	private Tile GetTile(int x, int y, tk2dTileMap tileMap, int layer, string layerName, DungeonData dungeonData)
	{
		CellData cellData;
		if (x < 0 || x >= m_cachedDungeonWidth || y < 0 || y >= m_cachedDungeonHeight || (cellData = dungeonData.cellData[x][y]) == null)
		{
			return null;
		}
		if (cellData.HasCachedPhysicsTile)
		{
			return cellData.CachedPhysicsTile;
		}
		if (cellData.type == CellType.WALL && GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(cellData.position + IntVector2.Up))
		{
			CellData cellData2 = GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Up];
			if (cellData2 != null && cellData2.isOccludedByTopWall && (cellData2.diagonalWallType == DiagonalWallType.SOUTHEAST || cellData2.diagonalWallType == DiagonalWallType.SOUTHWEST))
			{
				Tile tile = GetTile(x, y + 1, tileMap, layer, layerName, dungeonData);
				cellData2.HasCachedPhysicsTile = true;
				cellData2.CachedPhysicsTile = tile;
				return tile;
			}
		}
		int tile2 = GetTile(layer, cellData.positionInTilemap.x, cellData.positionInTilemap.y);
		List<PixelCollider> list = new List<PixelCollider>();
		Vector2 vector = Vector2.Scale(new Vector2(x, y), tileMap.data.tileSize.XY());
		IntVector2 pixelPosition = new Position((Vector2)tileMap.transform.position + vector).PixelPosition;
		if (tile2 >= 0)
		{
			tk2dSpriteDefinition tk2dSpriteDefinition2 = tileMap.SpriteCollectionInst.spriteDefinitions[tile2];
			if (tk2dSpriteDefinition2.IsTileSquare)
			{
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = tk2dSpriteDefinition2.collisionLayer;
				PixelCollider pixelCollider2 = pixelCollider;
				pixelCollider2.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(16, 16));
				pixelCollider2.Position = pixelPosition;
				list.Add(pixelCollider2);
			}
			else
			{
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = tk2dSpriteDefinition2.collisionLayer;
				PixelCollider pixelCollider3 = pixelCollider;
				pixelCollider3.RegenerateFrom3dCollider(tk2dSpriteDefinition2.colliderVertices, tileMap.transform);
				pixelCollider3.Position = pixelPosition;
				list.Add(pixelCollider3);
			}
		}
		else if (cellData.cellVisualData.precludeAllTileDrawing && cellData.type == CellType.WALL)
		{
			PixelCollider pixelCollider = new PixelCollider();
			pixelCollider.IsTileCollider = true;
			pixelCollider.CollisionLayer = CollisionLayer.HighObstacle;
			PixelCollider pixelCollider4 = pixelCollider;
			pixelCollider4.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(16, 16));
			pixelCollider4.Position = pixelPosition;
			list.Add(pixelCollider4);
		}
		if (cellData.isOccludedByTopWall && !GameManager.Instance.IsFoyer)
		{
			if (cellData.diagonalWallType == DiagonalWallType.SOUTHEAST || cellData.diagonalWallType == DiagonalWallType.SOUTHWEST)
			{
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = CollisionLayer.EnemyBlocker;
				PixelCollider pixelCollider5 = pixelCollider;
				pixelCollider5.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(16, 28));
				pixelCollider5.Position = pixelPosition + new IntVector2(0, -16);
				list.Add(pixelCollider5);
				if (cellData.diagonalWallType == DiagonalWallType.SOUTHEAST)
				{
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
					PixelCollider pixelCollider6 = pixelCollider;
					int num = 14;
					pixelCollider6.RegenerateFromLine(tileMap.transform, new IntVector2(1, num - 16), new IntVector2(16, num - 1));
					pixelCollider6.Position = pixelPosition + new IntVector2(0, num - 16);
					list.Add(pixelCollider6);
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
					pixelCollider6 = pixelCollider;
					pixelCollider6.RegenerateFromManual(tileMap.transform, new IntVector2(1, num - 16), new IntVector2(16, num));
					pixelCollider6.Position = pixelPosition + new IntVector2(0, -16);
					list.Add(pixelCollider6);
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.HighObstacle;
					PixelCollider pixelCollider7 = pixelCollider;
					num = 8;
					pixelCollider7.RegenerateFromLine(tileMap.transform, new IntVector2(1, num - 16), new IntVector2(16, num - 1));
					pixelCollider7.Position = pixelPosition + new IntVector2(0, num - 16);
					list.Add(pixelCollider7);
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.HighObstacle;
					pixelCollider7 = pixelCollider;
					pixelCollider7.RegenerateFromManual(tileMap.transform, new IntVector2(1, num - 16), new IntVector2(16, num));
					pixelCollider7.Position = pixelPosition + new IntVector2(0, -16);
					list.Add(pixelCollider7);
				}
				else if (cellData.diagonalWallType == DiagonalWallType.SOUTHWEST)
				{
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
					PixelCollider pixelCollider8 = pixelCollider;
					int num2 = 14;
					pixelCollider8.RegenerateFromLine(tileMap.transform, new IntVector2(0, num2 - 1), new IntVector2(15, num2 - 16));
					pixelCollider8.Position = pixelPosition + new IntVector2(0, num2 - 16);
					list.Add(pixelCollider8);
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
					pixelCollider8 = pixelCollider;
					pixelCollider8.RegenerateFromManual(tileMap.transform, new IntVector2(1, num2 - 16), new IntVector2(16, num2));
					pixelCollider8.Position = pixelPosition + new IntVector2(0, -16);
					list.Add(pixelCollider8);
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.HighObstacle;
					PixelCollider pixelCollider9 = pixelCollider;
					num2 = 8;
					pixelCollider9.RegenerateFromLine(tileMap.transform, new IntVector2(0, num2 - 1), new IntVector2(15, num2 - 16));
					pixelCollider9.Position = pixelPosition + new IntVector2(0, num2 - 16);
					list.Add(pixelCollider9);
					pixelCollider = new PixelCollider();
					pixelCollider.IsTileCollider = true;
					pixelCollider.CollisionLayer = CollisionLayer.HighObstacle;
					pixelCollider9 = pixelCollider;
					pixelCollider9.RegenerateFromManual(tileMap.transform, new IntVector2(1, num2 - 16), new IntVector2(16, num2));
					pixelCollider9.Position = pixelPosition + new IntVector2(0, -16);
					list.Add(pixelCollider9);
				}
			}
			else
			{
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = CollisionLayer.EnemyBlocker;
				PixelCollider pixelCollider10 = pixelCollider;
				pixelCollider10.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(16, 12));
				pixelCollider10.Position = pixelPosition;
				list.Add(pixelCollider10);
				pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
				PixelCollider pixelCollider11 = pixelCollider;
				pixelCollider11.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(16, 14));
				pixelCollider11.Position = pixelPosition;
				list.Add(pixelCollider11);
				pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = CollisionLayer.PlayerBlocker;
				PixelCollider pixelCollider12 = pixelCollider;
				pixelCollider12.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(16, 8));
				pixelCollider12.Position = pixelPosition;
				list.Add(pixelCollider12);
			}
		}
		if (cellData.IsLowerFaceWall() && !GameManager.Instance.IsFoyer && cellData.diagonalWallType != DiagonalWallType.SOUTHEAST && cellData.diagonalWallType == DiagonalWallType.SOUTHWEST)
		{
			if (!GameManager.Instance.Dungeon.data.isWall(cellData.position.x - 1, cellData.position.y))
			{
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = CollisionLayer.BulletBlocker;
				PixelCollider pixelCollider13 = pixelCollider;
				pixelCollider13.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(3, 10));
				pixelCollider13.Position = pixelPosition + new IntVector2(0, 6);
				pixelCollider13.DirectionIgnorer = (IntVector2 dir) => dir.x >= 0 || dir.y <= 0;
				pixelCollider13.NormalModifier = (Vector2 normal) => (!(normal.x > 0f)) ? normal : Vector2.down;
				list.Add(pixelCollider13);
			}
			if (!GameManager.Instance.Dungeon.data.isWall(cellData.position.x + 1, cellData.position.y))
			{
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.IsTileCollider = true;
				pixelCollider.CollisionLayer = CollisionLayer.BulletBlocker;
				PixelCollider pixelCollider14 = pixelCollider;
				pixelCollider14.RegenerateFromManual(tileMap.transform, new IntVector2(0, 0), new IntVector2(3, 10));
				pixelCollider14.Position = pixelPosition + new IntVector2(13, 6);
				pixelCollider14.DirectionIgnorer = (IntVector2 dir) => dir.x <= 0 || dir.y <= 0;
				pixelCollider14.NormalModifier = (Vector2 normal) => (!(normal.x < 0f)) ? normal : Vector2.down;
				list.Add(pixelCollider14);
			}
		}
		if (list.Count == 0)
		{
			cellData.HasCachedPhysicsTile = true;
			cellData.CachedPhysicsTile = null;
			return null;
		}
		Tile tile3 = new Tile(list, x, y, layerName);
		cellData.HasCachedPhysicsTile = true;
		cellData.CachedPhysicsTile = tile3;
		return tile3;
	}

	private void InitNearbyTileCheck(Vector2 worldMin, Vector2 worldMax, tk2dTileMap tileMap)
	{
		if (m_nbt.tileMap == null)
		{
			m_nbt.tileMap = tileMap;
			m_nbt.layerName = "Collision Layer";
			m_nbt.layer = BraveUtility.GetTileMapLayerByName("Collision Layer", TileMap);
		}
		m_nbt.Init(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
	}

	private void InitNearbyTileCheck(float worldMinX, float worldMinY, float worldMaxX, float worldMaxY, tk2dTileMap tileMap, IntVector2 pixelColliderDimensions, float positionX, float positionY, IntVector2 pixelsToMove, DungeonData dungeonData)
	{
		if (m_nbt.tileMap == null)
		{
			m_nbt.tileMap = tileMap;
			m_nbt.layerName = "Collision Layer";
			m_nbt.layer = BraveUtility.GetTileMapLayerByName("Collision Layer", TileMap);
		}
		m_nbt.Init(worldMinX, worldMinY, worldMaxX, worldMaxY, pixelColliderDimensions, positionX, positionY, pixelsToMove, dungeonData);
	}

	private Tile GetNextNearbyTile(DungeonData dungeonData)
	{
		return m_nbt.GetNextNearbyTile(dungeonData);
	}

	private void CleanupNearbyTileCheck()
	{
		m_nbt.Cleanup();
	}

	private int GetTile(int layer, int x, int y)
	{
		if (x >= 0 && x < TileMap.width && y >= 0 && y < TileMap.height)
		{
			return TileMap.Layers[layer].GetTile(x, y);
		}
		return -1;
	}

	public static void PixelMovementGenerator(IntVector2 pixelsToMove, List<PixelCollider.StepData> steps)
	{
		steps.Clear();
		IntVector2 zero = IntVector2.Zero;
		float deltaTime = 1f / (float)(Mathf.Abs(pixelsToMove.x) + Mathf.Abs(pixelsToMove.y));
		while (zero.x != pixelsToMove.x || zero.y != pixelsToMove.y)
		{
			IntVector2 intVector;
			if (zero.x == pixelsToMove.x)
			{
				intVector = new IntVector2(0, Math.Sign(pixelsToMove.y));
			}
			else if (zero.y == pixelsToMove.y)
			{
				intVector = new IntVector2(Math.Sign(pixelsToMove.x), 0);
			}
			else
			{
				float num = Mathf.Abs((float)zero.x / (float)pixelsToMove.x);
				float num2 = Mathf.Abs((float)zero.y / (float)pixelsToMove.y);
				intVector = ((!(num < num2)) ? new IntVector2(0, Math.Sign(pixelsToMove.y)) : new IntVector2(Math.Sign(pixelsToMove.x), 0));
			}
			zero += intVector;
			steps.Add(new PixelCollider.StepData
			{
				deltaPos = intVector,
				deltaTime = deltaTime
			});
		}
	}

	public static void PixelMovementGenerator(SpeculativeRigidbody rigidbody, List<PixelCollider.StepData> stepList)
	{
		PixelMovementGenerator(rigidbody.m_position.m_remainder, rigidbody.Velocity, rigidbody.PixelsToMove, stepList);
	}

	private static void PixelMovementGenerator(Vector2 remainder, Vector2 velocity, IntVector2 pixelsToMove, List<PixelCollider.StepData> stepList)
	{
		stepList.Clear();
		float num = 1f / 32f;
		IntVector2 intVector = default(IntVector2);
		intVector.x = 0;
		intVector.y = 0;
		int num2 = Math.Sign(pixelsToMove.x);
		int num3 = Math.Sign(pixelsToMove.y);
		if (pixelsToMove.y == 0)
		{
			while (intVector.x != pixelsToMove.x)
			{
				float num4 = Mathf.Max(0f, ((float)num2 * num - remainder.x) / velocity.x);
				intVector.x += num2;
				remainder.x = (float)num2 * (0f - num);
				remainder.y += num4 * velocity.y;
				stepList.Add(new PixelCollider.StepData(new IntVector2(num2, 0), num4));
			}
			return;
		}
		if (pixelsToMove.x == 0)
		{
			while (intVector.y != pixelsToMove.y)
			{
				float num5 = Mathf.Max(0f, ((float)num3 * num - remainder.y) / velocity.y);
				intVector.y += num3;
				remainder.x += num5 * velocity.x;
				remainder.y = (float)num3 * (0f - num);
				stepList.Add(new PixelCollider.StepData(new IntVector2(0, num3), num5));
			}
			return;
		}
		while (intVector.x != pixelsToMove.x || intVector.y != pixelsToMove.y)
		{
			float num6 = Mathf.Max(0f, ((float)num2 * num - remainder.x) / velocity.x);
			float num7 = Mathf.Max(0f, ((float)num3 * num - remainder.y) / velocity.y);
			if (intVector.x != pixelsToMove.x && (intVector.y == pixelsToMove.y || num6 < num7))
			{
				intVector.x += num2;
				remainder.x = (float)num2 * (0f - num);
				remainder.y += num6 * velocity.y;
				stepList.Add(new PixelCollider.StepData(new IntVector2(num2, 0), num6));
			}
			else
			{
				intVector.y += num3;
				remainder.x += num7 * velocity.x;
				remainder.y = (float)num3 * (0f - num);
				stepList.Add(new PixelCollider.StepData(new IntVector2(0, num3), num7));
			}
		}
	}

	private void SortRigidbodies()
	{
		bool flag = false;
		for (int i = 0; i < m_rigidbodies.Count; i++)
		{
			SpeculativeRigidbody speculativeRigidbody = m_rigidbodies[i];
			int num = (speculativeRigidbody.CanBePushed ? 1 : 0) << (speculativeRigidbody.CanPush ? 1 : 0) << 1 + (speculativeRigidbody.CanCarry ? 1 : 0) << 2;
			if (num != speculativeRigidbody.SortHash)
			{
				flag = true;
			}
			speculativeRigidbody.SortHash = num;
		}
		if (!flag)
		{
			return;
		}
		m_rigidbodies.Sort(delegate(SpeculativeRigidbody lhs, SpeculativeRigidbody rhs)
		{
			if (lhs.CanCarry && !rhs.CanCarry)
			{
				return -1;
			}
			if (!lhs.CanCarry && rhs.CanCarry)
			{
				return 1;
			}
			if (lhs.CanPush && !rhs.CanPush)
			{
				return -1;
			}
			if (!lhs.CanPush && rhs.CanPush)
			{
				return 1;
			}
			if (lhs.CanPush && rhs.CanPush)
			{
				if (!lhs.CanBePushed && rhs.CanBePushed)
				{
					return -1;
				}
				if (lhs.CanBePushed && !rhs.CanBePushed)
				{
					return 1;
				}
			}
			return 0;
		});
	}

	public static void UpdatePosition(SpeculativeRigidbody specRigidbody)
	{
		Vector2 displacement = specRigidbody.Velocity * UnityEngine.Random.Range(0.8f, 1.2f);
		if (specRigidbody.IsSimpleProjectile)
		{
			Instance.m_projectileTree.MoveProxy(specRigidbody.proxyId, specRigidbody.b2AABB, displacement);
		}
		else
		{
			Instance.m_rigidbodyTree.MoveProxy(specRigidbody.proxyId, specRigidbody.b2AABB, displacement);
		}
	}

	public static float PixelToUnit(int pixel)
	{
		return (float)pixel / 16f;
	}

	public static Vector2 PixelToUnit(IntVector2 pixel)
	{
		return (Vector2)pixel / 16f;
	}

	public static float PixelToUnitMidpoint(int pixel)
	{
		return (float)pixel / 16f + Instance.HalfPixelUnitWidth;
	}

	public static Vector2 PixelToUnitMidpoint(IntVector2 pixel)
	{
		return (Vector2)pixel / 16f + new Vector2(Instance.HalfPixelUnitWidth, Instance.HalfPixelUnitWidth);
	}

	public static int UnitToPixel(float unit)
	{
		return (int)(unit * 16f);
	}

	public static IntVector2 UnitToPixel(Vector2 unit)
	{
		return new IntVector2(UnitToPixel(unit.x), UnitToPixel(unit.y));
	}

	public static int UnitRoundToPixel(float unit)
	{
		return Mathf.RoundToInt(unit * 16f);
	}

	public static IntVector2 UnitRoundToPixel(Vector2 unit)
	{
		return new IntVector2(UnitRoundToPixel(unit.x), UnitRoundToPixel(unit.y));
	}

	private static Vector2 GetSlopeScalar(float slope)
	{
		return new Vector2(1f, slope).normalized;
	}

	private static Vector2 RotateXTowardSlope(Vector2 v, float slope)
	{
		return GetSlopeScalar(slope) * v.x + new Vector2(0f, v.y);
	}
}
