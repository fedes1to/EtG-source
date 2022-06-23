using System;
using UnityEngine;

public class SlideSurface : MonoBehaviour
{
	private FlippableCover m_table;

	private SurfaceDecorator m_surface;

	public void Awake()
	{
		m_table = GetComponent<FlippableCover>();
		if (!m_table && (bool)base.transform.parent)
		{
			m_table = base.transform.parent.GetComponent<FlippableCover>();
		}
		if ((bool)m_table)
		{
			m_surface = m_table.GetComponent<SurfaceDecorator>();
		}
		SpeculativeRigidbody component = GetComponent<SpeculativeRigidbody>();
		component.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(component.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
	}

	private void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!otherRigidbody)
		{
			return;
		}
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if ((bool)component && component.CurrentRollState == PlayerController.DodgeRollState.InAir && IsAccessible(component))
		{
			if (!component.IsSlidingOverSurface)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_SLID_OVER_TABLE, 1f);
			}
			component.IsSlidingOverSurface = true;
			PhysicsEngine.SkipCollision = true;
			if ((bool)m_surface)
			{
				m_surface.Destabilize(component.specRigidbody.Velocity);
			}
		}
	}

	public bool IsAccessible(PlayerController collidingPlayer)
	{
		if ((bool)m_table)
		{
			return !m_table.IsFlipped && !m_table.IsBroken && (collidingPlayer.IsSlidingOverSurface || true);
		}
		return true;
	}
}
