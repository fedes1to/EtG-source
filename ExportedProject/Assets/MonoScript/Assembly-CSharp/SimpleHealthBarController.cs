using UnityEngine;

public class SimpleHealthBarController : MonoBehaviour
{
	public Transform fg;

	public Transform bg;

	private HealthHaver m_healthHaver;

	private float m_baseScale = 1f;

	public void Initialize(SpeculativeRigidbody srb, HealthHaver h)
	{
		m_healthHaver = h;
		m_baseScale = 1f;
		if (srb.UnitWidth > 1f)
		{
			m_baseScale = srb.UnitWidth;
		}
		base.transform.parent = m_healthHaver.transform;
		base.transform.position = srb.UnitBottomCenter.Quantize(0.0625f).ToVector3ZisY() + new Vector3(0f, -0.25f, 0f);
		fg.localScale = fg.localScale.WithX(m_baseScale);
		bg.localScale = bg.localScale.WithX(m_baseScale);
		fg.localPosition = new Vector3(-1f * (m_baseScale * 0.5f), 0f, 0f);
	}

	private void Update()
	{
		if ((bool)m_healthHaver)
		{
			fg.localScale = fg.localScale.WithX(m_healthHaver.GetCurrentHealthPercentage() * m_baseScale);
		}
	}
}
