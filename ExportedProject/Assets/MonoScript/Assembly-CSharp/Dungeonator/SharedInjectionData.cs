using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class SharedInjectionData : ScriptableObject
	{
		[SerializeField]
		public List<ProceduralFlowModifierData> InjectionData;

		[SerializeField]
		public bool UseInvalidWeightAsNoInjection = true;

		[SerializeField]
		public bool PreventInjectionOfFailedPrerequisites;

		[SerializeField]
		public bool IsNPCCell;

		[SerializeField]
		public bool IgnoreUnmetPrerequisiteEntries;

		[SerializeField]
		public bool OnlyOne;

		[ShowInInspectorIf("OnlyOne", false)]
		public float ChanceToSpawnOne = 0.5f;

		[SerializeField]
		public List<SharedInjectionData> AttachedInjectionData;
	}
}
