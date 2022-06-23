using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
[AddComponentMenu("Wwise/AkEmitterObstructionOcclusion")]
public class AkEmitterObstructionOcclusion : AkObstructionOcclusion
{
	private AkGameObj m_gameObj;

	private void Awake()
	{
		InitIntervalsAndFadeRates();
		m_gameObj = GetComponent<AkGameObj>();
	}

	protected override void UpdateObstructionOcclusionValuesForListeners()
	{
		if (AkRoom.IsSpatialAudioEnabled)
		{
			UpdateObstructionOcclusionValues(AkSpatialAudioListener.TheSpatialAudioListener);
			return;
		}
		if (m_gameObj.IsUsingDefaultListeners)
		{
			UpdateObstructionOcclusionValues(AkAudioListener.DefaultListeners.ListenerList);
		}
		UpdateObstructionOcclusionValues(m_gameObj.ListenerList);
	}

	protected override void SetObstructionOcclusion(KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair)
	{
		if (AkRoom.IsSpatialAudioEnabled)
		{
			AkSoundEngine.SetEmitterObstruction(base.gameObject, ObsOccPair.Value.currentValue);
		}
		else
		{
			AkSoundEngine.SetObjectObstructionAndOcclusion(base.gameObject, ObsOccPair.Key.gameObject, 0f, ObsOccPair.Value.currentValue);
		}
	}
}
