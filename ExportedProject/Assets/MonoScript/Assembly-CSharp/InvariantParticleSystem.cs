using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class InvariantParticleSystem : BraveBehaviour
{
	private ParticleSystem m_particleSystem;

	public void Awake()
	{
		m_particleSystem = GetComponent<ParticleSystem>();
	}

	public void Update()
	{
		m_particleSystem.Simulate(GameManager.INVARIANT_DELTA_TIME, true, false);
	}
}
