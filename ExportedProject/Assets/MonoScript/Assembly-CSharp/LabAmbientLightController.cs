using System.Collections;
using UnityEngine;

public class LabAmbientLightController : MonoBehaviour
{
	public Gradient colorGradient;

	public float period = 5f;

	public Transform[] HallwayLights;

	public float HallwayXTranslation = 10f;

	public float HallwayPeriod = 3f;

	private ShadowSystem[] HallwayLightManagers;

	private float[] m_lightIntensities;

	private Vector3[] m_lightStarts;

	private int m_colorID;

	private IEnumerator Start()
	{
		yield return null;
		m_lightStarts = new Vector3[HallwayLights.Length];
		HallwayLightManagers = new ShadowSystem[HallwayLights.Length];
		m_lightIntensities = new float[HallwayLights.Length];
		m_colorID = Shader.PropertyToID("_TintColor");
		for (int i = 0; i < HallwayLights.Length; i++)
		{
			HallwayLightManagers[i] = HallwayLights[i].GetComponentInChildren<ShadowSystem>();
			m_lightStarts[i] = HallwayLights[i].position;
			m_lightIntensities[i] = HallwayLightManagers[i].uLightIntensity;
		}
	}

	private void Update()
	{
		if (m_lightStarts != null)
		{
			GameManager.Instance.Dungeon.OverrideAmbientLight = true;
			GameManager.Instance.Dungeon.OverrideAmbientColor = colorGradient.Evaluate(Time.timeSinceLevelLoad % period / period);
			float num = Time.timeSinceLevelLoad % HallwayPeriod / HallwayPeriod;
			float num2 = Mathf.PingPong(num, 0.5f) * 2f;
			for (int i = 0; i < HallwayLights.Length; i++)
			{
				HallwayLightManagers[i].uLightIntensity = m_lightIntensities[i] * num2;
				Material sharedMaterial = HallwayLightManagers[i].renderer.sharedMaterial;
				sharedMaterial.SetColor(m_colorID, sharedMaterial.GetColor(m_colorID).WithAlpha(num2));
				HallwayLights[i].position = m_lightStarts[i] + new Vector3(HallwayXTranslation * num, 0f, 0f);
			}
			PlatformInterface.SetAlienFXAmbientColor(new Color(1f, 0f, 0f, num2));
		}
	}
}
