using System.Collections;
using UnityEngine;

public class KaliberController : BraveBehaviour
{
	private int m_headsLeft = 3;

	private float m_minHealth = 1f;

	private bool m_isTransitioning;

	public void Start()
	{
		m_minHealth = Mathf.RoundToInt(base.healthHaver.GetMaxHealth() * 0.666f);
		base.healthHaver.minimumHealth = m_minHealth;
	}

	public void Update()
	{
		if (!m_isTransitioning && base.healthHaver.GetCurrentHealth() <= m_minHealth + 0.5f)
		{
			StartCoroutine(DestroyHead());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private IEnumerator DestroyHead()
	{
		m_isTransitioning = true;
		if (base.aiActor.IsFrozen)
		{
			base.aiActor.RemoveEffect("freeze");
		}
		if (base.behaviorSpeculator.IsStunned)
		{
			base.behaviorSpeculator.EndStun();
		}
		base.aiActor.ClearPath();
		base.knockbackDoer.SetImmobile(true, "KaliberController");
		base.behaviorSpeculator.InterruptAndDisable();
		string animName = m_headsLeft + 1 + "_die";
		base.aiAnimator.PlayUntilCancelled(animName);
		base.aiAnimator.PlayVfx("bottom_die");
		base.aiAnimator.IdleAnimation.Prefix = m_headsLeft + "_idle";
		base.aiAnimator.OtherAnimations[0].anim.Prefix = m_headsLeft + "_attack";
		if (m_headsLeft > 1)
		{
			base.specRigidbody.PixelColliders[1].SpecifyBagelFrame = string.Format("kaliber_{0}_idle_001", m_headsLeft);
		}
		else
		{
			base.specRigidbody.PixelColliders[1].SpecifyBagelFrame = "kaliber_1_die_001";
		}
		base.specRigidbody.ForceRegenerate();
		while (base.aiAnimator.IsPlaying(animName))
		{
			yield return null;
		}
		base.aiAnimator.EndAnimation();
		base.knockbackDoer.SetImmobile(false, "KaliberController");
		if (!base.aiActor.IsFrozen)
		{
			base.behaviorSpeculator.enabled = true;
		}
		m_headsLeft--;
		if (m_headsLeft == 2)
		{
			m_minHealth = Mathf.RoundToInt(base.healthHaver.GetMaxHealth() * 0.333f);
			base.healthHaver.minimumHealth = m_minHealth;
		}
		else if (m_headsLeft == 1)
		{
			m_minHealth = 1f;
			base.healthHaver.minimumHealth = m_minHealth;
		}
		else if (m_headsLeft == 0)
		{
			base.aiActor.ParentRoom.DeregisterEnemy(base.aiActor);
			base.aiActor.IgnoreForRoomClear = true;
			base.healthHaver.minimumHealth = 0f;
			base.healthHaver.ApplyDamage(10f, Vector2.zero, "death");
			base.enabled = false;
		}
		AttackBehaviorGroup attackGroup = (AttackBehaviorGroup)base.behaviorSpeculator.AttackBehaviors.Find((AttackBehaviorBase a) => a is AttackBehaviorGroup);
		int enableIndex = 3 - m_headsLeft;
		for (int i = 0; i < attackGroup.AttackBehaviors.Count; i++)
		{
			attackGroup.AttackBehaviors[i].Probability = ((i == enableIndex) ? 1 : 0);
		}
		m_isTransitioning = false;
	}
}
