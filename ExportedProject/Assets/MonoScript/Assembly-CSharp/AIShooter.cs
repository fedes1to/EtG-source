using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AIBulletBank))]
public class AIShooter : BraveBehaviour
{
	public ProjectileVolleyData volley;

	[HideInInspectorIf("volley", false)]
	[PickupIdentifier]
	public int equippedGunId = -1;

	[HideInInspectorIf("volley", false)]
	public bool shouldUseGunReload;

	[ShowInInspectorIf("volley", true)]
	public Transform volleyShootPosition;

	[ShowInInspectorIf("volley", true)]
	public GameObject volleyShellCasing;

	[ShowInInspectorIf("volley", true)]
	public Transform volleyShellTransform;

	[ShowInInspectorIf("volley", true)]
	public GameObject volleyShootVfx;

	[ShowInInspectorIf("volley", true)]
	public bool usesOctantShootVFX = true;

	[Header("Bullet Properties")]
	public string bulletName = "default";

	public float customShootCooldownPeriod;

	public bool doesScreenShake;

	public bool rampBullets;

	[ShowInInspectorIf("rampBullets", false)]
	public float rampStartHeight = 2f;

	[ShowInInspectorIf("rampBullets", false)]
	public float rampTime = 1f;

	[Header("Hands")]
	public Transform gunAttachPoint;

	[FormerlySerializedAs("bulletMLAttachPoint")]
	public Transform bulletScriptAttachPoint;

	public IntVector2 overallGunAttachOffset;

	public IntVector2 flippedGunAttachOffset;

	public PlayerHandController handObject;

	public bool AllowTwoHands;

	public bool ForceGunOnTop;

	public bool IsReallyBigBoy;

	public bool BackupAimInMoveDirection;

	public Action<Projectile> PostProcessProjectile;

	private BulletScriptSource m_cachedBraveBulletSource;

	private float m_aimTimeScale = 1f;

	private bool m_hasCachedGun;

	private Gun m_cachedGun;

	private OverridableBool m_hideGunRenderers = new OverridableBool(false);

	private OverridableBool m_hideHandRenderers = new OverridableBool(false);

	private Vector3 attachPointCachedPosition;

	private Vector3 attachPointCachedFlippedPosition;

	private bool m_onCooldown;

	private GunInventory m_inventory;

	private List<PlayerHandController> m_attachedHands = new List<PlayerHandController>();

	public bool CanShootOtherEnemies
	{
		get
		{
			return base.aiActor.CanTargetEnemies;
		}
	}

	public Vector2? OverrideAimPoint { get; set; }

	public Vector2? OverrideAimDirection
	{
		get
		{
			if (!OverrideAimPoint.HasValue)
			{
				return null;
			}
			return (OverrideAimPoint.Value - base.aiActor.CenterPosition).normalized;
		}
		set
		{
			Vector2? overrideAimPoint;
			if (!value.HasValue)
			{
				overrideAimPoint = null;
			}
			else
			{
				Vector2? vector = value * 5f;
				overrideAimPoint = ((!vector.HasValue) ? null : new Vector2?(base.aiActor.CenterPosition + vector.GetValueOrDefault()));
			}
			OverrideAimPoint = overrideAimPoint;
		}
	}

	public GunInventory Inventory
	{
		get
		{
			return m_inventory;
		}
	}

	public Transform BulletSourceTransform
	{
		get
		{
			if ((bool)bulletScriptAttachPoint)
			{
				return bulletScriptAttachPoint;
			}
			if ((bool)CurrentGun)
			{
				return CurrentGun.barrelOffset;
			}
			if ((bool)volley && (bool)volleyShootPosition)
			{
				return volleyShootPosition;
			}
			if ((bool)gunAttachPoint)
			{
				return gunAttachPoint;
			}
			return base.transform;
		}
	}

	public BulletScriptSource BraveBulletSource
	{
		get
		{
			if (m_cachedBraveBulletSource == null)
			{
				m_cachedBraveBulletSource = BulletSourceTransform.gameObject.GetOrAddComponent<BulletScriptSource>();
			}
			return m_cachedBraveBulletSource;
		}
	}

	public float AimTimeScale
	{
		get
		{
			if ((bool)base.aiActor)
			{
				return m_aimTimeScale * base.aiActor.LocalTimeScale;
			}
			return m_aimTimeScale;
		}
		set
		{
			m_aimTimeScale = value;
		}
	}

	public Gun EquippedGun
	{
		get
		{
			if (!m_hasCachedGun)
			{
				if (equippedGunId >= 0)
				{
					m_cachedGun = PickupObjectDatabase.GetById(equippedGunId) as Gun;
				}
				m_hasCachedGun = true;
			}
			return m_cachedGun;
		}
	}

	public bool IsPreFireComplete
	{
		get
		{
			if (!CurrentGun || string.IsNullOrEmpty(CurrentGun.enemyPreFireAnimation))
			{
				return true;
			}
			return !CurrentGun.spriteAnimator.IsPlaying(CurrentGun.enemyPreFireAnimation);
		}
	}

	public bool OnCooldown
	{
		get
		{
			return m_onCooldown;
		}
	}

	public bool ManualGunAngle { get; set; }

	public float GunAngle { get; set; }

	public Gun CurrentGun
	{
		get
		{
			if (m_inventory == null)
			{
				return null;
			}
			return m_inventory.CurrentGun;
		}
	}

	public void Start()
	{
		base.healthHaver.OnPreDeath += OnPreDeath;
	}

	private void Update()
	{
		if (base.aiActor.HasBeenEngaged)
		{
			HandleGunFlipping();
		}
		if (base.aiActor.IsFalling && m_cachedBraveBulletSource != null)
		{
			m_cachedBraveBulletSource.enabled = false;
		}
		if (BackupAimInMoveDirection && !base.aiActor.TargetRigidbody)
		{
			AimInDirection(BraveMathCollege.DegreesToVector(base.aiActor.FacingDirection));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ShootInDirection(Vector2 direction, string overrideBulletName = null)
	{
		if (!base.healthHaver.IsDead)
		{
			if (EquippedGun == null && volley != null)
			{
				ShootInDirection(direction, volley, overrideBulletName);
			}
			else if (EquippedGun != null && m_inventory.CurrentGun != null)
			{
				AimInDirection(direction);
				Shoot(overrideBulletName);
				m_inventory.CurrentGun.ClearCooldowns();
				m_inventory.CurrentGun.ClearReloadData();
			}
		}
	}

	public void ShootAtTarget(string overrideBulletName = null)
	{
		if (base.healthHaver.IsDead)
		{
			return;
		}
		if (!base.aiActor.OverrideTarget)
		{
			base.aiActor.OverrideTarget = null;
		}
		if (base.aiActor.TargetRigidbody == null)
		{
			return;
		}
		if (EquippedGun == null && volley != null)
		{
			ShootAtTarget(volley, overrideBulletName);
		}
		else if (EquippedGun != null && m_inventory.CurrentGun != null)
		{
			AimAtPoint(base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox));
			Shoot(overrideBulletName);
			if (m_inventory.CurrentGun != null)
			{
				m_inventory.CurrentGun.ClearCooldowns();
				m_inventory.CurrentGun.ClearReloadData();
			}
		}
	}

	public void CeaseAttack()
	{
		if (m_inventory != null && (bool)m_inventory.CurrentGun)
		{
			m_inventory.CurrentGun.CeaseAttack();
		}
	}

	public void Reload()
	{
		if (m_inventory.CurrentGun != null)
		{
			m_inventory.CurrentGun.Reload();
		}
	}

	public void ShootBulletScript(BulletScriptSelector bulletScript)
	{
		BulletScriptSource braveBulletSource = BraveBulletSource;
		braveBulletSource.BulletManager = base.bulletBank;
		braveBulletSource.BulletScript = bulletScript;
		braveBulletSource.Initialize();
	}

	public AIBulletBank.Entry GetBulletEntry(string overrideBulletName = null)
	{
		string text = (string.IsNullOrEmpty(overrideBulletName) ? bulletName : overrideBulletName);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		AIBulletBank.Entry entry = null;
		List<AIBulletBank.Entry> bullets = base.bulletBank.Bullets;
		for (int i = 0; i < bullets.Count; i++)
		{
			if (bullets[i].Name == text)
			{
				entry = bullets[i];
				break;
			}
		}
		if (entry == null)
		{
			Debug.LogError(string.Format("Unknown bullet type {0} on {1}", base.transform.name, text), base.gameObject);
			return null;
		}
		return entry;
	}

	private void OnPreDeath(Vector2 obj)
	{
		if (m_cachedBraveBulletSource != null)
		{
			m_cachedBraveBulletSource.enabled = false;
		}
		ToggleGunAndHandRenderers(false, "OnPreDeath");
	}

	public void ToggleGunRenderers(bool value, string reason)
	{
		if (string.IsNullOrEmpty(reason))
		{
			m_hideGunRenderers.BaseValue = !value;
			if (value)
			{
				m_hideGunRenderers.ClearOverrides();
			}
		}
		else
		{
			m_hideGunRenderers.SetOverride(reason, !value);
		}
		UpdateGunRenderers();
	}

	public void UpdateGunRenderers()
	{
		bool value = !m_hideGunRenderers.Value;
		if (CurrentGun != null)
		{
			CurrentGun.ToggleRenderers(value);
		}
	}

	public void ToggleHandRenderers(bool value, string reason)
	{
		if (string.IsNullOrEmpty(reason))
		{
			m_hideHandRenderers.BaseValue = !value;
			if (value)
			{
				m_hideHandRenderers.ClearOverrides();
			}
		}
		else
		{
			m_hideHandRenderers.SetOverride(reason, !value);
		}
		UpdateHandRenderers();
	}

	public void UpdateHandRenderers()
	{
		bool flag = !m_hideHandRenderers.Value;
		for (int i = 0; i < m_attachedHands.Count; i++)
		{
			m_attachedHands[i].ForceRenderersOff = !flag;
		}
	}

	public void ToggleGunAndHandRenderers(bool value, string reason)
	{
		ToggleGunRenderers(value, reason);
		ToggleHandRenderers(value, reason);
	}

	public void StartPreFireAnim()
	{
		if ((bool)CurrentGun && !string.IsNullOrEmpty(CurrentGun.enemyPreFireAnimation))
		{
			CurrentGun.spriteAnimator.Play(CurrentGun.enemyPreFireAnimation);
		}
	}

	protected void ShootAtTarget(ProjectileVolleyData volley, string overrideBulletName = null)
	{
		if (base.aiActor.TargetRigidbody == null)
		{
			return;
		}
		for (int i = 0; i < volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = volley.projectiles[i];
			float angleForShot = projectileModule.GetAngleForShot();
			ShootAtTarget(projectileModule, overrideBulletName, projectileModule.positionOffset, angleForShot);
			if (projectileModule.mirror)
			{
				ShootAtTarget(projectileModule, overrideBulletName, projectileModule.InversePositionOffset, 0f - angleForShot);
			}
			projectileModule.IncrementShootCount();
		}
	}

	protected void ShootAtTarget(ProjectileModule projectileModule, string overrideBulletName, Vector3 positionOffset, float angleOffset)
	{
		if (!(base.aiActor.TargetRigidbody == null))
		{
			Vector2 unitCenter = base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			Vector2 vector = unitCenter - volleyShootPosition.position.XY();
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			GameObject bulletObject = projectileModule.GetCurrentProjectile().gameObject;
			AIBulletBank.Entry bulletEntry = GetBulletEntry(overrideBulletName);
			if (bulletEntry != null && (bool)bulletEntry.BulletObject)
			{
				bulletObject = bulletEntry.BulletObject;
			}
			GameObject gameObject = SpawnManager.SpawnProjectile(bulletObject, volleyShootPosition.position + Quaternion.Euler(0f, 0f, num) * positionOffset, Quaternion.Euler(0f, 0f, num + angleOffset));
			Projectile component = gameObject.GetComponent<Projectile>();
			if (bulletEntry != null && bulletEntry.OverrideProjectile)
			{
				component.baseData.SetAll(bulletEntry.ProjectileData);
			}
			if ((bool)base.aiActor && base.aiActor.IsBlackPhantom)
			{
				component.baseData.speed *= base.aiActor.BlackPhantomProperties.BulletSpeedMultiplier;
			}
			component.collidesWithEnemies = base.aiActor.CanTargetEnemies;
			component.collidesWithPlayer = base.aiActor.CanTargetPlayers;
			component.Shooter = base.specRigidbody;
			if (PostProcessProjectile != null)
			{
				PostProcessProjectile(component);
			}
		}
	}

	protected void ShootInDirection(Vector2 direction, ProjectileVolleyData volley, string overrideBulletName = null)
	{
		for (int i = 0; i < volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = volley.projectiles[i];
			float angleForShot = projectileModule.GetAngleForShot();
			ShootInDirection(direction, projectileModule, overrideBulletName, projectileModule.positionOffset, angleForShot);
			if (projectileModule.mirror)
			{
				ShootInDirection(direction, projectileModule, overrideBulletName, projectileModule.InversePositionOffset, 0f - angleForShot);
			}
			projectileModule.IncrementShootCount();
		}
		SpawnVolleyShellCasing((!(volleyShellTransform != null)) ? volleyShootPosition.position : volleyShellTransform.position);
		if ((bool)gunAttachPoint && (bool)volleyShellTransform && (bool)volleyShootVfx)
		{
			if (usesOctantShootVFX)
			{
				int num = BraveMathCollege.VectorToOctant(volleyShootPosition.position - volleyShellTransform.position);
				GameObject gameObject = SpawnManager.SpawnVFX(volleyShootVfx, volleyShootPosition.position, Quaternion.Euler(0f, 0f, 90 + num * -45));
				tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
				component.HeightOffGround = 2f;
				base.sprite.AttachRenderer(component);
				component.IsPerpendicular = true;
				component.usesOverrideMaterial = true;
			}
			else
			{
				GameObject gameObject2 = SpawnManager.SpawnVFX(volleyShootVfx, volleyShootPosition.position, Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(direction)));
				tk2dSprite component2 = gameObject2.GetComponent<tk2dSprite>();
				component2.HeightOffGround = 2f;
				base.sprite.AttachRenderer(component2);
				component2.IsPerpendicular = true;
				component2.usesOverrideMaterial = true;
			}
		}
		AkSoundEngine.PostEvent("Play_ANM_Gull_Shoot_01", base.gameObject);
	}

	protected void ShootInDirection(Vector2 direction, ProjectileModule projectileModule, string overrideBulletName, Vector3 positionOffset, float angleOffset)
	{
		float num = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
		GameObject bulletObject = projectileModule.GetCurrentProjectile().gameObject;
		AIBulletBank.Entry bulletEntry = GetBulletEntry(overrideBulletName);
		if (bulletEntry != null && (bool)bulletEntry.BulletObject)
		{
			bulletObject = bulletEntry.BulletObject;
		}
		GameObject gameObject = SpawnManager.SpawnProjectile(bulletObject, volleyShootPosition.position + Quaternion.Euler(0f, 0f, num) * positionOffset, Quaternion.Euler(0f, 0f, num + angleOffset));
		Projectile component = gameObject.GetComponent<Projectile>();
		if (bulletEntry != null && bulletEntry.OverrideProjectile)
		{
			component.baseData.SetAll(bulletEntry.ProjectileData);
		}
		if ((bool)base.aiActor && base.aiActor.IsBlackPhantom)
		{
			component.baseData.speed *= base.aiActor.BlackPhantomProperties.BulletSpeedMultiplier;
		}
		component.Shooter = base.specRigidbody;
		if ((bool)base.aiActor)
		{
			component.SetOwnerSafe(base.aiActor, base.aiActor.GetActorName());
		}
		if (PostProcessProjectile != null)
		{
			PostProcessProjectile(component);
		}
	}

	protected void SpawnVolleyShellCasing(Vector3 position)
	{
		if (volleyShellCasing != null)
		{
			GameObject gameObject = SpawnManager.SpawnDebris(volleyShellCasing, position, Quaternion.identity);
			ShellCasing component = gameObject.GetComponent<ShellCasing>();
			if (component != null)
			{
				component.Trigger();
			}
			DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
			if (component2 != null)
			{
				int num = ((component2.transform.right.x > 0f) ? 1 : (-1));
				Vector3 vector = Vector3.up * (UnityEngine.Random.value * 1.5f + 1f) + -1.5f * Vector3.right * num * (UnityEngine.Random.value + 1.5f);
				Vector3 startingForce = new Vector3(vector.x, 0f, vector.y);
				float y = base.transform.position.y;
				float startingHeight = component2.transform.position.y - y + UnityEngine.Random.value * 0.5f;
				component2.Trigger(startingForce, startingHeight);
			}
		}
	}

	public void Initialize()
	{
		m_inventory = new GunInventory(base.aiActor);
		if (EquippedGun != null)
		{
			m_inventory.AddGunToInventory(EquippedGun, true);
			if (CurrentGun.singleModule != null && CurrentGun.singleModule.shootStyle == ProjectileModule.ShootStyle.Burst)
			{
				CurrentGun.singleModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
			}
			CurrentGun.doesScreenShake = doesScreenShake;
			CurrentGun.ammo = int.MaxValue;
			SpriteOutlineManager.AddOutlineToSprite(CurrentGun.GetSprite(), Color.black, 0.1f, 0.05f);
			base.sprite.AttachRenderer(CurrentGun.GetSprite());
		}
		Bounds untrimmedBounds = base.sprite.GetUntrimmedBounds();
		attachPointCachedPosition = gunAttachPoint.localPosition + (Vector3)PhysicsEngine.PixelToUnit(overallGunAttachOffset);
		attachPointCachedFlippedPosition = gunAttachPoint.localPosition.WithX(untrimmedBounds.center.x + (untrimmedBounds.center.x - gunAttachPoint.localPosition.x)) + (Vector3)PhysicsEngine.PixelToUnit(flippedGunAttachOffset) + (Vector3)PhysicsEngine.PixelToUnit(overallGunAttachOffset);
		if (handObject != null)
		{
			if (CurrentGun != null && CurrentGun.Handedness == GunHandedness.OneHanded)
			{
				AttachNewHandToTransform(CurrentGun.PrimaryHandAttachPoint);
			}
			else if (CurrentGun != null && CurrentGun.Handedness == GunHandedness.TwoHanded && AllowTwoHands)
			{
				AttachNewHandToTransform(CurrentGun.PrimaryHandAttachPoint);
				AttachNewHandToTransform(CurrentGun.SecondaryHandAttachPoint);
			}
		}
		AimAtPoint(gunAttachPoint.position + BraveUtility.RandomSign() * new Vector3(5f, 0f, 0f));
	}

	protected void AttachNewHandToTransform(Transform target)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(handObject.gameObject);
		gameObject.transform.parent = base.transform;
		PlayerHandController component = gameObject.GetComponent<PlayerHandController>();
		CurrentGun.GetSprite().AttachRenderer(component.sprite);
		component.attachPoint = target;
		m_attachedHands.Add(component);
		component.ForceRenderersOff = !base.renderer.enabled;
		if ((bool)base.healthHaver)
		{
			tk2dSprite component2 = component.GetComponent<tk2dSprite>();
			base.healthHaver.RegisterBodySprite(component2);
		}
	}

	public void AimAtOverride()
	{
		Gun currentGun = m_inventory.CurrentGun;
		if (!(currentGun == null))
		{
			GunAngle = currentGun.HandleAimRotation(OverrideAimPoint.Value);
			HandleGunFlipping();
		}
	}

	public void AimAtTarget()
	{
		if (!(base.aiActor.TargetRigidbody == null) || OverrideAimPoint.HasValue)
		{
			if (OverrideAimPoint.HasValue)
			{
				AimAtOverride();
			}
			else
			{
				AimAtPoint(base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox));
			}
		}
	}

	public void AimAtPoint(Vector2 point)
	{
		if (OverrideAimPoint.HasValue)
		{
			AimAtOverride();
		}
		else
		{
			if (m_inventory == null)
			{
				return;
			}
			Gun currentGun = m_inventory.CurrentGun;
			if (!(currentGun == null))
			{
				float num = ((!IsReallyBigBoy) ? 5f : 10f);
				if (Vector2.Distance(base.specRigidbody.UnitCenter, point) < num)
				{
					point = (point - base.specRigidbody.UnitCenter).normalized * num + base.specRigidbody.UnitCenter;
				}
				GunAngle = currentGun.HandleAimRotation(point, true, AimTimeScale);
				HandleGunFlipping();
			}
		}
	}

	public void AimInDirection(Vector2 direction)
	{
		if (OverrideAimPoint.HasValue)
		{
			AimAtOverride();
			return;
		}
		Vector3 vector = base.aiActor.CenterPosition + direction * 5f;
		AimAtPoint(vector);
	}

	public void Shoot(string overrideBulletName = null)
	{
		if (EquippedGun == null && volley != null)
		{
			for (int i = 0; i < volley.projectiles.Count; i++)
			{
				ProjectileModule projectileModule = volley.projectiles[i];
				float angleForShot = projectileModule.GetAngleForShot();
				ShootAtTarget(projectileModule, overrideBulletName, projectileModule.positionOffset, angleForShot);
				if (projectileModule.mirror)
				{
					ShootAtTarget(projectileModule, overrideBulletName, projectileModule.InversePositionOffset, 0f - angleForShot);
				}
				projectileModule.IncrementShootCount();
			}
		}
		else if (EquippedGun != null && m_inventory.CurrentGun != null)
		{
			Gun currentGun = m_inventory.CurrentGun;
			AIBulletBank.Entry bulletEntry = GetBulletEntry(overrideBulletName);
			if (PostProcessProjectile != null)
			{
				currentGun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(currentGun.PostProcessProjectile, PostProcessProjectile);
			}
			if (bulletEntry != null)
			{
				currentGun.Attack((!bulletEntry.OverrideProjectile) ? null : bulletEntry.ProjectileData, bulletEntry.BulletObject);
			}
			else
			{
				currentGun.Attack();
			}
			currentGun.PostProcessProjectile = (Action<Projectile>)Delegate.Remove(currentGun.PostProcessProjectile, PostProcessProjectile);
			if (m_inventory.CurrentGun != null)
			{
				m_inventory.CurrentGun.ClearCooldowns();
				m_inventory.CurrentGun.ClearReloadData();
			}
		}
	}

	public void ContinueShoot(Vector3 targetPosition)
	{
		AimAtPoint(targetPosition);
		m_inventory.CurrentGun.ContinueAttack();
	}

	public void ShootAtTarget(Vector3 targetPosition)
	{
		if (volley == null)
		{
			AimAtPoint(targetPosition);
			Shoot();
			m_inventory.CurrentGun.ClearCooldowns();
			if (!shouldUseGunReload)
			{
				m_inventory.CurrentGun.ClearReloadData();
			}
		}
		else
		{
			ShootVolleyAtTarget(targetPosition);
		}
	}

	public void Cooldown()
	{
		float t = ((!(volley == null)) ? volley.projectiles[0].cooldownTime : m_inventory.CurrentGun.GetPrimaryCooldown());
		if (customShootCooldownPeriod > 0f)
		{
			t = customShootCooldownPeriod;
		}
		StartCoroutine(HandleFireRate(t));
	}

	public void Cooldown(float t)
	{
		StartCoroutine(HandleFireRate(t));
	}

	private void HandleGunFlipping()
	{
		if (CurrentGun == null)
		{
			return;
		}
		if (Mathf.Abs(GunAngle) > 105f)
		{
			gunAttachPoint.localPosition = attachPointCachedPosition;
			CurrentGun.HandleSpriteFlip(true);
		}
		else if (Mathf.Abs(GunAngle) < 75f)
		{
			gunAttachPoint.localPosition = attachPointCachedFlippedPosition;
			CurrentGun.HandleSpriteFlip(false);
		}
		if (CurrentGun != null)
		{
			tk2dBaseSprite tk2dBaseSprite2 = CurrentGun.GetSprite();
			if (!ForceGunOnTop && GunAngle > 0f && GunAngle <= 155f && GunAngle >= 25f)
			{
				if (!CurrentGun.forceFlat)
				{
					tk2dBaseSprite2.HeightOffGround = -0.075f;
				}
				for (int i = 0; i < m_attachedHands.Count; i++)
				{
					m_attachedHands[i].handHeightFromGun = 0.05f;
					m_attachedHands[i].sprite.HeightOffGround = 0.05f;
				}
			}
			else
			{
				float heightOffGround = ((CurrentGun.Handedness != GunHandedness.TwoHanded) ? (-0.075f) : 0.875f);
				if (!CurrentGun.forceFlat)
				{
					tk2dBaseSprite2.HeightOffGround = heightOffGround;
				}
				for (int j = 0; j < m_attachedHands.Count; j++)
				{
					m_attachedHands[j].handHeightFromGun = 0.15f;
					m_attachedHands[j].sprite.HeightOffGround = 0.15f;
				}
			}
		}
		base.sprite.UpdateZDepth();
	}

	private void ShootVolleyAtTarget(Vector3 targetPosition)
	{
		Vector3 vector = targetPosition.XY() - base.aiActor.CenterPosition;
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		for (int i = 0; i < volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = volley.projectiles[i];
			float angleForShot = projectileModule.GetAngleForShot();
			GameObject bulletObject = projectileModule.GetCurrentProjectile().gameObject;
			AIBulletBank.Entry bulletEntry = GetBulletEntry();
			if (bulletEntry != null && (bool)bulletEntry.BulletObject)
			{
				bulletObject = bulletEntry.BulletObject;
			}
			GameObject gameObject = SpawnManager.SpawnProjectile(bulletObject, (Vector3)base.aiActor.CenterPosition + Quaternion.Euler(0f, 0f, num + angleForShot) * projectileModule.positionOffset, Quaternion.Euler(0f, 0f, num + angleForShot));
			Projectile component = gameObject.GetComponent<Projectile>();
			if (bulletEntry != null && bulletEntry.OverrideProjectile)
			{
				component.baseData.SetAll(bulletEntry.ProjectileData);
			}
			if ((bool)base.aiActor && base.aiActor.IsBlackPhantom)
			{
				component.baseData.speed *= base.aiActor.BlackPhantomProperties.BulletSpeedMultiplier;
			}
			component.Shooter = base.specRigidbody;
			if (projectileModule.mirror)
			{
				bulletObject = projectileModule.GetCurrentProjectile().gameObject;
				bulletEntry = GetBulletEntry();
				if (bulletEntry != null && (bool)bulletEntry.BulletObject)
				{
					bulletObject = bulletEntry.BulletObject;
				}
				gameObject = SpawnManager.SpawnProjectile(bulletObject, (Vector3)base.aiActor.CenterPosition + Quaternion.Euler(0f, 0f, num + angleForShot) * projectileModule.InversePositionOffset, Quaternion.Euler(0f, 0f, num - angleForShot));
				component = gameObject.GetComponent<Projectile>();
				if (bulletEntry != null && bulletEntry.OverrideProjectile)
				{
					component.baseData.SetAll(bulletEntry.ProjectileData);
				}
				if ((bool)base.aiActor && base.aiActor.IsBlackPhantom)
				{
					component.baseData.speed *= base.aiActor.BlackPhantomProperties.BulletSpeedMultiplier;
				}
				component.Shooter = base.specRigidbody;
			}
			projectileModule.IncrementShootCount();
		}
	}

	private IEnumerator HandleFireRate(float t)
	{
		m_onCooldown = true;
		yield return new WaitForSeconds(t);
		m_onCooldown = false;
	}
}
