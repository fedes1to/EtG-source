using System;

public class AkMIDIEvent : IDisposable
{
	public class tGen : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byParam1
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tGen_byParam1_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tGen_byParam1_set(swigCPtr, value);
			}
		}

		public byte byParam2
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tGen_byParam2_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tGen_byParam2_set(swigCPtr, value);
			}
		}

		internal tGen(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tGen()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tGen(), true)
		{
		}

		internal static IntPtr getCPtr(tGen obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tGen()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tGen(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	public class tNoteOnOff : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byNote
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteOnOff_byNote_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteOnOff_byNote_set(swigCPtr, value);
			}
		}

		public byte byVelocity
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteOnOff_byVelocity_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteOnOff_byVelocity_set(swigCPtr, value);
			}
		}

		internal tNoteOnOff(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tNoteOnOff()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tNoteOnOff(), true)
		{
		}

		internal static IntPtr getCPtr(tNoteOnOff obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tNoteOnOff()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tNoteOnOff(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	public class tCc : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byCc
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tCc_byCc_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tCc_byCc_set(swigCPtr, value);
			}
		}

		public byte byValue
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tCc_byValue_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tCc_byValue_set(swigCPtr, value);
			}
		}

		internal tCc(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tCc()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tCc(), true)
		{
		}

		internal static IntPtr getCPtr(tCc obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tCc()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tCc(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	public class tPitchBend : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byValueLsb
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tPitchBend_byValueLsb_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tPitchBend_byValueLsb_set(swigCPtr, value);
			}
		}

		public byte byValueMsb
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tPitchBend_byValueMsb_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tPitchBend_byValueMsb_set(swigCPtr, value);
			}
		}

		internal tPitchBend(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tPitchBend()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tPitchBend(), true)
		{
		}

		internal static IntPtr getCPtr(tPitchBend obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tPitchBend()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tPitchBend(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	public class tNoteAftertouch : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byNote
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteAftertouch_byNote_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteAftertouch_byNote_set(swigCPtr, value);
			}
		}

		public byte byValue
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteAftertouch_byValue_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tNoteAftertouch_byValue_set(swigCPtr, value);
			}
		}

		internal tNoteAftertouch(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tNoteAftertouch()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tNoteAftertouch(), true)
		{
		}

		internal static IntPtr getCPtr(tNoteAftertouch obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tNoteAftertouch()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tNoteAftertouch(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	public class tChanAftertouch : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byValue
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tChanAftertouch_byValue_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tChanAftertouch_byValue_set(swigCPtr, value);
			}
		}

		internal tChanAftertouch(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tChanAftertouch()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tChanAftertouch(), true)
		{
		}

		internal static IntPtr getCPtr(tChanAftertouch obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tChanAftertouch()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tChanAftertouch(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	public class tProgramChange : IDisposable
	{
		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		public byte byProgramNum
		{
			get
			{
				return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tProgramChange_byProgramNum_get(swigCPtr);
			}
			set
			{
				AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_tProgramChange_byProgramNum_set(swigCPtr, value);
			}
		}

		internal tProgramChange(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = cPtr;
		}

		public tProgramChange()
			: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent_tProgramChange(), true)
		{
		}

		internal static IntPtr getCPtr(tProgramChange obj)
		{
			return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
		}

		internal virtual void setCPtr(IntPtr cPtr)
		{
			Dispose();
			swigCPtr = cPtr;
		}

		~tProgramChange()
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
						AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent_tProgramChange(swigCPtr);
					}
					swigCPtr = IntPtr.Zero;
				}
				GC.SuppressFinalize(this);
			}
		}
	}

	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public byte byChan
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byChan_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byChan_set(swigCPtr, value);
		}
	}

	public tGen Gen
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_Gen_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tGen(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_Gen_set(swigCPtr, tGen.getCPtr(value));
		}
	}

	public tCc Cc
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_Cc_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tCc(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_Cc_set(swigCPtr, tCc.getCPtr(value));
		}
	}

	public tNoteOnOff NoteOnOff
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_NoteOnOff_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tNoteOnOff(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_NoteOnOff_set(swigCPtr, tNoteOnOff.getCPtr(value));
		}
	}

	public tPitchBend PitchBend
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_PitchBend_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tPitchBend(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_PitchBend_set(swigCPtr, tPitchBend.getCPtr(value));
		}
	}

	public tNoteAftertouch NoteAftertouch
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_NoteAftertouch_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tNoteAftertouch(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_NoteAftertouch_set(swigCPtr, tNoteAftertouch.getCPtr(value));
		}
	}

	public tChanAftertouch ChanAftertouch
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_ChanAftertouch_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tChanAftertouch(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_ChanAftertouch_set(swigCPtr, tChanAftertouch.getCPtr(value));
		}
	}

	public tProgramChange ProgramChange
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_ProgramChange_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new tProgramChange(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_ProgramChange_set(swigCPtr, tProgramChange.getCPtr(value));
		}
	}

	public AkMIDIEventTypes byType
	{
		get
		{
			return (AkMIDIEventTypes)AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byType_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byType_set(swigCPtr, (int)value);
		}
	}

	public byte byOnOffNote
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byOnOffNote_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byOnOffNote_set(swigCPtr, value);
		}
	}

	public byte byVelocity
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byVelocity_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byVelocity_set(swigCPtr, value);
		}
	}

	public AkMIDICcTypes byCc
	{
		get
		{
			return (AkMIDICcTypes)AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byCc_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byCc_set(swigCPtr, (int)value);
		}
	}

	public byte byCcValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byCcValue_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byCcValue_set(swigCPtr, value);
		}
	}

	public byte byValueLsb
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byValueLsb_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byValueLsb_set(swigCPtr, value);
		}
	}

	public byte byValueMsb
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byValueMsb_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byValueMsb_set(swigCPtr, value);
		}
	}

	public byte byAftertouchNote
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byAftertouchNote_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byAftertouchNote_set(swigCPtr, value);
		}
	}

	public byte byNoteAftertouchValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byNoteAftertouchValue_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byNoteAftertouchValue_set(swigCPtr, value);
		}
	}

	public byte byChanAftertouchValue
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byChanAftertouchValue_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byChanAftertouchValue_set(swigCPtr, value);
		}
	}

	public byte byProgramNum
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byProgramNum_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkMIDIEvent_byProgramNum_set(swigCPtr, value);
		}
	}

	internal AkMIDIEvent(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkMIDIEvent()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkMIDIEvent(), true)
	{
	}

	internal static IntPtr getCPtr(AkMIDIEvent obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkMIDIEvent()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkMIDIEvent(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}
}
