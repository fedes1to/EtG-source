using System.Collections;
using UnityEngine;

public class DraGunNeckPieceController : BraveBehaviour
{
	public string leftSprite;

	public string forwardSprite;

	public string rightSprite;

	public float flipThreshold;

	[CurveRange(0f, -6f, 6f, 12f)]
	public AnimationCurve xCurve;

	[CurveRange(-5f, -5f, 9f, 10f)]
	public AnimationCurve yCurve;

	public float idleTime;

	public float idleOffset;

	private bool m_initialized;

	private Vector2 m_startingPos;

	private bool m_isIdleUp;

	private float m_idleTimer;

	public IEnumerator Start()
	{
		yield return null;
		m_initialized = true;
		m_startingPos = base.transform.position;
		m_idleTimer = idleOffset;
	}

	public void Update()
	{
		if (m_initialized)
		{
			m_idleTimer -= BraveTime.DeltaTime;
			if (m_idleTimer < 0f)
			{
				m_idleTimer += idleTime;
				m_isIdleUp = !m_isIdleUp;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void UpdateHeadDelta(Vector2 headDelta)
	{
		if (m_initialized)
		{
			Vector2 startingPos = m_startingPos;
			startingPos += new Vector2(Mathf.Sign(headDelta.x) * xCurve.Evaluate(Mathf.Abs(headDelta.x)), 0f);
			startingPos += new Vector2(0f, yCurve.Evaluate(headDelta.y));
			if (m_isIdleUp)
			{
				startingPos += PhysicsEngine.PixelToUnit(new IntVector2(0, 1));
			}
			base.transform.position = new Vector3(BraveMathCollege.QuantizeFloat(startingPos.x, 0.0625f), BraveMathCollege.QuantizeFloat(startingPos.y, 0.0625f));
			if (Mathf.Abs(headDelta.x) > flipThreshold)
			{
				base.sprite.SetSprite((!(Mathf.Sign(headDelta.x) < 0f)) ? rightSprite : leftSprite);
			}
			else
			{
				base.sprite.SetSprite(forwardSprite);
			}
		}
	}
}
