using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	public class ToggleAllSimpleTurrets : FsmStateAction
	{
		public FsmBool toggle;

		public override void Reset()
		{
			toggle = false;
		}

		public override void OnEnter()
		{
			List<SimpleTurretController> componentsAbsoluteInRoom = base.Owner.GetComponent<TalkDoerLite>().ParentRoom.GetComponentsAbsoluteInRoom<SimpleTurretController>();
			for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
			{
				if (toggle.Value)
				{
					componentsAbsoluteInRoom[i].ActivateManual();
				}
				else
				{
					componentsAbsoluteInRoom[i].DeactivateManual();
				}
			}
			Finish();
		}
	}
}
