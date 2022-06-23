using UnityEngine;

namespace Brave.BulletScript
{
	public interface IBulletManager
	{
		float TimeScale { get; set; }

		Vector2 PlayerPosition();

		Vector2 PlayerVelocity();

		void BulletSpawnedHandler(Bullet bullet);

		void RemoveBullet(Bullet bullet);

		void DestroyBullet(Bullet deadBullet, bool suppressInAirEffects);

		Vector2 TransformOffset(Vector2 parentPos, string transform);

		float GetTransformRotation(string transform);

		Animation GetUnityAnimation();
	}
}
