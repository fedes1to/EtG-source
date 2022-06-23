using System;

public class AkPropagationPathInfoProxy : AkPropagationPathInfo
{
	private IntPtr swigCPtr;

	internal AkPropagationPathInfoProxy(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfoProxy_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkPropagationPathInfoProxy()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPropagationPathInfoProxy(), true)
	{
	}

	internal static IntPtr getCPtr(AkPropagationPathInfoProxy obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfoProxy_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkPropagationPathInfoProxy()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkPropagationPathInfoProxy(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfoProxy_GetSizeOf();
	}

	public AkVector GetNodePoint(uint idx)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfoProxy_GetNodePoint(swigCPtr, idx);
		return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
	}
}
