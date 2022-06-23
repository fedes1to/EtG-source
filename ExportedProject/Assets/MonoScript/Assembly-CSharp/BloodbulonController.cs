using System.Collections;
using UnityEngine;

public class BloodbulonController : BraveBehaviour
{
	private enum State
	{
		Small,
		Medium,
		Large
	}

	public float bloodbulubMoveSpeed = 2.5f;

	public float bloodbuburstMoveSpeed = 1.5f;

	public float bloodbulubGoopSize = 1.25f;

	public float bloodbuburstGoopSize = 2f;

	private GoopDoer m_goopDoer;

	private tk2dSpriteAnimator m_shadowAnimator;

	private State m_state;

	private bool m_isTransitioning;

	public void Start()
	{
		base.healthHaver.minimumHealth = 0.5f;
		GoopDoer[] components = GetComponents<GoopDoer>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].updateTiming == GoopDoer.UpdateTiming.Always)
			{
				m_goopDoer = components[i];
				break;
			}
		}
		m_shadowAnimator = base.aiActor.ShadowObject.GetComponent<tk2dSpriteAnimator>();
	}

	public void Update()
	{
		if ((bool)base.aiActor && (bool)base.healthHaver && !base.healthHaver.IsDead && !m_isTransitioning)
		{
			if (m_state == State.Small && base.healthHaver.GetCurrentHealthPercentage() <= 0.666f)
			{
				StartCoroutine(GetBigger());
			}
			else if (m_state == State.Medium && base.healthHaver.GetCurrentHealthPercentage() < 0.333f)
			{
				StartCoroutine(GetBigger());
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private IEnumerator GetBigger()
	{
		m_isTransitioning = true;
		float cachedResistance = base.aiActor.GetResistanceForEffectType(EffectResistanceType.Freeze);
		base.aiActor.SetResistance(EffectResistanceType.Freeze, 1000000f);
		string transformAnim = string.Empty;
		if (m_state == State.Small)
		{
			transformAnim = "bloodbulon_grow";
			base.aiAnimator.PlayUntilCancelled(transformAnim);
			m_shadowAnimator.Play("bloodbulon_grow");
			base.aiAnimator.IdleAnimation.Prefix = "bloodbulub_idle";
			for (int i = 0; i < 6; i++)
			{
				base.aiAnimator.MoveAnimation.AnimNames[i] = base.aiAnimator.MoveAnimation.AnimNames[i].Replace("bloodbulon", "bloodbulub");
			}
			base.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "pitfall").anim.Prefix = "bloodbulub_pitfall";
		}
		else if (m_state == State.Medium)
		{
			transformAnim = "bloodbulub_grow";
			base.aiAnimator.PlayUntilCancelled(transformAnim);
			m_shadowAnimator.Play("bloodbulub_grow");
			base.aiAnimator.IdleAnimation.Prefix = "blooduburst_idle";
			base.aiAnimator.MoveAnimation.Type = DirectionalAnimation.DirectionType.Single;
			base.aiAnimator.MoveAnimation.Prefix = "blooduburst_move";
			base.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "pitfall").anim.Prefix = "blooduburst_pitfall";
		}
		float startMoveSpeed = base.aiActor.MovementSpeed;
		float endMoveSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed((m_state != 0) ? bloodbuburstMoveSpeed : bloodbulubMoveSpeed);
		float startGoopSize = m_goopDoer.defaultGoopRadius;
		float endGoopSize = ((m_state != 0) ? bloodbuburstGoopSize : bloodbulubGoopSize);
		while (base.aiAnimator.IsPlaying(transformAnim))
		{
			base.aiActor.MovementSpeed = Mathf.Lerp(startMoveSpeed, endMoveSpeed, base.aiAnimator.CurrentClipProgress);
			m_goopDoer.defaultGoopRadius = Mathf.Lerp(startGoopSize, endGoopSize, base.aiAnimator.CurrentClipProgress);
			yield return null;
		}
		base.aiActor.MovementSpeed = endMoveSpeed;
		m_goopDoer.defaultGoopRadius = endGoopSize;
		base.aiAnimator.EndAnimation();
		if (m_state == State.Small)
		{
			base.specRigidbody.PixelColliders[1].SpecifyBagelFrame = "bloodbulub_idle_001";
		}
		else if (m_state == State.Medium)
		{
			base.specRigidbody.PixelColliders[1].SpecifyBagelFrame = "bloodbuburst_idle_001";
		}
		base.specRigidbody.ForceRegenerate();
		m_state++;
		if (m_state == State.Large)
		{
			base.healthHaver.minimumHealth = 0f;
		}
		if ((bool)base.aiActor)
		{
			base.aiActor.SetResistance(EffectResistanceType.Freeze, cachedResistance);
		}
		m_isTransitioning = false;
	}
}
