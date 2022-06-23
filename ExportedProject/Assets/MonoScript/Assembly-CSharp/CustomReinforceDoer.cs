using System.Collections;

public abstract class CustomReinforceDoer : BraveBehaviour
{
	public virtual bool IsFinished
	{
		get
		{
			return true;
		}
	}

	public virtual void StartIntro()
	{
	}

	public virtual void OnCleanup()
	{
	}

	public IEnumerator TimeInvariantWait(float duration)
	{
		for (float elapsed = 0f; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
