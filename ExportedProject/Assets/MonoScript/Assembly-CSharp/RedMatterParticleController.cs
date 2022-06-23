using UnityEngine;

[ExecuteInEditMode]
public class RedMatterParticleController : MonoBehaviour
{
	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	public Transform target;

	public float VortexScale = 1.5f;

	public float VortexSpeed = 2f;

	private void Awake()
	{
		m_system = GetComponent<ParticleSystem>();
		if (m_particles == null)
		{
			m_particles = new ParticleSystem.Particle[m_system.maxParticles];
		}
	}

	public void ProcessParticles()
	{
		int particles = m_system.GetParticles(m_particles);
		float vortexScale = VortexScale;
		for (int i = 0; i < particles; i++)
		{
			Vector3 position = m_particles[i].position;
			float t = Mathf.Lerp(0f, 1f, 1f - (m_particles[i].remainingLifetime - (m_particles[i].startLifetime - 1f)));
			float t2 = 1f - (m_particles[i].remainingLifetime - 0.5f) / m_particles[i].startLifetime;
			Vector3 b = ((!(target == null)) ? ((target.position - position).normalized * Mathf.Lerp(m_particles[i].velocity.magnitude, (target.position - position).magnitude * 10f, t2)) : m_particles[i].velocity);
			m_particles[i].velocity = Vector3.Lerp(m_particles[i].velocity, b, t);
			if ((target.position - position).sqrMagnitude <= 1f)
			{
				m_particles[i].remainingLifetime = m_particles[i].remainingLifetime - 0.1f;
			}
		}
		m_system.SetParticles(m_particles, particles);
	}
}
