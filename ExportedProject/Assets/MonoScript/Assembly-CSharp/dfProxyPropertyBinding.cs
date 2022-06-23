using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Data Binding/Proxy Property Binding")]
public class dfProxyPropertyBinding : MonoBehaviour, IDataBindingComponent
{
	public dfComponentMemberInfo DataSource;

	public dfComponentMemberInfo DataTarget;

	public bool TwoWay;

	private dfObservableProperty sourceProperty;

	private dfObservableProperty targetProperty;

	private bool isBound;

	private bool eventsAttached;

	public bool IsBound
	{
		get
		{
			return isBound;
		}
	}

	public void Awake()
	{
	}

	public void OnEnable()
	{
		if (!isBound && IsDataSourceValid() && DataTarget.IsValid)
		{
			Bind();
		}
	}

	public void Start()
	{
		if (!isBound && IsDataSourceValid() && DataTarget.IsValid)
		{
			Bind();
		}
	}

	public void OnDisable()
	{
		Unbind();
	}

	public void Update()
	{
		if (sourceProperty != null && targetProperty != null)
		{
			if (sourceProperty.HasChanged)
			{
				targetProperty.Value = sourceProperty.Value;
				sourceProperty.ClearChangedFlag();
			}
			else if (TwoWay && targetProperty.HasChanged)
			{
				sourceProperty.Value = targetProperty.Value;
				targetProperty.ClearChangedFlag();
			}
		}
	}

	public void Bind()
	{
		if (isBound)
		{
			return;
		}
		if (!IsDataSourceValid())
		{
			Debug.LogError(string.Format("Invalid data binding configuration - Source:{0}, Target:{1}", DataSource, DataTarget));
			return;
		}
		if (!DataTarget.IsValid)
		{
			Debug.LogError(string.Format("Invalid data binding configuration - Source:{0}, Target:{1}", DataSource, DataTarget));
			return;
		}
		dfDataObjectProxy dfDataObjectProxy2 = DataSource.Component as dfDataObjectProxy;
		sourceProperty = dfDataObjectProxy2.GetProperty(DataSource.MemberName);
		targetProperty = DataTarget.GetProperty();
		isBound = sourceProperty != null && targetProperty != null;
		if (isBound)
		{
			targetProperty.Value = sourceProperty.Value;
		}
		attachEvent();
	}

	public void Unbind()
	{
		if (isBound)
		{
			detachEvent();
			sourceProperty = null;
			targetProperty = null;
			isBound = false;
		}
	}

	private bool IsDataSourceValid()
	{
		return DataSource != null || DataSource.Component != null || !string.IsNullOrEmpty(DataSource.MemberName) || (DataSource.Component as dfDataObjectProxy).Data != null;
	}

	private void attachEvent()
	{
		if (!eventsAttached)
		{
			eventsAttached = true;
			dfDataObjectProxy dfDataObjectProxy2 = DataSource.Component as dfDataObjectProxy;
			if (dfDataObjectProxy2 != null)
			{
				dfDataObjectProxy2.DataChanged += handle_DataChanged;
			}
		}
	}

	private void detachEvent()
	{
		if (eventsAttached)
		{
			eventsAttached = false;
			dfDataObjectProxy dfDataObjectProxy2 = DataSource.Component as dfDataObjectProxy;
			if (dfDataObjectProxy2 != null)
			{
				dfDataObjectProxy2.DataChanged -= handle_DataChanged;
			}
		}
	}

	private void handle_DataChanged(object data)
	{
		Unbind();
		if (IsDataSourceValid())
		{
			Bind();
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
}
