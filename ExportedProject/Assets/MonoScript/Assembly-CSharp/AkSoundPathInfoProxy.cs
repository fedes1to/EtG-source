using System;

public class AkSoundPathInfoProxy : AkSoundPathInfo
{
	private IntPtr swigCPtr;

	internal AkSoundPathInfoProxy(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkSoundPathInfoProxy_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkSoundPathInfoProxy()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkSoundPathInfoProxy(), true)
	{
	}

	internal static IntPtr getCPtr(AkSoundPathInfoProxy obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkSoundPathInfoProxy_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkSoundPathInfoProxy()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkSoundPathInfoProxy(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkSoundPathInfoProxy_GetSizeOf();
	}

	public AkVector GetReflectionPoint(uint idx)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkSoundPathInfoProxy_GetReflectionPoint(swigCPtr, idx);
		return (!(intPtr == IntPtr.Zero)) ? new AkVector(intPtr, false) : null;
	}

	public AkTriangle GetTriangle(uint idx)
	{
		return new AkTriangle(AkSoundEnginePINVOKE.CSharp_AkSoundPathInfoProxy_GetTriangle(swigCPtr, idx), false);
	}
}
