using System.Collections;
using UnityEngine;

public static class GlobalSparksDoer
{
	public enum EmitRegionStyle
	{
		RANDOM,
		RADIAL
	}

	public enum SparksType
	{
		SPARKS_ADDITIVE_DEFAULT,
		BLACK_PHANTOM_SMOKE,
		FLOATY_CHAFF,
		SOLID_SPARKLES,
		EMBERS_SWIRLING,
		STRAIGHT_UP_FIRE,
		DARK_MAGICKS,
		BLOODY_BLOOD,
		STRAIGHT_UP_GREEN_FIRE,
		RED_MATTER
	}

	private static ParticleSystem m_particles;

	private static ParticleSystem m_phantomParticles;

	private static ParticleSystem m_chaffParticles;

	private static ParticleSystem m_sparkleParticles;

	private static ParticleSystem m_fireParticles;

	private static ParticleSystem m_darkMagicParticles;

	public static ParticleSystem EmberParticles;

	private static ParticleSystem m_bloodParticles;

	private static ParticleSystem m_greenFireParticles;

	private static ParticleSystem m_redMatterParticles;

	public static RedMatterParticleController GetRedMatterController()
	{
		return (!m_redMatterParticles) ? null : m_redMatterParticles.GetComponent<RedMatterParticleController>();
	}

	public static EmbersController GetEmbersController()
	{
		if (EmberParticles == null)
		{
			InitializeParticles(SparksType.EMBERS_SWIRLING);
		}
		return EmberParticles.GetComponent<EmbersController>();
	}

	public static void DoSingleParticle(Vector3 position, Vector3 direction, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
	{
		ParticleSystem particleSystem = m_particles;
		switch (systemType)
		{
		case SparksType.SPARKS_ADDITIVE_DEFAULT:
			particleSystem = m_particles;
			break;
		case SparksType.BLACK_PHANTOM_SMOKE:
			particleSystem = m_phantomParticles;
			break;
		case SparksType.FLOATY_CHAFF:
			particleSystem = m_chaffParticles;
			break;
		case SparksType.SOLID_SPARKLES:
			particleSystem = m_sparkleParticles;
			break;
		case SparksType.EMBERS_SWIRLING:
			particleSystem = EmberParticles;
			break;
		case SparksType.STRAIGHT_UP_FIRE:
			particleSystem = m_fireParticles;
			break;
		case SparksType.DARK_MAGICKS:
			particleSystem = m_darkMagicParticles;
			break;
		case SparksType.BLOODY_BLOOD:
			particleSystem = m_bloodParticles;
			break;
		case SparksType.STRAIGHT_UP_GREEN_FIRE:
			particleSystem = m_greenFireParticles;
			break;
		case SparksType.RED_MATTER:
			particleSystem = m_redMatterParticles;
			break;
		}
		if (particleSystem == null)
		{
			particleSystem = InitializeParticles(systemType);
		}
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.position = position;
		emitParams.velocity = direction;
		emitParams.startSize = ((!startSize.HasValue) ? particleSystem.startSize : startSize.Value);
		emitParams.startLifetime = ((!startLifetime.HasValue) ? particleSystem.startLifetime : startLifetime.Value);
		emitParams.startColor = ((!startColor.HasValue) ? particleSystem.startColor : startColor.Value);
		emitParams.randomSeed = (uint)Random.Range(1, 1000);
		ParticleSystem.EmitParams emitParams2 = emitParams;
		particleSystem.Emit(emitParams2, 1);
	}

	public static void DoRandomParticleBurst(int num, Vector3 minPosition, Vector3 maxPosition, Vector3 direction, float angleVariance, float magnitudeVariance, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
	{
		for (int i = 0; i < num; i++)
		{
			Vector3 position = new Vector3(Random.Range(minPosition.x, maxPosition.x), Random.Range(minPosition.y, maxPosition.y), Random.Range(minPosition.z, maxPosition.z));
			Vector3 direction2 = Quaternion.Euler(0f, 0f, Random.Range(0f - angleVariance, angleVariance)) * (direction.normalized * Random.Range(direction.magnitude - magnitudeVariance, direction.magnitude + magnitudeVariance));
			DoSingleParticle(position, direction2, startSize, startLifetime, startColor, systemType);
		}
	}

	public static void DoLinearParticleBurst(int num, Vector3 minPosition, Vector3 maxPosition, float angleVariance, float baseMagnitude, float magnitudeVariance, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
	{
		for (int i = 0; i < num; i++)
		{
			Vector3 position = Vector3.Lerp(minPosition, maxPosition, ((float)i + 1f) / (float)num);
			Vector3 vector = Random.insideUnitCircle.normalized.ToVector3ZUp();
			Vector3 direction = Quaternion.Euler(0f, 0f, Random.Range(0f - angleVariance, angleVariance)) * (vector.normalized * Random.Range(baseMagnitude - magnitudeVariance, vector.magnitude + magnitudeVariance));
			DoSingleParticle(position, direction, startSize, startLifetime, startColor, systemType);
		}
	}

	public static void DoRadialParticleBurst(int num, Vector3 minPosition, Vector3 maxPosition, float angleVariance, float baseMagnitude, float magnitudeVariance, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
	{
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = new Vector3(Random.Range(minPosition.x, maxPosition.x), Random.Range(minPosition.y, maxPosition.y), Random.Range(minPosition.z, maxPosition.z));
			Vector3 vector2 = vector - (maxPosition + minPosition) / 2f;
			Vector3 direction = Quaternion.Euler(0f, 0f, Random.Range(0f - angleVariance, angleVariance)) * (vector2.normalized * Random.Range(baseMagnitude - magnitudeVariance, vector2.magnitude + magnitudeVariance));
			DoSingleParticle(vector, direction, startSize, startLifetime, startColor, systemType);
		}
	}

	public static void EmitFromRegion(EmitRegionStyle emitStyle, float numPerSecond, float duration, Vector3 minPosition, Vector3 maxPosition, Vector3 direction, float angleVariance, float magnitudeVariance, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
	{
		GameUIRoot.Instance.StartCoroutine(HandleEmitFromRegion(emitStyle, numPerSecond, duration, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, startSize, startLifetime, startColor, systemType));
	}

	private static IEnumerator HandleEmitFromRegion(EmitRegionStyle emitStyle, float numPerSecond, float duration, Vector3 minPosition, Vector3 maxPosition, Vector3 direction, float angleVariance, float magnitudeVariance, float? startSize = null, float? startLifetime = null, Color? startColor = null, SparksType systemType = SparksType.SPARKS_ADDITIVE_DEFAULT)
	{
		float elapsed = 0f;
		float numReqToSpawn2 = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			numReqToSpawn2 += numPerSecond * BraveTime.DeltaTime;
			if (numReqToSpawn2 > 1f)
			{
				int num = Mathf.FloorToInt(numReqToSpawn2);
				switch (emitStyle)
				{
				case EmitRegionStyle.RANDOM:
					DoRandomParticleBurst(num, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, startSize, startLifetime, startColor, systemType);
					break;
				case EmitRegionStyle.RADIAL:
					DoRadialParticleBurst(num, minPosition, maxPosition, angleVariance, direction.magnitude, magnitudeVariance, startSize, startLifetime, startColor, systemType);
					break;
				}
			}
			numReqToSpawn2 %= 1f;
			yield return null;
		}
	}

	private static ParticleSystem InitializeParticles(SparksType targetType)
	{
		switch (targetType)
		{
		case SparksType.SPARKS_ADDITIVE_DEFAULT:
			m_particles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/SparkSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_particles;
		case SparksType.BLACK_PHANTOM_SMOKE:
			m_phantomParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/PhantomSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_phantomParticles;
		case SparksType.FLOATY_CHAFF:
			m_chaffParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/ChaffSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_chaffParticles;
		case SparksType.SOLID_SPARKLES:
			m_sparkleParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/SparklesSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_sparkleParticles;
		case SparksType.EMBERS_SWIRLING:
			EmberParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/EmberSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return EmberParticles;
		case SparksType.STRAIGHT_UP_FIRE:
			m_fireParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/GlobalFireSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_fireParticles;
		case SparksType.DARK_MAGICKS:
			m_darkMagicParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/DarkMagicSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_darkMagicParticles;
		case SparksType.BLOODY_BLOOD:
			m_bloodParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/BloodSystem"), Vector3.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_bloodParticles;
		case SparksType.STRAIGHT_UP_GREEN_FIRE:
			m_greenFireParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/GlobalGreenFireSystem"), Vector2.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_greenFireParticles;
		case SparksType.RED_MATTER:
			m_redMatterParticles = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/GlobalRedMatterSystem"), Vector2.zero, Quaternion.identity)).GetComponent<ParticleSystem>();
			return m_redMatterParticles;
		default:
			return m_particles;
		}
	}

	public static void CleanupOnSceneTransition()
	{
		m_particles = null;
		m_phantomParticles = null;
		m_chaffParticles = null;
		m_sparkleParticles = null;
		m_fireParticles = null;
		m_darkMagicParticles = null;
		m_bloodParticles = null;
		EmberParticles = null;
		m_greenFireParticles = null;
		m_redMatterParticles = null;
	}
}
