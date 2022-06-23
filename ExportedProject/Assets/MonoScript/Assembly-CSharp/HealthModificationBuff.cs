using System.Collections;
using UnityEngine;

public class HealthModificationBuff : AppliedEffectBase
{
	public enum HealthModificationType
	{
		BLEED,
		POISON,
		REGEN,
		UNIQUE
	}

	public HealthModificationType type;

	public bool supportsMultipleInstances;

	[Tooltip("Time between damage or healing ticks.")]
	public float tickPeriod;

	[Tooltip("How long each application lasts.")]
	public float lifetime;

	[Tooltip("Damage or healing at start of duration.")]
	public float healthChangeAtStart;

	[Tooltip("Damage or healing at end of duration.")]
	public float healthChangeAtEnd;

	[Tooltip("The maximum length of time this debuff can be extended to by repeat applications.")]
	public float maxLifetime;

	public GameObject vfx;

	public float ChanceToApplyVFX = 1f;

	private float elapsed;

	private GameObject instantiatedVFX;

	private HealthHaver hh;

	private bool wasDuplicate;

	private void InitializeSelf(float startChange, float endChange, float length, float period, float maxLength)
	{
		hh = GetComponent<HealthHaver>();
		healthChangeAtStart = startChange;
		healthChangeAtEnd = endChange;
		tickPeriod = period;
		lifetime = length;
		maxLifetime = maxLength;
		if (hh != null)
		{
			StartCoroutine(ApplyModification());
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public override void Initialize(AppliedEffectBase source)
	{
		if (source is HealthModificationBuff)
		{
			HealthModificationBuff healthModificationBuff = source as HealthModificationBuff;
			InitializeSelf(healthModificationBuff.healthChangeAtStart, healthModificationBuff.healthChangeAtEnd, healthModificationBuff.lifetime, healthModificationBuff.tickPeriod, healthModificationBuff.maxLifetime);
			type = healthModificationBuff.type;
			if (!(healthModificationBuff.vfx != null))
			{
				return;
			}
			bool flag = true;
			if (wasDuplicate && ChanceToApplyVFX < 1f && Random.value > ChanceToApplyVFX)
			{
				flag = false;
			}
			if (flag)
			{
				instantiatedVFX = SpawnManager.SpawnVFX(healthModificationBuff.vfx, base.transform.position, Quaternion.identity);
				tk2dSprite component = instantiatedVFX.GetComponent<tk2dSprite>();
				tk2dSprite component2 = GetComponent<tk2dSprite>();
				if (component != null && component2 != null)
				{
					component2.AttachRenderer(component);
					component.HeightOffGround = 0.1f;
					component.IsPerpendicular = true;
					component.usesOverrideMaterial = true;
				}
				BuffVFXAnimator component3 = instantiatedVFX.GetComponent<BuffVFXAnimator>();
				if (component3 != null)
				{
					component3.Initialize(GetComponent<GameActor>());
				}
			}
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void ExtendLength(float time)
	{
		lifetime = Mathf.Min(lifetime + time, elapsed + maxLifetime);
	}

	public override void AddSelfToTarget(GameObject target)
	{
		if (target.GetComponent<HealthHaver>() == null)
		{
			return;
		}
		bool flag = false;
		HealthModificationBuff[] components = target.GetComponents<HealthModificationBuff>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].type == type)
			{
				if (!supportsMultipleInstances)
				{
					components[i].ExtendLength(lifetime);
					return;
				}
				flag = true;
			}
		}
		HealthModificationBuff healthModificationBuff = target.AddComponent<HealthModificationBuff>();
		healthModificationBuff.wasDuplicate = flag;
		healthModificationBuff.Initialize(this);
	}

	private IEnumerator ApplyModification()
	{
		elapsed = 0f;
		while (elapsed < lifetime && (bool)hh && !hh.IsDead)
		{
			elapsed += tickPeriod;
			float changeThisTick = Mathf.Lerp(healthChangeAtStart, healthChangeAtEnd, elapsed / lifetime);
			if (changeThisTick < 0f)
			{
				hh.ApplyDamage(-1f * changeThisTick, Vector2.zero, base.name, CoreDamageTypes.None, DamageCategory.DamageOverTime);
			}
			else
			{
				hh.ApplyHealing(changeThisTick);
			}
			yield return new WaitForSeconds(tickPeriod);
		}
		if ((bool)instantiatedVFX)
		{
			BuffVFXAnimator component = instantiatedVFX.GetComponent<BuffVFXAnimator>();
			if (component != null && component.persistsOnDeath)
			{
				tk2dSpriteAnimator component2 = component.GetComponent<tk2dSpriteAnimator>();
				if (component2 != null)
				{
					component2.Stop();
				}
				instantiatedVFX.GetComponent<PersistentVFXBehaviour>().BecomeDebris(Vector3.zero, 0.5f);
			}
			else
			{
				Object.Destroy(instantiatedVFX);
			}
		}
		Object.Destroy(this);
	}
}
