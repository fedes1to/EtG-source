using UnityEngine;

namespace Brave.BulletScript
{
	public class Offset : IFireParam
	{
		public float x;

		public float y;

		public string transform;

		public float rotation;

		public DirectionType directionType;

		private Vector2? m_overridePosition;

		public Offset(float x = 0f, float y = 0f, float rotation = 0f, string transform = "", DirectionType directionType = DirectionType.Absolute)
		{
			this.x = x;
			this.y = y;
			this.rotation = rotation;
			this.transform = transform;
			this.directionType = directionType;
		}

		public Offset(Vector2 offset, float rotation = 0f, string transform = "", DirectionType directionType = DirectionType.Absolute)
		{
			x = offset.x;
			y = offset.y;
			this.rotation = rotation;
			this.transform = transform;
			this.directionType = directionType;
		}

		public Offset(string transform)
		{
			x = 0f;
			y = 0f;
			rotation = 0f;
			this.transform = transform;
			directionType = DirectionType.Relative;
		}

		public Vector2 GetPosition(Bullet bullet)
		{
			Vector2? overridePosition = m_overridePosition;
			if (overridePosition.HasValue)
			{
				return m_overridePosition.Value;
			}
			Vector2 vector = bullet.Position;
			if (!string.IsNullOrEmpty(transform))
			{
				vector = bullet.BulletManager.TransformOffset(bullet.Position, transform);
			}
			Vector2 vector2 = new Vector2(x, y);
			if (rotation != 0f)
			{
				vector2 = vector2.Rotate(rotation);
			}
			if (directionType != DirectionType.Absolute)
			{
				if (directionType == DirectionType.Relative)
				{
					vector2 = vector2.Rotate(bullet.Direction);
				}
				else
				{
					Debug.LogError("Cannot use DirectionType {0} in an Offset instance.");
				}
			}
			return vector + vector2;
		}

		public float? GetDirection(Bullet bullet)
		{
			if (string.IsNullOrEmpty(transform))
			{
				return null;
			}
			return bullet.BulletManager.GetTransformRotation(transform);
		}

		public string GetTransformValue()
		{
			return transform;
		}

		public static Offset OverridePosition(Vector2 overridePosition)
		{
			Offset offset = new Offset(0f, 0f, 0f, string.Empty);
			offset.m_overridePosition = overridePosition;
			return offset;
		}
	}
}
