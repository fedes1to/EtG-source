using System;

public class AkMonitoringCallbackInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkMonitorErrorCode errorCode
	{
		get
		{
			return (AkMonitorErrorCode)AkSoundEnginePINVOKE.CSharp_AkMonitoringCallbackInfo_errorCode_get(swigCPtr);
		}
	}

	public AkMonitorErrorLevel errorLevel
	{
		get
		{
			return (AkMonitorErrorLevel)AkSoundEnginePINVOKE.CSharp_AkMonitoringCallbackInfo_errorLevel_get(swigCPtr);
		}
	}

	public uint playingID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMonitoringCallbackInfo_playingID_get(swigCPtr);
		}
	}

	public ulong gameObjID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMonitoringCallbackInfo_gameObjID_get(swigCPtr);
		}
	}

	public string message
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrOSString(AkSoundEnginePINVOKE.CSharp_AkMonitoringCallbackInfo_message_get(swigCPtr));
		}
	}

	internal AkMonitoringCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkMonitoringCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMonitoringCallbackInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkMonitoringCallbackInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkMonitoringCallbackInfo()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkMonitoringCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
