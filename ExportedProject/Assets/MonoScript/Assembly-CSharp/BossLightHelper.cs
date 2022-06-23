using System.Collections;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(SpotLightHelper))]
[RequireComponent(typeof(Light))]
public class BossLightHelper : TimeInvariantMonoBehaviour
{
	public float MaxRotation = 360f;

	[Header("Intensity Pulse")]
	public float PulseThreshold = 0.2f;

	public float PulseMaxIntensity = 8f;

	public float PulsePeriod = 1f;

	[Header("On Death")]
	public float PulseStopTime = 5f;

	public float RotationStopTime = 10f;

	private HealthHaver m_bossHealth;

	private Light m_light;

	private SpotLightHelper m_lightHelper;

	private float m_startRotation;

	private float m_startIntensity;

	private float m_pulseTimer;

	private bool m_isDead;

	public void Start()
	{
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY());
		m_bossHealth = roomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All)[0].healthHaver;
		m_light = GetComponent<Light>();
		m_lightHelper = GetComponent<SpotLightHelper>();
		m_startRotation = m_lightHelper.rotationSpeed;
		m_startIntensity = m_light.intensity;
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (!m_isDead)
		{
			m_lightHelper.rotationSpeed = Mathf.Lerp(m_startRotation, MaxRotation, 1f - m_bossHealth.GetCurrentHealthPercentage());
		}
		if (m_bossHealth.IsDead && !m_isDead)
		{
			m_isDead = true;
			StartCoroutine(DeathEffects());
		}
		if (m_isDead || m_bossHealth.GetCurrentHealthPercentage() <= PulseThreshold)
		{
			m_pulseTimer += realDeltaTime;
			m_light.intensity = Mathf.Lerp(m_startIntensity, PulseMaxIntensity, Mathf.PingPong(m_pulseTimer, PulsePeriod) / PulsePeriod);
		}
	}

	private IEnumerator DeathEffects()
	{
		float timer = 0f;
		float startMaxIntensity = PulseMaxIntensity;
		yield return null;
		while (true)
		{
			timer += BraveTime.DeltaTime;
			m_lightHelper.rotationSpeed = Mathf.Lerp(MaxRotation, 0f, Mathf.Clamp01(timer / RotationStopTime));
			PulseMaxIntensity = Mathf.Lerp(startMaxIntensity, m_startIntensity, Mathf.Clamp01(timer / PulseStopTime));
			if (timer > RotationStopTime && timer > PulseStopTime)
			{
				break;
			}
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
