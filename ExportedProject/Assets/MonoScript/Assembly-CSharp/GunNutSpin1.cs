using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("GunNut/ChainSpin1")]
public class GunNutSpin1 : Script
{
	private class SpinBullet : Bullet
	{
		private GunNutSpin1 m_parentScript;

		private float m_maxDist;

		private bool m_isBall;

		public SpinBullet(GunNutSpin1 parentScript, float maxDist, bool isBall)
			: base((!isBall) ? "link" : "ball")
		{
			m_parentScript = parentScript;
			m_maxDist = maxDist;
			m_isBall = isBall;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = true;
			float startDist = Vector2.Distance(base.Position, m_parentScript.Position);
			while (!m_parentScript.Destroyed && !m_parentScript.IsEnded)
			{
				if (m_parentScript.BulletBank.healthHaver.IsDead)
				{
					Vanish();
					yield break;
				}
				float angle = 90f + m_parentScript.TurnSpeed / 60f * (float)(m_parentScript.Tick + 3);
				if (m_isBall && m_parentScript.ShouldThrowBolas)
				{
					float num = BraveMathCollege.ClampAngle180(base.AimDirection - (angle - 90f));
					if (num >= 0f && num < 45f && !IsPointInTile(base.Position))
					{
						float aimDirection = base.AimDirection;
						Fire(new Direction(aimDirection), new Speed(17f), new BolasBullet(true, -2f));
						Fire(new Direction(aimDirection), new Speed(17f), new BolasBullet(false, -1f));
						Fire(new Direction(aimDirection), new Speed(17f), new BolasBullet(false, 0f));
						Fire(new Direction(aimDirection), new Speed(17f), new BolasBullet(false, 1f));
						Fire(new Direction(aimDirection), new Speed(17f), new BolasBullet(true, 2f));
						m_parentScript.WasThrown();
						yield break;
					}
				}
				float dist = ((m_parentScript.TicksRemaining >= 60) ? Mathf.Lerp(startDist, m_maxDist, (float)m_parentScript.Tick / 30f) : Mathf.Lerp(0f, m_maxDist, (float)m_parentScript.TicksRemaining / 45f));
				base.Position = m_parentScript.Position + BraveMathCollege.DegreesToVector(angle, dist);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class BolasBullet : Bullet
	{
		public const float ExpandTime = 60f;

		public const float RotationTime = 60f;

		private float m_offset;

		public BolasBullet(bool isBall, float offset)
			: base((!isBall) ? "link" : "ball_trail")
		{
			m_offset = offset;
		}

		protected override IEnumerator Top()
		{
			Vector2 truePosition = base.Position;
			base.ManualControl = true;
			Projectile.IgnoreTileCollisionsFor(0.2f);
			while (true)
			{
				UpdateVelocity();
				truePosition += Velocity / 60f;
				Vector2 offset2 = new Vector2(m_offset * Mathf.Lerp(0f, 1f, (float)base.Tick / 60f), 0f);
				offset2 = offset2.Rotate((float)base.Tick / 60f * -360f);
				base.Position = truePosition + offset2;
				yield return null;
			}
		}
	}

	public static string[] Transforms = new string[4] { "bullet hand", "bullet limb 1", "bullet limb 2", "bullet limb 3" };

	public const int NumBullets = 9;

	public const int BaseTurnSpeed = 540;

	public const float MaxDist = 6f;

	public const int ExtendTime = 30;

	public const int Lifetime = 120;

	public const int ContractTime = 45;

	public const int TellTime = 30;

	public const int BolasThrowTime = 120;

	public float TurnSpeed;

	public int TicksRemaining;

	private List<SpinBullet> bullets;

	public bool IsTellingBolas { get; set; }

	public bool ShouldThrowBolas { get; set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		base.BulletBank.aiAnimator.ChildAnimator.OverrideIdleAnimation = "twirl";
		base.BulletBank.aiAnimator.ChildAnimator.OverrideMoveAnimation = "twirl_move";
		base.BulletBank.aiAnimator.ChildAnimator.renderer.enabled = false;
		float turnSign = ((BraveMathCollege.AbsAngleBetween(base.BulletBank.aiAnimator.FacingDirection, 0f) > 90f) ? 1 : (-1));
		TurnSpeed = 540f * turnSign;
		bullets = new List<SpinBullet>(9);
		for (int i = 0; i < 9; i++)
		{
			float num = ((float)i + 0.5f) / 8.5f;
			int num2 = Mathf.CeilToInt(Mathf.Lerp(Transforms.Length - 1, 0f, num));
			SpinBullet spinBullet = new SpinBullet(this, num * 6f, i == 8);
			Fire(new Offset(Transforms[num2]), new Speed(), spinBullet);
			bullets.Add(spinBullet);
		}
		TicksRemaining = 192;
		int respawnCooldown = 0;
		while (TicksRemaining > 0)
		{
			if (base.Tick == 30 && CanThrowBolas())
			{
				StartBolasTell();
			}
			if (base.Tick == 120 && IsTellingBolas)
			{
				ShouldThrowBolas = true;
			}
			respawnCooldown--;
			for (int j = 0; j < bullets.Count; j++)
			{
				SpinBullet spinBullet2 = bullets[j];
				if ((spinBullet2.Destroyed || ((bool)spinBullet2.Projectile && !spinBullet2.Projectile.isActiveAndEnabled)) && respawnCooldown <= 0)
				{
					float num3 = ((float)j + 1f) / 9f;
					float angle = 90f + TurnSpeed / 60f * (float)(base.Tick + 1 + 3);
					float magnitude = ((TicksRemaining >= 60) ? Mathf.Lerp(0f, num3 * 6f, (float)base.Tick / 30f) : Mathf.Lerp(0f, num3 * 6f, (float)TicksRemaining / 45f));
					Vector2 overridePosition = base.Position + BraveMathCollege.DegreesToVector(angle, magnitude);
					SpinBullet spinBullet3 = new SpinBullet(this, num3 * 6f, j == 8);
					Fire(Offset.OverridePosition(overridePosition), new Speed(), spinBullet3);
					bullets[j] = spinBullet3;
					respawnCooldown = 4;
					break;
				}
			}
			yield return Wait(1);
			TicksRemaining--;
		}
		for (int k = 0; k < bullets.Count; k++)
		{
			if (bullets[k] != null)
			{
				bullets[k].Vanish(k < 8);
			}
		}
		bullets = null;
		base.BulletBank.aiAnimator.ChildAnimator.OverrideIdleAnimation = null;
		base.BulletBank.aiAnimator.ChildAnimator.OverrideMoveAnimation = null;
		base.BulletBank.aiAnimator.ChildAnimator.renderer.enabled = false;
	}

	public override void OnForceEnded()
	{
		base.OnForceEnded();
		bullets = null;
		base.BulletBank.aiAnimator.ChildAnimator.OverrideIdleAnimation = null;
		base.BulletBank.aiAnimator.ChildAnimator.OverrideMoveAnimation = null;
		base.BulletBank.aiAnimator.ChildAnimator.renderer.enabled = false;
	}

	private bool CanThrowBolas()
	{
		if (GameManager.HasInstance && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST && GameManager.Instance.PrimaryPlayer.characterIdentity != PlayableCharacters.Bullet)
		{
			return false;
		}
		if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_CATACOMBS) <= 0f && GameStatsManager.Instance.QueryEncounterable(base.BulletBank.encounterTrackable) < 15)
		{
			return false;
		}
		if ((bool)base.BulletBank && (bool)base.BulletBank.aiActor && base.BulletBank.aiActor.ParentRoom != null)
		{
			return base.BulletBank.aiActor.ParentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) < 2;
		}
		return false;
	}

	public void StartBolasTell()
	{
		IsTellingBolas = true;
		PostWwiseEvent("Play_ENM_CannonArmor_Charge_01");
		for (int i = 0; i < bullets.Count; i++)
		{
			SpinBullet spinBullet = bullets[i];
			if (spinBullet != null && (bool)spinBullet.Projectile)
			{
				spinBullet.Projectile.spriteAnimator.Play();
			}
		}
		base.BulletBank.aiAnimator.ChildAnimator.renderer.enabled = true;
	}

	public void WasThrown()
	{
		IsTellingBolas = false;
		PostWwiseEvent("Play_OBJ_Chainpot_Drop_01");
		for (int num = bullets.Count - 1; num > 2; num--)
		{
			bullets[num].Vanish(true);
			bullets.RemoveAt(num);
		}
		for (int i = 0; i < bullets.Count; i++)
		{
			SpinBullet spinBullet = bullets[i];
			if (spinBullet != null && (bool)spinBullet.Projectile)
			{
				spinBullet.Projectile.spriteAnimator.StopAndResetFrameToDefault();
			}
		}
		base.BulletBank.aiAnimator.ChildAnimator.renderer.enabled = false;
	}
}
