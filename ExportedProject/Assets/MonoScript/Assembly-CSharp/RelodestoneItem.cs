using Dungeonator;
using UnityEngine;

public class RelodestoneItem : PlayerItem
{
	public float EffectRadius = 10f;

	public float duration = 3f;

	public float GravitationalForce = 500f;

	public GameObject ContinuousVFX;

	public RadialBurstInterface RelodestarBurst;

	private PlayerController m_owner;

	private GameObject m_instanceVFX;

	private int m_totalAbsorbedDuringCycle;

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_ammo_suck_01", base.gameObject);
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			m_owner = user;
			m_totalAbsorbedDuringCycle = 0;
			base.IsCurrentlyActive = true;
			m_activeElapsed = 0f;
			m_activeDuration = duration;
			m_instanceVFX = SpawnManager.SpawnVFX(ContinuousVFX);
			m_instanceVFX.transform.parent = user.transform;
			m_instanceVFX.transform.position = user.CenterPosition.ToVector3ZisY();
		}
	}

	public override void OnItemSwitched(PlayerController user)
	{
		BecomeInactive();
		base.OnItemSwitched(user);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		BecomeInactive();
		base.OnPreDrop(user);
	}

	private void BecomeInactive()
	{
		base.IsCurrentlyActive = false;
		if (m_totalAbsorbedDuringCycle > 0 && (bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.RELODESTAR))
		{
			int num = Mathf.CeilToInt((float)m_totalAbsorbedDuringCycle / 20f);
			int num2 = Mathf.CeilToInt((float)m_totalAbsorbedDuringCycle / (float)num);
			RelodestarBurst.MinToSpawnPerWave = num2;
			RelodestarBurst.MaxToSpawnPerWave = num2;
			RelodestarBurst.NumberWaves = num;
			RelodestarBurst.TimeBetweenWaves = 1f;
			RelodestarBurst.DoBurst(m_owner);
		}
		m_totalAbsorbedDuringCycle = 0;
		if ((bool)m_instanceVFX)
		{
			SpawnManager.Despawn(m_instanceVFX);
			m_instanceVFX = null;
		}
	}

	private void LateUpdate()
	{
		if (Dungeon.IsGenerating || !base.IsActive || !m_owner)
		{
			return;
		}
		Vector2 centerPosition = m_owner.CenterPosition;
		int num = 0;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && (bool)projectile.specRigidbody && AdjustRigidbodyVelocity(projectile.specRigidbody, centerPosition))
			{
				num++;
			}
		}
		m_totalAbsorbedDuringCycle += num;
		if (num > 0 && (bool)m_owner.CurrentGun && m_owner.CurrentGun.CanGainAmmo)
		{
			m_owner.CurrentGun.GainAmmo(num);
		}
		if (m_activeElapsed >= m_activeDuration)
		{
			BecomeInactive();
		}
	}

	private Vector2 GetFrameAccelerationForRigidbody(Vector2 unitCenter, Vector2 myCenter, float currentDistance, float g)
	{
		Vector2 zero = Vector2.zero;
		float num = Mathf.Clamp01(1f - currentDistance / EffectRadius);
		float num2 = g * num * num;
		Vector2 normalized = (myCenter - unitCenter).normalized;
		return normalized * num2;
	}

	private bool AdjustRigidbodyVelocity(SpeculativeRigidbody other, Vector2 myCenter)
	{
		bool result = false;
		Vector2 a = other.UnitCenter - myCenter;
		float effectRadius = EffectRadius;
		float num = Vector2.SqrMagnitude(a);
		if (num < effectRadius)
		{
			float gravitationalForce = GravitationalForce;
			Vector2 vector = other.Velocity;
			Projectile projectile = other.projectile;
			if ((bool)projectile && !(projectile.Owner is PlayerController))
			{
				if ((bool)projectile.GetComponent<ChainLightningModifier>())
				{
					Object.Destroy(projectile.GetComponent<ChainLightningModifier>());
				}
				projectile.collidesWithPlayer = false;
				if (other.GetComponent<BlackHoleDoer>() != null)
				{
					return false;
				}
				if (vector == Vector2.zero)
				{
					if (!projectile.IsBulletScript)
					{
						return false;
					}
					Vector2 vector2 = myCenter - other.UnitCenter;
					if (vector2 == Vector2.zero)
					{
						vector2 = new Vector2(Random.value - 0.5f, Random.value - 0.5f);
					}
					vector = vector2.normalized * 3f;
				}
				if (num < 2f)
				{
					projectile.DieInAir();
					result = true;
				}
				Vector2 frameAccelerationForRigidbody = GetFrameAccelerationForRigidbody(other.UnitCenter, myCenter, Mathf.Sqrt(num), gravitationalForce);
				float num2 = Mathf.Clamp(BraveTime.DeltaTime, 0f, 0.02f);
				Vector2 vector3 = frameAccelerationForRigidbody * num2;
				Vector2 vector4 = vector + vector3;
				if (BraveTime.DeltaTime > 0.02f)
				{
					vector4 *= 0.02f / BraveTime.DeltaTime;
				}
				other.Velocity = vector4;
				if (projectile != null)
				{
					projectile.collidesWithPlayer = false;
					if (projectile.IsBulletScript)
					{
						projectile.RemoveBulletScriptControl();
					}
					if (vector4 != Vector2.zero)
					{
						projectile.Direction = vector4.normalized;
						projectile.Speed = Mathf.Max(3f, vector4.magnitude);
						other.Velocity = projectile.Direction * projectile.Speed;
						if (projectile.shouldRotate && (vector4.x != 0f || vector4.y != 0f))
						{
							float num3 = BraveMathCollege.Atan2Degrees(projectile.Direction);
							if (!float.IsNaN(num3) && !float.IsInfinity(num3))
							{
								Quaternion rotation = Quaternion.Euler(0f, 0f, num3);
								if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y))
								{
									projectile.transform.rotation = rotation;
								}
							}
						}
					}
				}
				return result;
			}
			return false;
		}
		return result;
	}
}
