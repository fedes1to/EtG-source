using System.Runtime.InteropServices;

public class SCEPadWrapper
{
	public enum ScePadButtonDataOffset : uint
	{
		SCE_PAD_BUTTON_L3 = 2u,
		SCE_PAD_BUTTON_R3 = 4u,
		SCE_PAD_BUTTON_OPTIONS = 8u,
		SCE_PAD_BUTTON_UP = 0x10u,
		SCE_PAD_BUTTON_RIGHT = 0x20u,
		SCE_PAD_BUTTON_DOWN = 0x40u,
		SCE_PAD_BUTTON_LEFT = 0x80u,
		SCE_PAD_BUTTON_L2 = 0x100u,
		SCE_PAD_BUTTON_R2 = 0x200u,
		SCE_PAD_BUTTON_L1 = 0x400u,
		SCE_PAD_BUTTON_R1 = 0x800u,
		SCE_PAD_BUTTON_TRIANGLE = 0x1000u,
		SCE_PAD_BUTTON_CIRCLE = 0x2000u,
		SCE_PAD_BUTTON_CROSS = 0x4000u,
		SCE_PAD_BUTTON_SQUARE = 0x8000u,
		SCE_PAD_BUTTON_TOUCH_PAD = 0x100000u,
		SCE_PAD_BUTTON_INTERCEPTED = 0x80000000u
	}

	public struct ScePadAnalogStick
	{
		public byte x;

		public byte y;
	}

	public struct ScePadAnalogButtons
	{
		public byte l2;

		public byte r2;

		private byte pad1;

		private byte pad2;
	}

	public struct ScePadTouch
	{
		private ushort x;

		private ushort y;

		private byte id;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		private byte[] reserve;
	}

	public struct ScePadTouchData
	{
		private byte touchNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		private byte[] reserve;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		private ScePadTouch[] touch;
	}

	public struct ScePadExtensionUnitData
	{
		private uint extensionUnitId;

		private byte reserve;

		private byte dataLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		private byte[] data;
	}

	public struct SceFQuaternion
	{
		private float x;

		private float y;

		private float z;

		private float w;
	}

	public struct SceFVector3
	{
		private float x;

		private float y;

		private float z;
	}

	public struct ScePadData
	{
		public uint buttons;

		public ScePadAnalogStick leftStick;

		public ScePadAnalogStick rightStick;

		public ScePadAnalogButtons analogButtons;

		public bool connected;
	}

	private const int SCE_PAD_MAX_TOUCH_NUM = 2;

	private const int SCE_PAD_MAX_DEVICE_UNIQUE_DATA_SIZE = 12;

	public const int SCE_OK = 0;

	[DllImport("PS4NativePad")]
	public static extern int PadReadState(int handle, out ScePadData pData);
}
