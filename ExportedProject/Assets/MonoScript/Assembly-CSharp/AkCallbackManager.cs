using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class AkCallbackManager
{
	public delegate void EventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info);

	public delegate void MonitoringCallback(AkMonitorErrorCode in_errorCode, AkMonitorErrorLevel in_errorLevel, uint in_playingID, ulong in_gameObjID, string in_msg);

	public delegate void BankCallback(uint in_bankID, IntPtr in_InMemoryBankPtr, AKRESULT in_eLoadResult, uint in_memPoolId, object in_Cookie);

	public class EventCallbackPackage
	{
		public bool m_bNotifyEndOfEvent;

		public EventCallback m_Callback;

		public object m_Cookie;

		public uint m_playingID;

		public static EventCallbackPackage Create(EventCallback in_cb, object in_cookie, ref uint io_Flags)
		{
			if (io_Flags == 0 || in_cb == null)
			{
				io_Flags = 0u;
				return null;
			}
			EventCallbackPackage eventCallbackPackage = new EventCallbackPackage();
			eventCallbackPackage.m_Callback = in_cb;
			eventCallbackPackage.m_Cookie = in_cookie;
			eventCallbackPackage.m_bNotifyEndOfEvent = (io_Flags & 1) != 0;
			io_Flags |= 1u;
			m_mapEventCallbacks[eventCallbackPackage.GetHashCode()] = eventCallbackPackage;
			m_LastAddedEventPackage = eventCallbackPackage;
			return eventCallbackPackage;
		}

		~EventCallbackPackage()
		{
			if (m_Cookie != null)
			{
				RemoveEventCallbackCookie(m_Cookie);
			}
		}
	}

	public class BankCallbackPackage
	{
		public BankCallback m_Callback;

		public object m_Cookie;

		public BankCallbackPackage(BankCallback in_cb, object in_cookie)
		{
			m_Callback = in_cb;
			m_Cookie = in_cookie;
			m_mapBankCallbacks[GetHashCode()] = this;
		}
	}

	public delegate AKRESULT BGMCallback(bool in_bOtherAudioPlaying, object in_Cookie);

	public class BGMCallbackPackage
	{
		public BGMCallback m_Callback;

		public object m_Cookie;
	}

	private static readonly AkEventCallbackInfo AkEventCallbackInfo = new AkEventCallbackInfo(IntPtr.Zero, false);

	private static readonly AkDynamicSequenceItemCallbackInfo AkDynamicSequenceItemCallbackInfo = new AkDynamicSequenceItemCallbackInfo(IntPtr.Zero, false);

	private static readonly AkMIDIEventCallbackInfo AkMIDIEventCallbackInfo = new AkMIDIEventCallbackInfo(IntPtr.Zero, false);

	private static readonly AkMarkerCallbackInfo AkMarkerCallbackInfo = new AkMarkerCallbackInfo(IntPtr.Zero, false);

	private static readonly AkDurationCallbackInfo AkDurationCallbackInfo = new AkDurationCallbackInfo(IntPtr.Zero, false);

	private static readonly AkMusicSyncCallbackInfo AkMusicSyncCallbackInfo = new AkMusicSyncCallbackInfo(IntPtr.Zero, false);

	private static readonly AkMusicPlaylistCallbackInfo AkMusicPlaylistCallbackInfo = new AkMusicPlaylistCallbackInfo(IntPtr.Zero, false);

	private static readonly AkAudioSourceChangeCallbackInfo AkAudioSourceChangeCallbackInfo = new AkAudioSourceChangeCallbackInfo(IntPtr.Zero, false);

	private static readonly AkMonitoringCallbackInfo AkMonitoringCallbackInfo = new AkMonitoringCallbackInfo(IntPtr.Zero, false);

	private static readonly AkBankCallbackInfo AkBankCallbackInfo = new AkBankCallbackInfo(IntPtr.Zero, false);

	private static readonly Dictionary<int, EventCallbackPackage> m_mapEventCallbacks = new Dictionary<int, EventCallbackPackage>();

	private static readonly Dictionary<int, BankCallbackPackage> m_mapBankCallbacks = new Dictionary<int, BankCallbackPackage>();

	private static EventCallbackPackage m_LastAddedEventPackage;

	private static IntPtr m_pNotifMem = IntPtr.Zero;

	private static MonitoringCallback m_MonitoringCB;

	private static BGMCallbackPackage ms_sourceChangeCallbackPkg;

	public static void RemoveEventCallback(uint in_playingID)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, EventCallbackPackage> mapEventCallback in m_mapEventCallbacks)
		{
			if (mapEventCallback.Value.m_playingID == in_playingID)
			{
				list.Add(mapEventCallback.Key);
				break;
			}
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			m_mapEventCallbacks.Remove(list[i]);
		}
		AkSoundEnginePINVOKE.CSharp_CancelEventCallback(in_playingID);
	}

	public static void RemoveEventCallbackCookie(object in_cookie)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, EventCallbackPackage> mapEventCallback in m_mapEventCallbacks)
		{
			if (mapEventCallback.Value.m_Cookie == in_cookie)
			{
				list.Add(mapEventCallback.Key);
			}
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			int num = list[i];
			m_mapEventCallbacks.Remove(num);
			AkSoundEnginePINVOKE.CSharp_CancelEventCallbackCookie((IntPtr)num);
		}
	}

	public static void RemoveBankCallback(object in_cookie)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, BankCallbackPackage> mapBankCallback in m_mapBankCallbacks)
		{
			if (mapBankCallback.Value.m_Cookie == in_cookie)
			{
				list.Add(mapBankCallback.Key);
			}
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			int num = list[i];
			m_mapBankCallbacks.Remove(num);
			AkSoundEnginePINVOKE.CSharp_CancelBankCallbackCookie((IntPtr)num);
		}
	}

	public static void SetLastAddedPlayingID(uint in_playingID)
	{
		if (m_LastAddedEventPackage != null && m_LastAddedEventPackage.m_playingID == 0)
		{
			m_LastAddedEventPackage.m_playingID = in_playingID;
		}
	}

	public static AKRESULT Init(int BufferSize)
	{
		m_pNotifMem = ((BufferSize <= 0) ? IntPtr.Zero : Marshal.AllocHGlobal(BufferSize));
		return AkCallbackSerializer.Init(m_pNotifMem, (uint)BufferSize);
	}

	public static void Term()
	{
		if (m_pNotifMem != IntPtr.Zero)
		{
			AkCallbackSerializer.Term();
			Marshal.FreeHGlobal(m_pNotifMem);
			m_pNotifMem = IntPtr.Zero;
		}
	}

	public static void SetMonitoringCallback(AkMonitorErrorLevel in_Level, MonitoringCallback in_CB)
	{
		AkCallbackSerializer.SetLocalOutput((uint)((in_CB != null) ? in_Level : ((AkMonitorErrorLevel)0)));
		m_MonitoringCB = in_CB;
	}

	public static void SetBGMCallback(BGMCallback in_CB, object in_cookie)
	{
		BGMCallbackPackage bGMCallbackPackage = new BGMCallbackPackage();
		bGMCallbackPackage.m_Callback = in_CB;
		bGMCallbackPackage.m_Cookie = in_cookie;
		ms_sourceChangeCallbackPkg = bGMCallbackPackage;
	}

	public static int PostCallbacks()
	{
		if (m_pNotifMem == IntPtr.Zero)
		{
			return 0;
		}
		try
		{
			int num = 0;
			IntPtr intPtr = AkCallbackSerializer.Lock();
			while (intPtr != IntPtr.Zero)
			{
				IntPtr intPtr2 = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_pPackage_get(intPtr);
				AkCallbackType akCallbackType = (AkCallbackType)AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_eType_get(intPtr);
				IntPtr cPtr = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_GetData(intPtr);
				switch (akCallbackType)
				{
				case AkCallbackType.AK_AudioSourceChange:
					if (ms_sourceChangeCallbackPkg != null && ms_sourceChangeCallbackPkg.m_Callback != null)
					{
						AkAudioSourceChangeCallbackInfo.setCPtr(cPtr);
						ms_sourceChangeCallbackPkg.m_Callback(AkAudioSourceChangeCallbackInfo.bOtherAudioPlaying, ms_sourceChangeCallbackPkg.m_Cookie);
					}
					break;
				case AkCallbackType.AK_Monitoring:
					if (m_MonitoringCB != null)
					{
						AkMonitoringCallbackInfo.setCPtr(cPtr);
						m_MonitoringCB(AkMonitoringCallbackInfo.errorCode, AkMonitoringCallbackInfo.errorLevel, AkMonitoringCallbackInfo.playingID, AkMonitoringCallbackInfo.gameObjID, AkMonitoringCallbackInfo.message);
					}
					break;
				case AkCallbackType.AK_Bank:
				{
					BankCallbackPackage value2 = null;
					if (!m_mapBankCallbacks.TryGetValue((int)intPtr2, out value2))
					{
						Debug.LogError("WwiseUnity: BankCallbackPackage not found for <" + intPtr2 + ">.");
						return num;
					}
					m_mapBankCallbacks.Remove((int)intPtr2);
					if (value2 != null && value2.m_Callback != null)
					{
						AkBankCallbackInfo.setCPtr(cPtr);
						value2.m_Callback(AkBankCallbackInfo.bankID, AkBankCallbackInfo.inMemoryBankPtr, AkBankCallbackInfo.loadResult, (uint)AkBankCallbackInfo.memPoolId, value2.m_Cookie);
					}
					break;
				}
				default:
				{
					EventCallbackPackage value = null;
					if (!m_mapEventCallbacks.TryGetValue((int)intPtr2, out value))
					{
						Debug.LogError("WwiseUnity: EventCallbackPackage not found for <" + intPtr2 + ">.");
						return num;
					}
					AkCallbackInfo akCallbackInfo = null;
					switch (akCallbackType)
					{
					case AkCallbackType.AK_EndOfEvent:
						m_mapEventCallbacks.Remove(value.GetHashCode());
						if (value.m_bNotifyEndOfEvent)
						{
							AkEventCallbackInfo.setCPtr(cPtr);
							akCallbackInfo = AkEventCallbackInfo;
						}
						break;
					case AkCallbackType.AK_MusicPlayStarted:
						AkEventCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkEventCallbackInfo;
						break;
					case AkCallbackType.AK_EndOfDynamicSequenceItem:
						AkDynamicSequenceItemCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkDynamicSequenceItemCallbackInfo;
						break;
					case AkCallbackType.AK_MIDIEvent:
						AkMIDIEventCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkMIDIEventCallbackInfo;
						break;
					case AkCallbackType.AK_Marker:
						AkMarkerCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkMarkerCallbackInfo;
						break;
					case AkCallbackType.AK_Duration:
						AkDurationCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkDurationCallbackInfo;
						break;
					case AkCallbackType.AK_MusicSyncBeat:
					case AkCallbackType.AK_MusicSyncBar:
					case AkCallbackType.AK_MusicSyncEntry:
					case AkCallbackType.AK_MusicSyncExit:
					case AkCallbackType.AK_MusicSyncGrid:
					case AkCallbackType.AK_MusicSyncUserCue:
					case AkCallbackType.AK_MusicSyncPoint:
						AkMusicSyncCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkMusicSyncCallbackInfo;
						break;
					case AkCallbackType.AK_MusicPlaylistSelect:
						AkMusicPlaylistCallbackInfo.setCPtr(cPtr);
						akCallbackInfo = AkMusicPlaylistCallbackInfo;
						break;
					default:
						Debug.LogError(string.Concat("WwiseUnity: PostCallbacks aborted due to error: Undefined callback type <", akCallbackType, "> found. Callback object possibly corrupted."));
						return num;
					}
					if (akCallbackInfo != null)
					{
						value.m_Callback(value.m_Cookie, akCallbackType, akCallbackInfo);
					}
					break;
				}
				case AkCallbackType.AK_AudioInterruption:
					break;
				}
				intPtr = AkSoundEnginePINVOKE.CSharp_AkSerializedCallbackHeader_pNext_get(intPtr);
				num++;
			}
			return num;
		}
		finally
		{
			AkCallbackSerializer.Unlock();
		}
	}
}
