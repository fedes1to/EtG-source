using System;
using UnityEngine;

public class CollisionTriggerEvent : BraveBehaviour
{
	public bool onTriggerEnter;

	public bool onTriggerCollision;

	public bool onTriggerExit;

	public float delay;

	public string animationName;

	public bool destroyAfterAnimation;

	public VFXPool vfx;

	public Vector2 vfxOffset;

	private bool m_triggered;

	private float m_timer;

	public void Start()
	{
		if (onTriggerEnter)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnTrigger));
		}
		if (onTriggerCollision)
		{
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTrigger));
		}
		if (onTriggerExit)
		{
			SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
			speculativeRigidbody3.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody3.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(OnTriggerSimple));
		}
	}

	public void Update()
	{
		if (m_triggered)
		{
			m_timer -= BraveTime.DeltaTime;
			if (m_timer <= 0f)
			{
				DoEventStuff();
			}
		}
	}

	private void OnTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		OnTriggerSimple(specRigidbody, sourceSpecRigidbody);
	}

	private void OnTriggerSimple(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		if (delay <= 0f)
		{
			DoEventStuff();
			return;
		}
		m_triggered = true;
		m_timer = delay;
	}

	private void DoEventStuff()
	{
		if (!string.IsNullOrEmpty(animationName) && (bool)base.spriteAnimator)
		{
			base.spriteAnimator.Play(animationName);
			if (destroyAfterAnimation)
			{
				base.gameObject.AddComponent<SpriteAnimatorKiller>();
			}
		}
		vfx.SpawnAtLocalPosition(vfxOffset, 0f, base.transform, Vector2.zero, Vector2.zero);
		UnityEngine.Object.Destroy(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
