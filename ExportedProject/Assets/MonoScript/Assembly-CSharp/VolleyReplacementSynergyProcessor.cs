using System.Collections.Generic;
using UnityEngine;

public class VolleyReplacementSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public ProjectileVolleyData SynergyVolley;

	private ProjectileVolleyData m_cachedSourceVolley;

	private ProjectileModule m_cachedSingleModule;

	private Gun m_gun;

	private bool m_transformed;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		if (m_gun.RawSourceVolley != null)
		{
			m_cachedSourceVolley = m_gun.RawSourceVolley;
		}
		else
		{
			m_cachedSingleModule = m_gun.singleModule;
		}
	}

	private void Update()
	{
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (!m_transformed && (bool)playerController && playerController.HasActiveBonusSynergy(RequiredSynergy))
		{
			m_transformed = true;
			ProjectileVolleyData volley = m_gun.Volley;
			if ((bool)volley)
			{
				m_gun.RawSourceVolley = DuctTapeItem.TransferDuctTapeModules(volley, SynergyVolley, m_gun);
			}
			else
			{
				m_gun.RawSourceVolley = SynergyVolley;
			}
			playerController.stats.RecalculateStats(playerController);
		}
		else
		{
			if (!m_transformed || ((bool)playerController && playerController.HasActiveBonusSynergy(RequiredSynergy)))
			{
				return;
			}
			m_transformed = false;
			ProjectileVolleyData volley2 = m_gun.Volley;
			if ((bool)volley2)
			{
				ProjectileVolleyData projectileVolleyData = ScriptableObject.CreateInstance<ProjectileVolleyData>();
				if (m_cachedSourceVolley != null)
				{
					projectileVolleyData.InitializeFrom(m_cachedSourceVolley);
				}
				else
				{
					projectileVolleyData.projectiles = new List<ProjectileModule>();
					projectileVolleyData.projectiles.Add(m_cachedSingleModule);
				}
				m_gun.RawSourceVolley = DuctTapeItem.TransferDuctTapeModules(volley2, projectileVolleyData, m_gun);
			}
			else if (m_cachedSourceVolley != null)
			{
				m_gun.RawSourceVolley = m_cachedSourceVolley;
			}
			else
			{
				m_gun.RawSourceVolley = null;
			}
			if ((bool)playerController)
			{
				playerController.stats.RecalculateStats(playerController);
			}
		}
	}
}
