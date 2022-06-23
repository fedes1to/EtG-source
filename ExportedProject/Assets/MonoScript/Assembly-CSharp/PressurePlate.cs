using System;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : BraveBehaviour
{
	public bool PlayersCanTrigger = true;

	public bool EnemiesCanTrigger;

	public bool ArbitraryObjectsCanTrigger;

	public bool CanUnpress = true;

	public string depressAnimationName = string.Empty;

	public string unpressAnimationName = string.Empty;

	public Action<PressurePlate> OnPressurePlateDepressed;

	public Action<PressurePlate> OnPressurePlateUnpressed;

	private HashSet<GameObject> m_currentDepressors;

	private List<PlayerController> m_queuedDepressors = new List<PlayerController>();

	private bool m_pressed;

	private void Start()
	{
		m_currentDepressors = new HashSet<GameObject>();
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleEnterTriggerCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody2.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(HandleExitTriggerCollision));
	}

	private void HandleEnterTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		int count = m_currentDepressors.Count;
		if (PlayersCanTrigger)
		{
			PlayerController component = specRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				if (component.IsDodgeRolling && !component.IsGrounded && !component.IsFlying && GameManager.Instance.IsFoyer)
				{
					m_queuedDepressors.Add(component);
					return;
				}
				m_currentDepressors.Add(specRigidbody.gameObject);
				if (!m_pressed)
				{
					AkSoundEngine.PostEvent("Play_OBJ_plate_press_01", base.gameObject);
				}
			}
		}
		if (EnemiesCanTrigger && specRigidbody.GetComponent<AIActor>() != null)
		{
			m_currentDepressors.Add(specRigidbody.gameObject);
		}
		if (ArbitraryObjectsCanTrigger)
		{
			m_currentDepressors.Add(specRigidbody.gameObject);
		}
		int count2 = m_currentDepressors.Count;
		m_pressed = true;
		if (count == 0 && count2 > 0)
		{
			if (!string.IsNullOrEmpty(depressAnimationName))
			{
				base.spriteAnimator.Play(depressAnimationName);
			}
			if (OnPressurePlateDepressed != null)
			{
				OnPressurePlateDepressed(this);
			}
		}
	}

	private void Update()
	{
		if (m_queuedDepressors.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_queuedDepressors.Count; i++)
		{
			if (!m_queuedDepressors[i].IsDodgeRolling || m_queuedDepressors[i].IsGrounded)
			{
				HandleEnterTriggerCollision(m_queuedDepressors[i].specRigidbody, base.specRigidbody, null);
				m_queuedDepressors.RemoveAt(i);
				i--;
			}
		}
	}

	private void HandleExitTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		if (!CanUnpress)
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if ((bool)component && m_queuedDepressors.Contains(component))
		{
			m_queuedDepressors.Remove(component);
			return;
		}
		int count = m_currentDepressors.Count;
		if (m_currentDepressors.Contains(specRigidbody.gameObject))
		{
			m_currentDepressors.Remove(specRigidbody.gameObject);
		}
		int count2 = m_currentDepressors.Count;
		if (count > 0 && count2 == 0)
		{
			m_pressed = false;
			if (!string.IsNullOrEmpty(unpressAnimationName))
			{
				base.spriteAnimator.Play(unpressAnimationName);
			}
			if (OnPressurePlateUnpressed != null)
			{
				OnPressurePlateUnpressed(this);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
