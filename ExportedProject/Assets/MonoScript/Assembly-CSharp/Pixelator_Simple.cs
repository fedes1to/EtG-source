using UnityEngine;

public class Pixelator_Simple : MonoBehaviour
{
	public Shader renderShader;

	public Shader upsideDownShader;

	public Camera slaveCamera;

	private RenderTexture m_renderTarget;

	private Camera m_camera;

	private Material m_renderMaterial;

	private Material m_upsideDownMaterial;

	private int m_cachedCullingMask;

	private bool m_initialized;

	public Material RenderMaterial
	{
		get
		{
			return m_renderMaterial;
		}
		set
		{
			m_renderMaterial = value;
		}
	}

	private void Start()
	{
		slaveCamera.GetComponent<dfGUICamera>().transform.parent.GetComponent<dfGUIManager>().OverrideCamera = true;
		Initialize();
	}

	public void Initialize()
	{
		if (!m_initialized)
		{
			m_initialized = true;
			if (renderShader != null)
			{
				m_renderMaterial = new Material(renderShader);
			}
			if (upsideDownShader != null)
			{
				m_upsideDownMaterial = new Material(upsideDownShader);
			}
			m_cachedCullingMask = slaveCamera.cullingMask;
		}
	}

	private void RebuildRenderTarget(RenderTexture source)
	{
		if (Pixelator.Instance == null)
		{
			m_renderTarget = null;
			return;
		}
		int num = Pixelator.Instance.CurrentMacroResolutionX;
		int num2 = Pixelator.Instance.CurrentMacroResolutionY;
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN)
		{
			num = source.width;
			num2 = source.height;
		}
		if (m_renderTarget == null || m_renderTarget.width != num || m_renderTarget.height != num2)
		{
			m_renderTarget = new RenderTexture(num, num2, 1);
			m_renderTarget.filterMode = FilterMode.Point;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		if (m_camera == null)
		{
			m_camera = GetComponent<Camera>();
		}
		RebuildRenderTarget(source);
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, source.depth);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN)
		{
			temporary.filterMode = FilterMode.Point;
		}
		Graphics.Blit(Pixelator.SmallBlackTexture, temporary);
		if (m_renderTarget == null)
		{
			if (m_renderMaterial != null)
			{
				Graphics.Blit(source, target, m_renderMaterial);
			}
			else
			{
				Graphics.Blit(source, target);
			}
		}
		else
		{
			slaveCamera.CopyFrom(m_camera);
			slaveCamera.transform.position = slaveCamera.transform.position + CameraController.PLATFORM_CAMERA_OFFSET;
			slaveCamera.cullingMask = m_cachedCullingMask;
			slaveCamera.rect = new Rect(0f, 0f, 1f, 1f);
			slaveCamera.clearFlags = CameraClearFlags.Color;
			slaveCamera.backgroundColor = Color.clear;
			slaveCamera.targetTexture = temporary;
			slaveCamera.Render();
			slaveCamera.transform.position = slaveCamera.transform.position - CameraController.PLATFORM_CAMERA_OFFSET;
			Graphics.Blit(temporary, m_renderTarget);
			if (m_renderMaterial != null)
			{
				Graphics.Blit(source, temporary);
				Graphics.Blit(m_renderTarget, temporary, m_upsideDownMaterial);
				Graphics.Blit(temporary, target, m_renderMaterial);
			}
			else
			{
				Debug.LogError("Failing...");
				Graphics.Blit(source, target);
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
	}
}
