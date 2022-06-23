using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/MorphGun1")]
public class InfinilichMorphGun1 : Script
{
	public class GunBullet : Bullet
	{
		private int m_delay;

		public GunBullet(int delay)
		{
			m_delay = delay;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(m_delay * 8);
			Speed = 12f;
			Direction = GetAimDirection(BraveUtility.RandomBool() ? 1 : 0, Speed);
			AkSoundEngine.PostEvent("Play_WPN_minigun_shot_01", base.BulletBank.gameObject);
		}
	}

	private static int[][] LeftBulletOrder = new int[7][]
	{
		new int[2] { 1, 8 },
		new int[2] { 2, 9 },
		new int[2] { 3, 10 },
		new int[3] { 4, 11, 15 },
		new int[4] { 5, 12, 16, 19 },
		new int[5] { 6, 13, 17, 20, 22 },
		new int[5] { 7, 14, 18, 21, 23 }
	};

	private static int[][] RightBulletOrder = new int[7][]
	{
		new int[2] { 2, 9 },
		new int[2] { 1, 8 },
		new int[2] { 4, 11 },
		new int[3] { 3, 10, 15 },
		new int[4] { 7, 14, 18, 21 },
		new int[5] { 6, 13, 17, 20, 23 },
		new int[5] { 5, 12, 16, 19, 22 }
	};

	private float m_sign;

	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle180(base.BulletBank.aiAnimator.FacingDirection);
		m_sign = ((num <= 90f && num >= -90f) ? 1 : (-1));
		Vector2 vector = base.Position + new Vector2(m_sign * 2.5f, 1f);
		float direction = (BulletManager.PlayerPosition() - vector).ToAngle();
		int[][] array = ((!(m_sign < 0f)) ? RightBulletOrder : LeftBulletOrder);
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < array[i].Length; j++)
			{
				string transform = "morph bullet " + array[i][j];
				Fire(new Offset(transform), new Direction(direction), new Speed(), new GunBullet(i));
			}
		}
		return null;
	}
}
