using System;

public class AkMarkerCallbackInfo : AkEventCallbackInfo
{
	private IntPtr swigCPtr;

	public uint uIdentifier
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMarkerCallbackInfo_uIdentifier_get(swigCPtr);
		}
	}

	public uint uPosition
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMarkerCallbackInfo_uPosition_get(swigCPtr);
		}
	}

	public string strLabel
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrString(AkSoundEnginePINVOKE.CSharp_AkMarkerCallbackInfo_strLabel_get(swigCPtr));
		}
	}

	internal AkMarkerCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkMarkerCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkMarkerCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMarkerCallbackInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkMarkerCallbackInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkMarkerCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkMarkerCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMarkerCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}
}
