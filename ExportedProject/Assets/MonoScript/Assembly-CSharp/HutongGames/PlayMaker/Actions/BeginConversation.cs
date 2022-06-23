using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Starts an NPC conversation (makes the NPC uninteractable).")]
	[ActionCategory(".NPCs")]
	public class BeginConversation : FsmStateAction
	{
		public enum ConversationType
		{
			Normal,
			Passive
		}

		public enum LockedConversation
		{
			Default,
			Locked,
			Unlocked
		}

		public const float LetterBoxAmount = 0.35f;

		public const float LetterBoxLerpTime = 0.25f;

		public static Vector2 NpcScreenBuffer = new Vector2(0.3f, 0.3f);

		[Tooltip("Normal: Full conversation, press 'action' to advance.\nPassive: Just a speec bubble over the NPC's head.")]
		public ConversationType conversationType;

		[Tooltip("Whether or not to take control away from the player during the conversation.\nDefault will lock normal conversations but not passive conversations.")]
		public LockedConversation locked;

		[Tooltip("Whether or not to take control away from the player during the conversation.\nDefault will lock normal conversations but not passive conversations.")]
		public FsmFloat overrideNpcScreenHeight = -1f;

		public bool UsesCustomScreenBuffer;

		public Vector2 CustomScreenBuffer;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (component.TalkingPlayer == null)
			{
				component.TalkingPlayer = GameManager.Instance.PrimaryPlayer;
			}
			for (int i = 0; i < StaticReferenceManager.AllNpcs.Count; i++)
			{
				TalkDoerLite talkDoerLite = StaticReferenceManager.AllNpcs[i];
				if ((bool)talkDoerLite && talkDoerLite != component)
				{
					talkDoerLite.CloseTextBox(true);
				}
			}
			GameUIRoot.Instance.InitializeConversationPortrait(component.TalkingPlayer);
			GameUIRoot.Instance.levelNameUI.BanishLevelNameText();
			if (conversationType == ConversationType.Normal)
			{
				component.State = TalkDoerLite.TalkingState.Conversation;
			}
			else if (conversationType == ConversationType.Passive)
			{
				component.State = TalkDoerLite.TalkingState.Passive;
			}
			bool flag = locked == LockedConversation.Locked;
			if (locked == LockedConversation.Default)
			{
				flag = conversationType == ConversationType.Normal;
			}
			if (flag && !component.HasPlayerLocked)
			{
				component.HasPlayerLocked = true;
				component.TalkingPlayer.SetInputOverride("conversation");
				Pixelator.Instance.LerpToLetterbox(0.35f, 0.25f);
				Pixelator.Instance.DoFinalNonFadedLayer = true;
				GameUIRoot.Instance.ToggleLowerPanels(false, false, "conversation");
				GameUIRoot.Instance.HideCoreUI("conversation");
				if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
				{
					GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ForceHideMetaCurrencyPanel();
				}
				Minimap.Instance.TemporarilyPreventMinimap = true;
				Vector2 vector = component.speakPoint.transform.position.XY();
				Vector2 vector2 = ((!UsesCustomScreenBuffer) ? NpcScreenBuffer : CustomScreenBuffer);
				CameraController mainCameraController = GameManager.Instance.MainCameraController;
				Vector2 vector3 = CameraController.CameraToWorld(vector2.x, vector2.y);
				Vector2 vector4 = CameraController.CameraToWorld(1f - vector2.x, 1f - vector2.y);
				Vector2 vector5 = vector4 - vector3;
				if (overrideNpcScreenHeight.Value >= 0f)
				{
					vector4.y = (vector3.y = CameraController.CameraToWorld(0.5f, overrideNpcScreenHeight.Value).y);
					vector5.y = 0f;
				}
				mainCameraController.SetManualControl(true);
				if (new Rect(vector3.x, vector3.y, vector5.x, vector5.y).Contains(vector))
				{
					mainCameraController.OverridePosition = mainCameraController.transform.position;
				}
				else
				{
					Vector2 vector6 = BraveMathCollege.ClosestPointOnRectangle(vector, vector3, vector4 - vector3);
					mainCameraController.OverridePosition = mainCameraController.transform.position + (Vector3)(vector - vector6);
				}
			}
			Finish();
		}
	}
}
