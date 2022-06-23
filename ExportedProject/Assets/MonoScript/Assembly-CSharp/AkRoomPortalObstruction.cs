using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AkRoomPortal))]
[AddComponentMenu("Wwise/AkRoomPortalObstruction")]
public class AkRoomPortalObstruction : AkObstructionOcclusion
{
	private AkRoomPortal m_portal;

	private void Awake()
	{
		InitIntervalsAndFadeRates();
		m_portal = GetComponent<AkRoomPortal>();
	}

	protected override void UpdateObstructionOcclusionValuesForListeners()
	{
		UpdateObstructionOcclusionValues(AkSpatialAudioListener.TheSpatialAudioListener);
	}

	protected override void SetObstructionOcclusion(KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair)
	{
		AkSoundEngine.SetPortalObstruction(m_portal.GetID(), ObsOccPair.Value.currentValue);
	}
}
