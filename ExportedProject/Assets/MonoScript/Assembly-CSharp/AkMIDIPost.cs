using System;
using UnityEngine;

public class AkMIDIPost : AkMIDIEvent
{
	private IntPtr swigCPtr;

	public uint uOffset
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIPost_uOffset_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIPost_uOffset_set(swigCPtr, value);
		}
	}

	internal AkMIDIPost(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkMIDIPost_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkMIDIPost()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIPost(), true)
	{
	}

	internal static IntPtr getCPtr(AkMIDIPost obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkMIDIPost_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkMIDIPost()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMIDIPost(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public AKRESULT PostOnEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uNumPosts)
	{
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(in_gameObjectID);
		AkSoundEngine.PreGameObjectAPICall(in_gameObjectID, akGameObjectID);
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkMIDIPost_PostOnEvent(swigCPtr, in_eventID, akGameObjectID, in_uNumPosts);
	}

	public void Clone(AkMIDIPost other)
	{
		AkSoundEnginePINVOKE.CSharp_AkMIDIPost_Clone(swigCPtr, getCPtr(other));
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkMIDIPost_GetSizeOf();
	}
}
