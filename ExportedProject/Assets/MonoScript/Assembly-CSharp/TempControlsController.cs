using InControl;
using UnityEngine;

public class TempControlsController : MonoBehaviour
{
	private float m_elapsed;

	private float m_lastTime;

	private const float CLOSE_THRESHOLD = 0f;

	public bool CanClose
	{
		get
		{
			return m_elapsed > 0f;
		}
	}

	private void Awake()
	{
		m_elapsed = 0f;
		m_lastTime = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		m_elapsed += Time.realtimeSinceStartup - m_lastTime;
		m_lastTime = Time.realtimeSinceStartup;
		Debug.Log(m_elapsed);
		if (m_elapsed > 0f && (BraveInput.PrimaryPlayerInstance.ActiveActions.Device.AnyButton.WasPressed || BraveInput.PrimaryPlayerInstance.ActiveActions.Device.GetControl(InputControlType.Start).WasPressed || BraveInput.PrimaryPlayerInstance.ActiveActions.Device.GetControl(InputControlType.Select).WasPressed))
		{
			GameManager.Instance.Unpause();
			Object.Destroy(base.gameObject);
		}
	}
}
