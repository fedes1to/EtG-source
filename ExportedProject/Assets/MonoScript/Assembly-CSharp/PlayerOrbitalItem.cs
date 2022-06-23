using System;
using UnityEngine;

public class PlayerOrbitalItem : PassiveItem
{
	public PlayerOrbital OrbitalPrefab;

	public PlayerOrbitalFollower OrbitalFollowerPrefab;

	public bool HasUpgradeSynergy;

	[LongNumericEnum]
	public CustomSynergyType UpgradeSynergy;

	public GameObject UpgradeOrbitalPrefab;

	public GameObject UpgradeOrbitalFollowerPrefab;

	public bool CanBeMimicked;

	[Header("Random Stuff, probably for Ioun Stones")]
	public DamageTypeModifier[] modifiers;

	public DamageTypeModifier[] synergyModifiers;

	public bool BreaksUponContact;

	public bool BreaksUponOwnerDamage;

	public GameObject BreakVFX;

	protected GameObject m_extantOrbital;

	protected bool m_synergyUpgradeActive;

	public static GameObject CreateOrbital(PlayerController owner, GameObject targetOrbitalPrefab, bool isFollower, PlayerOrbitalItem sourceItem = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(targetOrbitalPrefab, owner.transform.position, Quaternion.identity);
		if (!isFollower)
		{
			PlayerOrbital component = gameObject.GetComponent<PlayerOrbital>();
			component.Initialize(owner);
			component.SourceItem = sourceItem;
		}
		else
		{
			PlayerOrbitalFollower component2 = gameObject.GetComponent<PlayerOrbitalFollower>();
			if ((bool)component2)
			{
				component2.Initialize(owner);
			}
		}
		return gameObject;
	}

	private void CreateOrbital(PlayerController owner)
	{
		GameObject targetOrbitalPrefab = ((!(OrbitalPrefab != null)) ? OrbitalFollowerPrefab.gameObject : OrbitalPrefab.gameObject);
		if (HasUpgradeSynergy && m_synergyUpgradeActive)
		{
			targetOrbitalPrefab = ((!(UpgradeOrbitalPrefab != null)) ? UpgradeOrbitalFollowerPrefab.gameObject : UpgradeOrbitalPrefab.gameObject);
		}
		m_extantOrbital = CreateOrbital(owner, targetOrbitalPrefab, OrbitalFollowerPrefab != null, this);
		if (BreaksUponContact && (bool)m_extantOrbital)
		{
			SpeculativeRigidbody component = m_extantOrbital.GetComponent<SpeculativeRigidbody>();
			if ((bool)component)
			{
				component.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(component.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleBreakOnCollision));
			}
		}
		if (BreaksUponOwnerDamage && (bool)owner)
		{
			owner.OnReceivedDamage += HandleBreakOnOwnerDamage;
		}
	}

	private void HandleBreakOnOwnerDamage(PlayerController arg1)
	{
		if ((bool)this)
		{
			if ((bool)BreakVFX && (bool)m_extantOrbital && (bool)m_extantOrbital.GetComponentInChildren<tk2dSprite>())
			{
				SpawnManager.SpawnVFX(BreakVFX, m_extantOrbital.GetComponentInChildren<tk2dSprite>().WorldCenter.ToVector3ZisY(), Quaternion.identity);
			}
			if ((bool)m_owner)
			{
				m_owner.RemovePassiveItem(PickupObjectId);
				m_owner.OnReceivedDamage -= HandleBreakOnOwnerDamage;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void HandleBreakOnCollision(CollisionData rigidbodyCollision)
	{
		if ((bool)m_owner)
		{
			m_owner.RemovePassiveItem(PickupObjectId);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void DecoupleOrbital()
	{
		m_extantOrbital = null;
		if (BreaksUponOwnerDamage && (bool)m_owner)
		{
			m_owner.OnReceivedDamage -= HandleBreakOnOwnerDamage;
		}
	}

	private void DestroyOrbital()
	{
		if ((bool)m_extantOrbital)
		{
			if (BreaksUponOwnerDamage && (bool)m_owner)
			{
				m_owner.OnReceivedDamage -= HandleBreakOnOwnerDamage;
			}
			UnityEngine.Object.Destroy(m_extantOrbital.gameObject);
			m_extantOrbital = null;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!HasUpgradeSynergy)
		{
			return;
		}
		if (m_synergyUpgradeActive && (!m_owner || !m_owner.HasActiveBonusSynergy(UpgradeSynergy)))
		{
			if ((bool)m_owner)
			{
				for (int i = 0; i < synergyModifiers.Length; i++)
				{
					m_owner.healthHaver.damageTypeModifiers.Remove(synergyModifiers[i]);
				}
			}
			m_synergyUpgradeActive = false;
			DestroyOrbital();
			if ((bool)m_owner)
			{
				CreateOrbital(m_owner);
			}
		}
		else if (!m_synergyUpgradeActive && (bool)m_owner && m_owner.HasActiveBonusSynergy(UpgradeSynergy))
		{
			m_synergyUpgradeActive = true;
			DestroyOrbital();
			if ((bool)m_owner)
			{
				CreateOrbital(m_owner);
			}
			for (int j = 0; j < synergyModifiers.Length; j++)
			{
				m_owner.healthHaver.damageTypeModifiers.Add(synergyModifiers[j]);
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		for (int i = 0; i < modifiers.Length; i++)
		{
			player.healthHaver.damageTypeModifiers.Add(modifiers[i]);
		}
		CreateOrbital(player);
	}

	private void HandleNewFloor(PlayerController obj)
	{
		DestroyOrbital();
		CreateOrbital(obj);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DestroyOrbital();
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		for (int i = 0; i < modifiers.Length; i++)
		{
			player.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
		}
		for (int j = 0; j < synergyModifiers.Length; j++)
		{
			player.healthHaver.damageTypeModifiers.Remove(synergyModifiers[j]);
		}
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			PlayerController owner = m_owner;
			owner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(owner.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
			for (int i = 0; i < modifiers.Length; i++)
			{
				m_owner.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
			}
			for (int j = 0; j < synergyModifiers.Length; j++)
			{
				m_owner.healthHaver.damageTypeModifiers.Remove(synergyModifiers[j]);
			}
			m_owner.OnReceivedDamage -= HandleBreakOnOwnerDamage;
		}
		DestroyOrbital();
		base.OnDestroy();
	}
}
