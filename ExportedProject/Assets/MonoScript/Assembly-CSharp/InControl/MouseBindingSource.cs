using System;
using System.IO;
using UnityEngine;

namespace InControl
{
	public class MouseBindingSource : BindingSource
	{
		public static float ScaleX = 0.05f;

		public static float ScaleY = 0.05f;

		public static float ScaleZ = 0.05f;

		public static float JitterThreshold = 0.05f;

		private static readonly int[] buttonTable = new int[16]
		{
			-1, 0, 1, 2, -1, -1, -1, -1, -1, -1,
			3, 4, 5, 6, 7, 8
		};

		public Mouse Control { get; protected set; }

		public override string Name
		{
			get
			{
				if ((bool)GameUIRoot.Instance)
				{
					dfControl p_playerCoinLabel = GameUIRoot.Instance.p_playerCoinLabel;
					if ((bool)p_playerCoinLabel)
					{
						if (Control == Mouse.LeftButton)
						{
							return p_playerCoinLabel.ForceGetLocalizedValue("#CONTROL_LMB");
						}
						if (Control == Mouse.MiddleButton)
						{
							return p_playerCoinLabel.ForceGetLocalizedValue("#CONTROL_MMB");
						}
						if (Control == Mouse.RightButton)
						{
							return p_playerCoinLabel.ForceGetLocalizedValue("#CONTROL_RMB");
						}
					}
				}
				return Control.ToString();
			}
		}

		public override string DeviceName
		{
			get
			{
				return "Mouse";
			}
		}

		public override InputDeviceClass DeviceClass
		{
			get
			{
				return InputDeviceClass.Mouse;
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
				return BindingSourceType.MouseBindingSource;
			}
		}

		internal MouseBindingSource()
		{
		}

		public MouseBindingSource(Mouse mouseControl)
		{
			Control = mouseControl;
		}

		internal static bool SafeGetMouseButton(int button)
		{
			try
			{
				return Input.GetMouseButton(button);
			}
			catch (ArgumentException)
			{
			}
			return false;
		}

		internal static bool ButtonIsPressed(Mouse control)
		{
			int num = buttonTable[(int)control];
			if (num >= 0)
			{
				return SafeGetMouseButton(num);
			}
			return false;
		}

		internal static bool NegativeScrollWheelIsActive(float threshold)
		{
			float num = Mathf.Min(Input.GetAxisRaw("mouse z") * ScaleZ, 0f);
			return num < 0f - threshold;
		}

		internal static bool PositiveScrollWheelIsActive(float threshold)
		{
			float num = Mathf.Max(0f, Input.GetAxisRaw("mouse z") * ScaleZ);
			return num > threshold;
		}

		internal static float GetValue(Mouse mouseControl)
		{
			int num = buttonTable[(int)mouseControl];
			if (num >= 0)
			{
				return (!SafeGetMouseButton(num)) ? 0f : 1f;
			}
			switch (mouseControl)
			{
			case Mouse.NegativeX:
				return 0f - Mathf.Min(Input.GetAxisRaw("mouse x") * ScaleX, 0f);
			case Mouse.PositiveX:
				return Mathf.Max(0f, Input.GetAxisRaw("mouse x") * ScaleX);
			case Mouse.NegativeY:
				return 0f - Mathf.Min(Input.GetAxisRaw("mouse y") * ScaleY, 0f);
			case Mouse.PositiveY:
				return Mathf.Max(0f, Input.GetAxisRaw("mouse y") * ScaleY);
			case Mouse.NegativeScrollWheel:
				return 0f - Mathf.Min(Input.GetAxisRaw("mouse z") * ScaleZ, 0f);
			case Mouse.PositiveScrollWheel:
				return Mathf.Max(0f, Input.GetAxisRaw("mouse z") * ScaleZ);
			default:
				return 0f;
			}
		}

		public override float GetValue(InputDevice inputDevice)
		{
			return GetValue(Control);
		}

		public override bool GetState(InputDevice inputDevice)
		{
			return Utility.IsNotZero(GetValue(inputDevice));
		}

		public static string GetLocalizedMouseButtonName(int buttonIndex)
		{
			if ((bool)GameUIRoot.Instance)
			{
				dfControl p_playerCoinLabel = GameUIRoot.Instance.p_playerCoinLabel;
				if ((bool)p_playerCoinLabel)
				{
					switch (buttonIndex)
					{
					case 0:
						return p_playerCoinLabel.ForceGetLocalizedValue("#CONTROL_LMB");
					case 1:
						return p_playerCoinLabel.ForceGetLocalizedValue("#CONTROL_MMB");
					case 2:
						return p_playerCoinLabel.ForceGetLocalizedValue("#CONTROL_RMB");
					}
				}
			}
			return string.Empty;
		}

		public override bool Equals(BindingSource other)
		{
			if (other == null)
			{
				return false;
			}
			MouseBindingSource mouseBindingSource = other as MouseBindingSource;
			if (mouseBindingSource != null)
			{
				return Control == mouseBindingSource.Control;
			}
			return false;
		}

		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			MouseBindingSource mouseBindingSource = other as MouseBindingSource;
			if (mouseBindingSource != null)
			{
				return Control == mouseBindingSource.Control;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Control.GetHashCode();
		}

		internal override void Save(BinaryWriter writer)
		{
			writer.Write((int)Control);
		}

		internal override void Load(BinaryReader reader, ushort dataFormatVersion, bool upgrade)
		{
			Control = (Mouse)reader.ReadInt32();
		}
	}
}
