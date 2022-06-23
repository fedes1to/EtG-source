using System;
using System.Collections.Generic;
using UnityEngine;

public class GunHandController : BraveBehaviour
{
	[Serializable]
	public class DirectionalAnimationBoolSixWay
	{
		public bool Back;

		public bool BackRight;

		public bool ForwardRight;

		public bool Forward;

		public bool ForwardLeft;

		public bool BackLeft;

		public bool IsBehindBody(float angle)
		{
			if (angle <= 155f && angle >= 25f)
			{
				if (angle < 120f && angle >= 60f)
				{
					return Back;
				}
				return (!(Mathf.Abs(angle) < 90f)) ? BackRight : BackLeft;
			}
			if (angle <= -60f && angle >= -120f)
			{
				return Forward;
			}
			return (!(Mathf.Abs(angle) >= 90f)) ? ForwardRight : ForwardLeft;
		}
	}

	[Serializable]
	public class DirectionalAnimationBoolEightWay
	{
		public bool North;

		public bool NorthEast;

		public bool East;

		public bool SouthEast;

		public bool South;

		public bool SouthWest;

		public bool West;

		public bool NorthWest;

		public bool IsBehindBody(float angle)
		{
			angle = BraveMathCollege.ClampAngle360(angle);
			if (angle < 22.5f)
			{
				return East;
			}
			if (angle < 67.5f)
			{
				return NorthEast;
			}
			if (angle < 112.5f)
			{
				return North;
			}
			if (angle < 157.5f)
			{
				return NorthWest;
			}
			if (angle < 202.5f)
			{
				return West;
			}
			if (angle < 247.5f)
			{
				return SouthWest;
			}
			if (angle < 292.5f)
			{
				return South;
			}
			if (angle < 337.5f)
			{
				return SouthEast;
			}
			return East;
		}
	}

	[PickupIdentifier]
	[Header("Gun")]
	public int GunId = -1;

	public Projectile OverrideProjectile;

	public bool UsesOverrideProjectileData;

	public ProjectileData OverrideProjectileData;

	public GunHandController GunFlipMaster;

	[Header("Hands")]
	public PlayerHandController handObject;

	public bool isEightWay;

	public DirectionalAnimationBoolSixWay gunBehindBody;

	public DirectionalAnimationBoolEightWay gunBehindBodyEight;

	[Header("Shooting")]
	public float PreFireDelay;

	public int NumShots;

	public float ShotCooldown;

	public float Cooldown;

	public bool RampBullets;

	[ShowInInspectorIf("RampBullets", false)]
	public float RampStartHeight = 2f;

	[ShowInInspectorIf("RampBullets", false)]
	public float RampTime = 1f;

	private AIActor m_body;

	private Gun m_gun;

	private ProjectileData m_overrideProjectileData;

	private Transform m_gunAttachPoint;

	private float m_gunAngle;

	private bool m_gunFlipped;

	private bool m_isFiring;

	private int m_shotsFired;

	private float m_shotCooldown;

	private float m_cooldown;

	private Vector2 m_targetLocation;

	private List<PlayerHandController> m_attachedHands = new List<PlayerHandController>();

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
			return !m_isFiring && m_cooldown <= 0f;
		}
	}

	public void Start()
	{
		m_body = base.transform.parent.GetComponent<AIActor>();
		Transform transform = new GameObject("gun").transform;
		transform.parent = base.transform;
		transform.localPosition = Vector3.zero;
		m_gun = UnityEngine.Object.Instantiate(PickupObjectDatabase.GetById(GunId)) as Gun;
		m_gun.transform.parent = transform;
		m_gun.NoOwnerOverride = true;
		m_gun.Initialize(m_body);
		m_gun.gameObject.SetActive(true);
		m_gun.sprite.HeightOffGround = 0.05f;
		m_body.sprite.AttachRenderer(m_gun.sprite);
		if ((bool)handObject && (bool)m_gun)
		{
			PlayerHandController playerHandController = AttachNewHandToTransform(m_gun.PrimaryHandAttachPoint);
			m_body.healthHaver.RegisterBodySprite(playerHandController.sprite);
		}
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
		if (RampBullets)
		{
			m_gun.rampBullets = true;
			m_gun.rampStartHeight = RampStartHeight;
			m_gun.rampTime = RampTime;
		}
		SpriteOutlineManager.AddOutlineToSprite(m_gun.sprite, Color.black, 0.35f);
		m_cooldown = Cooldown;
	}

	public void Update()
	{
		float facingDirection = m_body.aiAnimator.FacingDirection;
		bool flag = ((!isEightWay) ? gunBehindBody.IsBehindBody(facingDirection) : gunBehindBodyEight.IsBehindBody(facingDirection));
		m_gun.sprite.HeightOffGround = ((!flag) ? 0.1f : (-0.2f));
		if ((bool)m_body.TargetRigidbody)
		{
			m_targetLocation = m_body.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		m_gunAngle = m_gun.HandleAimRotation(m_targetLocation);
		if ((bool)GunFlipMaster)
		{
			if (GunFlipMaster.m_gunFlipped != m_gunFlipped)
			{
				m_gun.HandleSpriteFlip(GunFlipMaster.m_gunFlipped);
				m_gunFlipped = GunFlipMaster.m_gunFlipped;
			}
		}
		else if (!m_gunFlipped && Mathf.Abs(m_gunAngle) > 105f)
		{
			m_gun.HandleSpriteFlip(true);
			m_gunFlipped = true;
		}
		else if (m_gunFlipped && Mathf.Abs(m_gunAngle) < 75f)
		{
			m_gun.HandleSpriteFlip(false);
			m_gunFlipped = false;
		}
		if (m_isFiring)
		{
			m_shotCooldown -= BraveTime.DeltaTime;
			if (m_shotCooldown <= 0f)
			{
				Fire();
				m_shotCooldown = ShotCooldown;
				if (m_shotsFired >= NumShots)
				{
					CeaseAttack();
					m_isFiring = false;
				}
			}
		}
		else
		{
			m_cooldown = Mathf.Max(0f, m_cooldown - BraveTime.DeltaTime);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void StartFiring()
	{
		m_isFiring = true;
		m_shotsFired = 0;
		if (PreFireDelay > 0f)
		{
			m_shotCooldown = PreFireDelay;
			return;
		}
		Fire();
		m_shotCooldown = ShotCooldown;
	}

	public void CeaseAttack()
	{
		if ((bool)m_gun)
		{
			m_gun.CeaseAttack();
		}
		m_cooldown = Cooldown;
	}

	private void Fire(float? angleOffset = null)
	{
		if ((bool)m_gun)
		{
			if (angleOffset.HasValue)
			{
				m_gun.DefaultModule.angleFromAim = angleOffset.Value;
				m_gun.DefaultModule.angleVariance = 0f;
				m_gun.DefaultModule.alternateAngle = false;
			}
			m_gun.CeaseAttack();
			m_gun.ClearCooldowns();
			m_gun.ClearReloadData();
			m_gun.Attack(m_overrideProjectileData);
			m_shotsFired++;
		}
	}

	private PlayerHandController AttachNewHandToTransform(Transform target)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(handObject.gameObject);
		gameObject.transform.parent = base.transform;
		PlayerHandController component = gameObject.GetComponent<PlayerHandController>();
		m_gun.GetSprite().AttachRenderer(component.sprite);
		component.attachPoint = target;
		m_attachedHands.Add(component);
		if ((bool)base.healthHaver)
		{
			tk2dSprite component2 = component.GetComponent<tk2dSprite>();
			base.healthHaver.RegisterBodySprite(component2);
		}
		return component;
	}
}
