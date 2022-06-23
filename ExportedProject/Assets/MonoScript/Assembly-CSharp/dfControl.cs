using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
public abstract class dfControl : MonoBehaviour, IDFControlHost, IComparable<dfControl>, IAmmonomiconFocusable
{
	protected class SignalCache
	{
		private class SignalCacheItem
		{
			public readonly Type ComponentType;

			public readonly string EventName;

			private readonly MethodInfo method;

			private readonly bool usesParameters;

			public SignalCacheItem(Type componentType, string eventName, Type[] paramTypes)
			{
				ComponentType = componentType;
				EventName = eventName;
				MethodInfo methodInfo = getMethod(componentType, eventName, paramTypes);
				if (methodInfo != null)
				{
					method = methodInfo;
					usesParameters = true;
				}
				else
				{
					method = getMethod(componentType, eventName, dfReflectionExtensions.EmptyTypes);
					usesParameters = false;
				}
			}

			public bool Invoke(object target, object[] arguments, out object returnValue)
			{
				if (method == null)
				{
					returnValue = null;
					return false;
				}
				if (!usesParameters)
				{
					arguments = null;
				}
				returnValue = method.Invoke(target, arguments);
				return true;
			}
		}

		private static readonly List<SignalCacheItem> cache = new List<SignalCacheItem>();

		public static bool Invoke(Component target, string eventName, object[] arguments, out object returnValue)
		{
			returnValue = null;
			if (target == null)
			{
				return false;
			}
			Type type = target.GetType();
			SignalCacheItem signalCacheItem = getItem(type, eventName);
			if (signalCacheItem == null)
			{
				Type[] array = new Type[arguments.Length];
				for (int i = 0; i < array.Length; i++)
				{
					if (arguments[i] == null)
					{
						array[i] = typeof(object);
					}
					else
					{
						array[i] = arguments[i].GetType();
					}
				}
				signalCacheItem = new SignalCacheItem(type, eventName, array);
				cache.Add(signalCacheItem);
			}
			return signalCacheItem.Invoke(target, arguments, out returnValue);
		}

		private static SignalCacheItem getItem(Type componentType, string eventName)
		{
			for (int i = 0; i < cache.Count; i++)
			{
				SignalCacheItem signalCacheItem = cache[i];
				if (signalCacheItem.ComponentType == componentType && signalCacheItem.EventName == eventName)
				{
					return signalCacheItem;
				}
			}
			return null;
		}

		private static MethodInfo getMethod(Type type, string name, Type[] paramTypes)
		{
			return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
		}

		private static bool matchesParameterTypes(MethodInfo method, Type[] types)
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters.Length != types.Length)
			{
				return false;
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (!parameters[i].ParameterType.IsAssignableFrom(types[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	[Serializable]
	protected class AnchorLayout
	{
		[SerializeField]
		protected dfAnchorStyle anchorStyle;

		[SerializeField]
		protected dfAnchorMargins margins;

		[SerializeField]
		protected dfControl owner;

		private int suspendLayoutCounter;

		private bool performingLayout;

		private bool disposed;

		private bool pendingLayoutRequest;

		internal dfAnchorStyle AnchorStyle
		{
			get
			{
				return anchorStyle;
			}
			set
			{
				if (value != anchorStyle)
				{
					anchorStyle = value;
					Reset();
				}
			}
		}

		internal bool IsPerformingLayout
		{
			get
			{
				return performingLayout;
			}
		}

		internal bool IsLayoutSuspended
		{
			get
			{
				return suspendLayoutCounter > 0;
			}
		}

		internal bool HasPendingLayoutRequest
		{
			get
			{
				return pendingLayoutRequest;
			}
		}

		internal AnchorLayout(dfAnchorStyle anchorStyle)
		{
			this.anchorStyle = anchorStyle;
		}

		internal AnchorLayout(dfAnchorStyle anchorStyle, dfControl owner)
			: this(anchorStyle)
		{
			Attach(owner);
			Reset();
		}

		internal void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				owner = null;
			}
		}

		internal void SuspendLayout()
		{
			suspendLayoutCounter++;
		}

		internal void ResumeLayout()
		{
			bool flag = suspendLayoutCounter > 0;
			suspendLayoutCounter = Mathf.Max(0, suspendLayoutCounter - 1);
			if (flag && suspendLayoutCounter == 0 && pendingLayoutRequest)
			{
				PerformLayout();
			}
		}

		internal void PerformLayout()
		{
			if (!disposed)
			{
				if (suspendLayoutCounter > 0)
				{
					pendingLayoutRequest = true;
				}
				else
				{
					performLayoutInternal();
				}
			}
		}

		internal void Attach(dfControl ownerControl)
		{
			owner = ownerControl;
			if (ownerControl != null)
			{
				anchorStyle = ownerControl.anchorStyle;
			}
		}

		internal void Reset()
		{
			Reset(false);
		}

		internal void Reset(bool force)
		{
			if (!(owner == null) && !(owner.transform.parent == null) && anchorStyle != 0 && (force || (!IsPerformingLayout && !IsLayoutSuspended)) && !(owner == null) && owner.gameObject.activeSelf)
			{
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Proportional))
				{
					resetLayoutProportional();
				}
				else
				{
					resetLayoutAbsolute();
				}
			}
		}

		private void resetLayoutProportional()
		{
			Vector3 relativePosition = owner.RelativePosition;
			Vector2 size = owner.Size;
			Vector2 parentSize = getParentSize();
			float x = relativePosition.x;
			float y = relativePosition.y;
			float num = x + size.x;
			float num2 = y + size.y;
			if (margins == null)
			{
				margins = new dfAnchorMargins();
			}
			margins.left = x / parentSize.x;
			margins.right = num / parentSize.x;
			margins.top = y / parentSize.y;
			margins.bottom = num2 / parentSize.y;
		}

		private void resetLayoutAbsolute()
		{
			Vector3 relativePosition = owner.RelativePosition;
			Vector2 size = owner.Size;
			Vector2 parentSize = getParentSize();
			float x = relativePosition.x;
			float y = relativePosition.y;
			float right = parentSize.x - size.x - x;
			float bottom = parentSize.y - size.y - y;
			if (margins == null)
			{
				margins = new dfAnchorMargins();
			}
			margins.left = x;
			margins.right = right;
			margins.top = y;
			margins.bottom = bottom;
		}

		protected void performLayoutInternal()
		{
			if (anchorStyle == dfAnchorStyle.None)
			{
				return;
			}
			if (owner == null || owner.transform.parent == null)
			{
				pendingLayoutRequest = true;
			}
			else
			{
				if (margins == null || IsPerformingLayout || IsLayoutSuspended || !owner.gameObject.activeSelf)
				{
					return;
				}
				try
				{
					performingLayout = true;
					pendingLayoutRequest = false;
					Vector2 parentSize = getParentSize();
					Vector2 size = owner.Size;
					if (anchorStyle.IsFlagSet(dfAnchorStyle.Proportional))
					{
						performLayoutProportional(parentSize, size);
					}
					else
					{
						performLayoutAbsolute(parentSize, size);
					}
				}
				finally
				{
					performingLayout = false;
				}
			}
		}

		private void performLayoutProportional(Vector2 parentSize, Vector2 controlSize)
		{
			float x = margins.left * parentSize.x;
			float num = margins.right * parentSize.x;
			float y = margins.top * parentSize.y;
			float num2 = margins.bottom * parentSize.y;
			Vector3 relativePosition = owner.RelativePosition;
			Vector2 size = controlSize;
			if (anchorStyle.IsFlagSet(dfAnchorStyle.Left))
			{
				relativePosition.x = x;
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Right))
				{
					size.x = (margins.right - margins.left) * parentSize.x;
				}
			}
			else if (anchorStyle.IsFlagSet(dfAnchorStyle.Right))
			{
				relativePosition.x = num - controlSize.x;
			}
			else if (anchorStyle.IsFlagSet(dfAnchorStyle.CenterHorizontal))
			{
				relativePosition.x = (parentSize.x - controlSize.x) * 0.5f;
			}
			if (anchorStyle.IsFlagSet(dfAnchorStyle.Top))
			{
				relativePosition.y = y;
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Bottom))
				{
					size.y = (margins.bottom - margins.top) * parentSize.y;
				}
			}
			else if (anchorStyle.IsFlagSet(dfAnchorStyle.Bottom))
			{
				relativePosition.y = num2 - controlSize.y;
			}
			else if (anchorStyle.IsFlagSet(dfAnchorStyle.CenterVertical))
			{
				relativePosition.y = (parentSize.y - controlSize.y) * 0.5f;
			}
			owner.Size = size;
			owner.RelativePosition = relativePosition;
			dfGUIManager manager = owner.GetManager();
			if (manager != null && manager.PixelPerfectMode)
			{
				owner.MakePixelPerfect(false);
			}
		}

		private void performLayoutAbsolute(Vector2 parentSize, Vector2 controlSize)
		{
			float num = margins.left;
			float num2 = margins.top;
			float num3 = num + controlSize.x;
			float num4 = num2 + controlSize.y;
			if (anchorStyle.IsFlagSet(dfAnchorStyle.CenterHorizontal))
			{
				num = Mathf.RoundToInt((parentSize.x - controlSize.x) * 0.5f);
				num3 = Mathf.RoundToInt(num + controlSize.x);
			}
			else
			{
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Left))
				{
					num = margins.left;
					num3 = num + controlSize.x;
				}
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Right))
				{
					num3 = parentSize.x - margins.right;
					if (!anchorStyle.IsFlagSet(dfAnchorStyle.Left))
					{
						num = num3 - controlSize.x;
					}
				}
			}
			if (anchorStyle.IsFlagSet(dfAnchorStyle.CenterVertical))
			{
				num2 = Mathf.RoundToInt((parentSize.y - controlSize.y) * 0.5f);
				num4 = Mathf.RoundToInt(num2 + controlSize.y);
			}
			else
			{
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Top))
				{
					num2 = margins.top;
					num4 = num2 + controlSize.y;
				}
				if (anchorStyle.IsFlagSet(dfAnchorStyle.Bottom))
				{
					num4 = parentSize.y - margins.bottom;
					if (!anchorStyle.IsFlagSet(dfAnchorStyle.Top))
					{
						num2 = num4 - controlSize.y;
					}
				}
			}
			Vector2 size = new Vector2(Mathf.Max(0f, num3 - num), Mathf.Max(0f, num4 - num2));
			Vector3 value = new Vector3(num, num2);
			owner.Size = size;
			owner.setRelativePosition(ref value);
		}

		private Vector2 getParentSize()
		{
			dfControl parent = owner.parent;
			if (parent != null)
			{
				return parent.Size;
			}
			dfGUIManager manager = owner.GetManager();
			return manager.GetScreenSize();
		}

		public override string ToString()
		{
			if (owner == null)
			{
				return "NO OWNER FOR ANCHOR";
			}
			dfControl parent = owner.parent;
			return string.Format("{0}.{1} - {2}", (!(parent != null)) ? "SCREEN" : parent.name, owner.name, margins);
		}
	}

	[HideInInspector]
	public Action<dfControl> LanguageChanged;

	public Action<dfControl, Vector3, Vector3> ResolutionChangedPostLayout;

	private const float MINIMUM_OPACITY = 0.0125f;

	private static uint versionCounter;

	[SerializeField]
	protected dfAnchorStyle anchorStyle;

	[SerializeField]
	protected bool isEnabled = true;

	[SerializeField]
	protected bool isVisible = true;

	[SerializeField]
	protected bool isInteractive = true;

	[SerializeField]
	protected string tooltip;

	[SerializeField]
	protected dfPivotPoint pivot;

	[SerializeField]
	[HideInInspector]
	public int zindex = int.MaxValue;

	[SerializeField]
	protected Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[SerializeField]
	protected Color32 disabledColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[SerializeField]
	protected Vector2 size = Vector2.zero;

	[SerializeField]
	protected Vector2 minSize = Vector2.zero;

	[SerializeField]
	protected Vector2 maxSize = Vector2.zero;

	[SerializeField]
	protected bool clipChildren;

	[SerializeField]
	protected bool inverseClipChildren;

	[SerializeField]
	[HideInInspector]
	protected int tabIndex = -1;

	[HideInInspector]
	[SerializeField]
	protected bool canFocus;

	[SerializeField]
	protected bool autoFocus;

	[SerializeField]
	[HideInInspector]
	protected AnchorLayout layout;

	[SerializeField]
	[HideInInspector]
	protected int renderOrder = -1;

	[SerializeField]
	protected bool isLocalized;

	[SerializeField]
	protected Vector2 hotZoneScale = Vector2.one;

	[SerializeField]
	protected bool allowSignalEvents = true;

	private static object[] signal1 = new object[1];

	private static object[] signal2 = new object[2];

	private static object[] signal3 = new object[3];

	protected bool isControlInvalidated = true;

	protected bool isControlClipped;

	protected dfControl parent;

	protected dfList<dfControl> controls = dfList<dfControl>.Obtain();

	protected dfGUIManager cachedManager;

	protected dfLanguageManager languageManager;

	protected bool languageManagerChecked;

	protected int cachedChildCount;

	protected Vector3 cachedPosition = Vector3.one * float.MinValue;

	protected Quaternion cachedRotation = Quaternion.identity;

	protected Vector3 cachedScale = Vector3.one;

	protected Bounds? cachedBounds;

	protected Transform cachedParentTransform;

	protected float cachedPixelSize;

	protected Vector3 cachedRelativePosition = Vector3.one * float.MinValue;

	protected uint relativePositionCacheVersion = uint.MaxValue;

	protected dfRenderData renderData;

	protected bool isMouseHovering;

	private new object tag;

	protected bool isDisposing;

	private bool performingLayout;

	protected Vector3[] cachedCorners = new Vector3[4];

	protected Plane[] cachedClippingPlanes = new Plane[4];

	private bool shutdownInProgress;

	private uint version;

	protected bool isControlInitialized;

	private bool rendering;

	protected string localizationKey;

	public static readonly dfList<dfControl> ActiveInstances = new dfList<dfControl>();

	[NonSerialized]
	public bool ForceSuspendLayout;

	public bool PrecludeUpdateCycle;

	private Transform m_transform;

	public bool AllowSignalEvents
	{
		get
		{
			return allowSignalEvents;
		}
		set
		{
			allowSignalEvents = value;
		}
	}

	internal bool IsInvalid
	{
		get
		{
			return isControlInvalidated;
		}
	}

	internal bool IsControlClipped
	{
		get
		{
			return isControlClipped;
		}
	}

	public dfGUIManager GUIManager
	{
		get
		{
			return GetManager();
		}
	}

	public bool IsEnabled
	{
		get
		{
			if (!base.enabled)
			{
				return false;
			}
			if (base.gameObject != null && !base.gameObject.activeSelf)
			{
				return false;
			}
			return (!(parent != null)) ? isEnabled : (isEnabled && parent.IsEnabled);
		}
		set
		{
			if (value != isEnabled)
			{
				isEnabled = value;
				OnIsEnabledChanged();
			}
		}
	}

	[SerializeField]
	public bool IsVisible
	{
		get
		{
			return (parent == null) ? isVisible : (isVisible && parent.IsVisible);
		}
		set
		{
			if (value == isVisible)
			{
				return;
			}
			if (Application.isPlaying && !IsInteractive)
			{
				if ((bool)GetComponent<Collider>())
				{
					GetComponent<Collider>().enabled = false;
				}
			}
			else if ((bool)GetComponent<Collider>())
			{
				GetComponent<Collider>().enabled = value;
			}
			isVisible = value;
			OnIsVisibleChanged();
		}
	}

	public virtual bool IsInteractive
	{
		get
		{
			return isInteractive;
		}
		set
		{
			if (HasFocus && !value)
			{
				dfGUIManager.SetFocus(null);
			}
			isInteractive = value;
		}
	}

	[SerializeField]
	public string Tooltip
	{
		get
		{
			return tooltip;
		}
		set
		{
			if (value != tooltip)
			{
				tooltip = value;
				Invalidate();
			}
		}
	}

	[SerializeField]
	public dfAnchorStyle Anchor
	{
		get
		{
			ensureLayoutExists();
			return anchorStyle;
		}
		set
		{
			if (value != anchorStyle)
			{
				anchorStyle = value;
				OnAnchorChanged();
			}
		}
	}

	public float Opacity
	{
		get
		{
			return (float)(int)color.a / 255f;
		}
		set
		{
			value = Mathf.Max(0f, Mathf.Min(1f, value));
			float b = (float)(int)color.a / 255f;
			if (!Mathf.Approximately(value, b))
			{
				color.a = (byte)(value * 255f);
				OnOpacityChanged();
			}
		}
	}

	public Color32 Color
	{
		get
		{
			return color;
		}
		set
		{
			value.a = (byte)(Opacity * 255f);
			if (!color.EqualsNonAlloc(value))
			{
				color = value;
				OnColorChanged();
			}
		}
	}

	public Color32 DisabledColor
	{
		get
		{
			return disabledColor;
		}
		set
		{
			if (!value.Equals(disabledColor))
			{
				disabledColor = value;
				Invalidate();
			}
		}
	}

	public dfPivotPoint Pivot
	{
		get
		{
			return pivot;
		}
		set
		{
			if (value != pivot)
			{
				Vector3 position = Position;
				pivot = value;
				Vector3 vector = Position - position;
				SuspendLayout();
				Position = position;
				for (int i = 0; i < controls.Count; i++)
				{
					controls[i].Position += vector;
				}
				ResumeLayout();
				OnPivotChanged();
			}
		}
	}

	public Vector3 RelativePosition
	{
		get
		{
			return getRelativePosition();
		}
		set
		{
			setRelativePosition(ref value);
		}
	}

	public Vector3 Position
	{
		get
		{
			Vector3 vector = base.transform.localPosition / PixelsToUnits();
			return vector + pivot.TransformToUpperLeft(Size);
		}
		set
		{
			setPositionInternal(value);
		}
	}

	public Vector2 Size
	{
		get
		{
			return size;
		}
		set
		{
			value = Vector2.Max(CalculateMinimumSize(), value);
			value.x = ((!(maxSize.x > 0f)) ? value.x : Mathf.Min(value.x, maxSize.x));
			value.y = ((!(maxSize.y > 0f)) ? value.y : Mathf.Min(value.y, maxSize.y));
			if (!((value - size).sqrMagnitude <= 1f))
			{
				size = value;
				OnSizeChanged();
			}
		}
	}

	public float Width
	{
		get
		{
			return size.x;
		}
		set
		{
			Size = new Vector2(value, size.y);
		}
	}

	public float Height
	{
		get
		{
			return size.y;
		}
		set
		{
			Size = new Vector2(size.x, value);
		}
	}

	public Vector2 MinimumSize
	{
		get
		{
			return minSize;
		}
		set
		{
			value = Vector2.Max(Vector2.zero, value.RoundToInt());
			if (value != minSize)
			{
				minSize = value;
				Invalidate();
			}
		}
	}

	public Vector2 MaximumSize
	{
		get
		{
			return maxSize;
		}
		set
		{
			value = Vector2.Max(Vector2.zero, value.RoundToInt());
			if (value != maxSize)
			{
				maxSize = value;
				Invalidate();
			}
		}
	}

	[HideInInspector]
	public int ZOrder
	{
		get
		{
			return zindex;
		}
		set
		{
			if (value != zindex)
			{
				if (parent != null)
				{
					parent.SetControlIndex(this, value);
				}
				else
				{
					zindex = Mathf.Max(-1, value);
				}
				OnZOrderChanged();
			}
		}
	}

	[HideInInspector]
	public int TabIndex
	{
		get
		{
			return tabIndex;
		}
		set
		{
			if (value != tabIndex)
			{
				tabIndex = Mathf.Max(-1, value);
				OnTabIndexChanged();
			}
		}
	}

	public dfList<dfControl> Controls
	{
		get
		{
			return controls;
		}
	}

	public dfControl Parent
	{
		get
		{
			return parent;
		}
	}

	public bool ClipChildren
	{
		get
		{
			return clipChildren;
		}
		set
		{
			if (value != clipChildren)
			{
				clipChildren = value;
				Invalidate();
			}
		}
	}

	public bool InverseClipChildren
	{
		get
		{
			return inverseClipChildren;
		}
		set
		{
			if (value != inverseClipChildren)
			{
				inverseClipChildren = value;
				Invalidate();
			}
		}
	}

	protected bool IsLayoutSuspended
	{
		get
		{
			return ForceSuspendLayout || performingLayout || (layout != null && layout.IsLayoutSuspended);
		}
	}

	protected bool IsPerformingLayout
	{
		get
		{
			if (performingLayout)
			{
				return true;
			}
			if (layout != null && layout.IsPerformingLayout)
			{
				return true;
			}
			return false;
		}
	}

	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	internal uint Version
	{
		get
		{
			return version;
		}
	}

	public bool IsLocalized
	{
		get
		{
			return isLocalized;
		}
		set
		{
			isLocalized = value;
			if (value)
			{
				Localize();
			}
		}
	}

	public Vector2 HotZoneScale
	{
		get
		{
			return hotZoneScale;
		}
		set
		{
			hotZoneScale = Vector2.Max(value, Vector2.zero);
			Invalidate();
		}
	}

	public bool AutoFocus
	{
		get
		{
			return autoFocus;
		}
		set
		{
			if (value != autoFocus)
			{
				autoFocus = value;
				if (value && IsEnabled && CanFocus)
				{
					Focus();
				}
			}
		}
	}

	public virtual bool CanFocus
	{
		get
		{
			return canFocus && IsInteractive;
		}
		set
		{
			canFocus = value;
		}
	}

	public virtual bool ContainsFocus
	{
		get
		{
			return dfGUIManager.ContainsFocus(this);
		}
	}

	public virtual bool HasFocus
	{
		get
		{
			return dfGUIManager.HasFocus(this);
		}
	}

	public bool ContainsMouse
	{
		get
		{
			return isMouseHovering;
		}
	}

	[HideInInspector]
	public int RenderOrder
	{
		get
		{
			return renderOrder;
		}
	}

	[HideInInspector]
	public event ChildControlEventHandler ControlAdded;

	[HideInInspector]
	public event ChildControlEventHandler ControlRemoved;

	public event FocusEventHandler GotFocus;

	public event FocusEventHandler EnterFocus;

	public event FocusEventHandler LostFocus;

	public event FocusEventHandler LeaveFocus;

	public event PropertyChangedEventHandler<bool> ControlShown;

	public event PropertyChangedEventHandler<bool> ControlHidden;

	public event PropertyChangedEventHandler<bool> ControlClippingChanged;

	public event PropertyChangedEventHandler<int> TabIndexChanged;

	public event PropertyChangedEventHandler<Vector2> PositionChanged;

	public event PropertyChangedEventHandler<Vector2> SizeChanged;

	[HideInInspector]
	public event PropertyChangedEventHandler<Color32> ColorChanged;

	public event PropertyChangedEventHandler<bool> IsVisibleChanged;

	public event PropertyChangedEventHandler<bool> IsEnabledChanged;

	[HideInInspector]
	public event PropertyChangedEventHandler<float> OpacityChanged;

	[HideInInspector]
	public event PropertyChangedEventHandler<dfAnchorStyle> AnchorChanged;

	[HideInInspector]
	public event PropertyChangedEventHandler<dfPivotPoint> PivotChanged;

	[HideInInspector]
	public event PropertyChangedEventHandler<int> ZOrderChanged;

	public event DragEventHandler DragStart;

	public event DragEventHandler DragEnd;

	public event DragEventHandler DragDrop;

	public event DragEventHandler DragEnter;

	public event DragEventHandler DragLeave;

	public event DragEventHandler DragOver;

	public event KeyPressHandler KeyPress;

	public event KeyPressHandler KeyDown;

	public event KeyPressHandler KeyUp;

	public event ControlMultiTouchEventHandler MultiTouch;

	public event ControlCallbackHandler MultiTouchEnd;

	public event MouseEventHandler MouseEnter;

	public event MouseEventHandler MouseMove;

	public event MouseEventHandler MouseHover;

	public event MouseEventHandler MouseLeave;

	public event MouseEventHandler MouseDown;

	public event MouseEventHandler MouseUp;

	public event MouseEventHandler MouseWheel;

	public event MouseEventHandler Click;

	public event MouseEventHandler DoubleClick;

	public dfLanguageManager GetLanguageManager()
	{
		if (languageManager == null)
		{
			if (languageManagerChecked)
			{
				return null;
			}
			languageManagerChecked = true;
			languageManager = GetManager().GetComponent<dfLanguageManager>();
			if (languageManager == null)
			{
				languageManager = GameUIRoot.Instance.GetComponent<dfLanguageManager>();
			}
		}
		return languageManager;
	}

	public void ForceUpdateCachedParentTransform()
	{
		cachedParentTransform = base.transform.parent;
	}

	public string GetLocalizationKey()
	{
		return localizationKey;
	}

	internal void setRenderOrder(ref int order)
	{
		renderOrder = ++order;
		int count = controls.Count;
		dfControl[] items = controls.Items;
		for (int i = 0; i < count; i++)
		{
			if (items[i] != null)
			{
				items[i].setRenderOrder(ref order);
			}
		}
	}

	internal virtual void OnDragStart(dfDragEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDragStart", this, args);
			if (!args.Used && this.DragStart != null)
			{
				this.DragStart(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDragStart(args);
		}
	}

	internal virtual void OnDragEnd(dfDragEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDragEnd", this, args);
			if (!args.Used && this.DragEnd != null)
			{
				this.DragEnd(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDragEnd(args);
		}
	}

	internal virtual void OnDragDrop(dfDragEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDragDrop", this, args);
			if (!args.Used && this.DragDrop != null)
			{
				this.DragDrop(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDragDrop(args);
		}
	}

	internal virtual void OnDragEnter(dfDragEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDragEnter", this, args);
			if (!args.Used && this.DragEnter != null)
			{
				this.DragEnter(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDragEnter(args);
		}
	}

	internal virtual void OnDragLeave(dfDragEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDragLeave", this, args);
			if (!args.Used && this.DragLeave != null)
			{
				this.DragLeave(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDragLeave(args);
		}
	}

	internal virtual void OnDragOver(dfDragEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDragOver", this, args);
			if (!args.Used && this.DragOver != null)
			{
				this.DragOver(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDragOver(args);
		}
	}

	protected internal virtual void OnMultiTouchEnd()
	{
		Signal("OnMultiTouchEnd", this);
		if (this.MultiTouchEnd != null)
		{
			this.MultiTouchEnd(this);
		}
		if (parent != null)
		{
			parent.OnMultiTouchEnd();
		}
	}

	protected internal virtual void OnMultiTouch(dfTouchEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnMultiTouch", this, args);
			if (this.MultiTouch != null)
			{
				this.MultiTouch(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMultiTouch(args);
		}
	}

	protected internal virtual void OnMouseEnter(dfMouseEventArgs args)
	{
		isMouseHovering = true;
		if (!args.Used)
		{
			Signal("OnMouseEnter", this, args);
			if (this.MouseEnter != null)
			{
				this.MouseEnter(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseEnter(args);
		}
	}

	protected internal virtual void OnMouseLeave(dfMouseEventArgs args)
	{
		isMouseHovering = false;
		if (!args.Used)
		{
			Signal("OnMouseLeave", this, args);
			if (this.MouseLeave != null)
			{
				this.MouseLeave(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseLeave(args);
		}
	}

	protected internal virtual void OnMouseMove(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnMouseMove", this, args);
			if (this.MouseMove != null)
			{
				this.MouseMove(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseMove(args);
		}
	}

	protected internal virtual void OnMouseHover(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnMouseHover", this, args);
			if (this.MouseHover != null)
			{
				this.MouseHover(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseHover(args);
		}
	}

	protected internal virtual void OnMouseWheel(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnMouseWheel", this, args);
			if (this.MouseWheel != null)
			{
				this.MouseWheel(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseWheel(args);
		}
	}

	protected internal virtual void OnMouseDown(dfMouseEventArgs args)
	{
		if (IsInteractive && IsEnabled && IsVisible && CanFocus && !ContainsFocus)
		{
			Focus();
		}
		if (!args.Used)
		{
			Signal("OnMouseDown", this, args);
			if (this.MouseDown != null)
			{
				this.MouseDown(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseDown(args);
		}
	}

	protected internal virtual void OnMouseUp(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnMouseUp", this, args);
			if (this.MouseUp != null)
			{
				this.MouseUp(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnMouseUp(args);
		}
	}

	protected internal virtual void OnClick(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnClick", this, args);
			if (this.Click != null)
			{
				this.Click(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnClick(args);
		}
	}

	protected internal virtual void OnDoubleClick(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnDoubleClick", this, args);
			if (this.DoubleClick != null)
			{
				this.DoubleClick(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnDoubleClick(args);
		}
	}

	protected internal virtual void OnKeyPress(dfKeyEventArgs args)
	{
		if (IsInteractive && !args.Used)
		{
			Signal("OnKeyPress", this, args);
			if (this.KeyPress != null)
			{
				this.KeyPress(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnKeyPress(args);
		}
	}

	protected internal virtual void OnKeyDown(dfKeyEventArgs args)
	{
		if (IsInteractive && !args.Used)
		{
			if (args.KeyCode == KeyCode.Tab)
			{
				OnTabKeyPressed(args);
			}
			if (!args.Used)
			{
				Signal("OnKeyDown", this, args);
				if (this.KeyDown != null)
				{
					this.KeyDown(this, args);
				}
			}
		}
		if (parent != null)
		{
			parent.OnKeyDown(args);
		}
	}

	protected virtual void OnTabKeyPressed(dfKeyEventArgs args)
	{
		List<dfControl> list = (from c in GetManager().GetComponentsInChildren<dfControl>()
			where c != this && c.TabIndex >= 0 && c.IsInteractive && c.CanFocus && c.IsVisible
			select c).ToList();
		if (list.Count == 0)
		{
			return;
		}
		list.Sort((dfControl lhs, dfControl rhs) => (lhs.TabIndex == rhs.TabIndex) ? lhs.RenderOrder.CompareTo(rhs.RenderOrder) : lhs.TabIndex.CompareTo(rhs.TabIndex));
		if (!args.Shift)
		{
			for (int i = 0; i < list.Count; i++)
			{
				dfControl dfControl2 = list[i];
				if (dfControl2.TabIndex >= TabIndex)
				{
					list[i].Focus();
					args.Use();
					return;
				}
			}
			list[0].Focus();
			args.Use();
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			dfControl dfControl3 = list[num];
			if (dfControl3.TabIndex <= TabIndex)
			{
				list[num].Focus();
				args.Use();
				return;
			}
		}
		list[list.Count - 1].Focus();
		args.Use();
	}

	protected internal virtual void OnKeyUp(dfKeyEventArgs args)
	{
		if (IsInteractive)
		{
			Signal("OnKeyUp", this, args);
			if (this.KeyUp != null)
			{
				this.KeyUp(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnKeyUp(args);
		}
	}

	protected internal virtual void OnEnterFocus(dfFocusEventArgs args)
	{
		Signal("OnEnterFocus", this, args);
		if (this.EnterFocus != null)
		{
			this.EnterFocus(this, args);
		}
	}

	protected internal virtual void OnLeaveFocus(dfFocusEventArgs args)
	{
		Signal("OnLeaveFocus", this, args);
		if (this.LeaveFocus != null)
		{
			this.LeaveFocus(this, args);
		}
	}

	protected internal virtual void OnGotFocus(dfFocusEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnGotFocus", this, args);
			if (this.GotFocus != null)
			{
				this.GotFocus(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnGotFocus(args);
		}
	}

	protected internal virtual void OnLostFocus(dfFocusEventArgs args)
	{
		if (!args.Used)
		{
			Signal("OnLostFocus", this, args);
			if (this.LostFocus != null)
			{
				this.LostFocus(this, args);
			}
		}
		if (parent != null)
		{
			parent.OnLostFocus(args);
		}
	}

	protected internal bool Signal(string eventName, object arg)
	{
		signal1[0] = arg;
		return Signal(base.gameObject, eventName, signal1);
	}

	protected internal bool Signal(string eventName, object arg1, object arg2)
	{
		signal2[0] = arg1;
		signal2[1] = arg2;
		return Signal(base.gameObject, eventName, signal2);
	}

	protected internal bool Signal(string eventName, object arg1, object arg2, object arg3)
	{
		signal3[0] = arg1;
		signal3[1] = arg2;
		signal3[2] = arg3;
		return Signal(base.gameObject, eventName, signal3);
	}

	protected internal bool Signal(string eventName, object[] args)
	{
		return Signal(base.gameObject, eventName, args);
	}

	protected internal bool SignalHierarchy(string eventName, params object[] args)
	{
		if (!allowSignalEvents)
		{
			return false;
		}
		bool flag = false;
		Transform transform = base.transform;
		while (!flag && transform != null)
		{
			flag = Signal(transform.gameObject, eventName, args);
			transform = transform.parent;
		}
		return flag;
	}

	[HideInInspector]
	protected internal bool Signal(GameObject target, string eventName, object arg)
	{
		signal1[0] = arg;
		return Signal(target, eventName, signal1);
	}

	[HideInInspector]
	protected internal bool Signal(GameObject target, string eventName, object[] args)
	{
		if (!allowSignalEvents || target == null || shutdownInProgress || !Application.isPlaying)
		{
			return false;
		}
		MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
		if (components == null || (target == base.gameObject && components.Length == 1))
		{
			return false;
		}
		if (args.Length == 0 || !object.ReferenceEquals(args[0], this))
		{
			object[] array = new object[args.Length + 1];
			Array.Copy(args, 0, array, 1, args.Length);
			array[0] = this;
			args = array;
		}
		bool result = false;
		foreach (MonoBehaviour monoBehaviour in components)
		{
			if (monoBehaviour == null || monoBehaviour.GetType() == null || monoBehaviour == this || ((object)monoBehaviour != null && !monoBehaviour.enabled))
			{
				continue;
			}
			object returnValue = null;
			if (SignalCache.Invoke(monoBehaviour, eventName, args, out returnValue))
			{
				result = true;
				if (returnValue is IEnumerator && (object)monoBehaviour != null)
				{
					monoBehaviour.StartCoroutine((IEnumerator)returnValue);
				}
			}
		}
		return result;
	}

	internal bool IsTopLevelControl(dfGUIManager manager)
	{
		return parent == null && cachedManager == manager;
	}

	internal bool GetIsVisibleRaw()
	{
		return isVisible;
	}

	public void Localize()
	{
		if (!IsLocalized)
		{
			return;
		}
		if (languageManager == null)
		{
			languageManager = GetManager().GetComponent<dfLanguageManager>();
			if (languageManager == null)
			{
				languageManager = GameUIRoot.Instance.GetComponent<dfLanguageManager>();
			}
			if (languageManager == null)
			{
				return;
			}
		}
		OnLocalize();
	}

	public void DoClick()
	{
		Camera camera = GetCamera();
		Vector3 vector = camera.WorldToScreenPoint(GetCenter());
		Ray ray = camera.ScreenPointToRay(vector);
		OnClick(new dfMouseEventArgs(this, dfMouseButtons.Left, 1, ray, vector, 0f));
	}

	[HideInInspector]
	protected internal void RemoveEventHandlers(string eventName)
	{
		FieldInfo fieldInfo = GetType().GetAllFields().FirstOrDefault((FieldInfo f) => typeof(Delegate).IsAssignableFrom(f.FieldType) && f.Name == eventName);
		if (fieldInfo != null)
		{
			fieldInfo.SetValue(this, null);
		}
	}

	[HideInInspector]
	internal void RemoveAllEventHandlers()
	{
		FieldInfo[] array = (from f in GetType().GetAllFields()
			where typeof(Delegate).IsAssignableFrom(f.FieldType)
			select f).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetValue(this, null);
		}
	}

	public void Show()
	{
		IsVisible = true;
	}

	public void Hide()
	{
		IsVisible = false;
	}

	public void Enable()
	{
		IsEnabled = true;
	}

	public void Disable()
	{
		IsEnabled = false;
	}

	public bool HitTest(Ray ray)
	{
		Plane plane = new Plane(base.transform.TransformDirection(Vector3.back), base.transform.position);
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return false;
		}
		Vector3 point = ray.origin + ray.direction * enter;
		Plane[] array = ((!ClipChildren) ? null : GetClippingPlanes());
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
		return true;
	}

	public Vector2 GetHitPosition(Ray ray)
	{
		Vector2 position;
		if (!GetHitPosition(ray, out position, false))
		{
			return Vector2.one * float.MinValue;
		}
		return position;
	}

	public bool GetHitPosition(Ray ray, out Vector2 position)
	{
		return GetHitPosition(ray, out position, true);
	}

	public bool GetHitPosition(Ray ray, out Vector2 position, bool clamp)
	{
		position = Vector2.one * float.MinValue;
		Plane plane = new Plane(base.transform.TransformDirection(Vector3.back), base.transform.position);
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return false;
		}
		Vector3 point = ray.GetPoint(enter);
		Plane[] array = ((!ClipChildren) ? null : GetClippingPlanes());
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
		Vector3[] corners = GetCorners();
		Vector3 vector = corners[0];
		Vector3 vector2 = corners[1];
		Vector3 vector3 = corners[2];
		Vector3 vector4 = closestPointOnLine(vector, vector2, point, clamp);
		float num = (vector4 - vector).magnitude / (vector2 - vector).magnitude;
		float x = size.x * num;
		vector4 = closestPointOnLine(vector, vector3, point, clamp);
		num = (vector4 - vector).magnitude / (vector3 - vector).magnitude;
		float y = size.y * num;
		position = new Vector2(x, y);
		return true;
	}

	public T Find<T>(string controlName) where T : dfControl
	{
		if (base.name == controlName && this is T)
		{
			return (T)this;
		}
		updateControlHierarchy(true);
		for (int i = 0; i < controls.Count; i++)
		{
			T val = controls[i] as T;
			if ((UnityEngine.Object)val != (UnityEngine.Object)null && val.name == controlName)
			{
				return val;
			}
		}
		for (int j = 0; j < controls.Count; j++)
		{
			T val2 = controls[j].Find<T>(controlName);
			if ((UnityEngine.Object)val2 != (UnityEngine.Object)null)
			{
				return val2;
			}
		}
		return (T)null;
	}

	public dfControl Find(string controlName)
	{
		if (base.name == controlName)
		{
			return this;
		}
		updateControlHierarchy(true);
		for (int i = 0; i < controls.Count; i++)
		{
			dfControl dfControl2 = controls[i];
			if (dfControl2.name == controlName)
			{
				return dfControl2;
			}
		}
		for (int j = 0; j < controls.Count; j++)
		{
			dfControl dfControl3 = controls[j].Find(controlName);
			if (dfControl3 != null)
			{
				return dfControl3;
			}
		}
		return null;
	}

	public void Focus(bool allowScrolling = true)
	{
		if (CanFocus && !HasFocus && IsEnabled && IsVisible)
		{
			dfGUIManager.SetFocus(this, allowScrolling);
			Invalidate();
		}
	}

	public void Unfocus()
	{
		if (ContainsFocus)
		{
			dfGUIManager.SetFocus(null);
		}
	}

	public dfControl GetRootContainer()
	{
		dfControl dfControl2 = this;
		while (dfControl2.Parent != null)
		{
			dfControl2 = dfControl2.Parent;
		}
		return dfControl2;
	}

	public virtual void BringToFront()
	{
		if (parent == null)
		{
			GetManager().BringToFront(this);
		}
		else
		{
			parent.SetControlIndex(this, int.MaxValue);
		}
		Invalidate();
	}

	public virtual void SendToBack()
	{
		if (parent == null)
		{
			GetManager().SendToBack(this);
		}
		else
		{
			parent.SetControlIndex(this, 0);
		}
		Invalidate();
	}

	internal dfRenderData Render()
	{
		if (rendering)
		{
			return renderData;
		}
		try
		{
			rendering = true;
			bool flag = isVisible;
			bool flag2 = base.enabled && base.gameObject.activeSelf;
			if (!flag || !flag2)
			{
				return null;
			}
			if (renderData == null)
			{
				renderData = dfRenderData.Obtain();
				isControlInvalidated = true;
			}
			if (isControlInvalidated)
			{
				renderData.Clear();
				OnRebuildRenderData();
				updateCollider();
			}
			renderData.Transform = base.transform.localToWorldMatrix;
			return renderData;
		}
		finally
		{
			isControlInvalidated = false;
			rendering = false;
		}
	}

	[HideInInspector]
	public virtual void Invalidate()
	{
		if (!shutdownInProgress)
		{
			updateVersion();
			isControlInvalidated = true;
			cachedBounds = null;
			dfGUIManager manager = GetManager();
			if (manager != null)
			{
				manager.Invalidate();
			}
			dfRenderGroup.InvalidateGroupForControl(this);
		}
	}

	[HideInInspector]
	public void ResetLayout()
	{
		ResetLayout(false, false);
	}

	[HideInInspector]
	public void ResetLayout(bool recursive, bool force)
	{
		if (shutdownInProgress)
		{
			return;
		}
		bool flag = IsPerformingLayout || IsLayoutSuspended;
		if (!force && flag)
		{
			return;
		}
		if (layout == null)
		{
			layout = new AnchorLayout(anchorStyle, this);
		}
		else
		{
			layout.Attach(this);
			layout.Reset(true);
		}
		if (recursive)
		{
			int count = controls.Count;
			dfControl[] items = controls.Items;
			for (int i = 0; i < count; i++)
			{
				items[i].ResetLayout();
			}
		}
	}

	[HideInInspector]
	public void PerformLayout()
	{
		if (shutdownInProgress || isDisposing || performingLayout)
		{
			return;
		}
		try
		{
			performingLayout = true;
			ensureLayoutExists();
			layout.PerformLayout();
			if (GameUIRoot.Instance != null && GUIManager == GameUIRoot.Instance.Manager)
			{
				updateVersion();
				Vector3 relativePosition = RelativePosition;
				relativePosition = (RelativePosition = relativePosition.Quantize(3f));
			}
			Invalidate();
		}
		finally
		{
			performingLayout = false;
		}
	}

	[HideInInspector]
	public void SuspendLayout()
	{
		ensureLayoutExists();
		layout.SuspendLayout();
		for (int i = 0; i < controls.Count; i++)
		{
			controls[i].SuspendLayout();
		}
	}

	[HideInInspector]
	public void ResumeLayout()
	{
		ensureLayoutExists();
		layout.ResumeLayout();
		for (int i = 0; i < controls.Count; i++)
		{
			controls[i].ResumeLayout();
		}
	}

	public virtual Vector2 CalculateMinimumSize()
	{
		return MinimumSize;
	}

	[HideInInspector]
	public void MakePixelPerfect()
	{
		MakePixelPerfect(true);
	}

	[HideInInspector]
	public void MakePixelPerfect(bool recursive)
	{
	}

	public Bounds GetBounds()
	{
		if (isInteractive && GetComponent<Collider>() != null)
		{
			cachedBounds = GetComponent<Collider>().bounds;
			return cachedBounds.Value;
		}
		if (cachedBounds.HasValue)
		{
			return cachedBounds.Value;
		}
		Vector3[] corners = GetCorners();
		Vector3 vector = corners[0] + (corners[3] - corners[0]) * 0.5f;
		Vector3 vector2 = vector;
		Vector3 vector3 = vector;
		for (int i = 0; i < corners.Length; i++)
		{
			vector2 = Vector3.Min(vector2, corners[i]);
			vector3 = Vector3.Max(vector3, corners[i]);
		}
		cachedBounds = new Bounds(vector, vector3 - vector2);
		return cachedBounds.Value;
	}

	public Vector3 GetCenter()
	{
		Vector3[] corners = GetCorners();
		return corners[0] + (corners[3] - corners[0]) * 0.5f;
	}

	public Vector3 GetAbsolutePosition()
	{
		Vector3 zero = Vector3.zero;
		dfControl dfControl2 = this;
		while (dfControl2 != null)
		{
			zero += dfControl2.getRelativePosition();
			dfControl2 = dfControl2.Parent;
		}
		return zero;
	}

	public Vector3[] GetCorners()
	{
		float num = PixelsToUnits();
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		Vector3 vector = pivot.TransformToUpperLeft(size);
		Vector3 vector2 = vector + new Vector3(size.x, 0f);
		Vector3 vector3 = vector + new Vector3(0f, 0f - size.y);
		Vector3 vector4 = vector2 + new Vector3(0f, 0f - size.y);
		if (cachedCorners == null)
		{
			cachedCorners = new Vector3[4];
		}
		cachedCorners[0] = localToWorldMatrix.MultiplyPoint(vector * num);
		cachedCorners[1] = localToWorldMatrix.MultiplyPoint(vector2 * num);
		cachedCorners[2] = localToWorldMatrix.MultiplyPoint(vector3 * num);
		cachedCorners[3] = localToWorldMatrix.MultiplyPoint(vector4 * num);
		return cachedCorners;
	}

	public Camera GetCamera()
	{
		dfGUIManager manager = GetManager();
		if (manager == null)
		{
			Debug.LogError("The Manager hosting this control could not be determined");
			return null;
		}
		return manager.RenderCamera;
	}

	protected internal virtual RectOffset GetClipPadding()
	{
		return dfRectOffsetExtensions.Empty;
	}

	public Rect GetScreenRect()
	{
		Camera camera = GetCamera();
		Vector3[] corners = GetCorners();
		Vector2 lhs = Vector2.one * float.MaxValue;
		Vector2 lhs2 = Vector2.one * float.MinValue;
		int num = corners.Length;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = camera.WorldToScreenPoint(corners[i]);
			lhs = Vector2.Min(lhs, vector);
			lhs2 = Vector2.Max(lhs2, vector);
		}
		return new Rect(lhs.x, (float)Screen.height - lhs2.y, lhs2.x - lhs.x, lhs2.y - lhs.y);
	}

	public dfGUIManager GetManager()
	{
		if (cachedManager != null || !base.gameObject.activeInHierarchy)
		{
			return cachedManager;
		}
		if (parent != null && parent.cachedManager != null)
		{
			return cachedManager = parent.cachedManager;
		}
		GameObject gameObject = base.gameObject;
		while (gameObject != null)
		{
			dfGUIManager component = gameObject.GetComponent<dfGUIManager>();
			if (component != null)
			{
				return cachedManager = component;
			}
			if (gameObject.transform.parent == null)
			{
				break;
			}
			gameObject = gameObject.transform.parent.gameObject;
		}
		return dfGUIManager.ActiveManagers.FirstOrDefault();
	}

	public float PixelsToUnits()
	{
		if (cachedPixelSize > float.Epsilon)
		{
			return cachedPixelSize;
		}
		dfGUIManager manager = GetManager();
		if (manager == null)
		{
			return 0.0026f;
		}
		return cachedPixelSize = manager.PixelsToUnits();
	}

	protected internal virtual Plane[] GetClippingPlanes()
	{
		Vector3[] corners = GetCorners();
		Vector3 inNormal = base.transform.TransformDirection(Vector3.right);
		Vector3 inNormal2 = base.transform.TransformDirection(Vector3.left);
		Vector3 inNormal3 = base.transform.TransformDirection(Vector3.up);
		Vector3 inNormal4 = base.transform.TransformDirection(Vector3.down);
		cachedClippingPlanes[0] = new Plane(inNormal, corners[0]);
		cachedClippingPlanes[1] = new Plane(inNormal2, corners[1]);
		cachedClippingPlanes[2] = new Plane(inNormal3, corners[2]);
		cachedClippingPlanes[3] = new Plane(inNormal4, corners[0]);
		return cachedClippingPlanes;
	}

	public bool Contains(dfControl child)
	{
		return child != null && child.transform.IsChildOf(base.transform);
	}

	[HideInInspector]
	protected internal virtual void OnLocalize()
	{
		PerformLayout();
	}

	public string ForceGetLocalizedValue(string key)
	{
		dfLanguageManager component = GameUIRoot.Instance.GetComponent<dfLanguageManager>();
		return component.GetValue(key);
	}

	[HideInInspector]
	protected internal string getLocalizedValue(string key)
	{
		if (!IsLocalized || !Application.isPlaying)
		{
			return key;
		}
		if (languageManager == null)
		{
			if (languageManagerChecked)
			{
				return key;
			}
			languageManagerChecked = true;
			languageManager = GetManager().GetComponent<dfLanguageManager>();
			if (languageManager == null)
			{
				languageManager = GameUIRoot.Instance.GetComponent<dfLanguageManager>();
			}
			if (languageManager == null)
			{
				return key;
			}
		}
		string value = languageManager.GetValue(key);
		return value.Replace("\\n", "\n");
	}

	[HideInInspector]
	protected internal void updateCollider()
	{
		BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
		if (boxCollider == null)
		{
			if (GetComponent<Collider>() != null)
			{
				throw new Exception("Invalid collider type on control: " + GetComponent<Collider>().GetType().Name);
			}
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		if (Application.isPlaying && !isInteractive)
		{
			boxCollider.enabled = false;
			return;
		}
		float num = PixelsToUnits();
		Vector2 vector = size * num;
		Vector3 center = pivot.TransformToCenter(vector);
		Vector3 vector2 = new Vector3(vector.x * hotZoneScale.x, vector.y * hotZoneScale.y, 0.001f);
		bool flag = base.enabled && IsVisible;
		boxCollider.isTrigger = false;
		boxCollider.enabled = flag;
		boxCollider.size = vector2;
		boxCollider.center = center;
	}

	[HideInInspector]
	protected virtual void OnRebuildRenderData()
	{
	}

	[HideInInspector]
	protected internal virtual void OnControlAdded(dfControl child)
	{
		Invalidate();
		if (this.ControlAdded != null)
		{
			this.ControlAdded(this, child);
		}
		Signal("OnControlAdded", this, child);
	}

	[HideInInspector]
	protected internal virtual void OnControlRemoved(dfControl child)
	{
		Invalidate();
		if (this.ControlRemoved != null)
		{
			this.ControlRemoved(this, child);
		}
		Signal("OnControlRemoved", this, child);
	}

	[HideInInspector]
	protected internal virtual void OnPositionChanged()
	{
		updateVersion();
		GetManager().Invalidate();
		dfRenderGroup.InvalidateGroupForControl(this);
		cachedPosition = base.transform.localPosition;
		if (isControlInitialized && Application.isPlaying && GetComponent<Rigidbody>() == null)
		{
			Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
			rigidbody.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
			rigidbody.isKinematic = true;
			GetComponent<Rigidbody>().useGravity = false;
			rigidbody.detectCollisions = false;
		}
		ResetLayout();
		if (this.PositionChanged != null)
		{
			this.PositionChanged(this, Position);
		}
	}

	[HideInInspector]
	protected internal virtual void OnSizeChanged()
	{
		updateCollider();
		Invalidate();
		ResetLayout();
		if (Anchor.IsAnyFlagSet(dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical))
		{
			PerformLayout();
		}
		raiseSizeChangedEvent();
		for (int i = 0; i < controls.Count; i++)
		{
			controls[i].PerformLayout();
		}
	}

	[HideInInspector]
	protected internal virtual void OnPivotChanged()
	{
		Invalidate();
		if (Anchor.IsAnyFlagSet(dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical))
		{
			ResetLayout();
			PerformLayout();
		}
		if (this.PivotChanged != null)
		{
			this.PivotChanged(this, pivot);
		}
	}

	[HideInInspector]
	protected internal virtual void OnAnchorChanged()
	{
		ResetLayout();
		if (anchorStyle.IsAnyFlagSet(dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical))
		{
			PerformLayout();
		}
		if (this.AnchorChanged != null)
		{
			this.AnchorChanged(this, anchorStyle);
		}
		Invalidate();
	}

	[HideInInspector]
	protected internal virtual void OnOpacityChanged()
	{
		Invalidate();
		float opacity = Opacity;
		if (this.OpacityChanged != null)
		{
			this.OpacityChanged(this, opacity);
		}
		for (int i = 0; i < controls.Count; i++)
		{
			controls[i].OnOpacityChanged();
		}
	}

	[HideInInspector]
	protected internal virtual void OnColorChanged()
	{
		Invalidate();
		Color32 value = Color;
		if (this.ColorChanged != null)
		{
			this.ColorChanged(this, value);
		}
		for (int i = 0; i < controls.Count; i++)
		{
			controls[i].OnColorChanged();
		}
	}

	[HideInInspector]
	protected internal virtual void OnZOrderChanged()
	{
		if (this.ZOrderChanged != null)
		{
			this.ZOrderChanged(this, zindex);
		}
		Invalidate();
	}

	[HideInInspector]
	protected internal virtual void OnTabIndexChanged()
	{
		Invalidate();
		if (this.TabIndexChanged != null)
		{
			this.TabIndexChanged(this, tabIndex);
		}
	}

	[HideInInspector]
	protected virtual void OnControlClippingChanged()
	{
		if (this.ControlClippingChanged != null)
		{
			this.ControlClippingChanged(this, isControlClipped);
		}
		Signal("OnControlClippingChanged", this, isControlClipped);
	}

	[HideInInspector]
	protected internal virtual void OnIsVisibleChanged()
	{
		updateCollider();
		bool flag = IsVisible;
		if (HasFocus && !flag)
		{
			dfGUIManager.SetFocus(null);
		}
		Signal("OnIsVisibleChanged", this, flag);
		if (this.IsVisibleChanged != null)
		{
			this.IsVisibleChanged(this, flag);
		}
		dfControl[] items = controls.Items;
		int count = controls.Count;
		for (int i = 0; i < count; i++)
		{
			items[i].OnIsVisibleChanged();
		}
		if (flag)
		{
			if (this.ControlShown != null)
			{
				this.ControlShown(this, true);
			}
			Signal("OnControlShown", this, true);
		}
		else
		{
			if (this.ControlHidden != null)
			{
				this.ControlHidden(this, true);
			}
			Signal("OnControlHidden", this, false);
		}
		Invalidate();
		if (flag)
		{
			doAutoFocus();
		}
		else if (!Application.isPlaying)
		{
			BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
			boxCollider.size = Vector3.zero;
		}
	}

	[HideInInspector]
	protected internal virtual void OnIsEnabledChanged()
	{
		if (!shutdownInProgress)
		{
			bool flag = IsEnabled && base.enabled;
			updateCollider();
			if (dfGUIManager.ContainsFocus(this) && !flag)
			{
				dfGUIManager.SetFocus(null);
			}
			Invalidate();
			Signal("OnIsEnabledChanged", this, flag);
			if (this.IsEnabledChanged != null)
			{
				this.IsEnabledChanged(this, flag);
			}
			for (int i = 0; i < controls.Count; i++)
			{
				controls[i].OnIsEnabledChanged();
			}
			doAutoFocus();
		}
	}

	protected internal float CalculateOpacity()
	{
		if (parent == null)
		{
			return Opacity;
		}
		return Opacity * parent.CalculateOpacity();
	}

	protected internal Color32 ApplyOpacity(Color32 rawColor)
	{
		float num = CalculateOpacity();
		rawColor.a = (byte)((float)(int)rawColor.a * num);
		return rawColor;
	}

	protected internal Vector2 GetHitPosition(dfMouseEventArgs args)
	{
		Vector2 position;
		GetHitPosition(args.Ray, out position);
		return position;
	}

	protected internal Vector3 getScaledDirection(Vector3 direction)
	{
		Vector3 localScale = GetManager().transform.localScale;
		direction = base.transform.TransformDirection(direction);
		return Vector3.Scale(direction, localScale);
	}

	protected internal Vector3 transformOffset(Vector3 offset)
	{
		Vector3 vector = offset.x * getScaledDirection(Vector3.right);
		Vector3 vector2 = offset.y * getScaledDirection(Vector3.down);
		return (vector + vector2) * PixelsToUnits();
	}

	protected internal virtual void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution)
	{
		updateControlHierarchy();
		cachedPixelSize = 0f;
		Vector3 vector = base.transform.localPosition / (2f / previousResolution.y);
		Vector3 localPosition = vector * (2f / currentResolution.y);
		base.transform.localPosition = localPosition;
		cachedPosition = localPosition;
		Vector3 relativePosition = RelativePosition;
		if (Anchor.IsAnyFlagSet(dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical | dfAnchorStyle.Proportional))
		{
			PerformLayout();
		}
		updateCollider();
		if (ResolutionChangedPostLayout != null)
		{
			ResolutionChangedPostLayout(this, relativePosition, RelativePosition);
		}
		Signal("OnResolutionChanged", this, previousResolution, currentResolution);
		Invalidate();
	}

	[HideInInspector]
	public virtual void Awake()
	{
		cachedParentTransform = base.transform.parent;
		if (anchorStyle == dfAnchorStyle.None && layout != null)
		{
			anchorStyle = layout.AnchorStyle;
		}
		if (GetComponent<Collider>() == null)
		{
			base.gameObject.AddComponent<BoxCollider>();
		}
	}

	[HideInInspector]
	public virtual void Start()
	{
	}

	[HideInInspector]
	public virtual void OnEnable()
	{
		cachedManager = null;
		cachedBounds = null;
		cachedChildCount = 0;
		cachedParentTransform = base.transform.parent;
		cachedPosition = Vector3.zero;
		cachedRelativePosition = Vector3.zero;
		cachedRotation = Quaternion.identity;
		cachedScale = Vector3.one;
		ActiveInstances.Add(this);
		initializeControl();
		if (Application.isPlaying && IsLocalized)
		{
			Localize();
		}
		OnIsEnabledChanged();
	}

	[HideInInspector]
	public virtual void OnApplicationQuit()
	{
		shutdownInProgress = true;
		RemoveAllEventHandlers();
	}

	[HideInInspector]
	public virtual void OnDisable()
	{
		ActiveInstances.Remove(this);
		try
		{
			Invalidate();
			if (dfGUIManager.HasFocus(this))
			{
				dfGUIManager.SetFocus(null);
			}
			else if (dfGUIManager.GetModalControl() == this)
			{
				dfGUIManager.PopModal();
			}
			OnIsEnabledChanged();
		}
		catch
		{
		}
		finally
		{
			isControlInitialized = false;
		}
	}

	[HideInInspector]
	public virtual void OnDestroy()
	{
		isDisposing = true;
		isControlInitialized = false;
		if (Application.isPlaying)
		{
			RemoveAllEventHandlers();
		}
		if (dfGUIManager.GetModalControl() == this)
		{
			dfGUIManager.PopModal();
		}
		if (layout != null)
		{
			layout.Dispose();
		}
		if (parent != null && parent.controls != null && !parent.isDisposing && parent.controls.Remove(this))
		{
			parent.cachedChildCount--;
			parent.OnControlRemoved(this);
		}
		for (int i = 0; i < controls.Count; i++)
		{
			if (controls[i].layout != null)
			{
				controls[i].layout.Dispose();
				controls[i].layout = null;
			}
			controls[i].parent = null;
		}
		controls.Release();
		if (cachedManager != null)
		{
			cachedManager.Invalidate();
		}
		if (renderData != null)
		{
			renderData.Release();
		}
		layout = null;
		cachedManager = null;
		parent = null;
		cachedClippingPlanes = null;
		cachedCorners = null;
		renderData = null;
		controls = null;
	}

	[HideInInspector]
	public virtual void LateUpdate()
	{
		if (layout != null && layout.HasPendingLayoutRequest)
		{
			layout.PerformLayout();
		}
	}

	[HideInInspector]
	public virtual void Update()
	{
		if (PrecludeUpdateCycle)
		{
			return;
		}
		if (!isControlInitialized)
		{
			initializeControl();
		}
		if (m_transform == null)
		{
			m_transform = base.transform;
		}
		if (m_transform.parent != cachedParentTransform)
		{
			cachedManager = null;
			GetManager();
			cachedParentTransform = m_transform.parent;
			ResetLayout(false, true);
		}
		updateControlHierarchy();
		if (m_transform.hasChanged)
		{
			cachedBounds = null;
			if (cachedScale != m_transform.localScale)
			{
				cachedScale = m_transform.localScale;
				Invalidate();
			}
			if (Vector3.Distance(cachedPosition, m_transform.localPosition) > float.Epsilon)
			{
				cachedPosition = m_transform.localPosition;
				OnPositionChanged();
			}
			if (cachedRotation != m_transform.localRotation)
			{
				cachedRotation = m_transform.localRotation;
				Invalidate();
			}
			m_transform.hasChanged = false;
		}
	}

	protected internal void SetControlIndex(dfControl child, int zorder)
	{
		if (zorder < 0)
		{
			zorder = int.MaxValue;
		}
		controls.Sort();
		controls.Remove(child);
		if (zorder >= controls.Count)
		{
			controls.Add(child);
		}
		else
		{
			controls.Insert(zorder, child);
		}
		child.zindex = zorder;
		for (int i = 0; i < controls.Count; i++)
		{
			if (controls[i].zindex != i)
			{
				dfControl dfControl2 = controls[i];
				dfControl2.zindex = i;
				dfControl2.OnZOrderChanged();
			}
		}
	}

	public T AddControl<T>() where T : dfControl
	{
		return (T)AddControl(typeof(T));
	}

	public dfControl AddControl(Type controlType)
	{
		if (!typeof(dfControl).IsAssignableFrom(controlType))
		{
			throw new InvalidCastException(string.Format("Type {0} does not inherit from {1}", controlType.Name, typeof(dfControl).Name));
		}
		GameObject gameObject = new GameObject(controlType.Name);
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		Vector2 vector = Size * PixelsToUnits() * 0.5f;
		gameObject.transform.localPosition = new Vector3(vector.x, vector.y, 0f);
		dfControl dfControl2 = gameObject.AddComponent(controlType) as dfControl;
		dfControl2.parent = this;
		dfControl2.cachedManager = cachedManager;
		dfControl2.zindex = -1;
		AddControl(dfControl2);
		return dfControl2;
	}

	public void AddControl(dfControl child)
	{
		if (child.transform == null)
		{
			throw new NullReferenceException("The child control does not have a Transform");
		}
		if (child.parent != null && child.parent != this)
		{
			child.parent.RemoveControl(child);
		}
		if (!controls.Contains(child))
		{
			controls.Add(child);
			child.parent = this;
			child.transform.parent = base.transform;
			child.cachedManager = cachedManager;
			child.cachedParentTransform = base.transform;
		}
		if (child.zindex == -1 || child.zindex == int.MaxValue)
		{
			SetControlIndex(child, int.MaxValue);
		}
		else
		{
			controls.Sort();
		}
		OnControlAdded(child);
		child.Invalidate();
	}

	public dfControl AddPrefab(GameObject prefab)
	{
		if (prefab.GetComponent<dfControl>() == null)
		{
			throw new InvalidCastException();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		dfControl component = gameObject.GetComponent<dfControl>();
		component.parent = this;
		component.zindex = -1;
		AddControl(component);
		return component;
	}

	private int getMaxZOrder()
	{
		int num = -1;
		for (int i = 0; i < controls.Count; i++)
		{
			num = Mathf.Max(controls[i].zindex, num);
		}
		return num;
	}

	public void RemoveControl(dfControl child)
	{
		if (!isDisposing)
		{
			if (child.Parent == this)
			{
				child.parent = null;
			}
			if (controls.Remove(child))
			{
				OnControlRemoved(child);
				child.Invalidate();
				Invalidate();
			}
		}
	}

	[HideInInspector]
	public void RebuildControlOrder()
	{
		controls.Sort();
		for (int i = 0; i < controls.Count; i++)
		{
			if (controls[i].zindex != i)
			{
				dfControl dfControl2 = controls[i];
				dfControl2.zindex = i;
				dfControl2.OnZOrderChanged();
			}
		}
	}

	internal void setClippingState(bool isClipped)
	{
		if (isClipped != isControlClipped)
		{
			isControlClipped = isClipped;
			OnControlClippingChanged();
		}
	}

	private void doAutoFocus()
	{
		if (Application.isPlaying && IsEnabled && base.enabled && AutoFocus && CanFocus && IsVisible && base.gameObject.activeSelf && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(focusOnNextFrame());
		}
	}

	private IEnumerator focusOnNextFrame()
	{
		yield return null;
		Focus();
	}

	protected void raiseSizeChangedEvent()
	{
		if (this.SizeChanged != null)
		{
			this.SizeChanged(this, Size);
		}
	}

	protected void raiseMouseDownEvent(dfMouseEventArgs args)
	{
		if (this.MouseDown != null)
		{
			this.MouseDown(this, args);
		}
	}

	protected void raiseMouseMoveEvent(dfMouseEventArgs args)
	{
		if (this.MouseMove != null)
		{
			this.MouseMove(this, args);
		}
	}

	protected void raiseMouseWheelEvent(dfMouseEventArgs args)
	{
		if (this.MouseWheel != null)
		{
			this.MouseWheel(this, args);
		}
	}

	private void initializeControl()
	{
		Transform transform = base.transform.parent;
		if (transform == null || transform.GetComponent(typeof(IDFControlHost)) == null)
		{
			return;
		}
		if (transform != null || cachedParentTransform != transform)
		{
			dfControl component = transform.GetComponent<dfControl>();
			if (component != null)
			{
				parent = component;
				component.AddControl(this);
			}
			if (controls == null)
			{
				updateControlHierarchy();
			}
		}
		if (renderOrder == -1)
		{
			renderOrder = ZOrder;
		}
		updateCollider();
		ensureLayoutExists();
		layout.Attach(this);
		if (!Application.isPlaying)
		{
			PerformLayout();
		}
		Invalidate();
		isControlInitialized = true;
	}

	internal void updateControlHierarchy()
	{
		updateControlHierarchy(false);
	}

	internal void updateControlHierarchy(bool force)
	{
		int childCount = base.transform.childCount;
		if (!force && childCount == cachedChildCount)
		{
			return;
		}
		cachedChildCount = childCount;
		dfList<dfControl> childControls = getChildControls();
		for (int i = 0; i < childControls.Count; i++)
		{
			dfControl dfControl2 = childControls[i];
			if (!controls.Contains(dfControl2))
			{
				dfControl2.parent = this;
				dfControl2.cachedParentTransform = base.transform;
				if (!Application.isPlaying)
				{
					dfControl2.ResetLayout();
				}
				OnControlAdded(dfControl2);
				dfControl2.updateControlHierarchy();
			}
		}
		for (int j = 0; j < controls.Count; j++)
		{
			dfControl dfControl3 = controls[j];
			if (dfControl3 == null || !childControls.Contains(dfControl3))
			{
				OnControlRemoved(dfControl3);
				if (dfControl3 != null && dfControl3.parent == this)
				{
					dfControl3.parent = null;
				}
			}
		}
		controls.Release();
		controls = childControls;
		RebuildControlOrder();
	}

	private dfList<dfControl> getChildControls()
	{
		int childCount = base.transform.childCount;
		dfList<dfControl> dfList2 = dfList<dfControl>.Obtain(childCount);
		for (int i = 0; i < childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				dfControl component = child.GetComponent<dfControl>();
				if (component != null)
				{
					dfList2.Add(component);
				}
			}
		}
		return dfList2;
	}

	private void ensureLayoutExists()
	{
		if (layout == null)
		{
			dfAnchorStyle dfAnchorStyle2 = ((anchorStyle == dfAnchorStyle.None) ? (dfAnchorStyle.Top | dfAnchorStyle.Left) : anchorStyle);
			layout = new AnchorLayout(dfAnchorStyle2, this);
			anchorStyle = dfAnchorStyle2;
		}
		else
		{
			layout.Attach(this);
		}
		int num = 0;
		while (Controls != null && num < Controls.Count)
		{
			if (controls[num] != null)
			{
				controls[num].ensureLayoutExists();
			}
			num++;
		}
	}

	protected internal void updateVersion()
	{
		version = ++versionCounter;
	}

	private Vector3 getRelativePosition()
	{
		if (relativePositionCacheVersion == version)
		{
			return cachedRelativePosition;
		}
		relativePositionCacheVersion = version;
		if (base.transform.parent == null)
		{
			return Vector3.zero;
		}
		if (parent != null)
		{
			float num = PixelsToUnits();
			Vector3 position = base.transform.parent.position;
			Vector3 position2 = base.transform.position;
			Transform transform = base.transform.parent;
			Vector3 vector = transform.InverseTransformPoint(position / num);
			vector += parent.pivot.TransformToUpperLeft(parent.size);
			Vector3 vector2 = transform.InverseTransformPoint(position2 / num);
			vector2 += pivot.TransformToUpperLeft(size);
			Vector3 vector3 = vector2 - vector;
			return cachedRelativePosition = vector3.Scale(1f, -1f, 1f);
		}
		dfGUIManager manager = GetManager();
		if (manager == null)
		{
			Debug.LogError("Cannot get position: View not found");
			relativePositionCacheVersion = uint.MaxValue;
			return Vector3.zero;
		}
		float num2 = PixelsToUnits();
		Vector3 point = base.transform.position + pivot.TransformToUpperLeft(size) * num2;
		Plane[] clippingPlanes = manager.GetClippingPlanes();
		float x = clippingPlanes[0].GetDistanceToPoint(point) / num2;
		float y = clippingPlanes[3].GetDistanceToPoint(point) / num2;
		cachedRelativePosition = new Vector3(x, y).RoundToInt();
		return cachedRelativePosition;
	}

	private void setPositionInternal(Vector3 value)
	{
		value += pivot.UpperLeftToTransform(Size);
		value *= PixelsToUnits();
		if (!(Vector3.Distance(value, cachedPosition) <= float.Epsilon))
		{
			Vector3 localPosition = value;
			base.transform.localPosition = localPosition;
			cachedPosition = localPosition;
			OnPositionChanged();
		}
	}

	private void setRelativePosition(ref Vector3 value)
	{
		if (base.transform.parent == null)
		{
			Debug.LogError("Cannot set relative position without a parent Transform.");
		}
		else
		{
			if ((value - getRelativePosition()).magnitude <= float.Epsilon)
			{
				return;
			}
			if (parent != null)
			{
				Vector3 vector = value.Scale(1f, -1f, 1f) + pivot.UpperLeftToTransform(size) - parent.pivot.UpperLeftToTransform(parent.size);
				vector *= PixelsToUnits();
				if ((vector - base.transform.localPosition).sqrMagnitude >= float.Epsilon)
				{
					base.transform.localPosition = vector;
					cachedPosition = vector;
					OnPositionChanged();
				}
				return;
			}
			dfGUIManager manager = GetManager();
			if (manager == null)
			{
				Debug.LogError("Cannot get position: View not found");
				return;
			}
			Vector3[] corners = manager.GetCorners();
			Vector3 vector2 = corners[0];
			float num = PixelsToUnits();
			value = value.Scale(1f, -1f, 1f) * num;
			Vector3 vector3 = pivot.UpperLeftToTransform(Size) * num;
			Vector3 vector4 = vector2 + manager.transform.TransformDirection(value) + vector3;
			if (Vector3.Distance(vector4, cachedPosition) > float.Epsilon)
			{
				base.transform.position = vector4;
				cachedPosition = base.transform.localPosition;
				OnPositionChanged();
			}
		}
	}

	private static Vector3 closestPointOnLine(Vector3 start, Vector3 end, Vector3 test, bool clamp)
	{
		Vector3 rhs = test - start;
		Vector3 normalized = (end - start).normalized;
		float magnitude = (end - start).magnitude;
		float num = Vector3.Dot(normalized, rhs);
		if (clamp)
		{
			if (num < 0f)
			{
				return start;
			}
			if (num > magnitude)
			{
				return end;
			}
		}
		normalized *= num;
		return start + normalized;
	}

	public int CompareTo(dfControl other)
	{
		if (ZOrder < 0)
		{
			return (other.ZOrder >= 0) ? 1 : 0;
		}
		return ZOrder.CompareTo(other.ZOrder);
	}
}
