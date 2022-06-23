using System;
using System.Collections;
using UnityEngine;

public class MetalGearRatController : BraveBehaviour
{
	[Serializable]
	public class MetalGearPart
	{
		public AIAnimator AIAnimator;

		public float HealthPercentage = 0.1f;

		public int PixelCollider;

		public int DeathPixelCollider;

		public string AttackName;

		public AutoAimTarget AutoAimer;

		public float BodyDamageOnDeath;
	}

	public MetalGearPart Tailgun;

	public MetalGearPart Radome;

	public float MinBodyDamageHealth = 0.15f;

	public string IconAttackName = "Icon Stomp";

	public float IconAttackProbability = 4f;

	private PixelCollider m_tailgunPixelCollider;

	private bool m_isTailgunDestroyed;

	private PixelCollider m_radomePixelCollider;

	private bool m_isRadomeDestroyed;

	public void Start()
	{
		m_tailgunPixelCollider = base.specRigidbody.PixelColliders[Tailgun.PixelCollider];
		m_radomePixelCollider = base.specRigidbody.PixelColliders[Radome.PixelCollider];
		base.healthHaver.AddTrackedDamagePixelCollider(m_tailgunPixelCollider);
		base.healthHaver.AddTrackedDamagePixelCollider(m_radomePixelCollider);
		base.healthHaver.GlobalPixelColliderDamageMultiplier = 0.25f;
	}

	public void Update()
	{
		if (!m_isTailgunDestroyed)
		{
			float num = 1f;
			float value;
			if (base.healthHaver.PixelColliderDamage.TryGetValue(m_tailgunPixelCollider, out value))
			{
				float num2 = value / base.healthHaver.GetMaxHealth();
				num = 1f - num2 / Tailgun.HealthPercentage;
				if (num2 >= Tailgun.HealthPercentage && (bool)base.behaviorSpeculator && base.behaviorSpeculator.IsInterruptable)
				{
					m_isTailgunDestroyed = true;
					StartCoroutine(DestroyPartCR(Tailgun, "destroy_tailgun"));
					num = 0f;
				}
			}
		}
		if (m_isRadomeDestroyed)
		{
			return;
		}
		float num3 = 1f;
		float value2;
		if (base.healthHaver.PixelColliderDamage.TryGetValue(m_radomePixelCollider, out value2))
		{
			float num4 = value2 / base.healthHaver.GetMaxHealth();
			num3 = 1f - num4 / Radome.HealthPercentage;
			if (num4 > Radome.HealthPercentage && (bool)base.behaviorSpeculator && base.behaviorSpeculator.IsInterruptable)
			{
				m_isRadomeDestroyed = true;
				StartCoroutine(DestroyPartCR(Radome, "destroy_radome"));
				num3 = 0f;
			}
		}
	}

	private IEnumerator DestroyPartCR(MetalGearPart part, string destroyAnim)
	{
		base.behaviorSpeculator.InterruptAndDisable();
		base.aiAnimator.PlayUntilFinished(destroyAnim);
		for (int i = 0; i < part.AIAnimator.OtherAnimations.Count; i++)
		{
			if (!(part.AIAnimator.OtherAnimations[i].name == destroyAnim))
			{
				part.AIAnimator.OtherAnimations[i].anim.Type = DirectionalAnimation.DirectionType.Single;
				part.AIAnimator.OtherAnimations[i].anim.Prefix = "blank";
			}
		}
		part.AIAnimator.IdleAnimation.Prefix = "blank";
		base.specRigidbody.PixelColliders[part.DeathPixelCollider].Enabled = true;
		AttackBehaviorGroup group = base.behaviorSpeculator.AttackBehaviors[0] as AttackBehaviorGroup;
		group.AttackBehaviors.Find((AttackBehaviorGroup.AttackGroupItem a) => a.NickName == part.AttackName).Probability = 0f;
		part.AutoAimer.enabled = false;
		if (part.BodyDamageOnDeath > 0f)
		{
			float a2 = base.healthHaver.GetMaxHealth() * part.BodyDamageOnDeath;
			float num = base.healthHaver.GetMaxHealth() * MinBodyDamageHealth;
			a2 = Mathf.Min(a2, base.healthHaver.GetCurrentHealth() - num);
			if (a2 > 0f)
			{
				base.healthHaver.ApplyDamage(a2, Vector2.zero, "body part destruction", CoreDamageTypes.None, DamageCategory.Unstoppable, true, base.specRigidbody.HitboxPixelCollider, true);
			}
		}
		yield return new WaitForSeconds(base.aiAnimator.CurrentClipLength);
		if (m_isRadomeDestroyed && m_isTailgunDestroyed)
		{
			group.AttackBehaviors.Find((AttackBehaviorGroup.AttackGroupItem a) => a.NickName == IconAttackName).Probability = IconAttackProbability;
		}
		base.behaviorSpeculator.enabled = true;
	}
}
