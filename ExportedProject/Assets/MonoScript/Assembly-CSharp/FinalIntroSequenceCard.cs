using System.Collections;
using UnityEngine;

public class FinalIntroSequenceCard : MonoBehaviour
{
	public Transform StartCameraTransform;

	public Transform EndCameraTransform;

	public Renderer BGRenderer;

	public Renderer[] SpriteRenderers;

	public Renderer GunRenderer;

	public float StartHoldTime = 3f;

	public float PanTime = 5f;

	public float EndHoldTime = 3f;

	public float GunFadeDelay = 3f;

	public float GunFadeTime = 2.5f;

	public float CustomTextFadeInTime = -1f;

	public float CustomTextFadeOutTime = -1f;

	public string[] AssociatedKeys;

	public float[] AssociatedKeyTimes;

	public tk2dBaseSprite borderSprite;

	public AdditionalBraveLight[] additionalBraveLights;

	private float[] blIntensities;

	private float[] blRadii;

	private float m_elapsed;

	public float LightingFadeInDuration = 6f;

	public float LightingFadeOutDuration = 6f;

	public bool LightingReturnToNeutralGray;

	private bool m_hasLightingBeenEnabled;

	private bool m_gunBurn;

	public Transform clockhand1;

	public Transform clockhand2;

	private bool m_clockhandsInitialized;

	public float TotalTime
	{
		get
		{
			return StartHoldTime + PanTime + EndHoldTime;
		}
	}

	public string[] GetTargetKeys(float cardElapsed)
	{
		string[] array = new string[AssociatedKeys.Length];
		for (int i = 0; i < AssociatedKeyTimes.Length; i++)
		{
			if (cardElapsed > AssociatedKeyTimes[i])
			{
				array[i] = AssociatedKeys[i];
			}
			else
			{
				array[i] = string.Empty;
			}
		}
		return array;
	}

	private void Start()
	{
		blIntensities = new float[additionalBraveLights.Length];
		blRadii = new float[additionalBraveLights.Length];
		for (int i = 0; i < additionalBraveLights.Length; i++)
		{
			blIntensities[i] = additionalBraveLights[i].LightIntensity;
			blRadii[i] = additionalBraveLights[i].LightRadius;
		}
		for (int j = 0; j < SpriteRenderers.Length; j++)
		{
			SpriteRenderers[j].transform.localPosition = SpriteRenderers[j].transform.localPosition + CameraController.PLATFORM_CAMERA_OFFSET.WithZ(0f);
		}
	}

	public void SetVisibility(float v)
	{
		if (BGRenderer.material.HasProperty("_AlphaMod"))
		{
			BGRenderer.material.SetFloat("_AlphaMod", v);
		}
		for (int i = 0; i < SpriteRenderers.Length; i++)
		{
			if ((bool)SpriteRenderers[i].GetComponent<tk2dSprite>())
			{
				SpriteRenderers[i].GetComponent<tk2dSprite>().usesOverrideMaterial = true;
			}
			SpriteRenderers[i].material.SetFloat("_AlphaMod", v);
		}
		if (v > 0.5f)
		{
			InitializeClockhands();
			StartCoroutine(HandleGunBurn());
		}
	}

	public void ToggleLighting(bool togglon)
	{
		StartCoroutine(ToggleLightingCR(togglon));
	}

	private IEnumerator ToggleLightingCR(bool togglon)
	{
		if (togglon)
		{
			m_hasLightingBeenEnabled = true;
		}
		float ela = 0f;
		if (!togglon)
		{
			yield return new WaitForSeconds(1f);
		}
		float dura = ((!togglon) ? LightingFadeOutDuration : LightingFadeInDuration);
		Color[] lightSourceColors = new Color[additionalBraveLights.Length];
		Vector3[] lightLocalPositions = new Vector3[additionalBraveLights.Length];
		for (int i = 0; i < additionalBraveLights.Length; i++)
		{
			lightSourceColors[i] = additionalBraveLights[i].LightColor;
			lightLocalPositions[i] = additionalBraveLights[i].transform.localPosition;
		}
		while (ela < dura)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t2 = ((!togglon) ? (1f - ela / dura) : (ela / dura));
			t2 = Mathf.Clamp01(t2);
			for (int j = 0; j < additionalBraveLights.Length; j++)
			{
				if (LightingReturnToNeutralGray && !togglon && m_hasLightingBeenEnabled)
				{
					additionalBraveLights[j].LightColor = Color.Lerp(lightSourceColors[j], new Color(0.9f, 0.7f, 0.2f, 1f), 1f - t2);
					additionalBraveLights[j].transform.localPosition = Vector3.Lerp(lightLocalPositions[j], lightLocalPositions[j] + new Vector3(0f, 8.5f, 0f), 1f - t2);
				}
				additionalBraveLights[j].LightIntensity = Mathf.Lerp(0f, blIntensities[j], t2);
				additionalBraveLights[j].LightRadius = Mathf.Lerp(0f, blRadii[j], t2);
			}
			yield return null;
		}
	}

	private IEnumerator HandleGunBurn()
	{
		if (m_gunBurn || GunRenderer == null)
		{
			yield break;
		}
		GunRenderer.material.SetFloat("_RadialFade", 2f);
		m_gunBurn = true;
		ParticleSystem gunParticles = GunRenderer.GetComponentInChildren<ParticleSystem>(true);
		if ((bool)gunParticles)
		{
			gunParticles.gameObject.SetActive(true);
		}
		float ela2 = 0f;
		float dura2 = GunFadeTime;
		yield return new WaitForSeconds(GunFadeDelay);
		tk2dSprite gunSprite = GunRenderer.GetComponent<tk2dSprite>();
		float gunParticlesPerSecond = 30f;
		float m_elapsedParticles = 0f;
		while (ela2 < dura2)
		{
			ela2 += BraveTime.DeltaTime;
			m_elapsedParticles += BraveTime.DeltaTime * gunParticlesPerSecond;
			float t = ela2 / dura2;
			GunRenderer.material.SetFloat("_RadialFade", 2f - t * 2f);
			GunRenderer.material.SetFloat("_Emission", Mathf.Lerp(10f, 4f, t));
			if ((bool)gunParticles && m_elapsedParticles > 1f)
			{
				Vector2 normalized = (gunSprite.WorldTopLeft - gunSprite.WorldBottomRight).normalized;
				int num = Mathf.FloorToInt(m_elapsedParticles);
				m_elapsedParticles -= (float)num;
				float num2 = 2f - t * 2f;
				if (num2 < 1.55f && (double)num2 > 0.35)
				{
					float num3 = 1f - Mathf.Abs(0.95f - num2) / 0.6f;
					Vector2 vector = Vector2.Lerp(gunSprite.WorldTopRight, gunSprite.WorldBottomLeft, Mathf.InverseLerp(1.55f, 0.35f, num2)) + new Vector2(0f, 0.0625f);
					for (int i = 0; i < num; i++)
					{
						Vector2 vector2 = vector + Random.Range(-1f, 1f) * normalized * num3 * 0.875f;
						ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
						emitParams.position = vector2.ToVector3ZUp(gunParticles.transform.position.z);
						emitParams.velocity = gunParticles.startSpeed * gunParticles.transform.forward;
						emitParams.startSize = gunParticles.startSize;
						emitParams.startLifetime = gunParticles.startLifetime;
						emitParams.startColor = gunParticles.startColor;
						ParticleSystem.EmitParams emitParams2 = emitParams;
						gunParticles.Emit(emitParams2, 1);
					}
				}
			}
			yield return null;
		}
		ela2 = 0f;
		dura2 = 4f;
		while (ela2 < dura2)
		{
			ela2 += BraveTime.DeltaTime;
			float t2 = ela2 / dura2;
			GunRenderer.material.SetFloat("_Emission", Mathf.Lerp(4f, 0f, t2));
			yield return null;
		}
	}

	private void InitializeClockhands()
	{
		if (!m_clockhandsInitialized)
		{
			m_clockhandsInitialized = true;
			if ((bool)clockhand1 && (bool)clockhand2)
			{
				clockhand1.GetComponent<SimpleSpriteRotator>().enabled = true;
				clockhand2.GetComponent<SimpleSpriteRotator>().enabled = true;
				clockhand1.transform.localRotation = Quaternion.Euler(0f, 0f, 135f);
				clockhand2.transform.localRotation = Quaternion.Euler(0f, 0f, 315f);
			}
		}
	}

	private void Update()
	{
		m_elapsed += GameManager.INVARIANT_DELTA_TIME;
		for (int i = 0; i < additionalBraveLights.Length; i++)
		{
		}
	}
}
