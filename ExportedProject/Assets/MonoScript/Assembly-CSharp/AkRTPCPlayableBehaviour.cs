using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class AkRTPCPlayableBehaviour : PlayableBehaviour
{
	private bool m_OverrideTrackObject;

	private RTPC m_Parameter;

	private GameObject m_RTPCObject;

	private bool m_SetRTPCGlobally;

	public float RTPCValue;

	public bool setRTPCGlobally
	{
		set
		{
			m_SetRTPCGlobally = value;
		}
	}

	public bool overrideTrackObject
	{
		set
		{
			m_OverrideTrackObject = value;
		}
	}

	public GameObject rtpcObject
	{
		get
		{
			return m_RTPCObject;
		}
		set
		{
			m_RTPCObject = value;
		}
	}

	public RTPC parameter
	{
		set
		{
			m_Parameter = value;
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!m_OverrideTrackObject)
		{
			GameObject gameObject = playerData as GameObject;
			if (gameObject != null)
			{
				m_RTPCObject = gameObject;
			}
		}
		if (m_Parameter != null)
		{
			if (m_SetRTPCGlobally || m_RTPCObject == null)
			{
				m_Parameter.SetGlobalValue(RTPCValue);
			}
			else
			{
				m_Parameter.SetValue(m_RTPCObject, RTPCValue);
			}
		}
	}
}
