using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringGunSynergyProcessor : MonoBehaviour
{
	public enum TriggerStyle
	{
		CONSTANT,
		ON_DAMAGE,
		ON_ACTIVE_ITEM
	}

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	[PickupIdentifier]
	public int TargetGunID;

	public bool UsesMultipleGuns;

	[PickupIdentifier]
	public int[] TargetGunIDs;

	public HoveringGunController.HoverPosition PositionType;

	public HoveringGunController.AimType AimType;

	public HoveringGunController.FireType FireType;

	public float FireCooldown = 1f;

	public float FireDuration = 2f;

	public bool OnlyOnEmptyReload;

	public string ShootAudioEvent;

	public string OnEveryShotAudioEvent;

	public string FinishedShootingAudioEvent;

	public TriggerStyle Trigger;

	public int NumToTrigger = 1;

	public float TriggerDuration = -1f;

	public bool ConsumesTargetGunAmmo;

	public float ChanceToConsumeTargetGunAmmo = 0.5f;

	private bool m_actionsLinked;

	private PlayerController m_cachedLinkedPlayer;

	private Gun m_gun;

	private PassiveItem m_item;

	private List<HoveringGunController> m_hovers = new List<HoveringGunController>();

	private List<bool> m_initialized = new List<bool>();

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		m_item = GetComponent<PassiveItem>();
	}

	private bool IsInitialized(int index)
	{
		return m_initialized.Count > index && m_initialized[index];
	}

	public void Update()
	{
		if (Trigger == TriggerStyle.CONSTANT)
		{
			if ((bool)m_gun)
			{
				if ((bool)m_gun && m_gun.isActiveAndEnabled && (bool)m_gun.CurrentOwner && m_gun.OwnerHasSynergy(RequiredSynergy))
				{
					for (int i = 0; i < NumToTrigger; i++)
					{
						if (!IsInitialized(i))
						{
							Enable(i);
						}
					}
				}
				else
				{
					DisableAll();
				}
			}
			else
			{
				if (!m_item)
				{
					return;
				}
				if ((bool)m_item && (bool)m_item.Owner && m_item.Owner.HasActiveBonusSynergy(RequiredSynergy))
				{
					for (int j = 0; j < NumToTrigger; j++)
					{
						if (!IsInitialized(j))
						{
							Enable(j);
						}
					}
				}
				else
				{
					DisableAll();
				}
			}
		}
		else if (Trigger == TriggerStyle.ON_DAMAGE)
		{
			if (!m_actionsLinked && (bool)m_gun && (bool)m_gun.CurrentOwner)
			{
				(m_cachedLinkedPlayer = m_gun.CurrentOwner as PlayerController).OnReceivedDamage += HandleOwnerDamaged;
				m_actionsLinked = true;
			}
			else if (m_actionsLinked && (bool)m_gun && !m_gun.CurrentOwner && (bool)m_cachedLinkedPlayer)
			{
				m_cachedLinkedPlayer.OnReceivedDamage -= HandleOwnerDamaged;
				m_cachedLinkedPlayer = null;
				m_actionsLinked = false;
			}
		}
		else if (Trigger == TriggerStyle.ON_ACTIVE_ITEM)
		{
			if (!m_actionsLinked && (bool)m_gun && (bool)m_gun.CurrentOwner)
			{
				(m_cachedLinkedPlayer = m_gun.CurrentOwner as PlayerController).OnUsedPlayerItem += HandleOwnerItemUsed;
				m_actionsLinked = true;
			}
			else if (m_actionsLinked && (bool)m_gun && !m_gun.CurrentOwner && (bool)m_cachedLinkedPlayer)
			{
				m_cachedLinkedPlayer.OnUsedPlayerItem -= HandleOwnerItemUsed;
				m_cachedLinkedPlayer = null;
				m_actionsLinked = false;
			}
		}
	}

	private void HandleOwnerItemUsed(PlayerController sourcePlayer, PlayerItem sourceItem)
	{
		if (!sourcePlayer.HasActiveBonusSynergy(RequiredSynergy) || !GetOwner())
		{
			return;
		}
		for (int i = 0; i < NumToTrigger; i++)
		{
			int j;
			for (j = 0; IsInitialized(j); j++)
			{
			}
			Enable(j);
			StartCoroutine(ActiveItemDisable(j, sourcePlayer));
		}
	}

	private void HandleOwnerDamaged(PlayerController sourcePlayer)
	{
		if (!sourcePlayer.HasActiveBonusSynergy(RequiredSynergy))
		{
			return;
		}
		for (int i = 0; i < NumToTrigger; i++)
		{
			int j;
			for (j = 0; IsInitialized(j); j++)
			{
			}
			Enable(j);
			StartCoroutine(TimedDisable(j, TriggerDuration));
		}
	}

	private IEnumerator ActiveItemDisable(int index, PlayerController player)
	{
		yield return null;
		while ((bool)player && (bool)player.CurrentItem && player.CurrentItem.IsActive)
		{
			yield return null;
		}
		Disable(index);
	}

	private IEnumerator TimedDisable(int index, float duration)
	{
		yield return new WaitForSeconds(duration);
		Disable(index);
	}

	private void OnDisable()
	{
		DisableAll();
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

	private void Enable(int index)
	{
		if (m_initialized.Count > index && m_initialized[index])
		{
			return;
		}
		PlayerController owner = GetOwner();
		GameObject gameObject = Object.Instantiate(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, owner.CenterPosition.ToVector3ZisY(), Quaternion.identity);
		gameObject.transform.parent = owner.transform;
		while (m_hovers.Count < index + 1)
		{
			m_hovers.Add(null);
			m_initialized.Add(false);
		}
		m_hovers[index] = gameObject.GetComponent<HoveringGunController>();
		m_hovers[index].ShootAudioEvent = ShootAudioEvent;
		m_hovers[index].OnEveryShotAudioEvent = OnEveryShotAudioEvent;
		m_hovers[index].FinishedShootingAudioEvent = FinishedShootingAudioEvent;
		m_hovers[index].ConsumesTargetGunAmmo = ConsumesTargetGunAmmo;
		m_hovers[index].ChanceToConsumeTargetGunAmmo = ChanceToConsumeTargetGunAmmo;
		m_hovers[index].Position = PositionType;
		m_hovers[index].Aim = AimType;
		m_hovers[index].Trigger = FireType;
		m_hovers[index].CooldownTime = FireCooldown;
		m_hovers[index].ShootDuration = FireDuration;
		m_hovers[index].OnlyOnEmptyReload = OnlyOnEmptyReload;
		Gun gun = null;
		int num = TargetGunID;
		if (UsesMultipleGuns)
		{
			num = TargetGunIDs[index];
		}
		for (int i = 0; i < owner.inventory.AllGuns.Count; i++)
		{
			if (owner.inventory.AllGuns[i].PickupObjectId == num)
			{
				gun = owner.inventory.AllGuns[i];
			}
		}
		if (!gun)
		{
			gun = PickupObjectDatabase.Instance.InternalGetById(num) as Gun;
		}
		m_hovers[index].Initialize(gun, owner);
		m_initialized[index] = true;
	}

	private void Disable(int index)
	{
		if ((bool)m_hovers[index])
		{
			Object.Destroy(m_hovers[index].gameObject);
		}
	}

	private void DisableAll()
	{
		for (int i = 0; i < m_hovers.Count; i++)
		{
			if ((bool)m_hovers[i])
			{
				Object.Destroy(m_hovers[i].gameObject);
			}
		}
		m_hovers.Clear();
		m_initialized.Clear();
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
