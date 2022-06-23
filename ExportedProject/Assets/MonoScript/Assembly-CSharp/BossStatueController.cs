using System;
using System.Collections;
using System.Collections.Generic;
using FullInspector;
using UnityEngine;

public class BossStatueController : BaseBehavior<FullSerializerSerializer>
{
	public enum StatueState
	{
		HopToTarget,
		WaitForAttack,
		StandStill
	}

	[Serializable]
	public class LevelData
	{
		public string idleSprite;

		public string idleAnim;

		public string fireAnim;

		public string deathAnim;

		public GameObject EyeTrailVFX;
	}

	private const float c_maxHeightToBeGrounded = 1.5f;

	public tk2dBaseSprite shadowSprite;

	public tk2dSpriteAnimator landVfx;

	public tk2dSpriteAnimator attackVfx;

	public Transform shootPoint;

	public List<LevelData> levelData;

	public List<GameObject> transformVfx;

	public List<Transform> transformPoints;

	public float transformDelay = 0.5f;

	public float transformMidDelay = 1f;

	public string kaliTransformAnim;

	public Transform kaliExplosionTransform;

	public GameObject kaliExplosionVfx;

	public Transform kaliFireworksTransform;

	public GameObject kaliFireworkdsVfx;

	public float kaliPostTransformDelay = 1f;

	private BossStatuesController m_statuesController;

	private BulletScriptSource m_bulletScriptSource;

	private StatueState m_state;

	private int m_level;

	private Vector2? m_target;

	private float m_landTimer;

	private bool m_isAttacking;

	private float m_height;

	private float m_initialVelocity;

	private float m_gravity;

	private float m_totalAirTime;

	private Vector2 m_launchGroundPosition;

	private float m_airTimer;

	private float m_maxJumpHeight;

	private Vector2 m_shadowLocalPos;

	private Vector2 m_landVfxOffset;

	private Vector2 m_attackVfxOffset;

	private SpriteAnimatorKiller m_landVfxKiller;

	private SpriteAnimatorKiller m_attackVfxKiller;

	private GameObject m_currentEyeVfx;

	public LevelData CurrentLevel
	{
		get
		{
			return levelData[m_level];
		}
	}

	public Vector2? Target
	{
		get
		{
			return m_target;
		}
		set
		{
			m_target = value;
		}
	}

	public float DistancetoTarget
	{
		get
		{
			Vector2? target = m_target;
			if (!target.HasValue)
			{
				return 0f;
			}
			Vector2 b = base.specRigidbody.UnitCenter - new Vector2(0f, m_height);
			return Vector2.Distance(m_target.Value, b);
		}
	}

	public Vector2 Position
	{
		get
		{
			return base.specRigidbody.UnitCenter;
		}
	}

	public Vector2 GroundPosition
	{
		get
		{
			return base.specRigidbody.UnitCenter - new Vector2(0f, m_height);
		}
	}

	public bool IsKali
	{
		get
		{
			return m_level >= levelData.Count - 1;
		}
	}

	public bool IsGrounded { get; set; }

	public bool IsStomping { get; set; }

	public bool IsTransforming { get; set; }

	public bool ReadyToJump
	{
		get
		{
			return m_landTimer <= 0f;
		}
	}

	public float HangTime { get; set; }

	public List<BulletScriptSelector> QueuedBulletScript { get; set; }

	public bool SuppressShootVfx { get; set; }

	public StatueState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.aiActor.BehaviorOverridesVelocity = true;
		IsGrounded = true;
		QueuedBulletScript = new List<BulletScriptSelector>();
		base.specRigidbody.HitboxPixelCollider.CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox, CollisionLayer.PlayerBlocker);
	}

	public void Start()
	{
		m_statuesController = base.transform.parent.GetComponent<BossStatuesController>();
		m_maxJumpHeight = -0.5f * (m_statuesController.AttackHopSpeed * m_statuesController.AttackHopSpeed) / m_statuesController.AttackGravity;
		base.encounterTrackable = m_statuesController.encounterTrackable;
		m_shadowLocalPos = shadowSprite.transform.localPosition;
		m_landVfxOffset = base.specRigidbody.UnitCenter - landVfx.transform.position.XY();
		m_attackVfxOffset = base.specRigidbody.UnitCenter - attackVfx.transform.position.XY();
		m_landVfxKiller = landVfx.GetComponent<SpriteAnimatorKiller>();
		m_attackVfxKiller = attackVfx.GetComponent<SpriteAnimatorKiller>();
		landVfx.transform.parent = SpawnManager.Instance.VFX;
		attackVfx.transform.parent = SpawnManager.Instance.VFX;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(WallMovementResctrictor));
		base.bulletBank.CollidesWithEnemies = false;
		base.gameActor.PreventAutoAimVelocity = true;
	}

	public void Update()
	{
		Vector2? target = m_target;
		if (!target.HasValue)
		{
			return;
		}
		if ((bool)base.bulletBank)
		{
			PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(m_target.Value);
			if ((bool)activePlayerClosestToPoint)
			{
				base.bulletBank.FixedPlayerPosition = activePlayerClosestToPoint.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
		}
		base.specRigidbody.PixelColliders[0].Enabled = m_height < 1.5f;
		if (IsGrounded)
		{
			if (m_landTimer > 0f)
			{
				m_landTimer = Mathf.Max(0f, m_landTimer - BraveTime.DeltaTime);
				base.aiActor.BehaviorVelocity = Vector2.zero;
				return;
			}
			if (m_state == StatueState.StandStill)
			{
				return;
			}
			if (m_state == StatueState.WaitForAttack)
			{
				if (QueuedBulletScript.Count == 0)
				{
					return;
				}
				if (m_state == StatueState.WaitForAttack)
				{
					m_state = StatueState.HopToTarget;
				}
			}
			if (QueuedBulletScript.Count > 0)
			{
				m_initialVelocity = m_statuesController.AttackHopSpeed;
				m_gravity = m_statuesController.AttackGravity;
				m_totalAirTime = m_statuesController.attackHopTime;
				m_isAttacking = true;
			}
			else
			{
				m_initialVelocity = m_statuesController.MoveHopSpeed;
				m_gravity = m_statuesController.MoveGravity;
				m_totalAirTime = m_statuesController.moveHopTime;
			}
			IsGrounded = false;
			m_airTimer = 0f;
			m_launchGroundPosition = GroundPosition;
			AkSoundEngine.PostEvent("Play_ENM_statue_jump_01", base.gameObject);
		}
		m_airTimer += BraveTime.DeltaTime;
		float num = m_airTimer;
		Vector2 vector = Vector2.MoveTowards(GroundPosition, m_target.Value, m_statuesController.CurrentMoveSpeed * BraveTime.DeltaTime);
		if (IsStomping)
		{
			float num2 = m_airTimer / (m_totalAirTime / 2f);
			vector = ((!(num2 <= 1f)) ? GroundPosition : Vector2.Lerp(m_launchGroundPosition, Target.Value, num2));
			num = ((m_airTimer < m_totalAirTime / 2f) ? m_airTimer : ((!(m_airTimer < m_totalAirTime / 2f + HangTime)) ? (m_airTimer - HangTime) : (m_totalAirTime / 2f)));
		}
		m_height = m_initialVelocity * num + 0.5f * m_gravity * num * num;
		if (m_height <= 0f && !IsGrounded)
		{
			m_height = 0f;
			landVfx.gameObject.SetActive(true);
			m_landVfxKiller.Restart();
			landVfx.transform.position = vector - m_landVfxOffset;
			landVfx.sprite.UpdateZDepth();
			m_landTimer = m_statuesController.groundedTime;
			IsGrounded = true;
			if (m_isAttacking)
			{
				if (!SuppressShootVfx && QueuedBulletScript[0] != null && !QueuedBulletScript[0].IsNull)
				{
					attackVfx.gameObject.SetActive(true);
					m_attackVfxKiller.Restart();
					attackVfx.transform.position = vector - m_attackVfxOffset;
					attackVfx.sprite.UpdateZDepth();
				}
				if (QueuedBulletScript[0] != null && !QueuedBulletScript[0].IsNull)
				{
					ShootBulletScript(QueuedBulletScript[0]);
					base.spriteAnimator.Play(CurrentLevel.fireAnim);
				}
				QueuedBulletScript.RemoveAt(0);
				m_isAttacking = false;
			}
		}
		int frame = Mathf.RoundToInt((float)(shadowSprite.spriteAnimator.DefaultClip.frames.Length - 1) * Mathf.Clamp01(m_height / m_maxJumpHeight));
		shadowSprite.spriteAnimator.SetFrame(frame);
		shadowSprite.transform.localPosition = m_shadowLocalPos - new Vector2(0f, m_height);
		Vector2 vector2 = new Vector2(vector.x, vector.y + m_height);
		base.aiActor.BehaviorVelocity = (vector2 - base.specRigidbody.UnitCenter) / BraveTime.DeltaTime;
		base.sprite.HeightOffGround = m_height;
		base.sprite.UpdateZDepth();
	}

	protected override void OnDestroy()
	{
		if ((bool)landVfx)
		{
			UnityEngine.Object.Destroy(landVfx.gameObject);
		}
		if ((bool)attackVfx)
		{
			UnityEngine.Object.Destroy(attackVfx.gameObject);
		}
		base.OnDestroy();
	}

	public void LevelUp()
	{
		StartCoroutine(LevelUpCR());
	}

	private IEnumerator LevelUpCR()
	{
		IsTransforming = true;
		State = StatueState.StandStill;
		m_level++;
		while (!IsGrounded)
		{
			State = StatueState.StandStill;
			yield return null;
		}
		if (m_level >= 3)
		{
			base.spriteAnimator.Play(kaliTransformAnim);
			if (base.healthHaver.GetCurrentHealthPercentage() < 0.75f)
			{
				base.healthHaver.ForceSetCurrentHealth(0.75f * base.healthHaver.GetMaxHealth());
			}
			while (base.spriteAnimator.IsPlaying(kaliTransformAnim))
			{
				yield return null;
			}
			GameObject vfxObj2 = SpawnManager.SpawnVFX(kaliExplosionVfx, kaliExplosionTransform.position, Quaternion.identity);
			vfxObj2.transform.parent = kaliExplosionTransform;
			tk2dBaseSprite vfxSprite2 = vfxObj2.GetComponent<tk2dBaseSprite>();
			vfxSprite2.HeightOffGround = 0.2f;
			base.sprite.AttachRenderer(vfxSprite2);
			base.sprite.UpdateZDepth();
			vfxObj2 = SpawnManager.SpawnVFX(kaliFireworkdsVfx, kaliFireworksTransform.position, Quaternion.identity);
			vfxObj2.transform.parent = kaliFireworksTransform;
			vfxSprite2 = vfxObj2.GetComponent<tk2dBaseSprite>();
			vfxSprite2.HeightOffGround = 0.2f;
			base.sprite.AttachRenderer(vfxSprite2);
			base.sprite.UpdateZDepth();
			if (!string.IsNullOrEmpty(CurrentLevel.idleSprite))
			{
				base.sprite.SetSprite(CurrentLevel.idleSprite);
			}
			else
			{
				base.spriteAnimator.Play(CurrentLevel.idleAnim);
			}
			base.sprite.ForceUpdateMaterial();
			yield return new WaitForSeconds(kaliPostTransformDelay);
		}
		else
		{
			yield return new WaitForSeconds(transformDelay);
			for (int i = 0; i < transformPoints.Count && (m_level != 1 || i < 2); i++)
			{
				GameObject vfxPrefab = BraveUtility.RandomElement(transformVfx);
				GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, transformPoints[i].position, Quaternion.identity);
				vfxObj.transform.parent = transformPoints[i];
				tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
				vfxSprite.HeightOffGround = 0.2f;
				base.sprite.AttachRenderer(vfxSprite);
				base.sprite.UpdateZDepth();
				yield return new WaitForSeconds(transformMidDelay);
			}
			if (!string.IsNullOrEmpty(CurrentLevel.idleSprite))
			{
				base.sprite.SetSprite(CurrentLevel.idleSprite);
			}
			else
			{
				base.spriteAnimator.Play(CurrentLevel.idleAnim);
			}
		}
		if (m_currentEyeVfx != null)
		{
			UnityEngine.Object.Destroy(m_currentEyeVfx);
		}
		if (CurrentLevel.EyeTrailVFX != null)
		{
			m_currentEyeVfx = UnityEngine.Object.Instantiate(CurrentLevel.EyeTrailVFX);
			m_currentEyeVfx.transform.parent = base.transform;
			m_currentEyeVfx.transform.localPosition = new Vector3(0f, 0f, -20f);
			TrailRenderer[] componentsInChildren = m_currentEyeVfx.GetComponentsInChildren<TrailRenderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].sortingLayerName = "Foreground";
			}
		}
		if (IsKali)
		{
			base.spriteAnimator.Play(CurrentLevel.idleAnim);
			base.spriteAnimator.SetFrame(0);
			base.specRigidbody.ForceRegenerate();
		}
		IsTransforming = false;
	}

	public void ClearQueuedAttacks()
	{
		int num = (m_isAttacking ? 1 : 0);
		while (QueuedBulletScript.Count > num)
		{
			QueuedBulletScript.RemoveAt(QueuedBulletScript.Count - 1);
		}
	}

	public void FakeFireVFX()
	{
		AIBulletBank.Entry bullet = base.bulletBank.GetBullet();
		for (int i = 0; i < base.bulletBank.transforms.Count; i++)
		{
			Transform transform = base.bulletBank.transforms[i];
			bullet.MuzzleFlashEffects.SpawnAtLocalPosition(Vector3.zero, transform.localEulerAngles.z, transform);
		}
		if (bullet.PlayAudio)
		{
			if (!string.IsNullOrEmpty(bullet.AudioSwitch))
			{
				AkSoundEngine.SetSwitch("WPN_Guns", bullet.AudioSwitch, base.bulletBank.SoundChild);
				AkSoundEngine.PostEvent(bullet.AudioEvent, base.bulletBank.SoundChild);
			}
			else
			{
				AkSoundEngine.PostEvent(bullet.AudioEvent, base.gameObject);
			}
		}
	}

	public void ForceStopBulletScript()
	{
		if ((bool)m_bulletScriptSource)
		{
			UnityEngine.Object.Destroy(m_bulletScriptSource);
			m_bulletScriptSource = null;
		}
	}

	private void WallMovementResctrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		Func<IntVector2, bool> func = delegate(IntVector2 pixel)
		{
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(pixel);
			int x = (int)vector.x;
			int y = (int)vector.y;
			if (!GameManager.Instance.Dungeon.data.CheckInBounds(x, y))
			{
				return true;
			}
			if (GameManager.Instance.Dungeon.data.isWall(x, y))
			{
				return true;
			}
			return GameManager.Instance.Dungeon.data[x, y].isExitCell ? true : false;
		};
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider != null)
		{
			if (func(primaryPixelCollider.LowerLeft + pixelOffset))
			{
				validLocation = false;
			}
			else if (func(primaryPixelCollider.UpperRight + pixelOffset))
			{
				validLocation = false;
			}
		}
	}

	private void BeginState(StatueState state)
	{
	}

	private void EndState(StatueState state)
	{
	}

	private void ShootBulletScript(BulletScriptSelector bulletScript)
	{
		if (!m_bulletScriptSource)
		{
			m_bulletScriptSource = shootPoint.gameObject.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletScriptSource.BulletManager = base.bulletBank;
		m_bulletScriptSource.BulletScript = bulletScript;
		m_bulletScriptSource.Initialize();
	}
}
