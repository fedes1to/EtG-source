using UnityEngine;

public class SimpleLightIntensityCurve : MonoBehaviour
{
	public float Duration = 1f;

	public float MinIntensity;

	public float MaxIntensity = 1f;

	[CurveRange(0f, 0f, 1f, 1f)]
	public AnimationCurve Curve;

	protected Light m_light;

	protected float m_elapsed;

	private void Start()
	{
		m_light = GetComponent<Light>();
		m_light.intensity = Curve.Evaluate(0f) * (MaxIntensity - MinIntensity) + MinIntensity;
	}

	private void Update()
	{
		m_elapsed += BraveTime.DeltaTime;
		if (m_elapsed < Duration)
		{
			m_light.intensity = Curve.Evaluate(m_elapsed / Duration) * (MaxIntensity - MinIntensity) + MinIntensity;
			return;
		}
		m_light.intensity = Curve.Evaluate(1f) * (MaxIntensity - MinIntensity) + MinIntensity;
		Object.Destroy(this);
	}
}
