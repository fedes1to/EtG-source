using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PixelCollider
{
	public enum PixelColliderGeneration
	{
		Manual,
		Tk2dPolygon,
		BagelCollider,
		Circle,
		Line
	}

	private class PixelCache
	{
		public IntVector2 dimensions;

		public BitArray2D basePixels;

		public IntVector2 offset;
	}

	public struct StepData
	{
		public IntVector2 deltaPos;

		public float deltaTime;

		public StepData(IntVector2 deltaPos, float deltaTime)
		{
			this.deltaPos = deltaPos;
			this.deltaTime = deltaTime;
		}
	}

	public bool Enabled = true;

	public CollisionLayer CollisionLayer = CollisionLayer.LowObstacle;

	public bool IsTrigger;

	public PixelColliderGeneration ColliderGenerationMode = PixelColliderGeneration.Tk2dPolygon;

	public bool BagleUseFirstFrameOnly = true;

	[CheckSprite("Sprite")]
	public string SpecifyBagelFrame;

	public int BagelColliderNumber;

	public int ManualOffsetX;

	public int ManualOffsetY;

	public int ManualWidth;

	public int ManualHeight;

	[Obsolete("ManualRadius is deprecated, use ManualDiameter instead.")]
	public int ManualRadius;

	public int ManualDiameter;

	public int ManualLeftX;

	public int ManualLeftY;

	public int ManualRightX;

	public int ManualRightY;

	public tk2dBaseSprite Sprite;

	public Func<IntVector2, bool> DirectionIgnorer;

	public Func<Vector2, Vector2> NormalModifier;

	public IntVector2 m_offset;

	public IntVector2 m_transformOffset;

	private IntVector2 m_position;

	public IntVector2 m_dimensions;

	public float m_rotation;

	private Vector2 m_scale = new Vector2(1f, 1f);

	private BitArray2D m_basePixels = new BitArray2D();

	private BitArray2D m_modifiedPixels;

	private BitArray2D m_bestPixels;

	private Vector2? m_slopeStart;

	private Vector2? m_slopeEnd;

	[NonSerialized]
	public tk2dSpriteDefinition m_lastSpriteDef;

	[NonSerialized]
	private List<PixelCollider> m_frameSpecificCollisionExceptions = new List<PixelCollider>();

	[NonSerialized]
	private List<TriggerCollisionData> m_triggerCollisions = new List<TriggerCollisionData>();

	private Dictionary<int, PixelCache> m_cachedBasePixels;

	public static List<StepData> m_stepList = new List<StepData>();

	public Vector2 UnitTopLeft
	{
		get
		{
			return new Vector2(Min.x, Max.y) / 16f;
		}
	}

	public Vector2 UnitTopRight
	{
		get
		{
			return Max.ToVector2() / 16f;
		}
	}

	public Vector2 UnitTopCenter
	{
		get
		{
			return (Min.ToVector2() + new Vector2((float)Width / 2f, Height)) / 16f;
		}
	}

	public Vector2 UnitCenterLeft
	{
		get
		{
			return (Min.ToVector2() + new Vector2(0f, (float)Height / 2f)) / 16f;
		}
	}

	public Vector2 UnitCenter
	{
		get
		{
			return (Min.ToVector2() + new Vector2((float)Width / 2f, (float)Height / 2f)) / 16f;
		}
	}

	public Vector2 UnitCenterRight
	{
		get
		{
			return (Min.ToVector2() + new Vector2(Width, (float)Height / 2f)) / 16f;
		}
	}

	public Vector2 UnitBottomLeft
	{
		get
		{
			return Min.ToVector2() / 16f;
		}
	}

	public Vector2 UnitBottomCenter
	{
		get
		{
			return (Min.ToVector2() + new Vector2((float)Width / 2f, 0f)) / 16f;
		}
	}

	public Vector2 UnitBottomRight
	{
		get
		{
			return new Vector2(Max.x, Min.y) / 16f;
		}
	}

	public Vector2 UnitDimensions
	{
		get
		{
			return Dimensions.ToVector2() / 16f;
		}
	}

	public float UnitLeft
	{
		get
		{
			return (float)MinX / 16f;
		}
	}

	public float UnitRight
	{
		get
		{
			return (float)(MaxX + 1) / 16f;
		}
	}

	public float UnitBottom
	{
		get
		{
			return (float)MinY / 16f;
		}
	}

	public float UnitTop
	{
		get
		{
			return (float)(MaxY + 1) / 16f;
		}
	}

	public float UnitWidth
	{
		get
		{
			return (float)Dimensions.x / 16f;
		}
	}

	public float UnitHeight
	{
		get
		{
			return (float)Dimensions.y / 16f;
		}
	}

	public IntVector2 Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	public IntVector2 Dimensions
	{
		get
		{
			return m_dimensions;
		}
		set
		{
			m_dimensions = value;
		}
	}

	public IntVector2 Offset
	{
		get
		{
			return m_offset + m_transformOffset;
		}
	}

	public Vector2 UnitOffset
	{
		get
		{
			return PhysicsEngine.PixelToUnit(Offset);
		}
	}

	public IntVector2 Min
	{
		get
		{
			return m_position;
		}
	}

	public IntVector2 Max
	{
		get
		{
			return m_position + m_dimensions - IntVector2.One;
		}
	}

	public int MinX
	{
		get
		{
			return m_position.x;
		}
	}

	public int MaxX
	{
		get
		{
			return m_position.x + m_dimensions.x - 1;
		}
	}

	public int MinY
	{
		get
		{
			return m_position.y;
		}
	}

	public int MaxY
	{
		get
		{
			return m_position.y + m_dimensions.y - 1;
		}
	}

	public IntVector2 LowerLeft
	{
		get
		{
			return m_position;
		}
	}

	public IntVector2 LowerRight
	{
		get
		{
			return new IntVector2(m_position.x + m_dimensions.x - 1, m_position.y);
		}
	}

	public IntVector2 UpperLeft
	{
		get
		{
			return new IntVector2(m_position.x, m_position.y + m_dimensions.y - 1);
		}
	}

	public IntVector2 UpperRight
	{
		get
		{
			return m_position + m_dimensions - IntVector2.One;
		}
	}

	public int X
	{
		get
		{
			return m_position.x;
		}
	}

	public int Y
	{
		get
		{
			return m_position.y;
		}
	}

	public int Width
	{
		get
		{
			return m_dimensions.x;
		}
	}

	public int Height
	{
		get
		{
			return m_dimensions.y;
		}
	}

	public bool IsSlope { get; set; }

	public float Slope { get; set; }

	public IntVector2 UpslopeDirection { get; set; }

	public Vector2 SlopeStart
	{
		get
		{
			return m_slopeStart.Value;
		}
	}

	public Vector2 SlopeEnd
	{
		get
		{
			return m_slopeEnd.Value;
		}
	}

	public bool IsTileCollider { get; set; }

	public float Rotation
	{
		get
		{
			return m_rotation;
		}
		set
		{
			SetRotationAndScale(value, m_scale);
		}
	}

	public Vector2 Scale
	{
		get
		{
			return m_scale;
		}
		set
		{
			SetRotationAndScale(m_rotation, value);
		}
	}

	public int CollisionLayerCollidableOverride { get; set; }

	public int CollisionLayerIgnoreOverride { get; set; }

	public List<TriggerCollisionData> TriggerCollisions
	{
		get
		{
			return m_triggerCollisions;
		}
	}

	public bool this[int x, int y]
	{
		get
		{
			return m_bestPixels[x, y];
		}
	}

	public bool this[IntVector2 pos]
	{
		get
		{
			return m_bestPixels[pos.x, pos.y];
		}
	}

	public bool AABBOverlaps(PixelCollider otherCollider)
	{
		return IntVector2.AABBOverlap(m_position, m_dimensions, otherCollider.m_position, otherCollider.m_dimensions);
	}

	public bool AABBOverlaps(PixelCollider otherCollider, IntVector2 pixelsToMove)
	{
		int num = Mathf.Min(m_position.x, m_position.x + pixelsToMove.x);
		int num2 = m_position.x + m_dimensions.x - 1;
		int num3 = Mathf.Max(num2, num2 + pixelsToMove.x) - num + 1;
		if (num + num3 - 1 < otherCollider.m_position.x)
		{
			return false;
		}
		if (num > otherCollider.m_position.x + otherCollider.m_dimensions.x - 1)
		{
			return false;
		}
		int num4 = m_position.y + m_dimensions.y - 1;
		int num5 = Mathf.Min(m_position.y, m_position.y + pixelsToMove.y);
		int num6 = Mathf.Max(num4, num4 + pixelsToMove.y) - num5 + 1;
		if (num5 + num6 - 1 < otherCollider.m_position.y)
		{
			return false;
		}
		if (num5 > otherCollider.m_position.y + otherCollider.m_dimensions.y - 1)
		{
			return false;
		}
		return true;
	}

	public bool AABBOverlaps(IntVector2 pos, IntVector2 dimensions)
	{
		return IntVector2.AABBOverlap(m_position, m_dimensions, pos, dimensions);
	}

	public bool Overlaps(PixelCollider otherCollider)
	{
		return Overlaps(otherCollider, IntVector2.Zero);
	}

	public bool Overlaps(PixelCollider otherCollider, IntVector2 otherColliderOffset)
	{
		IntVector2 intVector = otherCollider.m_position - m_position + otherColliderOffset;
		int num = Math.Max(0, intVector.x);
		int num2 = Math.Max(0, intVector.y);
		int num3 = Math.Min(m_bestPixels.Width - 1, otherCollider.m_bestPixels.Width - 1 + intVector.x);
		int num4 = Math.Min(m_bestPixels.Height - 1, otherCollider.m_bestPixels.Height - 1 + intVector.y);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				if (m_bestPixels[i, j] && otherCollider.m_bestPixels[i - intVector.x, j - intVector.y])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanCollideWith(int mask, CollisionLayer? sourceLayer = null)
	{
		if (!Enabled)
		{
			return false;
		}
		if ((mask & CollisionLayerCollidableOverride) > 0)
		{
			return true;
		}
		if (sourceLayer.HasValue)
		{
			int num = CollisionMask.LayerToMask(sourceLayer.Value);
			if ((num & CollisionLayerCollidableOverride) == num)
			{
				return true;
			}
			if (IsTileCollider && sourceLayer.Value == CollisionLayer.TileBlocker)
			{
				return true;
			}
		}
		int num2 = CollisionMask.LayerToMask(CollisionLayer);
		return (mask & num2) == num2;
	}

	public bool CanCollideWith(PixelCollider otherCollider, bool ignoreFrameSpecificExceptions = false)
	{
		if (!Enabled || !otherCollider.Enabled)
		{
			return false;
		}
		if (IsTileCollider && otherCollider.CollisionLayer == CollisionLayer.TileBlocker)
		{
			return true;
		}
		int num = CollisionMask.LayerToMask(CollisionLayer);
		int num2 = CollisionMask.LayerToMask(otherCollider.CollisionLayer);
		if ((num & otherCollider.CollisionLayerCollidableOverride) != num && (num2 & CollisionLayerCollidableOverride) != num2)
		{
			int mask = CollisionLayerMatrix.GetMask(otherCollider.CollisionLayer);
			mask &= ~otherCollider.CollisionLayerIgnoreOverride;
			if ((mask & num) != num)
			{
				return false;
			}
			int mask2 = CollisionLayerMatrix.GetMask(CollisionLayer);
			mask2 &= ~CollisionLayerIgnoreOverride;
			if ((mask2 & num2) != num2)
			{
				return false;
			}
		}
		if (!ignoreFrameSpecificExceptions)
		{
			if (m_frameSpecificCollisionExceptions.Count > 0 && m_frameSpecificCollisionExceptions.Contains(otherCollider))
			{
				return false;
			}
			if (otherCollider.m_frameSpecificCollisionExceptions.Count > 0 && otherCollider.m_frameSpecificCollisionExceptions.Contains(this))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanCollideWith(CollisionLayer collisionLayer)
	{
		if (!Enabled)
		{
			return false;
		}
		if (IsTileCollider && collisionLayer == CollisionLayer.TileBlocker)
		{
			return true;
		}
		int num = CollisionMask.LayerToMask(collisionLayer);
		int mask = CollisionLayerMatrix.GetMask(CollisionLayer);
		return (mask & num) == num;
	}

	public bool Raycast(Vector2 origin, Vector2 direction, float distance, out RaycastResult result)
	{
		result = null;
		if (!Enabled)
		{
			return false;
		}
		direction.Normalize();
		IntVector2 posA = PhysicsEngine.UnitToPixel(origin);
		IntVector2 dimensionsA = PhysicsEngine.UnitToPixel(direction * distance);
		dimensionsA += new IntVector2((int)Mathf.Sign(direction.x), (int)Mathf.Sign(direction.y));
		if (dimensionsA.x < 0)
		{
			dimensionsA.x *= -1;
			posA.x -= dimensionsA.x;
		}
		if (dimensionsA.y < 0)
		{
			dimensionsA.y *= -1;
			posA.y -= dimensionsA.y;
		}
		if (!IntVector2.AABBOverlap(posA, dimensionsA, m_position, m_dimensions))
		{
			return false;
		}
		Vector2 l = origin + distance * direction;
		Vector2 vector = m_position.ToVector2() / 16f;
		Vector2 vector2 = m_dimensions.ToVector2() / 16f;
		bool flag = origin.IsWithin(vector, vector + vector2);
		Vector2 intersection;
		if (!BraveUtility.LineIntersectsAABB(origin, l, vector, vector2, out intersection) && !flag)
		{
			return false;
		}
		if (DirectionIgnorer != null && DirectionIgnorer(PhysicsEngine.UnitToPixel(direction * distance)))
		{
			return false;
		}
		IntVector2 intVector = IntVector2.NegOne;
		IntVector2 negOne = IntVector2.NegOne;
		float num = 0f;
		Vector2 vector3;
		if (flag)
		{
			vector3 = origin * 16f;
			intVector = new IntVector2((int)vector3.x, (int)vector3.y);
			negOne = IntVector2.Zero;
		}
		else
		{
			float num2 = Mathf.Abs(intersection.x - PhysicsEngine.PixelToUnit(Min.x));
			float num3 = Mathf.Abs(intersection.x - PhysicsEngine.PixelToUnit(Max.x + 1));
			float num4 = Mathf.Abs(intersection.y - PhysicsEngine.PixelToUnit(Min.y));
			float num5 = Mathf.Abs(intersection.y - PhysicsEngine.PixelToUnit(Max.y + 1));
			if (num2 <= num3 && num2 <= num5 && num2 <= num4 && direction.x > 0f)
			{
				intVector = new IntVector2(Min.X - 1, PhysicsEngine.UnitToPixel(intersection.y));
				negOne = IntVector2.Right;
				vector3 = new Vector2(Min.X, intersection.y * 16f);
			}
			else if (num3 <= num2 && num3 <= num5 && num3 <= num4 && direction.x < 0f)
			{
				intVector = new IntVector2(Max.X + 1, PhysicsEngine.UnitToPixel(intersection.y));
				negOne = IntVector2.Left;
				vector3 = new Vector2(Max.X + 1, intersection.y * 16f);
			}
			else if (num4 <= num3 && num4 <= num5 && num4 <= num2 && direction.y > 0f)
			{
				intVector = new IntVector2(PhysicsEngine.UnitToPixel(intersection.x), Min.Y - 1);
				negOne = IntVector2.Up;
				vector3 = new Vector2(intersection.x * 16f, Min.y);
			}
			else
			{
				if (!(num5 <= num3) || !(num5 <= num2) || !(num5 <= num4) || !(direction.y < 0f))
				{
					return false;
				}
				intVector = new IntVector2(PhysicsEngine.UnitToPixel(intersection.x), Max.y + 1);
				negOne = IntVector2.Down;
				vector3 = new Vector2(intersection.x * 16f, Max.y + 1);
			}
			num = Vector2.Distance(origin, intersection);
		}
		bool flag2 = false;
		int num6 = Math.Sign(direction.x);
		int num7 = Math.Sign(direction.y);
		while ((!flag2 || AABBContainsPixel(intVector)) && num < distance)
		{
			IntVector2 intVector2 = intVector + negOne;
			if (AABBContainsPixel(intVector2))
			{
				flag2 = true;
				if (this[intVector2 - Position])
				{
					result = RaycastResult.Pool.Allocate();
					result.Contact = vector3 / 16f;
					result.HitPixel = intVector2;
					result.LastRayPixel = intVector;
					result.Distance = num;
					result.Normal = ((Vector2)(-negOne)).normalized;
					if (NormalModifier != null)
					{
						result.Normal = NormalModifier(result.Normal);
					}
					result.OtherPixelCollider = this;
					return true;
				}
			}
			intVector = intVector2;
			float num8 = ((direction.x == 0f) ? float.PositiveInfinity : ((float)(intVector.x + num6) - vector3.x));
			float num9 = ((direction.y == 0f) ? float.PositiveInfinity : ((float)(intVector.y + num7) - vector3.y));
			if (num6 < 0)
			{
				num8 += 1f;
			}
			if (num7 < 0)
			{
				num9 += 1f;
			}
			float num10 = ((direction.x == 0f) ? float.PositiveInfinity : (num8 / direction.x));
			float num11 = ((direction.y == 0f) ? float.PositiveInfinity : (num9 / direction.y));
			Vector2 a = vector3;
			if (num10 < num11)
			{
				negOne = new IntVector2(num6, 0);
				vector3.x += num8;
				if (direction.y != 0f && num10 != 0f)
				{
					vector3.y += direction.y * num10;
				}
				num += Vector2.Distance(a, vector3) / 16f;
			}
			else
			{
				negOne = new IntVector2(0, num7);
				if (direction.x != 0f && num11 != 0f)
				{
					vector3.x += direction.x * num11;
				}
				vector3.y += num9;
				num += Vector2.Distance(a, vector3) / 16f;
			}
		}
		return false;
	}

	public bool LinearCast(PixelCollider otherCollider, IntVector2 pixelsToMove, out LinearCastResult result)
	{
		PhysicsEngine.PixelMovementGenerator(pixelsToMove, m_stepList);
		return LinearCast(otherCollider, pixelsToMove, m_stepList, out result);
	}

	public bool LinearCast(PixelCollider otherCollider, IntVector2 pixelsToMove, List<StepData> stepList, out LinearCastResult result, bool traverseSlopes = false, float currentSlope = 0f)
	{
		if (!Enabled)
		{
			result = null;
			return false;
		}
		if (otherCollider.DirectionIgnorer != null && otherCollider.DirectionIgnorer(pixelsToMove))
		{
			result = null;
			return false;
		}
		IntVector2 zero = IntVector2.Zero;
		IntVector2 intVector = otherCollider.m_position - m_position;
		result = LinearCastResult.Pool.Allocate();
		result.MyPixelCollider = this;
		result.OtherPixelCollider = null;
		result.TimeUsed = 0f;
		result.CollidedX = false;
		result.CollidedY = false;
		result.NewPixelsToMove.x = 0;
		result.NewPixelsToMove.y = 0;
		result.Overlap = false;
		float num = 0f;
		for (int i = 0; i < stepList.Count; i++)
		{
			IntVector2 deltaPos = stepList[i].deltaPos;
			float deltaTime = stepList[i].deltaTime;
			num += deltaTime;
			IntVector2 intVector2 = m_position + zero + deltaPos;
			if (IntVector2.AABBOverlap(intVector2, m_dimensions, otherCollider.Position, otherCollider.Dimensions))
			{
				IntVector2 intVector3 = IntVector2.Max(IntVector2.Zero, otherCollider.Position - intVector2);
				IntVector2 intVector4 = IntVector2.Min(m_dimensions - IntVector2.One, otherCollider.UpperRight - intVector2);
				for (int j = intVector3.x; j <= intVector4.x; j++)
				{
					for (int k = intVector3.y; k <= intVector4.y; k++)
					{
						if (!m_bestPixels[j, k])
						{
							continue;
						}
						IntVector2 pos = new IntVector2(j, k) - intVector + zero + deltaPos;
						if (pos.x >= 0 && pos.x < otherCollider.Dimensions.x && pos.y >= 0 && pos.y < otherCollider.Dimensions.y && otherCollider[pos] && (!otherCollider.IsSlope || !traverseSlopes || otherCollider.Slope != currentSlope))
						{
							result.TimeUsed = num;
							result.CollidedX = deltaPos.x != 0;
							result.CollidedY = deltaPos.y != 0;
							result.NewPixelsToMove = zero;
							if (!otherCollider.IsSlope || deltaPos.y == 1 || deltaPos.y < 0 || Math.Sign(deltaPos.x) == Math.Sign(otherCollider.SlopeEnd.y - otherCollider.SlopeStart.y))
							{
							}
							result.MyPixelCollider = this;
							result.OtherPixelCollider = otherCollider;
							IntVector2 value = Position + new IntVector2(j, k) + zero + deltaPos;
							result.Contact = FromCollisionVector(value) + new Vector2(0.5f, 0.5f) / 16f;
							result.Normal = (Vector2)(-deltaPos);
							if (otherCollider.NormalModifier != null)
							{
								result.Normal = otherCollider.NormalModifier(result.Normal);
							}
							return true;
						}
					}
				}
			}
			zero += deltaPos;
		}
		result.NewPixelsToMove = zero;
		return false;
	}

	public bool AABBContainsPixel(IntVector2 pixel)
	{
		return pixel.x >= Min.x && pixel.x <= Max.x && pixel.y >= Min.y && pixel.y <= Max.y;
	}

	public bool ContainsPixel(IntVector2 pixel)
	{
		if (!AABBContainsPixel(pixel))
		{
			return false;
		}
		return m_bestPixels[pixel.x - m_position.x, pixel.y - m_position.y];
	}

	public void SetRotationAndScale(float rotation, Vector2 scale)
	{
		BitArray2D bitArray2D = ((rotation != 0f || !(scale == Vector2.one)) ? m_modifiedPixels : m_basePixels);
		if (m_rotation == rotation && m_scale == scale && m_bestPixels == bitArray2D && m_bestPixels != null && m_bestPixels.IsValid)
		{
			return;
		}
		m_rotation = rotation;
		m_scale = scale;
		int width = m_basePixels.Width;
		int height = m_basePixels.Height;
		if (rotation == 0f && scale == Vector2.one)
		{
			m_bestPixels = m_basePixels;
			m_dimensions = new IntVector2(width, height);
			m_transformOffset = IntVector2.Zero;
			return;
		}
		if (m_modifiedPixels == null)
		{
			m_modifiedPixels = new BitArray2D();
		}
		Vector2 vector = -(Vector2)m_offset;
		Vector2 vector2 = TransformPixel(new Vector2(0.5f, 0.5f), vector, rotation, scale);
		Vector2 vector3 = TransformPixel(new Vector2((float)width - 0.5f, 0.5f), vector, rotation, scale);
		Vector2 vector4 = TransformPixel(new Vector2(0.5f, (float)height - 0.5f), vector, rotation, scale);
		Vector2 vector5 = TransformPixel(new Vector2((float)width - 0.5f, (float)height - 0.5f), vector, rotation, scale);
		int num = Mathf.FloorToInt(Mathf.Min(vector2.x, vector3.x, vector4.x, vector5.x));
		int num2 = Mathf.FloorToInt(Mathf.Min(vector2.y, vector3.y, vector4.y, vector5.y));
		int num3 = Mathf.CeilToInt(Mathf.Max(vector2.x, vector3.x, vector4.x, vector5.x));
		int num4 = Mathf.CeilToInt(Mathf.Max(vector2.y, vector3.y, vector4.y, vector5.y));
		m_transformOffset = new IntVector2(num, num2);
		Vector2 pivot = vector - (Vector2)m_transformOffset;
		int num5 = num3 - num;
		int num6 = num4 - num2;
		m_modifiedPixels.ReinitializeWithDefault(num5, num6, false);
		if (m_basePixels.IsAabb)
		{
			int num7 = 4;
			Vector2[] array = new Vector2[4]
			{
				vector2 - m_transformOffset.ToVector2(),
				vector3 - m_transformOffset.ToVector2(),
				vector5 - m_transformOffset.ToVector2(),
				vector4 - m_transformOffset.ToVector2()
			};
			int[] array2 = new int[4];
			for (int i = 0; i < num6; i++)
			{
				int num8 = 0;
				int num9 = num7 - 1;
				int j;
				for (j = 0; j < num7; j++)
				{
					if (((double)array[j].y < (double)i && (double)array[num9].y >= (double)i) || ((double)array[num9].y < (double)i && (double)array[j].y >= (double)i))
					{
						array2[num8++] = (int)(array[j].x + ((float)i - array[j].y) / (array[num9].y - array[j].y) * (array[num9].x - array[j].x));
					}
					num9 = j;
				}
				j = 0;
				while (j < num8 - 1)
				{
					if (array2[j] > array2[j + 1])
					{
						int num10 = array2[j];
						array2[j] = array2[j + 1];
						array2[j + 1] = num10;
						if (j != 0)
						{
							j--;
						}
					}
					else
					{
						j++;
					}
				}
				for (j = 0; j < num8 && array2[j] < num5 - 1; j += 2)
				{
					if (array2[j + 1] > 0)
					{
						if (array2[j] < 0)
						{
							array2[j] = 0;
						}
						if (array2[j + 1] > num5 - 1)
						{
							array2[j + 1] = num5 - 1;
						}
						for (int k = array2[j]; k < array2[j + 1]; k++)
						{
							m_modifiedPixels[k, i] = true;
						}
					}
				}
			}
		}
		else
		{
			float rotation2 = 0f - rotation;
			Vector2 scale2 = new Vector2(1f / scale.x, 1f / scale.y);
			for (int l = 0; l < num5; l++)
			{
				for (int m = 0; m < num6; m++)
				{
					Vector2 pixel = new Vector2((float)l + 0.5f, (float)m + 0.5f);
					Vector2 vector6 = TransformPixel(pixel, pivot, rotation2, scale2) + (Vector2)m_transformOffset;
					if (vector6.x < 0f || (int)vector6.x >= width || vector6.y < 0f || (int)vector6.y >= height)
					{
						m_modifiedPixels[l, m] = false;
					}
					else
					{
						m_modifiedPixels[l, m] = m_basePixels[(int)vector6.x, (int)vector6.y];
					}
				}
			}
		}
		m_dimensions = new IntVector2(num5, num6);
		m_bestPixels = m_modifiedPixels;
	}

	private Vector2 TransformPixel(Vector2 pixel, Vector2 pivot, float rotation, Vector2 scale)
	{
		Vector2 vector = pixel - pivot;
		Vector2 a = default(Vector2);
		a.x = vector.x * Mathf.Cos(rotation * ((float)Math.PI / 180f)) - vector.y * Mathf.Sin(rotation * ((float)Math.PI / 180f));
		a.y = vector.x * Mathf.Sin(rotation * ((float)Math.PI / 180f)) + vector.y * Mathf.Cos(rotation * ((float)Math.PI / 180f));
		return Vector2.Scale(a, scale) + pivot;
	}

	public void RegisterFrameSpecificCollisionException(SpeculativeRigidbody mySpecRigidbody, PixelCollider pixelCollider)
	{
		if (!m_frameSpecificCollisionExceptions.Contains(pixelCollider))
		{
			m_frameSpecificCollisionExceptions.Add(pixelCollider);
			mySpecRigidbody.HasFrameSpecificCollisionExceptions = true;
		}
	}

	public void ClearFrameSpecificCollisionExceptions()
	{
		m_frameSpecificCollisionExceptions.Clear();
	}

	public TriggerCollisionData RegisterTriggerCollision(SpeculativeRigidbody mySpecRigidbody, SpeculativeRigidbody otherSpecRigidbody, PixelCollider otherPixelCollider)
	{
		TriggerCollisionData triggerCollisionData = m_triggerCollisions.Find((TriggerCollisionData d) => d.PixelCollider == otherPixelCollider);
		if (triggerCollisionData == null)
		{
			triggerCollisionData = new TriggerCollisionData(otherSpecRigidbody, otherPixelCollider);
			m_triggerCollisions.Add(triggerCollisionData);
			mySpecRigidbody.HasTriggerCollisions = true;
		}
		else
		{
			triggerCollisionData.ContinuedCollision = true;
		}
		return triggerCollisionData;
	}

	public void ResetTriggerCollisionData()
	{
		for (int i = 0; i < m_triggerCollisions.Count; i++)
		{
			m_triggerCollisions[i].Reset();
		}
	}

	public void Regenerate(Transform transform, bool allowRotation = true, bool allowScale = true)
	{
		if (!Sprite)
		{
			Sprite = transform.GetComponentInChildren<tk2dBaseSprite>();
		}
		float rotation = ((!allowRotation) ? 0f : transform.eulerAngles.z);
		Vector2 vector = ((!allowScale) ? Vector2.one : ((Vector2)transform.localScale));
		if (allowScale && (bool)Sprite)
		{
			vector = Vector2.Scale(vector, Sprite.scale);
		}
		switch (ColliderGenerationMode)
		{
		case PixelColliderGeneration.Manual:
			RegenerateFromManual(transform, new IntVector2(ManualOffsetX, ManualOffsetY), new IntVector2(ManualWidth, ManualHeight), rotation, vector);
			break;
		case PixelColliderGeneration.Tk2dPolygon:
			RegenerateFrom3dCollider(Sprite.GetTrueCurrentSpriteDef().colliderVertices, transform, rotation, vector, Sprite.FlipX, Sprite.FlipY);
			break;
		case PixelColliderGeneration.BagelCollider:
			RegenerateFromBagelCollider(Sprite, transform, rotation, vector, Sprite.FlipX);
			break;
		case PixelColliderGeneration.Circle:
			if (ManualDiameter <= 0 && ManualRadius > 0)
			{
				ManualDiameter = 2 * ManualRadius;
			}
			RegenerateFromCircle(transform, new IntVector2(ManualOffsetX, ManualOffsetY), ManualDiameter);
			break;
		case PixelColliderGeneration.Line:
			RegenerateFromLine(transform, new IntVector2(ManualLeftX, ManualLeftY), new IntVector2(ManualRightX, ManualRightY));
			break;
		}
	}

	public void RegenerateFromManual(Transform transform, IntVector2 offset, IntVector2 dimensions, float rotation = 0f, Vector2? scale = null)
	{
		RegenerateFromManual(transform.position, offset, dimensions, rotation, scale);
	}

	public void RegenerateFromManual(Vector2 position, IntVector2 offset, IntVector2 dimensions, float rotation = 0f, Vector2? scale = null)
	{
		if (!scale.HasValue)
		{
			scale = new Vector2(1f, 1f);
		}
		m_offset = offset;
		m_dimensions = dimensions;
		m_position = ToCollisionVector(position) + m_offset;
		m_basePixels.ReinitializeWithDefault(m_dimensions.x, m_dimensions.y, true, 0f, true);
		m_bestPixels = m_basePixels;
		SetRotationAndScale(rotation, scale.Value);
	}

	public void RegenerateFrom3dCollider(Vector3[] allVertices, Transform transform, float rotation = 0f, Vector2? scale = null, bool flipX = false, bool flipY = false)
	{
		if (!scale.HasValue)
		{
			scale = new Vector2(1f, 1f);
		}
		if (allVertices.Length == 2)
		{
			Vector2[] array = new Vector2[4];
			Vector2 vector = allVertices[0];
			Vector2 vector2 = allVertices[1];
			if (flipX)
			{
				vector.x *= -1f;
			}
			if (flipY)
			{
				vector.y *= -1f;
			}
			array[0] = vector + new Vector2(0f - vector2.x, vector2.y);
			array[1] = vector + new Vector2(0f - vector2.x, 0f - vector2.y);
			array[2] = vector + new Vector2(vector2.x, 0f - vector2.y);
			array[3] = vector + new Vector2(vector2.x, vector2.y);
			RegenerateFromVertices(array, transform, rotation, scale);
			return;
		}
		Vector2[] array2 = new Vector2[allVertices.Length / 2];
		int num = 0;
		for (int i = 0; i < allVertices.Length; i++)
		{
			if (num >= array2.Length)
			{
				break;
			}
			if (!(allVertices[i].z < 0f))
			{
				continue;
			}
			Vector2 vector3 = allVertices[i];
			bool flag = false;
			for (int j = 0; j < num; j++)
			{
				if (Mathf.Approximately(array2[j].x, vector3.x) && Mathf.Approximately(array2[j].y, vector3.y))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array2[num++] = vector3;
			}
		}
		array2 = BraveUtility.ResizeArray(array2, num);
		RegenerateFromVertices(array2, transform, rotation, scale);
	}

	public void RegenerateFromBagelCollider(tk2dBaseSprite sprite, Transform transform, float rotation = 0f, Vector2? scale = null, bool flipX = false)
	{
		if (!scale.HasValue)
		{
			scale = new Vector2(1f, 1f);
		}
		tk2dSpriteDefinition tk2dSpriteDefinition2 = (m_lastSpriteDef = ((!BagleUseFirstFrameOnly || string.IsNullOrEmpty(SpecifyBagelFrame)) ? sprite.GetTrueCurrentSpriteDef() : sprite.Collection.GetSpriteDefinition(SpecifyBagelFrame)));
		if (!BagleUseFirstFrameOnly && m_cachedBasePixels == null)
		{
			m_cachedBasePixels = new Dictionary<int, PixelCache>();
		}
		int num = ((tk2dSpriteDefinition2 != null) ? sprite.GetSpriteIdByName(tk2dSpriteDefinition2.name) : (-1));
		if (!BagleUseFirstFrameOnly && m_cachedBasePixels.ContainsKey(num))
		{
			PixelCache pixelCache = m_cachedBasePixels[num];
			m_dimensions = pixelCache.dimensions;
			m_basePixels = pixelCache.basePixels;
			m_bestPixels = m_basePixels;
			m_offset = pixelCache.offset;
		}
		else
		{
			m_basePixels = new BitArray2D();
			BagelCollider[] bagelColliders = sprite.Collection.GetBagelColliders(num);
			int num2 = ((bagelColliders != null) ? bagelColliders.Length : 0);
			BagelCollider bagelCollider = ((BagelColliderNumber >= num2) ? null : bagelColliders[BagelColliderNumber]);
			if (bagelCollider == null)
			{
				RegenerateEmptyCollider(transform);
				if (!BagleUseFirstFrameOnly)
				{
					PixelCache pixelCache2 = new PixelCache();
					pixelCache2.dimensions = m_dimensions;
					pixelCache2.basePixels = m_basePixels;
					pixelCache2.offset = m_offset;
					pixelCache2.basePixels.ReadOnly = true;
					m_cachedBasePixels.Add(num, pixelCache2);
				}
				return;
			}
			tk2dSlicedSprite tk2dSlicedSprite2 = Sprite as tk2dSlicedSprite;
			IntVector2 intVector;
			IntVector2 intVector2;
			if ((bool)tk2dSlicedSprite2)
			{
				intVector = IntVector2.Zero;
				intVector2 = new IntVector2(Mathf.RoundToInt(tk2dSlicedSprite2.dimensions.x) - 1, Mathf.RoundToInt(tk2dSlicedSprite2.dimensions.y) - 1);
			}
			else
			{
				intVector = IntVector2.MaxValue;
				intVector2 = IntVector2.MinValue;
				for (int i = 0; i < bagelCollider.width; i++)
				{
					for (int j = 0; j < bagelCollider.height; j++)
					{
						if (bagelCollider[i, bagelCollider.height - j - 1])
						{
							intVector = IntVector2.Min(intVector, new IntVector2(i, j));
							intVector2 = IntVector2.Max(intVector2, new IntVector2(i, j));
						}
					}
				}
				if (intVector == IntVector2.MaxValue || intVector2 == IntVector2.MinValue)
				{
					RegenerateEmptyCollider(transform);
					return;
				}
			}
			m_dimensions = intVector2 - intVector + IntVector2.One;
			m_basePixels.Reinitialize(m_dimensions.x, m_dimensions.y, true);
			m_bestPixels = m_basePixels;
			if ((bool)tk2dSlicedSprite2)
			{
				m_offset = intVector - tk2dSlicedSprite2.anchorOffset.ToIntVector2();
				tk2dSpriteDefinition trueCurrentSpriteDef = tk2dSlicedSprite2.GetTrueCurrentSpriteDef();
				float num3 = trueCurrentSpriteDef.position1.x - trueCurrentSpriteDef.position0.x;
				float num4 = trueCurrentSpriteDef.position2.y - trueCurrentSpriteDef.position0.y;
				float x = trueCurrentSpriteDef.texelSize.x;
				float y = trueCurrentSpriteDef.texelSize.y;
				IntVector2 intVector3 = new IntVector2(Mathf.RoundToInt(num3 / x), Mathf.RoundToInt(num4 / y));
				Vector3 boundsDataExtents = trueCurrentSpriteDef.boundsDataExtents;
				Vector3 vector = new Vector3(boundsDataExtents.x / trueCurrentSpriteDef.texelSize.x, boundsDataExtents.y / trueCurrentSpriteDef.texelSize.y, 1f);
				IntVector2 intVector4 = new IntVector2(Mathf.RoundToInt(tk2dSlicedSprite2.dimensions.x), Mathf.RoundToInt(tk2dSlicedSprite2.dimensions.y));
				int num5 = Mathf.RoundToInt(tk2dSlicedSprite2.borderTop * vector.y);
				int num6 = Mathf.RoundToInt(tk2dSlicedSprite2.borderBottom * vector.y);
				int num7 = Mathf.RoundToInt(tk2dSlicedSprite2.borderLeft * vector.x);
				int num8 = Mathf.RoundToInt(tk2dSlicedSprite2.borderRight * vector.x);
				int num9 = intVector3.x - num7 - num8;
				int num10 = intVector3.y - num5 - num6;
				for (int k = intVector.x; k <= intVector2.x; k++)
				{
					int x2 = ((k < num7) ? k : ((k >= intVector4.x - num8) ? (intVector3.x - (intVector4.x - k)) : ((k - num7) % num9 + num7)));
					for (int l = intVector.y; l <= intVector2.y; l++)
					{
						int num11 = ((l >= num6) ? ((l >= intVector4.y - num5) ? (intVector3.y - (intVector4.y - l)) : ((l - num6) % num10 + num5)) : l);
						m_basePixels[k, l] = bagelCollider[x2, bagelCollider.height - num11 - 1];
					}
				}
			}
			else
			{
				m_offset = intVector - Sprite.GetAnchorPixelOffset();
				for (int m = intVector.x; m <= intVector2.x; m++)
				{
					for (int n = intVector.y; n <= intVector2.y; n++)
					{
						m_basePixels[m - intVector.x, n - intVector.y] = bagelCollider[m, bagelCollider.height - n - 1];
					}
				}
			}
			if (!BagleUseFirstFrameOnly)
			{
				PixelCache pixelCache3 = new PixelCache();
				pixelCache3.dimensions = m_dimensions;
				pixelCache3.basePixels = m_basePixels;
				pixelCache3.offset = m_offset;
				pixelCache3.basePixels.ReadOnly = true;
				m_cachedBasePixels.Add(num, pixelCache3);
			}
		}
		m_position = ToCollisionVector(transform.position) + m_offset;
		SetRotationAndScale(rotation, scale.Value);
	}

	public void RegenerateFromCircle(Transform transform, IntVector2 offset, int diameter)
	{
		RegenerateFromCircle(transform.position, offset, diameter);
	}

	public void RegenerateFromCircle(Vector2 position, IntVector2 offset, int diameter)
	{
		m_offset = offset;
		m_dimensions = new IntVector2(diameter, diameter);
		m_position = ToCollisionVector(position) + m_offset;
		m_basePixels.Reinitialize(m_dimensions.x, m_dimensions.y, true);
		m_bestPixels = m_basePixels;
		float num = (float)diameter / 2f;
		for (int i = 0; i < m_dimensions.x; i++)
		{
			for (int j = 0; j < m_dimensions.y; j++)
			{
				m_basePixels[i, j] = Vector2.Distance(new Vector2(i, j), new Vector2(num, num)) < num;
			}
		}
		SetRotationAndScale(0f, new Vector2(1f, 1f));
	}

	public void RegenerateFromLine(Transform transform, IntVector2 leftPoint, IntVector2 rightPoint)
	{
		RegenerateFromLine(transform.position, leftPoint, rightPoint);
	}

	public void RegenerateFromLine(Vector2 position, IntVector2 leftPoint, IntVector2 rightPoint)
	{
		m_offset = new IntVector2(Mathf.Min(leftPoint.x, rightPoint.x), Mathf.Min(leftPoint.y, rightPoint.y));
		m_dimensions = new IntVector2(Mathf.Abs(rightPoint.x - leftPoint.x) + 1, Mathf.Abs(rightPoint.y - leftPoint.y) + 1);
		m_position = ToCollisionVector(position) + m_offset;
		m_basePixels.ReinitializeWithDefault(m_dimensions.x, m_dimensions.y, false, 0f, true);
		m_bestPixels = m_basePixels;
		PlotPixelLines(new Vector2[2]
		{
			PhysicsEngine.PixelToUnit(leftPoint),
			PhysicsEngine.PixelToUnit(rightPoint)
		}, -PhysicsEngine.PixelToUnit(m_offset));
		SetRotationAndScale(0f, new Vector2(1f, 1f));
	}

	public void RegenerateEmptyCollider(Transform transform)
	{
		m_offset = IntVector2.Zero;
		m_dimensions = IntVector2.Zero;
		m_position = ToCollisionVector(transform.position) + m_offset;
		m_basePixels.Reinitialize(0, 0, true);
		m_bestPixels = m_basePixels;
		SetRotationAndScale(0f, new Vector2(1f, 1f));
	}

	public void RegenerateFromVertices(Vector2[] vertices, Transform transform, float rotation = 0f, Vector2? scale = null)
	{
		RegenerateFromVertices(vertices, ToCollisionVector(transform.position), rotation, scale);
	}

	public void RegenerateFromVertices(Vector2[] vertices, IntVector2 position, float rotation = 0f, Vector2? scale = null)
	{
		if (!scale.HasValue)
		{
			scale = new Vector2(1f, 1f);
		}
		m_position = position;
		m_bestPixels = m_basePixels;
		if (vertices.Length == 0)
		{
			m_dimensions = IntVector2.Zero;
			m_basePixels.Reinitialize(0, 0, true);
		}
		else
		{
			Vector2 lhs = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 lhs2 = new Vector2(float.MinValue, float.MinValue);
			foreach (Vector3 vector in vertices)
			{
				lhs = Vector2.Min(lhs, vector);
				lhs2 = Vector2.Max(lhs2, vector);
			}
			m_offset = new IntVector2(Mathf.FloorToInt(lhs.x * 16f), Mathf.FloorToInt(lhs.y * 16f));
			m_position += m_offset;
			m_dimensions = new IntVector2(Mathf.CeilToInt(lhs2.x * 16f), Mathf.CeilToInt(lhs2.y * 16f)) - m_offset;
			m_basePixels.ReinitializeWithDefault(m_dimensions.x, m_dimensions.y, false, 0f, true);
			PlotPixelLines(vertices, -PhysicsEngine.PixelToUnit(m_offset));
			FillInternalPixels();
		}
		SetRotationAndScale(rotation, scale.Value);
	}

	private static int ToCollisionPixel(float value)
	{
		return Mathf.RoundToInt(value * 16f);
	}

	private static IntVector2 ToCollisionVector(Vector2 value)
	{
		return new IntVector2(ToCollisionPixel(value.x), ToCollisionPixel(value.y));
	}

	private static float FromCollisionPixel(int value)
	{
		return (float)value / 16f;
	}

	private static Vector2 FromCollisionVector(IntVector2 value)
	{
		return new Vector2(FromCollisionPixel(value.x), FromCollisionPixel(value.y));
	}

	private void PlotPixelLines(Vector2[] vertices)
	{
		PlotPixelLines(vertices, Vector2.zero);
	}

	private void PlotPixelLines(Vector2[] vertices, Vector2 offset)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			IntVector2 intVector = ToCollisionVector(vertices[i] + offset);
			IntVector2 intVector2 = ToCollisionVector(vertices[(i + 1) % vertices.Length] + offset);
			if (intVector.x == m_dimensions.x)
			{
				intVector.x--;
			}
			if (intVector.y == m_dimensions.y)
			{
				intVector.y--;
			}
			if (intVector2.x == m_dimensions.x)
			{
				intVector2.x--;
			}
			if (intVector2.y == m_dimensions.y)
			{
				intVector2.y--;
			}
			PlotPixelLine(intVector.x, intVector.y, intVector2.x, intVector2.y);
		}
	}

	private void PlotPixelLine(int x0, int y0, int x1, int y1)
	{
		bool flag = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
		if (flag)
		{
			Swap(ref x0, ref y0);
			Swap(ref x1, ref y1);
		}
		if (x0 > x1)
		{
			Swap(ref x0, ref x1);
			Swap(ref y0, ref y1);
		}
		int num = x1 - x0;
		int num2 = Mathf.Abs(y1 - y0);
		int num3 = num / 2;
		int num4 = y0;
		int num5 = ((y0 < y1) ? 1 : (-1));
		for (int i = x0; i <= x1; i++)
		{
			if (flag)
			{
				m_basePixels[num4, i] = true;
			}
			else
			{
				m_basePixels[i, num4] = true;
			}
			num3 -= num2;
			if (num3 < 0)
			{
				num4 += num5;
				num3 += num;
			}
		}
	}

	private void Swap(ref int a, ref int b)
	{
		int num = a;
		a = b;
		b = num;
	}

	private void FillInternalPixels()
	{
		for (int i = 0; i < Width; i++)
		{
			int num = -1;
			int num2 = -1;
			for (int j = 0; j < Height; j++)
			{
				if (m_basePixels[i, j])
				{
					num = j;
					break;
				}
			}
			if (num == -1)
			{
				continue;
			}
			for (int num3 = Height - 1; num3 >= 0; num3--)
			{
				if (m_basePixels[i, num3])
				{
					num2 = num3;
					break;
				}
			}
			for (int k = num + 1; k < num2; k++)
			{
				m_basePixels[i, k] = true;
			}
		}
	}

	private void UpdateSlope()
	{
		if (m_slopeStart.HasValue && m_slopeEnd.HasValue)
		{
			IsSlope = true;
			Slope = (m_slopeEnd.Value.y - m_slopeStart.Value.y) / (m_slopeEnd.Value.x - m_slopeStart.Value.x);
		}
	}

	public static PixelCollider CreateRectangle(CollisionLayer layer, int x, int y, int width, int height, bool enabled = true)
	{
		PixelCollider pixelCollider = new PixelCollider();
		pixelCollider.CollisionLayer = layer;
		pixelCollider.ColliderGenerationMode = PixelColliderGeneration.Manual;
		pixelCollider.ManualOffsetX = x;
		pixelCollider.ManualOffsetY = y;
		pixelCollider.ManualWidth = width;
		pixelCollider.ManualHeight = height;
		pixelCollider.Enabled = enabled;
		return pixelCollider;
	}
}
