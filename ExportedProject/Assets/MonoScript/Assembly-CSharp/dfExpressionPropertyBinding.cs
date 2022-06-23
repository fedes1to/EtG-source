using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[Obsolete("The expression binding functionality is no longer supported and may be removed in future versions of DFGUI")]
public class dfExpressionPropertyBinding : MonoBehaviour, IDataBindingComponent
{
	public Component DataSource;

	public dfComponentMemberInfo DataTarget;

	[SerializeField]
	protected string expression;

	private Delegate compiledExpression;

	private dfObservableProperty targetProperty;

	private bool isBound;

	public bool IsBound
	{
		get
		{
			return isBound;
		}
	}

	public string Expression
	{
		get
		{
			return expression;
		}
		set
		{
			if (!string.Equals(value, expression))
			{
				Unbind();
				expression = value;
			}
		}
	}

	public void OnDisable()
	{
		Unbind();
	}

	public void Update()
	{
		if (isBound)
		{
			evaluate();
		}
		else if (DataSource != null && !string.IsNullOrEmpty(expression) && DataTarget.IsValid)
		{
			Bind();
		}
	}

	public void Unbind()
	{
		if (isBound)
		{
			compiledExpression = null;
			targetProperty = null;
			isBound = false;
		}
	}

	public void Bind()
	{
		if (!isBound && (!(DataSource is dfDataObjectProxy) || ((dfDataObjectProxy)DataSource).Data != null))
		{
			dfScriptEngineSettings dfScriptEngineSettings = new dfScriptEngineSettings();
			dfScriptEngineSettings.Constants = new Dictionary<string, object>
			{
				{
					"Application",
					typeof(Application)
				},
				{
					"Color",
					typeof(Color)
				},
				{
					"Color32",
					typeof(Color32)
				},
				{
					"Random",
					typeof(UnityEngine.Random)
				},
				{
					"Time",
					typeof(Time)
				},
				{
					"ScriptableObject",
					typeof(ScriptableObject)
				},
				{
					"Vector2",
					typeof(Vector2)
				},
				{
					"Vector3",
					typeof(Vector3)
				},
				{
					"Vector4",
					typeof(Vector4)
				},
				{
					"Quaternion",
					typeof(Quaternion)
				},
				{
					"Matrix",
					typeof(Matrix4x4)
				},
				{
					"Mathf",
					typeof(Mathf)
				}
			};
			dfScriptEngineSettings dfScriptEngineSettings2 = dfScriptEngineSettings;
			if (DataSource is dfDataObjectProxy)
			{
				dfDataObjectProxy dfDataObjectProxy2 = DataSource as dfDataObjectProxy;
				dfScriptEngineSettings2.AddVariable(new dfScriptVariable("source", null, dfDataObjectProxy2.DataType));
			}
			else
			{
				dfScriptEngineSettings2.AddVariable(new dfScriptVariable("source", DataSource));
			}
			compiledExpression = dfScriptEngine.CompileExpression(expression, dfScriptEngineSettings2);
			targetProperty = DataTarget.GetProperty();
			isBound = (object)compiledExpression != null && targetProperty != null;
		}
	}

	private void evaluate()
	{
		try
		{
			object obj = DataSource;
			if (obj is dfDataObjectProxy)
			{
				obj = ((dfDataObjectProxy)obj).Data;
			}
			object value = compiledExpression.DynamicInvoke(obj);
			targetProperty.Value = value;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public override string ToString()
	{
		string arg = ((DataTarget == null || !(DataTarget.Component != null)) ? "[null]" : DataTarget.Component.GetType().Name);
		string arg2 = ((DataTarget == null || string.IsNullOrEmpty(DataTarget.MemberName)) ? "[null]" : DataTarget.MemberName);
		return string.Format("Bind [expression] -> {0}.{1}", arg, arg2);
	}
}
