using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[ExecuteInEditMode]
public class HelicopterSnowController : MonoBehaviour
{
	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	public Vector3 WorldSpaceVortexCenter;

	public float VortexRadius = 5f;

	public float VortexSpeed = 5f;

	private AIActor m_helicopter;

	private void Start()
	{
		m_system = GetComponent<ParticleSystem>();
		if (m_particles == null)
		{
			m_particles = new ParticleSystem.Particle[m_system.main.maxParticles];
		}
	}

	private void OnEnable()
	{
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom == null)
		{
			return;
		}
		List<AIActor> activeEnemies = absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null || activeEnemies.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss)
			{
				m_helicopter = activeEnemies[i];
			}
		}
	}

	private void Update()
	{
		ProcessParticles();
	}

	private void ProcessParticles()
	{
		int particles = m_system.GetParticles(m_particles);
		if ((bool)m_helicopter)
		{
			WorldSpaceVortexCenter = m_helicopter.specRigidbody.UnitCenter + new Vector2(0f, 1.5f);
		}
		float num = VortexRadius * VortexRadius;
		if (!m_helicopter)
		{
			num = -1f;
		}
		for (int i = 0; i < particles; i++)
		{
			Vector3 position = m_particles[i].position;
			Vector3 worldSpaceVortexCenter = WorldSpaceVortexCenter;
			float num2 = position.x - worldSpaceVortexCenter.x;
			float num3 = position.y - worldSpaceVortexCenter.y;
			float num4 = num2 * num2 + num3 * num3;
			if (num4 < num)
			{
				float vortexSpeed = VortexSpeed;
				float x = (0f - num3) * vortexSpeed;
				float y = num2 * vortexSpeed;
				float num5 = 1f / (1f + num4 / num);
				Vector3 vector = new Vector3(x, y, 0f) * num5;
				m_particles[i].velocity += vector;
			}
		}
		m_system.SetParticles(m_particles, particles);
	}
}
