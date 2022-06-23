using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;

public class ArtfulDodgerRoomController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	[DwarfConfigurable]
	public float NumberShots = 3f;

	[DwarfConfigurable]
	public float NumberBounces = 1f;

	private Fsm m_fsm;

	private bool m_hasActivated;

	private bool m_rewardHandled;

	private List<ArtfulDodgerTargetController> m_targets = new List<ArtfulDodgerTargetController>();

	private List<ArtfulDodgerCameraManipulator> m_cameraZones = new List<ArtfulDodgerCameraManipulator>();

	[NonSerialized]
	public PlayerController gamePlayingPlayer;

	public bool Completed
	{
		get
		{
			return m_rewardHandled;
		}
	}

	public void RegisterTarget(ArtfulDodgerTargetController target)
	{
		m_targets.Add(target);
	}

	public void RegisterCameraZone(ArtfulDodgerCameraManipulator zone)
	{
		m_cameraZones.Add(zone);
	}

	public void Activate(Fsm sourceFsm)
	{
		m_hasActivated = true;
		m_fsm = sourceFsm;
		for (int i = 0; i < m_cameraZones.Count; i++)
		{
			m_cameraZones[i].Active = true;
		}
		for (int j = 0; j < m_targets.Count; j++)
		{
			m_targets[j].Activate();
		}
		GameManager.Instance.DungeonMusicController.StartArcadeGame();
	}

	public void DoHandleReward()
	{
		GameManager.Instance.DungeonMusicController.SwitchToArcadeMusic();
		StartCoroutine(HandleReward());
	}

	private void DoConfetti(Vector2 targetCenter)
	{
		string[] array = new string[3] { "Global VFX/Confetti_Blue_001", "Global VFX/Confetti_Yellow_001", "Global VFX/Confetti_Green_001" };
		for (int i = 0; i < 8; i++)
		{
			GameObject original = (GameObject)BraveResources.Load(array[UnityEngine.Random.Range(0, 3)]);
			WaftingDebrisObject component = UnityEngine.Object.Instantiate(original).GetComponent<WaftingDebrisObject>();
			component.sprite.PlaceAtPositionByAnchor(targetCenter.ToVector3ZUp() + new Vector3(0.5f, 0.5f, 0f), tk2dBaseSprite.Anchor.MiddleCenter);
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			insideUnitCircle.y = 0f - Mathf.Abs(insideUnitCircle.y);
			component.Trigger(insideUnitCircle.ToVector3ZUp(1.5f) * UnityEngine.Random.Range(0.5f, 2f), 0.5f, 0f);
		}
	}

	public IEnumerator HandleReward()
	{
		if (m_rewardHandled)
		{
			yield break;
		}
		if (GameManager.Instance.BestActivePlayer.CurrentRoom != GetAbsoluteParentRoom())
		{
			m_fsm.Variables.GetFsmBool("SilentEnd").Value = true;
		}
		m_rewardHandled = true;
		int numBroken = 0;
		for (int i = 0; i < m_targets.Count; i++)
		{
			if (m_targets[i].IsBroken)
			{
				numBroken++;
			}
		}
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.WINCHESTER_GAMES_PLAYED, 1f);
		if (numBroken == m_targets.Count)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.WINCHESTER_GAMES_ACED, 1f);
			GameStatsManager.Instance.SetFlag(GungeonFlags.WINCHESTER_ACED_ONCE, true);
		}
		if (numBroken > 0)
		{
			yield return new WaitForSeconds(0.25f);
			m_fsm.Variables.FindFsmString("VictoryTextKey").Value = "#DODGER_VICTORY_01";
			m_fsm.Variables.FindFsmString("VictoryAnim").Value = "clap";
			if (numBroken == m_targets.Count)
			{
				m_fsm.Variables.FindFsmString("VictoryTextKey").Value = "#DODGER_GREAT_VICTORY_01";
				m_fsm.Variables.FindFsmString("VictoryAnim").Value = "bow";
			}
			else if (numBroken == m_targets.Count - 1)
			{
				m_fsm.Variables.FindFsmString("VictoryTextKey").Value = "#DODGER_GOOD_VICTORY_01";
				m_fsm.Variables.FindFsmString("VictoryAnim").Value = "clap";
			}
			if ((bool)gamePlayingPlayer && m_fsm != null && (bool)m_fsm.Owner)
			{
				TalkDoerLite component = m_fsm.Owner.GetComponent<TalkDoerLite>();
				component.TalkingPlayer = gamePlayingPlayer;
			}
			GameManager.BroadcastRoomFsmEvent("ArtfulDodgerSuccess", GetAbsoluteParentRoom());
			while (!m_fsm.Variables.GetFsmBool("ShouldSpawnChest").Value)
			{
				yield return null;
			}
			IntVector2 pos = GetAbsoluteParentRoom().GetBestRewardLocation(new IntVector2(2, 1), RoomHandler.RewardLocationStyle.Original);
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
			Chest chestPrefab = null;
			switch (tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.GUNGEON:
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				chestPrefab = ((numBroken != m_targets.Count) ? ((numBroken != m_targets.Count - 1) ? ((numBroken != m_targets.Count - 2) ? GameManager.Instance.RewardManager.D_Chest : GameManager.Instance.RewardManager.C_Chest) : GameManager.Instance.RewardManager.B_Chest) : GameManager.Instance.RewardManager.A_Chest);
				break;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				chestPrefab = ((numBroken != m_targets.Count) ? ((numBroken != m_targets.Count - 1) ? ((numBroken != m_targets.Count - 2) ? GameManager.Instance.RewardManager.C_Chest : GameManager.Instance.RewardManager.B_Chest) : GameManager.Instance.RewardManager.A_Chest) : GameManager.Instance.RewardManager.S_Chest);
				break;
			}
			if (chestPrefab == null)
			{
				chestPrefab = ((numBroken == m_targets.Count) ? GameManager.Instance.RewardManager.A_Chest : ((numBroken == m_targets.Count - 1) ? GameManager.Instance.RewardManager.B_Chest : ((numBroken != m_targets.Count - 2) ? GameManager.Instance.RewardManager.D_Chest : GameManager.Instance.RewardManager.C_Chest)));
			}
			Chest c = Chest.Spawn(chestPrefab, pos, GetAbsoluteParentRoom(), true);
			AkSoundEngine.PostEvent("Play_OBJ_prize_won_01", c.gameObject);
			DoConfetti(c.sprite.WorldCenter);
			c.ForceUnlock();
			if (c != null)
			{
				c.RegisterChestOnMinimap(GetAbsoluteParentRoom());
			}
		}
		else
		{
			m_fsm.Variables.FindFsmString("VictoryTextKey").Value = "#DODGER_FAILURE_01";
			m_fsm.Variables.FindFsmString("VictoryTextKey2").Value = "#DODGER_FAILURE_02";
			m_fsm.Variables.FindFsmString("VictoryAnim").Value = "laugh";
			yield return new WaitForSeconds(0.25f);
			if ((bool)gamePlayingPlayer && m_fsm != null && (bool)m_fsm.Owner)
			{
				TalkDoerLite component2 = m_fsm.Owner.GetComponent<TalkDoerLite>();
				component2.TalkingPlayer = gamePlayingPlayer;
			}
			GameManager.BroadcastRoomFsmEvent("ArtfulDodgerFailure", GetAbsoluteParentRoom());
		}
		for (int j = 0; j < m_targets.Count; j++)
		{
			m_targets[j].DisappearSadly();
		}
		for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
		{
			if (!GameManager.Instance.AllPlayers[k] || !GameManager.Instance.AllPlayers[k].healthHaver.IsAlive)
			{
				continue;
			}
			for (int l = 0; l < GameManager.Instance.AllPlayers[k].inventory.AllGuns.Count; l++)
			{
				if (GameManager.Instance.AllPlayers[k].inventory.AllGuns[l].name.StartsWith("ArtfulDodger"))
				{
					GameManager.Instance.AllPlayers[k].inventory.DestroyGun(GameManager.Instance.AllPlayers[k].inventory.AllGuns[l]);
				}
			}
		}
	}

	public void LateUpdate()
	{
		if (!m_hasActivated || m_rewardHandled)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < m_targets.Count; i++)
		{
			if (!m_targets[i].IsBroken)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			StartCoroutine(HandleReward());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		if (room.RoomVisualSubtype >= 0 && room.RoomVisualSubtype < GameManager.Instance.BestGenerationDungeonPrefab.roomMaterialDefinitions.Length)
		{
			DungeonMaterial dungeonMaterial = GameManager.Instance.BestGenerationDungeonPrefab.roomMaterialDefinitions[room.RoomVisualSubtype];
			if (!dungeonMaterial.supportsPits)
			{
				room.RoomVisualSubtype = 0;
			}
		}
		room.IsWinchesterArcadeRoom = true;
		room.Entered += HandleArcadeMusicEvents;
		if (room.connectedRooms.Count == 1)
		{
			room.ShouldAttemptProceduralLock = true;
			room.AttemptProceduralLockChance = Mathf.Max(room.AttemptProceduralLockChance, UnityEngine.Random.Range(0.3f, 0.5f));
		}
	}

	private void HandleArcadeMusicEvents(PlayerController p)
	{
		GameManager.Instance.DungeonMusicController.SwitchToArcadeMusic();
	}
}
