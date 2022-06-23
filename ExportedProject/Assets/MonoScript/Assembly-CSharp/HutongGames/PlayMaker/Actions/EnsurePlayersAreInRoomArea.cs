using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Force teleport players to a certain area in the room if they're not already there.")]
	[ActionCategory(".NPCs")]
	public class EnsurePlayersAreInRoomArea : FsmStateAction
	{
		public Vector2 lowerLeftRoomTile;

		public Vector2 upperRightRoomTile;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			Vector2 min = component.ParentRoom.area.UnitBottomLeft + lowerLeftRoomTile;
			Vector2 max = component.ParentRoom.area.UnitBottomLeft + upperRightRoomTile;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if (!BraveMathCollege.AABBContains(min, max, playerController.specRigidbody.GetUnitCenter(ColliderType.HitBox)))
				{
					Vector2 targetPoint = new Vector2((min.x + max.x) / 2f - 0.5f, min.y + 1f);
					if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
					{
						targetPoint.x += 1.5f * (float)((i != 0) ? 1 : (-1));
					}
					playerController.WarpToPoint(targetPoint, true);
				}
			}
			Finish();
		}
	}
}
