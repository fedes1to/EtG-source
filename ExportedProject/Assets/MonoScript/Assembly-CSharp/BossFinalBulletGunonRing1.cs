using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/GunonRing1")]
public class BossFinalBulletGunonRing1 : Script
{
	public class RingBullet : Bullet
	{
		private const float ReleaseSpinSpeed = 30f;

		private const float ReleaseDriftSpeed = 1f;

		private float m_angle;

		private int m_index;

		private BossFinalBulletGunonRing1 m_parentScript;

		public RingBullet(float angle, int index, BossFinalBulletGunonRing1 parentScript)
		{
			m_angle = angle;
			m_index = index;
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Vector2 center = m_parentScript.Position;
			float radius = m_parentScript.Radius;
			while (!m_parentScript.Destroyed && !m_parentScript.Done)
			{
				m_angle += m_parentScript.SpinDirection * 180f / 60f;
				radius = m_parentScript.Radius;
				if (base.Tick <= 20)
				{
					radius = Mathf.Lerp(0.75f, 4f, (float)base.Tick / 20f);
				}
				base.Position = center + BraveMathCollege.DegreesToVector(m_angle, radius);
				yield return Wait(1);
			}
			int spinTime = m_index * 20;
			for (int i = 0; i < spinTime; i++)
			{
				m_angle += m_parentScript.SpinDirection * 30f / 60f;
				radius += 1f / 60f;
				base.Position = center + BraveMathCollege.DegreesToVector(m_angle, radius);
				yield return Wait(1);
			}
			if ((bool)base.BulletBank && (bool)base.BulletBank.aiAnimator)
			{
				AIAnimator aiAnimator = base.BulletBank.aiAnimator;
				string name = "bat_transform";
				Vector2? position = base.Position;
				aiAnimator.PlayVfx(name, null, null, position);
			}
			Fire(new BatBullet("bat"));
			Vanish(true);
		}
	}

	public class BatBullet : Bullet
	{
		public BatBullet(string name)
			: base(name)
		{
		}

		protected override IEnumerator Top()
		{
			Direction = GetAimDirection(base.Position, (!BraveUtility.RandomBool()) ? 1 : 0, 12f);
			ChangeSpeed(new Speed(12f), 20);
			if (IsPointInTile(base.Position))
			{
				Projectile.IgnoreTileCollisionsFor(1f);
			}
			return null;
		}
	}

	private const int NumBullets = 8;

	private const float FireRadius = 0.75f;

	private const float StartRadius = 4f;

	private const float EndRadius = 9f;

	private const int ExpandTime = 60;

	private const float SpinSpeed = 180f;

	private float SpinDirection;

	private float Radius { get; set; }

	private bool Done { get; set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		yield return Wait(36);
		float startAngle = 90f;
		Radius = 4f;
		bool facingRight = BraveMathCollege.AbsAngleBetween(base.BulletBank.aiAnimator.FacingDirection, 0f) < 90f;
		SpinDirection = ((!facingRight) ? 1 : (-1));
		for (int i = 0; i < 8; i++)
		{
			float angle2 = SubdivideCircle(startAngle, 8, i, 0f - SpinDirection);
			angle2 += SpinDirection * 180f / 60f * (float)(i * 20);
			Fire(new Offset(new Vector2(0.75f, 0f), angle2, string.Empty), new Direction(angle2), new RingBullet(angle2, i, this));
			yield return Wait(20);
		}
		float deltaRadius = 1f / 12f;
		for (int k = 0; k < 60; k++)
		{
			Radius += deltaRadius;
			yield return Wait(1);
		}
		yield return Wait(60);
		for (int j = 0; j < 60; j++)
		{
			Radius -= deltaRadius;
			yield return Wait(1);
		}
		yield return Wait(30);
		Done = true;
	}
}
