using System.Collections.Generic;
using UnityEngine;

public abstract class AkObstructionOcclusion : MonoBehaviour
{
	protected class ObstructionOcclusionValue
	{
		public float currentValue;

		public float targetValue;

		public bool Update(float fadeRate)
		{
			if (Mathf.Approximately(targetValue, currentValue))
			{
				return false;
			}
			currentValue += fadeRate * Mathf.Sign(targetValue - currentValue) * Time.deltaTime;
			currentValue = Mathf.Clamp(currentValue, 0f, 1f);
			return true;
		}
	}

	private readonly List<AkAudioListener> listenersToRemove = new List<AkAudioListener>();

	private readonly Dictionary<AkAudioListener, ObstructionOcclusionValue> ObstructionOcclusionValues = new Dictionary<AkAudioListener, ObstructionOcclusionValue>();

	protected float fadeRate;

	[Tooltip("Fade time in seconds")]
	public float fadeTime = 0.5f;

	[Tooltip("Layers of obstructers/occluders")]
	public LayerMask LayerMask = -1;

	[Tooltip("Maximum distance to perform the obstruction/occlusion. Negative values mean infinite")]
	public float maxDistance = -1f;

	[Tooltip("The number of seconds between raycasts")]
	public float refreshInterval = 1f;

	private float refreshTime;

	protected void InitIntervalsAndFadeRates()
	{
		refreshTime = Random.Range(0f, refreshInterval);
		fadeRate = 1f / fadeTime;
	}

	protected void UpdateObstructionOcclusionValues(List<AkAudioListener> listenerList)
	{
		for (int i = 0; i < listenerList.Count; i++)
		{
			if (!ObstructionOcclusionValues.ContainsKey(listenerList[i]))
			{
				ObstructionOcclusionValues.Add(listenerList[i], new ObstructionOcclusionValue());
			}
		}
		foreach (KeyValuePair<AkAudioListener, ObstructionOcclusionValue> obstructionOcclusionValue in ObstructionOcclusionValues)
		{
			if (!listenerList.Contains(obstructionOcclusionValue.Key))
			{
				listenersToRemove.Add(obstructionOcclusionValue.Key);
			}
		}
		for (int j = 0; j < listenersToRemove.Count; j++)
		{
			ObstructionOcclusionValues.Remove(listenersToRemove[j]);
		}
	}

	protected void UpdateObstructionOcclusionValues(AkAudioListener listener)
	{
		if (!listener)
		{
			return;
		}
		if (!ObstructionOcclusionValues.ContainsKey(listener))
		{
			ObstructionOcclusionValues.Add(listener, new ObstructionOcclusionValue());
		}
		foreach (KeyValuePair<AkAudioListener, ObstructionOcclusionValue> obstructionOcclusionValue in ObstructionOcclusionValues)
		{
			if (listener != obstructionOcclusionValue.Key)
			{
				listenersToRemove.Add(obstructionOcclusionValue.Key);
			}
		}
		for (int i = 0; i < listenersToRemove.Count; i++)
		{
			ObstructionOcclusionValues.Remove(listenersToRemove[i]);
		}
	}

	private void CastRays()
	{
		if (refreshTime > refreshInterval)
		{
			refreshTime -= refreshInterval;
			foreach (KeyValuePair<AkAudioListener, ObstructionOcclusionValue> obstructionOcclusionValue in ObstructionOcclusionValues)
			{
				AkAudioListener key = obstructionOcclusionValue.Key;
				ObstructionOcclusionValue value = obstructionOcclusionValue.Value;
				Vector3 vector = key.transform.position - base.transform.position;
				float magnitude = vector.magnitude;
				if (maxDistance > 0f && magnitude > maxDistance)
				{
					value.targetValue = value.currentValue;
				}
				else
				{
					value.targetValue = ((!Physics.Raycast(base.transform.position, vector / magnitude, magnitude, LayerMask.value)) ? 0f : 1f);
				}
			}
		}
		refreshTime += Time.deltaTime;
	}

	protected abstract void UpdateObstructionOcclusionValuesForListeners();

	protected abstract void SetObstructionOcclusion(KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair);

	private void Update()
	{
		UpdateObstructionOcclusionValuesForListeners();
		CastRays();
		foreach (KeyValuePair<AkAudioListener, ObstructionOcclusionValue> obstructionOcclusionValue in ObstructionOcclusionValues)
		{
			if (obstructionOcclusionValue.Value.Update(fadeRate))
			{
				SetObstructionOcclusion(obstructionOcclusionValue);
			}
		}
	}
}
