using FullInspector;

[InspectorDropdownName("Bosses/BossStatues/StompBehavior")]
public class BossStatuesStompBehavior : BossStatuesPatternBehavior
{
	public float HangTime = 1f;

	private int m_frameCount;

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive)
			{
				bossStatueController.IsStomping = false;
				bossStatueController.HangTime = 0f;
				bossStatueController.State = BossStatueController.StatueState.StandStill;
			}
		}
	}

	protected override void InitPositions()
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive)
			{
				PlayerController playerClosestToPoint = GameManager.Instance.GetPlayerClosestToPoint(bossStatueController.GroundPosition);
				if ((bool)playerClosestToPoint)
				{
					bossStatueController.Target = playerClosestToPoint.specRigidbody.UnitCenter;
				}
				if (attackType == null)
				{
					bossStatueController.QueuedBulletScript.Add(null);
				}
				bossStatueController.IsStomping = true;
				bossStatueController.HangTime = HangTime;
			}
		}
		m_frameCount = 0;
	}

	protected override void UpdatePositions()
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive)
			{
				PlayerController playerClosestToPoint = GameManager.Instance.GetPlayerClosestToPoint(bossStatueController.GroundPosition);
				if ((bool)playerClosestToPoint)
				{
					bossStatueController.Target = playerClosestToPoint.specRigidbody.UnitCenter;
				}
			}
		}
		m_frameCount++;
	}

	protected override bool IsFinished()
	{
		if (m_frameCount < 3)
		{
			return false;
		}
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			if (!m_activeStatues[i].IsGrounded)
			{
				return false;
			}
		}
		AkSoundEngine.PostEvent("Play_ENM_kali_shockwave_01", m_statuesController.bulletBank.gameObject);
		return true;
	}

	protected override void BeginState(PatternState state)
	{
		base.BeginState(state);
		if (state == PatternState.InProgress)
		{
			SetActiveState(BossStatueController.StatueState.WaitForAttack);
		}
	}
}
