using System;
using System.Collections;
using UnityEngine;

public class TetrisBuff : AppliedEffectBase
{
	public enum TetrisType
	{
		BLOCK,
		L,
		L_REVERSED,
		S,
		S_REVERSED,
		T,
		LINE
	}

	public TetrisType type;

	public ExplosionData tetrisExplosion;

	public float ExplosionDamagePerTetromino = 6f;

	[Tooltip("How long each application lasts.")]
	public float lifetime;

	[Tooltip("The maximum length of time this debuff can be extended to by repeat applications.")]
	public float maxLifetime;

	public GameObject vfx;

	[NonSerialized]
	public bool shouldBurst;

	private float elapsed;

	private GameObject instantiatedVFX;

	private HealthHaver hh;

	private bool wasDuplicate;

	private void InitializeSelf(float length, float maxLength)
	{
		hh = GetComponent<HealthHaver>();
		lifetime = length;
		maxLifetime = maxLength;
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
		if (source is TetrisBuff)
		{
			TetrisBuff tetrisBuff = source as TetrisBuff;
			InitializeSelf(tetrisBuff.lifetime, tetrisBuff.maxLifetime);
			type = tetrisBuff.type;
			if ((tetrisBuff.vfx != null) ? true : false)
			{
				instantiatedVFX = SpawnManager.SpawnVFX(tetrisBuff.vfx, base.transform.position, Quaternion.identity);
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
					component3.ClearData();
					component3.Initialize(GetComponent<GameActor>());
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
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
		bool flag = type == TetrisType.LINE;
		bool flag2 = false;
		TetrisBuff[] components = target.GetComponents<TetrisBuff>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].shouldBurst = components[i].shouldBurst || flag;
			if (components[i].type == type)
			{
				if (1 == 0)
				{
					components[i].ExtendLength(lifetime);
					return;
				}
				flag2 = true;
			}
		}
		TetrisBuff tetrisBuff = target.AddComponent<TetrisBuff>();
		tetrisBuff.shouldBurst = flag;
		tetrisBuff.tetrisExplosion = tetrisExplosion;
		tetrisBuff.Initialize(this);
	}

	private IEnumerator ApplyModification()
	{
		elapsed = 0f;
		while (elapsed < lifetime && !shouldBurst && (bool)hh && !hh.IsDead)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if (elapsed == 0f)
		{
			yield return null;
		}
		if (shouldBurst)
		{
			if (type == TetrisType.LINE && (bool)hh)
			{
				AIActor component = hh.GetComponent<AIActor>();
				if ((bool)component)
				{
					Exploder.Explode(component.CenterPosition.ToVector3ZisY(), tetrisExplosion, Vector2.zero);
				}
			}
			if ((bool)hh && !hh.IsDead)
			{
				hh.ApplyDamage(ExplosionDamagePerTetromino, Vector2.zero, base.name, CoreDamageTypes.None, DamageCategory.DamageOverTime);
			}
			shouldBurst = false;
		}
		if ((bool)instantiatedVFX)
		{
			BuffVFXAnimator component2 = instantiatedVFX.GetComponent<BuffVFXAnimator>();
			if (component2 != null && component2.persistsOnDeath)
			{
				tk2dSpriteAnimator component3 = component2.GetComponent<tk2dSpriteAnimator>();
				if (component3 != null)
				{
					component3.Stop();
				}
				instantiatedVFX.GetComponent<PersistentVFXBehaviour>().BecomeDebris(Vector3.zero, 0.5f);
			}
			else if ((bool)component2)
			{
				component2.ForceDrop();
			}
			else
			{
				UnityEngine.Object.Destroy(instantiatedVFX);
			}
		}
		UnityEngine.Object.Destroy(this);
	}
}
