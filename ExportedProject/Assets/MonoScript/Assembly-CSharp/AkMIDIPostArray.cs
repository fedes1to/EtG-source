using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AkMIDIPostArray
{
	private readonly int m_Count;

	private readonly int SIZE_OF = AkSoundEnginePINVOKE.CSharp_AkMIDIPost_GetSizeOf();

	private IntPtr m_Buffer = IntPtr.Zero;

	public AkMIDIPost this[int index]
	{
		get
		{
			if (index >= m_Count)
			{
				throw new IndexOutOfRangeException("Out of range access in AkMIDIPostArray");
			}
			return new AkMIDIPost(GetObjectPtr(index), false);
		}
		set
		{
			if (index >= m_Count)
			{
				throw new IndexOutOfRangeException("Out of range access in AkMIDIPostArray");
			}
			AkSoundEnginePINVOKE.CSharp_AkMIDIPost_Clone(GetObjectPtr(index), AkMIDIPost.getCPtr(value));
		}
	}

	public AkMIDIPostArray(int size)
	{
		m_Count = size;
		m_Buffer = Marshal.AllocHGlobal(m_Count * SIZE_OF);
	}

	~AkMIDIPostArray()
	{
		Marshal.FreeHGlobal(m_Buffer);
		m_Buffer = IntPtr.Zero;
	}

	public void PostOnEvent(uint in_eventID, GameObject gameObject)
	{
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(gameObject);
		AkSoundEngine.PreGameObjectAPICall(gameObject, akGameObjectID);
		AkSoundEnginePINVOKE.CSharp_AkMIDIPost_PostOnEvent(m_Buffer, in_eventID, akGameObjectID, (uint)m_Count);
	}

	public void PostOnEvent(uint in_eventID, GameObject gameObject, int count)
	{
		if (count >= m_Count)
		{
			throw new IndexOutOfRangeException("Out of range access in AkMIDIPostArray");
		}
		ulong akGameObjectID = AkSoundEngine.GetAkGameObjectID(gameObject);
		AkSoundEngine.PreGameObjectAPICall(gameObject, akGameObjectID);
		AkSoundEnginePINVOKE.CSharp_AkMIDIPost_PostOnEvent(m_Buffer, in_eventID, akGameObjectID, (uint)count);
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
		return (IntPtr)(m_Buffer.ToInt64() + SIZE_OF * index);
	}
}
