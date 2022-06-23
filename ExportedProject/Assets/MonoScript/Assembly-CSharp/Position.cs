using System;
using UnityEngine;

[Serializable]
public struct Position
{
	public IntVector2 m_position;

	public Vector2 m_remainder;

	public int X
	{
		get
		{
			return m_position.x;
		}
		set
		{
			m_position.x = value;
			m_remainder.x = 0f;
		}
	}

	public int Y
	{
		get
		{
			return m_position.y;
		}
		set
		{
			m_position.y = value;
			m_remainder.y = 0f;
		}
	}

	public float UnitX
	{
		get
		{
			return (float)m_position.x * 0.0625f + m_remainder.x;
		}
		set
		{
			m_position.x = Mathf.RoundToInt(value * 16f);
			m_remainder.x = value - (float)m_position.x * 0.0625f;
		}
	}

	public float UnitY
	{
		get
		{
			return (float)m_position.y * 0.0625f + m_remainder.y;
		}
		set
		{
			m_position.y = Mathf.RoundToInt(value * 16f);
			m_remainder.y = value - (float)m_position.y * 0.0625f;
		}
	}

	public IntVector2 PixelPosition
	{
		get
		{
			return m_position;
		}
		set
		{
			X = value.x;
			Y = value.y;
		}
	}

	public Vector2 UnitPosition
	{
		get
		{
			return new Vector2((float)m_position.x * 0.0625f + m_remainder.x, (float)m_position.y * 0.0625f + m_remainder.y);
		}
		set
		{
			UnitX = value.x;
			UnitY = value.y;
		}
	}

	public Vector2 Remainder
	{
		get
		{
			return m_remainder;
		}
		set
		{
			m_remainder = value;
		}
	}

	public Position(int pixelX, int pixelY)
	{
		m_position.x = pixelX;
		m_position.y = pixelY;
		m_remainder = Vector2.zero;
	}

	public Position(float unitX, float unitY)
	{
		m_position.x = Mathf.RoundToInt(unitX * 16f);
		m_position.y = Mathf.RoundToInt(unitY * 16f);
		m_remainder.x = unitX - (float)m_position.x * 0.0625f;
		m_remainder.y = unitY - (float)m_position.y * 0.0625f;
	}

	public Position(IntVector2 pixelPosition, Vector2 remainder)
	{
		m_position = pixelPosition;
		m_remainder = remainder;
	}

	public Position(Position position)
		: this(position.m_position, position.m_remainder)
	{
	}

	public Position(Vector2 unitPosition)
		: this(unitPosition.x, unitPosition.y)
	{
	}

	public Position(Vector3 unitPosition)
		: this(unitPosition.x, unitPosition.y)
	{
	}

	public Position(IntVector2 pixelPosition)
		: this(pixelPosition.x, pixelPosition.y)
	{
	}

	public static Position operator +(Position lhs, Vector2 rhs)
	{
		return new Position(lhs.UnitPosition + rhs);
	}

	public static Position operator +(Position lhs, IntVector2 rhs)
	{
		return new Position(lhs.PixelPosition + rhs, lhs.Remainder);
	}

	public Vector2 GetPixelVector2()
	{
		return (Vector2)m_position * 0.0625f;
	}

	public IntVector2 GetPixelDelta(Vector2 unitDelta)
	{
		IntVector2 zero = IntVector2.Zero;
		zero.x = Mathf.RoundToInt((UnitX + unitDelta.x) * 16f) - m_position.x;
		zero.y = Mathf.RoundToInt((UnitY + unitDelta.y) * 16f) - m_position.y;
		return zero;
	}
}
