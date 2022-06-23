using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class ChallengeShrineController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public string displayTextKey;

	public string acceptOptionKey;

	public string declineOptionKey;

	public Transform talkPoint;

	public GameObject onPlayerVFX;

	public Vector3 playerVFXOffset = Vector3.zero;

	public bool usesCustomChestTable;

	public WeightedGameObjectCollection CustomChestTable;

	public tk2dBaseSprite AlternativeOutlineTarget;

	private int m_useCount;

	private RoomHandler m_parentRoom;

	private GameObject m_instanceMinimapIcon;

	private float m_noEnemySealTime;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_parentRoom = room;
		m_parentRoom.PreventStandardRoomReward = true;
		RegisterMinimapIcon();
	}

	private void Update()
	{
		if (!m_parentRoom.IsSealed || !GameManager.Instance.PrimaryPlayer || GameManager.Instance.PrimaryPlayer.CurrentRoom == null)
		{
			return;
		}
		if (GameManager.Instance.PrimaryPlayer.CurrentRoom != m_parentRoom)
		{
			m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
			m_parentRoom.UnsealRoom();
		}
		else if (!m_parentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			m_noEnemySealTime += BraveTime.DeltaTime;
			if (m_noEnemySealTime > 3f)
			{
				m_parentRoom.TriggerNextReinforcementLayer();
			}
			if (m_noEnemySealTime > 5f)
			{
				m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
				m_parentRoom.UnsealRoom();
			}
		}
		else
		{
			m_noEnemySealTime = 0f;
		}
	}

	public void RegisterMinimapIcon()
	{
		m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_parentRoom, (GameObject)BraveResources.Load("Global Prefabs/Minimap_Shrine_Icon"));
	}

	public void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_parentRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	private void DoShrineEffect(PlayerController player)
	{
		m_parentRoom.TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.SHRINE_WAVE_A);
		m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealAll;
		m_parentRoom.SealRoom();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(player).ReuniteWithOtherPlayer(player);
		}
		RoomHandler parentRoom = m_parentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom.OnEnemiesCleared, new Action(HandleEnemiesClearedA));
		if (onPlayerVFX != null)
		{
			player.PlayEffectOnActor(onPlayerVFX, playerVFXOffset);
		}
		GetRidOfMinimapIcon();
	}

	private void HandleEnemiesClearedA()
	{
		RoomHandler parentRoom = m_parentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(HandleEnemiesClearedA));
		RoomHandler parentRoom2 = m_parentRoom;
		parentRoom2.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom2.OnEnemiesCleared, new Action(HandleEnemiesClearedB));
		if (!m_parentRoom.TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.SHRINE_WAVE_B))
		{
			HandleFinalEnemiesCleared();
		}
	}

	private void HandleEnemiesClearedB()
	{
		RoomHandler parentRoom = m_parentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(HandleEnemiesClearedB));
		if (!m_parentRoom.TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.SHRINE_WAVE_C))
		{
			HandleFinalEnemiesCleared();
			return;
		}
		RoomHandler parentRoom2 = m_parentRoom;
		parentRoom2.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom2.OnEnemiesCleared, new Action(HandleFinalEnemiesCleared));
	}

	private void HandleFinalEnemiesCleared()
	{
		m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
		RoomHandler parentRoom = m_parentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(HandleFinalEnemiesCleared));
		Chest chest = GameManager.Instance.RewardManager.SpawnRewardChestAt(m_parentRoom.GetBestRewardLocation(new IntVector2(3, 2)));
		if ((bool)chest)
		{
			chest.ForceUnlock();
			chest.RegisterChestOnMinimap(m_parentRoom);
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.AddOutlineToSprite(AlternativeOutlineTarget ?? base.sprite, Color.white);
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(AlternativeOutlineTarget ?? base.sprite);
	}

	private IEnumerator HandleShrineConversation(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(talkPoint.position, talkPoint, -1f, StringTableManager.GetString(displayTextKey));
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		yield return null;
		GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(acceptOptionKey), StringTableManager.GetString(declineOptionKey));
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
		if (selectedResponse == 0)
		{
			DoShrineEffect(interactor);
			yield break;
		}
		m_useCount--;
		m_parentRoom.RegisterInteractable(this);
	}

	public void Interact(PlayerController interactor)
	{
		if (m_useCount <= 0)
		{
			m_useCount++;
			m_parentRoom.DeregisterInteractable(this);
			StartCoroutine(HandleShrineConversation(interactor));
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
