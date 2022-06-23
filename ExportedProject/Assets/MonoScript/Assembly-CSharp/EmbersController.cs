using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EmbersController : MonoBehaviour
{
	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	[NonSerialized]
	public List<Vector4> AdditionalVortices = new List<Vector4>();

	public float VortexScale = 1.5f;

	public float VortexSpeed = 2f;

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

	private void ProcessVortex(int particleIndex, Vector2 particlePos, Vector2 vortex, float vortexScale, float speed)
	{
		float num = particlePos.x - vortex.x;
		float x = particlePos.y - vortex.y;
		Vector2 vector = new Vector2(x, 0f - num);
		float num2 = Mathf.Clamp01(1f - vector.magnitude / vortexScale);
		Vector3 vector2 = vector.normalized.ToVector3ZUp() * num2 * speed;
		m_particles[particleIndex].velocity += vector2;
	}

	private void ProcessParticles()
	{
		int particles = m_system.GetParticles(m_particles);
		float vortexScale = VortexScale;
		for (int i = 0; i < particles; i++)
		{
			Vector3 position = m_particles[i].position;
			Vector2 vector = position.XY().Quantize(2f);
			float num = position.x - vector.x;
			float num2 = position.y - vector.y;
			float num3 = Mathf.Sin(position.x + position.y);
			float num4 = vortexScale * Mathf.Lerp(0.75f, 1.75f, (Mathf.Cos(position.x + position.y) + 1f) / 2f);
			float num5 = VortexSpeed * Mathf.Lerp(0.75f, 1.75f, (num3 + 1f) / 2f);
			if (num3 > 0f)
			{
				num5 *= -1f;
			}
			float num6 = (0f - num2) * num5;
			float num7 = num * num5;
			float num8 = 1f / (1f + (num * num + num2 * num2) / num4);
			Vector3 vector2 = new Vector3(num6 - m_particles[i].velocity.x, num7 - m_particles[i].velocity.y, 0f) * num8;
			m_particles[i].velocity += vector2;
			if (AdditionalVortices.Count != 0)
			{
				for (int j = 0; j < AdditionalVortices.Count; j++)
				{
					ProcessVortex(i, position, new Vector2(AdditionalVortices[j].x, AdditionalVortices[j].y), AdditionalVortices[j].z, AdditionalVortices[j].w);
				}
			}
		}
		m_system.SetParticles(m_particles, particles);
	}
}
