using System;

public class AkObstructionOcclusionValues : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public float occlusion
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_occlusion_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_occlusion_set(swigCPtr, value);
		}
	}

	public float obstruction
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_obstruction_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkObstructionOcclusionValues_obstruction_set(swigCPtr, value);
		}
	}

	internal AkObstructionOcclusionValues(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkObstructionOcclusionValues()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkObstructionOcclusionValues(), true)
	{
	}

	internal static IntPtr getCPtr(AkObstructionOcclusionValues obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkObstructionOcclusionValues()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkObstructionOcclusionValues(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
