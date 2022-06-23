using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/BigNoseShot2")]
public class DraGunBigNoseShot2 : Script
{
	public class EnemyBullet : Bullet
	{
		public const int StartShootDelay = 60;

		public const int MinShootTime = 45;

		public const int MaxShootTime = 90;

		public const int LifeTimeMin = 480;

		public const int LifeTimeMax = 600;

		private Vector2 m_goalPos;

		public EnemyBullet(Vector2 goalPos)
			: base("homing")
		{
			m_goalPos = goalPos;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 startPos = base.Position;
			int travelTime = (int)((m_goalPos - base.Position).magnitude / Speed * 60f);
			int lifeTime = Random.Range(480, 600);
			int nextShot = 60 + Random.Range(45, 90);
			AIAnimator aiAnimator = Projectile.sprite.aiAnimator;
			for (int i = 0; i < travelTime; i++)
			{
				base.Position = Vector2.Lerp(startPos, m_goalPos, (float)i / (float)travelTime);
				aiAnimator.FacingDirection = -90f;
				yield return Wait(1);
			}
			while (base.Tick < lifeTime)
			{
				if (base.Tick >= nextShot)
				{
					aiAnimator.PlayUntilFinished("attack");
					yield return Wait(30);
					Fire(new Direction(0f, DirectionType.Aim), new Speed(12f));
					nextShot = base.Tick + Random.Range(45, 90);
				}
				aiAnimator.FacingDirection = base.AimDirection;
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public int NumTraps = 5;

	private static int[] s_xValues;

	private static int[] s_yValues;

	protected override IEnumerator Top()
	{
		if (s_xValues == null || s_yValues == null)
		{
			s_xValues = new int[NumTraps];
			s_yValues = new int[NumTraps];
			for (int i = 0; i < NumTraps; i++)
			{
				s_xValues[i] = i;
				s_yValues[i] = i;
			}
		}
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 vector = area.UnitBottomLeft + new Vector2(1f, 20f);
		Vector2 vector2 = new Vector2(34f, 11f);
		Vector2 vector3 = new Vector2(vector2.x / (float)NumTraps, vector2.y / (float)NumTraps);
		BraveUtility.RandomizeArray(s_xValues);
		BraveUtility.RandomizeArray(s_yValues);
		for (int j = 0; j < NumTraps; j++)
		{
			int num = s_xValues[j];
			int num2 = s_yValues[j];
			Vector2 goalPos = vector + new Vector2(((float)num + Random.value) * vector3.x, ((float)num2 + Random.value) * vector3.y);
			Fire(new Direction(-90f), new Speed(8f), new EnemyBullet(goalPos));
		}
		return null;
	}
}
