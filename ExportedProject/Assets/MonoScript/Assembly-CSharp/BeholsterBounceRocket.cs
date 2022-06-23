using System.Linq;
using UnityEngine;

public class BeholsterBounceRocket : BraveBehaviour
{
	public float modifiedAccelertionFactor = 0.5f;

	public float modifiedAccelerationTime = 1f;

	public AnimationCurve modifiedAccelerationCurve;

	private RobotechProjectile m_projectile;

	private bool m_modifyingAcceleration;

	private float m_modifiedAccelerationTimer;

	private float m_startAcceleration;

	private float m_endAcceleration;

	private bool m_destroyed;

	public void Start()
	{
		m_projectile = GetComponent<RobotechProjectile>();
		if ((bool)m_projectile)
		{
			m_projectile.OnDestruction += OnDestruction;
		}
		BounceProjModifier component = GetComponent<BounceProjModifier>();
		if ((bool)component && (bool)m_projectile)
		{
			component.OnBounce += OnBounce;
			m_startAcceleration = m_projectile.angularAcceleration * modifiedAccelertionFactor;
			m_endAcceleration = m_projectile.angularAcceleration;
		}
	}

	public void Update()
	{
		if (m_modifyingAcceleration)
		{
			m_modifiedAccelerationTimer += BraveTime.DeltaTime;
			m_projectile.angularAcceleration = Mathf.Lerp(m_startAcceleration, m_endAcceleration, modifiedAccelerationCurve.Evaluate(m_modifiedAccelerationTimer / modifiedAccelerationTime));
			if (m_modifiedAccelerationTimer > modifiedAccelerationTime)
			{
				m_modifyingAcceleration = false;
				m_projectile.angularAcceleration = m_endAcceleration;
			}
		}
	}

	private void OnBounce()
	{
		m_modifyingAcceleration = true;
		m_modifiedAccelerationTimer = 0f;
	}

	private void OnDestruction(Projectile source)
	{
		m_destroyed = true;
		BeholsterBounceRocket[] array = Object.FindObjectsOfType<BeholsterBounceRocket>();
		ExplosiveModifier component = GetComponent<ExplosiveModifier>();
		if (array.Length <= 1 || !component)
		{
			return;
		}
		float num = component.explosionData.pushRadius;
		if (base.specRigidbody.PrimaryPixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Circle)
		{
			num += PhysicsEngine.PixelToUnit(base.specRigidbody.PrimaryPixelCollider.ManualDiameter) / 2f;
		}
		for (int i = 0; i < array.Count(); i++)
		{
			BeholsterBounceRocket beholsterBounceRocket = array[i];
			if (!beholsterBounceRocket.m_destroyed && Vector2.Distance(base.specRigidbody.UnitCenter, beholsterBounceRocket.specRigidbody.UnitCenter) < num)
			{
				RobotechProjectile component2 = beholsterBounceRocket.GetComponent<RobotechProjectile>();
				LinearCastResult obj = LinearCastResult.Pool.Allocate();
				obj.Contact = (base.specRigidbody.UnitCenter + beholsterBounceRocket.specRigidbody.UnitCenter) * 0.5f;
				obj.Normal = base.specRigidbody.UnitCenter - beholsterBounceRocket.specRigidbody.UnitCenter;
				obj.OtherPixelCollider = base.specRigidbody.PrimaryPixelCollider;
				obj.MyPixelCollider = beholsterBounceRocket.specRigidbody.PrimaryPixelCollider;
				component2.ForceCollision(base.specRigidbody, obj);
				LinearCastResult.Pool.Free(ref obj);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
