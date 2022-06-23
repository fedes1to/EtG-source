using System;

public class AkIterator : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	public AkPlaylistItem pItem
	{
		get
		{
			IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkIterator_pItem_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new AkPlaylistItem(intPtr, false) : null;
		}
		set
		{
			AkSoundEnginePINVOKE.CSharp_AkIterator_pItem_set(swigCPtr, AkPlaylistItem.getCPtr(value));
		}
	}

	internal AkIterator(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public AkIterator()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkIterator(), true)
	{
	}

	internal static IntPtr getCPtr(AkIterator obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkIterator()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkIterator(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkIterator NextIter()
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkIterator_NextIter(swigCPtr), false);
	}

	public AkIterator PrevIter()
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkIterator_PrevIter(swigCPtr), false);
	}

	public AkPlaylistItem GetItem()
	{
		return new AkPlaylistItem(AkSoundEnginePINVOKE.CSharp_AkIterator_GetItem(swigCPtr), false);
	}

	public bool IsEqualTo(AkIterator in_rOp)
	{
		return AkSoundEnginePINVOKE.CSharp_AkIterator_IsEqualTo(swigCPtr, getCPtr(in_rOp));
	}

	public bool IsDifferentFrom(AkIterator in_rOp)
	{
		return AkSoundEnginePINVOKE.CSharp_AkIterator_IsDifferentFrom(swigCPtr, getCPtr(in_rOp));
	}
}
