using AK.Wwise;
using UnityEngine;

[AddComponentMenu("Wwise/AkSpatialAudioEmitter")]
[RequireComponent(typeof(AkGameObj))]
public class AkSpatialAudioEmitter : AkSpatialAudioBase
{
	[Header("Early Reflections")]
	public AuxBus reflectAuxBus;

	public float reflectionMaxPathLength = 1000f;

	[Range(0f, 1f)]
	public float reflectionsAuxBusGain = 1f;

	[Range(1f, 4f)]
	public uint reflectionsOrder = 1u;

	[Range(0f, 1f)]
	[Header("Rooms")]
	public float roomReverbAuxBusGain = 1f;

	private void OnEnable()
	{
		AkEmitterSettings akEmitterSettings = new AkEmitterSettings();
		akEmitterSettings.reflectAuxBusID = (uint)reflectAuxBus.ID;
		akEmitterSettings.reflectionMaxPathLength = reflectionMaxPathLength;
		akEmitterSettings.reflectionsAuxBusGain = reflectionsAuxBusGain;
		akEmitterSettings.reflectionsOrder = reflectionsOrder;
		akEmitterSettings.reflectorFilterMask = uint.MaxValue;
		akEmitterSettings.roomReverbAuxBusGain = roomReverbAuxBusGain;
		akEmitterSettings.useImageSources = 0;
		if (AkSoundEngine.RegisterEmitter(base.gameObject, akEmitterSettings) == AKRESULT.AK_Success)
		{
			SetGameObjectInRoom();
		}
	}

	private void OnDisable()
	{
		AkSoundEngine.UnregisterEmitter(base.gameObject);
	}
}
