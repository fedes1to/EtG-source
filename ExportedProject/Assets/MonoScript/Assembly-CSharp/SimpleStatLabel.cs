using UnityEngine;

public class SimpleStatLabel : MonoBehaviour
{
	public TrackedStats stat;

	protected dfLabel m_label;

	private void Start()
	{
		m_label = GetComponent<dfLabel>();
	}

	private void Update()
	{
		if ((bool)m_label && m_label.IsVisible)
		{
			int input = Mathf.FloorToInt(GameStatsManager.Instance.GetPlayerStatValue(stat));
			m_label.Text = IntToStringSansGarbage.GetStringForInt(input);
		}
	}
}
