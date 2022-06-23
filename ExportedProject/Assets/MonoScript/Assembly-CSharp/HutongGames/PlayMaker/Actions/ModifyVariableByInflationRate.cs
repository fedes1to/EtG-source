using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class ModifyVariableByInflationRate : FsmStateAction
	{
		public FsmInt TargetVariable;

		public FsmFloat AdditionalMultiplier = 1f;

		public override void Reset()
		{
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
			float num = ((lastLoadedLevelDefinition == null) ? 1f : lastLoadedLevelDefinition.priceMultiplier);
			TargetVariable.Value = Mathf.RoundToInt((float)TargetVariable.Value * num * AdditionalMultiplier.Value);
			if ((bool)base.Owner)
			{
				RoomHandler absoluteRoom = base.Owner.transform.position.GetAbsoluteRoom();
				if (absoluteRoom != null && absoluteRoom.connectedRooms != null && absoluteRoom.connectedRooms.Count == 1 && absoluteRoom.connectedRooms[0].area.PrototypeRoomName.Contains("Black Market"))
				{
					TargetVariable.Value = Mathf.RoundToInt((float)TargetVariable.Value * 0.5f);
				}
			}
			Finish();
		}
	}
}
