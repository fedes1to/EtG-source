using System;
using Dungeonator;
using UnityEngine;

public class KnifeShieldEffect : BraveBehaviour
{
	public int numKnives = 5;

	public float knifeDamage = 5f;

	public float circleRadius = 3f;

	public float rotationDegreesPerSecond = 360f;

	public float throwSpeed = 3f;

	public float throwRange = 25f;

	public float throwRadius = 3f;

	public float radiusChangeDistance = 3f;

	public float remainingHealth;

	public GameObject deathVFX;

	private bool m_lightknife;

	protected PlayerController m_player;

	protected SpeculativeRigidbody[] m_knives;

	protected float m_elapsed;

	protected float m_traversedDistance;

	protected Vector3 m_currentShieldVelocity = Vector3.zero;

	protected Vector3 m_currentShieldCenterOffset = Vector3.zero;

	private Shader m_lightknivesShader;

	private Vector3 m_cachedOffsetBase;

	public bool IsActive
	{
		get
		{
			if (m_currentShieldVelocity != Vector3.zero)
			{
				return false;
			}
			for (int i = 0; i < m_knives.Length; i++)
			{
				if (m_knives[i] != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Initialize(PlayerController player, GameObject knifePrefab)
	{
		m_player = player;
		m_knives = new SpeculativeRigidbody[numKnives];
		m_lightknife = player.HasActiveBonusSynergy(CustomSynergyType.LIGHTKNIVES);
		for (int i = 0; i < numKnives; i++)
		{
			Vector3 position = player.LockedApproximateSpriteCenter + Quaternion.Euler(0f, 0f, 360f / (float)numKnives * (float)i) * Vector3.right * circleRadius;
			GameObject gameObject = UnityEngine.Object.Instantiate(knifePrefab, position, Quaternion.identity);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.HeightOffGround = 1.5f;
			tk2dSpriteAnimator component2 = gameObject.GetComponent<tk2dSpriteAnimator>();
			component2.PlayFromFrame(UnityEngine.Random.Range(0, component2.DefaultClip.frames.Length));
			if (m_lightknife)
			{
				SetOverrideShader(component);
			}
			SpeculativeRigidbody component3 = gameObject.GetComponent<SpeculativeRigidbody>();
			component3.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(component3.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			component3.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component3.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleCollision));
			component3.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(component3.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(HandleTileCollision));
			m_knives[i] = component3;
		}
	}

	private void SetOverrideShader(tk2dSprite spr)
	{
		spr.usesOverrideMaterial = true;
		Material material = spr.GetComponent<MeshRenderer>().material;
		if (m_lightknivesShader == null)
		{
			m_lightknivesShader = Shader.Find("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
		}
		material.shader = m_lightknivesShader;
		material.SetColor("_OverrideColor", new Color(20f / 51f, 0.8235294f, 0.470588237f));
		material.SetFloat("_EmissivePower", 130f);
	}

	private void HandleTileCollision(CollisionData tileCollision)
	{
		int num = Array.IndexOf(m_knives, tileCollision.MyRigidbody);
		if (num != -1)
		{
			m_knives[num] = null;
		}
		tileCollision.MyRigidbody.sprite.PlayEffectOnSprite(deathVFX, Vector3.zero, false);
		UnityEngine.Object.Destroy(tileCollision.MyRigidbody.gameObject);
	}

	public void ThrowShield()
	{
		AkSoundEngine.PostEvent("Play_OBJ_daggershield_shot_01", base.gameObject);
		if (!(m_currentShieldVelocity == Vector3.zero))
		{
			return;
		}
		Vector3 vector = m_player.unadjustedAimPoint - (Vector3)m_player.CenterPosition;
		m_currentShieldVelocity = vector.WithZ(0f).normalized * throwSpeed;
		for (int i = 0; i < m_knives.Length; i++)
		{
			if (m_knives[i] != null && (bool)m_knives[i])
			{
				m_knives[i].specRigidbody.CollideWithTileMap = true;
			}
		}
	}

	protected Vector3 GetTargetPositionForKniveID(Vector3 center, int i, float radiusToUse)
	{
		float num = rotationDegreesPerSecond * m_elapsed % 360f;
		return center + Quaternion.Euler(0f, 0f, num + 360f / (float)numKnives * (float)i) * Vector3.right * radiusToUse;
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody other, PixelCollider otherCollider)
	{
		Projectile component = other.GetComponent<Projectile>();
		if (component != null)
		{
			if (component.Owner is PlayerController)
			{
				PhysicsEngine.SkipCollision = true;
			}
			else if (m_lightknife)
			{
				PassiveReflectItem.ReflectBullet(component, true, m_player, 10f);
				DestroyKnife(myRigidbody);
			}
		}
		GameActor component2 = other.GetComponent<GameActor>();
		if (component2 is PlayerController)
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (component2 is AIActor && !(component2 as AIActor).IsNormalEnemy)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void HandleCollision(SpeculativeRigidbody other, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (other.GetComponent<AIActor>() != null)
		{
			HealthHaver component = other.GetComponent<HealthHaver>();
			float num = knifeDamage;
			if (m_lightknife)
			{
				num *= 3f;
			}
			component.ApplyDamage(num, Vector2.zero, "Knife Shield");
			int num2 = Array.IndexOf(m_knives, source);
			if (num2 != -1)
			{
				m_knives[num2] = null;
			}
			source.sprite.PlayEffectOnSprite(deathVFX, Vector3.zero, false);
			UnityEngine.Object.Destroy(source.gameObject);
		}
		else
		{
			if (!(other.GetComponent<Projectile>() != null))
			{
				return;
			}
			Projectile component2 = other.GetComponent<Projectile>();
			if (!(component2.Owner is PlayerController))
			{
				if (!m_lightknife)
				{
					component2.DieInAir();
				}
				remainingHealth -= component2.ModifiedDamage;
				if (remainingHealth <= 0f)
				{
					DestroyKnife(source);
				}
			}
		}
	}

	private void DestroyKnife(SpeculativeRigidbody source)
	{
		int num = Array.IndexOf(m_knives, source);
		if (num != -1)
		{
			m_knives[num] = null;
		}
		source.sprite.PlayEffectOnSprite(deathVFX, Vector3.zero, false);
		UnityEngine.Object.Destroy(source.gameObject);
	}

	private void Update()
	{
		if (GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating)
		{
			return;
		}
		m_elapsed += BraveTime.DeltaTime;
		bool flag = m_currentShieldVelocity != Vector3.zero;
		Vector3 vector = m_currentShieldVelocity * BraveTime.DeltaTime;
		m_currentShieldCenterOffset += vector;
		if (!flag)
		{
			m_cachedOffsetBase = m_player.LockedApproximateSpriteCenter;
		}
		else
		{
			m_traversedDistance += vector.magnitude;
			if (m_traversedDistance > throwRange)
			{
				for (int i = 0; i < m_knives.Length; i++)
				{
					if (m_knives[i] != null && (bool)m_knives[i])
					{
						m_knives[i].sprite.PlayEffectOnSprite(deathVFX, Vector3.zero, false);
						UnityEngine.Object.Destroy(m_knives[i].gameObject);
						m_knives[i] = null;
					}
				}
			}
		}
		Vector3 center = m_cachedOffsetBase + m_currentShieldCenterOffset;
		float radiusToUse = circleRadius;
		if (flag)
		{
			radiusToUse = Mathf.Lerp(circleRadius, throwRadius, m_traversedDistance / radiusChangeDistance);
		}
		for (int j = 0; j < numKnives; j++)
		{
			if (m_knives[j] != null && (bool)m_knives[j])
			{
				Vector3 targetPositionForKniveID = GetTargetPositionForKniveID(center, j, radiusToUse);
				Vector3 vector2 = targetPositionForKniveID - m_knives[j].transform.position;
				Vector2 velocity = vector2.XY() / BraveTime.DeltaTime;
				m_knives[j].Velocity = velocity;
				m_knives[j].sprite.UpdateZDepth();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
