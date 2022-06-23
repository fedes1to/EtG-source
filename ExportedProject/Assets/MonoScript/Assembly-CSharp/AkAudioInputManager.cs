using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AK.Wwise;
using AOT;
using UnityEngine;

public static class AkAudioInputManager
{
	public delegate void AudioFormatDelegate(uint playingID, AkAudioFormat format);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void AudioFormatInteropDelegate(uint playingID, IntPtr format);

	public delegate bool AudioSamplesDelegate(uint playingID, uint channelIndex, float[] samples);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate bool AudioSamplesInteropDelegate(uint playingID, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] float[] samples, uint channelIndex, uint frames);

	private static bool initialized;

	private static readonly Dictionary<uint, AudioSamplesDelegate> audioSamplesDelegates = new Dictionary<uint, AudioSamplesDelegate>();

	private static readonly Dictionary<uint, AudioFormatDelegate> audioFormatDelegates = new Dictionary<uint, AudioFormatDelegate>();

	private static readonly AkAudioFormat audioFormat = new AkAudioFormat();

	private static readonly AudioSamplesInteropDelegate audioSamplesDelegate = InternalAudioSamplesDelegate;

	private static readonly AudioFormatInteropDelegate audioFormatDelegate = InternalAudioFormatDelegate;

	public static uint PostAudioInputEvent(AK.Wwise.Event akEvent, GameObject gameObject, AudioSamplesDelegate sampleDelegate, AudioFormatDelegate formatDelegate = null)
	{
		TryInitialize();
		uint num = akEvent.Post(gameObject, 1u, EventCallback);
		AddPlayingID(num, sampleDelegate, formatDelegate);
		return num;
	}

	public static uint PostAudioInputEvent(uint akEventID, GameObject gameObject, AudioSamplesDelegate sampleDelegate, AudioFormatDelegate formatDelegate = null)
	{
		TryInitialize();
		uint num = AkSoundEngine.PostEvent(akEventID, gameObject, 1u, EventCallback, null);
		AddPlayingID(num, sampleDelegate, formatDelegate);
		return num;
	}

	public static uint PostAudioInputEvent(string akEventName, GameObject gameObject, AudioSamplesDelegate sampleDelegate, AudioFormatDelegate formatDelegate = null)
	{
		TryInitialize();
		uint num = AkSoundEngine.PostEvent(akEventName, gameObject, 1u, EventCallback, null);
		AddPlayingID(num, sampleDelegate, formatDelegate);
		return num;
	}

	[AOT.MonoPInvokeCallback(typeof(AudioSamplesInteropDelegate))]
	private static bool InternalAudioSamplesDelegate(uint playingID, float[] samples, uint channelIndex, uint frames)
	{
		return audioSamplesDelegates.ContainsKey(playingID) && audioSamplesDelegates[playingID](playingID, channelIndex, samples);
	}

	[AOT.MonoPInvokeCallback(typeof(AudioFormatInteropDelegate))]
	private static void InternalAudioFormatDelegate(uint playingID, IntPtr format)
	{
		if (audioFormatDelegates.ContainsKey(playingID))
		{
			audioFormat.setCPtr(format);
			audioFormatDelegates[playingID](playingID, audioFormat);
		}
	}

	private static void TryInitialize()
	{
		if (!initialized)
		{
			initialized = true;
			AkSoundEngine.SetAudioInputCallbacks(audioSamplesDelegate, audioFormatDelegate);
		}
	}

	private static void AddPlayingID(uint playingID, AudioSamplesDelegate sampleDelegate, AudioFormatDelegate formatDelegate)
	{
		if (playingID != 0 && sampleDelegate != null)
		{
			audioSamplesDelegates.Add(playingID, sampleDelegate);
			if (formatDelegate != null)
			{
				audioFormatDelegates.Add(playingID, formatDelegate);
			}
		}
	}

	private static void EventCallback(object cookie, AkCallbackType type, AkCallbackInfo callbackInfo)
	{
		if (type == AkCallbackType.AK_EndOfEvent)
		{
			AkEventCallbackInfo akEventCallbackInfo = callbackInfo as AkEventCallbackInfo;
			if (akEventCallbackInfo != null)
			{
				audioSamplesDelegates.Remove(akEventCallbackInfo.playingID);
				audioFormatDelegates.Remove(akEventCallbackInfo.playingID);
			}
		}
	}
}
