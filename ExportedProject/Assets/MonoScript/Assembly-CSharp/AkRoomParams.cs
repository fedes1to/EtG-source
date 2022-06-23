using System;

public class AkRoomParams : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkVector Up
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkRoomParams_Up_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_Up_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public AkVector Front
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkRoomParams_Front_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_Front_set(swigCPtr, AkVector.getCPtr(value));
		}
	}

	public uint ReverbAuxBus
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbAuxBus_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbAuxBus_set(swigCPtr, value);
		}
	}

	public float ReverbLevel
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbLevel_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_ReverbLevel_set(swigCPtr, value);
		}
	}

	public float WallOcclusion
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_WallOcclusion_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_WallOcclusion_set(swigCPtr, value);
		}
	}

	public int Priority
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_Priority_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_Priority_set(swigCPtr, value);
		}
	}

	public float RoomGameObj_AuxSendLevelToSelf
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_AuxSendLevelToSelf_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_AuxSendLevelToSelf_set(swigCPtr, value);
		}
	}

	public bool RoomGameObj_KeepRegistered
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_KeepRegistered_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkRoomParams_RoomGameObj_KeepRegistered_set(swigCPtr, value);
		}
	}

	internal AkRoomParams(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkRoomParams()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkRoomParams(), true)
	{
	}

	internal static IntPtr getCPtr(AkRoomParams obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkRoomParams()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkRoomParams(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
