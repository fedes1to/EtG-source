using System;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Radar/Radar Marker")]
public class dfRadarMarker : MonoBehaviour
{
	public dfRadarMain radar;

	public string markerType;

	public string outOfRangeType;

	[NonSerialized]
	internal dfControl marker;

	[NonSerialized]
	internal dfControl outOfRangeMarker;

	public void OnEnable()
	{
		if (string.IsNullOrEmpty(markerType))
		{
			return;
		}
		if (radar == null)
		{
			radar = UnityEngine.Object.FindObjectOfType(typeof(dfRadarMain)) as dfRadarMain;
			if (radar == null)
			{
				Debug.LogWarning("No radar found");
				return;
			}
		}
		radar.AddMarker(this);
	}

	public void OnDisable()
	{
		if (radar != null)
		{
			radar.RemoveMarker(this);
		}
	}
}
