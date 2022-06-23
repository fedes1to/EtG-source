using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class WizardBlueSlam1 : Script
{
	public class ClusterBullet : Bullet
	{
		private WizardBlueSlam1 parent;

		private float clusterAngle;

		public ClusterBullet(WizardBlueSlam1 parent, float clusterAngle)
			: base("Trio")
		{
			this.parent = parent;
			this.clusterAngle = clusterAngle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			for (int j = 0; j < 36; j++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				clusterAngle += -8f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(clusterAngle, 0.4f);
				yield return Wait(1);
			}
			Direction = parent.aimDirection;
			Speed = 8f;
			for (int i = 0; i < 300; i++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				clusterAngle += -8f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(clusterAngle, 0.4f);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int BulletClusters = 22;

	private const int BulletsPerCluster = 3;

	private const float ClusterRadius = 0.4f;

	private const float ClusterRotationDegPerFrame = -8f;

	public float aimDirection;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		for (int i = 0; i < 22; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Fire(new Direction(16.363636f * (float)i), new Speed(9f), new ClusterBullet(this, 120f * (float)j));
			}
		}
		aimDirection = (BulletManager.PlayerPosition() - base.Position).ToAngle();
		yield return Wait(36);
		aimDirection = (BulletManager.PlayerPosition() - base.Position).ToAngle();
	}
}
