using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Sets various unusual player flags.")]
	public class SetExoticPlayerFlag : FsmStateAction
	{
		public FsmBool SetGunGameTrue;

		public FsmBool SetChallengeModTrue;

		public FsmBool SetMegaChallengeModeTrue;

		public FsmBool ToggleTurboMode;

		public FsmBool ToggleRainbowRun;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if ((bool)component && (bool)component.TalkingPlayer)
			{
				if (SetGunGameTrue.Value)
				{
					SetGunGame(true);
				}
				if (SetChallengeModTrue.Value)
				{
					ChallengeManager.ChallengeModeType = ChallengeModeType.ChallengeMode;
					component.TalkingPlayer.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_DaisukeFavor"), new Vector3(0f, -0.625f, 0f));
				}
				else if (SetMegaChallengeModeTrue.Value)
				{
					ChallengeManager.ChallengeModeType = ChallengeModeType.ChallengeMegaMode;
					component.TalkingPlayer.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_DaisukeFavor"), new Vector3(0f, -0.625f, 0f));
				}
				else if (ToggleRainbowRun.Value)
				{
					if (!GameStatsManager.Instance.rainbowRunToggled)
					{
						GameStatsManager.Instance.rainbowRunToggled = true;
						AkSoundEngine.PostEvent("Play_NPC_Blessing_Rainbow_Get_01", base.Owner.gameObject);
						GameUIRoot.Instance.notificationController.DoCustomNotification(GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#RAINBOW_POPUP_ACTIVE"), string.Empty, null, -1, UINotificationController.NotificationColor.SILVER, false, true);
						component.TalkingPlayer.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_BowlerFavor"), new Vector3(0f, -0.625f, 0f));
					}
					else
					{
						GameStatsManager.Instance.rainbowRunToggled = false;
						AkSoundEngine.PostEvent("Play_NPC_Blessing_Rainbow_Remove_01", base.Owner.gameObject);
						GameUIRoot.Instance.notificationController.DoCustomNotification(GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#RAINBOW_POPUP_INACTIVE"), string.Empty, null, -1, UINotificationController.NotificationColor.SILVER, false, true);
					}
					GameOptions.Save();
				}
				else if (ToggleTurboMode.Value)
				{
					if (!GameStatsManager.Instance.isTurboMode)
					{
						GameStatsManager.Instance.isTurboMode = true;
						AkSoundEngine.PostEvent("Play_NPC_Blessing_Speed_Tonic_01", base.Owner.gameObject);
						if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
						{
							GameUIRoot.Instance.notificationController.DoCustomNotification("Game Speed: Turbo", string.Empty, null, -1, UINotificationController.NotificationColor.SILVER, false, true);
						}
						else
						{
							GameUIRoot.Instance.notificationController.DoCustomNotification(GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#OPTIONS_GAMESPEED"), GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#OPTIONS_GAMESPEED_TURBO"), null, -1);
						}
					}
					else
					{
						GameStatsManager.Instance.isTurboMode = false;
						AkSoundEngine.PostEvent("Play_NPC_Blessing_Slow_Tonic_01", base.Owner.gameObject);
						if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
						{
							GameUIRoot.Instance.notificationController.DoCustomNotification("Game Speed: Classic", string.Empty, null, -1, UINotificationController.NotificationColor.SILVER, false, true);
						}
						else
						{
							GameUIRoot.Instance.notificationController.DoCustomNotification(GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#OPTIONS_GAMESPEED"), GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#OPTIONS_GAMESPEED_NORMAL"), null, -1);
						}
					}
					GameOptions.Save();
				}
			}
			Finish();
		}

		public static void SetGunGame(bool doEffects)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				playerController.CharacterUsesRandomGuns = true;
				int num;
				for (num = 1; num < playerController.inventory.AllGuns.Count; num++)
				{
					Gun gun = playerController.inventory.AllGuns[num];
					playerController.inventory.RemoveGunFromInventory(gun);
					Object.Destroy(gun.gameObject);
					num--;
				}
				if (doEffects)
				{
					playerController.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_MagicFavor_Light"), new Vector3(0f, -0.625f, 0f), true, true);
				}
			}
		}
	}
}
