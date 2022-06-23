using UnityEngine;

public class MinimapRenderer : MonoBehaviour
{
	public Transform QuadTransform;

	public Texture MapMaskFullscreen;

	public Texture MapMaskSmallscreen;

	private Material m_quadMaterial;

	private Camera m_camera;

	private Camera m_uiCamera;

	private int m_cmQuad;

	private int m_idMainTex;

	private int m_idMaskTex;

	private RenderTexture m_currentQuadRenderTexture;

	private void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_quadMaterial = QuadTransform.GetComponent<MeshRenderer>().material;
		QuadTransform.parent = QuadTransform.parent.parent;
		QuadTransform.gameObject.SetLayerRecursively(LayerMask.NameToLayer("GUI"));
		m_idMainTex = Shader.PropertyToID("_MainTex");
		m_idMaskTex = Shader.PropertyToID("_MaskTex");
	}

	private void Start()
	{
		m_uiCamera = GameUIRoot.Instance.Manager.RenderCamera;
	}

	private void CheckSize()
	{
		Rect rect = new Rect(1f - Minimap.Instance.currentXRectFactor, 1f - Minimap.Instance.currentYRectFactor, Minimap.Instance.currentXRectFactor, Minimap.Instance.currentYRectFactor);
		if (!Minimap.Instance.IsFullscreen && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !GameManager.Instance.IsLoadingLevel && (!GameManager.Instance.SecondaryPlayer.IsGhost || true))
		{
			rect.y -= 0.0875f;
		}
		QuadTransform.localScale = new Vector3(m_uiCamera.orthographicSize * 2f * 1.77777779f * rect.width, m_uiCamera.orthographicSize * 2f * rect.height, 1f);
		Vector3 vector = new Vector3(m_uiCamera.orthographicSize * m_uiCamera.aspect * -1f, m_uiCamera.orthographicSize * -1f, 0f);
		vector.x += rect.xMin * m_uiCamera.orthographicSize * 2f * m_uiCamera.aspect;
		vector.y += rect.yMin * m_uiCamera.orthographicSize * 2f;
		vector.x += QuadTransform.localScale.x * (m_uiCamera.aspect / 1.77777779f) / 2f;
		vector.y += QuadTransform.localScale.y / 2f;
		QuadTransform.position = (m_uiCamera.transform.position + vector).WithZ(3f);
		if (Minimap.Instance.IsFullscreen)
		{
			m_quadMaterial.SetTexture(m_idMaskTex, MapMaskFullscreen);
		}
		else
		{
			m_quadMaterial.SetTexture(m_idMaskTex, MapMaskSmallscreen);
		}
		int num = ((GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.HIGH) ? 960 : 1920);
		int num2 = ((GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.HIGH) ? 540 : 1080);
		if (m_currentQuadRenderTexture != null && (m_currentQuadRenderTexture.width != num || m_currentQuadRenderTexture.height != num2))
		{
			RenderTexture.ReleaseTemporary(m_currentQuadRenderTexture);
			m_currentQuadRenderTexture = null;
		}
		if (m_currentQuadRenderTexture == null)
		{
			m_currentQuadRenderTexture = RenderTexture.GetTemporary(num, num2);
			m_currentQuadRenderTexture.filterMode = FilterMode.Point;
			m_quadMaterial.SetTexture(m_idMainTex, m_currentQuadRenderTexture);
		}
		if (m_camera.targetTexture != m_currentQuadRenderTexture)
		{
			m_camera.targetTexture = m_currentQuadRenderTexture;
		}
	}

	private void LateUpdate()
	{
		CheckSize();
	}

	private void OnDestroy()
	{
		if (m_currentQuadRenderTexture != null)
		{
			RenderTexture.ReleaseTemporary(m_currentQuadRenderTexture);
		}
	}
}
