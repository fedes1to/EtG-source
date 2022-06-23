using UnityEngine;

public class DFScaleFixer : MonoBehaviour
{
	private dfGUIManager m_manager;

	private void Start()
	{
		m_manager = GetComponent<dfGUIManager>();
	}

	private void Update()
	{
		m_manager.UIScaleLegacyMode = false;
		m_manager.UIScale = (float)m_manager.RenderCamera.pixelHeight / (float)m_manager.FixedHeight;
	}
}
