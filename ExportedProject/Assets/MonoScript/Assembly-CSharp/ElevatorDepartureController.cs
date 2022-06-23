using System;
using System.Collections;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;

public class ElevatorDepartureController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public tk2dSpriteAnimator elevatorAnimator;

	public tk2dSpriteAnimator ceilingAnimator;

	public tk2dSpriteAnimator facewallAnimator;

	public tk2dSpriteAnimator floorAnimator;

	public tk2dSprite[] priorSprites;

	public tk2dSprite[] postSprites;

	public BreakableChunk chunker;

	public Transform spawnTransform;

	public GameObject elevatorFloor;

	public tk2dSpriteAnimator crumblyBumblyAnimator;

	public tk2dSpriteAnimator smokeAnimator;

	public string elevatorDescendAnimName;

	public string elevatorOpenAnimName;

	public string elevatorCloseAnimName;

	public string elevatorDepartAnimName;

	public ScreenShakeSettings arrivalShake;

	public ScreenShakeSettings doorOpenShake;

	public ScreenShakeSettings doorCloseShake;

	public ScreenShakeSettings departureShake;

	public bool ReturnToFoyerWithNewInstance;

	public bool UsesOverrideTargetFloor;

	public GlobalDungeonData.ValidTilesets OverrideTargetFloor;

	private Tribool m_isArrived = Tribool.Unready;

	private Tribool m_isCryoArrived = Tribool.Unready;

	private TalkDoerLite m_cryoButton;

	private FsmBool m_cryoBool;

	private FsmBool m_normalBool;

	public const bool c_savingEnabled = true;

	private tk2dSpriteAnimator m_activeCryoElevatorAnimator;

	private bool m_depatureIsPlayerless;

	private bool m_hasEverArrived;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 0; i < 6; i++)
		{
			for (int j = -2; j < 6; j++)
			{
				CellData cellData = GameManager.Instance.Dungeon.data.cellData[intVector.x + i][intVector.y + j];
				cellData.cellVisualData.precludeAllTileDrawing = true;
				if (j < 4)
				{
					cellData.type = CellType.PIT;
					cellData.fallingPrevented = true;
				}
				cellData.isOccupied = true;
			}
		}
		if ((GameManager.Instance.CurrentGameMode != 0 && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SHORTCUT) || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
		{
			return;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/CryoElevatorButton"), base.transform.position + new Vector3(-1f, 0f, 0f), Quaternion.identity);
		IntVector2 intVector2 = base.transform.position.IntXY(VectorConversions.Floor) + new IntVector2(-2, 0);
		for (int k = 0; k < 2; k++)
		{
			for (int l = -1; l < 2; l++)
			{
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector2 + new IntVector2(k, l)))
				{
					CellData cellData2 = GameManager.Instance.Dungeon.data[intVector2 + new IntVector2(k, l)];
					cellData2.cellVisualData.containsWallSpaceStamp = true;
					cellData2.cellVisualData.containsObjectSpaceStamp = true;
				}
			}
		}
		m_cryoButton = gameObject.GetComponentInChildren<TalkDoerLite>();
		room.RegisterInteractable(m_cryoButton);
		TalkDoerLite cryoButton = m_cryoButton;
		cryoButton.OnGenericFSMActionA = (Action)Delegate.Combine(cryoButton.OnGenericFSMActionA, new Action(SwitchToCryoElevator));
		TalkDoerLite cryoButton2 = m_cryoButton;
		cryoButton2.OnGenericFSMActionB = (Action)Delegate.Combine(cryoButton2.OnGenericFSMActionB, new Action(RescindCryoElevator));
		m_cryoBool = m_cryoButton.playmakerFsm.FsmVariables.GetFsmBool("IS_CRYO");
		m_normalBool = m_cryoButton.playmakerFsm.FsmVariables.GetFsmBool("IS_NORMAL");
	}

	private void ToggleSprites(bool prior)
	{
		for (int i = 0; i < priorSprites.Length; i++)
		{
			if ((bool)priorSprites[i] && (bool)priorSprites[i].renderer)
			{
				priorSprites[i].renderer.enabled = prior;
			}
		}
		for (int j = 0; j < postSprites.Length; j++)
		{
			if ((bool)postSprites[j] && (bool)postSprites[j].renderer)
			{
				postSprites[j].renderer.enabled = !prior;
			}
		}
	}

	public void SwitchToCryoElevator()
	{
		if (!(m_isArrived != Tribool.Ready))
		{
			DoPlayerlessDeparture();
			GameManager.Instance.Dungeon.StartCoroutine(CryoWaitForPreviousElevatorDeparture());
		}
	}

	private IEnumerator CryoWaitForPreviousElevatorDeparture()
	{
		m_isCryoArrived = Tribool.Unready;
		while (elevatorAnimator.enabled && elevatorAnimator.gameObject.activeSelf)
		{
			yield return null;
		}
		if (m_activeCryoElevatorAnimator == null)
		{
			GameObject original = (GameObject)BraveResources.Load("Global Prefabs/CryoElevator");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = elevatorAnimator.transform.localPosition;
			m_activeCryoElevatorAnimator = gameObject.GetComponent<tk2dSpriteAnimator>();
		}
		m_activeCryoElevatorAnimator.gameObject.SetActive(true);
		m_activeCryoElevatorAnimator.Play("arrive");
		yield return null;
		MeshRenderer floorObject = m_activeCryoElevatorAnimator.transform.Find("ElevatorInterior (1)").GetComponent<MeshRenderer>();
		floorObject.enabled = true;
		elevatorFloor.SetActive(true);
		elevatorFloor.GetComponent<MeshRenderer>().enabled = false;
		while (m_activeCryoElevatorAnimator.IsPlaying("arrive") && m_activeCryoElevatorAnimator.CurrentFrame < m_activeCryoElevatorAnimator.CurrentClip.frames.Length - 2)
		{
			yield return null;
		}
		tk2dSpriteAnimator mistAnimator = m_activeCryoElevatorAnimator.transform.Find("Sierra").GetComponent<tk2dSpriteAnimator>();
		tk2dSpriteAnimator doorAnimator = m_activeCryoElevatorAnimator.transform.Find("Door").GetComponent<tk2dSpriteAnimator>();
		doorAnimator.GetComponent<MeshRenderer>().enabled = true;
		doorAnimator.Play("door_open");
		mistAnimator.GetComponent<MeshRenderer>().enabled = true;
		mistAnimator.PlayAndDisableRenderer("mist");
		yield return null;
		while (doorAnimator.IsPlaying("door_open"))
		{
			yield return null;
		}
		m_isCryoArrived = Tribool.Ready;
	}

	private void SetFSMStates()
	{
		if ((bool)m_cryoButton)
		{
			m_cryoBool.Value = m_isCryoArrived == Tribool.Ready;
			m_normalBool.Value = m_isArrived == Tribool.Ready;
		}
	}

	public void RescindCryoElevator()
	{
		if (!(m_isCryoArrived != Tribool.Ready))
		{
			DoCryoDeparture();
		}
	}

	public void DoCryoDeparture(bool playerless = true)
	{
		if (m_activeCryoElevatorAnimator == null || m_isCryoArrived != Tribool.Ready)
		{
			return;
		}
		m_isCryoArrived = Tribool.Complete;
		if (!playerless)
		{
			if ((bool)Minimap.Instance)
			{
				Minimap.Instance.PreventAllTeleports = true;
			}
			if (GameManager.HasInstance && GameManager.Instance.AllPlayers != null)
			{
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					if ((bool)GameManager.Instance.AllPlayers[i])
					{
						GameManager.Instance.AllPlayers[i].CurrentInputState = PlayerInputState.NoInput;
					}
				}
			}
		}
		GameManager.Instance.Dungeon.StartCoroutine(HandleCryoDeparture(playerless));
	}

	public IEnumerator HandleCryoDeparture(bool playerless)
	{
		MeshRenderer floorObject = m_activeCryoElevatorAnimator.transform.Find("ElevatorInterior (1)").GetComponent<MeshRenderer>();
		tk2dSpriteAnimator doorAnimator = m_activeCryoElevatorAnimator.transform.Find("Door").GetComponent<tk2dSpriteAnimator>();
		doorAnimator.Play("door_close");
		yield return null;
		while (doorAnimator.IsPlaying("door_close"))
		{
			yield return null;
		}
		elevatorFloor.SetActive(false);
		GameManager.Instance.MainCameraController.DoDelayedScreenShake(departureShake, 0.25f, null);
		if (!playerless)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameManager.Instance.AllPlayers[i].PrepareForSceneTransition();
			}
			Pixelator.Instance.FadeToBlack(0.5f);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
			AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
			GlobalDungeonData.ValidTilesets nextTileset = GameManager.Instance.GetNextTileset(GameManager.Instance.Dungeon.tileIndices.tilesetId);
			GameManager.DoMidgameSave(nextTileset);
			float delay = 0.5f;
			GameManager.Instance.DelayedLoadCharacterSelect(delay, true, true);
		}
		doorAnimator.GetComponent<MeshRenderer>().enabled = false;
		floorObject.enabled = false;
		m_activeCryoElevatorAnimator.Play("depart");
		yield return null;
		while (m_activeCryoElevatorAnimator.IsPlaying("depart"))
		{
			yield return null;
		}
		m_activeCryoElevatorAnimator.gameObject.SetActive(false);
		if (playerless)
		{
			m_isArrived = Tribool.Unready;
		}
	}

	private void TransitionToDoorOpen(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDoorOpen));
		elevatorFloor.SetActive(true);
		elevatorFloor.GetComponent<MeshRenderer>().enabled = true;
		smokeAnimator.gameObject.SetActive(true);
		smokeAnimator.PlayAndDisableObject(string.Empty);
		GameManager.Instance.MainCameraController.DoScreenShake(doorOpenShake, null);
		animator.Play(elevatorOpenAnimName);
	}

	private void TransitionToDoorClose(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		GameManager.Instance.MainCameraController.DoScreenShake(doorCloseShake, null);
		animator.Play(elevatorCloseAnimName);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDepart));
	}

	private void TransitionToDepart(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		GameManager.Instance.MainCameraController.DoDelayedScreenShake(departureShake, 0.25f, null);
		if (!m_depatureIsPlayerless)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameManager.Instance.AllPlayers[i].PrepareForSceneTransition();
			}
			Pixelator.Instance.FadeToBlack(0.5f);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
			float delay = 0.5f;
			if (ReturnToFoyerWithNewInstance)
			{
				GameManager.Instance.DelayedReturnToFoyer(delay);
			}
			else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH)
			{
				GameManager.Instance.DelayedLoadBossrushFloor(delay);
			}
			else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
			{
				GameManager.Instance.DelayedLoadBossrushFloor(delay);
			}
			else
			{
				if (!GameManager.Instance.IsFoyer && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
				{
					GlobalDungeonData.ValidTilesets nextTileset = GameManager.Instance.GetNextTileset(GameManager.Instance.Dungeon.tileIndices.tilesetId);
					GameManager.DoMidgameSave(nextTileset);
				}
				if (UsesOverrideTargetFloor)
				{
					switch (OverrideTargetFloor)
					{
					case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
						GameManager.Instance.DelayedLoadCustomLevel(delay, "tt_catacombs");
						break;
					case GlobalDungeonData.ValidTilesets.FORGEGEON:
						GameManager.Instance.DelayedLoadCustomLevel(delay, "tt_forge");
						break;
					}
				}
				else
				{
					GameManager.Instance.DelayedLoadNextLevel(delay);
				}
				AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
			}
		}
		elevatorFloor.SetActive(false);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDepart));
		animator.PlayAndDisableObject(elevatorDepartAnimName);
	}

	private void DeflagCells()
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 0; i < 6; i++)
		{
			for (int j = -2; j < 6; j++)
			{
				if ((j != -2 || (i >= 2 && i <= 3)) && (j != -1 || (i >= 1 && i <= 4)))
				{
					CellData cellData = GameManager.Instance.Dungeon.data.cellData[intVector.x + i][intVector.y + j];
					if (j < 4)
					{
						cellData.fallingPrevented = false;
					}
				}
			}
		}
	}

	private IEnumerator HandleDepartMotion()
	{
		Transform elevatorTransform = elevatorAnimator.transform;
		Vector3 elevatorStartDepartPosition = elevatorTransform.position;
		float elapsed = 0f;
		float duration = 0.55f;
		float yDistance = 20f;
		bool hasLayerSwapped = false;
		while (elapsed < duration)
		{
			if (elapsed > 0.15f && !crumblyBumblyAnimator.gameObject.activeSelf)
			{
				crumblyBumblyAnimator.gameObject.SetActive(true);
				crumblyBumblyAnimator.PlayAndDisableObject(string.Empty);
			}
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			float yOffset = BraveMathCollege.SmoothLerp(0f, 0f - yDistance, t);
			if (yOffset < -2f && !hasLayerSwapped)
			{
				hasLayerSwapped = true;
				elevatorAnimator.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
			}
			elevatorTransform.position = elevatorStartDepartPosition + new Vector3(0f, yOffset, 0f);
			if (facewallAnimator != null)
			{
				facewallAnimator.Sprite.UpdateZDepth();
			}
			yield return null;
		}
	}

	private void Start()
	{
		Material material = UnityEngine.Object.Instantiate(priorSprites[1].renderer.material);
		material.shader = ShaderCache.Acquire("Brave/Unity Transparent Cutout");
		priorSprites[1].renderer.material = material;
		Material material2 = UnityEngine.Object.Instantiate(postSprites[2].renderer.material);
		material2.shader = ShaderCache.Acquire("Brave/Unity Transparent Cutout");
		postSprites[2].renderer.material = material2;
		postSprites[1].HeightOffGround = postSprites[1].HeightOffGround - 0.0625f;
		postSprites[3].HeightOffGround = postSprites[3].HeightOffGround - 0.0625f;
		postSprites[1].UpdateZDepth();
		SpeculativeRigidbody component = elevatorFloor.GetComponent<SpeculativeRigidbody>();
		if ((bool)component)
		{
			component.PrimaryPixelCollider.ManualOffsetY -= 8;
			component.PrimaryPixelCollider.ManualHeight += 8;
			component.Reinitialize();
			component.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnElevatorTriggerEnter));
		}
		ToggleSprites(true);
	}

	private void OnElevatorTriggerEnter(SpeculativeRigidbody otherSpecRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (m_isArrived == Tribool.Ready)
		{
			if (!(otherSpecRigidbody.GetComponent<PlayerController>() != null))
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
					DoDeparture();
				}
			}
			else
			{
				DoDeparture();
			}
		}
		else
		{
			if (!(m_isCryoArrived == Tribool.Ready) || !(m_activeCryoElevatorAnimator != null) || !(otherSpecRigidbody.GetComponent<PlayerController>() != null))
			{
				return;
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				bool flag2 = true;
				for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
				{
					if (!GameManager.Instance.AllPlayers[j].healthHaver.IsDead && !sourceSpecRigidbody.ContainsPoint(GameManager.Instance.AllPlayers[j].SpriteBottomCenter.XY(), int.MaxValue, true))
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					DoCryoDeparture(false);
				}
			}
			else
			{
				DoCryoDeparture(false);
			}
		}
	}

	private void Update()
	{
		PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(spawnTransform.position.XY(), true);
		if (activePlayerClosestToPoint != null && m_isArrived == Tribool.Unready && Vector2.Distance(spawnTransform.position.XY(), activePlayerClosestToPoint.CenterPosition) < 8f)
		{
			DoArrival();
		}
		if (m_cryoBool != null && m_normalBool != null)
		{
			SetFSMStates();
		}
	}

	public void DoPlayerlessDeparture()
	{
		m_depatureIsPlayerless = true;
		m_isArrived = Tribool.Complete;
		TransitionToDoorClose(elevatorAnimator, elevatorAnimator.CurrentClip);
	}

	public void DoDeparture()
	{
		m_depatureIsPlayerless = false;
		m_isArrived = Tribool.Complete;
		if ((bool)Minimap.Instance)
		{
			Minimap.Instance.PreventAllTeleports = true;
		}
		if (GameManager.HasInstance && GameManager.Instance.AllPlayers != null)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if ((bool)GameManager.Instance.AllPlayers[i])
				{
					GameManager.Instance.AllPlayers[i].CurrentInputState = PlayerInputState.NoInput;
				}
			}
		}
		TransitionToDoorClose(elevatorAnimator, elevatorAnimator.CurrentClip);
	}

	public void DoArrival()
	{
		m_isArrived = Tribool.Ready;
		m_hasEverArrived = true;
		StartCoroutine(HandleArrival(0f));
	}

	private IEnumerator HandleArrival(float initialDelay)
	{
		yield return new WaitForSeconds(initialDelay);
		elevatorAnimator.gameObject.SetActive(true);
		Transform elevatorTransform = elevatorAnimator.transform;
		Vector3 elevatorStartPosition = elevatorTransform.position;
		int cachedCeilingFrame = ((!(ceilingAnimator != null)) ? (-1) : ceilingAnimator.Sprite.spriteId);
		int cachedFacewallFrame = ((!(facewallAnimator != null)) ? (-1) : facewallAnimator.Sprite.spriteId);
		int cachedFloorframe = floorAnimator.Sprite.spriteId;
		elevatorFloor.SetActive(false);
		elevatorAnimator.Play(elevatorDescendAnimName);
		elevatorAnimator.StopAndResetFrame();
		if (ceilingAnimator != null)
		{
			ceilingAnimator.Sprite.SetSprite(cachedCeilingFrame);
		}
		if (facewallAnimator != null)
		{
			facewallAnimator.Sprite.SetSprite(cachedFacewallFrame);
		}
		floorAnimator.Sprite.SetSprite(cachedFloorframe);
		if (!m_hasEverArrived)
		{
			ToggleSprites(true);
		}
		float elapsed = 0f;
		float duration = 0.1f;
		float yDistance = 20f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			float yOffset = Mathf.Lerp(yDistance, 0f, t);
			elevatorTransform.position = elevatorStartPosition + new Vector3(0f, yOffset, 0f);
			if (facewallAnimator != null)
			{
				facewallAnimator.Sprite.UpdateZDepth();
			}
			yield return null;
		}
		GameManager.Instance.MainCameraController.DoScreenShake(arrivalShake, null);
		elevatorAnimator.Play();
		tk2dSpriteAnimator obj = elevatorAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDoorOpen));
		ToggleSprites(false);
		if (chunker != null)
		{
			chunker.Trigger(true, base.transform.position + new Vector3(3f, 3f, 3f));
		}
		if (ceilingAnimator != null)
		{
			ceilingAnimator.Play();
		}
		if (facewallAnimator != null)
		{
			facewallAnimator.Play();
		}
		floorAnimator.Play();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
