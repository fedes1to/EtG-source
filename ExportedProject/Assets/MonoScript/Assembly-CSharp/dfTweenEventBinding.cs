using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Tweens/Tween Event Binding")]
public class dfTweenEventBinding : MonoBehaviour
{
	public Component Tween;

	public Component EventSource;

	public string StartEvent;

	public string StopEvent;

	public string ResetEvent;

	private bool isBound;

	private dfEventBinding startEventBinding;

	private dfEventBinding stopEventBinding;

	private dfEventBinding resetEventBinding;

	private void OnEnable()
	{
		if (isValid())
		{
			Bind();
		}
	}

	private void Start()
	{
		if (isValid())
		{
			Bind();
		}
	}

	private void OnDisable()
	{
		Unbind();
	}

	public void Bind()
	{
		if (!isBound || isValid())
		{
			isBound = true;
			if (!string.IsNullOrEmpty(StartEvent))
			{
				startEventBinding = bindEvent(StartEvent, "Play");
			}
			if (!string.IsNullOrEmpty(StopEvent))
			{
				stopEventBinding = bindEvent(StopEvent, "Stop");
			}
			if (!string.IsNullOrEmpty(ResetEvent))
			{
				resetEventBinding = bindEvent(ResetEvent, "Reset");
			}
		}
	}

	public void Unbind()
	{
		if (isBound)
		{
			isBound = false;
			if (startEventBinding != null)
			{
				startEventBinding.Unbind();
				startEventBinding = null;
			}
			if (stopEventBinding != null)
			{
				stopEventBinding.Unbind();
				stopEventBinding = null;
			}
			if (resetEventBinding != null)
			{
				resetEventBinding.Unbind();
				resetEventBinding = null;
			}
		}
	}

	private bool isValid()
	{
		if (Tween == null || !(Tween is dfTweenComponentBase))
		{
			return false;
		}
		if (EventSource == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(StartEvent) && string.IsNullOrEmpty(StopEvent) && string.IsNullOrEmpty(ResetEvent))
		{
			return false;
		}
		Type type = EventSource.GetType();
		if (!string.IsNullOrEmpty(StartEvent) && getField(type, StartEvent) == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(StopEvent) && getField(type, StopEvent) == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ResetEvent) && getField(type, ResetEvent) == null)
		{
			return false;
		}
		return true;
	}

	private FieldInfo getField(Type type, string fieldName)
	{
		return (from f in type.GetAllFields()
			where f.Name == fieldName
			select f).FirstOrDefault();
	}

	private void unbindEvent(FieldInfo eventField, Delegate eventDelegate)
	{
		Delegate source = (Delegate)eventField.GetValue(EventSource);
		Delegate value = Delegate.Remove(source, eventDelegate);
		eventField.SetValue(EventSource, value);
	}

	private dfEventBinding bindEvent(string eventName, string handlerName)
	{
		MethodInfo method = Tween.GetType().GetMethod(handlerName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new MissingMemberException("Method not found: " + handlerName);
		}
		dfEventBinding dfEventBinding2 = base.gameObject.AddComponent<dfEventBinding>();
		dfEventBinding2.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
		dfEventBinding2.DataSource = new dfComponentMemberInfo
		{
			Component = EventSource,
			MemberName = eventName
		};
		dfEventBinding2.DataTarget = new dfComponentMemberInfo
		{
			Component = Tween,
			MemberName = handlerName
		};
		dfEventBinding2.Bind();
		return dfEventBinding2;
	}
}
