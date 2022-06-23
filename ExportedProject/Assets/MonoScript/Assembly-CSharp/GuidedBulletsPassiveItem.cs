using System;
using UnityEngine;

public class GuidedBulletsPassiveItem : PassiveItem
{
	public float trackingSpeed = 45f;

	public float trackingTime = 6f;

	[CurveRange(0f, 0f, 1f, 1f)]
	public AnimationCurve trackingCurve;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
		}
	}

	private void PostProcessBeam(BeamController obj)
	{
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		obj.PreMoveModifiers = (Action<Projectile>)Delegate.Combine(obj.PreMoveModifiers, new Action<Projectile>(PreMoveProjectileModifier));
	}

	private void PreMoveProjectileModifier(Projectile p)
	{
		if (!m_owner || !p || !(p.Owner is PlayerController))
		{
			return;
		}
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(m_owner.PlayerIDX);
		if (instanceForPlayer == null)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		if (instanceForPlayer.IsKeyboardAndMouse())
		{
			zero = (p.Owner as PlayerController).unadjustedAimPoint.XY() - p.specRigidbody.UnitCenter;
		}
		else
		{
			if (instanceForPlayer.ActiveActions == null)
			{
				return;
			}
			zero = instanceForPlayer.ActiveActions.Aim.Vector;
		}
		float target = zero.ToAngle();
		float current = BraveMathCollege.Atan2Degrees(p.Direction);
		float num = 0f;
		if (p.ElapsedTime < trackingTime)
		{
			num = trackingCurve.Evaluate(p.ElapsedTime / trackingTime) * trackingSpeed;
		}
		float target2 = Mathf.MoveTowardsAngle(current, target, num * BraveTime.DeltaTime);
		Vector2 vector = Quaternion.Euler(0f, 0f, Mathf.DeltaAngle(current, target2)) * p.Direction;
		if (p is HelixProjectile)
		{
			HelixProjectile helixProjectile = p as HelixProjectile;
			helixProjectile.AdjustRightVector(Mathf.DeltaAngle(current, target2));
		}
		if (p.OverrideMotionModule != null)
		{
			p.OverrideMotionModule.AdjustRightVector(Mathf.DeltaAngle(current, target2));
		}
		p.Direction = vector.normalized;
		if (p.shouldRotate)
		{
			p.transform.eulerAngles = new Vector3(0f, 0f, p.Direction.ToAngle());
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<GuidedBulletsPassiveItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
		}
	}
}
