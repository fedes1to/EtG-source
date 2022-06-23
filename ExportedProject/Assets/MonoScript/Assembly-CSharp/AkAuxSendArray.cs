using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AkAuxSendArray : IDisposable
{
	private const int MAX_COUNT = 4;

	private readonly int SIZE_OF_AKAUXSENDVALUE = AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_GetSizeOf();

	private IntPtr m_Buffer;

	private int m_Count;

	public AkAuxSendValue this[int index]
	{
		get
		{
			if (index >= m_Count)
			{
				throw new IndexOutOfRangeException("Out of range access in AkAuxSendArray");
			}
			return new AkAuxSendValue(GetObjectPtr(index), false);
		}
	}

	public bool isFull
	{
		get
		{
			return m_Count >= 4 || m_Buffer == IntPtr.Zero;
		}
	}

	public AkAuxSendArray()
	{
		m_Buffer = Marshal.AllocHGlobal(4 * SIZE_OF_AKAUXSENDVALUE);
		m_Count = 0;
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

	~AkAuxSendArray()
	{
		Dispose();
	}

	public void Reset()
	{
		m_Count = 0;
	}

	public bool Add(GameObject in_listenerGameObj, uint in_AuxBusID, float in_fValue)
	{
		if (isFull)
		{
			return false;
		}
		AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_Set(GetObjectPtr(m_Count), AkSoundEngine.GetAkGameObjectID(in_listenerGameObj), in_AuxBusID, in_fValue);
		m_Count++;
		return true;
	}

	public bool Add(uint in_AuxBusID, float in_fValue)
	{
		if (isFull)
		{
			return false;
		}
		AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_Set(GetObjectPtr(m_Count), ulong.MaxValue, in_AuxBusID, in_fValue);
		m_Count++;
		return true;
	}

	public bool Contains(GameObject in_listenerGameObj, uint in_AuxBusID)
	{
		if (m_Buffer == IntPtr.Zero)
		{
			return false;
		}
		for (int i = 0; i < m_Count; i++)
		{
			if (AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_IsSame(GetObjectPtr(i), AkSoundEngine.GetAkGameObjectID(in_listenerGameObj), in_AuxBusID))
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(uint in_AuxBusID)
	{
		if (m_Buffer == IntPtr.Zero)
		{
			return false;
		}
		for (int i = 0; i < m_Count; i++)
		{
			if (AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_IsSame(GetObjectPtr(i), ulong.MaxValue, in_AuxBusID))
			{
				return true;
			}
		}
		return false;
	}

	public AKRESULT SetValues(GameObject gameObject)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_SetGameObjectAuxSendValues(m_Buffer, AkSoundEngine.GetAkGameObjectID(gameObject), (uint)m_Count);
	}

	public AKRESULT GetValues(GameObject gameObject)
	{
		uint jarg = 4u;
		AKRESULT result = (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_GetGameObjectAuxSendValues(m_Buffer, AkSoundEngine.GetAkGameObjectID(gameObject), ref jarg);
		m_Count = (int)jarg;
		return result;
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
		return (IntPtr)(m_Buffer.ToInt64() + SIZE_OF_AKAUXSENDVALUE * index);
	}
}
