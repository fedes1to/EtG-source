using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/IconStomp1")]
public class MetalGearRatIconStomp1 : Script
{
	private class LineDummy : Bullet
	{
		protected override IEnumerator Top()
		{
			while (true)
			{
				yield return Wait(1);
			}
		}
	}

	private class IconBullet : Bullet
	{
		private LineDummy m_lineDummy;

		private float scale;

		public IconBullet(LineDummy lineDummy, float scale)
		{
			m_lineDummy = lineDummy;
			this.scale = scale;
		}

		protected override IEnumerator Top()
		{
			Vector2 startingOffset = base.Position - m_lineDummy.Position;
			Vector2 desiredOffset = BraveMathCollege.DegreesToVector(Direction + 90f, scale) + new Vector2(0f, Random.Range(-0.4f, 0.4f));
			base.ManualControl = true;
			for (int i = 0; i < 61; i++)
			{
				base.Position = m_lineDummy.Position + Vector2.Lerp(startingOffset, desiredOffset, (float)base.Tick / 60f);
				yield return Wait(1);
			}
			Speed = 9f;
			base.ManualControl = false;
		}
	}

	public int[] delay = new int[9] { 10, 15, 15, 15, 10, 15, 10, 10, 0 };

	public int[] xOffsets1 = new int[9] { 0, -4, -9, -13, -16, -19, -18, -15, -11 };

	public int[] yOffsets1 = new int[9] { 0, 3, 8, 13, 16, 21, 36, 39, 44 };

	public int[] xOffsets2 = new int[9] { 40, 33, 26, 19, 14, 8, 7, 11, 16 };

	public int[] yOffsets2 = new int[9] { -1, 2, 5, 8, 11, 18, 28, 38, 43 };

	private const int SpreadTime = 60;

	private const int HalfBulletsPerLine = 8;

	private const float LineWidth = 20f;

	private const float GapWidth = 3.5f;

	private const float BulletSpeed = 9f;

	private const float AngleVariance = 8f;

	private const float PositionVariance = 0.4f;

	protected override IEnumerator Top()
	{
		yield return Wait(160);
		List<LineDummy> lineDummies = new List<LineDummy>(delay.Length);
		StartTask(FireBullets(lineDummies));
		for (int i = 0; i < 180; i++)
		{
			for (int j = 0; j < lineDummies.Count; j++)
			{
				lineDummies[j].DoTick();
			}
			yield return Wait(1);
		}
	}

	private IEnumerator FireBullets(List<LineDummy> lineDummies)
	{
		float direction = base.AimDirection;
		float flipScalar = ((!(BraveMathCollege.AbsAngleBetween(direction, 90f) < 90f)) ? 1 : (-1));
		for (int i = 0; i < delay.Length; i++)
		{
			Vector2 spawnOffset1 = PhysicsEngine.PixelToUnit(new IntVector2(xOffsets1[i], yOffsets1[i]));
			Vector2 spawnOffset2 = PhysicsEngine.PixelToUnit(new IntVector2(xOffsets2[i], yOffsets2[i]));
			LineDummy lineDummy = new LineDummy();
			lineDummy.Position = base.Position + (spawnOffset1 + spawnOffset2) / 2f;
			lineDummy.Direction = direction;
			lineDummy.Speed = 9f;
			lineDummy.BulletManager = BulletManager;
			lineDummy.Initialize();
			lineDummies.Add(lineDummy);
			for (int j = 0; j < 8; j++)
			{
				float num = SubdivideRange(0f, 10f, 8, j) + 1.75f;
				Fire(new Offset(spawnOffset1, 0f, string.Empty), new Direction(direction), new IconBullet(lineDummy, (0f - num) * flipScalar));
				Fire(new Offset(spawnOffset2, 0f, string.Empty), new Direction(direction), new IconBullet(lineDummy, num * flipScalar));
			}
			yield return Wait(delay[i]);
			direction += (float)Random.Range(-1, 2) * 8f;
		}
	}
}
