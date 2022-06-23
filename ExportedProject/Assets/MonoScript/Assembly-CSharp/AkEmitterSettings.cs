using System;

public class AkEmitterSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint reflectAuxBusID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectAuxBusID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectAuxBusID_set(swigCPtr, value);
		}
	}

	public float reflectionMaxPathLength
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectionMaxPathLength_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectionMaxPathLength_set(swigCPtr, value);
		}
	}

	public float reflectionsAuxBusGain
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectionsAuxBusGain_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectionsAuxBusGain_set(swigCPtr, value);
		}
	}

	public uint reflectionsOrder
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectionsOrder_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectionsOrder_set(swigCPtr, value);
		}
	}

	public uint reflectorFilterMask
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectorFilterMask_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_reflectorFilterMask_set(swigCPtr, value);
		}
	}

	public float roomReverbAuxBusGain
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_roomReverbAuxBusGain_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_roomReverbAuxBusGain_set(swigCPtr, value);
		}
	}

	public byte useImageSources
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_useImageSources_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkEmitterSettings_useImageSources_set(swigCPtr, value);
		}
	}

	internal AkEmitterSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkEmitterSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkEmitterSettings(), true)
	{
	}

	internal static IntPtr getCPtr(AkEmitterSettings obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkEmitterSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkEmitterSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
