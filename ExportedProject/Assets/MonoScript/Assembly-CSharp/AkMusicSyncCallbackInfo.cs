using System;

public class AkMusicSyncCallbackInfo : AkCallbackInfo
{
	private IntPtr swigCPtr;

	public uint playingID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_playingID_get(swigCPtr);
		}
	}

	public int segmentInfo_iCurrentPosition
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iCurrentPosition_get(swigCPtr);
		}
	}

	public int segmentInfo_iPreEntryDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iPreEntryDuration_get(swigCPtr);
		}
	}

	public int segmentInfo_iActiveDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iActiveDuration_get(swigCPtr);
		}
	}

	public int segmentInfo_iPostExitDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iPostExitDuration_get(swigCPtr);
		}
	}

	public int segmentInfo_iRemainingLookAheadTime
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_iRemainingLookAheadTime_get(swigCPtr);
		}
	}

	public float segmentInfo_fBeatDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fBeatDuration_get(swigCPtr);
		}
	}

	public float segmentInfo_fBarDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fBarDuration_get(swigCPtr);
		}
	}

	public float segmentInfo_fGridDuration
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fGridDuration_get(swigCPtr);
		}
	}

	public float segmentInfo_fGridOffset
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_segmentInfo_fGridOffset_get(swigCPtr);
		}
	}

	public AkCallbackType musicSyncType
	{
		get
		{
			return (AkCallbackType)AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_musicSyncType_get(swigCPtr);
		}
	}

	public string userCueName
	{
		get
		{
			return AkSoundEngine.StringFromIntPtrString(AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_userCueName_get(swigCPtr));
		}
	}

	internal AkMusicSyncCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkMusicSyncCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMusicSyncCallbackInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkMusicSyncCallbackInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkMusicSyncCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkMusicSyncCallbackInfo()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkMusicSyncCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}
}
