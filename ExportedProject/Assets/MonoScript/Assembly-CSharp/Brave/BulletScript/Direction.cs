using UnityEngine;

namespace Brave.BulletScript
{
	public class Direction : IFireParam
	{
		public DirectionType type;

		public float direction;

		public float maxFrameDelta;

		public Direction(float direction = 0f, DirectionType type = DirectionType.Absolute, float maxFrameDelta = -1f)
		{
			this.direction = direction;
			this.type = type;
			this.maxFrameDelta = maxFrameDelta;
		}

		public float GetDirection(Bullet bullet, float? overrideBaseDirection = null)
		{
			float num;
			if (type == DirectionType.Aim)
			{
				num = (bullet.BulletManager.PlayerPosition() - bullet.Position).ToAngle() + direction;
			}
			else if (type == DirectionType.Relative || type == DirectionType.Sequence)
			{
				float num2 = ((!overrideBaseDirection.HasValue) ? bullet.Direction : overrideBaseDirection.Value);
				num = num2 + direction;
			}
			else
			{
				num = direction;
			}
			if (maxFrameDelta > 0f)
			{
				float value = BraveMathCollege.ClampAngle180(num - bullet.Direction);
				num = bullet.Direction + Mathf.Clamp(value, 0f - maxFrameDelta, maxFrameDelta);
			}
			return num;
		}
	}
}
