using System;
using System.Collections.Generic;
using Brave.BulletScript;
using UnityEngine;
using UnityEngine.Serialization;

public class AIBulletBank : BraveBehaviour, IBulletManager
{
	[Serializable]
	public class Entry
	{
		public string Name;

		public GameObject BulletObject;

		public bool OverrideProjectile;

		[ShowInInspectorIf("OverrideProjectile", false)]
		public ProjectileData ProjectileData;

		[FormerlySerializedAs("BulletMlAudio")]
		public bool PlayAudio;

		[ShowInInspectorIf("PlayAudio", true)]
		[FormerlySerializedAs("BulletMlAudioSwitch")]
		public string AudioSwitch;

		[ShowInInspectorIf("PlayAudio", true)]
		[FormerlySerializedAs("BulletMlAudioEvent")]
		public string AudioEvent;

		[ShowInInspectorIf("PlayAudio", true)]
		[FormerlySerializedAs("LimitBulletMlAudio")]
		public bool AudioLimitOncePerFrame = true;

		[ShowInInspectorIf("PlayAudio", true)]
		public bool AudioLimitOncePerAttack;

		public VFXPool MuzzleFlashEffects;

		[ShowInInspectorIf("MuzzleFlashEffects", true)]
		[FormerlySerializedAs("LimitBulletMlVfx")]
		public bool MuzzleLimitOncePerFrame = true;

		[ShowInInspectorIf("MuzzleFlashEffects", true)]
		public bool MuzzleInheritsTransformDirection;

		public bool SpawnShells;

		[ShowInInspectorIf("SpawnShells", true)]
		public Transform ShellTransform;

		[ShowInInspectorIf("SpawnShells", true)]
		public GameObject ShellPrefab;

		[ShowInInspectorIf("SpawnShells", true)]
		public float ShellForce = 1.75f;

		[ShowInInspectorIf("SpawnShells", true)]
		public float ShellForceVariance = 0.75f;

		[ShowInInspectorIf("SpawnShells", true)]
		public bool DontRotateShell;

		[ShowInInspectorIf("SpawnShells", true)]
		public float ShellGroundOffset;

		[ShowInInspectorIf("SpawnShells", true)]
		public bool ShellsLimitOncePerFrame;

		public bool rampBullets;

		[ShowInInspectorIf("rampBullets", true)]
		public float rampStartHeight = 2f;

		[ShowInInspectorIf("rampBullets", true)]
		public float rampTime = 1f;

		[ShowInInspectorIf("rampBullets", true)]
		public float conditionalMinDegFromNorth;

		public bool forceCanHitEnemies;

		public bool suppressHitEffectsIfOffscreen;

		public int preloadCount;

		public bool m_playedAudioThisFrame { get; set; }

		public bool m_playedEffectsThisFrame { get; set; }

		public bool m_playedShellsThisFrame { get; set; }

		public Entry()
		{
		}

		public Entry(Entry other)
		{
			Name = other.Name;
			BulletObject = other.BulletObject;
			OverrideProjectile = other.OverrideProjectile;
			ProjectileData = new ProjectileData(other.ProjectileData);
			PlayAudio = other.PlayAudio;
			AudioSwitch = other.AudioSwitch;
			AudioEvent = other.AudioEvent;
			AudioLimitOncePerFrame = other.AudioLimitOncePerFrame;
			AudioLimitOncePerAttack = other.AudioLimitOncePerAttack;
			MuzzleFlashEffects = other.MuzzleFlashEffects;
			MuzzleLimitOncePerFrame = other.MuzzleLimitOncePerFrame;
			MuzzleInheritsTransformDirection = other.MuzzleInheritsTransformDirection;
			SpawnShells = other.SpawnShells;
			ShellTransform = other.ShellTransform;
			ShellPrefab = other.ShellPrefab;
			ShellForce = other.ShellForce;
			ShellForceVariance = other.ShellForceVariance;
			DontRotateShell = other.DontRotateShell;
			ShellGroundOffset = other.ShellGroundOffset;
			ShellsLimitOncePerFrame = other.ShellsLimitOncePerFrame;
			rampBullets = other.rampBullets;
			rampStartHeight = other.rampStartHeight;
			rampTime = other.rampTime;
			conditionalMinDegFromNorth = other.conditionalMinDegFromNorth;
			forceCanHitEnemies = other.forceCanHitEnemies;
			suppressHitEffectsIfOffscreen = other.suppressHitEffectsIfOffscreen;
			preloadCount = other.preloadCount;
		}
	}

	public List<Entry> Bullets;

	public bool useDefaultBulletIfMissing;

	public List<Transform> transforms;

	[NonSerialized]
	public bool rampBullets;

	[NonSerialized]
	public float rampStartHeight = 2f;

	[NonSerialized]
	public float rampTime = 1f;

	[NonSerialized]
	public Gun OverrideGun;

	public Action<Projectile> OnProjectileCreated;

	public Action<string, Projectile> OnProjectileCreatedWithSource;

	public Vector2? FixedPlayerPosition;

	private GameObject m_cachedSoundChild;

	private float m_timeScale = 1f;

	private Vector2? m_cachedPlayerPosition;

	private bool m_playVfx = true;

	private bool m_playAudio = true;

	private bool m_playShells = true;

	private string m_cachedActorName;

	public bool PlayVfx
	{
		get
		{
			return m_playVfx;
		}
		set
		{
			m_playVfx = value;
		}
	}

	public bool PlayAudio
	{
		get
		{
			return m_playAudio;
		}
		set
		{
			m_playAudio = value;
		}
	}

	public bool PlayShells
	{
		get
		{
			return m_playShells;
		}
		set
		{
			m_playShells = value;
		}
	}

	public SpeculativeRigidbody FixedPlayerRigidbody { get; set; }

	public Vector2 FixedPlayerRigidbodyLastPosition { get; set; }

	public bool CollidesWithEnemies { get; set; }

	public SpeculativeRigidbody SpecificRigidbodyException { get; set; }

	public GameObject SoundChild
	{
		get
		{
			if (!m_cachedSoundChild)
			{
				m_cachedSoundChild = new GameObject("sound child");
				m_cachedSoundChild.transform.parent = base.transform;
				m_cachedSoundChild.transform.localPosition = Vector3.zero;
			}
			return m_cachedSoundChild;
		}
	}

	public string ActorName
	{
		get
		{
			return m_cachedActorName;
		}
		set
		{
			m_cachedActorName = value;
		}
	}

	public float TimeScale
	{
		get
		{
			return m_timeScale;
		}
		set
		{
			m_timeScale = value;
		}
	}

	public bool SuppressPlayerVelocityAveraging { get; set; }

	public event Action<Bullet, Projectile> OnBulletSpawned;

	public void Awake()
	{
		CollidesWithEnemies = (bool)base.aiShooter && base.aiShooter.CanShootOtherEnemies;
		SpecificRigidbodyException = base.specRigidbody;
		if (Bullets == null)
		{
			return;
		}
		for (int i = 0; i < Bullets.Count; i++)
		{
			Entry entry = Bullets[i];
			if (entry.preloadCount > 0)
			{
				Transform[] array = new Transform[entry.preloadCount];
				for (int j = 0; j < entry.preloadCount; j++)
				{
					array[j] = SpawnManager.PoolManager.Spawn(entry.BulletObject.transform);
				}
				for (int k = 0; k < entry.preloadCount; k++)
				{
					SpawnManager.PoolManager.Despawn(array[k]);
				}
			}
		}
	}

	public void Start()
	{
		if ((bool)base.aiActor)
		{
			m_cachedActorName = base.aiActor.GetActorName();
		}
		if ((bool)base.encounterTrackable && string.IsNullOrEmpty(m_cachedActorName))
		{
			m_cachedActorName = base.encounterTrackable.GetModifiedDisplayName();
		}
	}

	public void Update()
	{
		if ((bool)FixedPlayerRigidbody && (bool)FixedPlayerRigidbody.healthHaver && FixedPlayerRigidbody.healthHaver.IsAlive)
		{
			FixedPlayerRigidbodyLastPosition = FixedPlayerRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public void LateUpdate()
	{
		for (int i = 0; i < Bullets.Count; i++)
		{
			Bullets[i].m_playedEffectsThisFrame = false;
			Bullets[i].m_playedAudioThisFrame = false;
			Bullets[i].m_playedShellsThisFrame = false;
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)base.aiActor && (bool)base.aiActor.TargetRigidbody && PhysicsEngine.HasInstance)
		{
			FixedPlayerRigidbody = base.aiActor.TargetRigidbody;
			FixedPlayerRigidbodyLastPosition = FixedPlayerRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public GameObject CreateProjectileFromBank(Vector2 position, float direction, string bulletName, string spawnTransform = null, bool suppressVfx = false, bool firstBulletOfAttack = true, bool forceBlackBullet = false)
	{
		Entry bullet = GetBullet(bulletName);
		GameObject bulletObject = bullet.BulletObject;
		if (!bulletObject && (bool)base.aiShooter.CurrentGun)
		{
			bulletObject = base.aiShooter.CurrentGun.singleModule.GetCurrentProjectile().gameObject;
		}
		bool flag = false;
		Projectile component = bulletObject.GetComponent<Projectile>();
		if ((bool)component && component.BulletScriptSettings.preventPooling)
		{
			flag = true;
		}
		GameObject gameObject = SpawnManager.SpawnProjectile(bulletObject, position, Quaternion.Euler(0f, 0f, direction), flag);
		Projectile component2 = gameObject.GetComponent<Projectile>();
		if (component2 != null)
		{
			if (forceBlackBullet)
			{
				component2.ForceBlackBullet = true;
			}
			if ((bool)base.gameActor)
			{
				component2.SetOwnerSafe(base.gameActor, m_cachedActorName);
			}
			else if ((bool)base.encounterTrackable)
			{
				component2.OwnerName = base.encounterTrackable.GetModifiedDisplayName();
			}
			if (flag)
			{
				component2.OnSpawned();
			}
			if (bullet.suppressHitEffectsIfOffscreen || ((bool)base.healthHaver && base.healthHaver.IsBoss))
			{
				component2.hitEffects.suppressHitEffectsIfOffscreen = true;
			}
		}
		if (m_playAudio && bullet.PlayAudio)
		{
			bool flag2 = true;
			if (bullet.AudioLimitOncePerFrame)
			{
				flag2 &= !bullet.m_playedAudioThisFrame;
			}
			if (bullet.AudioLimitOncePerAttack)
			{
				flag2 = flag2 && firstBulletOfAttack;
			}
			if (flag2)
			{
				if (!string.IsNullOrEmpty(bullet.AudioSwitch))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", bullet.AudioSwitch, SoundChild);
					AkSoundEngine.PostEvent(bullet.AudioEvent, SoundChild);
				}
				else if ((bool)this)
				{
					AkSoundEngine.PostEvent(bullet.AudioEvent, base.gameObject);
				}
				bullet.m_playedAudioThisFrame = true;
			}
		}
		if (m_playVfx && !suppressVfx && (!bullet.MuzzleLimitOncePerFrame || !bullet.m_playedEffectsThisFrame))
		{
			float zRotation = direction;
			if (bullet.MuzzleInheritsTransformDirection && !string.IsNullOrEmpty(spawnTransform))
			{
				zRotation = GetTransformRotation(spawnTransform);
			}
			if (bullet.MuzzleFlashEffects.type != 0)
			{
				bullet.MuzzleFlashEffects.SpawnAtPosition(position, zRotation);
				bullet.m_playedEffectsThisFrame = true;
			}
			else
			{
				Gun gun = null;
				if ((bool)base.aiShooter && base.aiShooter.enabled)
				{
					gun = base.aiShooter.CurrentGun;
				}
				if ((bool)OverrideGun)
				{
					gun = OverrideGun;
				}
				if ((bool)gun)
				{
					gun.HandleShootAnimation(null);
					gun.HandleShootEffects(null);
					bullet.m_playedEffectsThisFrame = true;
				}
			}
		}
		if (m_playShells && (!bullet.ShellsLimitOncePerFrame || !bullet.m_playedShellsThisFrame) && bullet.SpawnShells)
		{
			SpawnShellCasingAtPosition(bullet);
			bullet.m_playedShellsThisFrame = true;
		}
		Projectile component3 = gameObject.GetComponent<Projectile>();
		if (bullet.OverrideProjectile)
		{
			component3.baseData.SetAll(bullet.ProjectileData);
			component3.UpdateSpeed();
		}
		if ((bool)base.aiActor && base.aiActor.IsBlackPhantom)
		{
			component3.baseData.speed *= base.aiActor.BlackPhantomProperties.BulletSpeedMultiplier;
			component3.UpdateSpeed();
		}
		if (GameManager.Options.DebrisQuantity != GameOptions.GenericHighMedLowOption.HIGH)
		{
			component3.damagesWalls = false;
		}
		if ((bool)base.healthHaver && base.healthHaver.IsBoss)
		{
			component3.damagesWalls = false;
		}
		bool flag3 = (bool)base.aiActor && base.aiActor.CanTargetEnemies;
		component3.collidesWithEnemies = CollidesWithEnemies || bullet.forceCanHitEnemies || flag3;
		component3.UpdateCollisionMask();
		component3.specRigidbody.RegisterSpecificCollisionException(SpecificRigidbodyException);
		component3.SendInDirection(BraveMathCollege.DegreesToVector(direction), false);
		if (bullet.rampBullets)
		{
			if (bullet.conditionalMinDegFromNorth <= 0f || BraveMathCollege.AbsAngleBetween(90f, base.aiAnimator.FacingDirection) > bullet.conditionalMinDegFromNorth)
			{
				component3.Ramp(bullet.rampStartHeight, bullet.rampTime);
			}
		}
		else if (rampBullets)
		{
			component3.Ramp(rampStartHeight, rampTime);
		}
		else if ((bool)base.aiShooter && base.aiShooter.rampBullets)
		{
			component3.Ramp(base.aiShooter.rampStartHeight, base.aiShooter.rampTime);
		}
		if (OnProjectileCreated != null)
		{
			OnProjectileCreated(component3);
		}
		return gameObject;
	}

	public void PostWwiseEvent(string AudioEvent, string AudioSwitch = null)
	{
		if (!string.IsNullOrEmpty(AudioSwitch))
		{
			AkSoundEngine.SetSwitch("WPN_Guns", AudioSwitch, SoundChild);
			AkSoundEngine.PostEvent(AudioEvent, SoundChild);
		}
		else if ((bool)this)
		{
			AkSoundEngine.PostEvent(AudioEvent, base.gameObject);
		}
	}

	public Entry GetBullet(string bulletName = "default")
	{
		Entry entry = null;
		if (string.IsNullOrEmpty(bulletName))
		{
			bulletName = "default";
		}
		for (int i = 0; i < Bullets.Count; i++)
		{
			if (string.Equals(Bullets[i].Name, bulletName, StringComparison.OrdinalIgnoreCase))
			{
				entry = Bullets[i];
			}
		}
		if (entry == null && useDefaultBulletIfMissing)
		{
			for (int j = 0; j < Bullets.Count; j++)
			{
				if (Bullets[j].Name.ToLower() == "default")
				{
					entry = Bullets[j];
				}
			}
			if (entry == null && Bullets.Count > 0)
			{
				entry = Bullets[0];
			}
		}
		if (entry == null)
		{
			Debug.LogError("Missing bank entry for bullet: " + bulletName + "!");
			return null;
		}
		return entry;
	}

	private void SpawnShellCasingAtPosition(Entry bankEntry)
	{
		if (bankEntry.ShellPrefab != null)
		{
			float num = BraveMathCollege.ClampAngle360(bankEntry.ShellTransform.eulerAngles.z);
			Vector3 position = bankEntry.ShellTransform.position;
			GameObject gameObject = SpawnManager.SpawnDebris(bankEntry.ShellPrefab, position, Quaternion.Euler(0f, 0f, (!bankEntry.DontRotateShell) ? num : 0f));
			ShellCasing component = gameObject.GetComponent<ShellCasing>();
			if (component != null)
			{
				component.Trigger();
			}
			DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
			if (component2 != null)
			{
				float magnitude = bankEntry.ShellForce + UnityEngine.Random.Range(0f - bankEntry.ShellForceVariance, bankEntry.ShellForceVariance);
				Vector3 vector = BraveMathCollege.DegreesToVector(num, magnitude);
				Vector3 startingForce = new Vector3(vector.x, 0f, vector.y);
				float num2 = base.specRigidbody.UnitBottom + bankEntry.ShellGroundOffset;
				float num3 = position.y - base.transform.position.y + 0.2f;
				float num4 = component2.transform.position.y - num2 + UnityEngine.Random.value * 0.5f;
				component2.additionalHeightBoost = num3 - num4;
				component2.Trigger(startingForce, num4);
			}
		}
	}

	public Vector2 PlayerPosition()
	{
		if (FixedPlayerPosition.HasValue)
		{
			return FixedPlayerPosition.Value;
		}
		if ((bool)FixedPlayerRigidbody)
		{
			if ((bool)FixedPlayerRigidbody.healthHaver)
			{
				return (!FixedPlayerRigidbody.healthHaver.IsAlive) ? FixedPlayerRigidbodyLastPosition : FixedPlayerRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			return FixedPlayerRigidbody.Velocity;
		}
		if (!base.aiActor)
		{
			Vector2 point = ((!base.transform) ? BraveUtility.ScreenCenterWorldPoint() : base.transform.position);
			PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(point);
			if ((bool)activePlayerClosestToPoint)
			{
				m_cachedPlayerPosition = activePlayerClosestToPoint.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				return m_cachedPlayerPosition.Value;
			}
			return BraveUtility.ScreenCenterWorldPoint();
		}
		if ((bool)base.aiActor.TargetRigidbody)
		{
			m_cachedPlayerPosition = base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			return m_cachedPlayerPosition.Value;
		}
		Vector2? cachedPlayerPosition = m_cachedPlayerPosition;
		if (cachedPlayerPosition.HasValue)
		{
			return m_cachedPlayerPosition.Value;
		}
		return BraveUtility.ScreenCenterWorldPoint();
	}

	public Vector2 PlayerVelocity()
	{
		if ((bool)FixedPlayerRigidbody)
		{
			if ((bool)FixedPlayerRigidbody.healthHaver && !FixedPlayerRigidbody.healthHaver.IsAlive)
			{
				return Vector2.zero;
			}
			PlayerController playerController = FixedPlayerRigidbody.gameActor as PlayerController;
			if ((bool)playerController)
			{
				return playerController.AverageVelocity;
			}
			return FixedPlayerRigidbody.Velocity;
		}
		if (!base.aiActor)
		{
			Vector2 point = ((!base.transform) ? BraveUtility.ScreenCenterWorldPoint() : base.transform.position);
			PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(point);
			if ((bool)activePlayerClosestToPoint)
			{
				return (!SuppressPlayerVelocityAveraging) ? activePlayerClosestToPoint.AverageVelocity : activePlayerClosestToPoint.Velocity;
			}
			return Vector2.zero;
		}
		if ((bool)base.aiActor.TargetRigidbody)
		{
			PlayerController playerController2 = base.aiActor.TargetRigidbody.gameActor as PlayerController;
			if ((bool)playerController2)
			{
				return (!SuppressPlayerVelocityAveraging) ? playerController2.AverageVelocity : playerController2.Velocity;
			}
			return base.aiActor.TargetRigidbody.Velocity;
		}
		return Vector2.zero;
	}

	public void BulletSpawnedHandler(Bullet bullet)
	{
		string bulletName = (string.IsNullOrEmpty(bullet.BankName) ? "default" : bullet.BankName);
		GameObject gameObject = CreateProjectileFromBank(bullet.Position, bullet.Direction, bulletName, bullet.SpawnTransform, bullet.SuppressVfx, bullet.FirstBulletOfAttack, bullet.ForceBlackBullet);
		Projectile projectile = (bullet.Projectile = gameObject.GetComponent<Projectile>());
		if (!projectile || !projectile.BulletScriptSettings.overrideMotion)
		{
			projectile.specRigidbody.Velocity = Vector2.zero;
			BulletScriptBehavior bulletScriptBehavior = gameObject.GetComponent<BulletScriptBehavior>();
			if (!bulletScriptBehavior)
			{
				bulletScriptBehavior = (projectile.braveBulletScript = gameObject.AddComponent<BulletScriptBehavior>());
			}
			projectile.IsBulletScript = true;
			if (this.OnBulletSpawned != null)
			{
				this.OnBulletSpawned(bullet, projectile);
			}
			bullet.Parent = gameObject;
			bullet.Initialize();
			bulletScriptBehavior.Initialize(bullet);
		}
	}

	public void RemoveBullet(Bullet deadBullet)
	{
		if (!deadBullet.DontDestroyGameObject)
		{
			if ((bool)deadBullet.Projectile && SpawnManager.HasInstance)
			{
				deadBullet.Projectile.DieInAir();
			}
			else
			{
				UnityEngine.Object.Destroy(deadBullet.Parent);
			}
		}
	}

	public void DestroyBullet(Bullet deadBullet, bool suppressInAirEffects = false)
	{
		if (deadBullet.Parent == null)
		{
			return;
		}
		BulletScriptBehavior component = deadBullet.Parent.GetComponent<BulletScriptBehavior>();
		if (deadBullet.DontDestroyGameObject)
		{
			if ((bool)component)
			{
				component.bullet = null;
			}
		}
		else if ((bool)deadBullet.Projectile && SpawnManager.HasInstance)
		{
			deadBullet.Projectile.DieInAir(suppressInAirEffects);
			if ((bool)component)
			{
				component.bullet = null;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(deadBullet.Parent);
		}
	}

	public Transform GetTransform(string transformName)
	{
		for (int i = 0; i < transforms.Count; i++)
		{
			if (transforms[i].name == transformName)
			{
				return transforms[i];
			}
		}
		return null;
	}

	public Vector2 TransformOffset(Vector2 pos, string transformName)
	{
		Transform transform = null;
		for (int i = 0; i < transforms.Count; i++)
		{
			if (transforms[i].name == transformName)
			{
				transform = transforms[i];
			}
		}
		if (transform == null)
		{
			return pos;
		}
		return transform.position.XY();
	}

	public float GetTransformRotation(string transformName)
	{
		Transform transform = null;
		for (int i = 0; i < transforms.Count; i++)
		{
			if (transforms[i].name == transformName)
			{
				transform = transforms[i];
			}
		}
		if (transform == null)
		{
			return 0f;
		}
		return transform.eulerAngles.z;
	}

	public Animation GetUnityAnimation()
	{
		return base.unityAnimation;
	}
}
