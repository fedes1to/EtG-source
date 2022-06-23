using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Blobulord/Firehose1")]
public class BlobulordFirehose1 : Script
{
	public class FirehoseBullet : Bullet
	{
		private float m_direction;

		public FirehoseBullet(float direction)
			: base("firehose")
		{
			m_direction = direction;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(Random.Range(5, 30));
			Direction += m_direction * Random.Range(10f, 25f);
		}
	}

	private const float SpawnVariance = 0.5f;

	private const float WobbleRange = 35f;

	private const float BreakAwayChance = 0.2f;

	protected override IEnumerator Top()
	{
		float aim = base.AimDirection;
		for (int i = 0; i < 210; i++)
		{
			float newAim = base.AimDirection;
			aim = Mathf.MoveTowardsAngle(aim, newAim, 1f);
			float t = Mathf.PingPong((float)i / 60f, 1f);
			Fire(bullet: ((!(t < 0.1f) && !(t > 0.9f)) || !(Random.value < 0.2f)) ? new Bullet("firehose") : new FirehoseBullet((!(t < 0.1f)) ? 1 : (-1)), offset: new Offset(Random.insideUnitCircle * 0.5f, 0f, string.Empty), direction: new Direction(aim + Mathf.SmoothStep(-35f, 35f, t)), speed: new Speed(14f));
			yield return Wait(1);
		}
	}
}
