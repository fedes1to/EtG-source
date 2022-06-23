using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningModifier : BraveBehaviour
{
	public GameObject LinkVFXPrefab;

	public CoreDamageTypes damageTypes;

	public bool RequiresSameProjectileClass;

	public float maximumLinkDistance = 8f;

	public float damagePerHit = 5f;

	public float damageCooldown = 1f;

	[NonSerialized]
	public bool CanChainToAnyProjectile;

	[NonSerialized]
	public bool UseForcedLinkProjectile;

	[NonSerialized]
	public Projectile ForcedLinkProjectile;

	[NonSerialized]
	public Projectile BackLinkProjectile;

	[NonSerialized]
	public bool DamagesPlayers;

	[NonSerialized]
	public bool DamagesEnemies = true;

	[Header("Dispersal")]
	public bool UsesDispersalParticles;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public float DispersalDensity = 3f;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public float DispersalMinCoherency = 0.2f;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public float DispersalMaxCoherency = 1f;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public GameObject DispersalParticleSystemPrefab;

	private Projectile m_frameLinkProjectile;

	private tk2dTiledSprite m_extantLink;

	private bool m_hasSetBlackBullet;

	private ParticleSystem m_dispersalParticles;

	private HashSet<AIActor> m_damagedEnemies = new HashSet<AIActor>();

	private void Start()
	{
		PhysicsEngine.Instance.OnPostRigidbodyMovement += PostRigidbodyUpdate;
	}

	protected override void OnDestroy()
	{
		if (PhysicsEngine.Instance != null)
		{
			PhysicsEngine.Instance.OnPostRigidbodyMovement -= PostRigidbodyUpdate;
		}
		ClearLink();
		if ((bool)BackLinkProjectile && (bool)BackLinkProjectile.GetComponent<ChainLightningModifier>())
		{
			ChainLightningModifier component = BackLinkProjectile.GetComponent<ChainLightningModifier>();
			component.ClearLink();
			component.ForcedLinkProjectile = null;
		}
		base.OnDestroy();
	}

	private void Update()
	{
		m_frameLinkProjectile = null;
	}

	private void UpdateLinkToProjectile(Projectile targetProjectile)
	{
		if (m_extantLink == null)
		{
			m_extantLink = SpawnManager.SpawnVFX(LinkVFXPrefab).GetComponent<tk2dTiledSprite>();
			int count = -1;
			if (DamagesPlayers && !m_hasSetBlackBullet)
			{
				m_hasSetBlackBullet = true;
				Material material = m_extantLink.GetComponent<Renderer>().material;
				material.SetFloat("_BlackBullet", 0.995f);
				material.SetFloat("_EmissiveColorPower", 4.9f);
			}
			else if (!DamagesPlayers && PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.TESLA_UNBOUND, out count))
			{
				Material material2 = m_extantLink.GetComponent<Renderer>().material;
				material2.SetFloat("_BlackBullet", 0.15f);
				material2.SetFloat("_EmissiveColorPower", 0.1f);
			}
		}
		m_frameLinkProjectile = targetProjectile;
		Vector2 unitCenter = base.projectile.specRigidbody.UnitCenter;
		Vector2 unitCenter2 = targetProjectile.specRigidbody.UnitCenter;
		m_extantLink.transform.position = unitCenter;
		Vector2 vector = unitCenter2 - unitCenter;
		float z = BraveMathCollege.Atan2Degrees(vector.normalized);
		int num = Mathf.RoundToInt(vector.magnitude / 0.0625f);
		m_extantLink.dimensions = new Vector2(num, m_extantLink.dimensions.y);
		m_extantLink.transform.rotation = Quaternion.Euler(0f, 0f, z);
		m_extantLink.UpdateZDepth();
		if (ApplyLinearDamage(unitCenter, unitCenter2) && UsesDispersalParticles)
		{
			DoDispersalParticles(unitCenter2, unitCenter);
		}
	}

	private void DoDispersalParticles(Vector2 posStart, Vector2 posEnd)
	{
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			if (!m_dispersalParticles)
			{
				m_dispersalParticles = GlobalDispersalParticleManager.GetSystemForPrefab(DispersalParticleSystemPrefab);
			}
			int num = Mathf.Max(Mathf.CeilToInt(Vector2.Distance(posStart, posEnd) * DispersalDensity), 1);
			for (int i = 0; i < num; i++)
			{
				float t = (float)i / (float)num;
				Vector3 position = Vector3.Lerp(posStart, posEnd, t);
				position += Vector3.back;
				float num2 = Mathf.PerlinNoise(position.x / 3f, position.y / 3f);
				Vector3 a = Quaternion.Euler(0f, 0f, num2 * 360f) * Vector3.right;
				Vector3 vector = Vector3.Lerp(a, UnityEngine.Random.insideUnitSphere, UnityEngine.Random.Range(DispersalMinCoherency, DispersalMaxCoherency));
				ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
				emitParams.position = position;
				emitParams.velocity = vector * m_dispersalParticles.startSpeed;
				emitParams.startSize = m_dispersalParticles.startSize;
				emitParams.startLifetime = m_dispersalParticles.startLifetime;
				emitParams.startColor = m_dispersalParticles.startColor;
				ParticleSystem.EmitParams emitParams2 = emitParams;
				m_dispersalParticles.Emit(emitParams2, 1);
			}
		}
	}

	private IEnumerator HandleDamageCooldown(AIActor damagedTarget)
	{
		m_damagedEnemies.Add(damagedTarget);
		yield return new WaitForSeconds(damageCooldown);
		m_damagedEnemies.Remove(damagedTarget);
	}

	private bool ApplyLinearDamage(Vector2 p1, Vector2 p2)
	{
		bool result = false;
		if (DamagesEnemies)
		{
			for (int i = 0; i < StaticReferenceManager.AllEnemies.Count; i++)
			{
				AIActor aIActor = StaticReferenceManager.AllEnemies[i];
				if (!m_damagedEnemies.Contains(aIActor) && (bool)aIActor && aIActor.HasBeenEngaged && aIActor.IsNormalEnemy && (bool)aIActor.specRigidbody)
				{
					Vector2 intersection = Vector2.zero;
					if (BraveUtility.LineIntersectsAABB(p1, p2, aIActor.specRigidbody.HitboxPixelCollider.UnitBottomLeft, aIActor.specRigidbody.HitboxPixelCollider.UnitDimensions, out intersection))
					{
						aIActor.healthHaver.ApplyDamage(damagePerHit, Vector2.zero, "Chain Lightning", damageTypes);
						result = true;
						GameManager.Instance.StartCoroutine(HandleDamageCooldown(aIActor));
					}
				}
			}
		}
		if (DamagesPlayers)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController && !playerController.IsGhost && (bool)playerController.healthHaver && playerController.healthHaver.IsAlive && playerController.healthHaver.IsVulnerable)
				{
					Vector2 intersection2 = Vector2.zero;
					if (BraveUtility.LineIntersectsAABB(p1, p2, playerController.specRigidbody.HitboxPixelCollider.UnitBottomLeft, playerController.specRigidbody.HitboxPixelCollider.UnitDimensions, out intersection2))
					{
						playerController.healthHaver.ApplyDamage(0.5f, Vector2.zero, base.projectile.OwnerName, damageTypes);
						result = true;
					}
				}
			}
		}
		return result;
	}

	private void ClearLink()
	{
		if (m_extantLink != null)
		{
			SpawnManager.Despawn(m_extantLink.gameObject);
			m_extantLink = null;
		}
	}

	private Projectile GetLinkProjectile()
	{
		Projectile projectile = null;
		float num = float.MaxValue;
		float num2 = maximumLinkDistance * maximumLinkDistance;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile2 = StaticReferenceManager.AllProjectiles[i];
			if (!projectile2 || !(projectile2 != base.projectile) || (!CanChainToAnyProjectile && !(projectile2.Owner == base.projectile.Owner)))
			{
				continue;
			}
			if (RequiresSameProjectileClass && !CanChainToAnyProjectile)
			{
				if ((bool)base.projectile.spriteAnimator && (bool)projectile2.spriteAnimator)
				{
					if (base.projectile.spriteAnimator.CurrentClip != projectile2.spriteAnimator.CurrentClip)
					{
						continue;
					}
				}
				else if ((bool)base.projectile.spriteAnimator || (bool)projectile2.spriteAnimator)
				{
					continue;
				}
				if ((bool)base.projectile.sprite && (bool)projectile2.sprite)
				{
					if (projectile2.sprite.spriteId != base.projectile.sprite.spriteId || projectile2.sprite.Collection != base.projectile.sprite.Collection)
					{
						continue;
					}
				}
				else if ((bool)base.projectile.sprite || (bool)projectile2.sprite)
				{
					continue;
				}
			}
			ChainLightningModifier component = projectile2.GetComponent<ChainLightningModifier>();
			if ((bool)component && component.m_frameLinkProjectile == null)
			{
				float sqrMagnitude = (component.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter).sqrMagnitude;
				if (sqrMagnitude < num && sqrMagnitude < num2)
				{
					projectile = projectile2;
					num = sqrMagnitude;
				}
			}
			else if (CanChainToAnyProjectile && (bool)projectile2 && (bool)projectile2.specRigidbody && (bool)this && (bool)base.specRigidbody)
			{
				float sqrMagnitude2 = (projectile2.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter).sqrMagnitude;
				if (sqrMagnitude2 < num && sqrMagnitude2 < num2)
				{
					projectile = projectile2;
					num = sqrMagnitude2;
				}
			}
		}
		if (projectile == null)
		{
			return null;
		}
		return projectile;
	}

	private void PostRigidbodyUpdate()
	{
		if ((bool)base.projectile)
		{
			Projectile projectile = ((!UseForcedLinkProjectile) ? GetLinkProjectile() : ForcedLinkProjectile);
			if ((bool)projectile)
			{
				UpdateLinkToProjectile(projectile);
			}
			else
			{
				ClearLink();
			}
		}
		else
		{
			ClearLink();
		}
	}
}
