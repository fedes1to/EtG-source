using System;
using UnityEngine;

public class SpotLightHelper : TimeInvariantMonoBehaviour
{
	[Header("Inital Position and Intensity")]
	public bool pointDirectlyAtFloor;

	public float zHeightOffset = -30f;

	[Header("Cookie Rotation")]
	public bool randomStartingRotation;

	public bool swayRotation;

	public float swaySpeed = 0.18f;

	public float rotationMin;

	public float rotationMax;

	[Header("Constant Rotation")]
	public float rotationSpeed;

	[Header("Inital Spot/Cookie Angle")]
	public bool randomStartingCookieAngle;

	public bool pulseCookieAngle;

	public float pulseCookieAngleHang = 1f;

	public float pulseCookieAngleSpeed4Real = 1f;

	public float cookieAngleMin;

	public float cookieAngleMax;

	[Header("Light Intensity")]
	public bool randomIntensity;

	public bool pulseIntensity;

	public float pulseIntensityHang;

	public float pulseIntensitySpeed4Real = 10f;

	public float intensityMin;

	public float intensityMax;

	[Header("Ambient Light Ping Pong")]
	public bool doPingPong;

	public Color startColor = Color.blue;

	public Color endColor = Color.red;

	public float pingPongTime = 2f;

	public float otherNumber = 1.5f;

	protected Transform m_transform;

	protected float magicNumberAngle;

	protected Light m_light;

	private void Start()
	{
		m_transform = base.transform;
		m_transform.position = m_transform.position.WithZ(m_transform.position.z + zHeightOffset);
		m_light = GetComponent<Light>();
		if (randomStartingRotation)
		{
			m_transform.rotation = Quaternion.Euler(m_transform.rotation.x, UnityEngine.Random.Range(rotationMin, rotationMax), m_transform.rotation.z);
		}
		if (pointDirectlyAtFloor)
		{
			magicNumberAngle = 45f;
			m_transform.rotation = Quaternion.Euler(magicNumberAngle, m_transform.rotation.y, m_transform.rotation.z);
		}
		if (randomStartingCookieAngle)
		{
			m_light.spotAngle = UnityEngine.Random.Range(cookieAngleMin, cookieAngleMax);
		}
		if (randomIntensity)
		{
			m_light.intensity = UnityEngine.Random.Range(intensityMin, intensityMax);
		}
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (rotationSpeed != 0f)
		{
			m_transform.Rotate(0f, 0f, rotationSpeed * realDeltaTime);
		}
		if (swayRotation)
		{
			Quaternion a = Quaternion.Euler(magicNumberAngle, rotationMin, m_transform.rotation.z);
			Quaternion b = Quaternion.Euler(magicNumberAngle, rotationMax, m_transform.rotation.z);
			float t = 0.5f * (1f + Mathf.Sin((float)Math.PI * Time.realtimeSinceStartup * (swaySpeed / 10f)));
			m_transform.rotation = Quaternion.Lerp(a, b, t);
		}
		if (pulseCookieAngle)
		{
			m_light.spotAngle = Mathf.SmoothStep(cookieAngleMin, cookieAngleMax, Mathf.PingPong(Time.time / pulseCookieAngleSpeed4Real, pulseCookieAngleHang));
		}
		if (pulseIntensity)
		{
			m_light.intensity = Mathf.SmoothStep(intensityMin, intensityMax, Mathf.PingPong(Time.time / pulseIntensitySpeed4Real, pulseIntensityHang));
		}
		if (doPingPong)
		{
			RenderSettings.ambientLight = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time * otherNumber, pingPongTime));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
