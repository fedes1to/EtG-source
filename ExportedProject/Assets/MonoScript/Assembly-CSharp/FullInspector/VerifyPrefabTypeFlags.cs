using System;

namespace FullInspector
{
	[Flags]
	public enum VerifyPrefabTypeFlags
	{
		None = 1,
		Prefab = 2,
		ModelPrefab = 4,
		PrefabInstance = 8,
		ModelPrefabInstance = 0x10,
		MissingPrefabInstance = 0x20,
		DisconnectedPrefabInstance = 0x40,
		DisconnectedModelPrefabInstance = 0x80
	}
}
