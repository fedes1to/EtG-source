using System;
using Dungeonator;
using UnityEngine;

public class Bloodthirst : MonoBehaviour
{
	private int m_currentNumKillsRequired;

	private int m_currentNumKills;

	private PlayerController m_player;

	private Action<AIActor, float> AuraAction;

	private void Awake()
	{
		m_player = GetComponent<PlayerController>();
		SpeculativeRigidbody specRigidbody = m_player.specRigidbody;
		specRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(specRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(HandlePostRigidbodyMovement));
		m_currentNumKillsRequired = GameManager.Instance.BloodthirstOptions.NumKillsForHealRequiredBase;
		m_currentNumKills = 0;
	}

	private void HandlePostRigidbodyMovement(SpeculativeRigidbody inSrb, Vector2 inVec2, IntVector2 inPixels)
	{
		if (!m_player || m_player.IsGhost || m_player.IsStealthed || Dungeon.IsGenerating || BraveTime.DeltaTime == 0f)
		{
			return;
		}
		RedMatterParticleController redMatterController = GlobalSparksDoer.GetRedMatterController();
		BloodthirstSettings bloodthirstOptions = GameManager.Instance.BloodthirstOptions;
		float radius = bloodthirstOptions.Radius;
		float damagePerSecond = bloodthirstOptions.DamagePerSecond;
		float percentAffected = bloodthirstOptions.PercentAffected;
		int gainPerHeal = bloodthirstOptions.NumKillsAddedPerHealthGained;
		int maxRequired = bloodthirstOptions.NumKillsRequiredCap;
		if (AuraAction == null)
		{
			AuraAction = delegate(AIActor actor, float dist)
			{
				if ((bool)actor && (bool)actor.healthHaver)
				{
					if (!actor.HasBeenBloodthirstProcessed)
					{
						actor.HasBeenBloodthirstProcessed = true;
						actor.CanBeBloodthirsted = UnityEngine.Random.value < percentAffected;
						if (actor.CanBeBloodthirsted && (bool)actor.sprite)
						{
							Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(actor.sprite);
							if (outlineMaterial != null)
							{
								outlineMaterial.SetColor("_OverrideColor", new Color(1f, 0f, 0f));
							}
						}
					}
					if (dist < radius && actor.CanBeBloodthirsted && !actor.IsGone)
					{
						float damage = damagePerSecond * BraveTime.DeltaTime;
						bool isDead = actor.healthHaver.IsDead;
						actor.healthHaver.ApplyDamage(damage, Vector2.zero, "Bloodthirst");
						if (!isDead && actor.healthHaver.IsDead)
						{
							m_currentNumKills++;
							if (m_currentNumKills >= m_currentNumKillsRequired)
							{
								m_currentNumKills = 0;
								if (m_player.healthHaver.GetCurrentHealthPercentage() < 1f)
								{
									m_player.healthHaver.ApplyHealing(0.5f);
									m_currentNumKillsRequired = Mathf.Min(maxRequired, m_currentNumKillsRequired + gainPerHeal);
									GameObject gameObject = BraveResources.Load<GameObject>("Global VFX/VFX_Healing_Sparkles_001");
									if (gameObject != null)
									{
										m_player.PlayEffectOnActor(gameObject, Vector3.zero);
									}
									AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
								}
							}
						}
						GlobalSparksDoer.DoRadialParticleBurst(3, actor.specRigidbody.HitboxPixelCollider.UnitBottomLeft, actor.specRigidbody.HitboxPixelCollider.UnitTopRight, 90f, 4f, 0f, null, null, null, GlobalSparksDoer.SparksType.RED_MATTER);
					}
				}
			};
		}
		if (m_player != null && m_player.CurrentRoom != null)
		{
			m_player.CurrentRoom.ApplyActionToNearbyEnemies(m_player.CenterPosition, 100f, AuraAction);
		}
		if ((bool)redMatterController)
		{
			redMatterController.target.position = m_player.CenterPosition;
			redMatterController.ProcessParticles();
		}
	}
}
