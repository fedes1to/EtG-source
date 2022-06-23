using System.Collections;
using UnityEngine;

public class SpeedModificationBuff : AppliedEffectBase
{
	public float maximumSpeedModifier;

	public float lifespan;

	public float maxLifespan;

	private float elapsed;

	public static void ApplySpeedModificationToTarget(GameObject target, float maxSpeedMod, float lifetime, float maxLifetime)
	{
		if (!(target.GetComponent<SpeculativeRigidbody>() == null))
		{
			SpeedModificationBuff component = target.GetComponent<SpeedModificationBuff>();
			if (component != null)
			{
				component.ExtendLength(lifetime);
				return;
			}
			component = target.AddComponent<SpeedModificationBuff>();
			component.maximumSpeedModifier = maxSpeedMod;
			component.lifespan = lifetime;
			component.maxLifespan = maxLifetime;
		}
	}

	public override void AddSelfToTarget(GameObject target)
	{
		if (!(target.GetComponent<SpeculativeRigidbody>() == null))
		{
			SpeedModificationBuff component = target.GetComponent<SpeedModificationBuff>();
			if (component != null)
			{
				component.ExtendLength(lifespan);
				return;
			}
			SpeedModificationBuff speedModificationBuff = target.AddComponent<SpeedModificationBuff>();
			speedModificationBuff.Initialize(this);
		}
	}

	public override void Initialize(AppliedEffectBase source)
	{
		if (source is SpeedModificationBuff)
		{
			SpeedModificationBuff speedModificationBuff = source as SpeedModificationBuff;
			maximumSpeedModifier = speedModificationBuff.maximumSpeedModifier;
			lifespan = speedModificationBuff.lifespan;
			maxLifespan = speedModificationBuff.maxLifespan;
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void ExtendLength(float time)
	{
		lifespan = Mathf.Min(lifespan + time, elapsed + maxLifespan);
	}

	private IEnumerator ApplyModification()
	{
		elapsed = 0f;
		while (elapsed < lifespan)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		Object.Destroy(this);
	}
}
