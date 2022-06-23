using System;
using System.Collections;
using UnityEngine;

public class ShaderProjModifier : BraveBehaviour
{
	public bool ProcessProperty = true;

	public string ShaderProperty;

	[HideInInspectorIf("ColorAttribute", false)]
	public float StartValue;

	[HideInInspectorIf("ColorAttribute", false)]
	public float EndValue = 1f;

	public float LerpTime;

	public bool ColorAttribute;

	[ShowInInspectorIf("ColorAttribute", false)]
	public Color StartColor;

	[ShowInInspectorIf("ColorAttribute", false)]
	public Color EndColor;

	public bool OnDeath;

	public bool PreventCorpse;

	public bool DisablesOutlines;

	public bool EnableEmission;

	public bool GlobalSparks;

	public Color GlobalSparksColor;

	public float GlobalSparksForce = 3f;

	public float GlobalSparksOverrideLifespan = -1f;

	public bool AddMaterialPass;

	public Material AddPass;

	public bool IsGlitter;

	public bool ShouldAffectBosses;

	public bool AddsEncircler;

	[Header("Combine Rifle")]
	public bool AppliesLocalSlowdown;

	public float LocalTimescaleMultiplier = 0.5f;

	public bool AppliesParticleSystem;

	public GameObject ParticleSystemToSpawn;

	private bool DoesScaleAmounts;

	public GlobalSparksDoer.SparksType GlobalSparkType;

	public bool GlobalSparksRepeat;

	private float GetStartValue()
	{
		return StartValue;
	}

	private float GetEndValue(HealthHaver hitEnemy)
	{
		float result = EndValue;
		if (DoesScaleAmounts && (bool)hitEnemy && (bool)hitEnemy.specRigidbody && hitEnemy.specRigidbody.HitboxPixelCollider != null)
		{
			result = Mathf.Lerp(EndValue, Mathf.Max(StartValue, EndValue / 10f), hitEnemy.specRigidbody.HitboxPixelCollider.UnitWidth * hitEnemy.specRigidbody.HitboxPixelCollider.UnitHeight / 5f);
		}
		return result;
	}

	private void Start()
	{
		if (ShaderProperty == "_EmissivePower")
		{
			DoesScaleAmounts = true;
		}
		Projectile obj = base.projectile;
		obj.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(obj.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(projectile_OnHitEnemy));
	}

	private void projectile_OnHitEnemy(Projectile proj, SpeculativeRigidbody enemyRigidbody, bool killed)
	{
		if (ColorAttribute && (bool)enemyRigidbody.gameActor && enemyRigidbody.gameActor.HasSourcedOverrideColor(ShaderProperty) && !GlobalSparksRepeat)
		{
			return;
		}
		HealthHaver healthHaver = enemyRigidbody.healthHaver;
		if (!healthHaver)
		{
			return;
		}
		if (killed && AppliesLocalSlowdown)
		{
			AIActor component = enemyRigidbody.GetComponent<AIActor>();
			if ((bool)component && (!component.healthHaver || !component.healthHaver.IsBoss))
			{
				component.LocalTimeScale *= LocalTimescaleMultiplier;
				if (component.ParentRoom != null)
				{
					component.ParentRoom.DeregisterEnemy(component);
				}
				if ((bool)component.aiAnimator)
				{
					component.aiAnimator.FpsScale *= LocalTimescaleMultiplier;
				}
				if ((bool)component.specRigidbody)
				{
					for (int i = 0; i < component.specRigidbody.PixelColliders.Count; i++)
					{
						component.specRigidbody.PixelColliders[i].Enabled = false;
					}
				}
				if ((bool)component.knockbackDoer)
				{
					component.knockbackDoer.timeScalar = 0f;
				}
				if ((bool)component.GetComponent<SpawnEnemyOnDeath>())
				{
					component.GetComponent<SpawnEnemyOnDeath>().chanceToSpawn = 0f;
				}
				if (AppliesParticleSystem)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(ParticleSystemToSpawn, component.CenterPosition.ToVector3ZisY(), Quaternion.identity);
					ParticleSystem component2 = gameObject.GetComponent<ParticleSystem>();
					gameObject.transform.parent = component.transform;
					if ((bool)component.sprite)
					{
						gameObject.transform.position = component.sprite.WorldCenter;
						Bounds bounds = component.sprite.GetBounds();
						ParticleSystem.ShapeModule shape = component2.shape;
						shape.scale = new Vector3(bounds.extents.x * 2f, bounds.extents.y * 2f, 0.125f);
					}
				}
			}
		}
		if (OnDeath && !killed)
		{
			return;
		}
		if ((bool)enemyRigidbody.aiActor && (IsGlitter || ShouldAffectBosses || !enemyRigidbody.healthHaver.IsBoss))
		{
			if (PreventCorpse)
			{
				if ((bool)enemyRigidbody.aiActor)
				{
					enemyRigidbody.aiActor.CorpseObject = null;
				}
				FreezeOnDeath component3 = enemyRigidbody.GetComponent<FreezeOnDeath>();
				if ((bool)component3)
				{
					component3.HandleDisintegration();
				}
			}
			if (DisablesOutlines && (bool)enemyRigidbody.sprite)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(enemyRigidbody.sprite);
			}
			if (ProcessProperty)
			{
				if (LerpTime <= 0f)
				{
					for (int j = 0; j < healthHaver.bodySprites.Count; j++)
					{
						tk2dBaseSprite tk2dBaseSprite2 = healthHaver.bodySprites[j];
						if (!tk2dBaseSprite2)
						{
							return;
						}
						tk2dBaseSprite2.usesOverrideMaterial = true;
						if (EnableEmission)
						{
							tk2dBaseSprite2.renderer.material.EnableKeyword("EMISSIVE_ON");
							tk2dBaseSprite2.renderer.material.DisableKeyword("EMISSIVE_OFF");
						}
						if (GlobalSparks)
						{
							int num = 100;
							if (GlobalSparksRepeat)
							{
								num = 20;
							}
							GlobalSparksDoer.EmitFromRegion(GlobalSparksDoer.EmitRegionStyle.RADIAL, num, LerpTime + 0.1f, tk2dBaseSprite2.WorldBottomLeft.ToVector3ZisY(), tk2dBaseSprite2.WorldTopRight.ToVector3ZisY(), new Vector3(GlobalSparksForce, 0f, 0f), 15f, 0.5f, null, (!(GlobalSparksOverrideLifespan > 0f)) ? null : new float?(GlobalSparksOverrideLifespan), GlobalSparksColor, GlobalSparkType);
						}
						tk2dBaseSprite2.renderer.material.SetFloat(ShaderProperty, GetEndValue(healthHaver));
						if (AddsEncircler)
						{
							tk2dBaseSprite2.gameObject.GetOrAddComponent<Encircler>();
						}
					}
				}
				else
				{
					GameManager.Instance.StartCoroutine(ApplyEffect(healthHaver, killed));
				}
			}
			if (AddMaterialPass)
			{
				for (int k = 0; k < healthHaver.bodySprites.Count; k++)
				{
					MeshRenderer component4 = healthHaver.bodySprites[k].GetComponent<MeshRenderer>();
					Material[] array = component4.sharedMaterials;
					Array.Resize(ref array, array.Length + 1);
					Material material = UnityEngine.Object.Instantiate(AddPass);
					material.SetTexture("_MainTex", array[0].GetTexture("_MainTex"));
					array[array.Length - 1] = material;
					component4.sharedMaterials = array;
				}
			}
		}
		if (IsGlitter && (bool)enemyRigidbody.aiActor)
		{
			enemyRigidbody.aiActor.HasBeenGlittered = true;
		}
	}

	private IEnumerator ApplyEffect(HealthHaver hh, bool killed)
	{
		float elapsed = 0f;
		bool processedOnce = false;
		while (elapsed < LerpTime)
		{
			if (!hh)
			{
				yield break;
			}
			float modifiedDeltaTime = BraveTime.DeltaTime;
			if (AppliesLocalSlowdown)
			{
				modifiedDeltaTime *= LocalTimescaleMultiplier;
			}
			elapsed += modifiedDeltaTime;
			float t = elapsed / LerpTime;
			for (int i = 0; i < hh.bodySprites.Count; i++)
			{
				hh.bodySprites[i].usesOverrideMaterial = true;
				if (EnableEmission && !processedOnce)
				{
					hh.bodySprites[i].renderer.material.EnableKeyword("EMISSIVE_ON");
					hh.bodySprites[i].renderer.material.DisableKeyword("EMISSIVE_OFF");
				}
				if (GlobalSparks && (GlobalSparksRepeat || !processedOnce))
				{
					int num = 100;
					if (GlobalSparksRepeat)
					{
						num = 20;
					}
					GlobalSparksDoer.EmitFromRegion(GlobalSparksDoer.EmitRegionStyle.RADIAL, num, LerpTime + 0.1f, hh.bodySprites[i].WorldBottomLeft.ToVector3ZisY(), hh.bodySprites[i].WorldTopRight.ToVector3ZisY(), new Vector3(GlobalSparksForce, 0f, 0f), 15f, 0.5f, null, (!(GlobalSparksOverrideLifespan > 0f)) ? null : new float?(GlobalSparksOverrideLifespan), GlobalSparksColor, GlobalSparkType);
				}
				if (ColorAttribute)
				{
					if ((bool)hh.gameActor)
					{
						hh.gameActor.RegisterOverrideColor(Color.Lerp(StartColor, EndColor, t), ShaderProperty);
					}
				}
				else
				{
					hh.bodySprites[i].renderer.material.SetFloat(ShaderProperty, Mathf.Lerp(GetStartValue(), GetEndValue(hh), t));
				}
			}
			processedOnce = true;
			yield return null;
		}
		if (AppliesLocalSlowdown && (bool)hh && (bool)hh.aiActor)
		{
			hh.aiActor.LocalTimeScale /= LocalTimescaleMultiplier;
			if ((bool)hh.aiAnimator)
			{
				hh.aiAnimator.FpsScale /= LocalTimescaleMultiplier;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
