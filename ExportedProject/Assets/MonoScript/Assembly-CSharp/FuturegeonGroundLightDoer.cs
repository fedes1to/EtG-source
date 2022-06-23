using System.Collections;
using Dungeonator;
using UnityEngine;

public class FuturegeonGroundLightDoer : MonoBehaviour
{
	public GameObject lightNorthVFX;

	public GameObject lightEastVFX;

	public GameObject lightSouthVFX;

	public GameObject lightWestVFX;

	public int maxActiveLightTrails = 5;

	private int numActiveLightTrails;

	private void Start()
	{
		StartCoroutine(HandleLightningTrails());
	}

	private IEnumerator HandleAllLightTrails()
	{
		while (true)
		{
			if (numActiveLightTrails < maxActiveLightTrails)
			{
				StartCoroutine(HandleSingleLightTrail());
			}
			yield return new WaitForSeconds(1.5f);
		}
	}

	private IEnumerator HandleLightningTrails()
	{
		while (true)
		{
			if (Dungeon.IsGenerating || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
			{
				yield return new WaitForSeconds(1f);
				continue;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if ((bool)playerController && !playerController.IsGhost)
				{
					RoomHandler currentRoom = playerController.CurrentRoom;
					if (currentRoom != null && (currentRoom.RoomVisualSubtype == 7 || currentRoom.RoomVisualSubtype == 8))
					{
						StartCoroutine(HandleLightingLightTrail(DungeonData.GetRandomCardinalDirection(), playerController.CenterPosition.ToIntVector2(VectorConversions.Floor)));
					}
				}
			}
			yield return new WaitForSeconds(Random.Range(0.5f, 1.25f));
		}
	}

	private IEnumerator HandleLightingLightTrail(DungeonData.Direction startDir, IntVector2 startPos, bool isBranch = false)
	{
		bool isAlive2 = true;
		IntVector2 currentCellPosition = startPos;
		DungeonData.Direction currentDirection = startDir;
		GameObject currentVFX2 = InstantiateVFXFromDirection(currentDirection);
		currentVFX2.transform.position = currentCellPosition.ToVector3((float)currentCellPosition.y + 1.5f);
		int numIterations = 0;
		int tilesSinceTurn = 0;
		float currentNextTimer = ((!isBranch) ? 0.25f : 0f);
		while (isAlive2)
		{
			if (currentNextTimer <= 0f)
			{
				tilesSinceTurn++;
				currentCellPosition += DungeonData.GetIntVector2FromDirection(currentDirection);
				CellData cellData = GameManager.Instance.Dungeon.data[currentCellPosition];
				if (cellData == null || cellData.type == CellType.WALL || cellData.type == CellType.PIT)
				{
					isAlive2 = false;
					break;
				}
				if (Random.value > 0.75f && tilesSinceTurn >= 2)
				{
					tilesSinceTurn = 0;
					switch (currentDirection)
					{
					case DungeonData.Direction.NORTH:
						if (Random.value > 0.95f && numIterations > 3)
						{
							StartCoroutine(HandleLightingLightTrail(DungeonData.Direction.WEST, currentCellPosition, true));
							currentDirection = DungeonData.Direction.EAST;
							currentCellPosition += IntVector2.Right;
						}
						else if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.EAST;
							currentCellPosition += IntVector2.Right;
						}
						else
						{
							currentDirection = DungeonData.Direction.WEST;
						}
						break;
					case DungeonData.Direction.EAST:
						if (Random.value > 0.95f && numIterations > 3)
						{
							StartCoroutine(HandleLightingLightTrail(DungeonData.Direction.SOUTH, currentCellPosition + IntVector2.Down + IntVector2.Left, true));
							currentDirection = DungeonData.Direction.NORTH;
							currentCellPosition += IntVector2.Left;
						}
						else if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.NORTH;
							currentCellPosition += IntVector2.Left;
						}
						else
						{
							currentDirection = DungeonData.Direction.SOUTH;
							currentCellPosition += IntVector2.Down + IntVector2.Left;
						}
						break;
					case DungeonData.Direction.SOUTH:
						if (Random.value > 0.95f && numIterations > 3)
						{
							StartCoroutine(HandleLightingLightTrail(DungeonData.Direction.WEST, currentCellPosition + IntVector2.Up, true));
							currentDirection = DungeonData.Direction.EAST;
							currentCellPosition += IntVector2.Right + IntVector2.Up;
						}
						else if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.EAST;
							currentCellPosition += IntVector2.Right + IntVector2.Up;
						}
						else
						{
							currentDirection = DungeonData.Direction.WEST;
							currentCellPosition += IntVector2.Up;
						}
						break;
					case DungeonData.Direction.WEST:
						if (Random.value > 0.95f && numIterations > 3)
						{
							StartCoroutine(HandleLightingLightTrail(DungeonData.Direction.SOUTH, currentCellPosition + IntVector2.Down, true));
							currentDirection = DungeonData.Direction.NORTH;
						}
						else if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.NORTH;
						}
						else
						{
							currentDirection = DungeonData.Direction.SOUTH;
							currentCellPosition += IntVector2.Down;
						}
						break;
					}
				}
				currentVFX2 = InstantiateVFXFromDirection(currentDirection);
				currentVFX2.transform.position = currentCellPosition.ToVector3((float)currentCellPosition.y + 1.5f);
				numIterations++;
			}
			else
			{
				currentNextTimer -= BraveTime.DeltaTime;
			}
			if (numIterations > 200)
			{
				isAlive2 = false;
			}
			yield return null;
		}
	}

	private GameObject InstantiateVFXFromDirection(DungeonData.Direction dir)
	{
		GameObject prefab = null;
		switch (dir)
		{
		case DungeonData.Direction.NORTH:
			prefab = lightNorthVFX;
			break;
		case DungeonData.Direction.EAST:
			prefab = lightEastVFX;
			break;
		case DungeonData.Direction.SOUTH:
			prefab = lightSouthVFX;
			break;
		case DungeonData.Direction.WEST:
			prefab = lightWestVFX;
			break;
		}
		return SpawnManager.SpawnVFX(prefab);
	}

	private IEnumerator HandleSingleLightTrail()
	{
		numActiveLightTrails++;
		bool isAlive2 = true;
		IntVector2 currentCellPosition = GameManager.Instance.PrimaryPlayer.CenterPosition.ToIntVector2(VectorConversions.Floor);
		DungeonData.Direction currentDirection = DungeonData.GetRandomCardinalDirection();
		GameObject currentVFX2 = InstantiateVFXFromDirection(currentDirection);
		currentVFX2.transform.position = currentCellPosition.ToVector3((float)currentCellPosition.y + 1.5f);
		int numIterations = 0;
		float currentNextTimer = 0.25f;
		while (isAlive2)
		{
			if (currentNextTimer <= 0f)
			{
				currentCellPosition += DungeonData.GetIntVector2FromDirection(currentDirection);
				CellData cellData = GameManager.Instance.Dungeon.data[currentCellPosition];
				if (cellData == null || cellData.type == CellType.WALL || cellData.type == CellType.PIT)
				{
					isAlive2 = false;
					break;
				}
				if (Random.value > 0.5f)
				{
					switch (currentDirection)
					{
					case DungeonData.Direction.NORTH:
						if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.EAST;
							currentCellPosition += IntVector2.Right;
						}
						else
						{
							currentDirection = DungeonData.Direction.WEST;
						}
						break;
					case DungeonData.Direction.EAST:
						if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.NORTH;
							currentCellPosition += IntVector2.Left;
						}
						else
						{
							currentDirection = DungeonData.Direction.SOUTH;
							currentCellPosition += IntVector2.Down + IntVector2.Left;
						}
						break;
					case DungeonData.Direction.SOUTH:
						if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.EAST;
							currentCellPosition += IntVector2.Right + IntVector2.Up;
						}
						else
						{
							currentDirection = DungeonData.Direction.WEST;
							currentCellPosition += IntVector2.Up;
						}
						break;
					case DungeonData.Direction.WEST:
						if (Random.value > 0.5f)
						{
							currentDirection = DungeonData.Direction.NORTH;
							break;
						}
						currentDirection = DungeonData.Direction.SOUTH;
						currentCellPosition += IntVector2.Down;
						break;
					}
				}
				currentVFX2 = InstantiateVFXFromDirection(currentDirection);
				currentVFX2.transform.position = currentCellPosition.ToVector3((float)currentCellPosition.y + 1.5f);
				numIterations++;
				currentNextTimer = 0.25f;
			}
			else
			{
				currentNextTimer -= BraveTime.DeltaTime;
			}
			if (numIterations > 20)
			{
				isAlive2 = false;
			}
			yield return null;
		}
		numActiveLightTrails--;
	}
}
