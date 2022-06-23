using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbitProjectileMotionModule : ProjectileAndBeamMotionModule
{
	private static Dictionary<int, List<OrbitProjectileMotionModule>> m_currentOrbiters = new Dictionary<int, List<OrbitProjectileMotionModule>>();

	public float MinRadius = 2f;

	public float MaxRadius = 5f;

	[NonSerialized]
	public float CustomSpawnVFXElapsed = -1f;

	[NonSerialized]
	public bool HasSpawnVFX;

	[NonSerialized]
	public GameObject SpawnVFX;

	public bool ForceInvert;

	private float m_radius;

	private float m_currentAngle;

	private bool m_initialized;

	private Vector2 m_initialRightVector;

	private Vector2 m_initialUpVector;

	[NonSerialized]
	private bool m_isOrbiting = true;

	[NonSerialized]
	public int OrbitGroup = -1;

	[NonSerialized]
	private bool m_hasDoneSpawnVFX;

	[NonSerialized]
	public float lifespan = -1f;

	[NonSerialized]
	public bool usesAlternateOrbitTarget;

	[NonSerialized]
	public SpeculativeRigidbody alternateOrbitTarget;

	private float m_beamOrbitRadius = 2.75f;

	private float m_beamOrbitRadiusCircumference = 17.27876f;

	private bool m_spawnVFXActive;

	private GameObject m_activeSpawnVFX;

	private float m_spawnVFXElapsed;

	public bool m_isBeam;

	public static int ActiveBeams = 0;

	public bool StackHelix;

	public float BeamOrbitRadius
	{
		get
		{
			return m_beamOrbitRadius;
		}
		set
		{
			m_beamOrbitRadius = value;
			m_beamOrbitRadiusCircumference = (float)Math.PI * 2f * m_beamOrbitRadius;
		}
	}

	public static int GetOrbitersInGroup(int group)
	{
		if (m_currentOrbiters.ContainsKey(group))
		{
			return (m_currentOrbiters[group] != null) ? m_currentOrbiters[group].Count : 0;
		}
		return 0;
	}

	public override void UpdateDataOnBounce(float angleDiff)
	{
		if (!float.IsNaN(angleDiff))
		{
			m_initialUpVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialUpVector;
			m_initialRightVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialRightVector;
		}
	}

	public override void AdjustRightVector(float angleDiff)
	{
		if (!float.IsNaN(angleDiff))
		{
			m_initialUpVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialUpVector;
			m_initialRightVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialRightVector;
		}
	}

	private List<OrbitProjectileMotionModule> RegisterSelfWithDictionary()
	{
		if (!m_currentOrbiters.ContainsKey(OrbitGroup))
		{
			m_currentOrbiters.Add(OrbitGroup, new List<OrbitProjectileMotionModule>());
		}
		List<OrbitProjectileMotionModule> list = m_currentOrbiters[OrbitGroup];
		if (!list.Contains(this))
		{
			list.Add(this);
		}
		return list;
	}

	private void DeregisterSelfWithDictionary()
	{
		if (m_currentOrbiters.ContainsKey(OrbitGroup))
		{
			List<OrbitProjectileMotionModule> list = m_currentOrbiters[OrbitGroup];
			list.Remove(this);
		}
	}

	public override void Move(Projectile source, Transform projectileTransform, tk2dBaseSprite projectileSprite, SpeculativeRigidbody specRigidbody, ref float m_timeElapsed, ref Vector2 m_currentDirection, bool Inverted, bool shouldRotate)
	{
		if (m_isOrbiting && (!source || (!usesAlternateOrbitTarget && !source.Owner) || (usesAlternateOrbitTarget && !alternateOrbitTarget)))
		{
			m_isOrbiting = false;
		}
		if (m_isOrbiting && lifespan > 0f && m_timeElapsed > lifespan)
		{
			m_isOrbiting = false;
			DeregisterSelfWithDictionary();
		}
		if (!m_isOrbiting)
		{
			return;
		}
		Vector2 vector = ((!projectileSprite) ? projectileTransform.position.XY() : projectileSprite.WorldCenter);
		if (HasSpawnVFX && !m_hasDoneSpawnVFX)
		{
			m_hasDoneSpawnVFX = true;
			m_spawnVFXActive = true;
			m_activeSpawnVFX = SpawnManager.SpawnVFX(SpawnVFX, vector, Quaternion.identity);
			source.sprite.renderer.enabled = false;
		}
		if (m_hasDoneSpawnVFX)
		{
			m_spawnVFXElapsed += BraveTime.DeltaTime;
		}
		if (m_spawnVFXActive && (!m_activeSpawnVFX || !m_activeSpawnVFX.activeSelf || (CustomSpawnVFXElapsed > 0f && m_spawnVFXElapsed > CustomSpawnVFXElapsed)))
		{
			m_activeSpawnVFX = null;
			m_spawnVFXActive = false;
			source.sprite.renderer.enabled = true;
		}
		if (!m_initialized)
		{
			m_initialized = true;
			m_initialRightVector = ((!shouldRotate) ? m_currentDirection : projectileTransform.right.XY());
			m_initialUpVector = ((!shouldRotate) ? (Quaternion.Euler(0f, 0f, 90f) * m_currentDirection) : projectileTransform.up);
			m_radius = UnityEngine.Random.Range(MinRadius, MaxRadius);
			m_currentAngle = m_initialRightVector.ToAngle();
			source.OnDestruction += OnDestroyed;
		}
		RegisterSelfWithDictionary();
		m_timeElapsed += BraveTime.DeltaTime;
		float radius = m_radius;
		float num = source.Speed * BraveTime.DeltaTime;
		float num2 = num / ((float)Math.PI * 2f * radius) * 360f;
		m_currentAngle += num2;
		Vector2 zero = Vector2.zero;
		zero = ((!usesAlternateOrbitTarget) ? source.Owner.CenterPosition : alternateOrbitTarget.UnitCenter);
		Vector2 vector2 = zero + (Quaternion.Euler(0f, 0f, m_currentAngle) * Vector2.right * radius).XY();
		if (StackHelix)
		{
			float num3 = 2f;
			float num4 = 1f;
			int num5 = ((!ForceInvert) ? 1 : (-1));
			float num6 = (float)num5 * num4 * Mathf.Sin(source.GetElapsedDistance() / num3);
			vector2 += (vector2 - zero).normalized * num6;
		}
		Vector2 velocity = (vector2 - vector) / BraveTime.DeltaTime;
		m_currentDirection = velocity.normalized;
		if (shouldRotate)
		{
			float num7 = m_currentDirection.ToAngle();
			if (float.IsNaN(num7) || float.IsInfinity(num7))
			{
				num7 = 0f;
			}
			projectileTransform.localRotation = Quaternion.Euler(0f, 0f, num7);
		}
		specRigidbody.Velocity = velocity;
		if (float.IsNaN(specRigidbody.Velocity.magnitude) || Mathf.Approximately(specRigidbody.Velocity.magnitude, 0f))
		{
			source.DieInAir();
		}
		if ((bool)m_activeSpawnVFX)
		{
			m_activeSpawnVFX.transform.position = vector2.ToVector3ZisY();
		}
	}

	public void BeamDestroyed()
	{
		OnDestroyed(null);
	}

	private void OnDestroyed(Projectile obj)
	{
		DeregisterSelfWithDictionary();
		if (m_isBeam)
		{
			m_isBeam = false;
			ActiveBeams--;
		}
	}

	public override void SentInDirection(ProjectileData baseData, Transform projectileTransform, tk2dBaseSprite projectileSprite, SpeculativeRigidbody specRigidbody, ref float m_timeElapsed, ref Vector2 m_currentDirection, bool shouldRotate, Vector2 dirVec, bool resetDistance, bool updateRotation)
	{
	}

	public void RegisterAsBeam(BeamController beam)
	{
		if (!m_isBeam)
		{
			BasicBeamController basicBeamController = beam as BasicBeamController;
			if ((bool)basicBeamController && !basicBeamController.IsReflectedBeam)
			{
				basicBeamController.IgnoreTilesDistance = m_beamOrbitRadiusCircumference;
			}
			m_isBeam = true;
			ActiveBeams++;
		}
	}

	public override Vector2 GetBoneOffset(BasicBeamController.BeamBone bone, BeamController sourceBeam, bool inverted)
	{
		if (sourceBeam.IsReflectedBeam)
		{
			return Vector2.zero;
		}
		PlayerController playerController = sourceBeam.Owner as PlayerController;
		Vector2 vector = playerController.unadjustedAimPoint.XY() - playerController.CenterPosition;
		float num = vector.ToAngle();
		Vector2 vector2 = bone.Position - playerController.CenterPosition;
		Vector2 result;
		if (bone.PosX < m_beamOrbitRadiusCircumference)
		{
			float num2 = bone.PosX / m_beamOrbitRadiusCircumference * 360f + num;
			float x = Mathf.Cos((float)Math.PI / 180f * num2) * BeamOrbitRadius;
			float y = Mathf.Sin((float)Math.PI / 180f * num2) * BeamOrbitRadius;
			bone.RotationAngle = num2 + 90f;
			result = new Vector2(x, y) - vector2;
		}
		else
		{
			bone.RotationAngle = num;
			result = vector.normalized * (bone.PosX - m_beamOrbitRadiusCircumference + m_beamOrbitRadius) - vector2;
		}
		if (StackHelix)
		{
			float num3 = 3f;
			float num4 = 1f;
			float num5 = 6f;
			int num6 = ((!(inverted ^ ForceInvert)) ? 1 : (-1));
			float num7 = bone.PosX - num5 * (Time.timeSinceLevelLoad % 600000f);
			float to = (float)num6 * num4 * Mathf.Sin(num7 * (float)Math.PI / num3);
			result += BraveMathCollege.DegreesToVector(bone.RotationAngle + 90f, Mathf.SmoothStep(0f, to, bone.PosX));
		}
		return result;
	}
}
