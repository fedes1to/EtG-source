using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Drag Handle")]
[ExecuteInEditMode]
public class dfDragHandle : dfControl
{
	private Vector3 lastPosition;

	public override void Start()
	{
		base.Start();
		if (base.Size.magnitude <= float.Epsilon)
		{
			if (base.Parent != null)
			{
				base.Size = new Vector2(base.Parent.Width, 30f);
				base.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Left | dfAnchorStyle.Right;
				base.RelativePosition = Vector2.zero;
			}
			else
			{
				base.Size = new Vector2(200f, 25f);
			}
		}
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		GetRootContainer().BringToFront();
		base.Parent.BringToFront();
		args.Use();
		Plane plane = new Plane(parent.transform.TransformDirection(Vector3.back), parent.transform.position);
		Ray ray = args.Ray;
		float enter = 0f;
		plane.Raycast(args.Ray, out enter);
		lastPosition = ray.origin + ray.direction * enter;
		base.OnMouseDown(args);
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		args.Use();
		if (args.Buttons.IsSet(dfMouseButtons.Left))
		{
			Ray ray = args.Ray;
			float enter = 0f;
			Vector3 inNormal = GetCamera().transform.TransformDirection(Vector3.back);
			new Plane(inNormal, lastPosition).Raycast(ray, out enter);
			Vector3 vector = (ray.origin + ray.direction * enter).Quantize(parent.PixelsToUnits());
			Vector3 vector2 = vector - lastPosition;
			Vector3 position = (parent.transform.position + vector2).Quantize(parent.PixelsToUnits());
			parent.transform.position = position;
			lastPosition = vector;
		}
		base.OnMouseMove(args);
	}

	protected internal override void OnMouseUp(dfMouseEventArgs args)
	{
		base.OnMouseUp(args);
		base.Parent.MakePixelPerfect();
	}
}
