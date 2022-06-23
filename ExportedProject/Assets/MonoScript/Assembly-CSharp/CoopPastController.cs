using System.Collections;
using UnityEngine;

public class CoopPastController : MonoBehaviour
{
	private Vector2 startingP1Position;

	private Vector2 startingP2Position;

	private void Start()
	{
		StartCoroutine(HandleIntro());
	}

	private void Update()
	{
		GameManager.Instance.IsFoyer = false;
		if (GameManager.PVP_ENABLED)
		{
			if (GameManager.Instance.PrimaryPlayer.healthHaver.IsDead)
			{
				HandlePlayerTwoVictory();
				GameManager.PVP_ENABLED = false;
			}
			else if (GameManager.Instance.SecondaryPlayer.healthHaver.IsDead)
			{
				HandlePlayerOneVictory();
				GameManager.PVP_ENABLED = false;
			}
		}
	}

	private void HandlePlayerOneVictory()
	{
		StartCoroutine(HandleOutro(false));
	}

	private void HandlePlayerTwoVictory()
	{
		StartCoroutine(HandleOutro(true));
	}

	private IEnumerator HandleOutro(bool coopPlayerWon)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("past");
		}
		GameManager.Instance.MainCameraController.OverridePosition = GameManager.Instance.MainCameraController.transform.position.XY();
		GameManager.Instance.MainCameraController.SetManualControl(true);
		GameManager.Instance.DungeonMusicController.EndBossMusic();
		if (!GameManager.Instance.MainCameraController.PointIsVisible(GameManager.Instance.PrimaryPlayer.CenterPosition, -0.2f) || !GameManager.Instance.MainCameraController.PointIsVisible(GameManager.Instance.SecondaryPlayer.CenterPosition, -0.2f))
		{
			while (GameManager.Instance.PrimaryPlayer.IsFalling)
			{
				yield return null;
			}
			while (GameManager.Instance.SecondaryPlayer.IsFalling)
			{
				yield return null;
			}
			yield return new WaitForSeconds(3f);
			Pixelator.Instance.FadeToBlack(1f);
			yield return new WaitForSeconds(1f);
			GameManager.Instance.PrimaryPlayer.WarpToPoint(startingP1Position + new Vector2(0f, 12f));
			GameManager.Instance.SecondaryPlayer.WarpToPoint(startingP2Position + new Vector2(0f, 12f));
			yield return null;
			GameManager.Instance.MainCameraController.SetManualControl(false, false);
			PastCameraUtility.LockConversation(new Vector2((startingP1Position.x + startingP2Position.x) / 2f, startingP1Position.y + 12f));
			GameManager.Instance.PrimaryPlayer.ForceIdleFacePoint(Vector2.right);
			GameManager.Instance.SecondaryPlayer.ForceIdleFacePoint(Vector2.left);
			Pixelator.Instance.FadeToBlack(1f, true);
			yield return new WaitForSeconds(1f);
		}
		else
		{
			PastCameraUtility.LockConversation(GameManager.Instance.MainCameraController.transform.position.XY());
		}
		if (coopPlayerWon)
		{
			yield return new WaitForSeconds(1f);
			yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_WIN_01", -1f, -1, 1));
			yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_WIN_02", -1f, 0, 1));
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_WIN_02", -1f, 1, 1));
			yield return new WaitForSeconds(0.5f);
		}
		else
		{
			yield return new WaitForSeconds(1f);
			yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_PLAYER_WIN_02", -1f, -1, 1));
			yield return new WaitForSeconds(1f);
			yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.PrimaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_PLAYER_WIN_01", -1f, -1, 0));
			yield return new WaitForSeconds(0.5f);
		}
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.CoopCultist, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		Pixelator.Instance.FreezeFrame();
		float ela = 0f;
		while (ela < ConvictPastController.FREEZE_FRAME_DURATION)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		BraveTime.ClearMultiplier(base.gameObject);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		ttcc.ClearDebris();
		GameObject borderObject = GameObject.Find("Foyer_Floor_Borders");
		if ((bool)borderObject)
		{
			tk2dBaseSprite component = borderObject.GetComponent<tk2dBaseSprite>();
			component.HeightOffGround -= 5f;
			component.UpdateZDepth();
		}
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits((!coopPlayerWon) ? GameManager.Instance.PrimaryPlayer.sprite.WorldCenter : GameManager.Instance.SecondaryPlayer.sprite.WorldCenter, false, null, (!coopPlayerWon) ? GameManager.Instance.PrimaryPlayer.PlayerIDX : GameManager.Instance.SecondaryPlayer.PlayerIDX));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private IEnumerator HandleIntro()
	{
		yield return null;
		Pixelator.Instance.TriggerPastFadeIn();
		GameStatsManager.Instance.SetFlag(GungeonFlags.COOP_PAST_REACHED, true);
		GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_SUNLIGHT_SPEAR_UNLOCK, true);
		yield return null;
		startingP1Position = GameManager.Instance.PrimaryPlayer.transform.position.XY();
		startingP2Position = GameManager.Instance.SecondaryPlayer.transform.position.XY();
		GameManager.Instance.PrimaryPlayer.ForceIdleFacePoint(Vector2.right);
		GameManager.Instance.SecondaryPlayer.ForceIdleFacePoint(Vector2.left);
		Vector2 convoCenter2 = (GameManager.Instance.SecondaryPlayer.CenterPosition + GameManager.Instance.PrimaryPlayer.CenterPosition) / 2f;
		PastCameraUtility.LockConversation(convoCenter2);
		float ela = 0f;
		while (ela < 1f)
		{
			ela += BraveTime.DeltaTime;
			convoCenter2 = (GameManager.Instance.SecondaryPlayer.CenterPosition + GameManager.Instance.PrimaryPlayer.CenterPosition) / 2f;
			GameManager.Instance.MainCameraController.OverridePosition = convoCenter2;
			yield return null;
		}
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_01", -1f, -1, 1));
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_02", -1f, -1, 1));
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.PrimaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_PLAYER_01", -1f, -1, 0));
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_03", -1f, -1, 1));
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.PrimaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_PLAYER_02", -1f, -1, 0));
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.SecondaryPlayer.transform, new Vector3(0.5f, 1.5f, 0f), "#COOPPAST_COOP_04", -1f, -1, 1));
		PastCameraUtility.UnlockConversation();
		GameManager.PVP_ENABLED = true;
		GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_Boss_Theme_Beholster", GameManager.Instance.Dungeon.gameObject);
	}

	public IEnumerator DoAmbientTalk(Transform baseTransform, Vector3 offset, string stringKey, float duration, int specificStringIndex = -1, int OnlyThisPlayerInput = -1)
	{
		TextBoxManager.ShowTextBox(baseTransform.position + offset, baseTransform, duration, (specificStringIndex == -1) ? StringTableManager.GetString(stringKey) : StringTableManager.GetExactString(stringKey, specificStringIndex), string.Empty, false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, true);
		bool advancedPressed = false;
		while (!advancedPressed)
		{
			switch (OnlyThisPlayerInput)
			{
			case 0:
				advancedPressed = BraveInput.GetInstanceForPlayer(0).MenuInteractPressed;
				break;
			case 1:
				advancedPressed = BraveInput.GetInstanceForPlayer(1).MenuInteractPressed;
				break;
			default:
				advancedPressed = BraveInput.GetInstanceForPlayer(0).MenuInteractPressed || BraveInput.GetInstanceForPlayer(1).MenuInteractPressed;
				break;
			}
			yield return null;
		}
		TextBoxManager.ClearTextBox(baseTransform);
	}
}
