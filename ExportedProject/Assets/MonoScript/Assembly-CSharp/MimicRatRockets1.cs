using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class MimicRatRockets1 : Script
{
	public class ArcBullet : Bullet
	{
		private Vector2 m_target;

		private float m_offsetAngle;

		public ArcBullet(Vector2 target, float offsetAngle)
		{
			m_target = target;
			m_offsetAngle = offsetAngle;
		}

		protected override IEnumerator Top()
		{
			Direction += m_offsetAngle;
			float turnDelta = Mathf.Abs(m_offsetAngle * 2f) / ((m_target - base.Position).magnitude / Speed);
			for (int i = 0; i < 120; i++)
			{
				float targetDirection = (m_target - base.Position).ToAngle();
				if (BraveMathCollege.AbsAngleBetween(Direction, targetDirection) > 145f)
				{
					break;
				}
				ChangeDirection(new Direction(targetDirection, DirectionType.Absolute, turnDelta / 75f));
				yield return Wait(1);
			}
		}
	}

	protected override IEnumerator Top()
	{
		Vector2 leftGun = base.BulletBank.GetTransform("left gun").transform.position.XY();
		FireRocket(leftGun, -5, -5);
		FireRocket(leftGun, -5, 5);
		FireRocket(leftGun, 5, -5);
		FireRocket(leftGun, 5, 5);
		yield return Wait(42);
		Vector2 rightGun = base.BulletBank.GetTransform("right gun").transform.position.XY();
		FireRocket(rightGun, -5, -5);
		FireRocket(rightGun, -5, 5);
		FireRocket(rightGun, 5, -5);
		FireRocket(rightGun, 5, 5);
	}

	private void FireRocket(Vector2 start, int xOffset, int yOffset)
	{
		for (int i = 0; i < 3; i++)
		{
			if (i != 1 || !BraveUtility.RandomBool())
			{
				Vector2 target = ((!BraveUtility.RandomBool()) ? GetPredictedTargetPosition(1f, 14f) : BulletManager.PlayerPosition());
				target += BraveMathCollege.DegreesToVector(RandomAngle(), Random.Range(0f, 2.5f));
				float offsetAngle = (float)(i - 1) * Random.Range(25f, 90f);
				Fire(Offset.OverridePosition(start + new Vector2((float)xOffset / 16f, (float)yOffset / 16f)), new Direction(0f, DirectionType.Aim), new Speed(Random.Range(10f, 11f)), new ArcBullet(target, offsetAngle));
			}
		}
	}
}
