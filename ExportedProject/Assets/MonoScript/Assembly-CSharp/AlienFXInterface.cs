using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class AlienFXInterface
{
	public struct _LFX_COLOR
	{
		public byte red;

		public byte green;

		public byte blue;

		public byte brightness;

		public _LFX_COLOR(Color32 combinedColor)
		{
			red = combinedColor.r;
			green = combinedColor.g;
			blue = combinedColor.b;
			brightness = combinedColor.a;
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SetDllDirectory(string lpPathName);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	[DllImport("LightFX")]
	public static extern uint LFX_Initialize();

	[DllImport("LightFX")]
	public static extern uint LFX_Update();

	[DllImport("LightFX")]
	public static extern uint LFX_Reset();

	[DllImport("LightFX")]
	public static extern uint LFX_Release();

	[DllImport("LightFX", CallingConvention = CallingConvention.StdCall)]
	public static extern uint LFX_SetLightColor(uint p1, uint p2, ref _LFX_COLOR c);

	[DllImport("LightFX")]
	public static extern uint LFX_GetNumDevices(ref uint numDevices);

	[DllImport("LightFX")]
	public static extern uint LFX_GetNumLights(uint devIndex, ref uint numLights);
}
