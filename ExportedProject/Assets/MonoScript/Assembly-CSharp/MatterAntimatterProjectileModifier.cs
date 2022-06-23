using System;
using System.Collections;
using UnityEngine;

public class MatterAntimatterProjectileModifier : BraveBehaviour
{
	public bool isAntimatter;

	private bool m_hasAnnihilated;

	public ExplosionData antimatterExplosion;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(0.25f);
		base.specRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.Projectile));
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (m_hasAnnihilated || !otherRigidbody.projectile)
		{
			return;
		}
		MatterAntimatterProjectileModifier component = otherRigidbody.GetComponent<MatterAntimatterProjectileModifier>();
		if ((bool)component && component.isAntimatter != isAntimatter)
		{
			m_hasAnnihilated = true;
			component.m_hasAnnihilated = true;
			otherRigidbody.projectile.DieInAir();
			base.projectile.DieInAir();
			Vector3 vector = (myRigidbody.UnitCenter + otherRigidbody.UnitCenter) / 2f;
			Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.05f);
			GameManager.Instance.BestActivePlayer.ForceBlank(25f, 0.5f, false, false, vector.XY());
			if (isAntimatter)
			{
				Exploder.Explode(vector, antimatterExplosion, Vector2.zero);
			}
			else
			{
				Exploder.Explode(vector, component.antimatterExplosion, Vector2.zero);
			}
		}
		PhysicsEngine.SkipCollision = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
