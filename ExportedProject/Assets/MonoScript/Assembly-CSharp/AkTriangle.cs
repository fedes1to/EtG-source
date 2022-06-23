using System;

public class AkTriangle : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkVector point0
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkTriangle_point0_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_point0_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public AkVector point1
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkTriangle_point1_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_point1_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public AkVector point2
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkTriangle_point2_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_point2_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public uint textureID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTriangle_textureID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_textureID_set(swigCPtr, value);
		}
	}

	public uint reflectorChannelMask
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkTriangle_reflectorChannelMask_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_reflectorChannelMask_set(swigCPtr, value);
		}
	}

	public string strName
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrString(AkSoundEnginePINVOKE.CSharp_AkTriangle_strName_get(swigCPtr));
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkTriangle_strName_set(swigCPtr, value);
		}
	}

	internal AkTriangle(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkTriangle()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTriangle(), true)
	{
	}

	internal static IntPtr getCPtr(AkTriangle obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkTriangle()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkTriangle(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
