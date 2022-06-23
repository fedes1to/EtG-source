using Dungeonator;
using UnityEngine;

public class CrowdOfFansSystemController : MonoBehaviour
{
	public PlayerController Target;

	public int MaxFans = 100;

	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	private Vector2[] m_offsets;

	private bool m_initialized;

	private int m_numEmitted;

	private void Start()
	{
		m_system = GetComponent<ParticleSystem>();
		if (m_particles == null)
		{
			m_particles = new ParticleSystem.Particle[m_system.maxParticles];
		}
		m_offsets = new Vector2[MaxFans];
		for (int i = 0; i < MaxFans; i++)
		{
			m_offsets[i] = Random.insideUnitCircle * 3f;
		}
		m_system.Play();
	}

	public void Initialize(PlayerController p)
	{
		m_initialized = true;
		Target = p;
	}

	private void Update()
	{
		if (!Dungeon.IsGenerating && m_initialized)
		{
			ProcessParticles();
		}
	}

	private void ProcessParticles()
	{
		int num = 10;
		if (m_numEmitted < num)
		{
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = (Target.CenterPosition + m_offsets[m_numEmitted]).ToVector3ZisY();
			emitParams.velocity = Vector3.zero;
			emitParams.startSize = m_system.startSize;
			emitParams.startLifetime = m_system.startLifetime;
			emitParams.startColor = m_system.startColor;
			m_system.Emit(emitParams, 1);
			Debug.LogError("emitting particle");
			m_numEmitted++;
		}
		int particles = m_system.GetParticles(m_particles);
		for (int i = 0; i < particles; i++)
		{
			Vector3 position = m_particles[i].position;
			Vector3 velocity = m_particles[i].velocity;
			Vector3 position2 = (Target.CenterPosition + m_offsets[i]).ToVector3ZisY();
			m_particles[i].position = position2;
			m_particles[i].velocity = Vector3.zero;
		}
		m_system.SetParticles(m_particles, particles);
	}
}
