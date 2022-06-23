using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/CarnageSpin1")]
public class InfinilichCarnageSpin1 : Script
{
	public class TipBullet : Bullet
	{
		private InfinilichCarnageSpin1 m_parentScript;

		public TipBullet(InfinilichCarnageSpin1 parentScript)
			: base("carnageTip")
		{
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			float spinSpeed = SpinDirection * Random.Range(0.5f, 0.8f);
			for (int i = 0; i < 60; i++)
			{
				Fire(new Direction(0f, DirectionType.Relative), new Speed(), new ChainBullet(m_parentScript, i, Speed, spinSpeed));
				yield return Wait((Speed > 20f) ? 1 : 2);
			}
			while (!m_parentScript.Spin)
			{
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class ChainBullet : Bullet
	{
		private const float WiggleMagnitude = 0.75f;

		private const float WigglePeriodMultiplier = 0.333f;

		private InfinilichCarnageSpin1 m_parentScript;

		private int m_spawnDelay;

		private float m_tipSpeed;

		private float m_spinSpeed;

		public ChainBullet(InfinilichCarnageSpin1 parentScript, int spawnDelay, float tipSpeed, float spinSpeed)
		{
			m_parentScript = parentScript;
			m_spawnDelay = spawnDelay;
			m_tipSpeed = tipSpeed;
			m_spinSpeed = spinSpeed;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			float wigglePeriod = 0.333f * m_tipSpeed;
			float currentOffset = 0f;
			for (int k = 0; k < 45 - m_spawnDelay; k++)
			{
				float magnitude3 = 0.75f;
				magnitude3 = Mathf.Min(magnitude3, Mathf.Lerp(0f, 0.75f, (float)k / 20f));
				magnitude3 = Mathf.Min(magnitude3, Mathf.Lerp(0f, 0.75f, (float)m_spawnDelay / 10f));
				currentOffset = Mathf.SmoothStep(0f - magnitude3, magnitude3, Mathf.PingPong(0.5f + (float)k / 60f * wigglePeriod, 1f));
				base.Position = truePosition + BraveMathCollege.DegreesToVector(Direction - 90f, currentOffset);
				yield return Wait(1);
			}
			float lastOffset = currentOffset;
			for (int j = 0; j < 3; j++)
			{
				base.Position = truePosition + BraveMathCollege.DegreesToVector(magnitude: Mathf.Lerp(lastOffset, 0f, (float)j / 2f), angle: Direction - 90f);
				yield return Wait(1);
			}
			while (!m_parentScript.Spin)
			{
				yield return Wait(1);
			}
			float angle = (base.Position - m_parentScript.Position).ToAngle();
			float radius = (base.Position - m_parentScript.Position).magnitude;
			for (int i = 0; i < 420; i++)
			{
				float deltaAngle = Mathf.Lerp(0f, m_spinSpeed, (float)i / 60f);
				angle += deltaAngle;
				base.Position = m_parentScript.Position + BraveMathCollege.DegreesToVector(angle, radius);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int SpinTime = 420;

	private static float SpinDirection;

	public bool Spin;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		SpinDirection = BraveUtility.RandomSign();
		Fire(new Offset("limb 1"), new Direction(45f), new Speed(32f), new TipBullet(this));
		Fire(new Offset("limb 2"), new Direction(-45f), new Speed(32f), new TipBullet(this));
		Fire(new Offset("limb 3"), new Direction(-135f), new Speed(32f), new TipBullet(this));
		Fire(new Offset("limb 4"), new Direction(135f), new Speed(32f), new TipBullet(this));
		yield return Wait(60);
		for (int i = 0; i < 6; i++)
		{
			Fire(new Direction(RandomAngle()), new Speed(16f), new TipBullet(this));
			yield return Wait(30);
		}
		yield return Wait(90);
		Spin = true;
		yield return Wait(420);
	}
}
