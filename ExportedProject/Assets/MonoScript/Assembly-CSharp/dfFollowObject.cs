using System;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Follow Object")]
public class dfFollowObject : MonoBehaviour
{
	public Camera mainCamera;

	public GameObject attach;

	public dfPivotPoint anchor = dfPivotPoint.MiddleCenter;

	public Vector3 offset;

	public float hideDistance = 20f;

	public float fadeDistance = 15f;

	public bool constantScale;

	public bool stickToScreenEdge;

	[HideInInspector]
	public bool overrideVisibility = true;

	private Transform followTransform;

	private dfControl myControl;

	private dfGUIManager manager;

	private void OnEnable()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				Debug.LogError("dfFollowObject component is unable to determine which camera is the MainCamera", base.gameObject);
				base.enabled = false;
				return;
			}
		}
		myControl = GetComponent<dfControl>();
		if (myControl == null)
		{
			Debug.LogError("No dfControl component on this GameObject: " + base.gameObject.name, base.gameObject);
			base.enabled = false;
		}
		if (myControl == null || attach == null)
		{
			Debug.LogWarning("Configuration incomplete: " + base.name);
			base.enabled = false;
			return;
		}
		followTransform = attach.transform;
		manager = myControl.GetManager();
		dfFollowObjectSorter.Register(this);
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		mainCameraController.OnFinishedFrame = (Action)Delegate.Remove(mainCameraController.OnFinishedFrame, new Action(OnMainCameraFinishedFrame));
		CameraController mainCameraController2 = GameManager.Instance.MainCameraController;
		mainCameraController2.OnFinishedFrame = (Action)Delegate.Combine(mainCameraController2.OnFinishedFrame, new Action(OnMainCameraFinishedFrame));
	}

	private void OnMainCameraFinishedFrame()
	{
	}

	private void OnDisable()
	{
		if (GameManager.HasInstance)
		{
			dfFollowObjectSorter.Unregister(this);
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.OnFinishedFrame = (Action)Delegate.Remove(mainCameraController.OnFinishedFrame, new Action(OnMainCameraFinishedFrame));
		}
	}

	private void OnDestroy()
	{
		if (GameManager.HasInstance && (bool)GameManager.Instance.MainCameraController)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.OnFinishedFrame = (Action)Delegate.Remove(mainCameraController.OnFinishedFrame, new Action(OnMainCameraFinishedFrame));
		}
	}

	public void ForceUpdate()
	{
		OnEnable();
		Update();
	}

	public static Vector3 ConvertWorldSpaces(Vector3 inPoint, Camera inCamera, Camera outCamera)
	{
		Vector3 position = inCamera.WorldToViewportPoint(inPoint);
		return outCamera.ViewportToWorldPoint(position);
	}

	private void Update()
	{
		if (!followTransform)
		{
			base.enabled = false;
			return;
		}
		base.transform.position = ConvertWorldSpaces(followTransform.position + offset, mainCamera, manager.RenderCamera).WithZ(0f);
		base.transform.position = base.transform.position.QuantizeFloor(myControl.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
	}

	private Vector2 getAnchoredControlPosition()
	{
		float height = myControl.Height;
		float x = myControl.Width / 2f;
		float width = myControl.Width;
		float y = myControl.Height / 2f;
		Vector2 result = default(Vector2);
		switch (anchor)
		{
		case dfPivotPoint.TopLeft:
			result.x = width;
			result.y = height;
			break;
		case dfPivotPoint.TopCenter:
			result.x = x;
			result.y = height;
			break;
		case dfPivotPoint.TopRight:
			result.x = 0f;
			result.y = height;
			break;
		case dfPivotPoint.MiddleLeft:
			result.x = width;
			result.y = y;
			break;
		case dfPivotPoint.MiddleCenter:
			result.x = x;
			result.y = y;
			break;
		case dfPivotPoint.MiddleRight:
			result.x = 0f;
			result.y = y;
			break;
		case dfPivotPoint.BottomLeft:
			result.x = width;
			result.y = 0f;
			break;
		case dfPivotPoint.BottomCenter:
			result.x = x;
			result.y = 0f;
			break;
		case dfPivotPoint.BottomRight:
			result.x = 0f;
			result.y = 0f;
			break;
		}
		return result;
	}
}
