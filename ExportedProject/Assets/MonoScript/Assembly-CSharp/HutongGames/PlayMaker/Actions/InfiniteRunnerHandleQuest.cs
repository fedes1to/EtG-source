using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class InfiniteRunnerHandleQuest : FsmStateAction
	{
		private TalkDoerLite m_talkDoer;

		private Vector2 m_lastPosition;

		private float m_elapsed;

		public override void Awake()
		{
			base.Awake();
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
		}

		public override void OnEnter()
		{
			m_lastPosition = m_talkDoer.specRigidbody.UnitCenter;
			base.Owner.GetComponent<InfiniteRunnerController>().StartQuest();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_elapsed += BraveTime.DeltaTime;
			if (!(m_elapsed < 0.75f))
			{
				if (m_talkDoer.CurrentPath != null)
				{
					m_talkDoer.specRigidbody.Velocity = m_talkDoer.GetPathVelocityContribution(m_lastPosition, 32);
					m_lastPosition = m_talkDoer.specRigidbody.UnitCenter;
				}
				else
				{
					Finish();
				}
			}
		}
	}
}
