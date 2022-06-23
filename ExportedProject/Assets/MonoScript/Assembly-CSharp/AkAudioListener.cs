using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkAudioListener")]
[DisallowMultipleComponent]
[RequireComponent(typeof(AkGameObj))]
public class AkAudioListener : MonoBehaviour
{
	public class BaseListenerList
	{
		private readonly List<ulong> listenerIdList = new List<ulong>();

		private readonly List<AkAudioListener> listenerList = new List<AkAudioListener>();

		public List<AkAudioListener> ListenerList
		{
			get
			{
				return listenerList;
			}
		}

		public virtual bool Add(AkAudioListener listener)
		{
			if (listener == null)
			{
				return false;
			}
			ulong akGameObjectID = listener.GetAkGameObjectID();
			if (listenerIdList.Contains(akGameObjectID))
			{
				return false;
			}
			listenerIdList.Add(akGameObjectID);
			listenerList.Add(listener);
			return true;
		}

		public virtual bool Remove(AkAudioListener listener)
		{
			if (listener == null)
			{
				return false;
			}
			ulong akGameObjectID = listener.GetAkGameObjectID();
			if (!listenerIdList.Contains(akGameObjectID))
			{
				return false;
			}
			listenerIdList.Remove(akGameObjectID);
			listenerList.Remove(listener);
			return true;
		}

		public ulong[] GetListenerIds()
		{
			return listenerIdList.ToArray();
		}
	}

	public class DefaultListenerList : BaseListenerList
	{
		public override bool Add(AkAudioListener listener)
		{
			bool flag = base.Add(listener);
			if (flag && AkSoundEngine.IsInitialized())
			{
				AkSoundEngine.AddDefaultListener(listener.gameObject);
			}
			return flag;
		}

		public override bool Remove(AkAudioListener listener)
		{
			bool flag = base.Remove(listener);
			if (flag && AkSoundEngine.IsInitialized())
			{
				AkSoundEngine.RemoveDefaultListener(listener.gameObject);
			}
			return flag;
		}
	}

	private static readonly DefaultListenerList defaultListeners = new DefaultListenerList();

	private ulong akGameObjectID = ulong.MaxValue;

	private List<AkGameObj> EmittersToStartListeningTo = new List<AkGameObj>();

	private List<AkGameObj> EmittersToStopListeningTo = new List<AkGameObj>();

	public bool isDefaultListener = true;

	[SerializeField]
	public int listenerId;

	public static DefaultListenerList DefaultListeners
	{
		get
		{
			return defaultListeners;
		}
	}

	public void StartListeningToEmitter(AkGameObj emitter)
	{
		EmittersToStartListeningTo.Add(emitter);
		EmittersToStopListeningTo.Remove(emitter);
	}

	public void StopListeningToEmitter(AkGameObj emitter)
	{
		EmittersToStartListeningTo.Remove(emitter);
		EmittersToStopListeningTo.Add(emitter);
	}

	public void SetIsDefaultListener(bool isDefault)
	{
		if (isDefaultListener != isDefault)
		{
			isDefaultListener = isDefault;
			if (isDefault)
			{
				DefaultListeners.Add(this);
			}
			else
			{
				DefaultListeners.Remove(this);
			}
		}
	}

	private void Awake()
	{
		AkGameObj orAddComponent = base.gameObject.GetOrAddComponent<AkGameObj>();
		if ((bool)orAddComponent)
		{
			orAddComponent.Register();
		}
		akGameObjectID = AkSoundEngine.GetAkGameObjectID(base.gameObject);
	}

	private void OnEnable()
	{
		if (isDefaultListener)
		{
			DefaultListeners.Add(this);
		}
	}

	private void OnDisable()
	{
		if (isDefaultListener)
		{
			DefaultListeners.Remove(this);
		}
	}

	private void Update()
	{
		for (int i = 0; i < EmittersToStartListeningTo.Count; i++)
		{
			EmittersToStartListeningTo[i].AddListener(this);
		}
		EmittersToStartListeningTo.Clear();
		for (int j = 0; j < EmittersToStopListeningTo.Count; j++)
		{
			EmittersToStopListeningTo[j].RemoveListener(this);
		}
		EmittersToStopListeningTo.Clear();
	}

	public ulong GetAkGameObjectID()
	{
		return akGameObjectID;
	}

	public void Migrate14()
	{
		bool flag = listenerId == 0;
		Debug.Log("WwiseUnity: AkAudioListener.Migrate14 for " + base.gameObject.name);
		isDefaultListener = flag;
	}
}
