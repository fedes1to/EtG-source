using System;
using UnityEngine;

public class AkAuxSendValue : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public ulong listenerID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_listenerID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_listenerID_set(swigCPtr, value);
		}
	}

	public uint auxBusID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_set(swigCPtr, value);
		}
	}

	public float fControlValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_fControlValue_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_fControlValue_set(swigCPtr, value);
		}
	}

	internal AkAuxSendValue(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkAuxSendValue obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkAuxSendValue()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkAuxSendValue(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Set(GameObject listener, uint id, float value)
	{
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(listener);
		AkSoundEngine.PreGameObjectAPICall(listener, akGameObjectID);
		AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_Set(swigCPtr, akGameObjectID, id, value);
	}

	public bool IsSame(GameObject listener, uint id)
	{
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(listener);
		AkSoundEngine.PreGameObjectAPICall(listener, akGameObjectID);
		return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_IsSame(swigCPtr, akGameObjectID, id);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_GetSizeOf();
	}

	public AKRESULT SetGameObjectAuxSendValues(GameObject in_gameObjectID, uint in_uNumSendValues)
	{
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkSoundEngine.PreGameObjectAPICall(in_gameObjectID, akGameObjectID);
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_SetGameObjectAuxSendValues(swigCPtr, akGameObjectID, in_uNumSendValues);
	}

	public AKRESULT GetGameObjectAuxSendValues(GameObject in_gameObjectID, ref uint io_ruNumSendValues)
	{
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkSoundEngine.PreGameObjectAPICall(in_gameObjectID, akGameObjectID);
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_GetGameObjectAuxSendValues(swigCPtr, akGameObjectID, ref io_ruNumSendValues);
	}
}
