using System.Collections;
using UnityEngine;

public class TitleDioramaController : MonoBehaviour
{
	public tk2dSpriteAnimator LichArmAnimator;

	public tk2dSpriteAnimator LichBodyAnimator;

	public tk2dSpriteAnimator LichCapeAnimator;

	public tk2dSprite BeaconBeams;

	public tk2dSprite Eyeholes;

	public MeshRenderer FadeQuad;

	public tk2dSprite PastIslandSprite;

	public MeshRenderer SkyRenderer;

	public Camera m_fadeCamera;

	public GameObject VFX_BulletImpact;

	public GameObject VFX_Splash;

	public GameObject VFX_TrailParticles;

	public GameObject CloudsPrefab;

	public GameObject BackupCloudsPrefab;

	private bool m_manualTrigger;

	private bool m_isRevealed;

	private RenderTexture m_cachedFadeBuffer;

	private bool m_rushed;

	private IEnumerator Start()
	{
		while (Foyer.DoIntroSequence)
		{
			yield return null;
		}
		if (m_fadeCamera != null)
		{
			BraveCameraUtility.MaintainCameraAspect(m_fadeCamera);
		}
		if (Foyer.DoMainMenu && PastIslandSprite == null)
		{
			if (ShouldUseLQ())
			{
				if ((bool)CloudsPrefab)
				{
					CloudsPrefab.SetActive(false);
				}
				if ((bool)BackupCloudsPrefab)
				{
					BackupCloudsPrefab.SetActive(true);
				}
			}
			else
			{
				if ((bool)CloudsPrefab)
				{
					CloudsPrefab.SetActive(true);
				}
				if ((bool)BackupCloudsPrefab)
				{
					BackupCloudsPrefab.SetActive(false);
				}
			}
			StartCoroutine(Core());
			StartCoroutine(HandleLichIdlePhase());
		}
		else if ((bool)FadeQuad)
		{
			FadeQuad.enabled = false;
		}
	}

	private bool ShouldUseLQ()
	{
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			return true;
		}
		return GameManager.Options.GetDefaultRecommendedGraphicalQuality() != GameOptions.GenericHighMedLowOption.HIGH;
	}

	public void CacheFrameToFadeBuffer(Camera cacheCamera)
	{
		FadeQuad.material.SetFloat("_UseAddlTex", 1f);
		FadeQuad.material.SetTexture("_AddlTex", Pixelator.Instance.GetCachedFrame());
	}

	public bool IsRevealed(bool doReveal = false)
	{
		if (doReveal)
		{
			m_rushed = true;
		}
		return m_isRevealed;
	}

	public void ManualTrigger()
	{
		m_manualTrigger = true;
		StartCoroutine(Core(false));
		StartCoroutine(HandleLichIdlePhase());
	}

	private void Update()
	{
		if (m_fadeCamera != null)
		{
			BraveCameraUtility.MaintainCameraAspect(m_fadeCamera);
		}
		if (ShouldUseLQ() && (bool)CloudsPrefab && CloudsPrefab.activeSelf)
		{
			if ((bool)CloudsPrefab)
			{
				CloudsPrefab.SetActive(false);
			}
			if ((bool)BackupCloudsPrefab)
			{
				BackupCloudsPrefab.SetActive(true);
			}
		}
		else if (!ShouldUseLQ() && (bool)BackupCloudsPrefab && BackupCloudsPrefab.activeSelf)
		{
			if ((bool)CloudsPrefab)
			{
				CloudsPrefab.SetActive(true);
			}
			if ((bool)BackupCloudsPrefab)
			{
				BackupCloudsPrefab.SetActive(false);
			}
		}
		if (FadeQuad != null && FadeQuad.enabled && IsRevealed())
		{
			FadeQuad.enabled = false;
		}
	}

	private IEnumerator Core(bool isFoyer = true)
	{
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		GameManager.Instance.MainCameraController.OverridePosition = base.transform.position + new Vector3(0.125f, 0.125f, 0f);
		if (GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
		{
			GameManager.Instance.MainCameraController.OverridePosition = base.transform.position;
		}
		GameManager.Instance.MainCameraController.OverrideZoomScale = 0.5f;
		GameManager.Instance.MainCameraController.CurrentZoomScale = 0.5f;
		Pixelator.Instance.DoOcclusionLayer = false;
		Pixelator.Instance.DoFinalNonFadedLayer = false;
		Pixelator.Instance.CompositePixelatedUnfadedLayer = false;
		if (isFoyer)
		{
			if (FadeQuad != null)
			{
				StartCoroutine(HandleReveal());
			}
		}
		else if (FadeQuad != null)
		{
			FadeQuad.material.SetFloat("_Threshold", 0f);
		}
		bool continueDoing = ((!isFoyer) ? m_manualTrigger : Foyer.DoMainMenu);
		while (continueDoing)
		{
			yield return null;
			if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
			{
				m_rushed = true;
			}
			if ((bool)BraveInput.PlayerlessInstance && BraveInput.PlayerlessInstance.ActiveActions != null && BraveInput.PlayerlessInstance.ActiveActions.IntroSkipActionPressed())
			{
				m_rushed = true;
			}
			continueDoing = ((!isFoyer) ? m_manualTrigger : Foyer.DoMainMenu);
		}
		Pixelator.Instance.DoOcclusionLayer = true;
		GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
		GameManager.Instance.MainCameraController.CurrentZoomScale = 1f;
		GameManager.Instance.MainCameraController.SetManualControl(false, false);
		if (FadeQuad != null)
		{
			FadeQuad.material.SetFloat("_Threshold", 0f);
			Release();
		}
	}

	private void OnDestroy()
	{
		Release();
	}

	private void Release()
	{
		if (m_cachedFadeBuffer != null)
		{
			RenderTexture.ReleaseTemporary(m_cachedFadeBuffer);
			m_cachedFadeBuffer = null;
		}
	}

	private IEnumerator LerpFadeValue(float startValue, float endValue, float duration, bool linearStep = false)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.Lerp(startValue, endValue, elapsed / duration);
			if (linearStep)
			{
				t = Mathf.Lerp(startValue, endValue, BraveMathCollege.HermiteInterpolation(elapsed / duration));
			}
			if ((bool)FadeQuad)
			{
				FadeQuad.material.SetFloat("_Threshold", t);
			}
			if (m_rushed && !linearStep)
			{
				break;
			}
			yield return null;
		}
	}

	private IEnumerator HandleFinalEyeholeEmission()
	{
		yield return StartCoroutine(LerpEyeholeEmission(1800f, 2400f, 0.2f, true));
		yield return StartCoroutine(LerpEyeholeEmission(2400f, 200f, 0.5f, true));
		while (Foyer.DoMainMenu)
		{
			yield return StartCoroutine(LerpEyeholeEmission(200f, 300f, 1f, true));
			yield return StartCoroutine(LerpEyeholeEmission(300f, 200f, 1f, true));
		}
	}

	private IEnumerator LerpEyeholeEmissionColorPower(float startValue, float endValue, float duration, bool really = false)
	{
		float elapsed = 0f;
		Eyeholes.usesOverrideMaterial = true;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			Eyeholes.renderer.material.SetFloat("_EmissiveColorPower", Mathf.Lerp(startValue, endValue, elapsed / duration));
			if (m_rushed && !really)
			{
				break;
			}
			yield return null;
		}
	}

	private IEnumerator LerpEyeholeEmission(float startValue, float endValue, float duration, bool really = false)
	{
		float elapsed = 0f;
		Eyeholes.usesOverrideMaterial = true;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			Eyeholes.renderer.material.SetFloat("_EmissivePower", Mathf.Lerp(startValue, endValue, elapsed / duration));
			if (m_rushed && !really)
			{
				break;
			}
			yield return null;
		}
	}

	public void ForceHideFadeQuad()
	{
		if ((bool)FadeQuad)
		{
			FadeQuad.material.SetFloat("_Threshold", 0f);
			FadeQuad.enabled = false;
		}
	}

	private IEnumerator HandleReveal()
	{
		if ((bool)FadeQuad)
		{
			FadeQuad.enabled = true;
			FadeQuad.material.SetFloat("_Threshold", 1.25f);
		}
		float elapsed2 = 0f;
		while (elapsed2 < 0.25f && !m_rushed)
		{
			elapsed2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		if (!m_rushed)
		{
			yield return StartCoroutine(LerpFadeValue(1.25f, 1f, 1.75f));
		}
		elapsed2 = 0f;
		while (elapsed2 < 0.75f && !m_rushed)
		{
			elapsed2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		if (!m_rushed)
		{
			StartCoroutine(LerpFadeValue(1f, 1.05f, 1.25f));
		}
		if (!m_rushed)
		{
			yield return StartCoroutine(LerpEyeholeEmission(200f, 1800f, 1.1f));
		}
		if (m_rushed)
		{
			StartCoroutine(HandleFinalEyeholeEmission());
			yield return StartCoroutine(LerpFadeValue(1.05f, 0f, 0.3f, true));
		}
		else
		{
			StartCoroutine(HandleFinalEyeholeEmission());
			yield return StartCoroutine(LerpFadeValue(1.05f, 0f, 0.6f, true));
		}
		m_isRevealed = true;
	}

	private IEnumerator HandleLichIdlePhase()
	{
		if ((bool)LichArmAnimator)
		{
			float duration = Random.Range(1f, 3f);
			LichArmAnimator.Play("lich_arm_idle");
			LichBodyAnimator.Play("lich_body_idle");
			LichCapeAnimator.Play("lich_cape_idle");
			yield return new WaitForSeconds(duration);
			if (Random.value < 0.5f)
			{
				StartCoroutine(HandleLichFiddlePhase());
			}
			else
			{
				StartCoroutine(HandleLichCapePhase());
			}
		}
	}

	private IEnumerator HandleLichFiddlePhase()
	{
		int numFiddles = Random.Range(1, 5);
		LichBodyAnimator.Play("lich_body_idle");
		LichCapeAnimator.Play("lich_cape_idle");
		for (int i = 0; i < numFiddles; i++)
		{
			LichArmAnimator.Play("lich_arm_fiddle");
			while (LichArmAnimator.IsPlaying("lich_arm_fiddle"))
			{
				yield return null;
			}
		}
		StartCoroutine(HandleLichIdlePhase());
	}

	private IEnumerator HandleLichCapePhase()
	{
		float duration = Random.Range(8f, 16f);
		LichArmAnimator.Play("lich_arm_windy");
		LichBodyAnimator.Play("lich_body_windy");
		LichCapeAnimator.Play("lich_cape_in");
		yield return new WaitForSeconds(duration);
		LichCapeAnimator.Play("lich_cape_out");
		while (LichCapeAnimator.IsPlaying("lich_cape_out"))
		{
			yield return null;
		}
		StartCoroutine(HandleLichIdlePhase());
	}
}
