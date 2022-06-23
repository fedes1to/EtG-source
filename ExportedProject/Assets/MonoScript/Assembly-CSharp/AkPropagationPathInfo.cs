using System;

public class AkPropagationPathInfo : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public const uint kMaxNodes = 8u;

	public AkVector nodePoint
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_nodePoint_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_nodePoint_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public uint numNodes
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_numNodes_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_numNodes_set(swigCPtr, value);
		}
	}

	public float length
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_length_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_length_set(swigCPtr, value);
		}
	}

	public float gain
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_gain_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_gain_set(swigCPtr, value);
		}
	}

	public float dryDiffractionAngle
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_dryDiffractionAngle_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_dryDiffractionAngle_set(swigCPtr, value);
		}
	}

	public float wetDiffractionAngle
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_wetDiffractionAngle_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfo_wetDiffractionAngle_set(swigCPtr, value);
		}
	}

	internal AkPropagationPathInfo(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkPropagationPathInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPropagationPathInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkPropagationPathInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkPropagationPathInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkPropagationPathInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
