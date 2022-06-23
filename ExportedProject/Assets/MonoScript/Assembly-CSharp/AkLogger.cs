using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

public class AkLogger
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void ErrorLoggerInteropDelegate([MarshalAs(UnmanagedType.LPStr)] string message);

	private static AkLogger ms_Instance = new AkLogger();

	private ErrorLoggerInteropDelegate errorLoggerDelegate = WwiseInternalLogError;

	public static AkLogger Instance
	{
		get
		{
			return ms_Instance;
		}
	}

	private AkLogger()
	{
		if (ms_Instance == null)
		{
			ms_Instance = this;
			AkSoundEngine.SetErrorLogger(errorLoggerDelegate);
		}
	}

	~AkLogger()
	{
		if (ms_Instance == this)
		{
			ms_Instance = null;
			errorLoggerDelegate = null;
			AkSoundEngine.SetErrorLogger();
		}
	}

	public void Init()
	{
	}

	[AOT.MonoPInvokeCallback(typeof(ErrorLoggerInteropDelegate))]
	public static void WwiseInternalLogError(string message)
	{
		Debug.LogError("Wwise: " + message);
	}

	public static void Message(string message)
	{
		Debug.Log("WwiseUnity: " + message);
	}

	public static void Warning(string message)
	{
		Debug.LogWarning("WwiseUnity: " + message);
	}

	public static void Error(string message)
	{
		Debug.LogError("WwiseUnity: " + message);
	}
}
