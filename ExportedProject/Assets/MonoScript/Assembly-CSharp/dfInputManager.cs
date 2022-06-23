using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Input Manager")]
public class dfInputManager : MonoBehaviour
{
	private class TouchInputManager
	{
		private class ControlTouchTracker
		{
			public readonly dfControl control;

			public readonly Dictionary<int, TouchRaycast> touches = new Dictionary<int, TouchRaycast>();

			public readonly List<int> capture = new List<int>();

			private dfInputManager manager;

			private Vector3 controlStartPosition;

			private dfDragDropState dragState;

			private object dragData;

			public bool IsDragging
			{
				get
				{
					return dragState == dfDragDropState.Dragging;
				}
			}

			public int TouchCount
			{
				get
				{
					return touches.Count;
				}
			}

			public ControlTouchTracker(dfInputManager manager, dfControl control)
			{
				this.manager = manager;
				this.control = control;
				controlStartPosition = control.transform.position;
			}

			public bool IsTrackingFinger(int fingerID)
			{
				return touches.ContainsKey(fingerID);
			}

			public bool Process(TouchRaycast info)
			{
				if (IsDragging)
				{
					if (!capture.Contains(info.FingerID))
					{
						return false;
					}
					if (info.Phase == TouchPhase.Stationary)
					{
						return true;
					}
					if (info.Phase == TouchPhase.Canceled)
					{
						control.OnDragEnd(new dfDragEventArgs(control, dfDragDropState.Cancelled, dragData, info.ray, info.position));
						dragState = dfDragDropState.None;
						touches.Clear();
						capture.Clear();
						return true;
					}
					if (info.Phase == TouchPhase.Ended)
					{
						if (info.control == null || info.control == control)
						{
							control.OnDragEnd(new dfDragEventArgs(control, dfDragDropState.CancelledNoTarget, dragData, info.ray, info.position));
							dragState = dfDragDropState.None;
							touches.Clear();
							capture.Clear();
							return true;
						}
						dfDragEventArgs dfDragEventArgs2 = new dfDragEventArgs(info.control, dfDragDropState.Dragging, dragData, info.ray, info.position);
						info.control.OnDragDrop(dfDragEventArgs2);
						if (!dfDragEventArgs2.Used || dfDragEventArgs2.State != dfDragDropState.Dropped)
						{
							dfDragEventArgs2.State = dfDragDropState.Cancelled;
						}
						dfDragEventArgs dfDragEventArgs3 = new dfDragEventArgs(control, dfDragEventArgs2.State, dragData, info.ray, info.position);
						dfDragEventArgs3.Target = info.control;
						control.OnDragEnd(dfDragEventArgs3);
						dragState = dfDragDropState.None;
						touches.Clear();
						capture.Clear();
						return true;
					}
					return true;
				}
				if (!touches.ContainsKey(info.FingerID))
				{
					if (info.control != control)
					{
						return false;
					}
					touches[info.FingerID] = info;
					if (touches.Count == 1)
					{
						control.OnMouseEnter((dfTouchEventArgs)info);
						if (info.Phase == TouchPhase.Began)
						{
							capture.Add(info.FingerID);
							controlStartPosition = control.transform.position;
							control.OnMouseDown((dfTouchEventArgs)info);
							if (Event.current != null)
							{
								Event.current.Use();
							}
						}
						return true;
					}
					if (info.Phase == TouchPhase.Began)
					{
						List<dfTouchInfo> activeTouches = getActiveTouches();
						dfTouchEventArgs args = new dfTouchEventArgs(control, activeTouches, info.ray);
						control.OnMultiTouch(args);
					}
					return true;
				}
				if (info.Phase == TouchPhase.Canceled || info.Phase == TouchPhase.Ended)
				{
					TouchPhase phase = info.Phase;
					TouchRaycast touch = touches[info.FingerID];
					touches.Remove(info.FingerID);
					if (touches.Count == 0 && phase != TouchPhase.Canceled)
					{
						if (capture.Contains(info.FingerID))
						{
							if (canFireClickEvent(info, touch) && info.control == control)
							{
								if (info.touch.tapCount > 1)
								{
									control.OnDoubleClick((dfTouchEventArgs)info);
								}
								else
								{
									control.OnClick((dfTouchEventArgs)info);
								}
							}
							info.control = control;
							if ((bool)control)
							{
								control.OnMouseUp((dfTouchEventArgs)info);
							}
						}
						capture.Remove(info.FingerID);
						return true;
					}
					capture.Remove(info.FingerID);
					if (touches.Count == 1)
					{
						control.OnMultiTouchEnd();
						return true;
					}
				}
				if (touches.Count > 1)
				{
					List<dfTouchInfo> activeTouches2 = getActiveTouches();
					dfTouchEventArgs args2 = new dfTouchEventArgs(control, activeTouches2, info.ray);
					control.OnMultiTouch(args2);
					return true;
				}
				if (!IsDragging && info.Phase == TouchPhase.Stationary)
				{
					if (info.control == control)
					{
						control.OnMouseHover((dfTouchEventArgs)info);
						return true;
					}
					return false;
				}
				if (capture.Contains(info.FingerID) && dragState == dfDragDropState.None && info.Phase == TouchPhase.Moved)
				{
					dfDragEventArgs dfDragEventArgs4 = info;
					control.OnDragStart(dfDragEventArgs4);
					if (dfDragEventArgs4.State == dfDragDropState.Dragging && dfDragEventArgs4.Used)
					{
						dragState = dfDragDropState.Dragging;
						dragData = dfDragEventArgs4.Data;
						return true;
					}
					dragState = dfDragDropState.Denied;
				}
				if (info.control != control && !capture.Contains(info.FingerID))
				{
					info.control = control;
					control.OnMouseLeave((dfTouchEventArgs)info);
					touches.Remove(info.FingerID);
					return true;
				}
				info.control = control;
				control.OnMouseMove((dfTouchEventArgs)info);
				return true;
			}

			private bool canFireClickEvent(TouchRaycast info, TouchRaycast touch)
			{
				if (control == null)
				{
					return false;
				}
				float num = control.PixelsToUnits();
				Vector3 a = controlStartPosition / num;
				Vector3 b = control.transform.position / num;
				if (Vector3.Distance(a, b) > 1f)
				{
					return false;
				}
				return true;
			}

			private List<dfTouchInfo> getActiveTouches()
			{
				IList<dfTouchInfo> list = manager.touchInputSource.Touches;
				List<dfTouchInfo> result = touches.Select((KeyValuePair<int, TouchRaycast> x) => x.Value.touch).ToList();
				int i = 0;
				while (i < result.Count)
				{
					bool flag = false;
					int num = 0;
					while (i < list.Count)
					{
						if (list[num].fingerId == result[i].fingerId)
						{
							flag = true;
							break;
						}
						num++;
					}
					if (flag)
					{
						result[i] = list.First((dfTouchInfo x) => x.fingerId == result[i].fingerId);
						i++;
					}
					else
					{
						result.RemoveAt(i);
					}
				}
				return result;
			}
		}

		private class TouchRaycast
		{
			public dfControl control;

			public dfTouchInfo touch;

			public Ray ray;

			public Vector2 position;

			public int FingerID
			{
				get
				{
					return touch.fingerId;
				}
			}

			public TouchPhase Phase
			{
				get
				{
					return touch.phase;
				}
			}

			public TouchRaycast(dfControl control, dfTouchInfo touch, Ray ray)
			{
				this.control = control;
				this.touch = touch;
				this.ray = ray;
				position = touch.position;
			}

			public static implicit operator dfTouchEventArgs(TouchRaycast touch)
			{
				return new dfTouchEventArgs(touch.control, touch.touch, touch.ray);
			}

			public static implicit operator dfDragEventArgs(TouchRaycast touch)
			{
				return new dfDragEventArgs(touch.control, dfDragDropState.None, null, touch.ray, touch.position);
			}
		}

		private List<ControlTouchTracker> tracked = new List<ControlTouchTracker>();

		private List<int> untracked = new List<int>();

		private dfInputManager manager;

		private TouchInputManager()
		{
		}

		public TouchInputManager(dfInputManager manager)
		{
			this.manager = manager;
		}

		internal void Process(Transform transform, Camera renderCamera, IDFTouchInputSource input, bool retainFocusSetting)
		{
			controlUnderMouse = null;
			IList<dfTouchInfo> touches = input.Touches;
			for (int i = 0; i < touches.Count; i++)
			{
				dfTouchInfo touch = touches[i];
				dfControl dfControl2 = dfGUIManager.HitTestAll(touch.position);
				if (dfControl2 != null && dfControl2.transform.IsChildOf(manager.transform))
				{
					controlUnderMouse = dfControl2;
				}
				if (controlUnderMouse == null && touch.phase == TouchPhase.Began)
				{
					if (!retainFocusSetting && untracked.Count == 0)
					{
						dfControl activeControl = dfGUIManager.ActiveControl;
						if (activeControl != null && activeControl.transform.IsChildOf(manager.transform))
						{
							activeControl.Unfocus();
						}
					}
					untracked.Add(touch.fingerId);
					continue;
				}
				if (untracked.Contains(touch.fingerId))
				{
					if (touch.phase == TouchPhase.Ended)
					{
						untracked.Remove(touch.fingerId);
					}
					continue;
				}
				Ray ray = renderCamera.ScreenPointToRay(touch.position);
				TouchRaycast info = new TouchRaycast(controlUnderMouse, touch, ray);
				ControlTouchTracker controlTouchTracker = tracked.FirstOrDefault((ControlTouchTracker x) => x.IsTrackingFinger(info.FingerID));
				if (controlTouchTracker != null)
				{
					controlTouchTracker.Process(info);
					continue;
				}
				bool flag = false;
				for (int j = 0; j < tracked.Count; j++)
				{
					if (tracked[j].Process(info))
					{
						flag = true;
						break;
					}
				}
				if (!flag && controlUnderMouse != null && !tracked.Any((ControlTouchTracker x) => x.control == controlUnderMouse))
				{
					ControlTouchTracker controlTouchTracker2 = new ControlTouchTracker(manager, controlUnderMouse);
					tracked.Add(controlTouchTracker2);
					controlTouchTracker2.Process(info);
				}
			}
		}

		private dfControl clipCast(Transform transform, RaycastHit[] hits)
		{
			if (hits == null || hits.Length == 0)
			{
				return null;
			}
			dfControl dfControl2 = null;
			dfControl modalControl = dfGUIManager.GetModalControl();
			for (int num = hits.Length - 1; num >= 0; num--)
			{
				RaycastHit hit = hits[num];
				dfControl component = hit.transform.GetComponent<dfControl>();
				if (!(component == null) && (!(modalControl != null) || component.transform.IsChildOf(modalControl.transform)) && component.enabled && component.IsEnabled && component.IsVisible && component.transform.IsChildOf(transform) && isInsideClippingRegion(hit, component) && (dfControl2 == null || component.RenderOrder > dfControl2.RenderOrder))
				{
					dfControl2 = component;
				}
			}
			return dfControl2;
		}

		private bool isInsideClippingRegion(RaycastHit hit, dfControl control)
		{
			Vector3 point = hit.point;
			while (control != null)
			{
				Plane[] array = ((!control.ClipChildren) ? null : control.GetClippingPlanes());
				if (array != null && array.Length > 0)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (!array[i].GetSide(point))
						{
							return false;
						}
					}
				}
				control = control.Parent;
			}
			return true;
		}
	}

	private class MouseInputManager
	{
		private const string scrollAxisName = "Mouse ScrollWheel";

		private const float DOUBLECLICK_TIME = 0.25f;

		private const int DRAG_START_DELTA = 2;

		private dfControl activeControl;

		private Vector3 activeControlPosition;

		private Vector2 lastPosition = Vector2.one * -2.14748365E+09f;

		private Vector2 mouseMoveDelta = Vector2.zero;

		private float lastClickTime;

		private float lastHoverTime;

		private dfDragDropState dragState;

		private object dragData;

		private dfControl lastDragControl;

		private dfMouseButtons buttonsDown;

		private dfMouseButtons buttonsReleased;

		private dfMouseButtons buttonsPressed;

		public void ProcessInput(dfInputManager manager, IInputAdapter adapter, Ray ray, dfControl control, bool retainFocusSetting)
		{
			Vector2 mousePosition = adapter.GetMousePosition();
			buttonsDown = dfMouseButtons.None;
			buttonsReleased = dfMouseButtons.None;
			buttonsPressed = dfMouseButtons.None;
			getMouseButtonInfo(adapter, ref buttonsDown, ref buttonsReleased, ref buttonsPressed);
			float num = adapter.GetAxis("Mouse ScrollWheel");
			if (!Mathf.Approximately(num, 0f))
			{
				num = Mathf.Sign(num) * Mathf.Max(1f, Mathf.Abs(num));
			}
			mouseMoveDelta = mousePosition - lastPosition;
			lastPosition = mousePosition;
			if (dragState == dfDragDropState.Dragging)
			{
				if (buttonsReleased == dfMouseButtons.None)
				{
					if (control == activeControl)
					{
						return;
					}
					if (control != lastDragControl)
					{
						if (lastDragControl != null)
						{
							dfDragEventArgs args = new dfDragEventArgs(lastDragControl, dragState, dragData, ray, mousePosition);
							lastDragControl.OnDragLeave(args);
						}
						if (control != null)
						{
							dfDragEventArgs args2 = new dfDragEventArgs(control, dragState, dragData, ray, mousePosition);
							control.OnDragEnter(args2);
						}
						lastDragControl = control;
					}
					else if (control != null && mouseMoveDelta.magnitude > 1f)
					{
						dfDragEventArgs args3 = new dfDragEventArgs(control, dragState, dragData, ray, mousePosition);
						control.OnDragOver(args3);
					}
					return;
				}
				if (control != null && control != activeControl)
				{
					dfDragEventArgs dfDragEventArgs2 = new dfDragEventArgs(control, dfDragDropState.Dragging, dragData, ray, mousePosition);
					control.OnDragDrop(dfDragEventArgs2);
					if (!dfDragEventArgs2.Used || dfDragEventArgs2.State == dfDragDropState.Dragging)
					{
						dfDragEventArgs2.State = dfDragDropState.Cancelled;
					}
					dfDragEventArgs2 = new dfDragEventArgs(activeControl, dfDragEventArgs2.State, dfDragEventArgs2.Data, ray, mousePosition);
					dfDragEventArgs2.Target = control;
					activeControl.OnDragEnd(dfDragEventArgs2);
				}
				else
				{
					dfDragDropState state = ((!(control == null)) ? dfDragDropState.Cancelled : dfDragDropState.CancelledNoTarget);
					dfDragEventArgs args4 = new dfDragEventArgs(activeControl, state, dragData, ray, mousePosition);
					activeControl.OnDragEnd(args4);
				}
				dragState = dfDragDropState.None;
				lastDragControl = null;
				activeControl = null;
				lastClickTime = 0f;
				lastHoverTime = 0f;
				lastPosition = mousePosition;
				return;
			}
			if (buttonsPressed != 0)
			{
				lastHoverTime = Time.realtimeSinceStartup + manager.hoverStartDelay;
				if (activeControl != null)
				{
					if (activeControl.transform.IsChildOf(manager.transform))
					{
						activeControl.OnMouseDown(new dfMouseEventArgs(activeControl, buttonsPressed, 0, ray, mousePosition, num));
					}
				}
				else if (control == null || control.transform.IsChildOf(manager.transform))
				{
					setActive(manager, control, mousePosition, ray);
					if (control != null)
					{
						dfGUIManager.SetFocus(control);
						control.OnMouseDown(new dfMouseEventArgs(control, buttonsPressed, 0, ray, mousePosition, num));
					}
					else if (!retainFocusSetting)
					{
						dfControl dfControl2 = dfGUIManager.ActiveControl;
						if (dfControl2 != null && dfControl2.transform.IsChildOf(manager.transform))
						{
							dfControl2.Unfocus();
						}
					}
				}
				if (buttonsReleased == dfMouseButtons.None)
				{
					return;
				}
			}
			if (buttonsReleased != 0)
			{
				lastHoverTime = Time.realtimeSinceStartup + manager.hoverStartDelay;
				if (activeControl == null)
				{
					setActive(manager, control, mousePosition, ray);
					return;
				}
				if (activeControl == control && buttonsDown == dfMouseButtons.None)
				{
					float num2 = activeControl.PixelsToUnits();
					Vector3 a = activeControlPosition / num2;
					Vector3 b = activeControl.transform.position / num2;
					if (Vector3.Distance(a, b) <= 1f)
					{
						if (Time.realtimeSinceStartup - lastClickTime < 0.25f)
						{
							lastClickTime = 0f;
							activeControl.OnDoubleClick(new dfMouseEventArgs(activeControl, buttonsReleased, 1, ray, mousePosition, num));
						}
						else
						{
							lastClickTime = Time.realtimeSinceStartup;
							activeControl.OnClick(new dfMouseEventArgs(activeControl, buttonsReleased, 1, ray, mousePosition, num));
						}
					}
				}
				activeControl.OnMouseUp(new dfMouseEventArgs(activeControl, buttonsReleased, 0, ray, mousePosition, num));
				if (buttonsDown == dfMouseButtons.None && activeControl != control)
				{
					setActive(manager, null, mousePosition, ray);
				}
				return;
			}
			if (activeControl != null && activeControl == control && mouseMoveDelta.magnitude == 0f && Time.realtimeSinceStartup - lastHoverTime > manager.hoverNotifactionFrequency)
			{
				activeControl.OnMouseHover(new dfMouseEventArgs(activeControl, buttonsDown, 0, ray, mousePosition, num));
				lastHoverTime = Time.realtimeSinceStartup;
			}
			if (buttonsDown == dfMouseButtons.None)
			{
				if (num != 0f && control != null)
				{
					setActive(manager, control, mousePosition, ray);
					control.OnMouseWheel(new dfMouseEventArgs(control, buttonsDown, 0, ray, mousePosition, num));
					return;
				}
				setActive(manager, control, mousePosition, ray);
			}
			else if (buttonsDown != 0 && activeControl != null)
			{
				if (!(control != null) || control.RenderOrder > activeControl.RenderOrder)
				{
				}
				if (mouseMoveDelta.magnitude >= 2f && (buttonsDown & (dfMouseButtons.Left | dfMouseButtons.Right)) != 0 && dragState != dfDragDropState.Denied)
				{
					dfDragEventArgs dfDragEventArgs3 = new dfDragEventArgs(activeControl);
					dfDragEventArgs3.Position = mousePosition;
					dfDragEventArgs dfDragEventArgs4 = dfDragEventArgs3;
					activeControl.OnDragStart(dfDragEventArgs4);
					if (dfDragEventArgs4.State == dfDragDropState.Dragging)
					{
						dragState = dfDragDropState.Dragging;
						dragData = dfDragEventArgs4.Data;
						return;
					}
					dragState = dfDragDropState.Denied;
				}
			}
			if (activeControl != null && mouseMoveDelta.magnitude >= 1f)
			{
				dfMouseEventArgs dfMouseEventArgs2 = new dfMouseEventArgs(activeControl, buttonsDown, 0, ray, mousePosition, num);
				dfMouseEventArgs2.MoveDelta = mouseMoveDelta;
				dfMouseEventArgs args5 = dfMouseEventArgs2;
				activeControl.OnMouseMove(args5);
			}
		}

		private static void getMouseButtonInfo(IInputAdapter adapter, ref dfMouseButtons buttonsDown, ref dfMouseButtons buttonsReleased, ref dfMouseButtons buttonsPressed)
		{
			for (int i = 0; i < 3; i++)
			{
				if (adapter.GetMouseButton(i))
				{
					buttonsDown |= (dfMouseButtons)(1 << i);
				}
				if (adapter.GetMouseButtonUp(i))
				{
					buttonsReleased |= (dfMouseButtons)(1 << i);
				}
				if (adapter.GetMouseButtonDown(i))
				{
					buttonsPressed |= (dfMouseButtons)(1 << i);
				}
			}
		}

		private void setActive(dfInputManager manager, dfControl control, Vector2 position, Ray ray)
		{
			if (activeControl != null && activeControl != control)
			{
				activeControl.OnMouseLeave(new dfMouseEventArgs(activeControl)
				{
					Position = position,
					Ray = ray
				});
			}
			if (control != null && control != activeControl)
			{
				lastClickTime = 0f;
				lastHoverTime = Time.realtimeSinceStartup + manager.hoverStartDelay;
				control.OnMouseEnter(new dfMouseEventArgs(control)
				{
					Position = position,
					Ray = ray
				});
			}
			activeControl = control;
			activeControlPosition = ((!(control != null)) ? (Vector3.one * float.MinValue) : control.transform.position);
			lastPosition = position;
			dragState = dfDragDropState.None;
		}
	}

	private class DefaultInput : IInputAdapter
	{
		public DefaultInput(Camera renderCam)
		{
		}

		public bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}

		public bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}

		public float GetAxis(string axisName)
		{
			return Input.GetAxis(axisName);
		}

		public Vector2 GetMousePosition()
		{
			return Input.mousePosition;
		}

		public bool GetMouseButton(int button)
		{
			return Input.GetMouseButton(button);
		}

		public bool GetMouseButtonDown(int button)
		{
			return Input.GetMouseButtonDown(button);
		}

		public bool GetMouseButtonUp(int button)
		{
			return Input.GetMouseButtonUp(button);
		}
	}

	private static dfControl controlUnderMouse = null;

	private static dfList<dfInputManager> activeInstances = new dfList<dfInputManager>();

	[SerializeField]
	protected Camera renderCamera;

	[SerializeField]
	protected bool useTouch = true;

	[SerializeField]
	protected bool useMouse = true;

	[SerializeField]
	protected bool useJoystick;

	[SerializeField]
	protected KeyCode joystickClickButton = KeyCode.Joystick1Button1;

	[SerializeField]
	protected string horizontalAxis = "Horizontal";

	[SerializeField]
	protected string verticalAxis = "Vertical";

	[SerializeField]
	protected float axisPollingInterval = 0.15f;

	[SerializeField]
	protected bool retainFocus;

	[SerializeField]
	[HideInInspector]
	protected int touchClickRadius = 125;

	[SerializeField]
	protected float hoverStartDelay = 0.25f;

	[SerializeField]
	protected float hoverNotifactionFrequency = 0.1f;

	private IDFTouchInputSource touchInputSource;

	private TouchInputManager touchHandler;

	private MouseInputManager mouseHandler;

	private dfGUIManager guiManager;

	private dfControl buttonDownTarget;

	private IInputAdapter adapter;

	private float lastAxisCheck;

	private Vector2 mousePositionLastFrame;

	public static IList<dfInputManager> ActiveInstances
	{
		get
		{
			return activeInstances;
		}
	}

	public static dfControl ControlUnderMouse
	{
		get
		{
			return controlUnderMouse;
		}
	}

	public Camera RenderCamera
	{
		get
		{
			return renderCamera;
		}
		set
		{
			renderCamera = value;
		}
	}

	public bool UseTouch
	{
		get
		{
			return useTouch;
		}
		set
		{
			useTouch = value;
		}
	}

	public bool UseMouse
	{
		get
		{
			return useMouse;
		}
		set
		{
			useMouse = value;
		}
	}

	public bool UseJoystick
	{
		get
		{
			return useJoystick;
		}
		set
		{
			useJoystick = value;
		}
	}

	public KeyCode JoystickClickButton
	{
		get
		{
			return joystickClickButton;
		}
		set
		{
			joystickClickButton = value;
		}
	}

	public string HorizontalAxis
	{
		get
		{
			return horizontalAxis;
		}
		set
		{
			horizontalAxis = value;
		}
	}

	public string VerticalAxis
	{
		get
		{
			return verticalAxis;
		}
		set
		{
			verticalAxis = value;
		}
	}

	public IInputAdapter Adapter
	{
		get
		{
			return adapter;
		}
		set
		{
			adapter = value ?? new DefaultInput(renderCamera);
		}
	}

	public bool RetainFocus
	{
		get
		{
			return retainFocus;
		}
		set
		{
			retainFocus = value;
		}
	}

	public IDFTouchInputSource TouchInputSource
	{
		get
		{
			return touchInputSource;
		}
		set
		{
			touchInputSource = value;
		}
	}

	public float HoverStartDelay
	{
		get
		{
			return hoverStartDelay;
		}
		set
		{
			hoverStartDelay = value;
		}
	}

	public float HoverNotificationFrequency
	{
		get
		{
			return hoverNotifactionFrequency;
		}
		set
		{
			hoverNotifactionFrequency = value;
		}
	}

	public void Awake()
	{
		base.useGUILayout = false;
	}

	public void Start()
	{
	}

	public void OnDisable()
	{
		activeInstances.Remove(this);
		dfControl activeControl = dfGUIManager.ActiveControl;
		if (activeControl != null && activeControl.transform.IsChildOf(base.transform))
		{
			dfGUIManager.SetFocus(null);
		}
	}

	public void OnEnable()
	{
		activeInstances.Add(this);
		mouseHandler = new MouseInputManager();
		if (useTouch)
		{
			touchHandler = new TouchInputManager(this);
		}
		if (adapter == null)
		{
			Component component = (from c in GetComponents(typeof(MonoBehaviour))
				where c != null && c.GetType() != null && typeof(IInputAdapter).IsAssignableFrom(c.GetType())
				select c).FirstOrDefault();
			adapter = ((IInputAdapter)component) ?? new DefaultInput(renderCamera);
		}
		Input.simulateMouseWithTouches = !useTouch;
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (guiManager == null)
		{
			guiManager = GetComponent<dfGUIManager>();
			if (guiManager == null)
			{
				Debug.LogWarning("No associated dfGUIManager instance", this);
				base.enabled = false;
				return;
			}
		}
		dfControl activeControl = dfGUIManager.ActiveControl;
		if (true && useTouch && processTouchInput())
		{
			return;
		}
		if (useMouse)
		{
			if (BraveInput.PrimaryPlayerInstance != null && BraveInput.PrimaryPlayerInstance.ActiveActions != null)
			{
				Vector2 vector = adapter.GetMousePosition() - mousePositionLastFrame;
				if (BraveInput.PrimaryPlayerInstance.ActiveActions.LastInputType == BindingSourceType.MouseBindingSource || (BraveInput.PrimaryPlayerInstance.ActiveActions.LastInputType == BindingSourceType.KeyBindingSource && vector.magnitude > float.Epsilon) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
				{
					processMouseInput();
				}
			}
			else
			{
				processMouseInput();
			}
		}
		if (!(activeControl == null) && !processKeyboard())
		{
			mousePositionLastFrame = adapter.GetMousePosition();
		}
	}

	private void processJoystick()
	{
		try
		{
			dfControl activeControl = dfGUIManager.ActiveControl;
			if (activeControl == null || !activeControl.transform.IsChildOf(base.transform))
			{
				return;
			}
			float axis = adapter.GetAxis(horizontalAxis);
			float axis2 = adapter.GetAxis(verticalAxis);
			if (Mathf.Abs(axis) < 0.5f && Mathf.Abs(axis2) <= 0.5f)
			{
				lastAxisCheck = BraveTime.DeltaTime - axisPollingInterval;
			}
			if (Time.realtimeSinceStartup - lastAxisCheck > axisPollingInterval)
			{
				if (Mathf.Abs(axis) >= 0.5f)
				{
					lastAxisCheck = Time.realtimeSinceStartup;
					KeyCode key = ((!(axis > 0f)) ? KeyCode.LeftArrow : KeyCode.RightArrow);
					activeControl.OnKeyDown(new dfKeyEventArgs(activeControl, key, false, false, false));
				}
				if (Mathf.Abs(axis2) >= 0.5f)
				{
					lastAxisCheck = Time.realtimeSinceStartup;
					KeyCode key2 = ((!(axis2 > 0f)) ? KeyCode.DownArrow : KeyCode.UpArrow);
					activeControl.OnKeyDown(new dfKeyEventArgs(activeControl, key2, false, false, false));
				}
			}
			if (joystickClickButton != 0)
			{
				if (adapter.GetKeyDown(joystickClickButton))
				{
					Vector3 center = activeControl.GetCenter();
					Camera camera = activeControl.GetCamera();
					Ray ray = camera.ScreenPointToRay(camera.WorldToScreenPoint(center));
					dfMouseEventArgs args = new dfMouseEventArgs(activeControl, dfMouseButtons.Left, 0, ray, center, 0f);
					activeControl.OnMouseDown(args);
					buttonDownTarget = activeControl;
				}
				if (adapter.GetKeyUp(joystickClickButton))
				{
					if (buttonDownTarget == activeControl)
					{
						activeControl.DoClick();
					}
					Vector3 center2 = activeControl.GetCenter();
					Camera camera2 = activeControl.GetCamera();
					Ray ray2 = camera2.ScreenPointToRay(camera2.WorldToScreenPoint(center2));
					dfMouseEventArgs args2 = new dfMouseEventArgs(activeControl, dfMouseButtons.Left, 0, ray2, center2, 0f);
					activeControl.OnMouseUp(args2);
					buttonDownTarget = null;
				}
			}
			for (KeyCode keyCode = KeyCode.Joystick1Button0; keyCode <= KeyCode.Joystick1Button19; keyCode++)
			{
				if (adapter.GetKeyDown(keyCode))
				{
					activeControl.OnKeyDown(new dfKeyEventArgs(activeControl, keyCode, false, false, false));
				}
			}
		}
		catch (UnityException ex)
		{
			Debug.LogError(ex.ToString(), this);
			useJoystick = false;
		}
	}

	private void processKeyEvent(EventType eventType, KeyCode keyCode, EventModifiers modifiers)
	{
		dfControl activeControl = dfGUIManager.ActiveControl;
		if (!(activeControl == null) && activeControl.IsEnabled && activeControl.transform.IsChildOf(base.transform))
		{
			bool control = (modifiers & EventModifiers.Control) == EventModifiers.Control || (modifiers & EventModifiers.Command) == EventModifiers.Command;
			bool flag = (modifiers & EventModifiers.Shift) == EventModifiers.Shift;
			bool alt = (modifiers & EventModifiers.Alt) == EventModifiers.Alt;
			dfKeyEventArgs dfKeyEventArgs2 = new dfKeyEventArgs(activeControl, keyCode, control, flag, alt);
			if (keyCode >= KeyCode.Space && keyCode <= KeyCode.Z)
			{
				char c = (char)keyCode;
				dfKeyEventArgs2.Character = ((!flag) ? char.ToLower(c) : char.ToUpper(c));
			}
			switch (eventType)
			{
			case EventType.KeyDown:
				activeControl.OnKeyDown(dfKeyEventArgs2);
				break;
			case EventType.KeyUp:
				activeControl.OnKeyUp(dfKeyEventArgs2);
				break;
			}
			if (!dfKeyEventArgs2.Used && eventType != EventType.KeyUp)
			{
			}
		}
	}

	private bool processKeyboard()
	{
		dfControl activeControl = dfGUIManager.ActiveControl;
		if (activeControl == null || string.IsNullOrEmpty(Input.inputString) || !activeControl.transform.IsChildOf(base.transform))
		{
			return false;
		}
		string inputString = Input.inputString;
		foreach (char c in inputString)
		{
			if (c != '\b' && c != '\n')
			{
				KeyCode key = (KeyCode)c;
				dfKeyEventArgs dfKeyEventArgs2 = new dfKeyEventArgs(activeControl, key, false, false, false);
				dfKeyEventArgs2.Character = c;
				activeControl.OnKeyPress(dfKeyEventArgs2);
			}
		}
		return true;
	}

	private bool processTouchInput()
	{
		if (touchInputSource == null)
		{
			dfTouchInputSourceComponent[] array = (from x in GetComponents<dfTouchInputSourceComponent>()
				orderby x.Priority descending
				select x).ToArray();
			foreach (dfTouchInputSourceComponent dfTouchInputSourceComponent2 in array)
			{
				if (dfTouchInputSourceComponent2.enabled)
				{
					touchInputSource = dfTouchInputSourceComponent2.Source;
					if (touchInputSource != null)
					{
						break;
					}
				}
			}
			if (touchInputSource == null)
			{
				touchInputSource = dfMobileTouchInputSource.Instance;
			}
		}
		touchInputSource.Update();
		if (touchInputSource.TouchCount == 0)
		{
			return false;
		}
		touchHandler.Process(base.transform, renderCamera, touchInputSource, retainFocus);
		return true;
	}

	public bool TestRectUnderMouseEquipment(Bounds b)
	{
		if (!renderCamera.enabled)
		{
			return false;
		}
		Vector2 mousePosition = adapter.GetMousePosition();
		Vector2 rectSize = BraveCameraUtility.GetRectSize();
		float num = (float)Screen.width * rectSize.x / 2f;
		float num2 = (float)Screen.height * rectSize.y / 2f;
		Vector2 vector = mousePosition - new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		Vector2 vector2 = vector / ((float)Screen.height / 2f);
		Vector2 vector3 = vector2 * renderCamera.orthographicSize;
		Vector3 vector4 = new Vector3(num / 2f / num2 * renderCamera.orthographicSize, 0f, 0f);
		Ray ray = new Ray(renderCamera.transform.position + vector3.ToVector3ZUp() + vector4, renderCamera.transform.forward);
		Debug.DrawRay(ray.origin, ray.direction, Color.yellow, 5f);
		if (b.IntersectRay(ray))
		{
			return true;
		}
		return false;
	}

	private Vector2 PreprocessUltrawideAmmonomiconMousePosition(Vector2 inPosition)
	{
		Vector2 result = inPosition;
		Rect rect = BraveCameraUtility.GetRect();
		float num = (float)Screen.height * rect.height;
		float num2 = (float)Screen.width * rect.width;
		float aSPECT = BraveCameraUtility.ASPECT;
		result.x -= (float)Screen.width * (1f - rect.width) / 2f;
		result.y -= (float)Screen.height * (1f - rect.height) / 2f;
		if (aSPECT > 1.77777779f)
		{
			float num3 = num * 1.77777779f;
			result.x -= (num2 - num3) / 2f;
			result.x = Mathf.Lerp(0f, 1920f, Mathf.Clamp01(result.x / num3));
			result.y = Mathf.Lerp(0f, 1080f, result.y / num);
		}
		else
		{
			float num4 = num2 * 0.5625f;
			result.y -= (num - num4) / 2f;
			result.x = Mathf.Lerp(0f, 1920f, result.x / num2);
			result.y = Mathf.Lerp(0f, 1080f, Mathf.Clamp01(result.y / num4));
		}
		return result;
	}

	private void processMouseInput()
	{
		if (guiManager == null)
		{
			return;
		}
		Vector2 vector = adapter.GetMousePosition();
		if (AmmonomiconController.HasInstance)
		{
			if (AmmonomiconController.Instance.CurrentRightPageRenderer != null && AmmonomiconController.Instance.CurrentRightPageRenderer.guiManager == guiManager)
			{
				vector = PreprocessUltrawideAmmonomiconMousePosition(vector);
				vector.x -= 960f;
			}
			else if (AmmonomiconController.Instance.CurrentLeftPageRenderer != null && AmmonomiconController.Instance.CurrentLeftPageRenderer.guiManager == guiManager)
			{
				vector = PreprocessUltrawideAmmonomiconMousePosition(vector);
			}
		}
		Ray ray = renderCamera.ScreenPointToRay(vector);
		controlUnderMouse = dfGUIManager.HitTestAll(vector);
		if (controlUnderMouse != null && !controlUnderMouse.transform.IsChildOf(base.transform))
		{
			controlUnderMouse = null;
		}
		mouseHandler.ProcessInput(this, adapter, ray, controlUnderMouse, retainFocus);
	}

	internal static int raycastHitSorter(RaycastHit lhs, RaycastHit rhs)
	{
		return lhs.distance.CompareTo(rhs.distance);
	}

	internal dfControl clipCast(RaycastHit[] hits)
	{
		if (hits == null || hits.Length == 0)
		{
			return null;
		}
		dfControl dfControl2 = null;
		dfControl modalControl = dfGUIManager.GetModalControl();
		for (int num = hits.Length - 1; num >= 0; num--)
		{
			RaycastHit raycastHit = hits[num];
			dfControl component = raycastHit.transform.GetComponent<dfControl>();
			if (!(component == null) && (!(modalControl != null) || component.transform.IsChildOf(modalControl.transform)) && component.enabled && !(combinedOpacity(component) <= 0.01f) && component.IsEnabled && component.IsVisible && component.transform.IsChildOf(base.transform) && isInsideClippingRegion(raycastHit.point, component) && (dfControl2 == null || component.RenderOrder > dfControl2.RenderOrder))
			{
				dfControl2 = component;
			}
		}
		return dfControl2;
	}

	internal static bool isInsideClippingRegion(Vector3 point, dfControl control)
	{
		while (control != null)
		{
			Plane[] array = ((!control.ClipChildren) ? null : control.GetClippingPlanes());
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].GetSide(point))
					{
						return false;
					}
				}
			}
			control = control.Parent;
		}
		return true;
	}

	private static float combinedOpacity(dfControl control)
	{
		float num = 1f;
		while (control != null)
		{
			num *= control.Opacity;
			control = control.Parent;
		}
		return num;
	}
}
