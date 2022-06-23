using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public abstract class MegalichPound1 : Script
{
	public class DyingBullet : Bullet
	{
		private bool m_disappear;

		public DyingBullet(string name, bool disappear)
			: base(name)
		{
			m_disappear = disappear;
		}

		protected override IEnumerator Top()
		{
			if (!m_disappear)
			{
				yield break;
			}
			Vector2 startPosition = base.Position;
			float deathDistance = Random.Range(7f, 19.5f);
			while (true)
			{
				if (Mathf.Abs(base.Position.x - startPosition.x) > deathDistance)
				{
					Vanish();
				}
				yield return Wait(1);
			}
		}
	}

	private const int NumBurstBullets = 12;

	private const int NumOtherBullets = 30;

	private const int NumWallBullets = 60;

	protected abstract float FireDirection { get; }

	protected override IEnumerator Top()
	{
		for (int j = 0; j < 12; j++)
		{
			Fire(new Direction(SubdivideArc(90f + FireDirection * 80f, FireDirection * 45f, 12, j)), new Speed(20f), new Bullet("poundLarge"));
		}
		for (int k = 0; k < 30; k++)
		{
			Fire(new Direction(Random.Range(90f + FireDirection * 115f, 90f + FireDirection * 270f)), new Speed(Random.Range(7, 14)), new Bullet("poundSmall"));
		}
		yield return Wait(60);
		for (int i = 0; i < 60; i++)
		{
			Fire(bullet: (!(Random.value < 0.33f)) ? new DyingBullet("poundSmall", true) : new DyingBullet("poundLarge", false), offset: new Offset(FireDirection * -19.5f, Random.Range(0f, -12f), 0f, string.Empty), direction: new Direction((!(FireDirection > 0f)) ? 180 : 0), speed: new Speed(14f));
			yield return Wait(1);
		}
		yield return Wait(60);
	}
}
