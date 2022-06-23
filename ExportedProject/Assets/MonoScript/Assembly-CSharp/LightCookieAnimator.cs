using UnityEngine;

public class LightCookieAnimator : MonoBehaviour
{
	public Texture2D[] frames;

	public float duration = 4f;

	public float initialDelay = 1f;

	public float additionalScreenShakeDelay = 0.1f;

	public bool doScreenShake;

	[ShowInInspectorIf("doScreenShake", false)]
	public ScreenShakeSettings screenShake;

	private float elapsed;

	private Light m_light;

	private bool m_hasTriggeredSS;

	public bool doVFX;

	public GameObject[] vfxs;

	public float[] vfxTimes;

	private void Start()
	{
		m_light = GetComponent<Light>();
		if (GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.LOW)
		{
			m_light.enabled = false;
		}
		elapsed = -1f * initialDelay;
	}

	private void Update()
	{
		elapsed += BraveTime.DeltaTime;
		float num = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
		if (elapsed >= additionalScreenShakeDelay && doScreenShake && !m_hasTriggeredSS)
		{
			m_hasTriggeredSS = true;
			GameManager.Instance.MainCameraController.DoScreenShake(screenShake, null);
			AkSoundEngine.PostEvent("Play_OBJ_moondoor_close_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		if (doVFX)
		{
			for (int i = 0; i < vfxTimes.Length; i++)
			{
				if (vfxs[i] != null && !vfxs[i].activeSelf && elapsed > vfxTimes[i])
				{
					vfxs[i].SetActive(true);
					vfxs[i] = null;
				}
			}
		}
		if (num == 1f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		int num2 = Mathf.FloorToInt((float)frames.Length * num);
		m_light.cookie = frames[num2];
	}
}
