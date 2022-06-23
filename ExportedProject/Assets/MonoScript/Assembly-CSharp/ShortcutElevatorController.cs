using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

public class ShortcutElevatorController : BraveBehaviour
{
	public ShortcutDefinition[] definedShortcuts;

	public tk2dSprite elevatorFloorSprite;

	public PressurePlate RotateLeftPlate;

	public PressurePlate RotateRightPlate;

	public SpeculativeRigidbody PlayerBlocker;

	public SpeculativeRigidbody BossRushTriggerZone;

	public tk2dSpriteAnimator RotatorBase;

	public tk2dSpriteAnimator RotatorShells;

	public tk2dSpriteAnimator Elevator;

	public MeshRenderer ElevatorFloor;

	[NonSerialized]
	private List<ShortcutDefinition> availableShortcuts = new List<ShortcutDefinition>();

	[NonSerialized]
	private int m_selectedShortcutIndex;

	private bool m_isDeparting;

	private bool m_isRotating;

	private bool m_bossRushValid;

	private bool m_queuedRotationRight;

	private bool m_queuedRotationLeft;

	private void Start()
	{
		DetermineAvailableShortcuts();
		if (availableShortcuts.Count > 0)
		{
			PressurePlate rotateLeftPlate = RotateLeftPlate;
			rotateLeftPlate.OnPressurePlateDepressed = (Action<PressurePlate>)Delegate.Combine(rotateLeftPlate.OnPressurePlateDepressed, new Action<PressurePlate>(RotateLeft));
			PressurePlate rotateRightPlate = RotateRightPlate;
			rotateRightPlate.OnPressurePlateDepressed = (Action<PressurePlate>)Delegate.Combine(rotateRightPlate.OnPressurePlateDepressed, new Action<PressurePlate>(RotateRight));
			StartCoroutine(RotateRight("bullet_elevator_turn", "bullet_elevator_bullet_turn", 1, GetCachedAvailableShortcut()));
			SpeculativeRigidbody component = ElevatorFloor.GetComponent<SpeculativeRigidbody>();
			component.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnElevatorTriggerEnter));
			if (BossRushTriggerZone != null)
			{
				SpeculativeRigidbody bossRushTriggerZone = BossRushTriggerZone;
				bossRushTriggerZone.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(bossRushTriggerZone.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleRushTriggerEntered));
			}
		}
	}

	private void HandleRushTriggerEntered(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!availableShortcuts[m_selectedShortcutIndex].IsBossRush || m_bossRushValid)
		{
			return;
		}
		for (int i = 0; i < StaticReferenceManager.AllNpcs.Count; i++)
		{
			TalkDoerLite talkDoerLite = StaticReferenceManager.AllNpcs[i];
			if ((bool)talkDoerLite && !Foyer.DoMainMenu && !GameManager.Instance.IsSelectingCharacter && GameManager.Instance.PrimaryPlayer.CurrentRoom == talkDoerLite.ParentRoom)
			{
				talkDoerLite.SendPlaymakerEvent("announceBossRush");
			}
		}
	}

	public void SetBossRushPaymentValid()
	{
		m_bossRushValid = true;
	}

	private int GetCachedAvailableShortcut()
	{
		string lastUsedShortcutTarget = GameManager.Options.lastUsedShortcutTarget;
		int result = -1;
		for (int i = 0; i < availableShortcuts.Count; i++)
		{
			if (availableShortcuts[i].targetLevelName == lastUsedShortcutTarget)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private bool CheckPlayerPositions()
	{
		if (!GameManager.Instance.IsSelectingCharacter)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].CurrentRoom == base.transform.position.GetAbsoluteRoom() && GameManager.Instance.AllPlayers[i].transform.position.y > RotatorBase.Sprite.WorldBottomCenter.y)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void RotateRight(PressurePlate obj)
	{
		bool flag = CheckPlayerPositions();
		if (!m_isRotating && flag)
		{
			StartCoroutine(RotateRight("bullet_elevator_turn", "bullet_elevator_bullet_turn", 1));
		}
		else if (flag)
		{
			m_queuedRotationLeft = false;
			m_queuedRotationRight = true;
		}
	}

	private void RotateLeft(PressurePlate obj)
	{
		bool flag = CheckPlayerPositions();
		if (!m_isRotating && flag)
		{
			StartCoroutine(RotateRight("bullet_elevator_turn_reverse", "bullet_elevator_bullet_turn_reverse", -1));
		}
		else if (flag)
		{
			m_queuedRotationLeft = true;
			m_queuedRotationRight = false;
		}
	}

	private void OnElevatorTriggerEnter(SpeculativeRigidbody otherSpecRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if ((availableShortcuts[m_selectedShortcutIndex].IsBossRush && !m_bossRushValid) || (availableShortcuts[m_selectedShortcutIndex].IsSuperBossRush && !m_bossRushValid) || !sourceSpecRigidbody.renderer.enabled || m_isDeparting || !(otherSpecRigidbody.GetComponent<PlayerController>() != null))
		{
			return;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			bool flag = true;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (!GameManager.Instance.AllPlayers[i].healthHaver.IsDead && !sourceSpecRigidbody.ContainsPoint(GameManager.Instance.AllPlayers[i].SpriteBottomCenter.XY(), int.MaxValue, true))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				StartCoroutine(TriggerShortcut());
			}
		}
		else
		{
			StartCoroutine(TriggerShortcut());
		}
	}

	private void DetermineAvailableShortcuts()
	{
		availableShortcuts.Clear();
		for (int i = 0; i < definedShortcuts.Length; i++)
		{
			if (definedShortcuts[i].requiredFlag == GungeonFlags.NONE || GameStatsManager.Instance.GetFlag(definedShortcuts[i].requiredFlag))
			{
				availableShortcuts.Add(definedShortcuts[i]);
			}
		}
		if (availableShortcuts.Count == 0)
		{
			m_selectedShortcutIndex = -1;
		}
		else
		{
			m_selectedShortcutIndex = 0;
		}
	}

	protected void SetSherpaText(string key)
	{
		for (int i = 0; i < StaticReferenceManager.AllNpcs.Count; i++)
		{
			TalkDoerLite talkDoerLite = StaticReferenceManager.AllNpcs[i];
			if (!talkDoerLite || Foyer.DoMainMenu || GameManager.Instance.IsSelectingCharacter || GameManager.Instance.PrimaryPlayer.CurrentRoom != talkDoerLite.ParentRoom)
			{
				continue;
			}
			for (int j = 0; j < talkDoerLite.playmakerFsms.Length; j++)
			{
				FsmString fsmString = talkDoerLite.playmakerFsms[j].FsmVariables.FindFsmString("CurrentShortcutText");
				if (fsmString != null)
				{
					fsmString.Value = key;
				}
			}
			talkDoerLite.SendPlaymakerEvent("rotatoPotato");
		}
	}

	public void UpdateFloorSprite(ShortcutDefinition currentDef)
	{
		if ((bool)elevatorFloorSprite && !string.IsNullOrEmpty(currentDef.elevatorFloorSpriteName))
		{
			elevatorFloorSprite.SetSprite(currentDef.elevatorFloorSpriteName);
		}
		else
		{
			elevatorFloorSprite.SetSprite("elevator_bottom_001");
		}
	}

	public IEnumerator RotateRight(string rotateAnim, string rotateAnimShells, int change, int specificAvailableToUse = -1)
	{
		if (availableShortcuts.Count == 0)
		{
			yield break;
		}
		m_isRotating = true;
		PlayerBlocker.enabled = true;
		Elevator.Play("bullet_elevator_no_thanks");
		float elapsed2 = 0f;
		while (Elevator.IsPlaying(Elevator.CurrentClip))
		{
			elapsed2 += BraveTime.DeltaTime;
			if (elapsed2 > 0.5f)
			{
				ElevatorFloor.enabled = false;
			}
			yield return null;
		}
		Elevator.sprite.renderer.enabled = false;
		m_selectedShortcutIndex = (m_selectedShortcutIndex + availableShortcuts.Count + change) % availableShortcuts.Count;
		if (specificAvailableToUse != -1 && specificAvailableToUse >= 0 && specificAvailableToUse < availableShortcuts.Count)
		{
			m_selectedShortcutIndex = specificAvailableToUse;
		}
		ShortcutDefinition currentDef = availableShortcuts[m_selectedShortcutIndex];
		GameManager.Options.lastUsedShortcutTarget = currentDef.targetLevelName;
		RotatorBase.Play(rotateAnim);
		RotatorShells.Play(rotateAnimShells);
		while (RotatorBase.IsPlaying(RotatorBase.CurrentClip))
		{
			yield return null;
		}
		UpdateFloorSprite(currentDef);
		SetSherpaText(currentDef.sherpaTextKey);
		Elevator.sprite.renderer.enabled = true;
		Elevator.Play("bullet_elevator_arrive");
		elapsed2 = 0f;
		while (Elevator.IsPlaying(Elevator.CurrentClip))
		{
			elapsed2 += BraveTime.DeltaTime;
			if (elapsed2 > 0.5f)
			{
				ElevatorFloor.enabled = true;
			}
			yield return null;
		}
		PlayerBlocker.enabled = false;
		m_isRotating = false;
		if (m_queuedRotationLeft)
		{
			StartCoroutine(RotateRight("bullet_elevator_turn_reverse", "bullet_elevator_bullet_turn_reverse", -1));
			m_queuedRotationLeft = false;
		}
		else if (m_queuedRotationRight)
		{
			StartCoroutine(RotateRight("bullet_elevator_turn", "bullet_elevator_bullet_turn", 1));
			m_queuedRotationRight = false;
		}
	}

	public IEnumerator TriggerShortcut()
	{
		m_isDeparting = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i].healthHaver.IsDead)
			{
				GameManager.Instance.AllPlayers[i].CurrentInputState = PlayerInputState.NoInput;
			}
		}
		for (int j = 0; j < availableShortcuts.Count; j++)
		{
			ShortcutDefinition shortcutDefinition = availableShortcuts[j];
			GameStatsManager.Instance.SetFlag(shortcutDefinition.requiredFlag, true);
		}
		RotatorShells.sprite.SetSprite("elevator_chamber_bullet_empty_idle_001");
		Elevator.Play("bullet_elevator_depart");
		float elapsed = 0f;
		bool postprocessed = false;
		while (Elevator.IsPlaying(Elevator.CurrentClip))
		{
			elapsed += BraveTime.DeltaTime;
			if (elapsed > 0.5f && !postprocessed)
			{
				postprocessed = true;
				ElevatorFloor.enabled = false;
				Foyer.Instance.OnDepartedFoyer();
				for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
				{
					GameManager.Instance.AllPlayers[k].PrepareForSceneTransition();
				}
			}
			yield return null;
		}
		if (availableShortcuts[m_selectedShortcutIndex].IsBossRush)
		{
			GameManager.Instance.CurrentGameMode = GameManager.GameMode.BOSSRUSH;
			Pixelator.Instance.FadeToBlack(0.5f);
			GameManager.Instance.SetNextLevelIndex(1);
			GameManager.Instance.DelayedLoadBossrushFloor(0.5f);
			yield break;
		}
		if (availableShortcuts[m_selectedShortcutIndex].IsSuperBossRush)
		{
			GameManager.Instance.CurrentGameMode = GameManager.GameMode.SUPERBOSSRUSH;
			Pixelator.Instance.FadeToBlack(0.5f);
			GameManager.Instance.SetNextLevelIndex(1);
			GameManager.Instance.DelayedLoadBossrushFloor(0.5f);
			yield break;
		}
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.NUMBER_ATTEMPTS, 1f);
		GameManager.Instance.CurrentGameMode = GameManager.GameMode.SHORTCUT;
		Pixelator.Instance.FadeToBlack(0.5f);
		switch (availableShortcuts[m_selectedShortcutIndex].requiredFlag)
		{
		case GungeonFlags.SHERPA_UNLOCK1_COMPLETE:
			GameManager.Instance.LastShortcutFloorLoaded = 1;
			break;
		case GungeonFlags.SHERPA_UNLOCK2_COMPLETE:
			GameManager.Instance.LastShortcutFloorLoaded = 2;
			break;
		case GungeonFlags.SHERPA_UNLOCK3_COMPLETE:
			GameManager.Instance.LastShortcutFloorLoaded = 3;
			break;
		case GungeonFlags.SHERPA_UNLOCK4_COMPLETE:
			GameManager.Instance.LastShortcutFloorLoaded = 4;
			break;
		default:
			GameManager.Instance.LastShortcutFloorLoaded = 1;
			break;
		}
		GameManager.Instance.IsLoadingFirstShortcutFloor = true;
		GameManager.Instance.DelayedLoadCustomLevel(0.5f, availableShortcuts[m_selectedShortcutIndex].targetLevelName);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
