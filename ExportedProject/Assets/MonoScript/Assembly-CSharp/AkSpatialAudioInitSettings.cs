using System;

public class AkSpatialAudioInitSettings : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public int uPoolID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uPoolID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uPoolID_set(swigCPtr, value);
		}
	}

	public uint uPoolSize
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uPoolSize_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uPoolSize_set(swigCPtr, value);
		}
	}

	public uint uMaxSoundPropagationDepth
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uMaxSoundPropagationDepth_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uMaxSoundPropagationDepth_set(swigCPtr, value);
		}
	}

	public uint uDiffractionFlags
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uDiffractionFlags_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_uDiffractionFlags_set(swigCPtr, value);
		}
	}

	public float fDiffractionShadowAttenFactor
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowAttenFactor_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowAttenFactor_set(swigCPtr, value);
		}
	}

	public float fDiffractionShadowDegrees
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowDegrees_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkSpatialAudioInitSettings_fDiffractionShadowDegrees_set(swigCPtr, value);
		}
	}

	internal AkSpatialAudioInitSettings(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkSpatialAudioInitSettings()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSpatialAudioInitSettings(), true)
	{
	}

	internal static IntPtr getCPtr(AkSpatialAudioInitSettings obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkSpatialAudioInitSettings()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSpatialAudioInitSettings(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
