namespace Brave.BulletScript
{
	public class Speed : IFireParam
	{
		public SpeedType type;

		public float speed;

		public Speed(float speed = 0f, SpeedType type = SpeedType.Absolute)
		{
			this.speed = speed;
			this.type = type;
		}

		public float GetSpeed(Bullet bullet)
		{
			if (type == SpeedType.Relative || type == SpeedType.Sequence)
			{
				return bullet.Speed + speed;
			}
			return speed;
		}
	}
}
