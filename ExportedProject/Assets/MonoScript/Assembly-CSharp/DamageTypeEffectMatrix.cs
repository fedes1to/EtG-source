using System.Collections.Generic;
using UnityEngine;

public class DamageTypeEffectMatrix : ScriptableObject
{
	public List<DamageTypeEffectDefinition> definitions;

	public DamageTypeEffectDefinition GetDefinitionForType(CoreDamageTypes typeFlags)
	{
		for (int i = 0; i < definitions.Count; i++)
		{
			if ((typeFlags & definitions[i].damageType) == definitions[i].damageType)
			{
				return definitions[i];
			}
		}
		return null;
	}
}
