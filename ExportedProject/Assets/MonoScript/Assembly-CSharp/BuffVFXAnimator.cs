using System;
using UnityEngine;

public class BuffVFXAnimator : BraveBehaviour
{
	protected enum BuffAnimationStyle
	{
		CIRCLE,
		SWARM,
		PIERCE,
		TETRIS
	}

	[SerializeField]
	protected BuffAnimationStyle animationStyle;

	public float motionPeriod = 1f;

	public float ChanceOfApplication = 1f;

	public bool persistsOnDeath = true;

	public float AdditionalPierceDepth;

	public bool UsesVFXToSpawnOnDeath;

	public VFXPool VFXToSpawnOnDeath;

	public GameObject NonPoolVFX;

	public bool DoesSparks;

	public GlobalSparksModule SparksModule;

	[Header("Tetris")]
	public TetrisBuff.TetrisType tetrominoType;

	private bool m_initialized;

	private GameActor m_target;

	private Transform m_transform;

	private float m_elapsed;

	private float parametricStartPoint;

	private float m_pierceAngle;

	private Vector2 m_hitboxOriginOffset;

	private bool ForceFailure;

	private float m_sparksAccum;

	public void InitializeTetris(GameActor target, Vector2 sourceVec)
	{
		if (!(target == null))
		{
			parametricStartPoint = UnityEngine.Random.value;
			m_target = target;
			m_transform = base.transform;
			if (animationStyle == BuffAnimationStyle.PIERCE && (bool)m_target && (bool)m_target.specRigidbody)
			{
				m_hitboxOriginOffset = m_target.specRigidbody.HitboxPixelCollider.UnitCenter - m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
				m_pierceAngle = BraveMathCollege.Atan2Degrees(-sourceVec.normalized);
				m_hitboxOriginOffset += sourceVec.normalized * (base.sprite.GetBounds().extents.x * UnityEngine.Random.Range(0.15f, 0.5f));
				m_transform.position = (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset).ToVector3ZUp();
				m_transform.rotation = Quaternion.Euler(0f, 0f, m_pierceAngle);
			}
			m_initialized = true;
		}
	}

	public void InitializePierce(GameActor target, Vector2 sourceVec)
	{
		if (!(target == null))
		{
			parametricStartPoint = UnityEngine.Random.value;
			m_target = target;
			m_transform = base.transform;
			if (animationStyle == BuffAnimationStyle.PIERCE && (bool)m_target && (bool)m_target.specRigidbody)
			{
				m_hitboxOriginOffset = m_target.specRigidbody.HitboxPixelCollider.UnitCenter - m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
				m_pierceAngle = BraveMathCollege.Atan2Degrees(-sourceVec.normalized);
				m_hitboxOriginOffset += sourceVec.normalized * (base.sprite.GetBounds().extents.x * UnityEngine.Random.Range(0.15f, 0.5f));
				m_transform.position = (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset).ToVector3ZUp();
				m_transform.rotation = Quaternion.Euler(0f, 0f, m_pierceAngle);
			}
			m_initialized = true;
		}
	}

	public void Initialize(GameActor target)
	{
		if (target == null)
		{
			return;
		}
		parametricStartPoint = UnityEngine.Random.value;
		m_target = target;
		m_transform = base.transform;
		if (animationStyle == BuffAnimationStyle.PIERCE)
		{
			if ((bool)m_target && (bool)m_target.specRigidbody)
			{
				m_hitboxOriginOffset = new Vector2(UnityEngine.Random.Range(0f, m_target.specRigidbody.HitboxPixelCollider.UnitDimensions.x), UnityEngine.Random.Range(0f, m_target.specRigidbody.HitboxPixelCollider.UnitDimensions.y));
				m_pierceAngle = BraveMathCollege.Atan2Degrees(m_hitboxOriginOffset + m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft - m_target.specRigidbody.HitboxPixelCollider.UnitCenter);
				m_hitboxOriginOffset += (m_target.specRigidbody.HitboxPixelCollider.UnitCenter - (m_hitboxOriginOffset + m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft)).normalized * (base.sprite.GetBounds().extents.x * UnityEngine.Random.Range(0.15f + AdditionalPierceDepth, 0.5f + AdditionalPierceDepth));
				m_transform.position = (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset).ToVector3ZUp();
				m_transform.rotation = Quaternion.Euler(0f, 0f, m_pierceAngle);
			}
		}
		else if (animationStyle == BuffAnimationStyle.TETRIS && (bool)m_target && (bool)m_target.specRigidbody)
		{
			m_hitboxOriginOffset = new Vector2(UnityEngine.Random.Range(0f, m_target.specRigidbody.HitboxPixelCollider.UnitDimensions.x), UnityEngine.Random.Range(0f, m_target.specRigidbody.HitboxPixelCollider.UnitDimensions.y));
			m_pierceAngle = 90 * UnityEngine.Random.Range(0, 4);
			m_hitboxOriginOffset += (m_target.specRigidbody.HitboxPixelCollider.UnitCenter - (m_hitboxOriginOffset + m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft)).normalized * (base.sprite.GetBounds().extents.x * UnityEngine.Random.Range(0.15f + AdditionalPierceDepth, 0.5f + AdditionalPierceDepth));
			m_hitboxOriginOffset = m_hitboxOriginOffset.Quantize(0.375f);
			if (m_pierceAngle == 0f || m_pierceAngle == 180f)
			{
				Vector2 vector = base.sprite.GetBounds().extents.XY();
				m_hitboxOriginOffset -= vector;
			}
			else
			{
				Vector2 vector2 = base.sprite.GetBounds().extents.XY();
				vector2 = new Vector2(vector2.y, vector2.x);
				m_hitboxOriginOffset -= vector2;
			}
			m_hitboxOriginOffset += new Vector2(0.375f, 0.375f);
			m_transform.position = (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset).ToVector3ZUp();
			m_transform.rotation = Quaternion.Euler(0f, 0f, m_pierceAngle);
			base.sprite.HeightOffGround = UnityEngine.Random.Range(0f, 3f).Quantize(0.05f);
			base.sprite.IsPerpendicular = true;
			base.sprite.UpdateZDepth();
		}
		if (UnityEngine.Random.value > ChanceOfApplication)
		{
			ForceFailure = true;
		}
		m_initialized = true;
	}

	public void ForceDrop()
	{
		ForceFailure = true;
	}

	public void ClearData()
	{
		m_initialized = false;
		m_target = null;
		ForceFailure = false;
	}

	private void OnDespawned()
	{
		m_initialized = false;
		m_target = null;
	}

	private void Update()
	{
		if (m_initialized)
		{
			if (!m_target || m_target.healthHaver.IsDead || ForceFailure)
			{
				if (UsesVFXToSpawnOnDeath && (bool)m_target && (bool)m_target.specRigidbody)
				{
					VFXToSpawnOnDeath.SpawnAtPosition(base.transform.position, 0f, null, Vector2.zero, m_target.specRigidbody.HitboxPixelCollider.UnitCenter - (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset), 1f);
					if ((bool)NonPoolVFX)
					{
						GameObject gameObject = SpawnManager.SpawnVFX(NonPoolVFX, base.transform.position, base.transform.rotation);
						Vector2 vector = m_target.specRigidbody.HitboxPixelCollider.UnitCenter - (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset);
						DebrisObject component = gameObject.GetComponent<DebrisObject>();
						if ((bool)component)
						{
							tk2dBaseSprite tk2dBaseSprite2 = component.sprite;
							tk2dBaseSprite2.IsPerpendicular = false;
							tk2dBaseSprite2.usesOverrideMaterial = true;
							Vector2 normalized = (-vector).normalized;
							float z = UnityEngine.Random.Range(-20f, 20f);
							normalized = Quaternion.Euler(0f, 0f, z) * normalized * 10f;
							component.Trigger(normalized.ToVector3ZUp(0.1f), 2f);
						}
					}
				}
				SpawnManager.Despawn(base.gameObject);
				return;
			}
			m_elapsed += BraveTime.DeltaTime;
			float num = m_elapsed / motionPeriod + parametricStartPoint;
			Vector3 vector2 = m_transform.position;
			if (DoesSparks)
			{
				m_sparksAccum += BraveTime.DeltaTime * SparksModule.RatePerSecond;
				if (m_sparksAccum > 0f)
				{
					int num2 = Mathf.FloorToInt(m_sparksAccum);
					m_sparksAccum -= num2;
					int num3 = num2;
					Vector3 minPosition = vector2;
					Vector3 maxPosition = vector2;
					Vector3 up = Vector3.up;
					float angleVariance = 30f;
					float magnitudeVariance = 0.5f;
					float? startSize = 0.1f;
					float? startLifetime = 1f;
					GlobalSparksDoer.SparksType sparksType = SparksModule.sparksType;
					GlobalSparksDoer.DoRandomParticleBurst(num3, minPosition, maxPosition, up, angleVariance, magnitudeVariance, startSize, startLifetime, null, sparksType);
				}
			}
			switch (animationStyle)
			{
			case BuffAnimationStyle.CIRCLE:
				vector2 = m_target.specRigidbody.UnitCenter.ToVector3ZUp() + Quaternion.Euler(0f, 0f, num * 360f) * Vector3.right;
				break;
			case BuffAnimationStyle.SWARM:
				num *= (float)Math.PI;
				vector2 = m_target.specRigidbody.UnitCenter.ToVector3ZUp() + new Vector3(Mathf.Sin(num) + 2f * Mathf.Sin(2f * num), Mathf.Cos(num) - 2f * Mathf.Cos(2f * num), 0f) / 2f;
				break;
			case BuffAnimationStyle.PIERCE:
				m_transform.position = (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset).ToVector3ZUp();
				return;
			case BuffAnimationStyle.TETRIS:
				m_transform.position = (m_target.specRigidbody.HitboxPixelCollider.UnitBottomLeft + m_hitboxOriginOffset).ToVector3ZUp();
				return;
			}
			m_transform.position = vector2;
		}
		else
		{
			SpawnManager.Despawn(base.gameObject);
		}
	}
}
