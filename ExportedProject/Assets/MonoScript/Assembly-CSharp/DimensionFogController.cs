using System;
using UnityEngine;

public class DimensionFogController : BraveBehaviour
{
	private enum State
	{
		Growing,
		Contracting,
		Stable
	}

	public float radius;

	public float minRadius = 4f;

	public float growSpeed = 8f;

	public float contractSpeed = 1f;

	public float targetRadius;

	[Header("Main Particle System")]
	public new ParticleSystem particleSystem;

	public float emissionRatePerArea = 0.2f;

	public float speedPerRadius = 0.33f;

	[Header("Bits Particle System")]
	public ParticleSystem bitsParticleSystem;

	public float bitsEmissionRatePerRadius = 5f;

	private State m_state = State.Contracting;

	public float ApparentRadius
	{
		get
		{
			return (m_state != 0) ? radius : Mathf.Max(0f, radius - 6f);
		}
	}

	public void Start()
	{
		BraveUtility.EnableEmission(particleSystem, false);
		BraveUtility.EnableEmission(bitsParticleSystem, false);
	}

	public void Update()
	{
		if (m_state == State.Growing)
		{
			radius = Mathf.MoveTowards(radius, targetRadius, growSpeed * BraveTime.DeltaTime);
			if (radius >= targetRadius)
			{
				targetRadius = 0f;
				m_state = State.Contracting;
			}
		}
		else if (m_state == State.Contracting)
		{
			radius = Mathf.MoveTowards(radius, minRadius, contractSpeed * BraveTime.DeltaTime);
			if (radius <= minRadius)
			{
				radius = 0f;
				targetRadius = 0f;
				m_state = State.Stable;
			}
		}
		else if (m_state == State.Stable && targetRadius > 0f)
		{
			radius = minRadius;
			m_state = State.Growing;
		}
		UpdateParticleSystem();
		UpdateBitsParticleSystem();
	}

	private void UpdateParticleSystem()
	{
		float num = (float)Math.PI * radius * radius;
		float num2 = emissionRatePerArea * num;
		BraveUtility.SetEmissionRate(particleSystem, num2);
		particleSystem.startSpeed = speedPerRadius * radius;
		Vector3 position = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0, 360)) * new Vector3(radius, 0f);
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = position;
		emitParams.velocity = particleSystem.startSpeed * -position.normalized;
		emitParams.startSize = particleSystem.startSize;
		emitParams.startLifetime = particleSystem.startLifetime;
		emitParams.startColor = particleSystem.startColor;
		ParticleSystem.EmitParams emitParams2 = emitParams;
		particleSystem.Emit(emitParams2, (int)(BraveTime.DeltaTime * num2));
	}

	private void UpdateBitsParticleSystem()
	{
		if ((bool)bitsParticleSystem)
		{
			float num = bitsEmissionRatePerRadius * radius;
			BraveUtility.SetEmissionRate(bitsParticleSystem, num);
			Vector3 position = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0, 360)) * new Vector3(radius, 0f);
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = position;
			emitParams.velocity = bitsParticleSystem.startSpeed * position.normalized;
			emitParams.startSize = bitsParticleSystem.startSize;
			emitParams.startLifetime = bitsParticleSystem.startLifetime;
			emitParams.startColor = bitsParticleSystem.startColor;
			ParticleSystem.EmitParams emitParams2 = emitParams;
			particleSystem.Emit(emitParams2, (int)(BraveTime.DeltaTime * num));
		}
	}
}
