using UnityEngine;

public class InfiniteAmmoSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool PreventsReload = true;

	private bool m_processed;

	private Gun m_gun;

	private float m_cachedReloadTime = -1f;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	public void Update()
	{
		bool flag = (bool)m_gun && m_gun.OwnerHasSynergy(RequiredSynergy);
		if (flag && !m_processed)
		{
			m_gun.GainAmmo(m_gun.AdjustedMaxAmmo);
			m_gun.InfiniteAmmo = true;
			m_processed = true;
			if (PreventsReload)
			{
				m_cachedReloadTime = m_gun.reloadTime;
				m_gun.reloadTime = 0f;
			}
		}
		else if (!flag && m_processed)
		{
			m_gun.InfiniteAmmo = false;
			m_processed = false;
			if (PreventsReload)
			{
				m_gun.reloadTime = m_cachedReloadTime;
			}
		}
	}
}
