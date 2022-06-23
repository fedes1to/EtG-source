using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class HammerOfDawnController : BraveBehaviour
{
	public List<tk2dSprite> BeamSections;

	public tk2dSprite BurstSprite;

	[CheckAnimation(null)]
	public string SectionStartAnimation;

	[CheckAnimation(null)]
	public string SectionAnimation;

	[CheckAnimation(null)]
	public string SectionEndAnimation;

	[CheckAnimation(null)]
	public string CapAnimation;

	[CheckAnimation(null)]
	public string CapEndAnimation;

	public GameObject InitialImpactVFX;

	public float TrackingSpeed = 5f;

	public float InitialDamage = 50f;

	public float DamagePerSecond = 20f;

	public float OverlapRadius = 2.5f;

	public float DamageRadius = 2.5f;

	public float RotationSpeed = 60f;

	public GoopDefinition FireGoop;

	public float FireGoopRadius = 1.5f;

	private PlayerController m_owner;

	private Projectile m_projectile;

	private float m_currentTrackingSpeed;

	private DeadlyDeadlyGoopManager m_manager;

	private float InputScale = 1f;

	private static Dictionary<Projectile, HammerOfDawnController> m_projectileHammerMap = new Dictionary<Projectile, HammerOfDawnController>();

	public static List<HammerOfDawnController> m_extantHammers = new List<HammerOfDawnController>();

	private float m_lifeElapsed;

	private Vector2 m_currentAimPoint;

	private float m_particleCounter;

	private bool m_hasDisposed;

	private float ModifiedDamageRadius
	{
		get
		{
			return DamageRadius * InputScale;
		}
	}

	public static bool HasExtantHammer(Projectile p)
	{
		if ((bool)p && m_projectileHammerMap.ContainsKey(p) && !m_projectileHammerMap[p])
		{
			m_projectileHammerMap.Remove(p);
		}
		return (bool)p && m_projectileHammerMap.ContainsKey(p) && (bool)m_projectileHammerMap[p];
	}

	public static void ClearPerLevelData()
	{
		m_projectileHammerMap.Clear();
	}

	public void AssignOwner(PlayerController p, Projectile beam)
	{
		m_owner = p;
		m_projectile = beam;
		if (beam != null && m_projectileHammerMap.ContainsKey(beam))
		{
			HammerOfDawnController hammerOfDawnController = m_projectileHammerMap[beam];
			AkSoundEngine.PostEvent("Play_WPN_dawnhammer_charge_01", base.gameObject);
			if ((bool)hammerOfDawnController)
			{
				hammerOfDawnController.Dispose();
			}
			if (m_projectileHammerMap.ContainsKey(beam))
			{
				m_projectileHammerMap.Remove(beam);
			}
		}
		Color? color = null;
		if ((bool)beam)
		{
			m_projectileHammerMap.Add(beam, this);
			if ((bool)beam.sprite)
			{
				color = beam.sprite.renderer.sharedMaterial.GetColor("_OverrideColor");
				if (color.Value.a > 0.1f)
				{
					color = color.Value.WithAlpha(1f);
				}
			}
		}
		if ((bool)p)
		{
			InputScale = p.BulletScaleModifier;
		}
		if (InputScale > 1f || color.HasValue)
		{
			tk2dBaseSprite[] componentsInChildren = GetComponentsInChildren<tk2dBaseSprite>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].scale = new Vector3(InputScale, 1f, 1f);
				if (color.HasValue)
				{
					componentsInChildren[i].usesOverrideMaterial = true;
					componentsInChildren[i].renderer.material.SetColor("_OverrideColor", color.Value);
				}
			}
		}
		m_extantHammers.Add(this);
	}

	private void Start()
	{
		for (int i = 0; i < BeamSections.Count; i++)
		{
			tk2dSpriteAnimator tk2dSpriteAnimator2 = BeamSections[i].spriteAnimator;
			if ((bool)tk2dSpriteAnimator2)
			{
				tk2dSpriteAnimator2.alwaysUpdateOffscreen = true;
				tk2dSpriteAnimator2.PlayForDuration(SectionStartAnimation, -1f, SectionAnimation);
				AkSoundEngine.PostEvent("Play_WPN_dawnhammer_loop_01", base.gameObject);
				AkSoundEngine.PostEvent("Play_State_Volume_Lower_01", base.gameObject);
			}
		}
		base.spriteAnimator.alwaysUpdateOffscreen = true;
		BurstSprite.UpdateZDepth();
		base.sprite.renderer.enabled = false;
		m_currentAimPoint = base.transform.position.XY();
		Exploder.DoRadialDamage(InitialDamage, base.transform.position, ModifiedDamageRadius, false, true);
		Exploder.DoRadialMajorBreakableDamage(InitialDamage, base.transform.position, ModifiedDamageRadius);
	}

	private void Update()
	{
		if (m_hasDisposed)
		{
			return;
		}
		if ((bool)m_owner && (bool)m_projectile)
		{
			m_lifeElapsed += BraveTime.DeltaTime;
			BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(m_owner.PlayerIDX);
			if (instanceForPlayer.IsKeyboardAndMouse())
			{
				m_currentAimPoint = m_owner.unadjustedAimPoint.XY();
			}
			else
			{
				m_currentAimPoint += instanceForPlayer.ActiveActions.Aim.Vector.normalized * m_currentTrackingSpeed * BraveTime.DeltaTime;
			}
			Vector2 currentAimPoint = m_currentAimPoint;
			Vector2 vector = base.transform.position.XY();
			if (m_extantHammers.Count > 1)
			{
				int count = m_extantHammers.Count;
				int num = Mathf.Clamp(m_extantHammers.IndexOf(this), 0, m_extantHammers.Count);
				float num2 = 360f / (float)count;
				float num3 = Time.time * RotationSpeed % 360f;
				currentAimPoint += (Quaternion.Euler(0f, 0f, num3 + num2 * (float)num) * Vector2.up * OverlapRadius).XY();
			}
			m_currentTrackingSpeed = Mathf.Lerp(0f, TrackingSpeed, Mathf.Clamp01(m_lifeElapsed / 3f));
			base.transform.position = (vector + (currentAimPoint - vector).normalized * m_currentTrackingSpeed * BraveTime.DeltaTime).ToVector3ZisY();
			base.transform.position = BraveMathCollege.ClampToBounds(base.transform.position.XY(), GameManager.Instance.MainCameraController.MinVisiblePoint + new Vector2(-15f, -15f), GameManager.Instance.MainCameraController.MaxVisiblePoint + new Vector2(15f, 15f)).ToVector3ZisY();
			Exploder.DoRadialDamage(DamagePerSecond * BraveTime.DeltaTime, vector.ToVector3ZisY(), ModifiedDamageRadius, false, true);
			Exploder.DoRadialMajorBreakableDamage(DamagePerSecond * BraveTime.DeltaTime, vector.ToVector3ZisY(), ModifiedDamageRadius);
			if ((bool)m_owner)
			{
				ApplyBeamTickToEnemiesInRadius();
			}
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.MEDIUM || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH)
			{
				int num4 = ((GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.HIGH) ? 50 : 125);
				m_particleCounter += BraveTime.DeltaTime * (float)num4;
				if (m_particleCounter > 1f)
				{
					GlobalSparksDoer.DoRadialParticleBurst(Mathf.FloorToInt(m_particleCounter), base.sprite.WorldBottomLeft, base.sprite.WorldTopRight, 30f, 2f, 1f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
					m_particleCounter %= 1f;
				}
			}
			if (m_manager == null)
			{
				m_manager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(FireGoop);
			}
			m_manager.AddGoopCircle(vector, FireGoopRadius * InputScale, -1, true);
		}
		else
		{
			Dispose();
		}
		base.sprite.UpdateZDepth();
		for (int i = 0; i < BeamSections.Count; i++)
		{
			BeamSections[i].UpdateZDepth();
		}
		BurstSprite.UpdateZDepth();
	}

	private void ApplyBeamTickToEnemiesInRadius()
	{
		Vector2 vector = base.transform.position.XY();
		float num = ModifiedDamageRadius * ModifiedDamageRadius;
		RoomHandler absoluteRoom = vector.GetAbsoluteRoom();
		if (absoluteRoom == null)
		{
			return;
		}
		List<AIActor> activeEnemies = absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor = activeEnemies[i];
			if ((bool)aIActor && (bool)aIActor.specRigidbody && (aIActor.CenterPosition - vector).sqrMagnitude < num && (bool)aIActor.healthHaver)
			{
				m_owner.DoPostProcessBeamTick(null, aIActor.specRigidbody, 1f);
			}
		}
	}

	private void LateUpdate()
	{
		if (!m_hasDisposed && !BurstSprite.renderer.enabled)
		{
			base.sprite.renderer.enabled = true;
			base.spriteAnimator.Play(CapAnimation);
		}
	}

	private void Dispose()
	{
		if (!m_hasDisposed)
		{
			base.sprite.renderer.enabled = true;
			m_hasDisposed = true;
			m_extantHammers.Remove(this);
			if (m_projectileHammerMap.ContainsKey(m_projectile))
			{
				m_projectileHammerMap.Remove(m_projectile);
			}
			m_owner = null;
			m_projectile = null;
			ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
			if ((bool)componentInChildren)
			{
				BraveUtility.EnableEmission(componentInChildren, false);
			}
			for (int i = 0; i < BeamSections.Count; i++)
			{
				BeamSections[i].spriteAnimator.Play(SectionEndAnimation);
			}
			base.spriteAnimator.PlayAndDestroyObject(CapEndAnimation);
			Object.Destroy(base.gameObject, 1f);
			AkSoundEngine.PostEvent("Stop_WPN_gun_loop_01", base.gameObject);
			AkSoundEngine.PostEvent("Stop_State_Volume_Lower_01", base.gameObject);
		}
	}
}
