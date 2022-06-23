using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Only use this in the Foyer!")]
	[ActionCategory(".NPCs")]
	public class ChangeToNewCharacter : FsmStateAction
	{
		public string PlayerPrefabPath;

		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			GameManager.Instance.StartCoroutine(HandleCharacterChange());
		}

		private IEnumerator HandleCharacterChange()
		{
			Pixelator.Instance.FadeToBlack(0.5f);
			bool wasInGunGame = false;
			if ((bool)GameManager.Instance.PrimaryPlayer)
			{
				wasInGunGame = GameManager.Instance.PrimaryPlayer.CharacterUsesRandomGuns;
			}
			GameManager.Instance.PrimaryPlayer.SetInputOverride("getting deleted");
			yield return new WaitForSeconds(0.5f);
			PlayerController newPlayer = GeneratePlayer();
			yield return null;
			GameManager.Instance.MainCameraController.ClearPlayerCache();
			GameManager.Instance.MainCameraController.SetManualControl(false);
			Foyer.Instance.ProcessPlayerEnteredFoyer(newPlayer);
			Foyer.Instance.PlayerCharacterChanged(newPlayer);
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(newPlayer.specRigidbody);
			Pixelator.Instance.FadeToBlack(0.5f, true);
			yield return new WaitForSeconds(0.1f);
			if (wasInGunGame)
			{
				PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
				primaryPlayer.CharacterUsesRandomGuns = true;
				int num;
				for (num = 1; num < primaryPlayer.inventory.AllGuns.Count; num++)
				{
					Gun gun = primaryPlayer.inventory.AllGuns[num];
					primaryPlayer.inventory.RemoveGunFromInventory(gun);
					Object.Destroy(gun.gameObject);
					num--;
				}
			}
			if ((bool)GameManager.Instance.SecondaryPlayer)
			{
				GameManager.Instance.SecondaryPlayer.UpdateRandomStartingEquipmentCoop(newPlayer.characterIdentity == PlayableCharacters.Eevee);
			}
			Finish();
		}

		private PlayerController GeneratePlayer()
		{
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			Vector3 position = primaryPlayer.transform.position;
			Object.Destroy(primaryPlayer.gameObject);
			GameManager.Instance.ClearPrimaryPlayer();
			GameManager.PlayerPrefabForNewGame = (GameObject)BraveResources.Load(PlayerPrefabPath);
			PlayerController component = GameManager.PlayerPrefabForNewGame.GetComponent<PlayerController>();
			GameStatsManager.Instance.BeginNewSession(component);
			PlayerController playerController = null;
			if (playerController == null)
			{
				GameObject gameObject = Object.Instantiate(GameManager.PlayerPrefabForNewGame, position, Quaternion.identity);
				GameManager.PlayerPrefabForNewGame = null;
				gameObject.SetActive(true);
				playerController = gameObject.GetComponent<PlayerController>();
			}
			FoyerCharacterSelectFlag component2 = base.Owner.GetComponent<FoyerCharacterSelectFlag>();
			if ((bool)component2 && component2.IsAlternateCostume)
			{
				playerController.SwapToAlternateCostume();
			}
			GameManager.Instance.PrimaryPlayer = playerController;
			playerController.PlayerIDX = 0;
			return playerController;
		}
	}
}
