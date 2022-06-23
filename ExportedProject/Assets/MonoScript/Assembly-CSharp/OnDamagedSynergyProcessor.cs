using UnityEngine;

public class OnDamagedSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool OnlyArmorDamage;

	public bool DoesRadialBurst;

	public RadialBurstInterface RadialBurst;

	public bool DoesRadialSlow;

	public RadialSlowInterface RadialSlow;

	public string OnTriggeredAudioEvent;

	private bool m_actionsLinked;

	private PlayerController m_cachedLinkedPlayer;

	private Gun m_gun;

	private PassiveItem m_item;

	private float m_cachedArmor;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		m_item = GetComponent<PassiveItem>();
	}

	public void Update()
	{
		PlayerController owner = GetOwner();
		if (!m_actionsLinked && (bool)owner)
		{
			m_cachedLinkedPlayer = owner;
			m_cachedArmor = owner.healthHaver.Armor;
			owner.OnReceivedDamage += HandleOwnerDamaged;
			m_actionsLinked = true;
		}
		else if (m_actionsLinked && !owner && (bool)m_cachedLinkedPlayer)
		{
			m_cachedLinkedPlayer.OnReceivedDamage -= HandleOwnerDamaged;
			m_cachedLinkedPlayer = null;
			m_actionsLinked = false;
		}
		if (m_actionsLinked && (bool)m_cachedLinkedPlayer)
		{
			m_cachedArmor = m_cachedLinkedPlayer.healthHaver.Armor;
		}
	}

	private void HandleOwnerDamaged(PlayerController sourcePlayer)
	{
		if (sourcePlayer.HasActiveBonusSynergy(RequiredSynergy) && (!OnlyArmorDamage || m_cachedArmor != sourcePlayer.healthHaver.Armor))
		{
			if (!string.IsNullOrEmpty(OnTriggeredAudioEvent))
			{
				AkSoundEngine.PostEvent(OnTriggeredAudioEvent, sourcePlayer.gameObject);
			}
			if (DoesRadialBurst)
			{
				RadialBurst.DoBurst(sourcePlayer);
			}
			if (DoesRadialSlow)
			{
				RadialSlow.DoRadialSlow(sourcePlayer.CenterPosition, sourcePlayer.CurrentRoom);
			}
		}
	}

	private PlayerController GetOwner()
	{
		if ((bool)m_gun)
		{
			return m_gun.CurrentOwner as PlayerController;
		}
		if ((bool)m_item)
		{
			return m_item.Owner;
		}
		return null;
	}

	public void OnDestroy()
	{
		if (m_actionsLinked && (bool)m_cachedLinkedPlayer)
		{
			m_cachedLinkedPlayer.OnReceivedDamage -= HandleOwnerDamaged;
			m_cachedLinkedPlayer = null;
			m_actionsLinked = false;
		}
	}
}
