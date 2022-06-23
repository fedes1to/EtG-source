using System;

public class AkMIDIEventCallbackInfo : AkEventCallbackInfo
{
	private IntPtr swigCPtr;

	public byte byChan
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byChan_get(swigCPtr);
		}
	}

	public byte byParam1
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byParam1_get(swigCPtr);
		}
	}

	public byte byParam2
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byParam2_get(swigCPtr);
		}
	}

	public AkMIDIEventTypes byType
	{
		get
		{
			return (AkMIDIEventTypes)AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byType_get(swigCPtr);
		}
	}

	public byte byOnOffNote
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byOnOffNote_get(swigCPtr);
		}
	}

	public byte byVelocity
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byVelocity_get(swigCPtr);
		}
	}

	public AkMIDICcTypes byCc
	{
		get
		{
			return (AkMIDICcTypes)AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byCc_get(swigCPtr);
		}
	}

	public byte byCcValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byCcValue_get(swigCPtr);
		}
	}

	public byte byValueLsb
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byValueLsb_get(swigCPtr);
		}
	}

	public byte byValueMsb
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byValueMsb_get(swigCPtr);
		}
	}

	public byte byAftertouchNote
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byAftertouchNote_get(swigCPtr);
		}
	}

	public byte byNoteAftertouchValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byNoteAftertouchValue_get(swigCPtr);
		}
	}

	public byte byChanAftertouchValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byChanAftertouchValue_get(swigCPtr);
		}
	}

	public byte byProgramNum
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_byProgramNum_get(swigCPtr);
		}
	}

	internal AkMIDIEventCallbackInfo(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkMIDIEventCallbackInfo()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEventCallbackInfo(), true)
	{
	}

	internal static IntPtr getCPtr(AkMIDIEventCallbackInfo obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkMIDIEventCallbackInfo_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkMIDIEventCallbackInfo()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEventCallbackInfo(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}
}
