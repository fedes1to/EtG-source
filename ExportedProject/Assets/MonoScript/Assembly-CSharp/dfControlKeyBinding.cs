using System;
using System.Reflection;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Data Binding/Key Binding")]
public class dfControlKeyBinding : MonoBehaviour, IDataBindingComponent
{
	[SerializeField]
	protected dfControl control;

	[SerializeField]
	protected KeyCode keyCode;

	[SerializeField]
	protected bool shiftPressed;

	[SerializeField]
	protected bool altPressed;

	[SerializeField]
	protected bool controlPressed;

	[SerializeField]
	protected dfComponentMemberInfo target;

	private bool isBound;

	public dfControl Control
	{
		get
		{
			return control;
		}
		set
		{
			if (isBound)
			{
				Unbind();
			}
			control = value;
		}
	}

	public KeyCode KeyCode
	{
		get
		{
			return keyCode;
		}
		set
		{
			keyCode = value;
		}
	}

	public bool AltPressed
	{
		get
		{
			return altPressed;
		}
		set
		{
			altPressed = value;
		}
	}

	public bool ControlPressed
	{
		get
		{
			return controlPressed;
		}
		set
		{
			controlPressed = value;
		}
	}

	public bool ShiftPressed
	{
		get
		{
			return shiftPressed;
		}
		set
		{
			shiftPressed = value;
		}
	}

	public dfComponentMemberInfo Target
	{
		get
		{
			return target;
		}
		set
		{
			if (isBound)
			{
				Unbind();
			}
			target = value;
		}
	}

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
	}

	public void Start()
	{
		if (control != null && target.IsValid)
		{
			Bind();
		}
	}

	public void Bind()
	{
		if (isBound)
		{
			Unbind();
		}
		if (control != null)
		{
			control.KeyDown += eventSource_KeyDown;
		}
		isBound = true;
	}

	public void Unbind()
	{
		if (control != null)
		{
			control.KeyDown -= eventSource_KeyDown;
		}
		isBound = false;
	}

	private void eventSource_KeyDown(dfControl sourceControl, dfKeyEventArgs args)
	{
		if (args.KeyCode == keyCode && args.Shift == shiftPressed && args.Control == controlPressed && args.Alt == altPressed)
		{
			MethodInfo method = target.GetMethod();
			method.Invoke(target.Component, null);
		}
	}
}
