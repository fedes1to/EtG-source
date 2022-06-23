using System;
using System.Collections;
using UnityEngine;

public class ClockhairController : TimeInvariantMonoBehaviour
{
	public float ClockhairInDuration = 2f;

	public float ClockhairSpinDuration = 1f;

	public float ClockhairPauseBeforeShot = 0.5f;

	public Transform hourHandPivot;

	public Transform minuteHandPivot;

	public Transform secondHandPivot;

	public tk2dSpriteAnimator hourAnimator;

	public tk2dSpriteAnimator minuteAnimator;

	public tk2dSpriteAnimator secondAnimator;

	private bool m_shouldDesat;

	private float m_desatRadius;

	private bool m_shouldDistort;

	private float m_distortIntensity;

	private float m_distortRadius;

	private float m_edgeRadius = 20f;

	public bool IsSpinningWildly;

	public bool HasMotionCoroutine;

	private float m_motionType;

	private void Start()
	{
		Initialize();
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unfaded"));
	}

	public void Initialize()
	{
		SetToTime(DateTime.Now.TimeOfDay);
	}

	public void SetMotionType(float motionType)
	{
		if (!IsSpinningWildly)
		{
			if (HasMotionCoroutine)
			{
				m_motionType = motionType;
				return;
			}
			m_motionType = motionType;
			StartCoroutine(HandleSimpleMotion());
		}
	}

	public void UpdateDesat(bool shouldDesat, float desatRadiusUV)
	{
		m_desatRadius = desatRadiusUV;
		if (shouldDesat)
		{
			if (!m_shouldDesat)
			{
				m_shouldDesat = true;
				StartCoroutine(HandleDesat());
			}
		}
		else
		{
			m_shouldDesat = false;
		}
	}

	private IEnumerator HandleDesat()
	{
		Material distMaterial = new Material(ShaderCache.Acquire("Brave/Internal/RadialDesaturateAndDarken"));
		Vector4 distortionSettings2 = GetCenterPointInScreenUV(base.sprite.WorldCenter, 1f, m_desatRadius);
		distMaterial.SetVector("_WaveCenter", distortionSettings2);
		Pixelator.Instance.RegisterAdditionalRenderPass(distMaterial);
		float elapsed = 0f;
		while (m_shouldDesat)
		{
			elapsed += BraveTime.DeltaTime;
			distortionSettings2 = GetCenterPointInScreenUV(base.sprite.WorldCenter, 1f, m_desatRadius);
			distMaterial.SetVector("_WaveCenter", distortionSettings2);
			yield return null;
		}
		Pixelator.Instance.DeregisterAdditionalRenderPass(distMaterial);
		UnityEngine.Object.Destroy(distMaterial);
	}

	public IEnumerator WipeoutDistortionAndFade(float duration)
	{
		m_shouldDesat = false;
		float startDistortValue = m_distortIntensity;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			m_distortIntensity = Mathf.Lerp(startDistortValue, 0f, elapsed / duration);
			yield return null;
		}
		m_shouldDistort = false;
	}

	public void UpdateDistortion(float distortionPower, float distortRadius, float edgeRadius)
	{
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			return;
		}
		if (distortionPower != 0f)
		{
			m_distortIntensity = distortionPower;
			m_distortRadius = distortRadius;
			m_edgeRadius = edgeRadius;
			if (!m_shouldDistort)
			{
				m_shouldDistort = true;
				StartCoroutine(HandleDistortion());
			}
		}
		else
		{
			m_shouldDistort = false;
			m_distortIntensity = 0f;
			m_distortRadius = 25f;
		}
	}

	private IEnumerator HandleDistortion()
	{
		Material distMaterial = new Material(ShaderCache.Acquire("Brave/Internal/DistortionWave"));
		Vector4 distortionSettings2 = GetCenterPointInScreenUV(base.sprite.WorldCenter, m_distortIntensity, m_distortRadius);
		distMaterial.SetVector("_WaveCenter", distortionSettings2);
		Pixelator.Instance.RegisterAdditionalRenderPass(distMaterial);
		float elapsed = 0f;
		while (m_shouldDistort)
		{
			elapsed += BraveTime.DeltaTime;
			distortionSettings2 = GetCenterPointInScreenUV(base.sprite.WorldCenter, m_distortIntensity, m_distortRadius);
			distMaterial.SetVector("_WaveCenter", distortionSettings2);
			distMaterial.SetFloat("_DistortProgress", Mathf.Clamp01(m_edgeRadius / 30f));
			yield return null;
		}
		Pixelator.Instance.DeregisterAdditionalRenderPass(distMaterial);
		UnityEngine.Object.Destroy(distMaterial);
	}

	public Vector4 GetCenterPointInScreenUV(Vector2 centerPoint, float dIntensity, float dRadius)
	{
		Vector3 vector = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(centerPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, dRadius, dIntensity);
	}

	public void BeginSpinningWildly()
	{
		IsSpinningWildly = true;
		StartCoroutine(HandleSpinningWildly());
	}

	private IEnumerator HandleSpinningWildly()
	{
		TimeSpan currentTime = DateTime.Now.TimeOfDay;
		while (IsSpinningWildly)
		{
			currentTime = currentTime.Add(new TimeSpan((long)(BraveTime.DeltaTime * 1E+07f * 60f)));
			SetToTime(currentTime);
			yield return null;
		}
	}

	private IEnumerator HandleSimpleMotion()
	{
		TimeSpan currentTime = DateTime.Now.TimeOfDay;
		HasMotionCoroutine = true;
		while (!IsSpinningWildly && HasMotionCoroutine)
		{
			currentTime = currentTime.Add(new TimeSpan((long)(BraveTime.DeltaTime * 1E+07f * m_motionType)));
			SetToTime(currentTime);
			yield return null;
		}
		HasMotionCoroutine = false;
	}

	public void SpinToSessionStart(float duration)
	{
		float sessionStatValue = GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TIME_PLAYED);
		TimeSpan targetTime = DateTime.Now.TimeOfDay.Subtract(new TimeSpan(0, 0, (int)sessionStatValue));
		StartCoroutine(SpinToTime(targetTime, duration));
	}

	public void SetToTime(TimeSpan time)
	{
		int num = time.Hours % 12;
		int minutes = time.Minutes;
		int seconds = time.Seconds;
		float z = ((float)num / 12f + (float)minutes / 720f) * -360f;
		float z2 = ((float)minutes / 60f + (float)seconds / 3600f) * -360f;
		float z3 = (float)seconds / 60f * -360f;
		if (hourHandPivot != null)
		{
			hourHandPivot.transform.localRotation = Quaternion.Euler(0f, 0f, z);
		}
		if (minuteHandPivot != null)
		{
			minuteHandPivot.transform.localRotation = Quaternion.Euler(0f, 0f, z2);
		}
		if (secondHandPivot != null)
		{
			secondHandPivot.transform.localRotation = Quaternion.Euler(0f, 0f, z3);
		}
	}

	private IEnumerator SpinToTime(TimeSpan targetTime, float duration = 5f)
	{
		TimeSpan currentTime = DateTime.Now.TimeOfDay;
		double secondDiff = currentTime.TotalSeconds - targetTime.TotalSeconds;
		int secondsToMovePerSecond = (int)(secondDiff / (double)duration);
		while (secondDiff > 0.0)
		{
			float adjSecondsToMove = Mathf.Lerp(secondsToMovePerSecond / 10, secondsToMovePerSecond, (float)secondDiff / (float)secondsToMovePerSecond);
			int secondsToMove = Mathf.CeilToInt(adjSecondsToMove * m_deltaTime);
			currentTime = currentTime.Subtract(new TimeSpan(0, 0, secondsToMove));
			SetToTime(currentTime);
			secondDiff = currentTime.TotalSeconds - targetTime.TotalSeconds;
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
