using Dungeonator;

namespace HutongGames.PlayMaker.Actions
{
	public class CheckRoomVisited : FsmStateAction
	{
		[Tooltip("Event sent if there are.")]
		public FsmEvent HasVisited;

		[Tooltip("Event sent if there aren't.")]
		public FsmEvent HasNotVisited;

		private RoomHandler m_targetRoom;

		public RoomHandler targetRoom
		{
			get
			{
				return m_targetRoom;
			}
			set
			{
				m_targetRoom = value;
			}
		}

		public override void Awake()
		{
			base.Awake();
		}

		public override void OnEnter()
		{
			if (targetRoom != null)
			{
				if (targetRoom.visibility == RoomHandler.VisibilityStatus.OBSCURED)
				{
					base.Fsm.Event(HasNotVisited);
				}
				else
				{
					base.Fsm.Event(HasVisited);
				}
			}
			Finish();
		}
	}
}
