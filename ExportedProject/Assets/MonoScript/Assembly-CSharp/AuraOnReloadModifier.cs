using System;
using UnityEngine;

public class AuraOnReloadModifier : MonoBehaviour
{
	public float AuraRadius = 5f;

	public CoreDamageTypes damageTypes;

	public float DamagePerSecond = 5f;

	public bool IgnitesEnemies;

	public GameActorFireEffect IgniteEffect;

	public bool DoRadialIndicatorAnyway;

	public bool HasRadiusSynergy;

	[LongNumericEnum]
	public CustomSynergyType RadiusSynergy;

	public float RadiusSynergyRadius = 10f;

	private Gun m_gun;

	private Action<AIActor, float> AuraAction;

	private bool m_radialIndicatorActive;

	private HeatIndicatorController m_radialIndicator;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnDropped = (Action)Delegate.Combine(gun.OnDropped, new Action(OnDropped));
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if ((bool)playerController)
		{
			playerController.inventory.OnGunChanged += OnGunChanged;
		}
	}

	private void Update()
	{
		if (m_gun.IsReloading && m_gun.CurrentOwner is PlayerController)
		{
			DoAura();
			if (IgnitesEnemies || DoRadialIndicatorAnyway)
			{
				HandleRadialIndicator();
			}
		}
		else
		{
			UnhandleRadialIndicator();
		}
	}

	private void HandleRadialIndicator()
	{
		if (!m_radialIndicatorActive)
		{
			m_radialIndicatorActive = true;
			m_radialIndicator = ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), m_gun.CurrentOwner.CenterPosition.ToVector3ZisY(), Quaternion.identity, m_gun.CurrentOwner.transform)).GetComponent<HeatIndicatorController>();
			if (!IgnitesEnemies)
			{
				m_radialIndicator.CurrentColor = new Color(0f, 0f, 1f);
				m_radialIndicator.IsFire = false;
			}
		}
	}

	private void UnhandleRadialIndicator()
	{
		if (m_radialIndicatorActive)
		{
			m_radialIndicatorActive = false;
			if ((bool)m_radialIndicator)
			{
				m_radialIndicator.EndEffect();
			}
			m_radialIndicator = null;
		}
	}

	protected virtual void DoAura()
	{
		bool didDamageEnemies = false;
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (AuraAction == null)
		{
			AuraAction = delegate(AIActor actor, float dist)
			{
				float num2 = DamagePerSecond * BraveTime.DeltaTime;
				if (IgnitesEnemies || num2 > 0f)
				{
					didDamageEnemies = true;
				}
				if (IgnitesEnemies)
				{
					actor.ApplyEffect(IgniteEffect);
				}
				actor.healthHaver.ApplyDamage(num2, Vector2.zero, "Aura", damageTypes);
			};
		}
		if (playerController != null && playerController.CurrentRoom != null)
		{
			float num = AuraRadius;
			if (HasRadiusSynergy && playerController.HasActiveBonusSynergy(RadiusSynergy))
			{
				num = RadiusSynergyRadius;
			}
			if ((bool)m_radialIndicator)
			{
				m_radialIndicator.CurrentRadius = num;
			}
			playerController.CurrentRoom.ApplyActionToNearbyEnemies(playerController.CenterPosition, num, AuraAction);
		}
		if (didDamageEnemies)
		{
			playerController.DidUnstealthyAction();
		}
	}

	private void OnDropped()
	{
		UnhandleRadialIndicator();
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if ((bool)playerController)
		{
			playerController.inventory.OnGunChanged -= OnGunChanged;
		}
	}

	private void OnGunChanged(Gun previous, Gun current, Gun previoussecondary, Gun currentsecondary, bool newgun)
	{
		if (current != this && currentsecondary != this)
		{
			UnhandleRadialIndicator();
		}
	}

	private void OnDestroy()
	{
		Gun gun = m_gun;
		gun.OnDropped = (Action)Delegate.Remove(gun.OnDropped, new Action(OnDropped));
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if ((bool)playerController)
		{
			playerController.inventory.OnGunChanged -= OnGunChanged;
		}
	}
}
