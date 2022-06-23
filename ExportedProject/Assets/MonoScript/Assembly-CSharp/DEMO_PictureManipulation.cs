using System.Collections;
using UnityEngine;

public class DEMO_PictureManipulation : MonoBehaviour
{
	private dfTextureSprite control;

	private bool isMouseDown;

	public void Start()
	{
		control = GetComponent<dfTextureSprite>();
	}

	protected void OnMouseUp()
	{
		isMouseDown = false;
	}

	protected void OnMouseDown()
	{
		isMouseDown = true;
		control.BringToFront();
	}

	public IEnumerator OnFlickGesture(dfGestureBase gesture)
	{
		return handleMomentum(gesture);
	}

	public IEnumerator OnDoubleTapGesture()
	{
		return resetZoom();
	}

	public void OnRotateGestureBegin(dfRotateGesture gesture)
	{
		rotateAroundPoint(gesture.StartPosition, gesture.AngleDelta * 0.5f);
	}

	public void OnRotateGestureUpdate(dfRotateGesture gesture)
	{
		rotateAroundPoint(gesture.CurrentPosition, gesture.AngleDelta * 0.5f);
	}

	public void OnResizeGestureUpdate(dfResizeGesture gesture)
	{
		zoomToPoint(gesture.StartPosition, gesture.SizeDelta * 1f);
	}

	public void OnPanGestureStart(dfPanGesture gesture)
	{
		control.BringToFront();
		control.RelativePosition += (Vector3)gesture.Delta.Scale(1f, -1f);
	}

	public void OnPanGestureMove(dfPanGesture gesture)
	{
		control.RelativePosition += (Vector3)gesture.Delta.Scale(1f, -1f);
	}

	public void OnMouseWheel(dfControl sender, dfMouseEventArgs args)
	{
		zoomToPoint(args.Position, Mathf.Sign(args.WheelDelta) * 75f);
	}

	private IEnumerator resetZoom()
	{
		Vector2 controlSize = control.Size;
		Vector2 screenSize = control.GetManager().GetScreenSize();
		dfAnimatedVector2 animatedSize = new dfAnimatedVector2(control.Size, controlSize, 0.2f);
		if (controlSize.x >= screenSize.x - 10f || controlSize.y >= screenSize.y - 10f)
		{
			animatedSize.EndValue = fitImage(screenSize.x * 0.75f, screenSize.y * 0.75f, control.Width, control.Height);
		}
		else
		{
			animatedSize.EndValue = fitImage(screenSize.x, screenSize.y, control.Width, control.Height);
		}
		dfAnimatedVector3 animatedPosition = new dfAnimatedVector3(EndValue: new Vector3(screenSize.x - animatedSize.EndValue.x, screenSize.y - animatedSize.EndValue.y, 0f) * 0.5f, StartValue: control.RelativePosition, Time: animatedSize.Length);
		dfAnimatedQuaternion animatedRotation = new dfAnimatedQuaternion(control.transform.rotation, Quaternion.identity, animatedSize.Length);
		while (!animatedSize.IsDone)
		{
			control.Size = animatedSize;
			control.RelativePosition = animatedPosition;
			control.transform.rotation = animatedRotation;
			yield return null;
		}
	}

	private IEnumerator handleMomentum(dfGestureBase gesture)
	{
		isMouseDown = false;
		Vector3 direction = (Vector3)(gesture.CurrentPosition - gesture.StartPosition) * control.PixelsToUnits();
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(control.GetCamera());
		float startTime = Time.realtimeSinceStartup;
		while (!isMouseDown)
		{
			float timeNow = Time.realtimeSinceStartup;
			float elapsed = timeNow - startTime;
			if (elapsed > 1f)
			{
				break;
			}
			control.transform.position += direction * BraveTime.DeltaTime * 10f * (1f - elapsed);
			yield return null;
		}
		if (!GeometryUtility.TestPlanesAABB(planes, control.GetComponent<Collider>().bounds))
		{
			control.enabled = false;
			Object.DestroyImmediate(base.gameObject);
		}
	}

	private void rotateAroundPoint(Vector2 point, float delta)
	{
		Transform transform = control.transform;
		Vector3[] corners = control.GetCorners();
		Plane plane = new Plane(corners[0], corners[1], corners[2]);
		Ray ray = control.GetCamera().ScreenPointToRay(point);
		float enter = 0f;
		plane.Raycast(ray, out enter);
		Vector3 point2 = ray.GetPoint(enter);
		Vector3 vector = new Vector3(0f, 0f, delta);
		Vector3 eulerAngles = transform.eulerAngles + vector;
		Vector3 vector3 = (transform.position = rotatePointAroundPivot(transform.position, point2, vector));
		transform.eulerAngles = eulerAngles;
	}

	private Vector3 rotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 vector = Quaternion.Euler(angles) * (point - pivot);
		return vector + pivot;
	}

	private void zoomToPoint(Vector2 point, float delta)
	{
		Transform transform = control.transform;
		Vector3[] corners = control.GetCorners();
		float num = control.PixelsToUnits();
		Plane plane = new Plane(corners[0], corners[1], corners[2]);
		Ray ray = control.GetCamera().ScreenPointToRay(point);
		float enter = 0f;
		plane.Raycast(ray, out enter);
		Vector3 point2 = ray.GetPoint(enter);
		Vector2 vector = control.Size * num;
		Vector3 vector2 = transform.position - point2;
		Vector3 vector3 = new Vector3(vector2.x / vector.x, vector2.y / vector.y);
		float num2 = control.Height / control.Width;
		float num3 = control.Width + delta;
		float num4 = num3 * num2;
		if (!(num3 < 256f) && !(num4 < 256f))
		{
			control.Size = new Vector2(num3, num4);
			Vector3 vector4 = new Vector3(control.Width * vector3.x * num, control.Height * vector3.y * num, vector2.z);
			transform.position += vector4 - vector2;
		}
	}

	private static Vector2 fitImage(float maxWidth, float maxHeight, float imageWidth, float imageHeight)
	{
		float a = maxWidth / imageWidth;
		float b = maxHeight / imageHeight;
		float num = Mathf.Min(a, b);
		return new Vector2(Mathf.Floor(imageWidth * num), Mathf.Ceil(imageHeight * num));
	}
}
