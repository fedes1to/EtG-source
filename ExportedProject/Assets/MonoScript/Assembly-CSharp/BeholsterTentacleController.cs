using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BeholsterTentacleController : BraveBehaviour
{
	[Serializable]
	public class DirectionalAnimationBool
	{
		public bool Back;

		public bool BackRight;

		public bool ForwardRight;

		public bool Forward;

		public bool ForwardLeft;

		public bool BackLeft;
	}

	public DirectionalAnimation IdleAnimation;

	public DirectionalAnimation ShootAnimation;

	[PickupIdentifier]
	public int GunId;

	public Projectile OverrideProjectile;

	public bool UsesOverrideProjectileData;

	public ProjectileData OverrideProjectileData;

	public float FireTime;

	public float ShotCooldown;

	public float Cooldown;

	public DirectionalAnimationBool gunBehindTentacle;

	public DirectionalAnimationBool tentacleBehindBody;

	public bool RampBullets;

	[ShowInInspectorIf("RampBullets", false)]
	public float RampStartHeight = 2f;

	[ShowInInspectorIf("RampBullets", false)]
	public float RampTime = 1f;

	public bool SpawnBullets;

	[ShowInInspectorIf("SpawnBullets", false)]
	public float MinSpawnRadius;

	[ShowInInspectorIf("SpawnBullets", false)]
	public float MaxSpawnRadius;

	[ShowInInspectorIf("SpawnBullets", false)]
	public float MaxConcurrentAdds;

	private BulletScriptSource m_cachedBulletScriptSource;

	private BeholsterController m_body;

	private Gun m_gun;

	private ProjectileData m_overrideProjectileData;

	private Transform m_gunAttachPoint;

	private float m_gunAngle;

	private bool m_gunFlipped;

	private DirectionalAnimation m_currentAnimation;

	private float m_fireTimer;

	private float m_shotCooldown;

	private float m_cooldown;

	private Vector2 m_targetLocation;

	private List<SpawningProjectile> m_spawningProjectiles;

	public Gun Gun
	{
		get
		{
			return m_gun;
		}
	}

	public bool IsReady
	{
		get
		{
			if (SpawnBullets && (float)CurrentAdds >= MaxConcurrentAdds)
			{
				return false;
			}
			if ((bool)m_cachedBulletScriptSource && !m_cachedBulletScriptSource.IsEnded)
			{
				return false;
			}
			return m_fireTimer <= 0f && m_cooldown <= 0f;
		}
	}

	public int CurrentAdds
	{
		get
		{
			if (!SpawnBullets)
			{
				return 0;
			}
			int num = 0;
			m_spawningProjectiles.RemoveAll((SpawningProjectile p) => !p);
			num += m_spawningProjectiles.Count;
			List<AIActor> activeEnemies = m_body.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				num += activeEnemies.Count - 1;
			}
			return num;
		}
	}

	public BulletScriptSource BulletScriptSource
	{
		get
		{
			if (m_cachedBulletScriptSource == null)
			{
				m_cachedBulletScriptSource = m_gun.barrelOffset.gameObject.GetOrAddComponent<BulletScriptSource>();
			}
			return m_cachedBulletScriptSource;
		}
	}

	public void Start()
	{
		m_body = base.transform.parent.GetComponent<BeholsterController>();
		m_gunAttachPoint = base.transform.Find("gun");
		m_gun = UnityEngine.Object.Instantiate(PickupObjectDatabase.GetById(GunId)) as Gun;
		m_gun.transform.parent = m_gunAttachPoint;
		m_gun.NoOwnerOverride = true;
		m_gun.Initialize(m_body.aiActor);
		m_gun.gameObject.SetActive(true);
		m_gun.sprite.HeightOffGround = 0.05f;
		base.sprite.AttachRenderer(m_gun.sprite);
		if ((bool)OverrideProjectile)
		{
			List<Projectile> list = new List<Projectile>();
			list.Add(OverrideProjectile);
			m_gun.DefaultModule.projectiles = list;
		}
		if (UsesOverrideProjectileData)
		{
			m_overrideProjectileData = OverrideProjectileData;
		}
		else
		{
			m_overrideProjectileData = new ProjectileData(m_gun.singleModule.projectiles[0].baseData)
			{
				damage = 0.5f
			};
		}
		m_gun.ammo = int.MaxValue;
		m_gun.DefaultModule.numberOfShotsInClip = 0;
		m_gun.DefaultModule.usesOptionalFinalProjectile = false;
		if (RampBullets)
		{
			m_gun.rampBullets = true;
			m_gun.rampStartHeight = RampStartHeight;
			m_gun.rampTime = RampTime;
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.3f);
		SpriteOutlineManager.AddOutlineToSprite(m_gun.sprite, Color.black, 0.35f);
		m_body.healthHaver.RegisterBodySprite(base.sprite);
		m_cooldown = Cooldown;
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)delegate
		{
			if (base.enabled)
			{
				m_currentAnimation = null;
			}
		});
		if (SpawnBullets)
		{
			m_spawningProjectiles = new List<SpawningProjectile>();
		}
	}

	public void Update()
	{
		float facingDirection = m_body.aiAnimator.FacingDirection;
		DirectionalAnimation.Info info = null;
		if (m_currentAnimation != null)
		{
			info = m_currentAnimation.GetInfo(facingDirection);
			base.spriteAnimator.Play(base.spriteAnimator.GetClipByName(info.name), base.spriteAnimator.ClipTimeSeconds, base.spriteAnimator.ClipFps);
		}
		else
		{
			info = IdleAnimation.GetInfo(facingDirection);
			base.spriteAnimator.Play(info.name);
		}
		base.sprite.FlipX = info.flipped;
		bool flag = false;
		bool flag2 = false;
		if (facingDirection <= 155f && facingDirection >= 25f)
		{
			if (facingDirection < 120f && facingDirection >= 60f)
			{
				flag = tentacleBehindBody.Back;
				flag2 = gunBehindTentacle.Back;
			}
			else
			{
				flag = ((!(Mathf.Abs(facingDirection) < 90f)) ? tentacleBehindBody.BackRight : tentacleBehindBody.BackLeft);
				flag2 = ((!(Mathf.Abs(facingDirection) < 90f)) ? gunBehindTentacle.BackRight : gunBehindTentacle.BackLeft);
			}
		}
		else if (facingDirection <= -60f && facingDirection >= -120f)
		{
			flag = tentacleBehindBody.Forward;
			flag2 = gunBehindTentacle.Forward;
		}
		else
		{
			flag = ((!(Mathf.Abs(facingDirection) >= 90f)) ? tentacleBehindBody.ForwardRight : tentacleBehindBody.ForwardLeft);
			flag2 = ((!(Mathf.Abs(facingDirection) >= 90f)) ? gunBehindTentacle.ForwardRight : gunBehindTentacle.ForwardLeft);
		}
		base.sprite.HeightOffGround = ((!flag) ? 0.1f : (-0.1f));
		m_gun.sprite.HeightOffGround = ((!flag2) ? 0.05f : (-0.05f));
		if (m_body.LaserActive)
		{
			float f = m_body.aiAnimator.FacingDirection * ((float)Math.PI / 180f);
			m_targetLocation = m_body.LaserFiringCenter + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * 10f;
		}
		else if ((bool)m_body.aiActor.TargetRigidbody)
		{
			m_targetLocation = m_body.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		m_gunAngle = m_gun.HandleAimRotation(m_targetLocation);
		if (!m_gunFlipped && Mathf.Abs(m_gunAngle) > 105f)
		{
			m_gun.HandleSpriteFlip(true);
			m_gunFlipped = true;
		}
		else if (m_gunFlipped && Mathf.Abs(m_gunAngle) < 75f)
		{
			m_gun.HandleSpriteFlip(false);
			m_gunFlipped = false;
		}
		if (m_fireTimer > 0f)
		{
			m_fireTimer -= BraveTime.DeltaTime;
			if (m_fireTimer <= 0f || (SpawnBullets && (float)CurrentAdds >= MaxConcurrentAdds))
			{
				CeaseAttack();
				return;
			}
			if (ShotCooldown <= 0f)
			{
				m_gun.ContinueAttack(true, m_overrideProjectileData);
				return;
			}
			m_shotCooldown -= BraveTime.DeltaTime;
			if (m_shotCooldown <= 0f)
			{
				m_gun.CeaseAttack();
				Fire();
				m_shotCooldown = ShotCooldown;
			}
		}
		else
		{
			m_cooldown = Mathf.Max(0f, m_cooldown - BraveTime.DeltaTime);
		}
	}

	public void StartFiring()
	{
		Fire();
		m_fireTimer = FireTime;
		m_shotCooldown = ShotCooldown;
		m_cooldown = Cooldown;
		if (m_gun.singleModule.shootStyle == ProjectileModule.ShootStyle.SemiAutomatic)
		{
			Play(ShootAnimation);
		}
	}

	public void SingleFire(float? angleOffset = null)
	{
		m_gun.ClearCooldowns();
		Fire(angleOffset);
		m_cooldown = Cooldown;
		if (m_gun.singleModule.shootStyle == ProjectileModule.ShootStyle.SemiAutomatic)
		{
			Play(ShootAnimation);
		}
	}

	public void CeaseAttack()
	{
		m_gun.CeaseAttack();
		m_cooldown = Cooldown;
	}

	public void Play(DirectionalAnimation anim)
	{
		m_currentAnimation = anim;
		DirectionalAnimation.Info info = anim.GetInfo(m_body.aiAnimator.FacingDirection, true);
		base.sprite.FlipX = info.flipped;
		base.spriteAnimator.Play(info.name);
	}

	public void ShootBulletScript(BulletScriptSelector bulletScript)
	{
		m_body.bulletBank.rampBullets = RampBullets;
		m_body.bulletBank.rampStartHeight = RampStartHeight;
		m_body.bulletBank.rampTime = RampTime;
		m_body.bulletBank.OverrideGun = Gun;
		BulletScriptSource bulletScriptSource = BulletScriptSource;
		bulletScriptSource.BulletManager = m_body.bulletBank;
		bulletScriptSource.BulletScript = bulletScript;
		bulletScriptSource.Initialize();
	}

	private void Fire(float? angleOffset = null)
	{
		if (SpawnBullets && (bool)m_body.aiActor.TargetRigidbody)
		{
			Vector2 unitCenter = m_body.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			Vector2 unitCenter2 = m_body.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			float num = (unitCenter2 - unitCenter).ToAngle();
			unitCenter += BraveMathCollege.DegreesToVector(num + UnityEngine.Random.Range(-90f, 90f), UnityEngine.Random.Range(MinSpawnRadius, MaxSpawnRadius));
			m_gun.HandleAimRotation(unitCenter);
			m_gun.Attack(m_overrideProjectileData);
			if ((bool)m_gun.LastProjectile && m_gun.LastProjectile is SpawningProjectile)
			{
				SpawningProjectile spawningProjectile = m_gun.LastProjectile as SpawningProjectile;
				float magnitude = (unitCenter - spawningProjectile.transform.position.XY()).magnitude;
				float num2 = magnitude / spawningProjectile.baseData.speed;
				spawningProjectile.gravity = -2f * spawningProjectile.startingHeight / (num2 * num2);
				m_spawningProjectiles.Add(spawningProjectile);
				m_gun.LastProjectile.collidesWithPlayer = false;
				m_gun.LastProjectile.UpdateCollisionMask();
			}
		}
		else
		{
			if (angleOffset.HasValue)
			{
				m_gun.DefaultModule.angleFromAim = angleOffset.Value;
				m_gun.DefaultModule.angleVariance = 0f;
				m_gun.DefaultModule.alternateAngle = false;
			}
			m_gun.Attack(m_overrideProjectileData);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
