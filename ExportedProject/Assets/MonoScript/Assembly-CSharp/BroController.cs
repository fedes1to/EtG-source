using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroController : BraveBehaviour
{
	public string enrageAnim;

	public float enrageAnimTime = 1f;

	public Color enrageColor;

	public GameObject enrageVfx;

	public Transform enrageVfxTransform;

	public GameObject overheadVfx;

	public float postEnrageMoveSpeed = -1f;

	public float enrageHealToPercent = 0.5f;

	private bool m_shouldEnrage;

	private float m_cachedSpawnProbability = 0.1f;

	private bool m_isEnraged;

	private GameObject m_overheadVfxInstance;

	private float m_overheadVfxTimer;

	private float m_particleCounter;

	private tk2dBaseSprite m_enrageVfx;

	public static void ClearPerLevelData()
	{
		StaticReferenceManager.AllBros.Clear();
	}

	public static BroController GetOtherBro(AIActor me)
	{
		return GetOtherBro(me.gameObject);
	}

	public static BroController GetOtherBro(GameObject me)
	{
		BroController broController = null;
		bool flag = false;
		List<BroController> allBros = StaticReferenceManager.AllBros;
		for (int i = 0; i < allBros.Count; i++)
		{
			if ((bool)allBros[i])
			{
				if (me == allBros[i].gameObject)
				{
					flag = true;
				}
				else
				{
					broController = allBros[i];
				}
			}
		}
		if (!flag)
		{
			Debug.LogWarning("Searched for a bro, but didn't find myself (" + me.name + ")", me);
		}
		return (!broController) ? null : broController;
	}

	public void Awake()
	{
		StaticReferenceManager.AllBros.Add(this);
	}

	public void Update()
	{
		if (!base.healthHaver.IsDead && m_shouldEnrage && base.behaviorSpeculator.IsInterruptable)
		{
			m_shouldEnrage = false;
			base.behaviorSpeculator.InterruptAndDisable();
			base.aiActor.ClearPath();
			StartCoroutine(EnrageCR());
		}
		if (!m_isEnraged)
		{
			return;
		}
		m_overheadVfxTimer += BraveTime.DeltaTime;
		if ((bool)m_overheadVfxInstance && m_overheadVfxTimer > 1.5f)
		{
			m_overheadVfxInstance.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
			m_overheadVfxInstance = null;
		}
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && (bool)base.aiActor && !base.aiActor.IsGone)
		{
			m_particleCounter += BraveTime.DeltaTime * 40f;
			if (m_particleCounter > 1f)
			{
				int num = Mathf.FloorToInt(m_particleCounter);
				m_particleCounter %= 1f;
				GlobalSparksDoer.DoRandomParticleBurst(num, base.aiActor.sprite.WorldBottomLeft.ToVector3ZisY(), base.aiActor.sprite.WorldTopRight.ToVector3ZisY(), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
			}
		}
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllBros.Remove(this);
		base.OnDestroy();
	}

	public void Enrage()
	{
		m_shouldEnrage = true;
	}

	private IEnumerator EnrageCR()
	{
		if (base.healthHaver.GetCurrentHealthPercentage() < enrageHealToPercent)
		{
			base.healthHaver.ForceSetCurrentHealth(enrageHealToPercent * base.healthHaver.GetMaxHealth());
		}
		for (int i = 0; i < base.behaviorSpeculator.AttackBehaviors.Count; i++)
		{
			if (base.behaviorSpeculator.AttackBehaviors[i] is AttackBehaviorGroup)
			{
				ProcessAttackGroup(base.behaviorSpeculator.AttackBehaviors[i] as AttackBehaviorGroup);
			}
		}
		base.aiShooter.ToggleGunAndHandRenderers(false, "BroController");
		base.aiAnimator.PlayUntilFinished(enrageAnim, true);
		Color startingColor = base.aiActor.CurrentOverrideColor;
		float timer = 0f;
		m_isEnraged = false;
		while (timer < enrageAnimTime)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			base.aiActor.RegisterOverrideColor(Color.Lerp(startingColor, enrageColor, timer / enrageAnimTime), "BroEnrage");
			if (!m_isEnraged && timer / enrageAnimTime >= 0.5f)
			{
				if ((bool)enrageVfx)
				{
					GameObject gameObject = SpawnManager.SpawnVFX(enrageVfx, enrageVfxTransform.position, Quaternion.identity);
					m_enrageVfx = gameObject.GetComponent<tk2dBaseSprite>();
					m_enrageVfx.transform.parent = enrageVfxTransform;
					m_enrageVfx.HeightOffGround = 0.5f;
					base.sprite.AttachRenderer(m_enrageVfx);
					base.healthHaver.OnPreDeath += OnPreDeath;
				}
				if ((bool)overheadVfx)
				{
					m_overheadVfxInstance = base.aiActor.PlayEffectOnActor(overheadVfx, new Vector3(0f, 1.375f, 0f), true, true);
					m_overheadVfxTimer = 0f;
				}
				m_isEnraged = true;
			}
		}
		base.aiAnimator.EndAnimationIf(enrageAnim);
		base.aiShooter.ToggleGunAndHandRenderers(true, "BroController");
		if (postEnrageMoveSpeed >= 0f)
		{
			base.aiActor.MovementSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed(postEnrageMoveSpeed);
		}
		base.behaviorSpeculator.enabled = true;
	}

	private void ProcessAttackGroup(AttackBehaviorGroup attackGroup)
	{
		for (int i = 0; i < attackGroup.AttackBehaviors.Count; i++)
		{
			AttackBehaviorGroup.AttackGroupItem attackGroupItem = attackGroup.AttackBehaviors[i];
			if (attackGroupItem.Behavior is AttackBehaviorGroup)
			{
				ProcessAttackGroup(attackGroupItem.Behavior as AttackBehaviorGroup);
			}
			else if (attackGroupItem.Behavior is ShootGunBehavior)
			{
				ShootGunBehavior shootGunBehavior = attackGroupItem.Behavior as ShootGunBehavior;
				if (shootGunBehavior.WeaponType == WeaponType.AIShooterProjectile)
				{
					attackGroupItem.Probability = 0f;
				}
				else if (shootGunBehavior.WeaponType == WeaponType.BulletScript)
				{
					attackGroupItem.Probability = 1f;
					shootGunBehavior.StopDuringAttack = false;
				}
			}
			else if (attackGroupItem.Behavior is SpawnReinforcementsBehavior)
			{
				if (attackGroupItem.Probability > 0f)
				{
					m_cachedSpawnProbability = attackGroupItem.Probability;
					attackGroupItem.Probability = 0f;
				}
				else
				{
					attackGroupItem.Probability = m_cachedSpawnProbability;
				}
			}
			else if (attackGroupItem.Behavior is ShootBehavior)
			{
				if (attackGroupItem.Probability > 0f)
				{
					attackGroupItem.Probability = 0f;
				}
				else
				{
					attackGroupItem.Probability = 1f;
				}
			}
		}
	}

	private void OnPreDeath(Vector2 finalDeathDir)
	{
		if ((bool)m_enrageVfx)
		{
			SpawnManager.Despawn(m_enrageVfx.gameObject);
		}
	}
}
