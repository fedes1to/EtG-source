using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class SecretRoomManager : MonoBehaviour
{
	public enum SecretRoomRevealStyle
	{
		Simple,
		ComplexPuzzle,
		ShootToBreak,
		FireplacePuzzle
	}

	public SecretRoomRevealStyle revealStyle = SecretRoomRevealStyle.ShootToBreak;

	public Renderer ceilingRenderer;

	public Renderer borderRenderer;

	public Renderer aoRenderer;

	public RoomHandler room;

	public List<SecretRoomDoorBeer> doorObjects = new List<SecretRoomDoorBeer>();

	private List<IntVector2> ceilingCells;

	private SimpleSecretRoomTrigger m_simpleTrigger;

	private bool m_isOpen;

	public bool IsOpen
	{
		get
		{
			return m_isOpen;
		}
		set
		{
			if (!m_isOpen && value)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.SECRET_ROOMS_FOUND, 1f);
			}
			m_isOpen = value;
		}
	}

	public bool OpenedByExplosion
	{
		get
		{
			return revealStyle != SecretRoomRevealStyle.ComplexPuzzle;
		}
	}

	public void InitializeCells(List<IntVector2> ceilingCellList)
	{
		ceilingCells = ceilingCellList;
	}

	public void InitializeForStyle()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			doorObjects[i].manager = this;
		}
		switch (revealStyle)
		{
		case SecretRoomRevealStyle.Simple:
			InitializeSimple();
			break;
		case SecretRoomRevealStyle.ComplexPuzzle:
			InitializeSecretRoomPuzzle();
			break;
		case SecretRoomRevealStyle.ShootToBreak:
			InitializeShootToBreak();
			break;
		}
		for (int j = 0; j < doorObjects.Count; j++)
		{
			doorObjects[j].GeneratePotentiallyNecessaryShards();
		}
		for (int k = 0; k < doorObjects.Count; k++)
		{
			doorObjects[k].exitDef.GenerateSecretRoomBlocker(GameManager.Instance.Dungeon.data, this, doorObjects[k], null);
		}
	}

	public void HandleDoorBrokenOpen(SecretRoomDoorBeer doorBroken)
	{
		ceilingRenderer.enabled = false;
		if (borderRenderer != null)
		{
			borderRenderer.enabled = false;
		}
		for (int i = 0; i < ceilingCells.Count; i++)
		{
			if (GameManager.Instance.Dungeon.data[ceilingCells[i]] != null)
			{
				GameManager.Instance.Dungeon.data[ceilingCells[i]].isSecretRoomCell = false;
			}
		}
		for (int j = 0; j < doorObjects.Count; j++)
		{
			foreach (IntVector2 item in doorObjects[j].exitDef.GetCellsForRoom(room))
			{
				GameManager.Instance.Dungeon.data[item].isSecretRoomCell = false;
			}
			foreach (IntVector2 item2 in doorObjects[j].exitDef.GetCellsForOtherRoom(room))
			{
				GameManager.Instance.Dungeon.data[item2].isSecretRoomCell = false;
			}
		}
		for (int k = 0; k < doorObjects.Count; k++)
		{
			if (doorObjects[k].subsidiaryBlocker != null)
			{
				doorObjects[k].subsidiaryBlocker.ToggleRenderers(true);
			}
		}
		room.visibility = RoomHandler.VisibilityStatus.VISITED;
		Minimap.Instance.RevealMinimapRoom(room, true, true, false);
		Pixelator.Instance.ProcessOcclusionChange(doorBroken.transform.position.IntXY(), 0.3f, room);
		for (int l = 0; l < doorObjects.Count; l++)
		{
			Pixelator.Instance.ProcessRoomAdditionalExits(doorObjects[l].exitDef.GetUpstreamBasePosition(), doorObjects[l].linkedRoom, false);
		}
		doorBroken.gameObject.SetActive(false);
		OnFinishedOpeningDoors();
		if (m_simpleTrigger != null)
		{
			m_simpleTrigger.Disable();
		}
	}

	protected void InitializeSimple()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			GameObject colliderObject = doorObjects[i].collider.colliderObject;
			IntVector2 intVector = colliderObject.transform.position.IntXY(VectorConversions.Floor);
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(doorObjects[i].collider.exitDirection);
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			RoomHandler roomHandler = ((cellData.parentRoom != null) ? cellData.parentRoom : cellData.nearestRoom);
			CellData nearestFaceOrSidewall = roomHandler.GetNearestFaceOrSidewall(intVector + intVector2FromDirection);
			GameObject gameObject = null;
			IntVector2 zero = IntVector2.Zero;
			bool flag = false;
			if (!nearestFaceOrSidewall.IsSideWallAdjacent())
			{
				gameObject = GameManager.Instance.Dungeon.SecretRoomSimpleTriggersFacewall[Random.Range(0, GameManager.Instance.Dungeon.SecretRoomSimpleTriggersFacewall.Count)];
				zero = IntVector2.Up;
			}
			else
			{
				gameObject = GameManager.Instance.Dungeon.SecretRoomSimpleTriggersSidewall[Random.Range(0, GameManager.Instance.Dungeon.SecretRoomSimpleTriggersSidewall.Count)];
				zero = IntVector2.Right + IntVector2.Up;
				if (GameManager.Instance.Dungeon.data[nearestFaceOrSidewall.position + IntVector2.Right].type == CellType.WALL)
				{
					flag = true;
				}
				else
				{
					zero += IntVector2.Left;
				}
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.transform.parent = roomHandler.hierarchyParent;
			if (flag)
			{
				gameObject2.GetComponent<tk2dSprite>().FlipX = true;
			}
			nearestFaceOrSidewall.cellVisualData.containsObjectSpaceStamp = true;
			nearestFaceOrSidewall.cellVisualData.containsWallSpaceStamp = true;
			GameManager.Instance.Dungeon.data[nearestFaceOrSidewall.position + IntVector2.Up].cellVisualData.containsObjectSpaceStamp = true;
			GameManager.Instance.Dungeon.data[nearestFaceOrSidewall.position + IntVector2.Up].cellVisualData.containsWallSpaceStamp = true;
			gameObject2.transform.position = (nearestFaceOrSidewall.position + zero).ToVector3();
			SimpleSecretRoomTrigger simpleSecretRoomTrigger = gameObject2.AddComponent<SimpleSecretRoomTrigger>();
			simpleSecretRoomTrigger.referencedSecretRoom = this;
			simpleSecretRoomTrigger.parentRoom = roomHandler;
			gameObject2.GetComponent<Renderer>().sortingLayerName = "Background";
			gameObject2.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
			gameObject2.GetComponent<tk2dSprite>().UpdateZDepth();
			simpleSecretRoomTrigger.Initialize();
			m_simpleTrigger = simpleSecretRoomTrigger;
		}
	}

	protected void InitializeFireplacePuzzle()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			doorObjects[i].InitializeFireplace();
		}
	}

	protected void InitializeSecretRoomPuzzle()
	{
		if (doorObjects.Count == 0)
		{
			return;
		}
		if (doorObjects.Count > 1)
		{
			Debug.LogError("Attempting to render a complex secret puzzle onto a multi-exit secret room. This is unsupported...");
			return;
		}
		GameObject colliderObject = doorObjects[0].collider.colliderObject;
		IntVector2 intVector = colliderObject.transform.position.IntXY(VectorConversions.Floor);
		IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(doorObjects[0].collider.exitDirection);
		CellData cellData = GameManager.Instance.Dungeon.data[intVector];
		RoomHandler roomHandler = ((cellData.parentRoom != null) ? cellData.parentRoom : cellData.nearestRoom);
		CellData nearestFloorFacewall = roomHandler.GetNearestFloorFacewall(intVector + intVector2FromDirection);
		if (nearestFloorFacewall == null)
		{
			Debug.LogError("failed complex puzzle generation due to lack of floor facewall.");
			return;
		}
		GameObject original = GameManager.Instance.Dungeon.SecretRoomComplexTriggers[Random.Range(0, GameManager.Instance.Dungeon.SecretRoomComplexTriggers.Count)].gameObject;
		IntVector2 up = IntVector2.Up;
		GameObject gameObject = Object.Instantiate(original);
		gameObject.transform.parent = roomHandler.hierarchyParent;
		nearestFloorFacewall.cellVisualData.containsObjectSpaceStamp = true;
		nearestFloorFacewall.cellVisualData.containsWallSpaceStamp = true;
		GameManager.Instance.Dungeon.data[nearestFloorFacewall.position + IntVector2.Up].cellVisualData.containsObjectSpaceStamp = true;
		GameManager.Instance.Dungeon.data[nearestFloorFacewall.position + IntVector2.Up].cellVisualData.containsWallSpaceStamp = true;
		gameObject.transform.position = (nearestFloorFacewall.position + up).ToVector3();
		ComplexSecretRoomTrigger component = gameObject.GetComponent<ComplexSecretRoomTrigger>();
		component.referencedSecretRoom = this;
		component.Initialize(roomHandler);
	}

	protected void InitializeShootToBreak()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			doorObjects[i].InitializeShootToBreak();
		}
	}

	public void DoSeal()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			if (doorObjects[i].subsidiaryBlocker != null)
			{
				doorObjects[i].subsidiaryBlocker.Seal();
			}
		}
	}

	public void DoUnseal()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			if (doorObjects[i].subsidiaryBlocker != null)
			{
				doorObjects[i].subsidiaryBlocker.Unseal();
			}
		}
	}

	public void OpenDoor()
	{
		AkSoundEngine.PostEvent("Play_UI_secret_reveal_01", base.gameObject);
		AkSoundEngine.PostEvent("Play_OBJ_secret_door_01", base.gameObject);
		IsOpen = true;
		ceilingRenderer.enabled = false;
		if (borderRenderer != null)
		{
			borderRenderer.enabled = false;
		}
		for (int i = 0; i < ceilingCells.Count; i++)
		{
			if (GameManager.Instance.Dungeon.data[ceilingCells[i]] != null)
			{
				GameManager.Instance.Dungeon.data[ceilingCells[i]].isSecretRoomCell = false;
			}
		}
		for (int j = 0; j < doorObjects.Count; j++)
		{
			if (doorObjects[j].subsidiaryBlocker != null)
			{
				doorObjects[j].subsidiaryBlocker.ToggleRenderers(true);
			}
		}
		Minimap.Instance.RevealMinimapRoom(room, true, true, false);
		if (doorObjects.Count > 0)
		{
			for (int k = 0; k < doorObjects.Count; k++)
			{
				StartCoroutine(HandleDoorOpening(IsOpen, doorObjects[k]));
			}
		}
	}

	private string GetFrameName(string name, DungeonData.Direction dir)
	{
		if (name.Contains("{0}"))
		{
			string arg;
			switch (dir)
			{
			case DungeonData.Direction.WEST:
				arg = "_left_top";
				break;
			case DungeonData.Direction.NORTH:
				arg = "_top_top";
				break;
			case DungeonData.Direction.EAST:
				arg = "_right_top";
				break;
			default:
				arg = string.Empty;
				break;
			}
			return string.Format(name, arg);
		}
		return name;
	}

	private GameObject SpawnVFXAtPoint(GameObject vfx, Vector3 position)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(vfx, position, Quaternion.identity);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.HeightOffGround = 0.25f;
		component.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.MiddleCenter);
		component.IsPerpendicular = false;
		component.UpdateZDepth();
		return gameObject;
	}

	private void DoSparkAtPoint(Vector3 position, List<Transform> refTransformList)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(GameManager.Instance.Dungeon.SecretRoomDoorSparkVFX);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.HeightOffGround = 3.5f;
		component.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.MiddleCenter);
		component.UpdateZDepth();
		refTransformList.Add(component.transform);
	}

	private void OnFinishedOpeningDoors()
	{
		for (int i = 0; i < doorObjects.Count; i++)
		{
			doorObjects[i].SetBreakable();
		}
		ShadowSystem.ForceRoomLightsUpdate(room, 0.1f);
		for (int j = 0; j < doorObjects.Count; j++)
		{
			ShadowSystem.ForceRoomLightsUpdate(doorObjects[j].linkedRoom, 0.1f);
		}
	}

	private IEnumerator HandleDoorOpening(bool openState, SecretRoomDoorBeer doorObject)
	{
		if (!openState)
		{
			doorObject.gameObject.SetActive(true);
		}
		if (doorObject.exitDef.GetDirectionFromRoom(room) == DungeonData.Direction.NORTH || doorObject.exitDef.GetDirectionFromRoom(room) == DungeonData.Direction.SOUTH)
		{
			doorObject.gameObject.layer = LayerMask.NameToLayer("BG_Nonsense");
		}
		float elapsed = 0f;
		float visibilityTrigger = 0f;
		float aoDisable = 2f;
		float duration = 3f;
		Transform target = doorObject.transform;
		Vector3 startPosition = target.position + ((!openState) ? Vector3.zero : new Vector3(0f, 0f, 1f));
		Vector3 endPosition = startPosition + ((!openState) ? new Vector3(0f, 2f, -2f) : new Vector3(0f, -2f, 2f));
		if (doorObjects[0].collider.exitDirection != 0 && doorObjects[0].collider.exitDirection != DungeonData.Direction.SOUTH)
		{
			startPosition += new Vector3(0f, -0.5f, -0.5f);
			endPosition += new Vector3(0f, -1f, -1f);
		}
		ScreenShakeSettings continuousShake = new ScreenShakeSettings(0.1f, 7f, 0.1f, 0f);
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(continuousShake, this);
		List<Transform> additionalTransformsToMove = new List<Transform>();
		List<GameObject> vfxToDestroy = new List<GameObject>();
		for (int i = 0; i < doorObjects.Count; i++)
		{
			SecretRoomExitData collider = doorObjects[i].collider;
			Vector3 vector = collider.colliderObject.GetComponent<SpeculativeRigidbody>().UnitBottomLeft.ToVector3ZUp();
			if (collider.exitDirection == DungeonData.Direction.NORTH || collider.exitDirection == DungeonData.Direction.SOUTH)
			{
				if (GameManager.Instance.Dungeon.SecretRoomDoorSparkVFX != null)
				{
					DoSparkAtPoint(vector + new Vector3(0f, 1.9f, -0.25f), additionalTransformsToMove);
					DoSparkAtPoint(vector + new Vector3(2f, 1.9f, -0.25f), additionalTransformsToMove);
				}
				if (GameManager.Instance.Dungeon.SecretRoomHorizontalPoofVFX != null)
				{
					float y = ((collider.exitDirection == DungeonData.Direction.NORTH) ? (-1) : 0);
					GameObject gameObject = SpawnVFXAtPoint(GameManager.Instance.Dungeon.SecretRoomHorizontalPoofVFX, vector + new Vector3(0f, y, 0f));
					tk2dSpriteAnimator component = gameObject.GetComponent<tk2dSpriteAnimator>();
					component.Play();
					vfxToDestroy.Add(gameObject);
				}
			}
			else if (GameManager.Instance.Dungeon.SecretRoomVerticalPoofVFX != null)
			{
				float x = ((collider.exitDirection != DungeonData.Direction.EAST) ? (-1) : (-1));
				GameObject gameObject2 = SpawnVFXAtPoint(GameManager.Instance.Dungeon.SecretRoomVerticalPoofVFX, vector + new Vector3(x, 0f, 0f));
				tk2dSpriteAnimator component2 = gameObject2.GetComponent<tk2dSpriteAnimator>();
				component2.Play();
				vfxToDestroy.Add(gameObject2);
			}
		}
		if (aoRenderer != null && (bool)aoRenderer.transform)
		{
			additionalTransformsToMove.Add(aoRenderer.transform);
		}
		bool hasTriggeredVisibility = false;
		if (visibilityTrigger == 0f)
		{
			hasTriggeredVisibility = true;
			Pixelator.Instance.ProcessRoomAdditionalExits(doorObject.exitDef.GetUpstreamBasePosition(), doorObject.linkedRoom, false);
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if (elapsed >= visibilityTrigger && !hasTriggeredVisibility)
			{
				hasTriggeredVisibility = true;
				Pixelator.Instance.ProcessRoomAdditionalExits(doorObject.exitDef.GetUpstreamBasePosition(), doorObject.linkedRoom, false);
			}
			if (elapsed >= aoDisable)
			{
				if (aoRenderer != null)
				{
					aoRenderer.enabled = false;
				}
				GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
				aoDisable = duration * 2f;
				for (int j = 0; j < additionalTransformsToMove.Count; j++)
				{
					additionalTransformsToMove[j].GetComponent<Renderer>().enabled = false;
				}
				for (int k = 0; k < doorObjects.Count; k++)
				{
					doorObjects[k].collider.colliderObject.GetComponent<SpeculativeRigidbody>().enabled = !openState;
				}
			}
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			Vector3 frameDisplacement2 = target.position;
			target.position = Vector3.Lerp(startPosition, endPosition, t);
			frameDisplacement2 = target.position - frameDisplacement2;
			for (int l = 0; l < additionalTransformsToMove.Count; l++)
			{
				additionalTransformsToMove[l].position += frameDisplacement2;
			}
			yield return null;
		}
		for (int m = 0; m < additionalTransformsToMove.Count; m++)
		{
			Object.Destroy(additionalTransformsToMove[m].gameObject);
		}
		for (int n = 0; n < vfxToDestroy.Count; n++)
		{
			Object.Destroy(vfxToDestroy[n]);
		}
		if (openState)
		{
			doorObject.gameObject.SetActive(false);
		}
		OnFinishedOpeningDoors();
	}
}
