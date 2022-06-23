using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Meduzi/Scream1")]
public class MeduziScream1 : Script
{
	private class TimedBullet : Bullet
	{
		private int m_bulletsFromSafeDir;

		private float m_direction;

		public TimedBullet(int bulletsFromSafeDir, float direction)
			: base("scream")
		{
			m_bulletsFromSafeDir = bulletsFromSafeDir;
			m_direction = direction;
		}

		protected override IEnumerator Top()
		{
			if (m_bulletsFromSafeDir == 4)
			{
				yield return Wait(95);
				UVScrollTriggerableInitializer animator2 = Projectile.sprite.GetComponent<UVScrollTriggerableInitializer>();
				animator2.TriggerAnimation();
				Projectile.Ramp(2f, 10f);
				yield return Wait(45);
				animator2.ResetAnimation();
				yield return Wait(200);
				Vanish();
				yield break;
			}
			if (m_bulletsFromSafeDir > 3)
			{
				yield return Wait(420);
				Vanish();
				yield break;
			}
			Vector2 origin = base.Position;
			int preDelay = 14 * m_bulletsFromSafeDir;
			if (preDelay > 0)
			{
				yield return Wait(preDelay);
			}
			float radius2 = Vector2.Distance(base.Position, origin);
			float angle = Direction;
			float deltaAngle = 45f / 112f;
			base.ManualControl = true;
			int moveTime2 = (3 - m_bulletsFromSafeDir + 1) * 14;
			for (int j = 0; j < moveTime2; j++)
			{
				UpdateVelocity();
				radius2 += Speed / 60f;
				angle -= m_direction * deltaAngle;
				base.Position = origin + BraveMathCollege.DegreesToVector(angle, radius2);
				yield return Wait(1);
			}
			base.ManualControl = false;
			Direction = angle;
			yield return Wait(1);
			yield return Wait(84);
			UVScrollTriggerableInitializer animator = Projectile.sprite.GetComponent<UVScrollTriggerableInitializer>();
			animator.TriggerAnimation();
			radius2 = Vector2.Distance(base.Position, origin);
			base.ManualControl = true;
			moveTime2 = (3 - m_bulletsFromSafeDir + 1) * 14;
			for (int i = 0; i < moveTime2; i++)
			{
				UpdateVelocity();
				radius2 += Speed / 60f;
				angle += m_direction * deltaAngle;
				base.Position = origin + BraveMathCollege.DegreesToVector(angle, radius2);
				yield return Wait(1);
			}
			base.ManualControl = false;
			Direction = angle;
			yield return Wait(240);
			Vanish(true);
		}
	}

	private const int NumWaves = 16;

	private const int NumBulletsPerWave = 64;

	private const int NumGaps = 3;

	private const int StepOpenTime = 14;

	private const int GapHalfWidth = 3;

	private const int GapHoldWaves = 6;

	private const float TurnDegPerWave = 12f;

	private static float[] s_gapAngles;

	private GoopDefinition m_goopDefinition;

	protected override IEnumerator Top()
	{
		bool isCoop = GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER;
		SpeculativeRigidbody target1 = GameManager.Instance.PrimaryPlayer.specRigidbody;
		SpeculativeRigidbody target2 = ((!isCoop) ? null : GameManager.Instance.SecondaryPlayer.specRigidbody);
		int numGaps = ((!isCoop) ? 3 : 2);
		if (s_gapAngles == null || s_gapAngles.Length != numGaps)
		{
			s_gapAngles = new float[numGaps];
		}
		base.EndOnBlank = true;
		m_goopDefinition = base.BulletBank.GetComponent<GoopDoer>().goopDefinition;
		float delta = 5.625f;
		float idealGapAngle1 = (target1.GetUnitCenter(ColliderType.HitBox) - base.Position).ToAngle();
		float idealGapAngle2 = ((!isCoop) ? 0f : (target2.GetUnitCenter(ColliderType.HitBox) - base.Position).ToAngle());
		for (int i = 0; i < 16; i++)
		{
			if (isCoop && numGaps > 1 && BraveMathCollege.AbsAngleBetween(idealGapAngle1, idealGapAngle2) < 22.5f)
			{
				numGaps = 1;
			}
			if (isCoop && numGaps > 1)
			{
				s_gapAngles[0] = idealGapAngle1;
				s_gapAngles[1] = idealGapAngle2;
			}
			else
			{
				for (int j = 0; j < numGaps; j++)
				{
					s_gapAngles[j] = SubdivideCircle(idealGapAngle1, numGaps, j);
				}
			}
			int skipCount = -1000;
			bool skipDirection = BraveUtility.RandomBool();
			if (i % 2 == 0)
			{
				skipCount = Random.Range(0, 3);
			}
			for (int k = 0; k < 64; k++)
			{
				float num = SubdivideCircle(idealGapAngle1, 64, k);
				float num2 = ((numGaps != 1) ? BraveMathCollege.GetNearestAngle(num, s_gapAngles) : s_gapAngles[0]);
				float num3 = BraveMathCollege.ClampAngle180(num2 - num);
				int num4 = Mathf.RoundToInt(Mathf.Abs(num3 / delta));
				if (num4 == skipCount && (num4 == 0 || num3 > 0f == skipDirection))
				{
					num4 = 100;
				}
				Fire(new Direction(num), new Speed(7f), new TimedBullet(num4, Mathf.Sign(num3)));
			}
			yield return Wait(20);
			if (isCoop && numGaps > 1)
			{
				s_gapAngles[0] = idealGapAngle1;
				s_gapAngles[1] = idealGapAngle2;
			}
			else
			{
				for (int l = 0; l < numGaps; l++)
				{
					s_gapAngles[l] = SubdivideCircle(idealGapAngle1, numGaps, l);
				}
			}
			if (!isCoop)
			{
				idealGapAngle1 = BraveMathCollege.GetNearestAngle(base.AimDirection, s_gapAngles);
			}
			SafeUpdateAngle(ref idealGapAngle1, target1);
			if (isCoop && numGaps > 1)
			{
				SafeUpdateAngle(ref idealGapAngle2, target2);
			}
		}
	}

	private void SafeUpdateAngle(ref float idealGapAngle, SpeculativeRigidbody target)
	{
		bool flag = IsSafeAngle(idealGapAngle + 12f, target);
		bool flag2 = IsSafeAngle(idealGapAngle - 12f, target);
		if ((flag && flag2) || (!flag && !flag2))
		{
			idealGapAngle += BraveUtility.RandomSign() * 12f;
		}
		else
		{
			idealGapAngle += (float)(flag ? 1 : (-1)) * 12f;
		}
	}

	private bool IsSafeAngle(float angle, SpeculativeRigidbody target)
	{
		float magnitude = Vector2.Distance(target.GetUnitCenter(ColliderType.HitBox), base.Position);
		Vector2 position = base.Position + BraveMathCollege.DegreesToVector(angle, magnitude);
		if (GameManager.Instance.Dungeon.data.isWall((int)position.x, (int)position.y))
		{
			return false;
		}
		if (DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(m_goopDefinition).IsPositionInGoop(position))
		{
			return false;
		}
		return true;
	}
}
