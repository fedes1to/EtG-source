using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
	public bool deparent;

	[ShowInInspectorIf("deparent", true)]
	public bool parentPosition = true;

	[ShowInInspectorIf("deparent", true)]
	public bool parentRotation = true;

	[ShowInInspectorIf("deparent", true)]
	public bool parentScale = true;

	[ShowInInspectorIf("deparent", true)]
	public bool parentPositionYDepth;

	public bool destroyAfterTimer;

	public float destroyTimer = 5f;

	public bool disableEmitterOnParentDeath;

	public bool destroyOnNoParticlesRemaining;

	public bool destroyAfterStartLifetime;

	public bool disableEmitterOnParentGrounded;

	public bool overrideXRotation;

	[ShowInInspectorIf("overrideXRotation", false)]
	public float xRotation;

	[Header("Manual Subemitter")]
	public bool transferToSubEmitter;

	public float transferToSubEmitterTimer;

	public ParticleSystem subEmitter;

	private ParticleSystem m_particleSystem;

	private DebrisObject m_debrisParent;

	private Transform m_parentTransform;

	private int m_noParticleCounter;

	private int c_framesOfNoParticlesToDestroy = 30;

	public void Awake()
	{
		if (!(base.transform.parent == null))
		{
			ForceInit();
		}
	}

	public void ForceInit()
	{
		m_particleSystem = GetComponent<ParticleSystem>();
		m_parentTransform = base.transform.parent;
		if ((bool)base.transform.parent)
		{
			DebrisObject component = base.transform.parent.GetComponent<DebrisObject>();
			if ((bool)component)
			{
				m_debrisParent = component;
				if (component.detachedParticleSystems == null)
				{
					component.detachedParticleSystems = new List<ParticleSystem>();
				}
				component.detachedParticleSystems.Add(m_particleSystem);
			}
		}
		if (destroyAfterStartLifetime)
		{
			StartCoroutine(TimedDespawn(m_particleSystem.startLifetime));
		}
		if (destroyAfterTimer)
		{
			StartCoroutine(TimedDespawn(destroyTimer));
		}
		if (deparent)
		{
			base.transform.parent = SpawnManager.Instance.VFX;
		}
		if (transferToSubEmitter)
		{
			StartCoroutine(TimedTransferToSubEmitter(transferToSubEmitterTimer));
		}
	}

	private void Start()
	{
		if ((bool)m_debrisParent && disableEmitterOnParentGrounded)
		{
			DebrisObject debrisParent = m_debrisParent;
			debrisParent.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisParent.OnGrounded, new Action<DebrisObject>(DisableOnParentGrounded));
		}
	}

	public void Update()
	{
		if (deparent)
		{
			if ((bool)m_parentTransform)
			{
				if (parentPosition)
				{
					if (parentPositionYDepth)
					{
						base.transform.position = m_parentTransform.position.WithZ(m_parentTransform.position.y);
					}
					else
					{
						base.transform.position = m_parentTransform.position;
					}
				}
				if (parentRotation)
				{
					base.transform.rotation = m_parentTransform.rotation;
				}
				if (parentScale)
				{
					base.transform.localScale = m_parentTransform.localScale;
				}
			}
			else
			{
				BraveUtility.EnableEmission(m_particleSystem, false);
			}
		}
		if (!destroyOnNoParticlesRemaining || !m_particleSystem)
		{
			return;
		}
		if (m_particleSystem.particleCount == 0)
		{
			m_noParticleCounter++;
			if (m_noParticleCounter >= c_framesOfNoParticlesToDestroy)
			{
				SpawnManager.Despawn(base.gameObject);
			}
		}
		else
		{
			m_noParticleCounter = 0;
		}
	}

	public void StopEmitting()
	{
		BraveUtility.EnableEmission(m_particleSystem, false);
	}

	private IEnumerator TimedDespawn(float t)
	{
		yield return new WaitForSeconds(t);
		SpawnManager.Despawn(base.gameObject);
	}

	private void DisableOnParentGrounded(DebrisObject obj)
	{
		BraveUtility.EnableEmission(m_particleSystem, false);
	}

	private IEnumerator TimedTransferToSubEmitter(float t)
	{
		yield return new WaitForSeconds(t);
		TransferToSubEmitter();
	}

	private void TransferToSubEmitter()
	{
		ParticleSystem.Particle[] array = new ParticleSystem.Particle[m_particleSystem.particleCount];
		int particles = m_particleSystem.GetParticles(array);
		for (int i = 0; i < particles; i++)
		{
			ParticleSystem.Particle particle = array[i];
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = particle.position;
			emitParams.rotation = particle.rotation;
			emitParams.startSize = particle.size;
			emitParams.startColor = particle.color;
			ParticleSystem.EmitParams emitParams2 = emitParams;
			subEmitter.Emit(emitParams2, 1);
		}
		m_particleSystem.Clear(false);
		m_particleSystem.Stop();
	}
}
