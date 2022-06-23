using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Radar/Radar Main")]
public class dfRadarMain : MonoBehaviour
{
	public GameObject target;

	public float maxDetectDistance = 100f;

	public int radarRadius = 100;

	public List<dfControl> markerTypes;

	private List<dfRadarMarker> markers = new List<dfRadarMarker>();

	private dfControl control;

	public void Start()
	{
		ensureControlReference();
		for (int i = 0; i < markerTypes.Count; i++)
		{
			markerTypes[i].IsVisible = false;
		}
	}

	public void LateUpdate()
	{
		updateMarkers();
	}

	public void AddMarker(dfRadarMarker item)
	{
		if (string.IsNullOrEmpty(item.markerType))
		{
			return;
		}
		ensureControlReference();
		item.marker = instantiateMarker(item.markerType);
		if (!(item.marker == null))
		{
			if (!string.IsNullOrEmpty(item.outOfRangeType))
			{
				item.outOfRangeMarker = instantiateMarker(item.outOfRangeType);
			}
			markers.Add(item);
		}
	}

	private dfControl instantiateMarker(string markerName)
	{
		dfControl dfControl2 = markerTypes.Find((dfControl x) => x.name == markerName);
		if (dfControl2 == null)
		{
			Debug.LogError("Marker type not found: " + markerName);
			return null;
		}
		dfControl dfControl3 = UnityEngine.Object.Instantiate(dfControl2);
		dfControl3.hideFlags = HideFlags.DontSave;
		dfControl3.IsVisible = true;
		control.AddControl(dfControl3);
		return dfControl3;
	}

	public void RemoveMarker(dfRadarMarker item)
	{
		if (markers.Remove(item))
		{
			ensureControlReference();
			if (item.marker != null)
			{
				UnityEngine.Object.Destroy(item.marker);
			}
			if (item.outOfRangeMarker != null)
			{
				UnityEngine.Object.Destroy(item.outOfRangeMarker);
			}
			control.RemoveControl(item.marker);
		}
	}

	private void ensureControlReference()
	{
		control = GetComponent<dfControl>();
		if (control == null)
		{
			Debug.LogError("Host control not found");
			base.enabled = false;
		}
		else
		{
			control.Pivot = dfPivotPoint.MiddleCenter;
		}
	}

	private void updateMarkers()
	{
		for (int i = 0; i < markers.Count; i++)
		{
			updateMarker(markers[i]);
		}
	}

	private void updateMarker(dfRadarMarker item)
	{
		Vector3 position = target.transform.position;
		Vector3 position2 = item.transform.position;
		float y = position.x - position2.x;
		float num = position.z - position2.z;
		float num2 = Mathf.Atan2(y, 0f - num) * 57.29578f + 90f + target.transform.eulerAngles.y;
		float num3 = Vector3.Distance(position, position2);
		if (num3 > maxDetectDistance)
		{
			item.marker.IsVisible = false;
			if (item.outOfRangeMarker != null)
			{
				dfControl outOfRangeMarker = item.outOfRangeMarker;
				outOfRangeMarker.IsVisible = true;
				outOfRangeMarker.transform.position = control.transform.position;
				outOfRangeMarker.transform.eulerAngles = new Vector3(0f, 0f, num2 - 90f);
			}
			return;
		}
		if (item.outOfRangeMarker != null)
		{
			item.outOfRangeMarker.IsVisible = false;
		}
		float num4 = num3 * Mathf.Cos(num2 * ((float)Math.PI / 180f));
		float num5 = num3 * Mathf.Sin(num2 * ((float)Math.PI / 180f));
		float num6 = (float)radarRadius / maxDetectDistance * control.PixelsToUnits();
		num4 *= num6;
		num5 *= num6;
		item.marker.transform.localPosition = new Vector3(num4, num5, 0f);
		item.marker.IsVisible = true;
		item.marker.Pivot = dfPivotPoint.MiddleCenter;
	}
}
