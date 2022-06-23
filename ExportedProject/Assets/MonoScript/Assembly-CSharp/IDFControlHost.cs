using System;
using UnityEngine;

public interface IDFControlHost
{
	T AddControl<T>() where T : dfControl;

	dfControl AddControl(Type controlType);

	void AddControl(dfControl child);

	dfControl AddPrefab(GameObject prefab);
}
