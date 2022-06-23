using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dungeonator;
using UnityEngine;

public class ResourcefulRatMazeSystemController : DungeonPlaceableBehaviour
{
	private List<RoomHandler> m_centralRoomSeries;

	private bool m_playerHasLeftEntrance;

	private DungeonData.Direction[] m_currentSolution;

	private int m_currentTargetDirectionIndex;

	private int m_currentLivingRoomIndex;

	private int m_sequentialErrors;

	private int m_errors;

	private bool m_mazeIsActive;

	private bool m_isInitialized;

	public IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		base.transform.parent = null;
		Initialize();
	}

	private void ResetMaze()
	{
		Debug.LogError("resetting the maze!!!!");
		m_currentTargetDirectionIndex = 0;
		m_currentLivingRoomIndex = 0;
		m_sequentialErrors = 0;
		m_errors = 0;
		m_playerHasLeftEntrance = false;
		m_mazeIsActive = true;
	}

	public void Initialize()
	{
		if (m_isInitialized)
		{
			return;
		}
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		m_centralRoomSeries = new List<RoomHandler>();
		m_centralRoomSeries.Add(absoluteRoom);
		absoluteRoom.OverrideVisibility = RoomHandler.VisibilityStatus.CURRENT;
		Pixelator.Instance.ProcessOcclusionChange(IntVector2.Zero, 1f, absoluteRoom, false);
		m_currentSolution = GameManager.GetResourcefulRatSolution();
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_06))
		{
			StringBuilder stringBuilder = new StringBuilder("Rat Solution: ");
			DungeonData.Direction[] currentSolution = m_currentSolution;
			foreach (DungeonData.Direction direction in currentSolution)
			{
				stringBuilder.Append(direction).Append(" ");
			}
			Debug.LogError(stringBuilder);
		}
		PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
		for (int j = 0; j < absoluteRoom.connectedRooms.Count; j++)
		{
			absoluteRoom.connectedRooms[j].PreventRevealEver = true;
		}
		for (int k = 0; k < GameManager.Instance.Dungeon.data.rooms.Count; k++)
		{
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[k];
			if (roomHandler.connectedRooms.Count == 1 && roomHandler.connectedRooms[0] == GameManager.Instance.Dungeon.data.Entrance)
			{
				roomHandler.TargetPitfallRoom = absoluteRoom;
				roomHandler.ForcePitfallForFliers = true;
			}
			if (roomHandler.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.NPC_STORY && roomHandler != absoluteRoom && !m_centralRoomSeries.Contains(roomHandler))
			{
				m_centralRoomSeries.Add(roomHandler);
				for (int l = 0; l < roomHandler.connectedRooms.Count; l++)
				{
					roomHandler.connectedRooms[l].PreventRevealEver = true;
				}
			}
		}
		m_mazeIsActive = true;
		m_isInitialized = true;
	}

	private void Update()
	{
		if (Dungeon.IsGenerating || !GameManager.HasInstance || GameManager.Instance.Dungeon == null)
		{
			return;
		}
		if (!m_isInitialized)
		{
			Initialize();
		}
		PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
		if (m_playerHasLeftEntrance && bestActivePlayer != null && bestActivePlayer.CurrentRoom == GameManager.Instance.Dungeon.data.Entrance)
		{
			ResetMaze();
		}
		else
		{
			if (!bestActivePlayer || bestActivePlayer.IsGhost || ((bool)bestActivePlayer && (bool)bestActivePlayer.healthHaver && bestActivePlayer.healthHaver.IsDead) || !m_mazeIsActive)
			{
				return;
			}
			if (!m_playerHasLeftEntrance)
			{
				if ((bool)bestActivePlayer && bestActivePlayer.CurrentRoom != null && bestActivePlayer.CurrentRoom == m_centralRoomSeries[0])
				{
					m_playerHasLeftEntrance = true;
				}
				return;
			}
			PlayerController playerController = bestActivePlayer;
			RoomHandler roomHandler = m_centralRoomSeries[m_currentLivingRoomIndex];
			DungeonData.Direction directionFromVector = DungeonData.GetDirectionFromVector2(BraveUtility.GetMajorAxis(playerController.CenterPosition - roomHandler.Epicenter.ToVector2()));
			if (playerController.CurrentRoom == roomHandler || playerController.InExitCell)
			{
				return;
			}
			if (m_errors < 2 && directionFromVector == m_currentSolution[m_currentTargetDirectionIndex])
			{
				if (m_currentTargetDirectionIndex == 5)
				{
					HandleVictory(playerController);
					m_mazeIsActive = false;
					return;
				}
				int newLivingRoomIndex = 0;
				if (m_currentLivingRoomIndex < 5)
				{
					newLivingRoomIndex = m_currentLivingRoomIndex + 1;
				}
				else if (m_currentLivingRoomIndex == 6)
				{
					newLivingRoomIndex = 1;
				}
				HandleContinuance(playerController, newLivingRoomIndex);
				m_currentTargetDirectionIndex++;
			}
			else if (m_errors >= 2)
			{
				HandleFailure(playerController);
				m_mazeIsActive = false;
			}
			else
			{
				HandleTemporaryFailure(playerController);
				m_errors++;
			}
		}
	}

	private void HandleVictory(PlayerController cp)
	{
		RoomHandler roomHandler = null;
		foreach (RoomHandler room in GameManager.Instance.Dungeon.data.rooms)
		{
			if (room.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				roomHandler = room;
				break;
			}
		}
		for (int i = 0; i < roomHandler.connectedRooms.Count; i++)
		{
			if (roomHandler.connectedRooms[i].distanceFromEntrance <= roomHandler.distanceFromEntrance)
			{
				roomHandler = roomHandler.connectedRooms[i];
				break;
			}
		}
		for (int j = 0; j < roomHandler.connectedRooms.Count; j++)
		{
			if (roomHandler.connectedRooms[j].distanceFromEntrance <= roomHandler.distanceFromEntrance)
			{
				roomHandler = roomHandler.connectedRooms[j];
				break;
			}
		}
		Vector2 vector = GameManager.Instance.MainCameraController.transform.position.XY() - cp.transform.position.XY();
		Vector2 vector2 = cp.transform.position.XY() - cp.CurrentRoom.area.basePosition.ToVector2();
		Vector2 targetPoint = roomHandler.area.basePosition.ToVector2() + vector2 + new Vector2(3f, 3f);
		cp.WarpToPointAndBringCoopPartner(targetPoint, false, true);
		cp.ForceChangeRoom(roomHandler);
		GameManager.Instance.MainCameraController.transform.position = (cp.transform.position.XY() + vector).ToVector3ZUp(GameManager.Instance.MainCameraController.transform.position.z);
	}

	private void HandleContinuance(PlayerController cp, int newLivingRoomIndex)
	{
		int currentLivingRoomIndex = m_currentLivingRoomIndex;
		m_currentLivingRoomIndex = newLivingRoomIndex;
		Vector2 vector = GameManager.Instance.MainCameraController.transform.position.XY() - cp.transform.position.XY();
		Vector2 vector2 = cp.transform.position.XY() - cp.CurrentRoom.area.basePosition.ToVector2();
		Vector2 targetPoint = m_centralRoomSeries[m_currentLivingRoomIndex].area.basePosition.ToVector2() + vector2;
		cp.WarpToPointAndBringCoopPartner(targetPoint, false, true);
		cp.ForceChangeRoom(m_centralRoomSeries[m_currentLivingRoomIndex]);
		HandleNearestExitOcclusion(cp);
		GameManager.Instance.MainCameraController.transform.position = (cp.transform.position.XY() + vector).ToVector3ZUp(GameManager.Instance.MainCameraController.transform.position.z);
	}

	private void HandleNearestExitOcclusion(PlayerController cp)
	{
		RuntimeExitDefinition runtimeExitDefinition = null;
		float num = float.MaxValue;
		for (int i = 0; i < cp.CurrentRoom.area.instanceUsedExits.Count; i++)
		{
			RuntimeRoomExitData runtimeRoomExitData = cp.CurrentRoom.area.exitToLocalDataMap[cp.CurrentRoom.area.instanceUsedExits[i]];
			float magnitude = ((cp.CurrentRoom.area.basePosition + runtimeRoomExitData.ExitOrigin - IntVector2.One).ToCenterVector2() - cp.CenterPosition).magnitude;
			if (magnitude < num && cp.CurrentRoom.exitDefinitionsByExit.ContainsKey(runtimeRoomExitData))
			{
				num = magnitude;
				runtimeExitDefinition = cp.CurrentRoom.exitDefinitionsByExit[runtimeRoomExitData];
			}
		}
		if (runtimeExitDefinition == null)
		{
			return;
		}
		IntVector2 intVector = IntVector2.MaxValue;
		IntVector2 intVector2 = IntVector2.MinValue;
		DungeonData data = GameManager.Instance.Dungeon.data;
		foreach (IntVector2 item in runtimeExitDefinition.GetCellsForRoom(runtimeExitDefinition.downstreamRoom))
		{
			ProcessCell(data, item);
			intVector = IntVector2.Min(intVector, item);
			intVector2 = IntVector2.Max(intVector2, item + new IntVector2(0, 2));
		}
		foreach (IntVector2 item2 in runtimeExitDefinition.GetCellsForRoom(runtimeExitDefinition.upstreamRoom))
		{
			ProcessCell(data, item2);
			intVector = IntVector2.Min(intVector, item2);
			intVector2 = IntVector2.Max(intVector2, item2 + new IntVector2(0, 2));
		}
		foreach (IntVector2 intermediaryCell in runtimeExitDefinition.IntermediaryCells)
		{
			ProcessCell(data, intermediaryCell);
			intVector = IntVector2.Min(intVector, intermediaryCell);
			intVector2 = IntVector2.Max(intVector2, intermediaryCell + new IntVector2(0, 2));
		}
		Pixelator.Instance.ProcessModifiedRanges(intVector, intVector2);
		Pixelator.Instance.MarkOcclusionDirty();
	}

	private void ProcessCell(DungeonData data, IntVector2 pos)
	{
		CellData cellData = data[pos];
		if (cellData != null)
		{
			cellData.occlusionData.cellRoomVisiblityCount = Mathf.RoundToInt(Mathf.Clamp01(1f));
			cellData.occlusionData.cellRoomVisitedCount = Mathf.RoundToInt(Mathf.Clamp01(1f));
			cellData.occlusionData.cellVisibleTargetOcclusion = 0f;
			cellData.occlusionData.cellVisitedTargetOcclusion = 0.7f;
			cellData.occlusionData.overrideOcclusion = true;
			cellData.occlusionData.cellOcclusion = 0f;
			BraveUtility.DrawDebugSquare(pos.ToVector2(), Color.green, 1000f);
		}
		CellData cellData2 = data[cellData.position + IntVector2.Up];
		if (cellData2 != null)
		{
			cellData2.occlusionData.cellRoomVisiblityCount = Mathf.RoundToInt(Mathf.Clamp01(1f));
			cellData2.occlusionData.cellRoomVisitedCount = Mathf.RoundToInt(Mathf.Clamp01(1f));
			cellData2.occlusionData.cellVisibleTargetOcclusion = 0f;
			cellData2.occlusionData.cellVisitedTargetOcclusion = 0.7f;
			cellData2.occlusionData.overrideOcclusion = true;
			cellData2.occlusionData.cellOcclusion = 0f;
		}
		CellData cellData3 = data[cellData.position + IntVector2.Up * 2];
		if (cellData3 != null)
		{
			cellData3.occlusionData.cellRoomVisiblityCount = Mathf.RoundToInt(Mathf.Clamp01(1f));
			cellData3.occlusionData.cellRoomVisitedCount = Mathf.RoundToInt(Mathf.Clamp01(1f));
			cellData3.occlusionData.cellVisibleTargetOcclusion = 0f;
			cellData3.occlusionData.cellVisitedTargetOcclusion = 0.7f;
			cellData3.occlusionData.overrideOcclusion = true;
			cellData3.occlusionData.cellOcclusion = 0f;
		}
	}

	private void HandleTemporaryFailure(PlayerController cp)
	{
		m_currentTargetDirectionIndex = 0;
		HandleContinuance(cp, (m_errors != 0) ? 7 : 6);
	}

	private void HandleFailure(PlayerController cp)
	{
		RoomHandler roomHandler = null;
		foreach (RoomHandler room in GameManager.Instance.Dungeon.data.rooms)
		{
			if (room.area.PrototypeRoomName.Contains("FailRoom"))
			{
				roomHandler = room;
				break;
			}
		}
		Vector2 vector = GameManager.Instance.MainCameraController.transform.position.XY() - cp.transform.position.XY();
		Vector2 vector2 = cp.transform.position.XY() - cp.CurrentRoom.area.basePosition.ToVector2();
		Vector2 targetPoint = roomHandler.area.basePosition.ToVector2() + vector2;
		cp.WarpToPointAndBringCoopPartner(targetPoint, false, true);
		cp.ForceChangeRoom(roomHandler);
		HandleNearestExitOcclusion(cp);
		GameManager.Instance.MainCameraController.transform.position = (cp.transform.position.XY() + vector).ToVector3ZUp(GameManager.Instance.MainCameraController.transform.position.z);
	}
}
