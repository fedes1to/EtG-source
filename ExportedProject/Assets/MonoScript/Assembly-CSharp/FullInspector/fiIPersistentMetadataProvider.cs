using System;
using UnityEngine;

namespace FullInspector
{
	public interface fiIPersistentMetadataProvider
	{
		Type MetadataType { get; }

		void RestoreData(UnityEngine.Object target);

		void Reset(UnityEngine.Object target);
	}
}
