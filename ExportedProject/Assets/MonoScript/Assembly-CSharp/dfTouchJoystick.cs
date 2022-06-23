using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class dfTouchJoystick : MonoBehaviour
{
	public enum TouchJoystickType
	{
		Joystick,
		Trackpad
	}

	private static Dictionary<string, dfTouchJoystick> joysticks = new Dictionary<string, dfTouchJoystick>();

	[SerializeField]
	public string JoystickID = "Joystick";

	[SerializeField]
	public TouchJoystickType JoystickType;

	[SerializeField]
	public int Radius = 80;

	[SerializeField]
	public float DeadzoneRadius = 0.25f;

	[SerializeField]
	public bool DynamicThumb;

	[SerializeField]
	public bool HideThumb;

	[SerializeField]
	public dfControl ThumbControl;

	[SerializeField]
	public dfControl AreaControl;

	private dfControl control;

	private Vector2 joystickPos = Vector2.zero;

	public Vector2 Position
	{
		get
		{
			return joystickPos;
		}
	}

	public static Vector2 GetJoystickPosition(string joystickID)
	{
		if (!joysticks.ContainsKey(joystickID))
		{
			throw new Exception("Joystick not registered: " + joystickID);
		}
		return joysticks[joystickID].Position;
	}

	public static void ResetJoystickPosition(string joystickID)
	{
		if (!joysticks.ContainsKey(joystickID))
		{
			throw new Exception("Joystick not registered: " + joystickID);
		}
		dfTouchJoystick dfTouchJoystick2 = joysticks[joystickID];
		if (dfTouchJoystick2.JoystickType == TouchJoystickType.Trackpad)
		{
			dfTouchJoystick2.joystickPos = Vector2.zero;
		}
		else
		{
			dfTouchJoystick2.recenter();
		}
	}

	public void Start()
	{
		control = GetComponent<dfControl>();
		if ((JoystickType != TouchJoystickType.Trackpad || !(control != null)) && (!(control != null) || !(ThumbControl != null) || !(AreaControl != null)))
		{
			Debug.LogError("Invalid virtual joystick configuration", this);
			base.enabled = false;
			return;
		}
		joysticks.Add(JoystickID, this);
		if (ThumbControl != null && HideThumb)
		{
			ThumbControl.Hide();
			if (DynamicThumb)
			{
				AreaControl.Hide();
			}
		}
		recenter();
	}

	public void OnDestroy()
	{
		joysticks.Remove(JoystickID);
	}

	public void OnMouseDown(dfControl control, dfMouseEventArgs args)
	{
		if (JoystickType != TouchJoystickType.Trackpad)
		{
			Vector2 position;
			control.GetHitPosition(args.Ray, out position, true);
			if (HideThumb)
			{
				ThumbControl.Show();
				AreaControl.Show();
			}
			if (DynamicThumb)
			{
				AreaControl.RelativePosition = position - AreaControl.Size * 0.5f;
				centerThumbInArea();
			}
			else
			{
				recenter();
			}
			processTouch(args);
		}
	}

	public void OnMouseHover()
	{
		if (JoystickType == TouchJoystickType.Trackpad)
		{
			joystickPos = Vector2.zero;
		}
	}

	public void OnMouseMove(dfControl control, dfMouseEventArgs args)
	{
		if (JoystickType == TouchJoystickType.Trackpad && args.Buttons.IsSet(dfMouseButtons.Left))
		{
			joystickPos = args.MoveDelta * 0.25f;
		}
		else if (args.Buttons.IsSet(dfMouseButtons.Left))
		{
			processTouch(args);
		}
	}

	public void OnMouseUp(dfControl control, dfMouseEventArgs args)
	{
		if (JoystickType == TouchJoystickType.Trackpad)
		{
			joystickPos = Vector2.zero;
			return;
		}
		recenter();
		if (HideThumb)
		{
			ThumbControl.Hide();
			if (DynamicThumb)
			{
				AreaControl.Hide();
			}
		}
	}

	private void recenter()
	{
		if (JoystickType != TouchJoystickType.Trackpad)
		{
			AreaControl.RelativePosition = (control.Size - AreaControl.Size) * 0.5f;
			Vector3 vector = AreaControl.RelativePosition + (Vector3)AreaControl.Size * 0.5f;
			Vector3 vector2 = (Vector3)ThumbControl.Size * 0.5f;
			ThumbControl.RelativePosition = vector - vector2;
			joystickPos = Vector2.zero;
		}
	}

	private void centerThumbInArea()
	{
		ThumbControl.RelativePosition = AreaControl.RelativePosition + (Vector3)(AreaControl.Size - ThumbControl.Size) * 0.5f;
	}

	private void processTouch(dfMouseEventArgs evt)
	{
		Vector2 vector = raycast(evt.Ray);
		Vector3 vector2 = AreaControl.RelativePosition + (Vector3)AreaControl.Size * 0.5f;
		Vector3 vector3 = (Vector3)vector - vector2;
		if (vector3.magnitude > (float)Radius)
		{
			vector3 = vector3.normalized * Radius;
		}
		Vector3 vector4 = (Vector3)ThumbControl.Size * 0.5f;
		ThumbControl.RelativePosition = vector2 - vector4 + vector3;
		vector3 /= (float)Radius;
		if (vector3.magnitude <= DeadzoneRadius)
		{
			joystickPos = Vector2.zero;
		}
		else
		{
			joystickPos = new Vector3(vector3.x, 0f - vector3.y);
		}
	}

	private Vector2 raycast(Ray ray)
	{
		Vector3[] corners = control.GetCorners();
		Plane plane = new Plane(corners[0], corners[1], corners[3]);
		float enter = 0f;
		plane.Raycast(ray, out enter);
		Vector3 point = ray.GetPoint(enter);
		Vector3 vector = (point - corners[0]).Scale(1f, -1f, 0f) / control.GetManager().PixelsToUnits();
		return vector;
	}
}
