using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/Sweep1")]
public class InfinilichSweep1 : Script
{
	private bool m_isFinished;

	protected override IEnumerator Top()
	{
		StartTask(VerticalAttacks());
		int frames = 45;
		for (int i = 0; i < 6; i++)
		{
			float startAngle = ((i % 2 != 0) ? 60 : 120);
			float endAngle = ((i % 2 != 0) ? (-60) : 240);
			for (int j = 0; j < frames; j++)
			{
				float angle = Mathf.Lerp(startAngle, endAngle, (float)j / ((float)frames - 1f));
				for (int k = 0; k < 1; k++)
				{
					Fire(new Offset(new Vector2(Random.Range(0.5f, 1.5f), 0f), angle, string.Empty), new Direction(angle + (float)Random.Range(-5, 5)), new Speed(Random.Range(9, 15)), new SpeedChangingBullet(12f, 30));
				}
				yield return Wait(1);
			}
		}
		m_isFinished = true;
	}

	private IEnumerator VerticalAttacks()
	{
		while (!m_isFinished)
		{
			float angle2 = Random.Range(60, 120);
			Fire(new Offset(new Vector2(1.5f, 0f), angle2, string.Empty), new Direction(angle2), new Speed(12f));
			angle2 = Random.Range(-60, -120);
			Fire(new Offset(new Vector2(0.75f, 0f), angle2, string.Empty), new Direction(angle2), new Speed(12f));
			yield return Wait(1);
		}
	}
}
