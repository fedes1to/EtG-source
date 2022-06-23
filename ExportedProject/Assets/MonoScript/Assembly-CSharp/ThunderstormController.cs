using System.Collections;
using UnityEngine;

public class ThunderstormController : MonoBehaviour
{
	public bool DoLighting = true;

	public float MinTimeBetweenLightningStrikes = 5f;

	public float MaxTimeBetweenLightningStrikes = 10f;

	public Transform RainSystemTransform;

	public ScreenShakeSettings ThunderShake;

	public bool TrackCamera = true;

	public bool DecayVertical;

	public Vector2 DecayYRange;

	public bool DecayTrackPlayer;

	public Renderer[] LightningRenderers;

	public bool ModifyAmbient;

	public float AmbientBoost = 0.25f;

	public float ZOffset = -20f;

	private Transform m_mainCameraTransform;

	private Vector3 m_lastCameraPosition;

	private ParticleSystem m_system;

	private ParticleSystem.Particle[] m_particles;

	private float m_cachedEmissionRate;

	private Vector3 m_currentWindForce = Vector3.zero;

	private float m_lightningTimer;

	private void Start()
	{
		m_mainCameraTransform = GameManager.Instance.MainCameraController.transform;
		m_lastCameraPosition = m_mainCameraTransform.position;
		RainSystemTransform.position = m_mainCameraTransform.position + new Vector3(0f, 20f, 20f);
		m_lightningTimer = Random.Range(MinTimeBetweenLightningStrikes, MaxTimeBetweenLightningStrikes);
		m_system = RainSystemTransform.GetComponent<ParticleSystem>();
		m_cachedEmissionRate = m_system.emission.rate.constant;
		if (m_particles == null)
		{
			m_particles = new ParticleSystem.Particle[m_system.maxParticles];
		}
	}

	private void Update()
	{
		if (GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if (TrackCamera)
		{
			Vector3 vector = m_mainCameraTransform.transform.position - m_lastCameraPosition;
			m_lastCameraPosition = m_mainCameraTransform.transform.position;
			RainSystemTransform.position += vector;
			RainSystemTransform.position = RainSystemTransform.position.WithZ(RainSystemTransform.position.y + ZOffset);
			if (DecayVertical)
			{
				float y = m_lastCameraPosition.y;
				if (DecayTrackPlayer)
				{
					y = GameManager.Instance.PrimaryPlayer.CenterPosition.y;
				}
				float num = Mathf.Lerp(1f, 0f, (y - DecayYRange.x) / (DecayYRange.y - DecayYRange.x));
				BraveUtility.SetEmissionRate(m_system, m_cachedEmissionRate * num);
			}
		}
		if (m_system.emission.rate.constant > 0f && !TimeTubeCreditsController.IsTimeTubing && (bool)AmmonomiconController.Instance && !AmmonomiconController.Instance.IsOpen)
		{
			AkSoundEngine.PostEvent("Play_ENV_rain_loop_01", base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("Stop_ENV_rain_loop_01", base.gameObject);
		}
		if (DoLighting)
		{
			m_lightningTimer -= ((!GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
			if (m_lightningTimer <= 0f)
			{
				if (!DecayVertical || m_lastCameraPosition.y < DecayYRange.y)
				{
					StartCoroutine(DoLightningStrike());
				}
				for (int i = 0; i < LightningRenderers.Length; i++)
				{
					StartCoroutine(ProcessLightningRenderer(LightningRenderers[i]));
				}
				if (ModifyAmbient)
				{
					StartCoroutine(HandleLightningAmbientBoost());
				}
				m_lightningTimer = Random.Range(MinTimeBetweenLightningStrikes, MaxTimeBetweenLightningStrikes);
			}
		}
		ProcessParticles();
	}

	private void ProcessParticles()
	{
		int particles = m_system.GetParticles(m_particles);
		m_currentWindForce = new Vector3(Mathf.Sin(Time.timeSinceLevelLoad / 20f) * 7f, 0f, 0f);
		Vector3 vector = m_currentWindForce * ((!GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
		for (int i = 0; i < particles; i++)
		{
			m_particles[i].velocity += vector;
		}
		m_system.SetParticles(m_particles, particles);
	}

	protected IEnumerator InvariantWait(float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
	}

	protected IEnumerator HandleLightningAmbientBoost()
	{
		Color cachedAmbient = RenderSettings.ambientLight;
		Color modAmbient = new Color(cachedAmbient.r + AmbientBoost, cachedAmbient.g + AmbientBoost, cachedAmbient.b + AmbientBoost);
		GameManager.Instance.Dungeon.OverrideAmbientLight = true;
		for (int i = 0; i < 2; i++)
		{
			float elapsed = 0f;
			float duration = 0.15f * (float)(i + 1);
			while (elapsed < duration)
			{
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				float t = elapsed / duration;
				GameManager.Instance.Dungeon.OverrideAmbientColor = Color.Lerp(modAmbient, cachedAmbient, t);
				yield return null;
			}
		}
		GameManager.Instance.Dungeon.OverrideAmbientLight = false;
	}

	protected IEnumerator ProcessLightningRenderer(Renderer target)
	{
		target.enabled = true;
		yield return StartCoroutine(InvariantWait(0.05f));
		target.enabled = false;
		yield return StartCoroutine(InvariantWait(0.1f));
		target.enabled = true;
		yield return StartCoroutine(InvariantWait(0.1f));
		target.enabled = false;
	}

	protected IEnumerator DoLightningStrike()
	{
		AkSoundEngine.PostEvent("Play_ENV_thunder_flash_01", base.gameObject);
		PlatformInterface.SetAlienFXColor(new Color(1f, 1f, 1f, 1f), 0.25f);
		Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.05f);
		yield return new WaitForSeconds(0.15f);
		Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.05f);
		yield return new WaitForSeconds(0.1f);
		GameManager.Instance.MainCameraController.DoScreenShake(ThunderShake, null);
	}
}
