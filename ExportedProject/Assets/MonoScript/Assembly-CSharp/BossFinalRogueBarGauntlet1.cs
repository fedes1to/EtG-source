using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRogue/BarGauntlet1")]
public class BossFinalRogueBarGauntlet1 : Script
{
	public class BarBullet : Bullet
	{
		private int m_gunNum;

		private float m_centerX;

		private float m_lineWidth;

		private int m_numBullets;

		public BarBullet(int gunNum, float centerX, float lineWidth, int numBullets)
			: base("bar")
		{
			m_gunNum = gunNum;
			m_centerX = centerX;
			m_lineWidth = lineWidth;
			m_numBullets = numBullets;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			float startingX = base.Position.x;
			float desiredX = m_centerX + SubdivideRange(0f, m_lineWidth, m_numBullets, m_gunNum - 1) - m_lineWidth / 2f;
			for (int i = 0; i < 240; i++)
			{
				UpdateVelocity();
				if (i < 30)
				{
					base.Position = new Vector2(Mathf.Lerp(startingX, desiredX, (float)i / 29f), base.Position.y + Velocity.y / 60f);
				}
				else
				{
					UpdatePosition();
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		yield return Wait(180);
		base.EndOnBlank = true;
		switch (Random.Range(0, 3))
		{
		case 0:
		{
			float bulletSeparation3 = 6f;
			Fire(2, 3, bulletSeparation3, 6);
			yield return Wait(5);
			Fire(3, 4, bulletSeparation3, 6);
			yield return Wait(5);
			Fire(4, 5, bulletSeparation3, 6);
			yield return Wait(5);
			Fire(5, 6, bulletSeparation3, 6);
			yield return Wait(5);
			yield return Wait(14);
			for (int i = 0; i < 3; i++)
			{
				Fire(1, 1, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(2, 2, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(3, 3, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(4, 4, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(3, 3, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(2, 2, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(1, 1, bulletSeparation3, 6);
				yield return Wait(5);
				yield return Wait(14);
				Fire(5, 6, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(4, 5, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(3, 4, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(2, 3, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(3, 4, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(4, 5, bulletSeparation3, 6);
				yield return Wait(5);
				Fire(5, 6, bulletSeparation3, 6);
				yield return Wait(5);
				yield return Wait(14);
			}
			break;
		}
		case 1:
		{
			int skipIndex = 3;
			float bulletSeparation3 = 6f;
			for (int j = 0; j < 9; j++)
			{
				skipIndex = BraveUtility.SequentialRandomRange(1, 6, skipIndex, 2, true);
				for (int m = 1; m <= 6; m++)
				{
					if (m < skipIndex || m > skipIndex + 1)
					{
						int num2 = m;
						if (m > 3)
						{
							num2--;
						}
						Fire(num2, m, bulletSeparation3, 6);
					}
				}
				if (j != 8)
				{
					yield return Wait(40);
				}
			}
			break;
		}
		case 2:
		{
			float bulletSeparation3 = 6f;
			for (int k = 0; k < 5; k++)
			{
				for (int l = 1; l <= 6; l++)
				{
					int num = l;
					if (l > 3)
					{
						num--;
					}
					Fire(num, l, bulletSeparation3, 6);
				}
				if (k != 4)
				{
					yield return Wait(80);
				}
			}
			break;
		}
		}
	}

	private void Fire(int gunNum, float lineWidth, int numBullets)
	{
		Fire(gunNum, gunNum, lineWidth, numBullets);
	}

	private void Fire(int shootPointNum, int gunNum, float bulletSeparation, int numBullets)
	{
		Fire(new Offset("bar gun " + shootPointNum), new Direction(-90f), new Speed(8f), new BarBullet(gunNum, base.Position.x, bulletSeparation, numBullets));
	}
}
