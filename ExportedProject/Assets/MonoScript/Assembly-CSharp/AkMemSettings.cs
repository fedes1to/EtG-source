using System;

public class AkMemSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint uMaxNumPools
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMemSettings_uMaxNumPools_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMemSettings_uMaxNumPools_set(swigCPtr, value);
		}
	}

	public uint uDebugFlags
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMemSettings_uDebugFlags_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMemSettings_uDebugFlags_set(swigCPtr, value);
		}
	}

	internal AkMemSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkMemSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMemSettings(), true)
	{
	}

	internal static IntPtr getCPtr(AkMemSettings obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkMemSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMemSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
