using System;
using System.Collections.Generic;
using UnityEngine;

public class OnActiveItemUsedSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public bool FiresOnActivation;

	[ShowInInspectorIf("FiresOnActivation", false)]
	public RadialBurstInterface ActivationBurst;

	[ShowInInspectorIf("FiresOnActivation", false)]
	public float ActivationBurstCooldown;

	public bool CreatesHoveringGun;

	[ShowInInspectorIf("CreatesHoveringGun", false)]
	public HoveringGunController.HoverPosition PositionType;

	[ShowInInspectorIf("CreatesHoveringGun", false)]
	public HoveringGunController.AimType AimType;

	[ShowInInspectorIf("CreatesHoveringGun", false)]
	public HoveringGunController.FireType FireType;

	[ShowInInspectorIf("CreatesHoveringGun", false)]
	public float HoverDuration = 5f;

	private PlayerItem m_item;

	private float m_internalCooldown;

	private List<HoveringGunController> m_hovers = new List<HoveringGunController>();

	private List<bool> m_hoverInitialized = new List<bool>();

	public void Awake()
	{
		m_item = GetComponent<PlayerItem>();
		PlayerItem item = m_item;
		item.OnActivationStatusChanged = (Action<PlayerItem>)Delegate.Combine(item.OnActivationStatusChanged, new Action<PlayerItem>(HandleActivationStatusChanged));
		PlayerItem item2 = m_item;
		item2.OnPreDropEvent = (Action)Delegate.Combine(item2.OnPreDropEvent, new Action(HandlePreDrop));
	}

	private void HandlePreDrop()
	{
		if (CreatesHoveringGun)
		{
			DisableAllHoveringGuns();
		}
	}

	private void Update()
	{
		m_internalCooldown -= BraveTime.DeltaTime;
		if (CreatesHoveringGun && m_hovers.Count > 0 && (!m_item || !m_item.LastOwner || m_item.LastOwner.CurrentItem != m_item || m_item.LastOwner.IsGhost))
		{
			DisableAllHoveringGuns();
		}
	}

	private void HandleActivationStatusChanged(PlayerItem sourceItem)
	{
		if ((bool)m_item.LastOwner && m_item.LastOwner.HasActiveBonusSynergy(SynergyToCheck))
		{
			if (sourceItem.IsCurrentlyActive)
			{
				HandleActivated();
			}
			else
			{
				HandleDeactivated();
			}
		}
	}

	private void HandleActivated()
	{
		if (FiresOnActivation && m_internalCooldown <= 0f)
		{
			ActivationBurst.DoBurst(m_item.LastOwner);
			m_internalCooldown = ActivationBurstCooldown;
		}
		if (CreatesHoveringGun)
		{
			EnableHoveringGun(0);
		}
	}

	private void HandleDeactivated()
	{
		if (CreatesHoveringGun)
		{
			DisableAllHoveringGuns();
		}
	}

	private void EnableHoveringGun(int index)
	{
		if (m_hoverInitialized.Count <= index || !m_hoverInitialized[index])
		{
			PlayerController lastOwner = m_item.LastOwner;
			GameObject gameObject = UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, lastOwner.CenterPosition.ToVector3ZisY(), Quaternion.identity);
			gameObject.transform.parent = lastOwner.transform;
			while (m_hovers.Count < index + 1)
			{
				m_hovers.Add(null);
				m_hoverInitialized.Add(false);
			}
			m_hovers[index] = gameObject.GetComponent<HoveringGunController>();
			m_hovers[index].Position = PositionType;
			m_hovers[index].Aim = AimType;
			m_hovers[index].Trigger = FireType;
			Gun currentGun = lastOwner.CurrentGun;
			m_hovers[index].CooldownTime = 10f;
			m_hovers[index].ShootDuration = HoverDuration;
			m_hovers[index].Initialize(currentGun, lastOwner);
			m_hoverInitialized[index] = true;
		}
	}

	private void DisableHoveringGun(int index)
	{
		if ((bool)m_hovers[index])
		{
			UnityEngine.Object.Destroy(m_hovers[index].gameObject);
		}
	}

	private void DisableAllHoveringGuns()
	{
		for (int i = 0; i < m_hovers.Count; i++)
		{
			if ((bool)m_hovers[i])
			{
				UnityEngine.Object.Destroy(m_hovers[i].gameObject);
			}
		}
		m_hovers.Clear();
		m_hoverInitialized.Clear();
	}

	private void OnDestroy()
	{
		if (CreatesHoveringGun)
		{
			DisableAllHoveringGuns();
		}
	}
}
