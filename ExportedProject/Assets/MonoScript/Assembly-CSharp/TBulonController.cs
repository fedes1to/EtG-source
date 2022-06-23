using System.Collections.Generic;
using UnityEngine;

public class TBulonController : BraveBehaviour
{
	private enum State
	{
		Normal,
		Transforming,
		Enraged
	}

	public float newHealth = 50f;

	[CheckDirectionalAnimation(null)]
	public string transformAnim;

	[CheckDirectionalAnimation(null)]
	public string enrageAnim;

	public float overrideMoveSpeed = -1f;

	public float overrideWeight = -1f;

	public List<DamageTypeModifier> onFireDamageTypeModifiers;

	private State m_state;

	private GoopDoer m_goopDoer;

	private float m_startGoopRadius;

	public void Start()
	{
		base.healthHaver.minimumHealth = 1f;
		base.healthHaver.OnDamaged += OnDamaged;
		m_goopDoer = GetComponent<GoopDoer>();
	}

	public void Update()
	{
		if (!base.aiActor || !base.healthHaver || base.healthHaver.IsDead || m_state == State.Normal)
		{
			return;
		}
		if (m_state == State.Transforming)
		{
			base.sprite.ForceUpdateMaterial();
			if (!base.aiAnimator.IsPlaying(transformAnim))
			{
				base.aiAnimator.PlayUntilFinished(enrageAnim, true);
				base.behaviorSpeculator.enabled = true;
				if (overrideMoveSpeed >= 0f)
				{
					base.aiActor.MovementSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed(overrideMoveSpeed);
				}
				if (overrideWeight >= 0f)
				{
					base.knockbackDoer.weight = overrideWeight;
				}
				m_goopDoer.enabled = true;
				m_startGoopRadius = m_goopDoer.defaultGoopRadius;
				m_state = State.Enraged;
			}
		}
		else if (m_state == State.Enraged)
		{
			if (!base.aiAnimator.IsPlaying(enrageAnim))
			{
				base.healthHaver.ManualDeathHandling = true;
				base.aiActor.ForceDeath(Vector2.zero, false);
				Object.Destroy(base.gameObject);
			}
			else
			{
				m_goopDoer.defaultGoopRadius = Mathf.Lerp(m_startGoopRadius, 0.2f, base.aiAnimator.CurrentClipProgress);
			}
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnDamaged -= OnDamaged;
		}
		base.OnDestroy();
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (m_state == State.Normal && resultValue == 1f)
		{
			base.aiAnimator.PlayUntilFinished(transformAnim, true);
			base.healthHaver.ApplyDamageModifiers(onFireDamageTypeModifiers);
			base.healthHaver.SetHealthMaximum(newHealth);
			base.healthHaver.ForceSetCurrentHealth(newHealth);
			base.healthHaver.minimumHealth = 0f;
			base.behaviorSpeculator.InterruptAndDisable();
			base.aiActor.ClearPath();
			base.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "pitfall").anim.Prefix = "pitfall_hot";
			m_state = State.Transforming;
		}
	}
}
