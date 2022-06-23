using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AkGameObjListenerList : AkAudioListener.BaseListenerList
{
	[NonSerialized]
	private AkGameObj akGameObj;

	[SerializeField]
	public List<AkAudioListener> initialListenerList = new List<AkAudioListener>();

	[SerializeField]
	public bool useDefaultListeners = true;

	public void SetUseDefaultListeners(bool useDefault)
	{
		if (useDefaultListeners == useDefault)
		{
			return;
		}
		useDefaultListeners = useDefault;
		if (useDefault)
		{
			AkSoundEngine.ResetListenersToDefault(akGameObj.gameObject);
			for (int i = 0; i < base.ListenerList.Count; i++)
			{
				AkSoundEngine.AddListener(akGameObj.gameObject, base.ListenerList[i].gameObject);
			}
		}
		else
		{
			ulong[] listenerIds = GetListenerIds();
			AkSoundEngine.SetListeners(akGameObj.gameObject, listenerIds, (listenerIds != null) ? ((uint)listenerIds.Length) : 0u);
		}
	}

	public void Init(AkGameObj akGameObj)
	{
		this.akGameObj = akGameObj;
		if (!useDefaultListeners)
		{
			AkSoundEngine.SetListeners(akGameObj.gameObject, null, 0u);
		}
		for (int i = 0; i < initialListenerList.Count; i++)
		{
			initialListenerList[i].StartListeningToEmitter(akGameObj);
		}
	}

	public override bool Add(AkAudioListener listener)
	{
		bool flag = base.Add(listener);
		if (flag && AkSoundEngine.IsInitialized())
		{
			AkSoundEngine.AddListener(akGameObj.gameObject, listener.gameObject);
		}
		return flag;
	}

	public override bool Remove(AkAudioListener listener)
	{
		bool flag = base.Remove(listener);
		if (flag && AkSoundEngine.IsInitialized())
		{
			AkSoundEngine.RemoveListener(akGameObj.gameObject, listener.gameObject);
		}
		return flag;
	}
}
