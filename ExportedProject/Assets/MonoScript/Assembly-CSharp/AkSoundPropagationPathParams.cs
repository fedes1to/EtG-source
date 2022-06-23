using System;

public class AkSoundPropagationPathParams : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkVector listenerPos
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkSoundPropagationPathParams_listenerPos_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPropagationPathParams_listenerPos_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public AkVector emitterPos
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkSoundPropagationPathParams_emitterPos_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPropagationPathParams_emitterPos_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public uint numValidPaths
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSoundPropagationPathParams_numValidPaths_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSoundPropagationPathParams_numValidPaths_set(swigCPtr, value);
		}
	}

	internal AkSoundPropagationPathParams(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkSoundPropagationPathParams()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSoundPropagationPathParams(), true)
	{
	}

	internal static IntPtr getCPtr(AkSoundPropagationPathParams obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkSoundPropagationPathParams()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSoundPropagationPathParams(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
