using System;
using System.Collections.Generic;

namespace Dungeonator
{
	public class RuntimeInjectionMetadata
	{
		public SharedInjectionData injectionData;

		public bool forceSecret;

		[NonSerialized]
		public bool HasAssignedModDataExactRoom;

		[NonSerialized]
		public ProceduralFlowModifierData AssignedModifierData;

		public Dictionary<ProceduralFlowModifierData, bool> SucceededRandomizationCheckMap = new Dictionary<ProceduralFlowModifierData, bool>();

		public RuntimeInjectionMetadata(SharedInjectionData data)
		{
			injectionData = data;
		}

		public void CopyMetadata(RuntimeInjectionMetadata other)
		{
			forceSecret = other.forceSecret;
		}
	}
}
