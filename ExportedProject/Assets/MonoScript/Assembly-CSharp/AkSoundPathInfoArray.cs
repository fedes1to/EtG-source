using System;
using System.Runtime.InteropServices;

public class AkSoundPathInfoArray : IDisposable
{
	private readonly int SIZE_OF_STRUCTURE = AkSoundEnginePINVOKE.CSharp_AkSoundPathInfoProxy_GetSizeOf();

	private IntPtr m_Buffer;

	private int m_Count;

	public AkSoundPathInfoArray(int count)
	{
		m_Count = count;
		m_Buffer = Marshal.AllocHGlobal(count * SIZE_OF_STRUCTURE);
	}

	public void Dispose()
	{
		if (m_Buffer != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(m_Buffer);
			m_Buffer = IntPtr.Zero;
			m_Count = 0;
		}
	}

	~AkSoundPathInfoArray()
	{
		Dispose();
	}

	public void Reset()
	{
		m_Count = 0;
	}

	public AkSoundPathInfoProxy GetSoundPathInfo(int index)
	{
		if (index >= m_Count)
		{
			return null;
		}
		return new AkSoundPathInfoProxy(GetObjectPtr(index), false);
	}

	public IntPtr GetBuffer()
	{
		return m_Buffer;
	}

	public int Count()
	{
		return m_Count;
	}

	private IntPtr GetObjectPtr(int index)
	{
		return (IntPtr)(m_Buffer.ToInt64() + SIZE_OF_STRUCTURE * index);
	}
}
