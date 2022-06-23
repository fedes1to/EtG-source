namespace HutongGames.PlayMaker.Actions
{
	public class BraveFsmStateAction : FsmStateAction
	{
		protected void SetReplacementString(string targetString)
		{
			FsmString fsmString = base.Fsm.Variables.GetFsmString("npcReplacementString");
			if (fsmString != null)
			{
				fsmString.Value = targetString;
			}
		}

		protected T GetActionInPreviousNode<T>() where T : FsmStateAction
		{
			for (int i = 0; i < base.Fsm.PreviousActiveState.Actions.Length; i++)
			{
				if (base.Fsm.PreviousActiveState.Actions[i] is T)
				{
					return base.Fsm.PreviousActiveState.Actions[i] as T;
				}
			}
			return (T)null;
		}

		protected T FindActionOfType<T>() where T : FsmStateAction
		{
			for (int i = 0; i < base.Fsm.States.Length; i++)
			{
				for (int j = 0; j < base.Fsm.States[i].Actions.Length; j++)
				{
					if (base.Fsm.States[i].Actions[j] is T)
					{
						return base.Fsm.States[i].Actions[j] as T;
					}
				}
			}
			return (T)null;
		}
	}
}
