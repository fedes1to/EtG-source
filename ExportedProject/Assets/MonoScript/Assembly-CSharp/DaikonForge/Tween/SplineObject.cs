using System.Collections.Generic;
using System.Linq;
//using DaikonForge.Editor;
using UnityEngine;

namespace DaikonForge.Tween
{
	[AddComponentMenu("Daikon Forge/Tween/Spline Path")]
	//[InspectorGroupOrder(new string[] { "General", "Path", "Control Points" })]
	[ExecuteInEditMode]
	public class SplineObject : MonoBehaviour
	{
		public Spline Spline;

	//	[Inspector("Path", Order = 0, Tooltip = "If set to TRUE, the end of the path will wrap around to the beginning")]
		public bool Wrap;

	//	[Inspector("Control Points", Order = 1, Tooltip = "Contains the list of Transforms that represent the control points of the path's curve")]
		public List<Transform> ControlPoints = new List<Transform>();

		public void Awake()
		{
			if (ControlPoints.Count == 0)
			{
				SplineNode[] componentsInChildren = base.transform.GetComponentsInChildren<SplineNode>();
				ControlPoints.AddRange(componentsInChildren.Select((SplineNode x) => x.transform).ToList());
			}
			CalculateSpline();
		}

		public Vector3 Evaluate(float time)
		{
			return Spline.GetPosition(time);
		}

		public float GetTimeAtNode(int nodeIndex)
		{
			CalculateSpline();
			Spline.ControlPoint controlPoint = Spline.ControlPoints[nodeIndex];
			return controlPoint.Time;
		}

		public SplineNode AddNode()
		{
			CalculateSpline();
			Vector3 position = base.transform.position;
			if (Spline.ControlPoints.Count > 2)
			{
				Vector3 position2 = Spline.ControlPoints.Last().Position;
				Vector3 tangent = Spline.GetTangent(0.95f);
				position = position2 + tangent;
			}
			return AddNode(position);
		}

		public SplineNode AddNode(Vector3 position)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "SplineNode" + Spline.ControlPoints.Count;
			GameObject gameObject2 = gameObject;
			SplineNode result = gameObject2.AddComponent<SplineNode>();
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.position = position;
			ControlPoints.Add(gameObject2.transform);
			CalculateSpline();
			return result;
		}

		public void CalculateSpline()
		{
			int num = 0;
			while (num < ControlPoints.Count)
			{
				if (ControlPoints[num] == null)
				{
					ControlPoints.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (Spline == null)
			{
				Spline = new Spline();
			}
			Spline.Wrap = Wrap;
			Spline.ControlPoints.Clear();
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				Transform transform = ControlPoints[i];
				if (!(transform == null))
				{
					Spline.ControlPoints.Add(new Spline.ControlPoint(transform.position, transform.forward));
				}
			}
			Spline.ComputeSpline();
		}

		public Bounds GetBounds()
		{
			Vector3 vector = Vector3.one * float.MaxValue;
			Vector3 vector2 = Vector3.one * float.MinValue;
			int num = 0;
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				if (!(ControlPoints[i] == null))
				{
					num++;
					Vector3 position = ControlPoints[i].transform.position;
					vector = Vector3.Min(vector, position);
					vector2 = Vector3.Max(vector2, position);
				}
			}
			if (num == 0)
			{
				return new Bounds(base.transform.position, Vector3.zero);
			}
			Vector3 vector3 = vector2 - vector;
			return new Bounds(vector + vector3 * 0.5f, vector3);
		}
	}
}
