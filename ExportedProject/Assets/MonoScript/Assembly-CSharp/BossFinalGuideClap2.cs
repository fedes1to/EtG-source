using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalGuide/Clap2")]
public class BossFinalGuideClap2 : Script
{
	public class WingBullet : Bullet
	{
		private float m_direction;

		private float m_speedT;

		private float m_timeMultiplier;

		public WingBullet(float direction, float speedT, float timeMultiplier)
		{
			m_direction = direction;
			m_speedT = speedT;
			m_timeMultiplier = timeMultiplier;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(40f * m_timeMultiplier);
			Speed = 0f;
			yield return Wait(90f + 40f * (1f - m_timeMultiplier));
			Speed = 8f * Mathf.Lerp(0.1f, 1f, m_speedT);
			Direction = m_direction;
			yield return Wait(180);
			Vanish();
		}
	}

	private const int SetupTime = 40;

	private const int HoldTime = 90;

	private const float FireSpeed = 8f;

	protected override IEnumerator Top()
	{
		AkSoundEngine.PostEvent("Play_BOSS_cyborg_eagle_01", GameManager.Instance.gameObject);
		base.EndOnBlank = true;
		Vector2 leftOrigin = new Vector2(0f, 0f);
		Vector2 rightOrigin = new Vector2(0f, 0f);
		FireLine(leftOrigin, new Vector2(-3.5f, 2.5f), new Vector2(-12.5f, 2.5f), 16, 180f, 1f, true);
		FireLine(leftOrigin, new Vector2(-7f, 2f), new Vector2(-12f, 2f), 8, 190f);
		FireLine(leftOrigin, new Vector2(-6.75f, 1.5f), new Vector2(-11.5f, 1.5f), 7, 200f);
		FireLine(leftOrigin, new Vector2(-6.5f, 1f), new Vector2(-11f, 1f), 6, 210f);
		FireLine(leftOrigin, new Vector2(-6.25f, 0.5f), new Vector2(-10f, 0.5f), 6, 220f);
		FireLine(leftOrigin, new Vector2(-6f, 0f), new Vector2(-9f, 0f), 5, 230f);
		Vector2 center2 = new Vector2(-1f, 2f);
		for (int i = 0; i < 10; i++)
		{
			float num = -180f + (float)i * 10f;
			float magnitude = 4f - (float)Mathf.Min(i, 5) * 0.1f;
			float magnitude2 = 5f + (float)Mathf.Max(0, 3 - i) * 0.125f;
			Vector2 vector = center2 + BraveMathCollege.DegreesToVector(num, magnitude);
			Vector2 vector2 = center2 + BraveMathCollege.DegreesToVector(num - (float)i * 0.5f, magnitude2);
			FireLine(leftOrigin, vector, vector2, 4, (vector2 - vector).ToAngle(), 1f, true);
		}
		FireLine(rightOrigin, new Vector2(3.5f, 2.5f), new Vector2(12.5f, 2.5f), 16, 0f, 1f, true);
		FireLine(rightOrigin, new Vector2(7f, 2f), new Vector2(12f, 2f), 8, -10f);
		FireLine(rightOrigin, new Vector2(6.75f, 1.5f), new Vector2(11.5f, 1.5f), 7, -20f);
		FireLine(rightOrigin, new Vector2(6.5f, 1f), new Vector2(11f, 1f), 6, -30f);
		FireLine(rightOrigin, new Vector2(6.25f, 0.5f), new Vector2(10f, 0.5f), 6, -40f);
		FireLine(rightOrigin, new Vector2(6f, 0f), new Vector2(9f, 0f), 5, -50f);
		center2 = new Vector2(1f, 2f);
		for (int j = 0; j < 10; j++)
		{
			float num2 = (float)j * -10f;
			float magnitude3 = 4f - (float)Mathf.Min(j, 5) * 0.1f;
			float magnitude4 = 5f + (float)Mathf.Max(0, 3 - j) * 0.125f;
			Vector2 vector3 = center2 + BraveMathCollege.DegreesToVector(num2, magnitude3);
			Vector2 vector4 = center2 + BraveMathCollege.DegreesToVector(num2 + (float)j * 0.5f, magnitude4);
			FireLine(rightOrigin, vector3, vector4, 4, (vector4 - vector3).ToAngle(), 1f, true);
		}
		FireLine(leftOrigin, new Vector2(-0.5f, -3.8f) * 1.5f, 1, -100f);
		FireLine(leftOrigin, new Vector2(-0.3f, -3.9f) * 1.5f, 1, -100f);
		FireLine(leftOrigin, new Vector2(-0.5f, -4f) * 1.5f, 1, -100f);
		FireLine(leftOrigin, new Vector2(-0.7f, -4f) * 1.5f, 1, -100f);
		FireLine(leftOrigin, new Vector2(-0.45f, -4.2f) * 1.5f, 1, -100f);
		FireLine(rightOrigin, new Vector2(0.5f, -3.8f) * 1.5f, 1, -80f);
		FireLine(rightOrigin, new Vector2(0.3f, -3.9f) * 1.5f, 1, -80f);
		FireLine(rightOrigin, new Vector2(0.5f, -4f) * 1.5f, 1, -80f);
		FireLine(rightOrigin, new Vector2(0.7f, -4f) * 1.5f, 1, -80f);
		FireLine(rightOrigin, new Vector2(0.45f, -4.2f) * 1.5f, 1, -80f);
		float angle13 = 180f;
		float delta = -16.363636f;
		FireLine(leftOrigin, new Vector2(-0.9f, 3.5f), angle13);
		angle13 += delta;
		FireLine(leftOrigin, new Vector2(-0.7f, 4.1f), angle13);
		angle13 += delta;
		FireLine(leftOrigin, new Vector2(-0.5f, 4.7f), angle13);
		angle13 += delta;
		FireLine(leftOrigin, new Vector2(-0.4f, 5.3f), angle13);
		angle13 += delta;
		FireLine(leftOrigin, new Vector2(-0.1f, 5.5f), angle13);
		angle13 += delta;
		FireLine(leftOrigin, new Vector2(0.3f, 5.5f), angle13);
		angle13 += delta;
		FireLine(rightOrigin, new Vector2(0.5f, 5.3f), angle13);
		angle13 += delta;
		FireLine(rightOrigin, new Vector2(0.9f, 5.3f), angle13);
		angle13 += delta;
		FireLine(rightOrigin, new Vector2(1f, 4.9f), angle13);
		angle13 += delta;
		FireLine(rightOrigin, new Vector2(0.7f, 4.8f), angle13);
		angle13 += delta;
		FireLine(rightOrigin, new Vector2(0.6f, 4.4f), angle13);
		angle13 += delta;
		FireLine(rightOrigin, new Vector2(0.7f, 3.8f), angle13);
		angle13 += delta;
		yield return Wait(310);
	}

	private void FireLine(Vector2 spawn, Vector2 start, float direction)
	{
		FireLine(spawn, start, start, 1, direction);
	}

	private void FireLine(Vector2 start, Vector2 end, int numBullets, float direction, float timeMultiplier = 1f, bool lerpSpeed = false)
	{
		FireLine(start, start, end, numBullets, direction, timeMultiplier, lerpSpeed);
	}

	private void FireLine(Vector2 spawnPoint, Vector2 start, Vector2 end, int numBullets, float direction, float timeMultiplier = 1f, bool lerpSpeed = false)
	{
		Vector2 vector = (end - start) / Mathf.Max(1, numBullets - 1);
		float num = 2f / 3f * timeMultiplier;
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 vector2 = ((numBullets != 1) ? (start + vector * i) : end);
			float speed = Vector2.Distance(vector2, spawnPoint) / num;
			Fire(new Offset(spawnPoint, 0f, string.Empty), new Direction((vector2 - spawnPoint).ToAngle()), new Speed(speed), new WingBullet(direction, (!lerpSpeed) ? 1f : ((float)i / (float)numBullets), timeMultiplier));
		}
	}
}
