using System;

public class AkPlaylistItem : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public uint audioNodeID
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_audioNodeID_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_audioNodeID_set(swigCPtr, value);
		}
	}

	public int msDelay
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_msDelay_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_msDelay_set(swigCPtr, value);
		}
	}

	public IntPtr pCustomInfo
	{
		get
		{
			return AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_pCustomInfo_get(swigCPtr);
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_pCustomInfo_set(swigCPtr, value);
		}
	}

	internal AkPlaylistItem(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkPlaylistItem()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPlaylistItem__SWIG_0(), true)
	{
	}

	public AkPlaylistItem(AkPlaylistItem in_rCopy)
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPlaylistItem__SWIG_1(getCPtr(in_rCopy)), true)
	{
	}

	internal static IntPtr getCPtr(AkPlaylistItem obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkPlaylistItem()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkPlaylistItem(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkPlaylistItem Assign(AkPlaylistItem in_rCopy)
	{
		return new AkPlaylistItem(AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_Assign(swigCPtr, getCPtr(in_rCopy)), false);
	}

	public bool IsEqualTo(AkPlaylistItem in_rCopy)
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_IsEqualTo(swigCPtr, getCPtr(in_rCopy));
	}

	public AKRESULT SetExternalSources(uint in_nExternalSrc, AkExternalSourceInfo in_pExternalSrc)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistItem_SetExternalSources(swigCPtr, in_nExternalSrc, AkExternalSourceInfo.getCPtr(in_pExternalSrc));
	}
}
