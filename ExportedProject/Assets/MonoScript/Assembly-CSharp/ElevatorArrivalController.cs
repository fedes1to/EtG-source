using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ElevatorArrivalController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public tk2dSpriteAnimator elevatorAnimator;

	public tk2dSpriteAnimator floorAnimator;

	public SpeculativeRigidbody elevatorCollider;

	public tk2dSprite[] priorSprites;

	public tk2dSprite[] postSprites;

	public BreakableChunk chunker;

	public List<GameObject> poofObjects;

	public Transform spawnTransform;

	public GameObject elevatorFloor;

	public tk2dSpriteAnimator crumblyBumblyAnimator;

	public tk2dSpriteAnimator smokeAnimator;

	[CheckAnimation("elevatorAnimator")]
	public string elevatorDescendAnimName;

	[CheckAnimation("elevatorAnimator")]
	public string elevatorOpenAnimName;

	[CheckAnimation("elevatorAnimator")]
	public string elevatorCloseAnimName;

	[CheckAnimation("elevatorAnimator")]
	public string elevatorDepartAnimName;

	public ScreenShakeSettings arrivalShake;

	public ScreenShakeSettings doorOpenShake;

	public ScreenShakeSettings doorCloseShake;

	public ScreenShakeSettings departureShake;

	private bool m_isArrived;

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
			}
		}
		for (int k = 0; k < 6; k++)
		{
			for (int l = -2; l < 8; l++)
			{
				CellData cellData2 = GameManager.Instance.Dungeon.data.cellData[intVector.x + k][intVector.y + l];
				cellData2.cellVisualData.containsObjectSpaceStamp = true;
				cellData2.cellVisualData.containsWallSpaceStamp = true;
			}
		}
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_READY_FOR_UNLOCKS))
		{
			return;
		}
		bool flag = false;
		switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
		{
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_COMPLETE))
			{
				flag = true;
			}
			break;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_COMPLETE))
			{
				flag = true;
			}
			break;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_COMPLETE))
			{
				flag = true;
			}
			break;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK4_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_COMPLETE))
			{
				flag = true;
			}
			break;
		}
		if (flag)
		{
			GameObject gameObject = ResourceCache.Acquire("Global Prefabs/ElevatorMaintenanceSign") as GameObject;
			UnityEngine.Object.Instantiate(gameObject, base.transform.position + gameObject.transform.position, Quaternion.identity);
		}
	}

	private void TransitionToDoorOpen(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDoorOpen));
		elevatorFloor.SetActive(true);
		smokeAnimator.gameObject.SetActive(true);
		smokeAnimator.PlayAndDisableObject(string.Empty);
		GameManager.Instance.MainCameraController.DoScreenShake(doorOpenShake, null);
		animator.Play(elevatorOpenAnimName);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnDoorOpened));
	}

	private void OnDoorOpened(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnDoorOpened));
		if ((bool)animator.specRigidbody)
		{
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(animator.specRigidbody);
		}
		if ((bool)elevatorCollider)
		{
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(elevatorCollider);
		}
	}

	private void TransitionToDoorClose(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		GameManager.Instance.MainCameraController.DoScreenShake(doorCloseShake, null);
		animator.Play(elevatorCloseAnimName);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDepart));
	}

	private void TransitionToDepart(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		elevatorFloor.SetActive(false);
		GameManager.Instance.MainCameraController.DoDelayedScreenShake(departureShake, 0.25f, animator.sprite.WorldCenter);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToDepart));
		animator.PlayAndDisableObject(elevatorDepartAnimName);
		StartCoroutine(HandleDepartBumbly());
		if ((bool)elevatorCollider)
		{
			elevatorCollider.enabled = false;
		}
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

	private IEnumerator HandleDepartBumbly()
	{
		float elapsed = 0f;
		float duration = 0.55f;
		while (elapsed < duration)
		{
			if (elapsed > 0.3f && !crumblyBumblyAnimator.gameObject.activeSelf)
			{
				crumblyBumblyAnimator.gameObject.SetActive(true);
				crumblyBumblyAnimator.PlayAndDisableObject(string.Empty);
			}
			elapsed += BraveTime.DeltaTime;
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
		postSprites[3].renderer.material = material2;
		postSprites[1].HeightOffGround = postSprites[1].HeightOffGround - 0.0625f;
		postSprites[3].HeightOffGround = postSprites[3].HeightOffGround - 0.0625f;
		postSprites[1].UpdateZDepth();
		ToggleSprites(true);
	}

	private void Update()
	{
		if (!m_isArrived)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (Vector2.Distance(spawnTransform.position.XY(), GameManager.Instance.AllPlayers[i].CenterPosition) < 6f)
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

	public void DoDeparture()
	{
		m_isArrived = false;
		TransitionToDoorClose(elevatorAnimator, elevatorAnimator.CurrentClip);
		DeflagCells();
	}

	public void DoArrival(PlayerController player, float initialDelay)
	{
		if (!m_isArrived)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				playerController.ToggleGunRenderers(false, string.Empty);
				playerController.ToggleShadowVisiblity(false);
				playerController.ToggleHandRenderers(false, string.Empty);
				playerController.ToggleFollowerRenderers(false);
				playerController.SetInputOverride("elevator arrival");
			}
			m_isArrived = true;
			StartCoroutine(HandleArrival(initialDelay));
		}
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

	private IEnumerator HandleArrival(float initialDelay)
	{
		float ela = 0f;
		while (ela < initialDelay)
		{
			ela += BraveTime.DeltaTime;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameManager.Instance.AllPlayers[i].ToggleFollowerRenderers(false);
			}
			yield return null;
		}
		elevatorAnimator.gameObject.SetActive(true);
		Transform elevatorTransform = elevatorAnimator.transform;
		Vector3 elevatorStartPosition = elevatorTransform.position;
		int cachedFloorframe = floorAnimator.Sprite.spriteId;
		elevatorFloor.SetActive(false);
		elevatorAnimator.Play(elevatorDescendAnimName);
		elevatorAnimator.StopAndResetFrame();
		ToggleSprites(true);
		floorAnimator.Sprite.SetSprite(cachedFloorframe);
		float elapsed = 0f;
		float duration = 0.2f;
		float yDistance = 20f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			float yOffset = Mathf.Lerp(yDistance, 0f, t);
			elevatorTransform.position = elevatorStartPosition + new Vector3(0f, yOffset, 0f);
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
		floorAnimator.Play();
		yield return new WaitForSeconds(0.1f);
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[j];
			playerController.ClearInputOverride("elevator arrival");
		}
		for (int k = 0; k < poofObjects.Count; k++)
		{
			poofObjects[k].SetActive(true);
			poofObjects[k].GetComponent<tk2dBaseSprite>().IsPerpendicular = true;
			poofObjects[k].GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject(string.Empty);
			poofObjects[k].GetComponent<tk2dSpriteAnimator>().ClipFps = poofObjects[k].GetComponent<tk2dSpriteAnimator>().ClipFps * UnityEngine.Random.Range(0.8f, 1.1f);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
