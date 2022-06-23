using System;

public class AkRamp : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public float fPrev
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRamp_fPrev_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRamp_fPrev_set(swigCPtr, value);
		}
	}

	public float fNext
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRamp_fNext_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRamp_fNext_set(swigCPtr, value);
		}
	}

	internal AkRamp(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkRamp()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkRamp__SWIG_0(), true)
	{
	}

	public AkRamp(float in_fPrev, float in_fNext)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkRamp__SWIG_1(in_fPrev, in_fNext), true)
	{
	}

	internal static IntPtr getCPtr(AkRamp obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkRamp()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkRamp(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
