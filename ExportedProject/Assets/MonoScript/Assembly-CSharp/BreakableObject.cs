using System;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
	public string breakAnimName = string.Empty;

	private SpeculativeRigidbody m_srb;

	private void Start()
	{
		m_srb = GetComponent<SpeculativeRigidbody>();
		SpeculativeRigidbody srb = m_srb;
		srb.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(srb.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
	}

	private void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		Break();
	}

	private void Break()
	{
		tk2dSpriteAnimator component = GetComponent<tk2dSpriteAnimator>();
		if (breakAnimName != string.Empty)
		{
			component.Play(breakAnimName);
		}
		else
		{
			component.Play();
		}
		UnityEngine.Object.Destroy(m_srb);
	}
}
