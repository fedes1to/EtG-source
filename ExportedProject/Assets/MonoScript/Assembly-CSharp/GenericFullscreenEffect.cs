using UnityEngine;

public class GenericFullscreenEffect : MonoBehaviour
{
	public Shader shader;

	public bool dualPass;

	public Material materialInstance;

	private bool m_cacheCurrentFrameToBuffer;

	[SerializeField]
	protected Material m_material;

	private RenderTexture m_cachedFrame;

	public bool CacheCurrentFrameToBuffer
	{
		get
		{
			return m_cacheCurrentFrameToBuffer;
		}
		set
		{
			m_cacheCurrentFrameToBuffer = value;
		}
	}

	public Material ActiveMaterial
	{
		get
		{
			return m_material;
		}
	}

	private void Awake()
	{
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

	public RenderTexture GetCachedFrame()
	{
		return m_cachedFrame;
	}

	public void ClearCachedFrame()
	{
		if (m_cachedFrame != null)
		{
			RenderTexture.ReleaseTemporary(m_cachedFrame);
		}
		m_cachedFrame = null;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		if (!dualPass)
		{
			Graphics.Blit(source, target, m_material);
		}
		else
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
			Graphics.Blit(source, temporary, m_material, 0);
			Graphics.Blit(temporary, target, m_material, 1);
			RenderTexture.ReleaseTemporary(temporary);
		}
		if (CacheCurrentFrameToBuffer)
		{
			ClearCachedFrame();
			m_cachedFrame = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			m_cachedFrame.filterMode = FilterMode.Point;
			Graphics.Blit(source, m_cachedFrame, m_material);
			CacheCurrentFrameToBuffer = false;
		}
	}
}
