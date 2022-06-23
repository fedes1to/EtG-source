using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DemonWall/BasicLines1")]
public class DemonWallBasicLines1 : Script
{
	public class LineBullet : Bullet
	{
		private float m_horizontalSign;

		public LineBullet(bool doVfx, float horizontalSign)
			: base("line")
		{
			base.SuppressVfx = !doVfx;
			m_horizontalSign = horizontalSign;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			ChangeSpeed(new Speed(9f), 90);
			for (int i = 0; i < 600; i++)
			{
				UpdateVelocity();
				UpdatePosition();
				if (i < 90)
				{
					base.Position += new Vector2(m_horizontalSign * (1f / 90f), 0f);
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public static string[][] shootPoints = new string[3][]
	{
		new string[3] { "sad bullet", "blobulon", "dopey bullet" },
		new string[3] { "left eye", "right eye", "crashed bullet" },
		new string[4] { "sideways bullet", "shotgun bullet", "cultist", "angry bullet" }
	};

	public const int NumBursts = 10;

	protected override IEnumerator Top()
	{
		int group = 1;
		for (int i = 0; i < 10; i++)
		{
			group = BraveUtility.SequentialRandomRange(0, shootPoints.Length, group, null, true);
			int wallLine = 0;
			if (i % 6 == 1)
			{
				wallLine = -1;
			}
			if (i % 6 == 5)
			{
				wallLine = 1;
			}
			FireLine(BraveUtility.RandomElement(shootPoints[group]), wallLine);
			int otherGroup = ((!BraveUtility.RandomBool()) ? 2 : 0);
			if (group != 1)
			{
				otherGroup = ((group == 0) ? 2 : 0);
			}
			float angle = -90 + ((otherGroup != 0) ? 45 : (-45));
			FireCrossBullets(BraveUtility.RandomElement(shootPoints[group]), angle);
			yield return Wait(20);
		}
	}

	private void FireLine(string transform, int wallLine)
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Fire(new Offset(transform), new Direction(-90f), new Speed(9f - (float)i * 1.5f), new LineBullet(i == 0, j - 1));
			}
		}
		if (wallLine != 0)
		{
			Vector2 offset = ((wallLine >= 0) ? new Vector2(23.75f, 3f) : new Vector2(0.5f, 3f));
			for (int k = 0; k < 5; k++)
			{
				Fire(new Offset(offset, 0f, string.Empty), new Direction(-90f), new Speed(9f - (float)k * 1.5f), new LineBullet(true, 0f));
			}
		}
	}

	private void FireCrossBullets(string transform, float angle)
	{
		for (int i = 0; i < 2; i++)
		{
			Offset offset = new Offset(transform);
			Direction direction = new Direction(angle + (float)Random.Range(-30, 30));
			Speed speed = new Speed(2 + 5 * i);
			string name = "wave";
			float newSpeed = 7f;
			int term = 90;
			bool suppressVfx = i != 3;
			Fire(offset, direction, speed, new SpeedChangingBullet(name, newSpeed, term, -1, suppressVfx));
		}
	}
}
