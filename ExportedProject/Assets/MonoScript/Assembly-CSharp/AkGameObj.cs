using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[AddComponentMenu("Wwise/AkGameObj")]
public class AkGameObj : MonoBehaviour
{
	[SerializeField]
	private AkGameObjListenerList m_listeners = new AkGameObjListenerList();

	public bool isEnvironmentAware = true;

	[SerializeField]
	private bool isStaticObject;

	private Collider m_Collider;

	private AkGameObjEnvironmentData m_envData;

	private AkGameObjPositionData m_posData;

	public AkGameObjPositionOffsetData m_positionOffsetData;

	private bool isRegistered;

	[SerializeField]
	private AkGameObjPosOffsetData m_posOffsetData;

	private const int AK_NUM_LISTENERS = 8;

	[SerializeField]
	private int listenerMask = 1;

	public bool IsUsingDefaultListeners
	{
		get
		{
			return m_listeners.useDefaultListeners;
		}
	}

	public List<AkAudioListener> ListenerList
	{
		get
		{
			return m_listeners.ListenerList;
		}
	}

	internal void AddListener(AkAudioListener listener)
	{
		m_listeners.Add(listener);
	}

	internal void RemoveListener(AkAudioListener listener)
	{
		m_listeners.Remove(listener);
	}

	public AKRESULT Register()
	{
		if (isRegistered)
		{
			return AKRESULT.AK_Success;
		}
		isRegistered = true;
		return AkSoundEngine.RegisterGameObj(base.gameObject, base.gameObject.name);
	}

	private void Awake()
	{
		if (!isStaticObject)
		{
			m_posData = new AkGameObjPositionData();
		}
		m_Collider = GetComponent<Collider>();
		if (Register() != AKRESULT.AK_Success)
		{
			return;
		}
		AkSoundEngine.SetObjectPosition(base.gameObject, GetPosition(), GetForward(), GetUpward());
		if (isEnvironmentAware)
		{
			m_envData = new AkGameObjEnvironmentData();
			if ((bool)m_Collider)
			{
				m_envData.AddAkEnvironment(m_Collider, m_Collider);
			}
			m_envData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		m_listeners.Init(this);
	}

	private void CheckStaticStatus()
	{
	}

	private void OnEnable()
	{
		base.enabled = !isStaticObject;
	}

	private void OnDestroy()
	{
		AkUnityEventHandler[] components = base.gameObject.GetComponents<AkUnityEventHandler>();
		AkUnityEventHandler[] array = components;
		foreach (AkUnityEventHandler akUnityEventHandler in array)
		{
			if (akUnityEventHandler.triggerList.Contains(-358577003))
			{
				akUnityEventHandler.DoDestroy();
			}
		}
		if (AkSoundEngine.IsInitialized())
		{
			AkSoundEngine.UnregisterGameObj(base.gameObject);
		}
	}

	private void Update()
	{
		if (m_envData != null)
		{
			m_envData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		if (!isStaticObject)
		{
			Vector3 position = GetPosition();
			Vector3 forward = GetForward();
			Vector3 upward = GetUpward();
			if (!(m_posData.position == position) || !(m_posData.forward == forward) || !(m_posData.up == upward))
			{
				m_posData.position = position;
				m_posData.forward = forward;
				m_posData.up = upward;
				AkSoundEngine.SetObjectPosition(base.gameObject, position, forward, upward);
			}
		}
	}

	public Vector3 GetPosition()
	{
		Vector3 vector2;
		if (m_positionOffsetData != null)
		{
			Vector3 vector = base.transform.rotation * m_positionOffsetData.positionOffset;
			vector2 = base.transform.position + vector;
		}
		else
		{
			vector2 = base.transform.position;
		}
		return vector2.WithZ(vector2.y);
	}

	public virtual Vector3 GetForward()
	{
		return base.transform.forward;
	}

	public virtual Vector3 GetUpward()
	{
		return base.transform.up;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.AddAkEnvironment(other, m_Collider);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.RemoveAkEnvironment(other, m_Collider);
		}
	}
}
