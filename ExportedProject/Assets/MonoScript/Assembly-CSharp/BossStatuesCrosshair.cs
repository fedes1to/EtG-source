using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossStatues/Crosshair")]
public class BossStatuesCrosshair : Script
{
	public class LineBullet : Bullet
	{
		public int spawnTime;

		public LineBullet(int spawnTime)
			: base("defaultLine", false, false, true)
		{
			this.spawnTime = spawnTime;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(), SetupTime);
			yield return Wait(SetupTime);
			ChangeDirection(new Direction(90f, DirectionType.Relative));
			yield return Wait(1);
			ChangeSpeed(new Speed((float)spawnTime / (float)BulletCount * (Radius * 2f) * QuarterPi * (60f / (float)QuaterRotTime), SpeedType.Relative));
			ChangeDirection(new Direction(90f / (float)QuaterRotTime, DirectionType.Sequence), SpinTime);
			yield return Wait(SpinTime - 1);
			Vanish();
		}
	}

	public class CircleBullet : Bullet
	{
		public int spawnTime;

		public CircleBullet(int spawnTime)
			: base("defaultCircle")
		{
			this.spawnTime = spawnTime;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(), SetupTime);
			yield return Wait(SetupTime);
			ChangeDirection(new Direction(90f, DirectionType.Relative));
			yield return Wait(1);
			ChangeSpeed(new Speed(Radius * 2f * QuarterPi * (60f / (float)QuaterRotTime), SpeedType.Relative));
			ChangeDirection(new Direction(90f / (float)QuaterRotTime, DirectionType.Sequence), QuaterRotTime);
			yield return Wait((float)spawnTime * ((float)QuaterRotTime / (float)BulletCount));
			ChangeSpeed(new Speed());
			for (int i = 1; i < 7; i++)
			{
				Fire(new Direction(-90f, DirectionType.Relative), new Speed(i * 3), new CircleExtraBullet(spawnTime));
			}
			yield return Wait((float)(BulletCount - spawnTime) * ((float)QuaterRotTime / (float)BulletCount));
			yield return Wait(SpinTime - QuaterRotTime);
			Vanish();
		}
	}

	public class CircleExtraBullet : Bullet
	{
		public int spawnTime;

		public CircleExtraBullet(int spawnTime)
			: base("defaultCircleExtra")
		{
			this.spawnTime = spawnTime;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(), 60);
			yield return Wait((float)(BulletCount - spawnTime) * ((float)QuaterRotTime / (float)BulletCount));
			yield return Wait(SpinTime - QuaterRotTime);
			ChangeSpeed(new Speed(6f));
			yield return Wait(120);
			Vanish();
		}
	}

	public static float QuarterPi = 0.785f;

	public static int SkipSetupBulletNum = 6;

	public static int ExtraSetupBulletNum;

	public static int SetupTime = 90;

	public static int BulletCount = 25;

	public static float Radius = 11f;

	public static int QuaterRotTime = 120;

	public static int SpinTime = 600;

	public static int PulseInitialDelay = 120;

	public static int PulseDelay = 120;

	public static int PulseCount = 4;

	public static int PulseTravelTime = 100;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		FireSpinningLine(90f);
		FireCircleSegment(90f);
		FireSpinningLine(0f);
		FireCircleSegment(0f);
		FireSpinningLine(-90f);
		FireCircleSegment(-90f);
		FireSpinningLine(180f);
		FireCircleSegment(180f);
		yield return Wait(SetupTime + PulseInitialDelay - PulseDelay);
		for (int i = 0; i < PulseCount; i++)
		{
			yield return Wait(PulseDelay);
			FirePulse();
		}
		yield return Wait(SpinTime - (PulseDelay * (PulseCount - 1) + PulseInitialDelay));
	}

	private void FireSpinningLine(float dir)
	{
		float num = (float)SkipSetupBulletNum * (Radius * 2f * (60f / (float)SetupTime) / (float)BulletCount);
		float num2 = Radius * 2f * (60f / (float)SetupTime) / (float)BulletCount;
		for (int i = 0; i < BulletCount + ExtraSetupBulletNum - SkipSetupBulletNum; i++)
		{
			Fire(new Direction(dir), new Speed(num + num2 * (float)i), new LineBullet(i + SkipSetupBulletNum));
		}
	}

	private void FireCircleSegment(float dir)
	{
		for (int i = 0; i < BulletCount; i++)
		{
			Fire(new Direction(dir), new Speed(Radius * 2f * (60f / (float)SetupTime)), new CircleBullet(i));
		}
	}

	private void FirePulse()
	{
		float num = 4.5f;
		for (int i = 0; i < 80; i++)
		{
			Fire(new Direction(((float)i + 0.5f) * num), new Speed(Radius / ((float)PulseTravelTime / 60f)), new Bullet("defaultPulse", false, false, true));
		}
	}
}
