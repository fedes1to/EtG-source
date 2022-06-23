using System.Collections;
using Dungeonator;
using UnityEngine;

public class BlackHoleDoer : SpawnObjectItem
{
	public enum BlackHoleIntroStyle
	{
		Gradual,
		Instant
	}

	public enum BlackHoleOutroStyle
	{
		FadeAway,
		Nova
	}

	[Header("Intro Settings")]
	public BlackHoleIntroStyle introStyle;

	[ShowInInspectorIf("introStyle", 0, false)]
	public float introDuration = 0.5f;

	[Header("Core Settings")]
	public float coreDuration = 5f;

	public float damageRadius = 0.5f;

	public float radius = 15f;

	public float gravitationalForce = 10f;

	public float gravitationalForceActors = 50f;

	public bool affectsBullets = true;

	public bool destroysBullets = true;

	public bool affectsDebris = true;

	public bool destroysDebris = true;

	public bool affectsEnemies = true;

	public float damageToEnemiesPerSecond = 30f;

	public bool affectsPlayer;

	public float damageToPlayerPerSecond;

	[Header("Outro Settings")]
	public BlackHoleOutroStyle outroStyle;

	[ShowInInspectorIf("outroStyle", 0, false)]
	public float outroDuration = 0.5f;

	[ShowInInspectorIf("outroStyle", 1, false)]
	public float novaForce = 50f;

	public float distortStrength = 0.01f;

	public float distortTimeScale = 0.5f;

	public float distortRadiusFactor = 1f;

	private int m_currentPhase;

	private bool m_currentPhaseInitiated;

	private float m_currentPhaseTimer = -1000f;

	private float m_radiusSquared;

	[Header("Synergy Settings")]
	public bool HasHellSynergy;

	[LongNumericEnum]
	public CustomSynergyType HellSynergy;

	public GameObject HellSynergyVFX;

	public GameObject OuterLimitsVFX;

	public GameObject OuterLimitsDamageVFX;

	private bool m_cachedOuterLimitsSynergy;

	private int m_planetsEaten;

	private float m_elapsed;

	private float m_fadeStartDistortStrength;

	private Material m_distortMaterial;

	private void Start()
	{
		m_distortMaterial = new Material(ShaderCache.Acquire("Brave/Internal/DistortionRadius"));
		m_distortMaterial.SetFloat("_Strength", distortStrength);
		m_distortMaterial.SetFloat("_TimePulse", distortTimeScale);
		m_distortMaterial.SetFloat("_RadiusFactor", distortRadiusFactor);
		m_distortMaterial.SetVector("_WaveCenter", GetCenterPointInScreenUV(base.sprite.WorldCenter));
		Pixelator.Instance.RegisterAdditionalRenderPass(m_distortMaterial);
		m_radiusSquared = radius * radius;
		m_currentPhase = 0;
		m_currentPhaseInitiated = false;
		m_currentPhaseTimer = -1000f;
		if (HasHellSynergy && SpawningPlayer.HasActiveBonusSynergy(HellSynergy))
		{
			GameObject gameObject = Object.Instantiate(HellSynergyVFX, base.transform.position + new Vector3(0f, -0.5f, 0.5f), Quaternion.Euler(45f, 0f, 0f), base.transform);
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			radius *= 2f;
			damageRadius *= 4f;
			gravitationalForceActors *= 4f;
			damageToEnemiesPerSecond *= 3f;
			StartCoroutine(HoldPortalOpen(component));
		}
	}

	private IEnumerator HoldPortalOpen(MeshRenderer portal)
	{
		portal.material.SetFloat("_UVDistCutoff", 0f);
		yield return new WaitForSeconds(introDuration);
		float elapsed = 0f;
		float duration = coreDuration;
		float t2 = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			t2 = Mathf.Clamp01(elapsed / 0.25f);
			portal.material.SetFloat("_UVDistCutoff", Mathf.Lerp(0f, 0.21f, t2));
			yield return null;
		}
	}

	private Vector4 GetCenterPointInScreenUV(Vector2 centerPoint)
	{
		Vector3 vector = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(centerPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, 0f, 0f);
	}

	private float GetDistanceToRigidbody(SpeculativeRigidbody other)
	{
		return Vector2.Distance(other.UnitCenter, base.specRigidbody.UnitCenter);
	}

	private Vector2 GetFrameAccelerationForRigidbody(Vector2 unitCenter, float currentDistance, float g)
	{
		Vector2 zero = Vector2.zero;
		float num = Mathf.Clamp01(1f - currentDistance / radius);
		float num2 = g * num * num;
		Vector2 normalized = (base.specRigidbody.UnitCenter - unitCenter).normalized;
		return normalized * num2;
	}

	private bool AdjustDebrisVelocity(DebrisObject debris)
	{
		if (debris.IsPickupObject)
		{
			return false;
		}
		if (debris.GetComponent<BlackHoleDoer>() != null)
		{
			return false;
		}
		Vector2 a = debris.sprite.WorldCenter - base.specRigidbody.UnitCenter;
		float num = Vector2.SqrMagnitude(a);
		if (num < m_radiusSquared)
		{
			float g = gravitationalForceActors;
			float num2 = Mathf.Sqrt(num);
			if (num2 < damageRadius)
			{
				Object.Destroy(debris.gameObject);
				return true;
			}
			Vector2 frameAccelerationForRigidbody = GetFrameAccelerationForRigidbody(debris.sprite.WorldCenter, num2, g);
			float num3 = Mathf.Clamp(BraveTime.DeltaTime, 0f, 0.02f);
			if (debris.HasBeenTriggered)
			{
				debris.ApplyVelocity(frameAccelerationForRigidbody * num3);
			}
			else if (num2 < radius / 2f)
			{
				debris.Trigger(frameAccelerationForRigidbody * num3, 0.5f);
			}
			return true;
		}
		return false;
	}

	private bool AdjustRigidbodyVelocity(SpeculativeRigidbody other)
	{
		Vector2 a = other.UnitCenter - base.specRigidbody.UnitCenter;
		float num = Vector2.SqrMagnitude(a);
		if (num < m_radiusSquared)
		{
			float num2 = gravitationalForce;
			Vector2 velocity = other.Velocity;
			Projectile projectile = other.projectile;
			if ((bool)projectile)
			{
				projectile.collidesWithPlayer = false;
				if (other.GetComponent<BlackHoleDoer>() != null)
				{
					return false;
				}
				if (velocity == Vector2.zero)
				{
					return false;
				}
				if (num < 4f && (destroysBullets || m_cachedOuterLimitsSynergy))
				{
					if ((bool)projectile.GetComponent<BecomeOrbitProjectileModifier>())
					{
						m_planetsEaten++;
					}
					projectile.DieInAir();
					if (m_planetsEaten > 2)
					{
						if ((bool)OuterLimitsVFX)
						{
							GameObject gameObject = SpawnManager.SpawnVFX(OuterLimitsVFX);
							gameObject.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(base.transform.position.XY(), tk2dBaseSprite.Anchor.MiddleCenter);
						}
						RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
						absoluteRoom.ApplyActionToNearbyEnemies(base.transform.position.XY(), 100f, OuterLimitsProcessEnemy);
						AkSoundEngine.PostEvent("Stop_SND_OBJ", base.gameObject);
						AkSoundEngine.PostEvent("Play_WPN_blackhole_impact_01", base.gameObject);
						AkSoundEngine.PostEvent("Play_OBJ_lightning_flash_01", base.gameObject);
						Object.Destroy(base.gameObject);
					}
				}
				num2 = gravitationalForce;
			}
			else
			{
				if (!other.aiActor)
				{
					return false;
				}
				num2 = gravitationalForceActors;
				if (!other.aiActor.enabled)
				{
					return false;
				}
				if (!other.aiActor.HasBeenEngaged)
				{
					return false;
				}
				if (BraveMathCollege.DistToRectangle(base.specRigidbody.UnitCenter, other.UnitBottomLeft, other.UnitDimensions) < damageRadius)
				{
					other.healthHaver.ApplyDamage(damageToEnemiesPerSecond * BraveTime.DeltaTime, a.normalized, string.Empty, CoreDamageTypes.None, DamageCategory.DamageOverTime);
				}
				if (other.healthHaver.IsBoss)
				{
					return false;
				}
			}
			Vector2 frameAccelerationForRigidbody = GetFrameAccelerationForRigidbody(other.UnitCenter, Mathf.Sqrt(num), num2);
			float num3 = Mathf.Clamp(BraveTime.DeltaTime, 0f, 0.02f);
			Vector2 vector = frameAccelerationForRigidbody * num3;
			Vector2 vector2 = velocity + vector;
			if (BraveTime.DeltaTime > 0.02f)
			{
				vector2 *= 0.02f / BraveTime.DeltaTime;
			}
			other.Velocity = vector2;
			if (projectile != null)
			{
				projectile.collidesWithPlayer = false;
				if (projectile.IsBulletScript)
				{
					projectile.RemoveBulletScriptControl();
				}
				if (vector2 != Vector2.zero)
				{
					projectile.Direction = vector2.normalized;
					projectile.Speed = Mathf.Max(3f, vector2.magnitude);
					other.Velocity = projectile.Direction * projectile.Speed;
					if (projectile.shouldRotate && (vector2.x != 0f || vector2.y != 0f))
					{
						float num4 = BraveMathCollege.Atan2Degrees(projectile.Direction);
						if (!float.IsNaN(num4) && !float.IsInfinity(num4))
						{
							Quaternion rotation = Quaternion.Euler(0f, 0f, num4);
							if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y))
							{
								projectile.transform.rotation = rotation;
							}
						}
					}
				}
			}
			return true;
		}
		return false;
	}

	public void OuterLimitsProcessEnemy(AIActor a, float b)
	{
		if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver && !a.IsGone)
		{
			a.healthHaver.ApplyDamage(100f, Vector2.zero, "projectile");
			if (OuterLimitsDamageVFX != null)
			{
				a.PlayEffectOnActor(OuterLimitsDamageVFX, Vector3.zero, false, true);
			}
		}
	}

	private void LateUpdate()
	{
		m_elapsed += BraveTime.DeltaTime;
		m_cachedOuterLimitsSynergy = (bool)GameManager.Instance.BestActivePlayer && GameManager.Instance.BestActivePlayer.HasActiveBonusSynergy(CustomSynergyType.OUTER_LIMITS);
		if ((bool)this && (bool)base.projectile && m_elapsed > 9f)
		{
			base.projectile.DieInAir();
			return;
		}
		if (m_distortMaterial != null)
		{
			m_distortMaterial.SetVector("_WaveCenter", GetCenterPointInScreenUV(base.sprite.WorldCenter));
		}
		switch (m_currentPhase)
		{
		case 0:
			LateUpdateIntro();
			break;
		case 1:
			LateUpdateCore();
			break;
		case 2:
			LateUpdateOutro();
			break;
		default:
			Debug.LogError("Invalid State in BlackHoleDoer: " + m_currentPhase);
			break;
		}
	}

	private void LateUpdateIntro()
	{
		if (introStyle == BlackHoleIntroStyle.Instant)
		{
			m_currentPhase = 1;
		}
		else if (introStyle == BlackHoleIntroStyle.Gradual)
		{
			if (!m_currentPhaseInitiated)
			{
				m_currentPhaseInitiated = true;
				m_currentPhaseTimer = introDuration;
			}
			else if (m_currentPhaseTimer > 0f)
			{
				m_currentPhaseTimer -= BraveTime.DeltaTime;
			}
			else
			{
				m_currentPhase = 1;
				m_currentPhaseInitiated = false;
				m_currentPhaseTimer = -1000f;
			}
		}
	}

	private void LateUpdateCore()
	{
		if (!m_currentPhaseInitiated)
		{
			m_currentPhaseInitiated = true;
			m_currentPhaseTimer = coreDuration;
		}
		else if (m_currentPhaseTimer > 0f)
		{
			m_currentPhaseTimer -= BraveTime.DeltaTime;
			for (int i = 0; i < PhysicsEngine.Instance.AllRigidbodies.Count; i++)
			{
				if (PhysicsEngine.Instance.AllRigidbodies[i].gameObject.activeSelf && PhysicsEngine.Instance.AllRigidbodies[i].enabled)
				{
					AdjustRigidbodyVelocity(PhysicsEngine.Instance.AllRigidbodies[i]);
				}
			}
			for (int j = 0; j < StaticReferenceManager.AllDebris.Count; j++)
			{
				AdjustDebrisVelocity(StaticReferenceManager.AllDebris[j]);
			}
		}
		else
		{
			m_currentPhase = 2;
			m_currentPhaseInitiated = false;
			m_currentPhaseTimer = -1000f;
		}
	}

	private void LateUpdateOutro()
	{
		switch (outroStyle)
		{
		case BlackHoleOutroStyle.FadeAway:
			LateUpdateOutro_Fade();
			break;
		case BlackHoleOutroStyle.Nova:
			LateUpdateOutro_Nova();
			break;
		}
	}

	private void LateUpdateOutro_Fade()
	{
		if (!m_currentPhaseInitiated)
		{
			m_currentPhaseInitiated = true;
			m_currentPhaseTimer = outroDuration;
			m_fadeStartDistortStrength = m_distortMaterial.GetFloat("_Strength");
			tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName("black_hole_out_item_vfx");
			outroDuration = clipByName.BaseClipLength;
			base.spriteAnimator.PlayAndDestroyObject("black_hole_out_item_vfx");
		}
		else if (m_currentPhaseTimer > 0f)
		{
			m_currentPhaseTimer -= BraveTime.DeltaTime;
			float t = 1f - m_currentPhaseTimer / outroDuration;
			if (m_distortMaterial != null)
			{
				m_distortMaterial.SetFloat("_Strength", Mathf.Lerp(m_fadeStartDistortStrength, 0f, t));
			}
		}
	}

	private void LateUpdateOutro_Nova()
	{
		if (!m_currentPhaseInitiated)
		{
			m_currentPhaseInitiated = true;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		if (Pixelator.Instance != null)
		{
			Pixelator.Instance.DeregisterAdditionalRenderPass(m_distortMaterial);
		}
		base.OnDestroy();
	}
}
