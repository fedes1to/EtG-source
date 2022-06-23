using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct dfTouchInfo
{
	private int m_FingerId;

	private Vector2 m_Position;

	private Vector2 m_RawPosition;

	private Vector2 m_PositionDelta;

	private float m_TimeDelta;

	private int m_TapCount;

	private TouchPhase m_Phase;

	public int fingerId
	{
		get
		{
			return m_FingerId;
		}
	}

	public Vector2 position
	{
		get
		{
			return m_Position;
		}
	}

	public Vector2 rawPosition
	{
		get
		{
			return m_RawPosition;
		}
	}

	public Vector2 deltaPosition
	{
		get
		{
			return m_PositionDelta;
		}
	}

	public float deltaTime
	{
		get
		{
			return m_TimeDelta;
		}
	}

	public int tapCount
	{
		get
		{
			return m_TapCount;
		}
	}

	public TouchPhase phase
	{
		get
		{
			return m_Phase;
		}
	}

	public dfTouchInfo(int fingerID, TouchPhase phase, int tapCount, Vector2 position, Vector2 positionDelta, float timeDelta)
	{
		m_FingerId = fingerID;
		m_Phase = phase;
		m_Position = position;
		m_PositionDelta = positionDelta;
		m_TapCount = tapCount;
		m_TimeDelta = timeDelta;
		m_RawPosition = position;
	}

	public static implicit operator dfTouchInfo(Touch touch)
	{
		dfTouchInfo result = default(dfTouchInfo);
		result.m_PositionDelta = touch.deltaPosition;
		result.m_TimeDelta = touch.deltaTime;
		result.m_FingerId = touch.fingerId;
		result.m_Phase = touch.phase;
		result.m_Position = touch.position;
		result.m_TapCount = touch.tapCount;
		return result;
	}
}
