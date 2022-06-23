using System;
using System.Collections;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;

public class CrestDoorController : BraveBehaviour, IPlayerInteractable
{
	public SpeculativeRigidbody AltarRigidbody;

	public SpeculativeRigidbody SarcoRigidbody;

	public ScreenShakeSettings SlideShake;

	public string displayTextKey;

	public string acceptOptionKey;

	public string declineOptionKey;

	public tk2dSprite CrestSprite;

	public Transform talkPoint;

	public tk2dSpriteAnimator cryoAnimator;

	public string cryoArriveAnimation;

	public string cyroDepartAnimation;

	private bool m_isOpen;

	private FsmBool m_cryoBool;

	private FsmBool m_normalBool;

	private float m_transitionTime;

	private float m_previousTransitionTime;

	private IEnumerator Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleGoToCathedral));
		TalkDoerLite cryoButton = base.transform.position.GetAbsoluteRoom().hierarchyParent.GetComponentInChildren<TalkDoerLite>();
		if ((bool)cryoButton && cryoButton.name.Contains("CryoButton"))
		{
			cryoButton.OnGenericFSMActionA = (Action)Delegate.Combine(cryoButton.OnGenericFSMActionA, new Action(SwitchToCryoElevator));
			cryoButton.OnGenericFSMActionB = (Action)Delegate.Combine(cryoButton.OnGenericFSMActionB, new Action(RescindCryoElevator));
			m_cryoBool = cryoButton.playmakerFsm.FsmVariables.GetFsmBool("IS_CRYO");
			m_normalBool = cryoButton.playmakerFsm.FsmVariables.GetFsmBool("IS_NORMAL");
			m_cryoBool.Value = false;
			m_normalBool.Value = true;
		}
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		bool hasCrest = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (playerController.healthHaver.HasCrest)
			{
				hasCrest = true;
				break;
			}
		}
		if (!hasCrest && (bool)cryoButton)
		{
			cryoButton.transform.position.GetAbsoluteRoom().DeregisterInteractable(cryoButton);
			RoomHandler.unassignedInteractableObjects.Remove(cryoButton);
			cryoButton.gameObject.SetActive(false);
			cryoButton.transform.parent.gameObject.SetActive(false);
			SpeculativeRigidbody[] componentsInChildren = cryoButton.gameObject.GetComponentsInChildren<SpeculativeRigidbody>(true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = false;
			}
		}
	}

	private void RescindCryoElevator()
	{
		m_cryoBool.Value = false;
		m_normalBool.Value = true;
		if ((bool)cryoAnimator && !string.IsNullOrEmpty(cyroDepartAnimation))
		{
			cryoAnimator.Play(cyroDepartAnimation);
		}
	}

	private void SwitchToCryoElevator()
	{
		m_cryoBool.Value = true;
		m_normalBool.Value = false;
		if ((bool)cryoAnimator && !string.IsNullOrEmpty(cryoArriveAnimation))
		{
			cryoAnimator.Play(cryoArriveAnimation);
		}
	}

	private void HandleGoToCathedral(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (GameManager.Instance.IsLoadingLevel || !m_isOpen || !specRigidbody.gameActor || !(specRigidbody.gameActor is PlayerController))
		{
			return;
		}
		PlayerController playerController = specRigidbody.gameActor as PlayerController;
		if (playerController.IsDodgeRolling)
		{
			m_transitionTime = 0f;
			return;
		}
		m_transitionTime += BraveTime.DeltaTime;
		if (m_transitionTime > 0.5f)
		{
			Pixelator.Instance.FadeToBlack(0.5f);
			playerController.CurrentInputState = PlayerInputState.NoInput;
			specRigidbody.Velocity.x = 0f;
			if (m_cryoBool != null && m_cryoBool.Value)
			{
				GameUIRoot.Instance.HideCoreUI(string.Empty);
				GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
				AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
				GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.CATHEDRALGEON);
				float delay = 0.6f;
				GameManager.Instance.DelayedLoadCharacterSelect(delay, true, true);
			}
			else
			{
				GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.CATHEDRALGEON);
				GameManager.Instance.DelayedLoadCustomLevel(0.5f, "tt_cathedral");
			}
		}
	}

	private void LateUpdate()
	{
		if (m_transitionTime == m_previousTransitionTime)
		{
			m_transitionTime = 0f;
		}
		m_previousTransitionTime = m_transitionTime;
	}

	private IEnumerator Open()
	{
		m_isOpen = true;
		CrestSprite.renderer.enabled = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i] || GameManager.Instance.AllPlayers[i].passiveItems == null)
			{
				continue;
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers[i].passiveItems.Count; j++)
			{
				if (GameManager.Instance.AllPlayers[i].passiveItems[j] is CathedralCrestItem)
				{
					GameManager.Instance.AllPlayers[i].RemovePassiveItem(GameManager.Instance.AllPlayers[i].passiveItems[j].PickupObjectId);
					break;
				}
			}
		}
		float elapsed = 0f;
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(SlideShake, this);
		tk2dSpriteAnimator vfxChild = SarcoRigidbody.GetComponentInChildren<tk2dSpriteAnimator>();
		vfxChild.renderer.enabled = true;
		vfxChild.PlayAndDisableObject(string.Empty);
		while (elapsed < 4f)
		{
			elapsed += BraveTime.DeltaTime;
			SarcoRigidbody.Velocity = new Vector2(0f, -0.5f);
			yield return null;
		}
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		SarcoRigidbody.Velocity = Vector2.zero;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = AltarRigidbody.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + AltarRigidbody.sprite.transform.position, bounds.max + AltarRigidbody.sprite.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && !m_isOpen)
		{
			SpriteOutlineManager.AddOutlineToSprite(AltarRigidbody.sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this && !m_isOpen)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(AltarRigidbody.sprite);
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_isOpen)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(AltarRigidbody.sprite);
			GameManager.Instance.Dungeon.StartCoroutine(HandleShrineConversation(interactor));
		}
	}

	private IEnumerator HandleShrineConversation(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(text: StringTableManager.GetString(displayTextKey), worldPosition: talkPoint.position, parent: talkPoint, duration: -1f);
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		yield return null;
		bool canUse = interactor.healthHaver.HasCrest;
		if (canUse)
		{
			string string2 = StringTableManager.GetString(acceptOptionKey);
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, string2, StringTableManager.GetString(declineOptionKey));
		}
		else
		{
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(declineOptionKey), string.Empty);
		}
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
		if (canUse && selectedResponse == 0)
		{
			GameManager.Instance.Dungeon.StartCoroutine(Open());
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}
}
