using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using InControl;
using UnityEngine;

public class TalkDoer : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public List<TalkModule> modules;

	public Transform speakPoint;

	public string audioCharacterSpeechTag = string.Empty;

	[Header("Interactable Region")]
	public bool usesOverrideInteractionRegion;

	[ShowInInspectorIf("usesOverrideInteractionRegion", false)]
	public Vector2 overrideRegionOffset = Vector2.zero;

	[ShowInInspectorIf("usesOverrideInteractionRegion", false)]
	public Vector2 overrideRegionDimensions = Vector2.zero;

	[Header("Core Speech")]
	public string FirstMeetingEverModule = string.Empty;

	public string FirstMeetingSessionModule = string.Empty;

	public string RepeatMeetingSessionModule = string.Empty;

	[Header("Other Options")]
	[CheckAnimation(null)]
	public string fallbackAnimName = "idle";

	[CheckAnimation(null)]
	public string defaultSpeechAnimName = "talk";

	public bool usesCustomBetrayalLogic;

	public string betrayalSpeechKey = string.Empty;

	public bool betrayalSpeechSequential;

	private int betrayalSpeechIndex = -1;

	public string hitAnimName;

	public bool DoesVanish = true;

	public string vanishAnimName = "exit";

	public Action OnBetrayalWarning;

	public Action OnBetrayal;

	public List<GameObject> itemsToLeaveBehind;

	public bool alwaysWaitsForInput;

	public TalkDoer echo1;

	public TalkDoer echo2;

	public float conversationBreakRadius = 5f;

	public List<CharacterTalkModuleOverride> characterOverrides;

	public bool OverrideNoninteractable;

	protected TalkModule defaultModule;

	protected bool m_isTalking;

	protected bool m_uninteractable;

	protected int numTimesSpokenTo;

	protected int hitCount;

	protected bool m_isDealingWithBetrayal;

	private bool m_hack_isOpeningTruthChest;

	private bool m_isDoingForcedSpeech;

	protected PlayerController talkingPlayer;

	private void Start()
	{
		EncounterTrackable component = GetComponent<EncounterTrackable>();
		if (!string.IsNullOrEmpty(FirstMeetingEverModule) && component != null && GameStatsManager.Instance.QueryEncounterable(component) == 0)
		{
			defaultModule = GetModuleFromName(FirstMeetingEverModule);
		}
		else if (!string.IsNullOrEmpty(FirstMeetingSessionModule))
		{
			defaultModule = GetModuleFromName(FirstMeetingSessionModule);
		}
		else if (!string.IsNullOrEmpty(RepeatMeetingSessionModule))
		{
			defaultModule = GetModuleFromName(RepeatMeetingSessionModule);
		}
		else
		{
			defaultModule = GetModuleFromName("start");
		}
		if (base.specRigidbody != null)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		}
		if (base.name.Contains("Truth_Knower"))
		{
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			List<Chest> componentsInRoom = roomFromPosition.GetComponentsInRoom<Chest>();
			for (int i = 0; i < componentsInRoom.Count; i++)
			{
				if (componentsInRoom[i].name.ToLowerInvariant().Contains("truth"))
				{
					MajorBreakable obj = componentsInRoom[i].majorBreakable;
					obj.OnBreak = (Action)Delegate.Combine(obj.OnBreak, new Action(OnTruthChestBroken));
				}
			}
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
	}

	private void Update()
	{
		if (m_isTalking && Vector2.Distance(talkingPlayer.sprite.WorldCenter, base.sprite.WorldCenter) > conversationBreakRadius)
		{
			ForceEndConversation();
		}
	}

	private void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (m_isTalking)
		{
			return;
		}
		Projectile component = rigidbodyCollision.OtherRigidbody.GetComponent<Projectile>();
		if (!(component != null) || !(component.Owner is PlayerController) || m_isDealingWithBetrayal)
		{
			return;
		}
		hitCount++;
		if (usesCustomBetrayalLogic)
		{
			if (hitCount < 2)
			{
				if (OnBetrayalWarning != null)
				{
					OnBetrayalWarning();
				}
			}
			else if (hitCount < 3 && OnBetrayal != null)
			{
				OnBetrayal();
			}
			return;
		}
		if (!string.IsNullOrEmpty(hitAnimName))
		{
			base.spriteAnimator.PlayForDuration(hitAnimName, -1f, fallbackAnimName);
		}
		if (!DoesVanish || hitCount < 2)
		{
			if (!string.IsNullOrEmpty(betrayalSpeechKey))
			{
				StartCoroutine(HandleBetrayal());
			}
		}
		else
		{
			Vanish();
		}
	}

	private void OnTruthChestBroken()
	{
		StartCoroutine(HandleBetrayal());
		StartCoroutine(DelayedVanish(2f));
	}

	private IEnumerator DelayedVanish(float delay)
	{
		yield return new WaitForSeconds(delay);
		Vanish();
	}

	private void Vanish()
	{
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		roomFromPosition.DeregisterInteractable(this);
		TextBoxManager.ClearTextBox(speakPoint);
		if (base.specRigidbody != null)
		{
			base.specRigidbody.enabled = false;
		}
		tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(vanishAnimName);
		for (int i = 0; i < itemsToLeaveBehind.Count; i++)
		{
			itemsToLeaveBehind[i].transform.parent = base.transform.parent;
		}
		if (clipByName != null)
		{
			base.spriteAnimator.PlayAndDestroyObject(vanishAnimName);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator HandleBetrayal()
	{
		m_isDealingWithBetrayal = true;
		TextBoxManager.ClearTextBox(speakPoint);
		yield return null;
		string displayString = ((!betrayalSpeechSequential) ? StringTableManager.GetString(betrayalSpeechKey) : StringTableManager.GetStringSequential(betrayalSpeechKey, ref betrayalSpeechIndex));
		if (string.IsNullOrEmpty(audioCharacterSpeechTag))
		{
			TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -5f), speakPoint, -1f, displayString, string.Empty);
		}
		else
		{
			TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -5f), speakPoint, -1f, displayString, audioCharacterSpeechTag);
		}
		yield return new WaitForSeconds(4f);
		TextBoxManager.ClearTextBox(speakPoint);
		m_isDealingWithBetrayal = false;
	}

	private TalkModule GetModuleFromName(string ID)
	{
		for (int i = 0; i < modules.Count; i++)
		{
			if (modules[i].moduleID == ID)
			{
				return modules[i];
			}
		}
		return null;
	}

	private void ProcessResponseAction(TalkResult result)
	{
		switch (result.action)
		{
		case TalkResult.TalkResultAction.CHANGE_DEFAULT_MODULE:
			defaultModule = GetModuleFromName(result.actionData);
			break;
		case TalkResult.TalkResultAction.OPEN_TRUTH_CHEST:
			StartCoroutine(DelayedChestOpen(3f));
			break;
		case TalkResult.TalkResultAction.VANISH:
			StartCoroutine(DelayedVanish(3f));
			break;
		case TalkResult.TalkResultAction.TOSS_CURRENT_GUN_IN_POT:
		{
			WitchCauldronController component = base.transform.parent.GetComponent<WitchCauldronController>();
			if (component != null)
			{
				component.TossPlayerEquippedGun(talkingPlayer);
			}
			break;
		}
		case TalkResult.TalkResultAction.RENDER_SILENT:
			StartCoroutine(MakeUninteractable(float.Parse(result.actionData)));
			break;
		case TalkResult.TalkResultAction.CHANGE_DEFAULT_MODULE_OF_OTHER_TALKDOER:
			result.objectData.GetComponent<TalkDoer>().defaultModule = result.objectData.GetComponent<TalkDoer>().GetModuleFromName(result.actionData);
			break;
		case TalkResult.TalkResultAction.SPAWN_ITEM:
			LootEngine.SpewLoot(result.objectData, (!(base.specRigidbody != null)) ? base.sprite.WorldCenter : base.specRigidbody.UnitCenter);
			break;
		case TalkResult.TalkResultAction.MAKE_TALKDOER_INTERACTABLE:
			result.objectData.GetComponent<TalkDoer>().OverrideNoninteractable = false;
			break;
		case TalkResult.TalkResultAction.SPAWN_ITEM_FROM_TABLE:
		{
			GameObject itemToSpawn = result.lootTableData.SelectByWeightWithoutDuplicatesFullPrereqs(null);
			LootEngine.SpewLoot(itemToSpawn, (!(base.specRigidbody != null)) ? base.sprite.WorldCenter : base.specRigidbody.UnitCenter);
			break;
		}
		case TalkResult.TalkResultAction.CUSTOM_ACTION:
			ProcessCustomAction(result.customActionID, result.actionData, result.objectData);
			break;
		}
	}

	private IEnumerator MakeUninteractable(float duration)
	{
		m_uninteractable = true;
		yield return new WaitForSeconds(duration);
		m_uninteractable = false;
	}

	private void OpenTruthChest()
	{
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		List<Chest> componentsInRoom = roomFromPosition.GetComponentsInRoom<Chest>();
		for (int i = 0; i < componentsInRoom.Count; i++)
		{
			if (componentsInRoom[i].name.ToLowerInvariant().Contains("truth"))
			{
				componentsInRoom[i].IsLocked = false;
				componentsInRoom[i].IsSealed = false;
				tk2dSpriteAnimator componentInChildren = componentsInRoom[i].transform.Find("lock").GetComponentInChildren<tk2dSpriteAnimator>();
				if (componentInChildren != null)
				{
					componentInChildren.StopAndResetFrame();
					StartCoroutine(PlayDelayedTruthChestLockOpen(componentInChildren, 1f));
				}
			}
		}
	}

	private IEnumerator DelayedChestOpen(float delay)
	{
		m_hack_isOpeningTruthChest = true;
		yield return new WaitForSeconds(delay);
		OpenTruthChest();
		m_hack_isOpeningTruthChest = false;
	}

	private IEnumerator PlayDelayedTruthChestLockOpen(tk2dSpriteAnimator lockAnimator, float delay)
	{
		yield return new WaitForSeconds(delay);
		lockAnimator.PlayAndDestroyObject("truth_lock_open");
	}

	private void ProcessCustomAction(string customActionID, string actionData, GameObject objectData)
	{
		Debug.LogError("Custom action: " + customActionID + " is not implemented!");
	}

	private void BeginConversation(PlayerController player)
	{
		m_isTalking = true;
		GameUIRoot.Instance.InitializeConversationPortrait(player);
		EncounterTrackable component = GetComponent<EncounterTrackable>();
		if (numTimesSpokenTo == 0 && component != null)
		{
			GameStatsManager.Instance.HandleEncounteredObject(component);
		}
		numTimesSpokenTo++;
		StartCoroutine(HandleConversationModule(defaultModule));
		if ((defaultModule.moduleID == FirstMeetingSessionModule || defaultModule.moduleID == FirstMeetingEverModule) && !string.IsNullOrEmpty(RepeatMeetingSessionModule))
		{
			defaultModule = GetModuleFromName(RepeatMeetingSessionModule);
		}
	}

	private void ForceEndConversation()
	{
		TextBoxManager.ClearTextBox(speakPoint);
		StopAllCoroutines();
		if (m_hack_isOpeningTruthChest)
		{
			OpenTruthChest();
			m_hack_isOpeningTruthChest = false;
		}
		EndConversation();
	}

	private void EndConversation()
	{
		m_isTalking = false;
		if (!string.IsNullOrEmpty(fallbackAnimName))
		{
			base.spriteAnimator.Play(fallbackAnimName);
		}
	}

	public void ForceTimedSpeech(string words, float initialDelay, float duration, TextBoxManager.BoxSlideOrientation slideOrientation)
	{
		Debug.Log("starting forced timed speech: " + words);
		StartCoroutine(HandleForcedTimedSpeech(words, initialDelay, duration, slideOrientation));
	}

	private IEnumerator HandleForcedTimedSpeech(string words, float initialDelay, float duration, TextBoxManager.BoxSlideOrientation slideOrientation)
	{
		m_isDoingForcedSpeech = true;
		while (initialDelay > 0f)
		{
			initialDelay -= BraveTime.DeltaTime;
			if (!m_isDoingForcedSpeech)
			{
				Debug.Log("breaking forced timed speech: " + words);
				yield break;
			}
			yield return null;
		}
		TextBoxManager.ClearTextBox(speakPoint);
		if (!string.IsNullOrEmpty(defaultSpeechAnimName))
		{
			base.spriteAnimator.Play(defaultSpeechAnimName);
		}
		if (string.IsNullOrEmpty(audioCharacterSpeechTag))
		{
			TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -4f), speakPoint, -1f, words, string.Empty, true, slideOrientation);
		}
		else
		{
			TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -4f), speakPoint, -1f, words, audioCharacterSpeechTag, false, slideOrientation);
		}
		if (duration > 0f)
		{
			while (duration > 0f && m_isDoingForcedSpeech)
			{
				duration -= BraveTime.DeltaTime;
				yield return null;
			}
		}
		else
		{
			while (m_isDoingForcedSpeech)
			{
				yield return null;
			}
		}
		Debug.Log("ending forced timed speech: " + words);
		TextBoxManager.ClearTextBox(speakPoint);
		if (!string.IsNullOrEmpty(fallbackAnimName))
		{
			base.spriteAnimator.Play(fallbackAnimName);
		}
		m_isDoingForcedSpeech = false;
	}

	private IEnumerator HandleConversationModule(TalkModule module)
	{
		int textIndex = 0;
		yield return null;
		if (module.usesAnimation)
		{
			if (!string.IsNullOrEmpty(module.animationName))
			{
				if (module.animationDuration > 0f)
				{
					base.spriteAnimator.PlayForDuration(module.animationName, module.animationDuration, fallbackAnimName);
				}
				else
				{
					base.spriteAnimator.Play(module.animationName);
				}
			}
			else if (module.animationDuration > 0f)
			{
				base.spriteAnimator.PlayForDuration(defaultSpeechAnimName, module.animationDuration, fallbackAnimName);
			}
			else
			{
				base.spriteAnimator.Play(defaultSpeechAnimName);
			}
		}
		string overrideResponseValue1 = string.Empty;
		string overrideResponseValue2 = string.Empty;
		string overrideFollowupModule1 = string.Empty;
		string overrideFollowupModule2 = string.Empty;
		while (textIndex < module.stringKeys.Length)
		{
			if (textIndex > 0)
			{
				TextBoxManager.ClearTextBox(speakPoint);
				if (module.usesAnimation && !string.IsNullOrEmpty(module.additionalAnimationName))
				{
					base.spriteAnimator.Play(module.additionalAnimationName);
				}
			}
			string stringKey = module.stringKeys[textIndex];
			if (stringKey == "$anim")
			{
				if (module.animationDuration > 0f)
				{
					yield return new WaitForSeconds(module.animationDuration);
				}
				else
				{
					tk2dSpriteAnimationClip clip = base.spriteAnimator.GetClipByName(module.animationName);
					yield return new WaitForSeconds((float)clip.frames.Length / clip.fps);
				}
			}
			else
			{
				string displayString = ((!module.sequentialStrings) ? StringTableManager.GetString(stringKey) : StringTableManager.GetStringSequential(stringKey, ref module.sequentialStringLastIndex));
				if (displayString.Contains("$"))
				{
					string[] array = displayString.Split('$');
					displayString = array[0];
					overrideResponseValue1 = array[1];
					overrideResponseValue2 = array[2];
					if (array.Length == 4)
					{
						overrideFollowupModule1 = "#" + array[3];
						overrideFollowupModule2 = overrideFollowupModule1;
					}
					else if (array.Length == 5)
					{
						overrideFollowupModule1 = "#" + array[3];
						overrideFollowupModule2 = "#" + array[4];
					}
				}
				else if (displayString.Contains("&"))
				{
					string[] array2 = displayString.Split('&');
					displayString = array2[0];
					if (echo1 != null)
					{
						echo1.ForceTimedSpeech(array2[1], 1f, 4f, TextBoxManager.BoxSlideOrientation.FORCE_RIGHT);
					}
					if (echo2 != null && array2.Length > 2)
					{
						echo2.ForceTimedSpeech(array2[2], 2f, 4f, TextBoxManager.BoxSlideOrientation.FORCE_LEFT);
					}
				}
				if (string.IsNullOrEmpty(audioCharacterSpeechTag))
				{
					TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -5f), speakPoint, -1f, displayString, string.Empty);
				}
				else
				{
					TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -5f), speakPoint, -1f, displayString, audioCharacterSpeechTag, false);
				}
				if (module.responses.Count > 0 && textIndex == module.stringKeys.Length - 1)
				{
					yield return StartCoroutine(WaitForTextRevealed());
					break;
				}
				yield return StartCoroutine(WaitForPlayer());
			}
			if (echo1 != null)
			{
				echo1.m_isDoingForcedSpeech = false;
			}
			if (echo2 != null)
			{
				echo2.m_isDoingForcedSpeech = false;
			}
			textIndex++;
			yield return new WaitForSeconds(0.05f);
		}
		if (module.moduleResultActions.Count > 0)
		{
			for (int i = 0; i < module.moduleResultActions.Count; i++)
			{
				ProcessResponseAction(module.moduleResultActions[i]);
			}
		}
		if (module.responses.Count > 0)
		{
			if (module.responses.Count == 1 && string.IsNullOrEmpty(module.responses[0].response))
			{
				if (alwaysWaitsForInput)
				{
					yield return StartCoroutine(WaitForPlayer());
					if (echo1 != null)
					{
						echo1.m_isDoingForcedSpeech = false;
					}
					if (echo2 != null)
					{
						echo2.m_isDoingForcedSpeech = false;
					}
				}
				StartCoroutine(HandleConversationModule(GetModuleFromName(module.responses[0].followupModuleID)));
			}
			else
			{
				StartCoroutine(HandleResponses(module, overrideResponseValue1, overrideResponseValue2, overrideFollowupModule1, overrideFollowupModule2));
			}
		}
		else if (module.responses.Count == 0 && !string.IsNullOrEmpty(module.noResponseFollowupModule))
		{
			StartCoroutine(HandleConversationModule(GetModuleFromName(module.noResponseFollowupModule)));
		}
		else
		{
			TextBoxManager.ClearTextBox(speakPoint);
			EndConversation();
		}
	}

	private IEnumerator HandleResponses(TalkModule module, string overrideResponse1, string overrideResponse2, string overrideFollowupModule1, string overrideFollowupModule2)
	{
		int selectedResponse = -1;
		talkingPlayer.SetInputOverride("talkDoerResponse");
		GameUIRoot.Instance.DisplayPlayerConversationOptions(talkingPlayer, module, overrideResponse1, overrideResponse2);
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		talkingPlayer.ClearInputOverride("talkDoerResponse");
		TextBoxManager.ClearTextBox(speakPoint);
		TalkResponse response = module.responses[selectedResponse];
		TalkModule nextModule = null;
		for (int i = 0; i < response.resultActions.Count; i++)
		{
			ProcessResponseAction(response.resultActions[i]);
		}
		if (selectedResponse == 0 && !string.IsNullOrEmpty(overrideFollowupModule1))
		{
			nextModule = new TalkModule();
			nextModule.CopyFrom(module);
			nextModule.stringKeys = new string[1] { overrideFollowupModule1 };
		}
		else if (selectedResponse == 1 && !string.IsNullOrEmpty(overrideFollowupModule2))
		{
			nextModule = new TalkModule();
			nextModule.CopyFrom(module);
			nextModule.stringKeys = new string[1] { overrideFollowupModule2 };
		}
		else if (!string.IsNullOrEmpty(response.followupModuleID))
		{
			nextModule = GetModuleFromName(response.followupModuleID);
		}
		if (nextModule != null)
		{
			StartCoroutine(HandleConversationModule(nextModule));
		}
		else
		{
			EndConversation();
		}
	}

	private IEnumerator WaitForTextRevealed()
	{
		while (TextBoxManager.TextBoxCanBeAdvanced(speakPoint))
		{
			if (BraveInput.WasSelectPressed(InputManager.ActiveDevice))
			{
				TextBoxManager.AdvanceTextBox(speakPoint);
			}
			yield return null;
		}
	}

	private IEnumerator WaitForPlayer()
	{
		while (true)
		{
			if (BraveInput.WasSelectPressed(InputManager.ActiveDevice))
			{
				if (!TextBoxManager.TextBoxCanBeAdvanced(speakPoint))
				{
					break;
				}
				TextBoxManager.AdvanceTextBox(speakPoint);
			}
			yield return null;
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!this)
		{
			return 1000f;
		}
		if (m_uninteractable || OverrideNoninteractable)
		{
			return 1000f;
		}
		if (usesOverrideInteractionRegion)
		{
			return BraveMathCollege.DistToRectangle(point, base.transform.position.XY() + overrideRegionOffset * 0.0625f, overrideRegionDimensions * 0.0625f);
		}
		Bounds bounds = base.sprite.GetBounds();
		return BraveMathCollege.DistToRectangle(point, base.sprite.transform.position + bounds.center - bounds.extents, bounds.size);
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (!m_isTalking && (bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (!m_isTalking && (bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
			base.sprite.UpdateZDepth();
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_isTalking)
		{
			talkingPlayer = interactor;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
			BeginConversation(interactor);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
