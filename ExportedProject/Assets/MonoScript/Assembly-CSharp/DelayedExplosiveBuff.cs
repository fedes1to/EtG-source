using System;
using System.Collections;
using UnityEngine;

public class DelayedExplosiveBuff : AppliedEffectBase
{
	public bool additionalInstancesRefreshDelay = true;

	public float delayBeforeBurst = 0.25f;

	public ExplosionData explosionData;

	public GameObject vfx;

	[NonSerialized]
	public bool IsSecondaryBuff;

	private float elapsed;

	private GameObject instantiatedVFX;

	private HealthHaver hh;

	private void InitializeSelf(float delayBefore, bool doRefresh, ExplosionData data)
	{
		explosionData = data;
		additionalInstancesRefreshDelay = doRefresh;
		delayBeforeBurst = delayBefore;
		hh = GetComponent<HealthHaver>();
		if (hh != null)
		{
			StartCoroutine(ApplyModification());
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public override void Initialize(AppliedEffectBase source)
	{
		if (source is DelayedExplosiveBuff)
		{
			DelayedExplosiveBuff delayedExplosiveBuff = source as DelayedExplosiveBuff;
			InitializeSelf(delayedExplosiveBuff.delayBeforeBurst, delayedExplosiveBuff.additionalInstancesRefreshDelay, delayedExplosiveBuff.explosionData);
			if (delayedExplosiveBuff.vfx != null)
			{
				instantiatedVFX = SpawnManager.SpawnVFX(delayedExplosiveBuff.vfx, base.transform.position, Quaternion.identity, true);
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
			UnityEngine.Object.Destroy(this);
		}
	}

	public void ExtendLength()
	{
		elapsed = 0f;
	}

	public override void AddSelfToTarget(GameObject target)
	{
		if (target.GetComponent<HealthHaver>() == null)
		{
			return;
		}
		bool isSecondaryBuff = false;
		if (additionalInstancesRefreshDelay)
		{
			DelayedExplosiveBuff[] components = target.GetComponents<DelayedExplosiveBuff>();
			for (int i = 0; i < components.Length; i++)
			{
				isSecondaryBuff = true;
				components[i].ExtendLength();
			}
		}
		DelayedExplosiveBuff delayedExplosiveBuff = target.AddComponent<DelayedExplosiveBuff>();
		delayedExplosiveBuff.IsSecondaryBuff = isSecondaryBuff;
		delayedExplosiveBuff.Initialize(this);
	}

	private IEnumerator ApplyModification()
	{
		elapsed = 0f;
		while (elapsed < delayBeforeBurst && (bool)hh && !hh.IsDead)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if ((bool)hh)
		{
			if (IsSecondaryBuff)
			{
				hh.ApplyDamage(explosionData.damage, Vector2.zero, string.Empty);
			}
			else
			{
				Exploder.Explode(hh.aiActor.CenterPosition, explosionData, Vector2.zero, null, true);
			}
		}
		if ((bool)instantiatedVFX)
		{
			BuffVFXAnimator component = instantiatedVFX.GetComponent<BuffVFXAnimator>();
			if (component != null && component.persistsOnDeath)
			{
				tk2dSpriteAnimator component2 = component.GetComponent<tk2dSpriteAnimator>();
				component2.Sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_COMPLEX;
				component2.PlayAndDestroyObject(string.Empty);
			}
			else
			{
				UnityEngine.Object.Destroy(instantiatedVFX);
			}
		}
		UnityEngine.Object.Destroy(this);
	}
}
