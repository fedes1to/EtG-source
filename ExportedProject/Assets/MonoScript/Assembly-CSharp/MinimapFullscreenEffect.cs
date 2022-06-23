using UnityEngine;

public class MinimapFullscreenEffect : MonoBehaviour
{
	public Shader shader;

	public Material materialInstance;

	public Camera slaveCamera;

	protected Camera m_camera;

	protected Material m_material;

	protected int m_cachedCullingMask;

	private int m_bgTexID = -1;

	private int m_bgTexUVID = -1;

	private int m_cameraRectID = -1;

	private void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_cachedCullingMask = m_camera.cullingMask;
		m_camera.cullingMask = 0;
		if (materialInstance != null)
		{
			m_material = materialInstance;
		}
		else
		{
			m_material = new Material(shader);
		}
	}

	public void SetMaterial(Material m)
	{
		m_material = m;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		if (!GameManager.Instance.IsFoyer)
		{
			slaveCamera.CopyFrom(m_camera);
			slaveCamera.clearFlags = CameraClearFlags.Color;
			Rect rect = new Rect(1f - Minimap.Instance.currentXRectFactor, 1f - Minimap.Instance.currentYRectFactor, Minimap.Instance.currentXRectFactor, Minimap.Instance.currentYRectFactor);
			if (!Minimap.Instance.IsFullscreen && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !GameManager.Instance.IsLoadingLevel && (!GameManager.Instance.SecondaryPlayer.IsGhost || true))
			{
				rect.y -= 0.0875f;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
			Graphics.Blit(Pixelator.SmallBlackTexture, temporary);
			slaveCamera.cullingMask = m_cachedCullingMask;
			slaveCamera.targetTexture = temporary;
			slaveCamera.Render();
			Rect rect2 = BraveCameraUtility.GetRect();
			Vector4 value = new Vector4(rect.xMin + rect2.xMin, rect.yMin + rect2.yMin, rect.width * rect2.width, rect.height * rect2.height);
			Vector4 value2 = new Vector4(rect.xMin, rect.yMin, rect.width, rect.height);
			if (m_bgTexID == -1)
			{
				m_bgTexID = Shader.PropertyToID("_BGTex");
				m_bgTexUVID = Shader.PropertyToID("_BGTexUV");
				m_cameraRectID = Shader.PropertyToID("_CameraRect");
			}
			m_material.SetTexture(m_bgTexID, temporary);
			m_material.SetVector(m_bgTexUVID, value2);
			m_material.SetVector(m_cameraRectID, value);
			Graphics.Blit(source, target, m_material);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
