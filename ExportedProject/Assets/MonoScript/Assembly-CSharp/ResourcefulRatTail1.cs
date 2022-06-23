using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/ResourcefulRat/Tail1")]
public class ResourcefulRatTail1 : Script
{
	public class TailBullet : Bullet
	{
		private ResourcefulRatTail1 m_parentScript;

		private int m_index;

		private int m_spawnCountdown = -1;

		public TailBullet(ResourcefulRatTail1 parentScript, int index)
			: base("tail", true)
		{
			m_parentScript = parentScript;
			m_index = index;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = true;
			bool hasTold = false;
			while (!m_parentScript.Destroyed && !m_parentScript.IsEnded && !m_parentScript.Done)
			{
				base.Position = m_parentScript.GetPosition(m_index, m_parentScript.Tick);
				if (m_parentScript.ShouldTell)
				{
					if (!hasTold)
					{
						Projectile.sprite.spriteAnimator.Play();
					}
				}
				else
				{
					m_spawnCountdown--;
					if (m_spawnCountdown == 0)
					{
						Fire(new SubtailBullet(m_parentScript));
						Projectile.sprite.spriteAnimator.StopAndResetFrameToDefault();
						m_spawnCountdown = -1;
					}
				}
				yield return Wait(1);
			}
			Speed = 20f + Random.Range(-2f, 2f);
			Direction = m_parentScript.FireAngle + Random.Range(-15f, 15f);
			Projectile.sprite.spriteAnimator.StopAndResetFrameToDefault();
			AkSoundEngine.PostEvent("Play_BOSS_Rat_Tail_Whip_01", GameManager.Instance.gameObject);
			base.ManualControl = false;
			Projectile.specRigidbody.CollideWithTileMap = true;
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = false;
		}

		public void SpawnBullet()
		{
			if (m_spawnCountdown >= 0 || !Projectile)
			{
				Debug.Log("skipped");
				return;
			}
			Debug.Log(Projectile);
			Debug.Log(Projectile.sprite);
			Debug.Log(Projectile.sprite.spriteAnimator);
			m_spawnCountdown = 45;
			Projectile.sprite.spriteAnimator.Play();
			Debug.LogWarning("STARTING SOME SHIT");
		}
	}

	public class SubtailBullet : Bullet
	{
		private ResourcefulRatTail1 m_parentScript;

		public SubtailBullet(ResourcefulRatTail1 parentScript)
			: base(null, true)
		{
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			while (!m_parentScript.Destroyed && !m_parentScript.IsEnded && !m_parentScript.Done)
			{
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public const int NumBullets = 33;

	public const int SpawnDelay = 3;

	public const float RotationSpeed = -360f;

	public const int RotationTime = 266;

	public const int FlashTime = 45;

	public bool ShouldTell { get; set; }

	public bool Done { get; set; }

	public float FireAngle { get; set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		TailBullet[] bullets = new TailBullet[33];
		yield return Wait(10);
		for (int j = 0; j < 33; j++)
		{
			Vector2 pos = GetPosition(j, base.Tick + 1);
			TailBullet bullet = new TailBullet(this, j);
			Fire(Offset.OverridePosition(pos), bullet);
			bullets[j] = bullet;
			if (j % 2 == 0)
			{
				yield return Wait(3);
			}
		}
		int spinTime = 167;
		float currentAngle = 0f;
		for (int i = 0; i < spinTime + 60; i++)
		{
			currentAngle = (GetPosition(16, base.Tick) - base.Position).ToAngle();
			if (i > spinTime && BraveMathCollege.AbsAngleBetween(currentAngle, base.AimDirection + 45f) < 10f)
			{
				break;
			}
			if (i == spinTime - 1)
			{
				ShouldTell = true;
			}
			yield return Wait(1);
		}
		Done = true;
		FireAngle = currentAngle - 90f;
	}

	public Vector2 GetPosition(int index, int tick)
	{
		float num = ((base.Tick > 120) ? (-450f + (float)(tick - 120) / 60f * -360f) : (-90f + -90f * ((float)tick / 60f) * ((float)tick / 60f)));
		float num2 = BraveMathCollege.AbsAngleBetween(num, -90f);
		float num3 = Mathf.Lerp(0.5f, 0.75f, num2 / 70f);
		float magnitude = 1f + (float)index * num3;
		float num4 = (float)index * Mathf.Lerp(0f, 3f, num2 / 120f);
		return BraveMathCollege.DegreesToVector(num + num4, magnitude) + base.Position;
	}
}
