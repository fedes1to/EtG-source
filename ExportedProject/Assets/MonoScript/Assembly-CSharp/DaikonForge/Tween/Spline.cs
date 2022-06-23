using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class Spline : IPathIterator, IEnumerable<Spline.ControlPoint>, IEnumerable
	{
		public class ControlPoint
		{
			public Vector3 Position;

			public Vector3 Orientation;

			public float Length;

			public float Time;

			public ControlPoint(Vector3 Position, Vector3 Tangent)
			{
				this.Position = Position;
				Orientation = Tangent;
				Length = 0f;
			}
		}

		public ISplineInterpolator SplineMethod = new CatmullRomSpline();

		public List<ControlPoint> ControlPoints = new List<ControlPoint>();

		public bool Wrap;

		private float length;

		public float Length
		{
			get
			{
				return length;
			}
		}

		public Vector3 GetPosition(float time)
		{
			time = Mathf.Abs(time) % 1f;
			float num = 1f / (float)ControlPoints.Count;
			if (!Wrap)
			{
				time *= num * (float)(ControlPoints.Count - 1);
			}
			int num2 = Mathf.FloorToInt(time * (float)ControlPoints.Count);
			ControlPoint node = getNode(num2 - 1);
			ControlPoint node2 = getNode(num2);
			ControlPoint node3 = getNode(num2 + 1);
			ControlPoint node4 = getNode(num2 + 2);
			float num3 = num * (float)num2;
			time -= num3;
			time /= num;
			return SplineMethod.Evaluate(node.Position, node2.Position, node3.Position, node4.Position, time);
		}

		public float AdjustTimeToConstant(float time)
		{
			if (time < 0f || time > 1f)
			{
				throw new ArgumentException("The length parameter must be a value between 0 and 1 (inclusive)");
			}
			int parameterIndex = getParameterIndex(time);
			int num = ControlPoints.Count + ((!Wrap) ? (-1) : 0);
			float num2 = 1f / (float)num;
			ControlPoint controlPoint = ControlPoints[parameterIndex];
			float num3 = controlPoint.Length / length;
			float num4 = (time - controlPoint.Time) / num3;
			return (float)parameterIndex * num2 + num4 * num2;
		}

		public Vector3 GetOrientation(float time)
		{
			int num = ControlPoints.Count - (Wrap ? 1 : 2);
			while (ControlPoints[num].Time > time)
			{
				num--;
			}
			int index = ((num != ControlPoints.Count - 1) ? (num + 1) : 0);
			ControlPoint controlPoint = ControlPoints[num];
			ControlPoint controlPoint2 = ControlPoints[index];
			float num2 = controlPoint.Length / length;
			float t = (time - controlPoint.Time) / num2;
			return Vector3.Lerp(controlPoint.Orientation, controlPoint2.Orientation, t);
		}

		public Vector3 GetTangent(float time)
		{
			return GetTangent(time, 0.01f);
		}

		public Vector3 GetTangent(float time, float lookAhead)
		{
			if (!Wrap && time > 1f - lookAhead)
			{
				time = 1f - lookAhead;
			}
			Vector3 position = GetPosition(time);
			Vector3 position2 = GetPosition(time + lookAhead);
			Vector3 result = position2 - position;
			result.Normalize();
			return result;
		}

		public Spline AddNode(ControlPoint node)
		{
			ControlPoints.Add(node);
			return this;
		}

		public void ComputeSpline()
		{
			length = 0f;
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				ControlPoints[i].Time = 0f;
				ControlPoints[i].Length = 0f;
			}
			if (ControlPoints.Count < 2)
			{
				return;
			}
			int num = 16;
			int num2 = ControlPoints.Count + ((!Wrap) ? (-1) : 0);
			float num3 = 1f / (float)num2;
			float num4 = num3 / (float)num;
			for (int j = 0; j < num2; j++)
			{
				float num5 = (float)j * num3;
				Vector3 a = ControlPoints[j].Position;
				for (int k = 1; k < num; k++)
				{
					Vector3 position = GetPosition(num5 + (float)k * num4);
					ControlPoints[j].Length += Vector3.Distance(a, position);
					a = position;
				}
			}
			length = 0f;
			for (int l = 0; l < ControlPoints.Count; l++)
			{
				length += ControlPoints[l].Length;
			}
			float num6 = 0f;
			for (int m = 0; m < num2; m++)
			{
				ControlPoints[m].Time = num6 / length;
				num6 += ControlPoints[m].Length;
			}
			if (!Wrap)
			{
				ControlPoints[ControlPoints.Count - 1].Time = 1f;
			}
		}

		private int getParameterIndex(float time)
		{
			int result = 0;
			for (int i = 1; i < ControlPoints.Count; i++)
			{
				ControlPoint controlPoint = ControlPoints[i];
				if (controlPoint.Length == 0f || controlPoint.Time > time)
				{
					break;
				}
				result = i;
			}
			return result;
		}

		private ControlPoint getNode(int nodeIndex)
		{
			if (Wrap)
			{
				if (nodeIndex < 0)
				{
					nodeIndex += ControlPoints.Count;
				}
				if (nodeIndex >= ControlPoints.Count)
				{
					nodeIndex -= ControlPoints.Count;
				}
			}
			else
			{
				nodeIndex = Mathf.Clamp(nodeIndex, 0, ControlPoints.Count - 1);
			}
			return ControlPoints[nodeIndex];
		}

		private IEnumerator<ControlPoint> GetCustomEnumerator()
		{
			int index = 0;
			while (index < ControlPoints.Count)
			{
				yield return ControlPoints[index++];
			}
		}

		public IEnumerator<ControlPoint> GetEnumerator()
		{
			return GetCustomEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetCustomEnumerator();
		}
	}
}
