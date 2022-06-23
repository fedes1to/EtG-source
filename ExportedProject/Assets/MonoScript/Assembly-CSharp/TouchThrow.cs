using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Touch/Touch Throw")]
public class TouchThrow : MonoBehaviour
{
	private dfControl control;

	private dfGUIManager manager;

	private Vector2 momentum;

	private Vector3 lastPosition;

	private bool animating;

	private bool dragging;

	public void Start()
	{
		control = GetComponent<dfControl>();
		manager = control.GetManager();
	}

	public void Update()
	{
		Vector2 screenSize = control.GetManager().GetScreenSize();
		Vector2 vector = control.RelativePosition;
		Vector2 rhs = vector;
		if (animating)
		{
			if (vector.x + momentum.x < 0f || vector.x + momentum.x + control.Width > screenSize.x)
			{
				momentum.x *= -1f;
			}
			if (vector.y + momentum.y < 0f || vector.y + momentum.y + control.Height > screenSize.y)
			{
				momentum.y *= -1f;
			}
			rhs += momentum;
			momentum *= 1f - Time.fixedDeltaTime;
		}
		rhs = Vector2.Max(Vector2.zero, rhs);
		rhs = Vector2.Min(screenSize - control.Size, rhs);
		if (Vector2.Distance(rhs, vector) > float.Epsilon)
		{
			control.RelativePosition = rhs;
		}
	}

	public void OnMultiTouch(dfControl control, dfTouchEventArgs touchData)
	{
		momentum = Vector2.zero;
		control.Color = Color.yellow;
		dfTouchInfo dfTouchInfo2 = touchData.Touches[0];
		dfTouchInfo dfTouchInfo3 = touchData.Touches[1];
		Vector2 vector = (dfTouchInfo2.deltaPosition * (BraveTime.DeltaTime / dfTouchInfo2.deltaTime)).Scale(1f, -1f);
		Vector2 vector2 = (dfTouchInfo3.deltaPosition * (BraveTime.DeltaTime / dfTouchInfo3.deltaTime)).Scale(1f, -1f);
		Vector2 vector3 = screenToGUI(dfTouchInfo2.position);
		Vector2 vector4 = screenToGUI(dfTouchInfo3.position);
		Vector2 vector5 = vector3 - vector4;
		Vector2 vector6 = vector3 - vector - (vector4 - vector2);
		float num = vector5.magnitude - vector6.magnitude;
		if (Mathf.Abs(num) > float.Epsilon)
		{
			Vector3 vector7 = Vector3.Min(vector3, vector4);
			Vector3 vector8 = vector7 - control.RelativePosition;
			control.Size += Vector2.one * num;
			control.RelativePosition = vector7 + vector8;
		}
	}

	private Vector2 screenToGUI(Vector2 position)
	{
		position.y = manager.GetScreenSize().y - position.y;
		return position;
	}

	public void OnMouseMove(dfControl control, dfMouseEventArgs args)
	{
		if (!animating && dragging)
		{
			momentum = (momentum + args.MoveDelta.Scale(1f, -1f)) * 0.5f;
			args.Use();
			if (args.Buttons.IsSet(dfMouseButtons.Left))
			{
				Ray ray = args.Ray;
				float enter = 0f;
				Vector3 inNormal = Camera.main.transform.TransformDirection(Vector3.back);
				new Plane(inNormal, lastPosition).Raycast(ray, out enter);
				Vector3 vector = (ray.origin + ray.direction * enter).Quantize(control.PixelsToUnits());
				Vector3 vector2 = vector - lastPosition;
				Vector3 position = (control.transform.position + vector2).Quantize(control.PixelsToUnits());
				control.transform.position = position;
				lastPosition = vector;
			}
		}
	}

	public void OnMouseEnter(dfControl control, dfMouseEventArgs args)
	{
		control.Color = Color.white;
	}

	public void OnMouseDown(dfControl control, dfMouseEventArgs args)
	{
		control.BringToFront();
		animating = false;
		momentum = Vector2.zero;
		dragging = true;
		args.Use();
		Plane plane = new Plane(control.transform.TransformDirection(Vector3.back), control.transform.position);
		Ray ray = args.Ray;
		float enter = 0f;
		plane.Raycast(args.Ray, out enter);
		lastPosition = ray.origin + ray.direction * enter;
	}

	public void OnMouseUp()
	{
		animating = true;
		dragging = false;
		control.Color = Color.white;
	}
}
