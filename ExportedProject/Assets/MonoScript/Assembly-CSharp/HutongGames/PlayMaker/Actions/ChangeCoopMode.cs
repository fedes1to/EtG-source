using System.Collections;
using InControl;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Only use this in the Foyer!")]
	[ActionCategory(".NPCs")]
	public class ChangeCoopMode : FsmStateAction
	{
		public string PlayerPrefabPath;

		public bool TargetCoopMode = true;

		public bool IsTestCoopValid;

		public FsmEvent IfCoopValidEvent;

		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			if (IsTestCoopValid)
			{
				base.Fsm.Event(IfCoopValidEvent);
			}
			else
			{
				base.Fsm.GameObject.GetComponent<TalkDoerLite>().StartCoroutine(HandleCharacterChange());
			}
		}

		private IEnumerator HandleCharacterChange()
		{
			InputDevice lastActiveDevice = GameManager.Instance.LastUsedInputDeviceForConversation;
			if (TargetCoopMode)
			{
				GameManager.Instance.CurrentGameType = GameManager.GameType.COOP_2_PLAYER;
				if ((bool)GameManager.Instance.PrimaryPlayer)
				{
					GameManager.Instance.PrimaryPlayer.ReinitializeMovementRestrictors();
				}
				PlayerController newPlayer = GeneratePlayer();
				yield return null;
				GameUIRoot.Instance.ConvertCoreUIToCoopMode();
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(newPlayer.specRigidbody);
				GameManager.Instance.MainCameraController.ClearPlayerCache();
				Foyer.Instance.ProcessPlayerEnteredFoyer(newPlayer);
			}
			else
			{
				GameManager.Instance.SecondaryPlayer.SetInputOverride("getting deleted");
				Object.Destroy(GameManager.Instance.SecondaryPlayer.gameObject);
				GameManager.Instance.SecondaryPlayer = null;
				GameManager.Instance.CurrentGameType = GameManager.GameType.SINGLE_PLAYER;
				if ((bool)GameManager.Instance.PrimaryPlayer)
				{
					GameManager.Instance.PrimaryPlayer.ReinitializeMovementRestrictors();
				}
			}
			BraveInput.ReassignAllControllers(lastActiveDevice);
			if (Foyer.Instance.OnCoopModeChanged != null)
			{
				Foyer.Instance.OnCoopModeChanged();
			}
			Finish();
		}

		private PlayerController GeneratePlayer()
		{
			if (GameManager.Instance.SecondaryPlayer != null)
			{
				return GameManager.Instance.SecondaryPlayer;
			}
			GameManager.Instance.ClearSecondaryPlayer();
			GameManager.LastUsedCoopPlayerPrefab = (GameObject)BraveResources.Load(PlayerPrefabPath);
			PlayerController playerController = null;
			if (playerController == null)
			{
				GameObject gameObject = Object.Instantiate(GameManager.LastUsedCoopPlayerPrefab, base.Fsm.GameObject.transform.position, Quaternion.identity);
				gameObject.SetActive(true);
				playerController = gameObject.GetComponent<PlayerController>();
			}
			FoyerCharacterSelectFlag component = base.Owner.GetComponent<FoyerCharacterSelectFlag>();
			if ((bool)component && component.IsAlternateCostume)
			{
				playerController.SwapToAlternateCostume();
			}
			GameManager.Instance.SecondaryPlayer = playerController;
			playerController.PlayerIDX = 1;
			return playerController;
		}
	}
}
