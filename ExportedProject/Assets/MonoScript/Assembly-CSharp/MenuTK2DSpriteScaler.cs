using System;
using UnityEngine;

public class MenuTK2DSpriteScaler : MonoBehaviour
{
	[NonSerialized]
	protected float TargetResolution = 1080f;

	protected Transform m_transform;

	protected dfGUIManager m_manager;

	protected int m_cachedWidth;

	protected int m_cachedHeight;

	protected bool m_cachedFullscreen;

	protected float m_cachedParentScale = 1f;

	private void Start()
	{
		m_manager = UnityEngine.Object.FindObjectOfType<dfGUIManager>();
		m_transform = base.transform;
		m_cachedFullscreen = Screen.fullScreen;
	}

	private void LateUpdate()
	{
		float num = 1f;
		if (m_transform.parent != null)
		{
			num = m_transform.parent.lossyScale.x;
		}
		if (m_cachedWidth != Screen.width || m_cachedHeight != Screen.height || m_cachedFullscreen != Screen.fullScreen || m_cachedParentScale != num)
		{
			float num2 = (float)Screen.height * m_manager.RenderCamera.rect.height / TargetResolution * 4f;
			float num3 = num2 * 16f * m_manager.PixelsToUnits();
			m_transform.localScale = new Vector3(num3 / num, num3 / num, 1f);
			m_transform.position = m_transform.position.Quantize(m_manager.PixelsToUnits() * num2);
			m_cachedParentScale = num;
			m_cachedWidth = Screen.width;
			m_cachedHeight = Screen.height;
			m_cachedFullscreen = Screen.fullScreen;
		}
	}
}
