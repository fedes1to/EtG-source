using System.Collections.Generic;
using UnityEngine;

public class LootDataGlobalSettings : ScriptableObject
{
	private static LootDataGlobalSettings m_instance;

	public float GunClassModifier = 0.2f;

	[SerializeField]
	public List<GunClassModifierOverride> GunClassOverrides;

	public static LootDataGlobalSettings Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = (LootDataGlobalSettings)BraveResources.Load("GlobalLootSettings", ".asset");
			}
			return m_instance;
		}
	}

	public float GetModifierForClass(GunClass targetClass)
	{
		for (int i = 0; i < GunClassOverrides.Count; i++)
		{
			if (GunClassOverrides[i].classToModify == targetClass)
			{
				return GunClassOverrides[i].modifier;
			}
		}
		return GunClassModifier;
	}
}
