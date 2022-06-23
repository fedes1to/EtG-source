using System;
using System.Collections.Generic;
using TestSimpleRNG;
using UnityEngine;

public static class BraveMathCollege
{
	private static float[] LowDiscrepancyPseudoRandoms = new float[20]
	{
		0.546f, 0.153f, 0.925f, 0.471f, 0.739f, 0.062f, 0.383f, 0.817f, 0.696f, 0.205f,
		0.554f, 0.847f, 0.075f, 0.639f, 0.261f, 0.938f, 0.617f, 0.183f, 0.304f, 0.795f
	};

	public static float GetLowDiscrepancyRandom(int iterator)
	{
		return LowDiscrepancyPseudoRandoms[iterator % LowDiscrepancyPseudoRandoms.Length];
	}

	private static float ANG_NoiseInternal(float freq)
	{
		float num = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		return Mathf.Sin((float)Math.PI * 2f * freq + num);
	}

	private static float ANG_WeightedSumNoise(float[] amplitudes, float[] noises)
	{
		float num = 0f;
		for (int i = 0; i < noises.Length; i++)
		{
			num += amplitudes[i] * noises[i];
		}
		return num;
	}

	private static float AdvancedNoiseGenerator(Func<float, float> amplitudeLambda)
	{
		float[] array = new float[30]
		{
			1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f,
			11f, 12f, 13f, 14f, 15f, 16f, 17f, 18f, 19f, 20f,
			21f, 22f, 23f, 24f, 25f, 26f, 27f, 28f, 29f, 30f
		};
		float[] array2 = new float[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = amplitudeLambda(array[i] / (float)array.Length);
		}
		float[] array3 = new float[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array3[j] = ANG_NoiseInternal(array[j] / (float)array.Length);
		}
		return ANG_WeightedSumNoise(array2, array3);
	}

	public static float GetRedNoise()
	{
		return AdvancedNoiseGenerator((float f) => 1f / f / f);
	}

	public static float GetPinkNoise()
	{
		return AdvancedNoiseGenerator((float f) => 1f / f);
	}

	public static float GetWhiteNoise()
	{
		return AdvancedNoiseGenerator((float f) => 1f);
	}

	public static float GetBlueNoise()
	{
		return AdvancedNoiseGenerator((float f) => f);
	}

	public static float GetVioletNoise()
	{
		return AdvancedNoiseGenerator((float f) => f * f);
	}

	public static float GetRandomByNormalDistribution(float mean, float stddev)
	{
		return (float)SimpleRNG.GetNormal(mean, stddev);
	}

	public static float NormalDistributionAtPosition(float x, float mean, float stddev)
	{
		float oneOverTwoPi = 1f / (2f * (float)Math.PI);
		float twoTimeStdDev = 2f * stddev;
		return NormalDistributionAtPosition(x, oneOverTwoPi, mean, twoTimeStdDev);
	}

	public static float NormalDistributionAtPosition(float x, float oneOverTwoPi, float mean, float twoTimeStdDev)
	{
		return oneOverTwoPi * Mathf.Exp((0f - (x - mean)) * (x - mean) / (2f * twoTimeStdDev));
	}

	public static float UnboundedLerp(float from, float to, float t)
	{
		return (to - from) * t + from;
	}

	public static float SmoothLerp(float from, float to, float t)
	{
		return Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
	}

	public static float Bilerp(float x0y0, float x1y0, float x0y1, float x1y1, float u, float v)
	{
		float a = Mathf.Lerp(x0y0, x1y0, u);
		float b = Mathf.Lerp(x0y1, x1y1, u);
		return Mathf.Lerp(a, b, v);
	}

	public static float DoubleLerp(float from, float intermediary, float to, float t)
	{
		return (!(t < 0.5f)) ? Mathf.Lerp(intermediary, to, t * 2f - 1f) : Mathf.Lerp(from, intermediary, t * 2f);
	}

	public static Vector2 DoubleLerp(Vector2 from, Vector2 intermediary, Vector2 to, float t)
	{
		return (!(t < 0.5f)) ? Vector2.Lerp(intermediary, to, t * 2f - 1f) : Vector2.Lerp(from, intermediary, t * 2f);
	}

	public static Vector3 DoubleLerp(Vector3 from, Vector3 intermediary, Vector3 to, float t)
	{
		return (!(t < 0.5f)) ? Vector3.Lerp(intermediary, to, t * 2f - 1f) : Vector3.Lerp(from, intermediary, t * 2f);
	}

	public static float DoubleLerpSmooth(float from, float intermediary, float to, float t)
	{
		return (!(t < 0.5f)) ? Mathf.Lerp(intermediary, to, Mathf.SmoothStep(0f, 1f, t * 2f - 1f)) : Mathf.Lerp(from, intermediary, Mathf.SmoothStep(0f, 1f, t * 2f));
	}

	public static Vector2 DoubleLerpSmooth(Vector2 from, Vector2 intermediary, Vector2 to, float t)
	{
		return (!(t < 0.5f)) ? Vector2.Lerp(intermediary, to, Mathf.SmoothStep(0f, 1f, t * 2f - 1f)) : Vector2.Lerp(from, intermediary, Mathf.SmoothStep(0f, 1f, t * 2f));
	}

	public static Vector3 DoubleLerpSmooth(Vector3 from, Vector3 intermediary, Vector3 to, float t)
	{
		return (!(t < 0.5f)) ? Vector3.Lerp(intermediary, to, Mathf.SmoothStep(0f, 1f, t * 2f - 1f)) : Vector3.Lerp(from, intermediary, Mathf.SmoothStep(0f, 1f, t * 2f));
	}

	public static Vector2 VectorToCone(Vector2 source, float angleVariance)
	{
		return (Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f - angleVariance, angleVariance)) * source.ToVector3ZUp()).XY();
	}

	public static float ActualSign(float f)
	{
		if (f < 0f)
		{
			return -1f;
		}
		if (f > 0f)
		{
			return 1f;
		}
		return 0f;
	}

	public static int AngleToQuadrant(float angle)
	{
		angle = (angle + 360f) % 360f;
		angle += 45f;
		angle %= 360f;
		int num = Mathf.FloorToInt(angle / 90f);
		return (3 - num + 2) % 4;
	}

	public static int VectorToQuadrant(Vector2 inVec)
	{
		return AngleToQuadrant(inVec.ToAngle());
	}

	public static int VectorToOctant(Vector2 inVec)
	{
		float num = Mathf.Atan2(inVec.y, inVec.x) * 57.29578f;
		num = (num + 360f) % 360f;
		num += 22.5f;
		num %= 360f;
		int num2 = Mathf.FloorToInt(num / 45f);
		return (7 - num2 + 3) % 8;
	}

	public static int VectorToSextant(Vector2 inVec)
	{
		float num = Mathf.Atan2(inVec.y, inVec.x) * 57.29578f;
		num = (num + 360f) % 360f;
		num %= 360f;
		int num2 = Mathf.FloorToInt(num / 60f);
		return (5 - num2 + 2) % 6;
	}

	public static int GreatestCommonDivisor(int a, int b)
	{
		return (b != 0) ? GreatestCommonDivisor(b, a % b) : a;
	}

	public static int AngleToOctant(float angle)
	{
		return (int)((472.5f - angle) / 45f) % 8;
	}

	public static Vector2 ReflectVectorAcrossNormal(Vector2 vector, Vector2 normal)
	{
		float num = (Mathf.Atan2(normal.y, normal.x) - Mathf.Atan2(vector.y, vector.x)) * 57.29578f;
		return Quaternion.Euler(0f, 0f, 180f + 2f * num) * vector;
	}

	public static Vector2 CircleCenterFromThreePoints(Vector2 a, Vector2 b, Vector2 c)
	{
		float num = b.y - a.y;
		float num2 = b.x - a.x;
		float num3 = c.y - b.y;
		float num4 = c.x - b.x;
		float num5 = num / num2;
		float num6 = num3 / num4;
		float num7 = (num5 * num6 * (a.y - c.y) + num6 * (a.x + b.x) - num5 * (b.x + c.x)) / (2f * (num6 - num5));
		float y = -1f * (num7 - (a.x + b.x) / 2f) / num5 + (a.y + b.y) / 2f;
		return new Vector2(num7, y);
	}

	public static float QuantizeFloat(float input, float multiplesOf)
	{
		return Mathf.Round(input / multiplesOf) * multiplesOf;
	}

	public static float LinearToSmoothStepInterpolate(float from, float to, float t, int iterations)
	{
		float num = t;
		for (int i = 0; i < iterations; i++)
		{
			num = LinearToSmoothStepInterpolate(from, to, num);
		}
		return num;
	}

	public static float LinearToSmoothStepInterpolate(float from, float to, float t)
	{
		return Mathf.Lerp(Mathf.Lerp(from, to, t), Mathf.SmoothStep(from, to, t), t);
	}

	public static float SmoothStepToLinearStepInterpolate(float from, float to, float t)
	{
		return Mathf.Lerp(Mathf.SmoothStep(from, to, t), Mathf.Lerp(from, to, t), t);
	}

	public static float HermiteInterpolation(float t)
	{
		return (0f - t) * t * t * 2f + t * t * 3f;
	}

	public static bool LineSegmentRectangleIntersection(Vector2 p0, Vector2 p1, Vector2 rectMin, Vector2 rectMax, ref Vector2 result)
	{
		Vector2 result2 = Vector2.zero;
		Vector2 result3 = Vector2.zero;
		Vector2 result4 = Vector2.zero;
		Vector2 result5 = Vector2.zero;
		bool flag = LineSegmentIntersection(p0, p1, rectMin, rectMin.WithX(rectMax.x), ref result2);
		bool flag2 = LineSegmentIntersection(p0, p1, rectMin.WithX(rectMax.x), rectMax, ref result3);
		bool flag3 = LineSegmentIntersection(p0, p1, rectMax, rectMin.WithY(rectMax.y), ref result4);
		bool flag4 = LineSegmentIntersection(p0, p1, rectMin, rectMin.WithY(rectMax.y), ref result5);
		float num = float.MaxValue;
		bool result6 = false;
		result = Vector2.zero;
		if (flag && Vector2.Distance(p0, result2) < num)
		{
			num = Vector2.Distance(p0, result2);
			result = result2;
			result6 = true;
		}
		if (flag2 && Vector2.Distance(p0, result3) < num)
		{
			num = Vector2.Distance(p0, result3);
			result = result3;
			result6 = true;
		}
		if (flag3 && Vector2.Distance(p0, result4) < num)
		{
			num = Vector2.Distance(p0, result4);
			result = result4;
			result6 = true;
		}
		if (flag4 && Vector2.Distance(p0, result5) < num)
		{
			num = Vector2.Distance(p0, result5);
			result = result5;
			result6 = true;
		}
		return result6;
	}

	public static bool LineSegmentIntersection(Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1, ref Vector2 result)
	{
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		zero.x = p1.x - p0.x;
		zero.y = p1.y - p0.y;
		zero2.x = q1.x - q0.x;
		zero2.y = q1.y - q0.y;
		float num = ((0f - zero.y) * (p0.x - q0.x) + zero.x * (p0.y - q0.y)) / ((0f - zero2.x) * zero.y + zero.x * zero2.y);
		float num2 = (zero2.x * (p0.y - q0.y) - zero2.y * (p0.x - q0.x)) / ((0f - zero2.x) * zero.y + zero.x * zero2.y);
		result = Vector2.zero;
		if (num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f)
		{
			result.x = p0.x + num2 * zero.x;
			result.y = p0.y + num2 * zero.y;
			return true;
		}
		return false;
	}

	public static Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 v, Vector2 w)
	{
		float sqrMagnitude = (w - v).sqrMagnitude;
		if ((double)sqrMagnitude == 0.0)
		{
			return v;
		}
		float num = Vector2.Dot(p - v, w - v) / sqrMagnitude;
		if (num < 0f)
		{
			return v;
		}
		if (num > 1f)
		{
			return w;
		}
		return v + num * (w - v);
	}

	public static Vector2 ClosestPointOnRectangle(Vector2 point, Vector2 origin, Vector2 dimensions)
	{
		Vector2 vector = origin;
		Vector2 vector2 = new Vector2(origin.x + dimensions.x, origin.y);
		Vector2 vector3 = origin + dimensions;
		Vector2 vector4 = new Vector2(origin.x, origin.y + dimensions.y);
		Vector2 vector5 = ClosestPointOnLineSegment(point, vector, vector2);
		float num = Vector2.Distance(point, vector5);
		Vector2 result = vector5;
		vector5 = ClosestPointOnLineSegment(point, vector2, vector3);
		float num2 = Vector2.Distance(point, vector5);
		if (num2 < num)
		{
			num = num2;
			result = vector5;
		}
		vector5 = ClosestPointOnLineSegment(point, vector3, vector4);
		num2 = Vector2.Distance(point, vector5);
		if (num2 < num)
		{
			num = num2;
			result = vector5;
		}
		vector5 = ClosestPointOnLineSegment(point, vector4, vector);
		num2 = Vector2.Distance(point, vector5);
		if (num2 < num)
		{
			num = num2;
			result = vector5;
		}
		return result;
	}

	public static Vector2 ClosestPointOnPolygon(List<Vector2> polygon, Vector2 point)
	{
		Vector2 result = Vector2.zero;
		float num = float.MaxValue;
		for (int i = 0; i < polygon.Count; i++)
		{
			Vector2 v = polygon[i];
			Vector2 w = polygon[(i + 1) % polygon.Count];
			Vector2 vector = ClosestPointOnLineSegment(point, v, w);
			float num2 = Vector2.Distance(point, vector);
			if (num2 < num)
			{
				num = num2;
				result = vector;
			}
		}
		return result;
	}

	public static float DistToRectangle(Vector2 point, Vector2 origin, Vector2 dimensions)
	{
		Vector2 b = ClosestPointOnRectangle(point, origin, dimensions);
		return Vector2.Distance(point, b);
	}

	public static float DistBetweenRectangles(Vector2 o1, Vector2 d1, Vector2 o2, Vector2 d2)
	{
		Vector2 vector = Vector2.Min(o1, o2);
		Vector2 vector2 = Vector2.Max(o1 + d1, o2 + d2);
		Vector2 vector3 = vector2 - vector;
		float num = vector3.x - (d1.x + d2.x);
		float num2 = vector3.y - (d1.y + d2.y);
		if (num > 0f && num2 > 0f)
		{
			return Mathf.Sqrt(num * num + num2 * num2);
		}
		if (num > 0f)
		{
			return num;
		}
		if (num2 > 0f)
		{
			return num2;
		}
		return 0f;
	}

	public static float ClampAngle360(float angleDeg)
	{
		angleDeg %= 360f;
		if (angleDeg < 0f)
		{
			angleDeg += 360f;
		}
		return angleDeg;
	}

	public static float ClampAngle180(float angleDeg)
	{
		angleDeg %= 360f;
		if (angleDeg < -180f)
		{
			angleDeg += 360f;
		}
		else if (angleDeg > 180f)
		{
			angleDeg -= 360f;
		}
		return angleDeg;
	}

	public static float ClampAngle2Pi(float angleRad)
	{
		angleRad %= (float)Math.PI * 2f;
		if (angleRad < 0f)
		{
			angleRad += (float)Math.PI * 2f;
		}
		return angleRad;
	}

	public static float ClampAnglePi(float angleRad)
	{
		angleRad %= (float)Math.PI * 2f;
		if (angleRad < -(float)Math.PI)
		{
			angleRad += (float)Math.PI * 2f;
		}
		else if (angleRad > (float)Math.PI)
		{
			angleRad -= (float)Math.PI * 2f;
		}
		return angleRad;
	}

	public static float Atan2Degrees(float y, float x)
	{
		return Mathf.Atan2(y, x) * 57.29578f;
	}

	public static float Atan2Degrees(Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x) * 57.29578f;
	}

	public static float AbsAngleBetween(float a, float b)
	{
		return Mathf.Abs(ClampAngle180(a - b));
	}

	public static Vector2 DegreesToVector(float angle, float magnitude = 1f)
	{
		float f = angle * ((float)Math.PI / 180f);
		return new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * magnitude;
	}

	public static float GetNearestAngle(float angle, float[] options)
	{
		if (options == null || options.Length == 0)
		{
			return angle;
		}
		int num = 0;
		float num2 = AbsAngleBetween(angle, options[0]);
		for (int i = 1; i < options.Length; i++)
		{
			float num3 = AbsAngleBetween(angle, options[i]);
			if (num3 < num2)
			{
				num2 = num3;
				num = i;
			}
		}
		return options[num];
	}

	public static float EstimateBezierPathLength(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int divisions)
	{
		float num = 1f / (float)divisions;
		float num2 = 0f;
		for (int i = 0; i < divisions; i++)
		{
			Vector2 vector = CalculateBezierPoint(num * (float)i, p0, p1, p2, p3);
			Vector2 vector2 = CalculateBezierPoint(num * (float)(i + 1), p0, p1, p2, p3);
			num2 += (vector2 - vector).magnitude;
		}
		return num2;
	}

	public static Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		float num = 1f - t;
		float num2 = t * t;
		float num3 = num * num;
		float num4 = num3 * num;
		float num5 = num2 * t;
		Vector2 vector = num4 * p0;
		vector += 3f * num3 * t * p1;
		vector += 3f * num * num2 * p2;
		return vector + num5 * p3;
	}

	public static int LineCircleIntersections(Vector2 center, float radius, Vector2 p1, Vector2 p2, out Vector2 i1, out Vector2 i2)
	{
		float num = p2.x - p1.x;
		float num2 = p2.y - p1.y;
		float num3 = num * num + num2 * num2;
		float num4 = 2f * (num * (p1.x - center.x) + num2 * (p1.y - center.y));
		float num5 = (p1.x - center.x) * (p1.x - center.x) + (p1.y - center.y) * (p1.y - center.y) - radius * radius;
		float num6 = num4 * num4 - 4f * num3 * num5;
		if (num3 <= 1E-07f || num6 < 0f)
		{
			i1 = new Vector2(float.NaN, float.NaN);
			i2 = new Vector2(float.NaN, float.NaN);
			return 0;
		}
		float num7;
		if (num6 == 0f)
		{
			num7 = (0f - num4) / (2f * num3);
			i1 = new Vector2(p1.x + num7 * num, p1.y + num7 * num2);
			i2 = new Vector2(float.NaN, float.NaN);
			return 1;
		}
		num7 = (float)(((double)(0f - num4) + Math.Sqrt(num6)) / (double)(2f * num3));
		i1 = new Vector2(p1.x + num7 * num, p1.y + num7 * num2);
		num7 = (float)(((double)(0f - num4) - Math.Sqrt(num6)) / (double)(2f * num3));
		i2 = new Vector2(p1.x + num7 * num, p1.y + num7 * num2);
		return 2;
	}

	public static int LineSegmentCircleIntersections(Vector2 center, float radius, Vector2 p1, Vector2 p2, out Vector2 i1, out Vector2 i2)
	{
		int num = LineCircleIntersections(center, radius, p1, p2, out i1, out i2);
		if (num == 0)
		{
			i1 = new Vector2(float.NaN, float.NaN);
			i2 = new Vector2(float.NaN, float.NaN);
			return 0;
		}
		Vector2 vector = Vector2.Min(p1, p2);
		Vector2 vector2 = Vector2.Max(p1, p2);
		int num2 = 0;
		if (num >= 1)
		{
			if (vector.x <= i1.x && vector2.x >= i1.x && vector.y <= i1.y && vector2.y >= i1.y)
			{
				num2++;
			}
			else
			{
				i1 = new Vector2(float.NaN, float.NaN);
			}
		}
		if (num >= 2)
		{
			if (vector.x <= i2.x && vector2.x >= i2.x && vector.y <= i2.y && vector2.y >= i2.y)
			{
				num2++;
				if (num2 == 1)
				{
					i1 = i2;
					i2 = new Vector2(float.NaN, float.NaN);
				}
			}
			else
			{
				i2 = new Vector2(float.NaN, float.NaN);
			}
		}
		return num2;
	}

	public static Vector2 ClosestLineCircleIntersect(Vector2 center, float radius, Vector2 lineStart, Vector2 lineEnd)
	{
		Vector2 i;
		Vector2 i2;
		switch (LineCircleIntersections(center, radius, lineStart, lineEnd, out i, out i2))
		{
		case 1:
			return i;
		case 2:
			return (!(Vector2.Distance(i, lineStart) < Vector2.Distance(i2, lineStart))) ? i2 : i;
		default:
			return Vector2.zero;
		}
	}

	public static float SliceProbability(float chancePerSecond, float tickTime)
	{
		return 1f - Mathf.Pow(1f - chancePerSecond, tickTime);
	}

	public static bool AABBContains(Vector2 min, Vector2 max, Vector2 point)
	{
		return point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;
	}

	public static float AABBDistance(Vector2 aMin, Vector2 aMax, Vector2 bMin, Vector2 bMax)
	{
		Vector2 a = new Vector2((bMin.x + bMax.x) / 2f, (bMin.y + bMax.y) / 2f);
		Vector2 b = new Vector2((aMin.x + aMax.x) / 2f, (aMin.y + aMax.y) / 2f);
		if (a.x < aMin.x)
		{
			a.x = aMin.x;
		}
		if (a.x > aMax.x)
		{
			a.x = aMax.x;
		}
		if (a.y < aMin.y)
		{
			a.y = aMin.y;
		}
		if (a.y > aMax.y)
		{
			a.y = aMax.y;
		}
		if (b.x < aMin.x)
		{
			b.x = aMin.x;
		}
		if (b.x > aMax.x)
		{
			b.x = aMax.x;
		}
		if (b.y < aMin.y)
		{
			b.y = aMin.y;
		}
		if (b.y > aMax.y)
		{
			b.y = aMax.y;
		}
		return Vector2.Distance(a, b);
	}

	public static float AABBDistanceSquared(Vector2 aMin, Vector2 aMax, Vector2 bMin, Vector2 bMax)
	{
		Vector2 vector = new Vector2((bMin.x + bMax.x) / 2f, (bMin.y + bMax.y) / 2f);
		Vector2 vector2 = new Vector2((aMin.x + aMax.x) / 2f, (aMin.y + aMax.y) / 2f);
		if (vector.x < aMin.x)
		{
			vector.x = aMin.x;
		}
		if (vector.x > aMax.x)
		{
			vector.x = aMax.x;
		}
		if (vector.y < aMin.y)
		{
			vector.y = aMin.y;
		}
		if (vector.y > aMax.y)
		{
			vector.y = aMax.y;
		}
		if (vector2.x < bMin.x)
		{
			vector2.x = bMin.x;
		}
		if (vector2.x > bMax.x)
		{
			vector2.x = bMax.x;
		}
		if (vector2.y < bMin.y)
		{
			vector2.y = bMin.y;
		}
		if (vector2.y > bMax.y)
		{
			vector2.y = bMax.y;
		}
		return Vector2.SqrMagnitude(vector - vector2);
	}

	public static Vector2 GetPredictedPosition(Vector2 targetOrigin, Vector2 targetVelocity, float time)
	{
		return targetOrigin + targetVelocity * time;
	}

	public static Vector2 GetPredictedPosition(Vector2 targetOrigin, Vector2 targetVelocity, Vector2 aimOrigin, float firingSpeed)
	{
		float magnitude = targetVelocity.magnitude;
		if (magnitude < 1E-05f)
		{
			return targetOrigin;
		}
		Vector2 vector = aimOrigin - targetOrigin;
		float num = targetVelocity.ToAngle() - vector.ToAngle();
		float num2 = Mathf.Asin(magnitude * Mathf.Sin(num * ((float)Math.PI / 180f)) / firingSpeed) * 57.29578f;
		if (float.IsNaN(num2))
		{
			return targetOrigin;
		}
		float num3 = ClampAngle360((targetOrigin - aimOrigin).ToAngle());
		float num4 = ClampAngle360(180f - num2 - num);
		if ((double)num4 < 0.0001 || num4 > 359.9999f)
		{
			return targetOrigin;
		}
		float num5 = Vector2.Distance(aimOrigin, targetOrigin) * Mathf.Sin(num2 * ((float)Math.PI / 180f)) / Mathf.Sin(num4 * ((float)Math.PI / 180f)) / magnitude;
		if (num5 < 0f)
		{
			return targetOrigin;
		}
		return aimOrigin + DegreesToVector(num3 - num2, firingSpeed * num5);
	}

	public static bool NextPermutation(ref int[] numList)
	{
		int num = -1;
		for (int num2 = numList.Length - 2; num2 >= 0; num2--)
		{
			if (numList[num2] < numList[num2 + 1])
			{
				num = num2;
				break;
			}
		}
		if (num < 0)
		{
			return false;
		}
		int num3 = -1;
		for (int num4 = numList.Length - 1; num4 >= 0; num4--)
		{
			if (numList[num] < numList[num4])
			{
				num3 = num4;
				break;
			}
		}
		int num5 = numList[num];
		numList[num] = numList[num3];
		numList[num3] = num5;
		int num6 = num + 1;
		int num7 = numList.Length - 1;
		while (num6 < num7)
		{
			num5 = numList[num6];
			numList[num6] = numList[num7];
			numList[num7] = num5;
			num6++;
			num7--;
		}
		return true;
	}

	public static Vector2 ClampToBounds(Vector2 value, Vector2 min, Vector2 max)
	{
		return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
	}

	public static Vector2 ClampSafe(Vector2 value, float min, float max)
	{
		return new Vector2(ClampSafe(value.x, min, max), ClampSafe(value.y, min, max));
	}

	public static float ClampSafe(float value, float min, float max)
	{
		if (float.IsNaN(value))
		{
			return 0f;
		}
		return Mathf.Clamp(value, min, max);
	}

	public static float WeightedAverage(float newValue, ref float prevAverage, ref int prevCount)
	{
		prevCount++;
		prevAverage = prevAverage * (((float)prevCount - 1f) / (float)prevCount) + newValue / (float)prevCount;
		return prevAverage;
	}

	public static Vector2 WeightedAverage(Vector2 newValue, ref Vector2 prevAverage, ref int prevCount)
	{
		prevCount++;
		prevAverage = prevAverage * (((float)prevCount - 1f) / (float)prevCount) + newValue / prevCount;
		return prevAverage;
	}

	public static float MovingAverage(float avg, float newValue, int n)
	{
		if (avg == 0f)
		{
			return newValue;
		}
		float num = 1f / (float)n;
		return avg + num * (newValue - avg);
	}

	public static Vector2 MovingAverage(Vector2 avg, Vector2 newValue, int n)
	{
		if (avg == Vector2.zero)
		{
			return newValue;
		}
		float num = 1f / (float)n;
		return avg + num * (newValue - avg);
	}

	public static float MovingAverageSpeed(float movingAverage, float newSpeed, float newDeltaTime, float n)
	{
		if (newDeltaTime <= 0f)
		{
			return movingAverage;
		}
		if (movingAverage == 0f || newDeltaTime >= n)
		{
			return newSpeed;
		}
		float num = newDeltaTime / n;
		return movingAverage + num * (newSpeed - movingAverage);
	}

	public static Vector3 LShapedMoveTowards(Vector3 current, Vector3 target, float maxDeltaX, float maxDeltaY)
	{
		if (Mathf.RoundToInt(current.x) != Mathf.RoundToInt(target.x) && Mathf.RoundToInt(current.y) != Mathf.RoundToInt(target.y))
		{
			if (target.y > current.y)
			{
				return Vector3.MoveTowards(current, target.WithX(current.x), maxDeltaX);
			}
			return Vector3.MoveTowards(current, target.WithY(current.y), maxDeltaY);
		}
		if (Mathf.RoundToInt(current.y) == Mathf.RoundToInt(target.y))
		{
			return Vector3.MoveTowards(current, target, maxDeltaX);
		}
		return Vector3.MoveTowards(current, target, maxDeltaY);
	}

	public static bool IsAngleWithinSweepArea(float testAngle, float startAngle, float sweepAngle)
	{
		if (sweepAngle > 360f || sweepAngle < -360f)
		{
			return true;
		}
		float num = Mathf.Sign(sweepAngle);
		float num2 = ClampAngle180(testAngle - startAngle);
		if (Mathf.Sign(num2) != num)
		{
			num2 += num * 360f;
		}
		if (num > 0f)
		{
			return num2 < sweepAngle;
		}
		return num2 > sweepAngle;
	}

	public static Vector2 GetEllipsePoint(Vector2 center, float a, float b, float angle)
	{
		Vector2 result = center;
		float num = ClampAngle360(angle);
		float num2 = Mathf.Tan(num * ((float)Math.PI / 180f));
		float num3 = ((!(num >= 90f) || !(num < 270f)) ? 1 : (-1));
		float num4 = Mathf.Sqrt(b * b + a * a * (num2 * num2));
		result.x += num3 * a * b / num4;
		result.y += num3 * a * b * num2 / num4;
		return result;
	}

	public static Vector2 GetEllipsePointSmooth(Vector2 center, float a, float b, float angle)
	{
		return center + Vector2.Scale(new Vector2(a, b), DegreesToVector(angle));
	}
}
