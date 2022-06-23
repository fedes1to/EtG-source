using System.Collections.Generic;
using UnityEngine;

public class GunInventory
{
	public delegate void OnGunChangedEvent(Gun previous, Gun current, Gun previousSecondary, Gun currentSecondary, bool newGun);

	public bool GunChangeForgiveness;

	public List<GunClass> m_gunClassOverrides = new List<GunClass>();

	private GameActor m_owner;

	private Gun m_currentGun;

	private OverridableBool m_dualWielding = new OverridableBool(false);

	private Gun m_currentSecondaryGun;

	private int m_maxGuns = -1;

	public OverridableBool GunLocked = new OverridableBool(false);

	private List<Gun> m_guns;

	private Dictionary<Gun, float> m_gunStowedTime;

	private List<float> m_perGunDrainData;

	public Gun CurrentGun
	{
		get
		{
			if (ForceNoGun)
			{
				return null;
			}
			return m_currentGun;
		}
	}

	public Gun CurrentSecondaryGun
	{
		get
		{
			if (!DualWielding)
			{
				return null;
			}
			if (ForceNoGun)
			{
				return null;
			}
			return m_currentSecondaryGun;
		}
	}

	public GameActor Owner
	{
		get
		{
			return m_owner;
		}
	}

	public bool DualWielding
	{
		get
		{
			return m_dualWielding.Value;
		}
	}

	public List<Gun> AllGuns
	{
		get
		{
			return m_guns;
		}
	}

	public int GunCountModified
	{
		get
		{
			int num = 0;
			for (int i = 0; i < m_guns.Count; i++)
			{
				if (!m_guns[i].name.StartsWith("ArtfulDodger"))
				{
					num++;
				}
			}
			return num;
		}
	}

	public int maxGuns
	{
		get
		{
			return m_maxGuns;
		}
		set
		{
			m_maxGuns = value;
		}
	}

	public bool ForceNoGun { get; set; }

	public event OnGunChangedEvent OnGunChanged;

	public GunInventory(GameActor owner)
	{
		m_owner = owner;
		m_guns = new List<Gun>();
		m_perGunDrainData = new List<float>();
		m_gunStowedTime = new Dictionary<Gun, float>();
	}

	public void SetDualWielding(bool value, string reason)
	{
		bool value2 = m_dualWielding.Value;
		Gun gun = ((!value2) ? null : CurrentSecondaryGun);
		m_dualWielding.SetOverride(reason, value);
		if (value2 && !m_dualWielding.Value && (bool)gun)
		{
			if (!gun.IsPreppedForThrow)
			{
				gun.CeaseAttack(false);
			}
			gun.OnPrePlayerChange();
			gun.gameObject.SetActive(false);
		}
	}

	public bool ContainsGun(int gunID)
	{
		for (int i = 0; i < m_guns.Count; i++)
		{
			if (m_guns[i].PickupObjectId == gunID)
			{
				return true;
			}
		}
		return false;
	}

	public int ContainsGunOfClass(GunClass targetClass, bool respectsOverrides)
	{
		int num = 0;
		if (respectsOverrides && m_gunClassOverrides.Contains(targetClass))
		{
			return 0;
		}
		for (int i = 0; i < m_guns.Count; i++)
		{
			if (m_guns[i].gunClass == targetClass)
			{
				num++;
			}
		}
		return num;
	}

	public void RegisterGunClassOverride(GunClass overridden)
	{
		if (!m_gunClassOverrides.Contains(overridden))
		{
			m_gunClassOverrides.Add(overridden);
		}
	}

	public void DeregisterGunClassOverride(GunClass overridden)
	{
		m_gunClassOverrides.Remove(overridden);
	}

	public void HandleAmmoDrain(float percentAmmoDrain)
	{
		for (int i = 0; i < m_guns.Count; i++)
		{
			if (m_guns[i].AdjustedMaxAmmo > 0 && m_guns[i].ammo > 0)
			{
				m_perGunDrainData[i] += percentAmmoDrain;
				int num = Mathf.FloorToInt((float)m_guns[i].AdjustedMaxAmmo * m_perGunDrainData[i]);
				if (num >= 1)
				{
					float num2 = (float)num / (float)m_guns[i].AdjustedMaxAmmo;
					m_perGunDrainData[i] -= num2;
					m_guns[i].LoseAmmo(num);
				}
			}
		}
	}

	public void ClearAmmoDrain()
	{
		for (int i = 0; i < m_guns.Count; i++)
		{
			m_perGunDrainData[i] = 0f;
		}
	}

	public void FrameUpdate()
	{
		for (int i = 0; i < AllGuns.Count; i++)
		{
			if (AllGuns[i] == CurrentGun)
			{
				m_gunStowedTime[AllGuns[i]] = 0f;
				continue;
			}
			m_gunStowedTime[AllGuns[i]] += BraveTime.DeltaTime;
			if (m_gunStowedTime[AllGuns[i]] > 2f * AllGuns[i].reloadTime)
			{
				AllGuns[i].ForceImmediateReload();
				m_gunStowedTime[AllGuns[i]] = -1000f;
			}
		}
	}

	public Gun AddGunToInventory(Gun gun, bool makeActive = false)
	{
		if ((bool)gun && gun.ShouldBeDestroyedOnExistence(!(m_owner is PlayerController)))
		{
			return null;
		}
		Gun ownedCopy = GetOwnedCopy(gun);
		if (ownedCopy != null)
		{
			ownedCopy.GainAmmo(gun);
			return ownedCopy;
		}
		if (!gun.name.StartsWith("ArtfulDodger") && maxGuns > 0 && GunCountModified >= maxGuns)
		{
			if (!(m_owner is PlayerController))
			{
				return null;
			}
			Gun currentGun = m_owner.CurrentGun;
			RemoveGunFromInventory(currentGun);
			currentGun.DropGun();
		}
		Gun gun2 = CreateGunForAdd(gun);
		gun2.HasBeenPickedUp = true;
		gun2.HasProcessedStatMods = gun.HasProcessedStatMods;
		gun2.CopyStateFrom(gun);
		m_guns.Add(gun2);
		m_perGunDrainData.Add(0f);
		m_gunStowedTime.Add(gun2, 0f);
		if (m_guns.Count == 1)
		{
			m_currentGun = m_guns[0];
			ChangeGun(0, true);
		}
		if (makeActive)
		{
			int amt = m_guns.Count - 1 - m_guns.IndexOf(m_currentGun);
			ChangeGun(amt, true);
			gun2.HandleSpriteFlip(m_owner.SpriteFlipped);
		}
		if (m_owner is PlayerController)
		{
			(m_owner as PlayerController).stats.RecalculateStats(m_owner as PlayerController);
		}
		return gun2;
	}

	public Gun GetTargetGunWithChange(int amt)
	{
		if (m_guns.Count == 0)
		{
			return null;
		}
		int num = m_guns.IndexOf(m_currentGun);
		for (num += amt; num < 0; num += m_guns.Count)
		{
		}
		num %= m_guns.Count;
		return m_guns[num];
	}

	public void SwapDualGuns()
	{
		if (DualWielding && (bool)m_currentSecondaryGun && (bool)m_currentGun)
		{
			Gun currentGun = m_currentGun;
			Gun currentSecondaryGun = m_currentSecondaryGun;
			m_currentGun = m_currentSecondaryGun;
			m_currentSecondaryGun = currentGun;
			m_currentGun.OnEnable();
			m_currentSecondaryGun.OnEnable();
			m_currentGun.HandleSpriteFlip(m_currentGun.CurrentOwner.SpriteFlipped);
			m_currentSecondaryGun.HandleSpriteFlip(m_currentSecondaryGun.CurrentOwner.SpriteFlipped);
			if (this.OnGunChanged != null)
			{
				this.OnGunChanged(currentGun, m_currentGun, currentSecondaryGun, CurrentSecondaryGun, false);
			}
		}
	}

	public void ChangeGun(int amt, bool newGun = false, bool overrideGunLock = false)
	{
		if (m_guns.Count == 0 || (m_currentGun != null && m_currentGun.UnswitchableGun) || (GunLocked.Value && !overrideGunLock))
		{
			return;
		}
		Gun currentGun = m_currentGun;
		Gun currentSecondaryGun = m_currentSecondaryGun;
		if (m_currentGun != null && !ForceNoGun)
		{
			if (!m_currentGun.IsPreppedForThrow)
			{
				CurrentGun.CeaseAttack(false);
			}
			m_currentGun.OnPrePlayerChange();
			m_currentGun.gameObject.SetActive(false);
		}
		if (DualWielding && (bool)CurrentSecondaryGun)
		{
			if (!CurrentSecondaryGun.IsPreppedForThrow)
			{
				CurrentSecondaryGun.CeaseAttack(false);
			}
			CurrentSecondaryGun.OnPrePlayerChange();
			CurrentSecondaryGun.gameObject.SetActive(false);
		}
		int num = m_guns.IndexOf(m_currentGun);
		for (num += amt; num < 0; num += m_guns.Count)
		{
		}
		num %= m_guns.Count;
		m_currentGun = m_guns[num];
		m_currentGun.gameObject.SetActive(true);
		if (DualWielding)
		{
			if (m_guns.Count <= 1)
			{
				m_currentSecondaryGun = null;
			}
			if ((m_currentSecondaryGun == null || m_currentSecondaryGun == m_currentGun) && m_guns.Count > 1)
			{
				m_currentSecondaryGun = m_guns[(num + 1) % m_guns.Count];
			}
			if ((bool)CurrentSecondaryGun)
			{
				CurrentSecondaryGun.gameObject.SetActive(true);
			}
		}
		if (this.OnGunChanged != null)
		{
			this.OnGunChanged(currentGun, m_currentGun, currentSecondaryGun, CurrentSecondaryGun, newGun);
		}
	}

	public Gun CreateGunForAdd(Gun gunPrototype)
	{
		GameObject gameObject = Object.Instantiate(gunPrototype.gameObject);
		gameObject.name = gunPrototype.name;
		Gun component = gameObject.GetComponent<Gun>();
		if (!component.enabled)
		{
			component.enabled = true;
		}
		component.prefabName = ((!(gunPrototype.prefabName == string.Empty)) ? gunPrototype.prefabName : gunPrototype.name);
		Transform gunPivot = m_owner.GunPivot;
		IGunInheritable[] interfaces = gameObject.GetInterfaces<IGunInheritable>();
		if (interfaces != null)
		{
			for (int i = 0; i < interfaces.Length; i++)
			{
				interfaces[i].InheritData(gunPrototype);
			}
		}
		gameObject.transform.parent = gunPivot;
		if (component.PrimaryHandAttachPoint != null)
		{
			gameObject.transform.localPosition = -component.PrimaryHandAttachPoint.localPosition;
		}
		gameObject.SetActive(false);
		component.Initialize(m_owner);
		if (!gunPrototype.HasBeenPickedUp && gunPrototype.ArmorToGainOnPickup > 0)
		{
			m_owner.healthHaver.Armor += gunPrototype.ArmorToGainOnPickup;
		}
		if (!gunPrototype.HasBeenPickedUp && !component.InfiniteAmmo)
		{
			float num = (float)component.AdjustedMaxAmmo / (float)component.GetBaseMaxAmmo();
			int num2 = Mathf.CeilToInt(num * (float)component.ammo);
			if (num2 > component.ammo)
			{
				component.GainAmmo(num2 - component.ammo);
			}
			else if (num2 < component.ammo)
			{
				component.LoseAmmo(component.ammo - num2);
			}
		}
		if ((bool)component && component.DefaultModule != null && (bool)m_owner && m_owner is AIActor && component.DefaultModule.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			component.DefaultModule.projectiles = new List<Projectile>();
			component.DefaultModule.projectiles.Add(component.DefaultModule.GetChargeProjectile(1000f).Projectile);
			component.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
		}
		return component;
	}

	public void DestroyGun(Gun g)
	{
		RemoveGunFromInventory(g);
		Object.Destroy(g.gameObject);
	}

	public void DestroyCurrentGun()
	{
		Gun currentGun = m_currentGun;
		if (currentGun != null)
		{
			RemoveGunFromInventory(currentGun);
			Object.Destroy(currentGun.gameObject);
		}
	}

	public void DestroyAllGuns()
	{
		int num;
		for (num = 0; num < m_guns.Count; num++)
		{
			Gun gun = m_guns[num];
			RemoveGunFromInventory(gun);
			Object.Destroy(gun.gameObject);
			num--;
		}
		GunLocked.ClearOverrides();
	}

	public void RemoveGunFromInventory(Gun gun)
	{
		Gun ownedCopy = GetOwnedCopy(gun);
		if (ownedCopy == null)
		{
			Debug.Log("Removing unknown gun " + gun.gunName + " from player inventory!");
			return;
		}
		bool flag = (ownedCopy == CurrentGun || ownedCopy == CurrentSecondaryGun) && DualWielding;
		bool flag2 = flag && ownedCopy == CurrentGun;
		int num = m_guns.IndexOf(ownedCopy);
		int num2 = m_guns.IndexOf(m_currentGun);
		if (flag)
		{
			if (flag2)
			{
				m_currentGun = m_currentSecondaryGun;
				m_currentSecondaryGun = null;
				m_currentGun.OnEnable();
				m_dualWielding.ClearOverrides();
				ChangeGun(0);
			}
		}
		else if (num == num2 && m_guns.Count > 1)
		{
			ChangeGun(-1, false, true);
		}
		else if (num == num2)
		{
			num2 = -1;
			m_currentGun = null;
		}
		m_guns.RemoveAt(num);
		m_perGunDrainData.RemoveAt(num);
		m_gunStowedTime.Remove(ownedCopy);
		if (m_owner is PlayerController)
		{
			(m_owner as PlayerController).stats.RecalculateStats(m_owner as PlayerController);
		}
	}

	private Gun GetOwnedCopy(Gun w)
	{
		Gun result = null;
		for (int i = 0; i < m_guns.Count; i++)
		{
			if (m_guns[i].PickupObjectId == w.PickupObjectId)
			{
				result = m_guns[i];
				break;
			}
		}
		return result;
	}
}
