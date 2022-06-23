public class TimeInvariantMonoBehaviour : BraveBehaviour
{
	protected float m_deltaTime;

	protected virtual void Update()
	{
		m_deltaTime = GameManager.INVARIANT_DELTA_TIME;
		InvariantUpdate(GameManager.INVARIANT_DELTA_TIME);
	}

	protected virtual void InvariantUpdate(float realDeltaTime)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
