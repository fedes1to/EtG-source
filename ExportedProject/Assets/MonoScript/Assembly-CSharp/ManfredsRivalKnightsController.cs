using System.Collections.Generic;
using Dungeonator;

public class ManfredsRivalKnightsController : BraveBehaviour
{
	public float[] HealthThresholds = new float[2] { 0.8f, 0.6f };

	private List<AIActor> m_knights = new List<AIActor>();

	private int m_activeKnights;

	public void Start()
	{
		List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (!(activeEnemies[i] == base.aiActor))
			{
				activeEnemies[i].behaviorSpeculator.enabled = false;
				m_knights.Add(activeEnemies[i]);
			}
		}
	}

	public void Update()
	{
		for (int i = 0; i < m_knights.Count; i++)
		{
			if (!(m_knights[i] == null))
			{
				if (!m_knights[i] || !m_knights[i].healthHaver || m_knights[i].healthHaver.IsDead)
				{
					m_activeKnights++;
					m_knights[i] = null;
				}
				else if (m_knights[i].healthHaver.GetCurrentHealthPercentage() < 1f)
				{
					ActivateKnight(i);
				}
				else
				{
					m_knights[i].aiAnimator.LockFacingDirection = true;
					m_knights[i].aiAnimator.FacingDirection = -90f;
				}
			}
		}
		for (int j = 0; j < HealthThresholds.Length; j++)
		{
			if (base.healthHaver.GetCurrentHealthPercentage() < HealthThresholds[j] && m_activeKnights <= j)
			{
				ActivateKnight();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void ActivateKnight(int index = -1)
	{
		if (index == -1)
		{
			index = 0;
			while (index < m_knights.Count && m_knights[index] == null)
			{
				index++;
			}
		}
		if (index >= 0 && index < m_knights.Count)
		{
			m_knights[index].behaviorSpeculator.enabled = true;
			m_knights[index].aiAnimator.LockFacingDirection = false;
			m_knights[index].aiActor.State = AIActor.ActorState.Normal;
			m_activeKnights++;
			m_knights[index] = null;
		}
	}
}
