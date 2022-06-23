using System;

public class AkDurationCallbackInfo : AkEventCallbackInfo
{
	private IntPtr swigCPtr;

	public float fDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_fDuration_get(swigCPtr);
		}
	}

	public float fEstimatedDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_fEstimatedDuration_get(swigCPtr);
		}
	}

	public uint audioNodeID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_audioNodeID_get(swigCPtr);
		}
	}

	public uint mediaID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_mediaID_get(swigCPtr);
		}
	}

	public bool bStreaming
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_bStreaming_get(swigCPtr);
		}
	}

	internal AkDurationCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkDurationCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkDurationCallbackInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkDurationCallbackInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkDurationCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkDurationCallbackInfo()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkDurationCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}
}
