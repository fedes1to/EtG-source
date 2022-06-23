using UnityEngine;

public class ComplexLightColorAnimator : MonoBehaviour
{
	public Gradient colorGradient;

	public float period = 3f;

	public float timeOffset;

	private Light m_light;

	private void Start()
	{
		m_light = GetComponent<Light>();
	}

	private void Update()
	{
		float time = (Time.realtimeSinceStartup + timeOffset) % period / period;
		m_light.color = colorGradient.Evaluate(time);
	}
}
