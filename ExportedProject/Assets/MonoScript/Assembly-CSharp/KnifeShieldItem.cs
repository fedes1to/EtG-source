using UnityEngine;

public class KnifeShieldItem : PlayerItem
{
	[Header("Knife Properties")]
	public int numKnives = 5;

	public float knifeHealth = 0.5f;

	public float knifeDamage = 5f;

	public float circleRadius = 3f;

	public float rotationDegreesPerSecond = 360f;

	[Header("Thrown Properties")]
	public float throwSpeed = 10f;

	public float throwRange = 25f;

	public float throwRadius = 3f;

	public float radiusChangeDistance = 3f;

	public GameObject knifePrefab;

	public GameObject knifeDeathVFX;

	protected KnifeShieldEffect m_extantEffect;

	protected KnifeShieldEffect m_secondaryEffect;

	protected override void DoEffect(PlayerController user)
	{
		m_extantEffect = CreateEffect(user);
		if (user.HasActiveBonusSynergy(CustomSynergyType.TWO_BLADES))
		{
			m_secondaryEffect = CreateEffect(user, 1.25f, -1f);
		}
		AkSoundEngine.PostEvent("Play_OBJ_daggershield_start_01", base.gameObject);
	}

	private KnifeShieldEffect CreateEffect(PlayerController user, float radiusMultiplier = 1f, float rotationSpeedMultiplier = 1f)
	{
		GameObject gameObject = new GameObject("knife shield effect");
		gameObject.transform.position = user.LockedApproximateSpriteCenter;
		gameObject.transform.parent = user.transform;
		KnifeShieldEffect knifeShieldEffect = gameObject.AddComponent<KnifeShieldEffect>();
		knifeShieldEffect.numKnives = numKnives;
		knifeShieldEffect.remainingHealth = knifeHealth;
		knifeShieldEffect.knifeDamage = knifeDamage;
		knifeShieldEffect.circleRadius = circleRadius * radiusMultiplier;
		knifeShieldEffect.rotationDegreesPerSecond = rotationDegreesPerSecond * rotationSpeedMultiplier;
		knifeShieldEffect.throwSpeed = throwSpeed;
		knifeShieldEffect.throwRange = throwRange;
		knifeShieldEffect.throwRadius = throwRadius;
		knifeShieldEffect.radiusChangeDistance = radiusChangeDistance;
		knifeShieldEffect.deathVFX = knifeDeathVFX;
		knifeShieldEffect.Initialize(user, knifePrefab);
		return knifeShieldEffect;
	}

	public override void Update()
	{
		base.Update();
		if (m_extantEffect != null && !m_extantEffect.IsActive)
		{
			m_extantEffect = null;
		}
		if (m_secondaryEffect != null && !m_secondaryEffect.IsActive)
		{
			m_secondaryEffect = null;
		}
	}

	protected override void DoOnCooldownEffect(PlayerController user)
	{
		if (m_extantEffect != null && m_extantEffect.IsActive)
		{
			m_extantEffect.ThrowShield();
		}
		if (m_secondaryEffect != null && m_secondaryEffect.IsActive)
		{
			m_secondaryEffect.ThrowShield();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
