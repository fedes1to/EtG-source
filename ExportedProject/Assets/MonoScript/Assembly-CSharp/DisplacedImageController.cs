using System.Collections;
using UnityEngine;

public class DisplacedImageController : BraveBehaviour
{
	public float DamagePercentForMaxFade = 0.7f;

	public float UnfadeDelayTime = 1f;

	public float FadeRecoveryTime = 1f;

	private bool m_initialized;

	private AIActor m_host;

	private float m_lastHostHealth;

	private float m_lastImageHealth;

	private float m_fade = -1f;

	private float m_unfadeDelayTimer;

	public float Fade
	{
		get
		{
			return m_fade;
		}
		set
		{
			if (m_fade != value)
			{
				m_fade = value;
				OnFadeChange(base.aiActor, Mathf.Clamp(m_fade, 0f, 0.85f), false);
				OnFadeChange(m_host, 1f - m_fade, true);
			}
		}
	}

	public void Update()
	{
		if (m_unfadeDelayTimer > 0f)
		{
			m_unfadeDelayTimer = Mathf.Max(0f, m_unfadeDelayTimer - BraveTime.DeltaTime);
		}
		else if (Fade > 0f)
		{
			Fade -= BraveTime.DeltaTime / FadeRecoveryTime;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearHost();
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath -= OnImagePreDeath;
			base.healthHaver.OnDeath -= OnImageDeath;
		}
	}

	public void Init()
	{
		if (!m_initialized)
		{
			base.aiActor.CanDropCurrency = false;
			base.aiActor.CanDropItems = false;
			base.aiActor.CollisionDamage = 0f;
			base.aiActor.MovementSpeed = 6f;
			base.aiActor.CorpseObject = null;
			base.aiActor.shadowDeathType = AIActor.ShadowDeathType.None;
			if ((bool)base.aiActor.encounterTrackable)
			{
				Object.Destroy(base.aiActor.encounterTrackable);
			}
			base.healthHaver.OnPreDeath += OnImagePreDeath;
			m_lastImageHealth = base.healthHaver.GetMaxHealth();
			base.aiAnimator.OtherAnimations[3].anim.Prefix = "poof";
			RegenerateCache();
			SeekTargetBehavior seekTargetBehavior = new SeekTargetBehavior();
			seekTargetBehavior.StopWhenInRange = false;
			base.behaviorSpeculator.InstantFirstTick = true;
			base.behaviorSpeculator.PostAwakenDelay = 0f;
			base.behaviorSpeculator.MovementBehaviors[0] = seekTargetBehavior;
			AttackBehaviorGroup attackBehaviorGroup = base.behaviorSpeculator.AttackBehaviorGroup;
			if (attackBehaviorGroup != null)
			{
				attackBehaviorGroup.AttackBehaviors[0].Probability = 0f;
				attackBehaviorGroup.AttackBehaviors[1].Probability = 1f;
			}
			BulletLimbController[] componentsInChildren = GetComponentsInChildren<BulletLimbController>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			Fade = 0f;
			base.aiActor.SetOutlines(true);
			UpdateOutlineMaterial(base.sprite);
			base.healthHaver.OnDeath += OnImageDeath;
			m_initialized = true;
		}
	}

	public void SetHost(AIActor host)
	{
		m_host = host;
		if ((bool)m_host)
		{
			base.aiAnimator.CopyStateFrom(m_host.aiAnimator);
			m_lastHostHealth = host.healthHaver.GetMaxHealth();
			m_host.healthHaver.OnPreDeath += OnHostPreDeath;
			m_host.healthHaver.OnDamaged += OnHostDamaged;
			base.healthHaver.OnDamaged += OnImageDamaged;
			host.SetOutlines(true);
			UpdateOutlineMaterial(host.sprite);
			OnFadeChange(m_host, 1f - Fade, true);
		}
	}

	public void ClearHost()
	{
		if (!(m_host == null))
		{
			m_host.healthHaver.OnPreDeath -= OnHostPreDeath;
			m_host.healthHaver.OnDamaged -= OnHostDamaged;
			base.healthHaver.OnDamaged -= OnImageDamaged;
			OnFadeChange(m_host, 0f, true);
			m_host = null;
		}
	}

	private void OnHostPreDeath(Vector2 deathDir)
	{
		ClearHost();
		base.healthHaver.ApplyDamage(100000f, Vector2.zero, "Mirror Host Death", CoreDamageTypes.None, DamageCategory.Unstoppable);
	}

	private void OnImagePreDeath(Vector2 deathDir)
	{
		OnFadeChange(base.aiActor, 0f, false);
		StartCoroutine(DeathFade());
	}

	private void OnHostDamaged(float resultValue, float maxValue, CoreDamageTypes damagetypes, DamageCategory damagecategory, Vector2 damagedirection)
	{
		float damage = m_lastHostHealth - resultValue;
		OnEitherDamaged(damage, maxValue);
		m_lastHostHealth = resultValue;
	}

	private void OnImageDamaged(float resultValue, float maxValue, CoreDamageTypes damagetypes, DamageCategory damagecategory, Vector2 damagedirection)
	{
		float damage = m_lastImageHealth - resultValue;
		OnEitherDamaged(damage, maxValue);
		m_lastImageHealth = resultValue;
	}

	private void OnImageDeath(Vector2 vector2)
	{
		base.aiAnimator.PlayVfx("death_poof");
	}

	private void OnFadeChange(AIActor aiActor, float fade, bool isHost)
	{
		if (!aiActor)
		{
			return;
		}
		aiActor.renderer.material.SetFloat("_DisplacerFade", fade * 1.5f);
		aiActor.sprite.usesOverrideMaterial = fade > 0f;
		if (isHost)
		{
			if (fade <= 0f)
			{
				aiActor.SetOutlines(true);
				UpdateOutlineMaterial(aiActor.sprite);
			}
			else
			{
				bool flag = false;
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[i];
					if ((bool)playerController && playerController.CanDetectHiddenEnemies)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					aiActor.SetOutlines(false);
				}
			}
		}
		tk2dSprite component = aiActor.ShadowObject.GetComponent<tk2dSprite>();
		component.color = component.color.WithAlpha(1f - fade);
	}

	private void OnEitherDamaged(float damage, float maxHealth)
	{
		float num = damage / maxHealth / DamagePercentForMaxFade;
		Fade = Mathf.Clamp01(Fade + num);
		m_unfadeDelayTimer = UnfadeDelayTime;
	}

	private IEnumerator DeathFade()
	{
		tk2dSprite shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dSprite>();
		float startAlpha = shadowSprite.color.a;
		while (true)
		{
			shadowSprite.color = shadowSprite.color.WithAlpha(startAlpha * (1f - base.aiAnimator.CurrentClipProgress));
			yield return null;
		}
	}

	private void UpdateOutlineMaterial(tk2dBaseSprite sprite)
	{
		Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(sprite);
		outlineMaterial.SetColor("_OverrideColor", new Color(0f, 11f, 33f));
		outlineMaterial.EnableKeyword("EXCLUDE_INTERIOR");
		outlineMaterial.DisableKeyword("INCLUDE_INTERIOR");
	}
}
