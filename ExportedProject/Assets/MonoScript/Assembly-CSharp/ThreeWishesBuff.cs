using System.Collections;
using UnityEngine;

public class ThreeWishesBuff : AppliedEffectBase
{
	public bool SynergyContingent;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool OnlyOncePerEnemy;

	public bool TriggersOnFrozenEnemy;

	public int NumRequired = 3;

	public float DamageDelay = 0.4f;

	public float DamageDealt = 50f;

	public Vector3 DirectionalVFXOffset = new Vector3(3f, 0f, 0f);

	public bool doesExplosion;

	public ExplosionData explosionData;

	public GameObject OverheadVFX;

	public GameObject FinalVFX_Right;

	public GameObject FinalVFX_Left;

	public GameObject FinalVFX_Shared;

	private int m_extantCount = 1;

	private GameObject instantiatedVFX;

	private GameObject instantiatedVFX2;

	private HealthHaver hh;

	public override void Initialize(AppliedEffectBase source)
	{
		hh = GetComponent<HealthHaver>();
		m_extantCount = 1;
		if (source is ThreeWishesBuff)
		{
			ThreeWishesBuff threeWishesBuff = source as ThreeWishesBuff;
			NumRequired = threeWishesBuff.NumRequired;
			TriggersOnFrozenEnemy = threeWishesBuff.TriggersOnFrozenEnemy;
			OnlyOncePerEnemy = threeWishesBuff.OnlyOncePerEnemy;
			if (threeWishesBuff.OverheadVFX != null)
			{
				GameActor component = GetComponent<GameActor>();
				if ((bool)component && (bool)component.specRigidbody && component.specRigidbody.HitboxPixelCollider != null)
				{
					instantiatedVFX = component.PlayEffectOnActor(threeWishesBuff.OverheadVFX, new Vector3(0f, component.specRigidbody.HitboxPixelCollider.UnitDimensions.y, 0f), true, false, true);
				}
			}
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void Increment(ThreeWishesBuff source)
	{
		m_extantCount++;
		DamageDelay = source.DamageDelay;
		DirectionalVFXOffset = source.DirectionalVFXOffset;
		bool flag = m_extantCount == NumRequired;
		if (TriggersOnFrozenEnemy)
		{
			flag = hh.gameActor.IsFrozen && m_extantCount > 0;
		}
		if (flag)
		{
			if ((bool)instantiatedVFX)
			{
				Object.Destroy(instantiatedVFX);
			}
			if ((bool)instantiatedVFX2)
			{
				Object.Destroy(instantiatedVFX2);
			}
			instantiatedVFX2 = null;
			instantiatedVFX = null;
			if (source.transform.position.x < hh.gameActor.CenterPosition.x)
			{
				GameObject gameObject = hh.gameActor.PlayEffectOnActor(source.FinalVFX_Right, DirectionalVFXOffset.WithX(DirectionalVFXOffset.x * -1f));
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				component.HeightOffGround = 3f;
				component.UpdateZDepth();
				GameManager.Instance.StartCoroutine(DelayedDamage(source.DamageDealt, gameObject, source.FinalVFX_Shared));
			}
			else
			{
				GameObject gameObject2 = hh.gameActor.PlayEffectOnActor(source.FinalVFX_Left, DirectionalVFXOffset);
				tk2dBaseSprite component2 = gameObject2.GetComponent<tk2dBaseSprite>();
				component2.HeightOffGround = 3f;
				component2.UpdateZDepth();
				GameManager.Instance.StartCoroutine(DelayedDamage(source.DamageDealt, gameObject2, source.FinalVFX_Shared));
			}
			if (source.doesExplosion)
			{
				Exploder.Explode(hh.gameActor.CenterPosition, source.explosionData, Vector2.zero);
			}
			if (!OnlyOncePerEnemy)
			{
				Object.Destroy(this);
			}
			else
			{
				m_extantCount = -1000000;
			}
		}
		else if (source.OverheadVFX != null)
		{
			GameActor component3 = GetComponent<GameActor>();
			if ((bool)component3 && (bool)component3.specRigidbody && component3.specRigidbody.HitboxPixelCollider != null)
			{
				instantiatedVFX2 = component3.PlayEffectOnActor(source.OverheadVFX, new Vector3(0f, component3.specRigidbody.HitboxPixelCollider.UnitDimensions.y + 0.5f, 0f), true, false, true);
			}
		}
	}

	private IEnumerator DelayedDamage(float source, GameObject vfx, GameObject finalVfx)
	{
		float ela = 0f;
		while (ela < DamageDelay)
		{
			ela += BraveTime.DeltaTime;
			if (!hh || hh.IsDead)
			{
				if ((bool)vfx)
				{
					Object.Destroy(vfx);
				}
				yield break;
			}
			yield return null;
		}
		if ((bool)hh)
		{
			hh.ApplyDamage(source, Vector2.zero, "Wish");
			if ((bool)hh.gameActor && (bool)finalVfx)
			{
				hh.gameActor.PlayEffectOnActor(finalVfx, Vector3.zero, false);
			}
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(0.1f, 0.1f, false);
			if ((bool)vfx)
			{
				vfx.transform.parent = null;
				GameManager.Instance.StartCoroutine(TimedDestroy(vfx, 2f));
			}
			if ((bool)hh.knockbackDoer)
			{
				hh.knockbackDoer.ApplyKnockback(Vector2.up, (!(hh.GetCurrentHealth() <= 0f)) ? 30 : 100);
			}
		}
		else if ((bool)vfx)
		{
			Object.Destroy(vfx);
		}
	}

	private IEnumerator TimedDestroy(GameObject target, float delay)
	{
		float ela = 0f;
		tk2dSpriteAnimator anim = target.GetComponent<tk2dSpriteAnimator>();
		SpriteAnimatorKiller killer = target.GetComponent<SpriteAnimatorKiller>();
		while (ela < delay)
		{
			ela += BraveTime.DeltaTime;
			if ((bool)anim && !anim.enabled)
			{
				anim.enabled = true;
			}
			if ((bool)killer && !killer.enabled)
			{
				killer.enabled = true;
			}
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		Object.Destroy(target);
	}

	public override void AddSelfToTarget(GameObject target)
	{
		if (SynergyContingent)
		{
			Projectile component = GetComponent<Projectile>();
			if ((bool)component && (bool)component.PossibleSourceGun && !component.PossibleSourceGun.OwnerHasSynergy(RequiredSynergy))
			{
				return;
			}
		}
		HealthHaver healthHaver = target.GetComponent<HealthHaver>();
		if (!healthHaver)
		{
			SpeculativeRigidbody component2 = target.GetComponent<SpeculativeRigidbody>();
			if ((bool)component2)
			{
				healthHaver = component2.healthHaver;
				if ((bool)healthHaver)
				{
					target = healthHaver.gameObject;
				}
			}
		}
		if ((bool)healthHaver)
		{
			ThreeWishesBuff[] components = target.GetComponents<ThreeWishesBuff>();
			if (components.Length > 0)
			{
				components[0].Increment(this);
				return;
			}
			ThreeWishesBuff threeWishesBuff = target.AddComponent<ThreeWishesBuff>();
			threeWishesBuff.Initialize(this);
		}
	}
}
