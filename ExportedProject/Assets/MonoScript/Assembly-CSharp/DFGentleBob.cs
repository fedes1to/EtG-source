using UnityEngine;

public class DFGentleBob : MonoBehaviour
{
	public int upPixels = 6;

	public int downPixels = 6;

	public float bounceSpeed = 1f;

	public bool Quantized;

	private dfControl m_control;

	private SpeculativeRigidbody m_rigidbody;

	private Transform m_transform;

	public bool BobDuringBossIntros;

	private Vector3 m_startAbsolutePosition;

	private Vector3 m_startRelativePosition;

	private float t;

	public Vector3 AbsoluteStartPosition
	{
		get
		{
			return m_startAbsolutePosition;
		}
		set
		{
			m_startAbsolutePosition = value;
		}
	}

	private void Start()
	{
		m_transform = base.transform;
		m_control = GetComponent<dfControl>();
		m_rigidbody = GetComponent<SpeculativeRigidbody>();
		m_startAbsolutePosition = m_transform.position;
		if (m_control != null)
		{
			m_startRelativePosition = m_control.RelativePosition;
		}
		t = Random.value;
	}

	private void Update()
	{
		if (t == 0f)
		{
			t = Random.value;
		}
		float num = ((!BobDuringBossIntros || !GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
		t += num * bounceSpeed;
		if (m_control != null)
		{
			m_control.RelativePosition = m_startRelativePosition + new Vector3(0f, Mathf.Lerp(upPixels, downPixels, Mathf.SmoothStep(0f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t, 1f)))), 0f);
			return;
		}
		if (m_rigidbody != null)
		{
			Vector3 vector = m_startAbsolutePosition + new Vector3(0f, 0.0625f * Mathf.Lerp(upPixels, -downPixels, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t, 1f))), 0f).Quantize(0.0625f);
			Vector2 vector2 = vector.XY() - base.transform.position.XY();
			m_rigidbody.Velocity = vector2 / num;
			return;
		}
		m_transform.position = m_startAbsolutePosition + new Vector3(0f, 0.0625f * Mathf.Lerp(upPixels, -downPixels, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t, 1f))), 0f);
		if (Quantized)
		{
			m_transform.position = m_transform.position.Quantize(0.0625f);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_rigidbody)
		{
			m_rigidbody.Velocity = Vector2.zero;
		}
	}
}
