using System;

public class AkImageSourceParams : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkVector sourcePosition
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkImageSourceParams_sourcePosition_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkImageSourceParams_sourcePosition_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public float fDistanceScalingFactor
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkImageSourceParams_fDistanceScalingFactor_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkImageSourceParams_fDistanceScalingFactor_set(swigCPtr, value);
		}
	}

	public float fLevel
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkImageSourceParams_fLevel_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkImageSourceParams_fLevel_set(swigCPtr, value);
		}
	}

	internal AkImageSourceParams(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkImageSourceParams()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkImageSourceParams__SWIG_0(), true)
	{
	}

	public AkImageSourceParams(AkVector in_sourcePosition, float in_fDistanceScalingFactor, float in_fLevel)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkImageSourceParams__SWIG_1(AkVector.getCPtr(in_sourcePosition), in_fDistanceScalingFactor, in_fLevel), true)
	{
	}

	internal static IntPtr getCPtr(AkImageSourceParams obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkImageSourceParams()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkImageSourceParams(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
