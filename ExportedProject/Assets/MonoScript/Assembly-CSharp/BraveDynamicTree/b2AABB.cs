using UnityEngine;

namespace BraveDynamicTree
{
	public struct b2AABB
	{
		public Vector2 lowerBound;

		public Vector2 upperBound;

		public b2AABB(float lowX, float lowY, float upperX, float upperY)
		{
			lowerBound.x = lowX;
			lowerBound.y = lowY;
			upperBound.x = upperX;
			upperBound.y = upperY;
		}

		public b2AABB(Vector2 lowerBound, Vector2 upperBound)
		{
			this.lowerBound = lowerBound;
			this.upperBound = upperBound;
		}

		public bool IsValid()
		{
			Vector2 vector = upperBound - lowerBound;
			return vector.x >= 0f && vector.y >= 0f;
		}

		public Vector2 GetCenter()
		{
			return 0.5f * (lowerBound + upperBound);
		}

		public Vector2 GetExtents()
		{
			return 0.5f * (upperBound - lowerBound);
		}

		public float GetPerimeter()
		{
			float num = upperBound.x - lowerBound.x;
			float num2 = upperBound.y - lowerBound.y;
			return 2f * (num + num2);
		}

		public void Combine(b2AABB aabb)
		{
			lowerBound = Vector2.Min(lowerBound, aabb.lowerBound);
			upperBound = Vector2.Max(upperBound, aabb.upperBound);
		}

		public void Combine(b2AABB aabb1, b2AABB aabb2)
		{
			lowerBound = Vector2.Min(aabb1.lowerBound, aabb2.lowerBound);
			upperBound = Vector2.Max(aabb1.upperBound, aabb2.upperBound);
		}

		public bool Contains(b2AABB aabb)
		{
			return lowerBound.x <= aabb.lowerBound.x && lowerBound.y <= aabb.lowerBound.y && aabb.upperBound.x <= upperBound.x && aabb.upperBound.y <= upperBound.y;
		}

		public bool RayCast(ref b2RayCastOutput output, b2RayCastInput input)
		{
			float num = float.MinValue;
			float num2 = float.MaxValue;
			Vector2 p = input.p1;
			Vector2 vector = input.p2 - input.p1;
			Vector2 vector2 = vector.Abs();
			Vector2 zero = Vector2.zero;
			for (int i = 0; i < 2; i++)
			{
				if (vector2[i] < float.Epsilon)
				{
					if (p[i] < lowerBound[i] || upperBound[i] < p[i])
					{
						return false;
					}
					continue;
				}
				float num3 = 1f / vector[i];
				float num4 = (lowerBound[i] - p[i]) * num3;
				float num5 = (upperBound[i] - p[i]) * num3;
				float value = -1f;
				if (num4 > num5)
				{
					float num6 = num4;
					num4 = num5;
					num5 = num6;
					value = 1f;
				}
				if (num4 > num)
				{
					zero = Vector2.zero;
					zero[i] = value;
					num = num4;
				}
				num2 = Mathf.Min(num2, num5);
				if (num > num2)
				{
					return false;
				}
			}
			if (num < 0f || input.maxFraction < num)
			{
				return false;
			}
			output.fraction = num;
			output.normal = zero;
			return true;
		}

		public static bool b2TestOverlap(ref b2AABB a, ref b2AABB b)
		{
			return b.lowerBound.x <= a.upperBound.x && a.lowerBound.x <= b.upperBound.x && b.lowerBound.y <= a.upperBound.y && a.lowerBound.y <= b.upperBound.y;
		}
	}
}
