using System;

public class AkSoundPathInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkVector imageSource
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_imageSource_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_imageSource_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public uint numReflections
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_numReflections_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_numReflections_set(swigCPtr, value);
		}
	}

	public AkVector occlusionPoint
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_occlusionPoint_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_occlusionPoint_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public bool isOccluded
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_isOccluded_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPathInfo_isOccluded_set(swigCPtr, value);
		}
	}

	internal AkSoundPathInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkSoundPathInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSoundPathInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkSoundPathInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkSoundPathInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSoundPathInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
