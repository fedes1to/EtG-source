using System.Collections.Generic;
using UnityEngine;

public class GoopDefinitionModificationSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool MakesGoopIgnitable;

	public bool ChangesGoopDefinition;

	public GoopDefinition ChangedDefinition;

	private BasicBeamController m_beam;

	private GoopModifier m_gooper;

	private static Dictionary<GoopDefinition, GoopDefinition> m_modifiedGoops = new Dictionary<GoopDefinition, GoopDefinition>();

	public void Awake()
	{
		m_gooper = GetComponent<GoopModifier>();
		int count = -1;
		if (!PlayerController.AnyoneHasActiveBonusSynergy(RequiredSynergy, out count))
		{
			return;
		}
		if (MakesGoopIgnitable && (bool)m_gooper)
		{
			if (!m_modifiedGoops.ContainsKey(m_gooper.goopDefinition))
			{
				GoopDefinition goopDefinition = Object.Instantiate(m_gooper.goopDefinition);
				goopDefinition.CanBeIgnited = true;
				m_modifiedGoops.Add(m_gooper.goopDefinition, goopDefinition);
			}
			m_gooper.goopDefinition = m_modifiedGoops[m_gooper.goopDefinition];
		}
		if (ChangesGoopDefinition && (bool)m_gooper)
		{
			m_gooper.goopDefinition = ChangedDefinition;
		}
	}
}
