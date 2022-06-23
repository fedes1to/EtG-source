using System;
using UnityEngine;

public class StatHolder : MonoBehaviour
{
	public bool RequiresPlayerItemActive;

	public StatModifier[] modifiers;

	private void Start()
	{
		if (!RequiresPlayerItemActive)
		{
			return;
		}
		PlayerItem component = GetComponent<PlayerItem>();
		if (!component)
		{
			return;
		}
		component.OnActivationStatusChanged = (Action<PlayerItem>)Delegate.Combine(component.OnActivationStatusChanged, (Action<PlayerItem>)delegate(PlayerItem a)
		{
			if ((bool)a.LastOwner)
			{
				a.LastOwner.stats.RecalculateStats(a.LastOwner);
			}
		});
	}
}
