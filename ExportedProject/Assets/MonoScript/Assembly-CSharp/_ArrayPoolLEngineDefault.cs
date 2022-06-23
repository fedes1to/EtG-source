using System;

public class _ArrayPoolLEngineDefault : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal _ArrayPoolLEngineDefault(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	public _ArrayPoolLEngineDefault()
		: this(AkSoundEnginePINVOKE.CSharp_new__ArrayPoolLEngineDefault(), true)
	{
	}

	internal static IntPtr getCPtr(_ArrayPoolLEngineDefault obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~_ArrayPoolLEngineDefault()
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
					AkSoundEnginePINVOKE.CSharp_delete__ArrayPoolLEngineDefault(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public static int Get()
	{
		return AkSoundEnginePINVOKE.CSharp__ArrayPoolLEngineDefault_Get();
	}
}
