using System.Collections.Generic;
using UnityEngine;

namespace FullInspector
{
	public interface ISerializedObject
	{
		bool IsRestored { get; set; }

		List<Object> SerializedObjectReferences { get; set; }

		List<string> SerializedStateKeys { get; set; }

		List<string> SerializedStateValues { get; set; }

		void RestoreState();

		void SaveState();
	}
}
