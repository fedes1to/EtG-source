using UnityEngine;

[ExecuteInEditMode]
public class BulletCurtainParticleController : MonoBehaviour
{
	public float LocalXMin = 3.5f;

	public float LocalXMax = 4.5f;

	public float LocalYMax = 2.5f;

	public float AccelFactor = 1f;

	public Transform rootTransform;

	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	private void Start()
	{
		m_system = GetComponent<ParticleSystem>();
		if (m_particles == null)
		{
			m_particles = new ParticleSystem.Particle[m_system.maxParticles];
		}
	}

	private void Update()
	{
		ProcessParticles();
	}

	private void ProcessParticles()
	{
		int particles = m_system.GetParticles(m_particles);
		Transform transform = m_system.transform;
		float num = 0f;
		if (Application.isPlaying)
		{
			num = BraveTime.DeltaTime;
		}
		for (int i = 0; i < particles; i++)
		{
			Vector3 vector = transform.TransformPoint(m_particles[i].position);
			vector -= rootTransform.position;
			float num2 = (float)(m_particles[i].randomSeed % 30u) / 30f * 0.5f;
			if ((vector.x < LocalXMin - num2 || vector.x > LocalXMax + num2) && vector.y > 1.25f)
			{
				m_particles[i].velocity = m_particles[i].velocity.WithX(0f);
			}
			else if (vector.x > LocalXMin && vector.x < LocalXMax && vector.y < LocalYMax)
			{
				if (vector.x > (LocalXMin + LocalXMax) / 2f)
				{
					m_particles[i].velocity += Vector3.right * num * AccelFactor;
				}
				else
				{
					m_particles[i].velocity += Vector3.right * -1f * num * AccelFactor;
				}
			}
		}
		m_system.SetParticles(m_particles, particles);
	}
}
