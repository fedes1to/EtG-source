using UnityEngine;

public class EmissivePulse : MonoBehaviour
{
	public float minEmissivePower = 10f;

	public float maxEmissivePower = 20f;

	public float period = 2f;

	private Material m_material;

	private int m_id = -1;

	private void Start()
	{
		m_id = Shader.PropertyToID("_EmissivePower");
		m_material = GetComponent<Renderer>().material;
	}

	private void Update()
	{
		m_material.SetFloat(m_id, Mathf.Lerp(minEmissivePower, maxEmissivePower, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.timeSinceLevelLoad / period, 1f))));
	}
}
