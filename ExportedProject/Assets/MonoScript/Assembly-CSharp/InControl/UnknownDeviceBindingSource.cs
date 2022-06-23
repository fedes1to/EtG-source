using System.IO;
using UnityEngine;

namespace InControl
{
	public class UnknownDeviceBindingSource : BindingSource
	{
		public UnknownDeviceControl Control { get; protected set; }

		public override string Name
		{
			get
			{
				if (base.BoundTo == null)
				{
					return string.Empty;
				}
				string text = string.Empty;
				if (Control.SourceRange == InputRangeType.ZeroToMinusOne)
				{
					text = "Negative ";
				}
				else if (Control.SourceRange == InputRangeType.ZeroToOne)
				{
					text = "Positive ";
				}
				InputDevice device = base.BoundTo.Device;
				if (device == InputDevice.Null)
				{
					return text + Control.Control;
				}
				InputControl control = device.GetControl(Control.Control);
				if (control == InputControl.Null)
				{
					return text + Control.Control;
				}
				return text + control.Handle;
			}
		}

		public override string DeviceName
		{
			get
			{
				if (base.BoundTo == null)
				{
					return string.Empty;
				}
				InputDevice device = base.BoundTo.Device;
				if (device == InputDevice.Null)
				{
					return "Unknown Controller";
				}
				return device.Name;
			}
		}

		public override InputDeviceClass DeviceClass
		{
			get
			{
				return InputDeviceClass.Controller;
			}
		}

		public override InputDeviceStyle DeviceStyle
		{
			get
			{
				return InputDeviceStyle.Unknown;
			}
		}

		public override BindingSourceType BindingSourceType
		{
			get
			{
				return BindingSourceType.UnknownDeviceBindingSource;
			}
		}

		internal override bool IsValid
		{
			get
			{
				if (base.BoundTo == null)
				{
					Debug.LogError("Cannot query property 'IsValid' for unbound BindingSource.");
					return false;
				}
				InputDevice device = base.BoundTo.Device;
				return device == InputDevice.Null || device.HasControl(Control.Control);
			}
		}

		internal UnknownDeviceBindingSource()
		{
			Control = UnknownDeviceControl.None;
		}

		public UnknownDeviceBindingSource(UnknownDeviceControl control)
		{
			Control = control;
		}

		public override float GetValue(InputDevice device)
		{
			return Control.GetValue(device);
		}

		public override bool GetState(InputDevice device)
		{
			if (device == null)
			{
				return false;
			}
			return Utility.IsNotZero(GetValue(device));
		}

		public override bool Equals(BindingSource other)
		{
			if (other == null)
			{
				return false;
			}
			UnknownDeviceBindingSource unknownDeviceBindingSource = other as UnknownDeviceBindingSource;
			if (unknownDeviceBindingSource != null)
			{
				return Control == unknownDeviceBindingSource.Control;
			}
			return false;
		}

		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			UnknownDeviceBindingSource unknownDeviceBindingSource = other as UnknownDeviceBindingSource;
			if (unknownDeviceBindingSource != null)
			{
				return Control == unknownDeviceBindingSource.Control;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Control.GetHashCode();
		}

		internal override void Load(BinaryReader reader, ushort dataFormatVersion, bool upgrade)
		{
			UnknownDeviceControl control = default(UnknownDeviceControl);
			control.Load(reader, upgrade);
			Control = control;
		}

		internal override void Save(BinaryWriter writer)
		{
			Control.Save(writer);
		}
	}
}
