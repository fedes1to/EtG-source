using System.Collections;
using UnityEngine;

public class NebulaRegisterer : MonoBehaviour
{
	private Renderer m_renderer;

	private IEnumerator Start()
	{
		m_renderer = GetComponent<Renderer>();
		yield return new WaitForSeconds(0.25f);
		if ((bool)m_renderer)
		{
			EndTimesNebulaController endTimesNebulaController = Object.FindObjectOfType<EndTimesNebulaController>();
			if ((bool)endTimesNebulaController)
			{
				endTimesNebulaController.NebulaRegisteredVisuals.Add(m_renderer);
			}
		}
	}

	private void Update()
	{
		if ((bool)m_renderer)
		{
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW)
			{
				m_renderer.enabled = false;
			}
			else if (!m_renderer.enabled)
			{
				m_renderer.enabled = true;
			}
		}
	}
}
