using UnityEngine;

public class CameraRendererRenderer : MonoBehaviour
{
	private RendererRenderer[] m_renderers;

	private void Start()
	{
		m_renderers = Object.FindObjectsOfType<RendererRenderer>();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		m_renderers[0].transform.position += new Vector3(3f, 0f, 0f);
		for (int i = 0; i < m_renderers.Length; i++)
		{
			m_renderers[i].GetComponent<Renderer>().sharedMaterial.SetPass(0);
		}
		m_renderers[0].transform.position -= new Vector3(3f, 0f, 0f);
		Graphics.Blit(source, target);
	}
}
