using System;

public class AkSerializedCallbackHeader : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public IntPtr pPackage
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_pPackage_get(swigCPtr);
		}
	}

	public AkSerializedCallbackHeader pNext
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_pNext_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkSerializedCallbackHeader(intPtr, false) : null;
		}
	}

	public AkCallbackType eType
	{
		get
		{
			return (AkCallbackType)AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_eType_get(swigCPtr);
		}
	}

	internal AkSerializedCallbackHeader(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkSerializedCallbackHeader()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSerializedCallbackHeader(), true)
	{
	}

	internal static IntPtr getCPtr(AkSerializedCallbackHeader obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkSerializedCallbackHeader()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSerializedCallbackHeader(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public IntPtr GetData()
	{
		return AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_GetData(swigCPtr);
	}
}
