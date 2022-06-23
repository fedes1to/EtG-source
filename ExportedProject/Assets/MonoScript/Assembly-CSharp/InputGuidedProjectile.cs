using UnityEngine;

public class InputGuidedProjectile : Projectile
{
	[Header("Input Guiding")]
	public float trackingSpeed = 45f;

	public float dumbfireTime;

	private float m_dumbfireTimer;

	protected override void Move()
	{
		bool flag = true;
		if (dumbfireTime > 0f && m_dumbfireTimer < dumbfireTime)
		{
			m_dumbfireTimer += BraveTime.DeltaTime;
			flag = false;
		}
		if (flag && base.Owner is PlayerController)
		{
			BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer((base.Owner as PlayerController).PlayerIDX);
			Vector2 zero = Vector2.zero;
			zero = ((!instanceForPlayer.IsKeyboardAndMouse()) ? instanceForPlayer.ActiveActions.Aim.Vector : ((base.Owner as PlayerController).unadjustedAimPoint.XY() - base.specRigidbody.UnitCenter));
			float target = zero.ToAngle();
			float z = base.transform.eulerAngles.z;
			float z2 = Mathf.MoveTowardsAngle(z, target, trackingSpeed * BraveTime.DeltaTime);
			base.transform.rotation = Quaternion.Euler(0f, 0f, z2);
		}
		base.specRigidbody.Velocity = base.transform.right * baseData.speed;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
