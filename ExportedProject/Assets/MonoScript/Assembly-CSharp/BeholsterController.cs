using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BeholsterController : BraveBehaviour
{
	[Header("Eye Sprites")]
	public tk2dSprite eyeSprite;

	public Transform pupilTransform;

	public tk2dSprite pupilSprite;

	[Header("Beam Data")]
	public Transform beamTransform;

	public VFXPool chargeUpVfx;

	public VFXPool chargeDownVfx;

	public ProjectileModule beamModule;

	[Header("Beam Firing Point")]
	public Vector2 firingEllipseCenter;

	public float firingEllipseA;

	public float firingEllipseB;

	public float GlitchWorldHealthModifier = 1f;

	private BeholsterTentacleController[] m_tentacles;

	private bool m_laserActive;

	private bool m_firingLaser;

	private float m_laserAngle;

	private BasicBeamController m_laserBeam;

	public bool LaserActive
	{
		get
		{
			return m_laserActive;
		}
	}

	public bool FiringLaser
	{
		get
		{
			return m_firingLaser;
		}
	}

	public float LaserAngle
	{
		get
		{
			return m_laserAngle;
		}
		set
		{
			m_laserAngle = value;
			if (m_firingLaser)
			{
				base.aiAnimator.FacingDirection = value;
			}
		}
	}

	public BasicBeamController LaserBeam
	{
		get
		{
			return m_laserBeam;
		}
	}

	public Vector2 LaserFiringCenter
	{
		get
		{
			return base.transform.position.XY() + firingEllipseCenter;
		}
	}

	public void Start()
	{
		if (base.aiActor.ParentRoom != null && base.aiActor.ParentRoom.area.PrototypeRoomName == "DoubleBeholsterRoom01")
		{
			GameManager.Instance.Dungeon.IsGlitchDungeon = true;
			base.healthHaver.SetHealthMaximum(base.healthHaver.GetMaxHealth() * GlitchWorldHealthModifier);
			base.healthHaver.ForceSetCurrentHealth(base.healthHaver.GetMaxHealth());
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/Glitch");
		}
		m_tentacles = GetComponentsInChildren<BeholsterTentacleController>();
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.2f);
		if ((bool)eyeSprite)
		{
			base.healthHaver.RegisterBodySprite(eyeSprite);
			eyeSprite.usesOverrideMaterial = false;
		}
		if ((bool)pupilSprite)
		{
			base.healthHaver.RegisterBodySprite(pupilSprite);
		}
		base.aiAnimator.FacingDirection = -90f;
		base.aiAnimator.Update();
		base.healthHaver.OnDamaged += OnDamaged;
	}

	public void Update()
	{
		float facingDirection = base.aiAnimator.FacingDirection;
		if (base.spriteAnimator.CurrentClip != null && base.spriteAnimator.CurrentClip.name.Contains("idle"))
		{
			if (!(facingDirection <= 155f) || !(facingDirection >= 25f))
			{
				if (facingDirection <= -60f && facingDirection >= -120f)
				{
					float num = Mathf.InverseLerp(-120f, -60f, facingDirection);
					pupilSprite.transform.localPosition = new Vector3(PhysicsEngine.PixelToUnit((int)(num * 11f) - 5), (!((double)Mathf.Abs(num - 0.5f) > 0.35)) ? 0f : PhysicsEngine.PixelToUnit(1), pupilSprite.transform.localPosition.z);
				}
				else if (Mathf.Abs(facingDirection) >= 90f)
				{
					float num2 = ((!(facingDirection > 0f)) ? facingDirection : (facingDirection - 360f));
					if (num2 < -180f)
					{
						pupilSprite.transform.localPosition = new Vector3(0f, 0f, pupilSprite.transform.localPosition.z);
					}
					else
					{
						float num3 = Mathf.InverseLerp(-180f, -120f, num2);
						pupilSprite.transform.localPosition = new Vector3(PhysicsEngine.PixelToUnit((int)(num3 * 21f)), 0f - PhysicsEngine.PixelToUnit(Mathf.Min((int)(num3 * 26f), 7)), pupilSprite.transform.localPosition.z);
					}
				}
				else if (facingDirection > 0f)
				{
					pupilSprite.transform.localPosition = new Vector3(0f, 0f, pupilSprite.transform.localPosition.z);
				}
				else
				{
					float num4 = Mathf.InverseLerp(0f, -60f, facingDirection);
					pupilSprite.transform.localPosition = new Vector3(0f - PhysicsEngine.PixelToUnit((int)(num4 * 21f)), 0f - PhysicsEngine.PixelToUnit(Mathf.Min((int)(num4 * 26f), 7)), pupilSprite.transform.localPosition.z);
				}
			}
		}
		else
		{
			pupilSprite.transform.localPosition = new Vector3(0f, 0f, pupilSprite.transform.localPosition.z);
		}
		if (m_firingLaser)
		{
			base.aiAnimator.PlayUntilCancelled("eyelaser", true);
		}
	}

	public void LateUpdate()
	{
		string text = GetEyeSprite(base.sprite.CurrentSprite.name);
		int spriteIdByName = base.sprite.GetSpriteIdByName(text);
		if (spriteIdByName > 0)
		{
			eyeSprite.usesOverrideMaterial = false;
			eyeSprite.renderer.enabled = true;
			eyeSprite.SetSprite(spriteIdByName);
		}
		else
		{
			eyeSprite.renderer.enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void StartFiringTentacles(BeholsterTentacleController[] tentacles = null)
	{
		if (tentacles == null)
		{
			tentacles = m_tentacles;
		}
		List<BeholsterTentacleController> list = new List<BeholsterTentacleController>();
		for (int i = 0; i < tentacles.Length; i++)
		{
			if (tentacles[i].IsReady)
			{
				list.Add(tentacles[i]);
			}
		}
		if (list.Count > 0)
		{
			list[UnityEngine.Random.Range(0, list.Count)].StartFiring();
		}
	}

	public void SingleFireTentacle(BeholsterTentacleController[] tentacles = null, float? angleOffset = null)
	{
		if (tentacles == null)
		{
			tentacles = m_tentacles;
		}
		List<BeholsterTentacleController> list = new List<BeholsterTentacleController>();
		for (int i = 0; i < tentacles.Length; i++)
		{
			if (tentacles[i].IsReady)
			{
				list.Add(tentacles[i]);
			}
		}
		if (list.Count > 0)
		{
			list[UnityEngine.Random.Range(0, list.Count)].SingleFire(angleOffset);
		}
	}

	public void StopFiringTentacles(BeholsterTentacleController[] tentacles = null)
	{
		if (tentacles == null)
		{
			tentacles = m_tentacles;
		}
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].CeaseAttack();
		}
	}

	public void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (!(resultValue <= 0f))
		{
			return;
		}
		if (m_firingLaser)
		{
			chargeUpVfx.DestroyAll();
			chargeDownVfx.DestroyAll();
			StopFiringLaser();
			if (m_laserBeam != null)
			{
				m_laserBeam.DestroyBeam();
				m_laserBeam = null;
			}
		}
		BeholsterTentacleController[] tentacles = m_tentacles;
		foreach (BeholsterTentacleController beholsterTentacleController in tentacles)
		{
			Renderer[] componentsInChildren = beholsterTentacleController.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = false;
			}
		}
	}

	public void PrechargeFiringLaser()
	{
		AkSoundEngine.PostEvent("Play_ENM_beholster_charging_01", base.gameObject);
		m_laserActive = true;
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = ((!(base.aiAnimator.FacingDirection > 0f) || !(base.aiAnimator.FacingDirection < 180f)) ? (-90) : 90);
		base.aiAnimator.PlayUntilCancelled("charge", true);
	}

	public void ChargeFiringLaser(float time)
	{
		AkSoundEngine.PostEvent("Play_ENM_deathray_charge_01", base.gameObject);
		m_laserActive = true;
		bool flag = base.aiAnimator.FacingDirection > 0f && base.aiAnimator.FacingDirection < 180f;
		if (flag)
		{
			chargeUpVfx.SpawnAtLocalPosition(Vector3.zero, 0f, beamTransform, Vector2.zero, Vector2.zero, true);
		}
		else
		{
			chargeDownVfx.SpawnAtLocalPosition(Vector3.zero, 0f, beamTransform, Vector2.zero, Vector2.zero, true);
		}
		SpriteAnimatorChanger[] componentsInChildren = beamTransform.GetComponentsInChildren<SpriteAnimatorChanger>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].time = time / 2f;
		}
		tk2dSprite[] componentsInChildren2 = beamTransform.GetComponentsInChildren<tk2dSprite>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].HeightOffGround += ((!flag) ? 1 : (-1));
			componentsInChildren2[j].UpdateZDepth();
		}
	}

	public void StartFiringLaser(float laserAngle)
	{
		AkSoundEngine.PostEvent("Play_ENM_deathray_shot_01", base.gameObject);
		m_laserActive = true;
		m_firingLaser = true;
		LaserAngle = laserAngle;
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.PlayUntilCancelled("eyelaser", true);
		StartCoroutine(FireBeam(beamModule));
	}

	public void StopFiringLaser()
	{
		if (m_firingLaser)
		{
			AkSoundEngine.PostEvent("Stop_ENM_deathray_loop_01", base.gameObject);
			m_laserActive = false;
			m_firingLaser = false;
			base.aiAnimator.LockFacingDirection = false;
			base.aiAnimator.EndAnimationIf("eyelaser");
		}
	}

	protected IEnumerator FireBeam(ProjectileModule mod)
	{
		GameObject beamObject = UnityEngine.Object.Instantiate(mod.GetCurrentProjectile().gameObject);
		m_laserBeam = beamObject.GetComponent<BasicBeamController>();
		List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor = activeEnemies[i];
			if ((bool)aIActor && aIActor != base.aiActor && (bool)aIActor.healthHaver && aIActor.healthHaver.IsBoss)
			{
				m_laserBeam.IgnoreRigidbodes.Add(aIActor.specRigidbody);
			}
		}
		m_laserBeam.Owner = base.aiActor;
		m_laserBeam.HitsPlayers = true;
		m_laserBeam.HitsEnemies = true;
		bool facingNorth2 = BraveMathCollege.ClampAngle180(base.aiAnimator.FacingDirection) > 0f;
		m_laserBeam.HeightOffset = 1.9f;
		m_laserBeam.RampHeightOffset = ((!facingNorth2) ? 5 : 0);
		m_laserBeam.ContinueBeamArtToWall = true;
		float enemyTickCooldown = 0f;
		m_laserBeam.OverrideHitChecks = delegate(SpeculativeRigidbody hitRigidbody, Vector2 dirVec)
		{
			HealthHaver healthHaver = ((!hitRigidbody) ? null : hitRigidbody.healthHaver);
			if ((bool)hitRigidbody && (bool)hitRigidbody.projectile && (bool)hitRigidbody.GetComponent<BeholsterBounceRocket>())
			{
				BounceProjModifier component = hitRigidbody.GetComponent<BounceProjModifier>();
				if ((bool)component)
				{
					component.numberOfBounces = 0;
				}
				hitRigidbody.projectile.DieInAir();
			}
			if (healthHaver != null)
			{
				if ((bool)healthHaver.aiActor)
				{
					if (enemyTickCooldown <= 0f)
					{
						Projectile currentProjectile = mod.GetCurrentProjectile();
						healthHaver.ApplyDamage(ProjectileData.FixedFallbackDamageToEnemies, dirVec, base.aiActor.GetActorName(), currentProjectile.damageTypes);
						enemyTickCooldown = mod.cooldownTime;
					}
				}
				else
				{
					Projectile currentProjectile2 = mod.GetCurrentProjectile();
					healthHaver.ApplyDamage(currentProjectile2.baseData.damage, dirVec, base.aiActor.GetActorName(), currentProjectile2.damageTypes);
				}
			}
			if ((bool)hitRigidbody.majorBreakable)
			{
				hitRigidbody.majorBreakable.ApplyDamage(26f * BraveTime.DeltaTime, dirVec, false);
			}
		};
		bool firstFrame = true;
		while (m_laserBeam != null && m_firingLaser)
		{
			enemyTickCooldown = Mathf.Max(enemyTickCooldown - BraveTime.DeltaTime, 0f);
			float clampedAngle = BraveMathCollege.ClampAngle360(LaserAngle);
			Vector3 dirVec2 = new Vector3(Mathf.Cos(clampedAngle * ((float)Math.PI / 180f)), Mathf.Sin(clampedAngle * ((float)Math.PI / 180f)), 0f) * 10f;
			Vector2 startingPoint = LaserFiringCenter;
			float tanAngle = Mathf.Tan(clampedAngle * ((float)Math.PI / 180f));
			float sign = ((!(clampedAngle > 90f) || !(clampedAngle < 270f)) ? 1 : (-1));
			float denominator = Mathf.Sqrt(firingEllipseB * firingEllipseB + firingEllipseA * firingEllipseA * (tanAngle * tanAngle));
			startingPoint.x += sign * firingEllipseA * firingEllipseB / denominator;
			startingPoint.y += sign * firingEllipseA * firingEllipseB * tanAngle / denominator;
			m_laserBeam.Origin = startingPoint;
			m_laserBeam.Direction = dirVec2;
			if (firstFrame)
			{
				yield return null;
				firstFrame = false;
				continue;
			}
			facingNorth2 = BraveMathCollege.ClampAngle180(base.aiAnimator.FacingDirection) > 0f;
			m_laserBeam.RampHeightOffset = ((!facingNorth2) ? 5 : 0);
			m_laserBeam.LateUpdatePosition(startingPoint);
			yield return null;
			if (m_firingLaser && !m_laserBeam)
			{
				StopFiringLaser();
				break;
			}
			while (Time.timeScale == 0f)
			{
				yield return null;
			}
		}
		if (!m_firingLaser && m_laserBeam != null)
		{
			m_laserBeam.DestroyBeam();
			m_laserBeam = null;
		}
	}

	private string GetEyeSprite(string sprite)
	{
		int n = 2;
		if (sprite.Contains("appear") || sprite.Contains("die"))
		{
			n = 1;
		}
		else if (sprite.Contains("eyelaser") || sprite.Contains("idle"))
		{
			n = 2;
		}
		else if (sprite.Contains("charge"))
		{
			n = 3;
		}
		return sprite.Insert(BraveUtility.GetNthIndexOf(sprite, '_', n), "_eye");
	}
}
