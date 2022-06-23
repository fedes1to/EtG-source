using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Data Binding/Event Binding")]
public class dfEventBinding : MonoBehaviour, IDataBindingComponent
{
	public dfComponentMemberInfo DataSource;

	public dfComponentMemberInfo DataTarget;

	public bool AutoBind = true;

	public bool AutoUnbind = true;

	private bool isBound;

	private Component sourceComponent;

	private Component targetComponent;

	private EventInfo eventInfo;

	private FieldInfo eventField;

	private Delegate eventDelegate;

	private MethodInfo handlerProxy;

	private ParameterInfo[] handlerParameters;

	public bool IsBound
	{
		get
		{
			return isBound;
		}
	}

	public void OnEnable()
	{
		if (AutoBind && DataSource != null && !isBound && DataSource.IsValid && DataTarget.IsValid)
		{
			Bind();
		}
	}

	public void Start()
	{
		if (AutoBind && DataSource != null && !isBound && DataSource.IsValid && DataTarget.IsValid)
		{
			Bind();
		}
	}

	public void OnDisable()
	{
		if (AutoUnbind)
		{
			Unbind();
		}
	}

	public void OnDestroy()
	{
		Unbind();
	}

	public void Bind()
	{
		if (isBound || DataSource == null)
		{
			return;
		}
		if (!DataSource.IsValid || !DataTarget.IsValid)
		{
			Debug.LogError(string.Format("Invalid event binding configuration - Source:{0}, Target:{1}", DataSource, DataTarget));
			return;
		}
		sourceComponent = DataSource.Component;
		targetComponent = DataTarget.Component;
		MethodInfo method = DataTarget.GetMethod();
		if (method == null)
		{
			Debug.LogError("Event handler not found: " + targetComponent.GetType().Name + "." + DataTarget.MemberName);
		}
		else if (bindToEventProperty(method))
		{
			isBound = true;
		}
		else if (bindToEventField(method))
		{
			isBound = true;
		}
	}

	public void Unbind()
	{
		if (isBound)
		{
			isBound = false;
			if (eventField != null)
			{
				Delegate source = (Delegate)eventField.GetValue(sourceComponent);
				Delegate value = Delegate.Remove(source, eventDelegate);
				eventField.SetValue(sourceComponent, value);
			}
			else if (eventInfo != null)
			{
				MethodInfo removeMethod = eventInfo.GetRemoveMethod();
				removeMethod.Invoke(sourceComponent, new object[1] { eventDelegate });
			}
			eventInfo = null;
			eventField = null;
			eventDelegate = null;
			handlerProxy = null;
			sourceComponent = null;
			targetComponent = null;
		}
	}

	public override string ToString()
	{
		string text = ((DataSource == null || !(DataSource.Component != null)) ? "[null]" : DataSource.Component.GetType().Name);
		string text2 = ((DataSource == null || string.IsNullOrEmpty(DataSource.MemberName)) ? "[null]" : DataSource.MemberName);
		string text3 = ((DataTarget == null || !(DataTarget.Component != null)) ? "[null]" : DataTarget.Component.GetType().Name);
		string text4 = ((DataTarget == null || string.IsNullOrEmpty(DataTarget.MemberName)) ? "[null]" : DataTarget.MemberName);
		return string.Format("Bind {0}.{1} -> {2}.{3}", text, text2, text3, text4);
	}

	[dfEventProxy]
	[HideInInspector]
	public void NotificationEventProxy()
	{
		callProxyEventHandler();
	}

	[HideInInspector]
	[dfEventProxy]
	public void GenericCallbackProxy(object sender)
	{
		callProxyEventHandler(sender);
	}

	[HideInInspector]
	[dfEventProxy]
	public void AnimationEventProxy(dfTweenPlayableBase tween)
	{
		callProxyEventHandler(tween);
	}

	[dfEventProxy]
	[HideInInspector]
	public void MouseEventProxy(dfControl control, dfMouseEventArgs mouseEvent)
	{
		callProxyEventHandler(control, mouseEvent);
	}

	[dfEventProxy]
	[HideInInspector]
	public void KeyEventProxy(dfControl control, dfKeyEventArgs keyEvent)
	{
		callProxyEventHandler(control, keyEvent);
	}

	[dfEventProxy]
	[HideInInspector]
	public void DragEventProxy(dfControl control, dfDragEventArgs dragEvent)
	{
		callProxyEventHandler(control, dragEvent);
	}

	[dfEventProxy]
	[HideInInspector]
	public void ChildControlEventProxy(dfControl container, dfControl child)
	{
		callProxyEventHandler(container, child);
	}

	[dfEventProxy]
	[HideInInspector]
	public void FocusEventProxy(dfControl control, dfFocusEventArgs args)
	{
		callProxyEventHandler(control, args);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, int value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, float value)
	{
		callProxyEventHandler(control, value);
	}

	[dfEventProxy]
	[HideInInspector]
	public void PropertyChangedProxy(dfControl control, bool value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, string value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, Vector2 value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, Vector3 value)
	{
		callProxyEventHandler(control, value);
	}

	[dfEventProxy]
	[HideInInspector]
	public void PropertyChangedProxy(dfControl control, Vector4 value)
	{
		callProxyEventHandler(control, value);
	}

	[dfEventProxy]
	[HideInInspector]
	public void PropertyChangedProxy(dfControl control, Quaternion value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, dfButton.ButtonState value)
	{
		callProxyEventHandler(control, value);
	}

	[dfEventProxy]
	[HideInInspector]
	public void PropertyChangedProxy(dfControl control, dfPivotPoint value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, Texture value)
	{
		callProxyEventHandler(control, value);
	}

	[dfEventProxy]
	[HideInInspector]
	public void PropertyChangedProxy(dfControl control, Texture2D value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void PropertyChangedProxy(dfControl control, Material value)
	{
		callProxyEventHandler(control, value);
	}

	[HideInInspector]
	[dfEventProxy]
	public void SystemEventHandlerProxy(object sender, EventArgs args)
	{
		callProxyEventHandler(sender, args);
	}

	private bool bindToEventField(MethodInfo eventHandler)
	{
		eventField = getField(sourceComponent, DataSource.MemberName);
		if (eventField == null)
		{
			return false;
		}
		try
		{
			MethodInfo method = eventField.FieldType.GetMethod("Invoke");
			ParameterInfo[] parameters = method.GetParameters();
			ParameterInfo[] parameters2 = eventHandler.GetParameters();
			if (parameters.Length == parameters2.Length && method.ReturnType == eventHandler.ReturnType)
			{
				eventDelegate = Delegate.CreateDelegate(eventField.FieldType, targetComponent, eventHandler, true);
			}
			else
			{
				eventDelegate = createEventProxyDelegate(targetComponent, eventField.FieldType, parameters, eventHandler);
			}
			Delegate value = Delegate.Combine(eventDelegate, (Delegate)eventField.GetValue(sourceComponent));
			eventField.SetValue(sourceComponent, value);
		}
		catch (Exception ex)
		{
			base.enabled = false;
			string message = string.Format("Event binding failed - Failed to create event handler for {0} ({1}) - {2}", DataSource, eventHandler, ex.ToString());
			Debug.LogError(message, this);
			return false;
		}
		return true;
	}

	private bool bindToEventProperty(MethodInfo eventHandler)
	{
		eventInfo = sourceComponent.GetType().GetEvent(DataSource.MemberName);
		if (eventInfo == null)
		{
			return false;
		}
		try
		{
			Type eventHandlerType = eventInfo.EventHandlerType;
			MethodInfo addMethod = eventInfo.GetAddMethod();
			MethodInfo method = eventHandlerType.GetMethod("Invoke");
			ParameterInfo[] parameters = method.GetParameters();
			ParameterInfo[] parameters2 = eventHandler.GetParameters();
			if (parameters.Length == parameters2.Length && method.ReturnType == eventHandler.ReturnType)
			{
				eventDelegate = Delegate.CreateDelegate(eventHandlerType, targetComponent, eventHandler, true);
			}
			else
			{
				eventDelegate = createEventProxyDelegate(targetComponent, eventHandlerType, parameters, eventHandler);
			}
			addMethod.Invoke(DataSource.Component, new object[1] { eventDelegate });
		}
		catch (Exception ex)
		{
			base.enabled = false;
			string message = string.Format("Event binding failed - Failed to create event handler for {0} ({1}) - {2}", DataSource, eventHandler, ex.ToString());
			Debug.LogError(message, this);
			return false;
		}
		return true;
	}

	private void callProxyEventHandler(params object[] arguments)
	{
		if (handlerProxy != null)
		{
			if (handlerParameters.Length == 0)
			{
				arguments = null;
			}
			object obj = handlerProxy.Invoke(targetComponent, arguments);
			if (obj is IEnumerator && targetComponent is MonoBehaviour)
			{
				((MonoBehaviour)targetComponent).StartCoroutine((IEnumerator)obj);
			}
		}
	}

	private static FieldInfo getField(Component component, string fieldName)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		return component.GetType().GetAllFields().FirstOrDefault((FieldInfo f) => f.Name == fieldName);
	}

	private Delegate createEventProxyDelegate(object target, Type delegateType, ParameterInfo[] eventParams, MethodInfo eventHandler)
	{
		MethodInfo methodInfo = (from m in typeof(dfEventBinding).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where m.IsDefined(typeof(dfEventProxyAttribute), true) && signatureIsCompatible(eventParams, m.GetParameters())
			select m).FirstOrDefault();
		if (methodInfo == null)
		{
			return null;
		}
		handlerProxy = eventHandler;
		handlerParameters = eventHandler.GetParameters();
		return Delegate.CreateDelegate(delegateType, this, methodInfo, true);
	}

	private bool signatureIsCompatible(ParameterInfo[] lhs, ParameterInfo[] rhs)
	{
		if (lhs == null || rhs == null)
		{
			return false;
		}
		if (lhs.Length != rhs.Length)
		{
			return false;
		}
		for (int i = 0; i < lhs.Length; i++)
		{
			if (!areTypesCompatible(lhs[i], rhs[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool areTypesCompatible(ParameterInfo lhs, ParameterInfo rhs)
	{
		if (lhs.ParameterType.Equals(rhs.ParameterType))
		{
			return true;
		}
		if (lhs.ParameterType.IsAssignableFrom(rhs.ParameterType))
		{
			return true;
		}
		return false;
	}
}
