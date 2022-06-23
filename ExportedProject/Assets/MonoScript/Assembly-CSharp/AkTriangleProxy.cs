using System;

public class AkTriangleProxy : AkTriangle
{
	private IntPtr swigCPtr;

	internal AkTriangleProxy(IntPtr cPtr, bool cMemoryOwn)
		: base(AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = cPtr;
	}

	public AkTriangleProxy()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkTriangleProxy(), true)
	{
	}

	internal static IntPtr getCPtr(AkTriangleProxy obj)
	{
		return (obj != null) ? obj.swigCPtr : IntPtr.Zero;
	}

	internal override void setCPtr(IntPtr cPtr)
	{
		base.setCPtr(AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_SWIGUpcast(cPtr));
		swigCPtr = cPtr;
	}

	~AkTriangleProxy()
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
					AkSoundEnginePINVOKE.CSharp_delete_AkTriangleProxy(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public void Clear()
	{
		AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_Clear(swigCPtr);
	}

	public void DeleteName()
	{
		AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_DeleteName(swigCPtr);
	}

	public static int GetSizeOf()
	{
		return AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_GetSizeOf();
	}
}
