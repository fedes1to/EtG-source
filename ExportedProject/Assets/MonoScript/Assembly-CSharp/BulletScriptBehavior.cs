using Brave.BulletScript;
using UnityEngine;

public class BulletScriptBehavior : BraveBehaviour
{
	public Bullet bullet;

	private int m_firstFrame = -1;

	public void Initialize(Bullet newBullet)
	{
		bullet = newBullet;
		m_firstFrame = -1;
		base.enabled = true;
		if ((bool)base.projectile)
		{
			bullet.AutoRotation = base.projectile.shouldRotate;
			base.projectile.braveBulletScript = this;
		}
		Update();
		m_firstFrame = Time.frameCount;
	}

	public void Update()
	{
		if (m_firstFrame == Time.frameCount || bullet == null)
		{
			return;
		}
		bullet.FrameUpdate();
		if (bullet == null)
		{
			return;
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		if (bullet.DisableMotion)
		{
			if ((bool)speculativeRigidbody)
			{
				speculativeRigidbody.Velocity = Vector2.zero;
			}
		}
		else if ((bool)speculativeRigidbody)
		{
			float deltaTime = BraveTime.DeltaTime;
			Vector2 predictedPosition = bullet.PredictedPosition;
			Vector2 unitPosition = speculativeRigidbody.Position.UnitPosition;
			speculativeRigidbody.Velocity.x = (predictedPosition.x - unitPosition.x) / deltaTime;
			speculativeRigidbody.Velocity.y = (predictedPosition.y - unitPosition.y) / deltaTime;
		}
		else
		{
			base.transform.position = bullet.PredictedPosition;
		}
		if (bullet.AutoRotation)
		{
			base.transform.rotation = Quaternion.identity;
			base.transform.Rotate(0f, 0f, bullet.Direction);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void RemoveBullet()
	{
		if (bullet != null)
		{
			bullet.OnForceRemoved();
			bullet.BulletManager.RemoveBullet(bullet);
		}
	}

	public virtual void HandleBulletDestruction(Bullet.DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool allowProjectileSpawns)
	{
		if (bullet != null)
		{
			if (destroyType != 0)
			{
				bullet.Position = bullet.Projectile.specRigidbody.UnitCenter;
			}
			bullet.HandleBulletDestruction(destroyType, hitRigidbody, allowProjectileSpawns);
		}
	}

	public void OnDespawned()
	{
		bullet = null;
	}
}
