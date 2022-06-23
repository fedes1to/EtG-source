using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

public class ConvictPastController : MonoBehaviour
{
	public bool InstantBossFight;

	public TalkDoerLite InitialTalkDoer;

	public TalkDoerLite BaldoTalkDoer;

	public TalkDoerLite BaldoBossTalkDoer;

	public NightclubCrowdController crowdController;

	public TalkDoerLite[] HmonSoldiers;

	public tk2dSpriteAnimator DeskAnimator;

	public GameObject DeskAnimatorPoof;

	public TalkDoerLite PhantomEndTalkDoer;

	public tk2dSpriteAnimator Car;

	public Renderer CarHeadlightsRenderer;

	public SpeculativeRigidbody ExitDoorRigidbody;

	public SpeculativeRigidbody MainDoorBlocker;

	private bool m_hasStartedBossSequence;

	public static float FREEZE_FRAME_DURATION = 2f;

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		NightclubCrowdController nightclubCrowdController = crowdController;
		nightclubCrowdController.OnPanic = (Action)Delegate.Combine(nightclubCrowdController.OnPanic, new Action(HandlePrematurePanic));
		if (InstantBossFight)
		{
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			primaryPlayer.transform.position = new Vector2(31f, 27f);
			primaryPlayer.specRigidbody.Reinitialize();
			BaldoBossTalkDoer.specRigidbody.enabled = false;
			BaldoBossTalkDoer.gameObject.SetActive(false);
			List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
			for (int i = 0; i < allHealthHavers.Count; i++)
			{
				if (allHealthHavers[i].IsBoss)
				{
					allHealthHavers[i].GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
					allHealthHavers[i].GetComponent<GenericIntroDoer>().TriggerSequence(primaryPlayer);
				}
			}
			yield break;
		}
		GameManager.Instance.PrimaryPlayer.SetInputOverride("past");
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.SecondaryPlayer.SetInputOverride("past");
		}
		GameManager.Instance.PrimaryPlayer.ForceIdleFacePoint(Vector2.down);
		for (int j = 0; j < HmonSoldiers.Length; j++)
		{
			if (j == 0)
			{
				Fsm fsm = HmonSoldiers[0].playmakerFsm.Fsm;
				for (int k = 0; k < fsm.States.Length; k++)
				{
					FsmState fsmState = fsm.States[k];
					for (int l = 0; l < fsmState.Actions.Length; l++)
					{
						if (fsmState.Actions[l] is BecomeHostile)
						{
							switch (l)
							{
							case 0:
								(fsmState.Actions[l] as BecomeHostile).alternativeTarget = HmonSoldiers[0];
								break;
							case 1:
								(fsmState.Actions[l] as BecomeHostile).alternativeTarget = HmonSoldiers[1];
								break;
							case 2:
								(fsmState.Actions[l] as BecomeHostile).alternativeTarget = HmonSoldiers[2];
								break;
							case 3:
								(fsmState.Actions[l] as BecomeHostile).alternativeTarget = HmonSoldiers[3];
								break;
							}
						}
					}
				}
			}
			HmonSoldiers[j].renderer.enabled = true;
			HmonSoldiers[j].specRigidbody.enabled = true;
			HmonSoldiers[j].specRigidbody.Reinitialize();
		}
		Pixelator.Instance.FadeToBlack(0.25f, true, 0.05f);
		Pixelator.Instance.TriggerPastFadeIn();
		yield return new WaitForSeconds(0.4f);
		Pixelator.Instance.SetOcclusionDirty();
		PastCameraUtility.LockConversation(InitialTalkDoer.speakPoint.transform.position.XY() + new Vector2(0f, 3f));
		yield return null;
		Pixelator.Instance.SetOcclusionDirty();
		yield return new WaitForSeconds(6f);
		yield return StartCoroutine(DoAmbientTalk(GameManager.Instance.PrimaryPlayer.transform, new Vector3(0.75f, 1.5f, 0f), "#CONVICTPAST_THOUGHTS_01", -1f, true));
		yield return new WaitForSeconds(1f);
		InitialTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		while (InitialTalkDoer.IsTalking)
		{
			yield return null;
		}
		GameManager.Instance.MainCameraController.UpdateOverridePosition(GameManager.Instance.MainCameraController.OverridePosition + new Vector3(0f, -2.5f, 0f), 5f);
		InitialTalkDoer.PathfindToPosition(InitialTalkDoer.specRigidbody.UnitCenter + new Vector2(-7f, -3f));
		Vector2 lastPositionFlunky = InitialTalkDoer.specRigidbody.UnitCenter;
		BaldoTalkDoer.PathfindToPosition(BaldoTalkDoer.specRigidbody.UnitCenter + new Vector2(0f, 25f));
		Vector2 lastPosition = BaldoTalkDoer.specRigidbody.UnitCenter;
		Vector2[] HmonTargetPositions = new Vector2[4]
		{
			new Vector2(-5f, 24f),
			new Vector2(-2f, 22f),
			new Vector2(2.5f, 22f),
			new Vector2(5f, 24f)
		};
		Vector2[] hmonLastPositions = new Vector2[HmonSoldiers.Length];
		for (int m = 0; m < HmonSoldiers.Length; m++)
		{
			HmonSoldiers[m].PathfindToPosition(HmonSoldiers[0].specRigidbody.UnitCenter + HmonTargetPositions[m]);
			hmonLastPositions[m] = HmonSoldiers[m].specRigidbody.UnitCenter;
		}
		bool hasPath = true;
		while (hasPath)
		{
			hasPath = false;
			if (InitialTalkDoer.CurrentPath != null)
			{
				hasPath = true;
				InitialTalkDoer.specRigidbody.Velocity = InitialTalkDoer.GetPathVelocityContribution(lastPositionFlunky, 16);
				lastPositionFlunky = InitialTalkDoer.specRigidbody.UnitCenter;
			}
			if (BaldoTalkDoer.CurrentPath != null)
			{
				hasPath = true;
				BaldoTalkDoer.specRigidbody.Velocity = BaldoTalkDoer.GetPathVelocityContribution(lastPosition, 16);
				lastPosition = BaldoTalkDoer.specRigidbody.UnitCenter;
			}
			for (int n = 0; n < HmonSoldiers.Length; n++)
			{
				if (HmonSoldiers[n].CurrentPath != null)
				{
					hasPath = true;
					HmonSoldiers[n].specRigidbody.Velocity = HmonSoldiers[n].GetPathVelocityContribution(hmonLastPositions[n], 16);
					hmonLastPositions[n] = HmonSoldiers[n].specRigidbody.UnitCenter;
				}
			}
			yield return null;
		}
		BaldoTalkDoer.specRigidbody.Velocity = Vector2.zero;
		BaldoTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		while (BaldoTalkDoer.IsTalking)
		{
			yield return null;
		}
		BaldoTalkDoer.PathfindToPosition(BaldoTalkDoer.specRigidbody.UnitCenter + new Vector2(-0.5f, -25f));
		StartCoroutine(DoPath(BaldoTalkDoer, true));
		yield return new WaitForSeconds(1f);
		HmonSoldiers[0].Interact(GameManager.Instance.PrimaryPlayer);
		while (HmonSoldiers[0].IsTalking)
		{
			yield return null;
		}
		PastCameraUtility.UnlockConversation();
		DeskAnimator.sprite.HeightOffGround = -4.5f;
		DeskAnimator.sprite.UpdateZDepth();
		DeskAnimator.Play();
		MinorBreakable[] componentsInChildren = DeskAnimator.GetComponentsInChildren<MinorBreakable>();
		foreach (MinorBreakable minorBreakable in componentsInChildren)
		{
			minorBreakable.Break(Vector2.down);
		}
		AkSoundEngine.PostEvent("Play_MUS_Ending_State_02", GameManager.Instance.gameObject);
		yield return new WaitForSeconds(0.4f);
		DeskAnimatorPoof.SetActive(true);
		while (true)
		{
			bool shouldBreak2 = false;
			for (int num2 = 0; num2 < GameManager.Instance.AllPlayers.Length; num2++)
			{
				if (GameManager.Instance.AllPlayers[num2].CenterPosition.x < BaldoBossTalkDoer.specRigidbody.UnitBottomCenter.x + 20f)
				{
					shouldBreak2 = true;
				}
			}
			if (shouldBreak2)
			{
				break;
			}
			yield return null;
		}
		while (true)
		{
			bool shouldBreak = false;
			for (int num3 = 0; num3 < GameManager.Instance.AllPlayers.Length; num3++)
			{
				Vector2 centerPosition = GameManager.Instance.AllPlayers[num3].CenterPosition;
				if (centerPosition.y > BaldoBossTalkDoer.specRigidbody.UnitBottomCenter.y - 7f)
				{
					shouldBreak = true;
				}
				if (Mathf.Abs(BaldoBossTalkDoer.specRigidbody.UnitBottomCenter.x - centerPosition.x) < 3f && centerPosition.y > BaldoBossTalkDoer.specRigidbody.UnitBottomCenter.y - 13f)
				{
					shouldBreak = true;
				}
			}
			if (shouldBreak)
			{
				break;
			}
			yield return null;
		}
		MainDoorBlocker.gameObject.SetActive(true);
		MainDoorBlocker.enabled = true;
		MainDoorBlocker.Reinitialize();
		yield return StartCoroutine(HandleBossStart());
	}

	private void HandlePrematurePanic()
	{
		if (!m_hasStartedBossSequence)
		{
			StartCoroutine(HandleBossStart(3f));
		}
	}

	private IEnumerator HandleBossStart(float initialDelay = 0f)
	{
		if (initialDelay > 0f)
		{
			yield return new WaitForSeconds(initialDelay);
		}
		if (m_hasStartedBossSequence)
		{
			yield break;
		}
		m_hasStartedBossSequence = true;
		PastCameraUtility.LockConversation(BaldoBossTalkDoer.speakPoint.transform.position.XY() + new Vector2(0f, -1f));
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			PlayerController secondaryPlayer = GameManager.Instance.SecondaryPlayer;
			if (primaryPlayer.transform.position.x > secondaryPlayer.transform.position.x && primaryPlayer.transform.position.x > BaldoBossTalkDoer.specRigidbody.UnitBottomCenter.x + 16f)
			{
				primaryPlayer.ReuniteWithOtherPlayer(secondaryPlayer);
			}
			else if (secondaryPlayer.transform.position.x > BaldoBossTalkDoer.specRigidbody.UnitBottomCenter.x + 16f)
			{
				secondaryPlayer.ReuniteWithOtherPlayer(primaryPlayer);
			}
		}
		BaldoBossTalkDoer.Interact(GameManager.Instance.PrimaryPlayer);
		while (BaldoBossTalkDoer.IsTalking)
		{
			yield return null;
		}
		PastCameraUtility.UnlockConversation();
		TextBoxManager.ClearTextBoxImmediate(BaldoBossTalkDoer.speakPoint);
		BaldoBossTalkDoer.specRigidbody.enabled = false;
		BaldoBossTalkDoer.gameObject.SetActive(false);
		List<HealthHaver> healthHavers = StaticReferenceManager.AllHealthHavers;
		for (int i = 0; i < healthHavers.Count; i++)
		{
			if (healthHavers[i].IsBoss)
			{
				healthHavers[i].specRigidbody.transform.position = BaldoBossTalkDoer.transform.position;
				healthHavers[i].specRigidbody.Reinitialize();
				healthHavers[i].GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
				healthHavers[i].GetComponent<GenericIntroDoer>().TriggerSequence(GameManager.Instance.PrimaryPlayer);
			}
		}
	}

	private IEnumerator DoPath(TalkDoerLite source, bool doDestroy)
	{
		Vector2 lastPos = source.specRigidbody.UnitCenter;
		while (source.CurrentPath != null)
		{
			source.specRigidbody.Velocity = source.GetPathVelocityContribution(lastPos, 16);
			lastPos = source.specRigidbody.UnitCenter;
			yield return null;
		}
		if (doDestroy)
		{
			UnityEngine.Object.Destroy(source.gameObject);
		}
	}

	public void OnBossKilled(Transform bossTransform)
	{
		StartCoroutine(HandleBossKilled(bossTransform));
	}

	private IEnumerator EnableHeadlights()
	{
		float ela = 0f;
		float dura = 0.5f;
		CarHeadlightsRenderer.enabled = true;
		Color targetColor = CarHeadlightsRenderer.material.GetColor("_TintColor");
		Color startColor = targetColor.WithAlpha(0f);
		while (ela < dura)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t = ela / dura;
			CarHeadlightsRenderer.material.SetColor("_TintColor", Color.Lerp(startColor, targetColor, t));
			yield return null;
		}
	}

	private IEnumerator HandleBossKilled(Transform bossTransform)
	{
		GameStatsManager.Instance.SetCharacterSpecificFlag(PlayableCharacters.Convict, CharacterSpecificGungeonFlags.KILLED_PAST, true);
		GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_CONVICT_PAST, true);
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_KILLED_PAST, 1f);
		if (PhantomEndTalkDoer != null)
		{
			PhantomEndTalkDoer.gameObject.SetActive(true);
			PhantomEndTalkDoer.ShowOutlines = false;
		}
		GameUIRoot.Instance.ToggleLowerPanels(false, true, string.Empty);
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		ExitDoorRigidbody.enabled = false;
		BaldoBossTalkDoer.specRigidbody.enabled = true;
		BaldoBossTalkDoer.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.Projectile));
		BaldoBossTalkDoer.gameObject.SetActive(true);
		BaldoBossTalkDoer.transform.position = bossTransform.position;
		BaldoBossTalkDoer.specRigidbody.Reinitialize();
		BaldoBossTalkDoer.aiAnimator.PlayUntilCancelled("die");
		BaldoBossTalkDoer.sprite.IsPerpendicular = false;
		BaldoBossTalkDoer.sprite.HeightOffGround = -1f;
		BaldoBossTalkDoer.sprite.UpdateZDepth();
		dfControl goToCarPanel = GameUIRoot.Instance.Manager.AddPrefab(BraveResources.Load("Global Prefabs/GoToCarPanel") as GameObject);
		GameUIRoot.Instance.AddControlToMotionGroups(goToCarPanel, DungeonData.Direction.WEST, true);
		GameUIRoot.Instance.MoveNonCoreGroupOnscreen(goToCarPanel, true);
		while (GameManager.Instance.MainCameraController.ManualControl)
		{
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		GameUIRoot.Instance.MoveNonCoreGroupOnscreen(goToCarPanel);
		bool atCar2 = false;
		bool hasOpened = false;
		bool isClosing = false;
		bool coopInCar = false;
		while (!atCar2)
		{
			if (!hasOpened && Vector2.Distance(Car.Sprite.WorldCenter, GameManager.Instance.PrimaryPlayer.CenterPosition) < 5f)
			{
				hasOpened = true;
				Car.Play("getaway_car_open");
			}
			if (hasOpened && !isClosing)
			{
				bool flag = false;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					if (Vector2.Distance(Car.Sprite.WorldCenter, GameManager.Instance.PrimaryPlayer.CenterPosition) < 0.8f)
					{
						float num = Vector2.Distance(Car.Sprite.WorldCenter, GameManager.Instance.SecondaryPlayer.CenterPosition);
						if (num < 0.8f)
						{
							flag = true;
							coopInCar = true;
						}
						else if (num > 4f)
						{
							flag = true;
							coopInCar = false;
						}
					}
				}
				else
				{
					flag = Vector2.Distance(Car.Sprite.WorldCenter, GameManager.Instance.PrimaryPlayer.CenterPosition) < 0.8f;
				}
				if (flag)
				{
					isClosing = true;
					Car.Play("getaway_car_close");
					GameUIRoot.Instance.RemoveControlFromMotionGroups(goToCarPanel);
					UnityEngine.Object.Destroy(goToCarPanel.gameObject);
					for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
					{
						if ((bool)GameManager.Instance.AllPlayers[i])
						{
							GameManager.Instance.AllPlayers[i].CurrentInputState = PlayerInputState.NoInput;
							GameManager.Instance.AllPlayers[i].ForceStopDodgeRoll();
							GameManager.Instance.AllPlayers[i].specRigidbody.Velocity = Vector2.zero;
						}
					}
				}
			}
			if (isClosing && !Car.IsPlaying("getaway_car_close"))
			{
				atCar2 = true;
				break;
			}
			yield return null;
		}
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			if ((bool)GameManager.Instance.AllPlayers[j])
			{
				GameManager.Instance.AllPlayers[j].CurrentInputState = PlayerInputState.NoInput;
			}
		}
		ParticleSystem[] carExhausts = Car.GetComponentsInChildren<ParticleSystem>(true);
		for (int k = 0; k < carExhausts.Length; k++)
		{
			carExhausts[k].gameObject.SetActive(true);
		}
		StartCoroutine(EnableHeadlights());
		float elapsed = 0f;
		float duration = 2f;
		Vector3 startPos = Car.transform.position;
		Vector3 playerStartPos = GameManager.Instance.PrimaryPlayer.transform.position;
		GameManager.Instance.MainCameraController.SetManualControl(true);
		GameManager.Instance.MainCameraController.OverridePosition = Car.Sprite.WorldCenter;
		GameManager.Instance.PrimaryPlayer.IsVisible = false;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && coopInCar)
		{
			GameManager.Instance.SecondaryPlayer.IsVisible = false;
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			Vector3 offset = Vector3.Lerp(Vector3.zero, Vector3.right * 30f, BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, elapsed / duration));
			Car.transform.position = startPos + offset;
			GameManager.Instance.PrimaryPlayer.transform.position = playerStartPos + offset;
			GameManager.Instance.PrimaryPlayer.specRigidbody.Reinitialize();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && coopInCar)
			{
				GameManager.Instance.SecondaryPlayer.transform.position = playerStartPos + offset;
				GameManager.Instance.SecondaryPlayer.specRigidbody.Reinitialize();
			}
			GameManager.Instance.MainCameraController.OverridePosition = Car.Sprite.WorldCenter;
			yield return null;
		}
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		GameManager.Instance.MainCameraController.OverridePosition = GameManager.Instance.MainCameraController.transform.position;
		Pixelator.Instance.FreezeFrame();
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		float ela = 0f;
		while (ela < FREEZE_FRAME_DURATION)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		BraveTime.ClearMultiplier(base.gameObject);
		PastCameraUtility.LockConversation(GameManager.Instance.PrimaryPlayer.CenterPosition);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		ttcc.ClearDebris();
		ttcc.ForceNoTimefallForCoop = !coopInCar;
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	public IEnumerator DoAmbientTalk(Transform baseTransform, Vector3 offset, string stringKey, float duration, bool isThoughtBubble)
	{
		if (isThoughtBubble)
		{
			TextBoxManager.ShowThoughtBubble(baseTransform.position + offset, baseTransform, duration, StringTableManager.GetString(stringKey), false, true, GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag);
		}
		else
		{
			TextBoxManager.ShowTextBox(baseTransform.position + offset, baseTransform, duration, StringTableManager.GetString(stringKey), GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag, false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, true);
		}
		bool advancedPressed = false;
		while (!advancedPressed)
		{
			advancedPressed = BraveInput.GetInstanceForPlayer(0).WasAdvanceDialoguePressed();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				advancedPressed |= BraveInput.GetInstanceForPlayer(1).WasAdvanceDialoguePressed();
			}
			yield return null;
		}
		TextBoxManager.ClearTextBox(baseTransform);
	}

	private void Update()
	{
	}
}
