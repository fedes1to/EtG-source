using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;

public class BulletPastRoomController : MonoBehaviour
{
	public enum BulletRoomCategory
	{
		ROOM_A,
		ROOM_B,
		ROOM_C,
		ROOM_D
	}

	public BulletRoomCategory RoomIdentifier;

	public WarpPointHandler EntranceWarp;

	public WarpPointHandler ExitWarp;

	private TalkDoerLite OldBulletTalkDoer;

	public SpeculativeRigidbody OldBulletTalkTrigger;

	public TalkDoerLite AgunimPreDeathTalker;

	public TalkDoerLite AgunimPostDeathTalker;

	public tk2dSpriteAnimator AgunimFloorChunk;

	public ScreenShakeSettings AgunimPostDeathShake;

	public SpeculativeRigidbody AgunimFlightCollider;

	public SpeculativeRigidbody ThroneRoomDoor;

	public SpeculativeRigidbody ThroneRoomDoorTrigger;

	public VFXPool ThroneRoomDoorVfx;

	public ScreenShakeSettings ThroneRoomDoorShake;

	private IntVector2 m_agunimFloorBasePosition;

	private BulletPastRoomController RoomB;

	private BulletPastRoomController RoomC;

	private BulletPastRoomController RoomD;

	public GameObject BulletmanEndingQuad;

	public Texture2D[] BulletmanEndingFrames;

	private bool m_readyForTests;

	private float m_timeHovering;

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer.CurrentGun)
		{
			GameManager.Instance.SecondaryPlayer.inventory.DestroyCurrentGun();
		}
		if (RoomIdentifier == BulletRoomCategory.ROOM_A)
		{
			BulletPastRoomController[] componentsInChildren = base.transform.root.GetComponentsInChildren<BulletPastRoomController>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].RoomIdentifier == BulletRoomCategory.ROOM_B)
				{
					RoomB = componentsInChildren[i];
				}
				if (componentsInChildren[i].RoomIdentifier == BulletRoomCategory.ROOM_C)
				{
					RoomC = componentsInChildren[i];
				}
				if (componentsInChildren[i].RoomIdentifier == BulletRoomCategory.ROOM_D)
				{
					RoomD = componentsInChildren[i];
				}
			}
			RoomB.ExitWarp.DISABLED_TEMPORARILY = true;
			RoomHandler absoluteRoom = RoomC.transform.position.GetAbsoluteRoom();
			absoluteRoom.TargetPitfallRoom = RoomD.transform.position.GetAbsoluteRoom();
			absoluteRoom.OnTargetPitfallRoom = (Action)Delegate.Combine(absoluteRoom.OnTargetPitfallRoom, new Action(PitfallIntoGunonRoom));
			if ((bool)OldBulletTalkTrigger)
			{
				RoomHandler absoluteRoom2 = base.transform.position.GetAbsoluteRoom();
				if (absoluteRoom2 != null)
				{
					List<TalkDoerLite> componentsInRoom = absoluteRoom2.GetComponentsInRoom<TalkDoerLite>();
					if (componentsInRoom.Count > 0)
					{
						OldBulletTalkDoer = componentsInRoom[0];
					}
				}
				SpeculativeRigidbody oldBulletTalkTrigger = OldBulletTalkTrigger;
				oldBulletTalkTrigger.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(oldBulletTalkTrigger.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(WalkedByOldBullet));
			}
		}
		if (RoomIdentifier == BulletRoomCategory.ROOM_B)
		{
			RoomHandler absoluteRoom3 = base.transform.position.GetAbsoluteRoom();
			absoluteRoom3.Entered += PlayerEntered;
			absoluteRoom3.OnEnemiesCleared = (Action)Delegate.Combine(absoluteRoom3.OnEnemiesCleared, new Action(HandleRoomBCleared));
			ThroneRoomDoorTrigger.enabled = false;
			SpeculativeRigidbody throneRoomDoorTrigger = ThroneRoomDoorTrigger;
			throneRoomDoorTrigger.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(throneRoomDoorTrigger.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(EnteredThroneDoorTrigger));
		}
		if (RoomIdentifier == BulletRoomCategory.ROOM_C)
		{
			m_agunimFloorBasePosition = AgunimFloorChunk.transform.position.IntXY() + new IntVector2(2, 2);
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					IntVector2 key = m_agunimFloorBasePosition + new IntVector2(j, k);
					GameManager.Instance.Dungeon.data[key].fallingPrevented = true;
				}
			}
		}
		yield return new WaitForSeconds(1f);
		if (RoomIdentifier == BulletRoomCategory.ROOM_A)
		{
			ExitWarp.DISABLED_TEMPORARILY = true;
		}
		m_readyForTests = true;
	}

	private void HandleFlightCollider(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if ((bool)component && component.IsFlying && !GameManager.Instance.IsLoadingLevel)
		{
			m_timeHovering += BraveTime.DeltaTime;
			if (m_timeHovering > 0.5f)
			{
				component.ForceFall();
			}
		}
	}

	private void PitfallIntoGunonRoom()
	{
		StartCoroutine(DoGunonPitfall());
	}

	private IEnumerator DoGunonPitfall()
	{
		Pixelator.Instance.FadeToBlack(5f, true);
		GunonIntroDoer gunonIntroDoer = null;
		List<AIActor> allActors = StaticReferenceManager.AllEnemies;
		for (int i = 0; i < allActors.Count; i++)
		{
			if ((bool)allActors[i])
			{
				gunonIntroDoer = allActors[i].GetComponent<GunonIntroDoer>();
				if ((bool)gunonIntroDoer)
				{
					break;
				}
			}
		}
		GenericIntroDoer genericIntroDoer = gunonIntroDoer.GetComponent<GenericIntroDoer>();
		genericIntroDoer.cameraMoveSpeed = 100f;
		genericIntroDoer.SuppressSkipping = true;
		float timer = 5f;
		while (timer > 0f)
		{
			timer -= GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		genericIntroDoer.cameraMoveSpeed = 15f;
		genericIntroDoer.SuppressSkipping = false;
	}

	private void PlayerEntered(PlayerController p)
	{
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom != null)
		{
			absoluteRoom.SealRoom();
		}
	}

	private void WalkedByOldBullet(SpeculativeRigidbody specrigidbody, SpeculativeRigidbody sourcespecrigidbody, CollisionData collisiondata)
	{
		if ((bool)OldBulletTalkDoer)
		{
			FsmBool fsmBool = OldBulletTalkDoer.playmakerFsm.FsmVariables.FindFsmBool("giftGiven");
			if (fsmBool != null && !fsmBool.Value)
			{
				OldBulletTalkDoer.playmakerFsm.SendEvent("playerInteract");
			}
		}
	}

	private void EnteredThroneDoorTrigger(SpeculativeRigidbody specrigidbody, SpeculativeRigidbody sourcespecrigidbody, CollisionData collisiondata)
	{
		if ((bool)ThroneRoomDoor)
		{
			StartCoroutine(MoveThroneRoomDoor());
		}
		SpeculativeRigidbody throneRoomDoorTrigger = ThroneRoomDoorTrigger;
		throneRoomDoorTrigger.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Remove(throneRoomDoorTrigger.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(EnteredThroneDoorTrigger));
	}

	private void HandleRoomBCleared()
	{
		if ((bool)ThroneRoomDoorTrigger)
		{
			ThroneRoomDoorTrigger.enabled = true;
		}
		ExitWarp.DISABLED_TEMPORARILY = false;
	}

	private IEnumerator MoveThroneRoomDoor()
	{
		float elapsed = 0f;
		float duration = 10f;
		SpeculativeRigidbody doorRigidbody = ThroneRoomDoor.specRigidbody;
		float poofDelta = 0.2f;
		float nextPoofTime = 0f;
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(ThroneRoomDoorShake, this);
		AkSoundEngine.PostEvent("Play_ENV_quake_loop_01", base.gameObject);
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = BraveMathCollege.DoubleLerpSmooth(0f, 1f, 0f, elapsed / duration);
			ThroneRoomDoor.Velocity = new Vector2(-1f * t, 0f);
			while (elapsed > nextPoofTime)
			{
				float num = BraveMathCollege.DoubleLerpSmooth(0f, 1f, 0f, nextPoofTime / duration);
				nextPoofTime += poofDelta;
				if (UnityEngine.Random.value < num)
				{
					ThroneRoomDoorVfx.SpawnAtPosition(BraveUtility.RandomVector2(doorRigidbody.UnitBottomLeft, doorRigidbody.UnitBottomRight));
				}
			}
			yield return null;
		}
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		AkSoundEngine.PostEvent("Stop_ENV_quake_loop_01", base.gameObject);
		ThroneRoomDoor.Velocity = Vector2.zero;
	}

	private void Update()
	{
		if (!Dungeon.IsGenerating && RoomIdentifier == BulletRoomCategory.ROOM_A && m_readyForTests && ExitWarp.DISABLED_TEMPORARILY && GameManager.Instance.PrimaryPlayer.CurrentGun != null)
		{
			Debug.LogError(GameManager.Instance.PrimaryPlayer.CurrentGun);
			ExitWarp.DISABLED_TEMPORARILY = false;
		}
	}

	public IEnumerator HandleAgunimIntro(Transform bossTransform)
	{
		bossTransform.GetComponent<AIActor>().ToggleRenderers(false);
		AgunimPreDeathTalker.gameObject.SetActive(true);
		AgunimPreDeathTalker.transform.position = bossTransform.position;
		AgunimPreDeathTalker.specRigidbody.Reinitialize();
		AgunimPreDeathTalker.sprite.IsPerpendicular = true;
		AgunimPreDeathTalker.sprite.HeightOffGround = -1f;
		AgunimPreDeathTalker.sprite.UpdateZDepth();
		AgunimPreDeathTalker.Interact(GameManager.Instance.PrimaryPlayer);
		GameManager.Instance.MainCameraController.OverridePosition = AgunimPreDeathTalker.specRigidbody.UnitCenter;
		while (AgunimPreDeathTalker.IsTalking)
		{
			yield return null;
		}
		AgunimPreDeathTalker.specRigidbody.enabled = false;
		AgunimPreDeathTalker.gameObject.SetActive(false);
		bossTransform.GetComponent<AIActor>().ToggleRenderers(true);
		List<AIActor> enemiesInRoom = AgunimPreDeathTalker.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < enemiesInRoom.Count; i++)
		{
			GenericIntroDoer component = enemiesInRoom[i].GetComponent<GenericIntroDoer>();
			if ((bool)component)
			{
				component.TriggerSequence(GameManager.Instance.PrimaryPlayer);
			}
		}
	}

	public void OnGanonDeath(Transform bossTransform)
	{
		StartCoroutine(HandleGanonDeath(bossTransform));
	}

	public IEnumerator HandleGanonDeath(Transform bossTransform)
	{
		yield return null;
	}

	public IEnumerator HandleAgunimDeath(Transform bossTransform)
	{
		AgunimPostDeathTalker.gameObject.SetActive(true);
		AgunimPostDeathTalker.transform.position = bossTransform.position;
		AgunimPostDeathTalker.specRigidbody.Reinitialize();
		AgunimPostDeathTalker.sprite.IsPerpendicular = true;
		AgunimPostDeathTalker.sprite.HeightOffGround = -1f;
		AgunimPostDeathTalker.sprite.UpdateZDepth();
		AgunimPostDeathTalker.Interact(GameManager.Instance.PrimaryPlayer);
		while (AgunimPostDeathTalker.IsTalking)
		{
			yield return null;
		}
		GameManager.Instance.MainCameraController.StartTrackingPlayer();
		GameManager.Instance.MainCameraController.SetManualControl(false);
		yield return new WaitForSeconds(1f);
		while (AgunimPostDeathTalker.aiAnimator.IsPlaying("die"))
		{
			yield return null;
		}
		UnityEngine.Object.Destroy(AgunimPostDeathTalker.gameObject);
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(AgunimPostDeathShake, this);
		AkSoundEngine.PostEvent("Play_ENV_quake_loop_01", base.gameObject);
		yield return new WaitForSeconds(3f);
		TriggerAgumimFloorBreak();
		AkSoundEngine.PostEvent("Stop_ENV_quake_loop_01", base.gameObject);
		yield return new WaitForSeconds(1f);
		if ((bool)AgunimFlightCollider)
		{
			SpeculativeRigidbody agunimFlightCollider = AgunimFlightCollider;
			agunimFlightCollider.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(agunimFlightCollider.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleFlightCollider));
		}
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
	}

	public void TriggerAgumimFloorBreak()
	{
		if (!(AgunimFloorChunk != null))
		{
			return;
		}
		AgunimFloorChunk.transform.localPosition = new Vector3(9.75f, 10.625f, 12.375f);
		AgunimFloorChunk.PlayAndDisableRenderer("agunim_lair_burst");
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				IntVector2 key = m_agunimFloorBasePosition + new IntVector2(i, j);
				GameManager.Instance.Dungeon.data[key].fallingPrevented = false;
			}
		}
	}

	public void TriggerBulletmanEnding()
	{
		if (RoomIdentifier == BulletRoomCategory.ROOM_D)
		{
			GameStatsManager.Instance.SetCharacterSpecificFlag(CharacterSpecificGungeonFlags.KILLED_PAST, true);
			StartCoroutine(TriggerBulletmanEnding_CR());
		}
	}

	public IEnumerator TriggerBulletmanEnding_CR()
	{
		PastCameraUtility.LockConversation(GameManager.Instance.MainCameraController.transform.position.XY());
		tk2dSpriteFromTexture animSprite = null;
		yield return new WaitForSeconds(1.5f);
		Pixelator.Instance.SetVignettePower(5f);
		if (RoomIdentifier == BulletRoomCategory.ROOM_D && BulletmanEndingFrames.Length > 0 && BulletmanEndingFrames[0] != null)
		{
			GameObject gameObject = new GameObject("ending sprite");
			gameObject.AddComponent<tk2dSprite>();
			tk2dSpriteFromTexture tk2dSpriteFromTexture2 = gameObject.AddComponent<tk2dSpriteFromTexture>();
			tk2dSpriteCollectionSize spriteCollectionSize = tk2dSpriteCollectionSize.Default();
			animSprite = tk2dSpriteFromTexture2;
			tk2dSpriteFromTexture2.Create(spriteCollectionSize, BulletmanEndingFrames[0], tk2dBaseSprite.Anchor.MiddleCenter);
			tk2dSpriteFromTexture2.spriteCollectionSize.type = tk2dSpriteCollectionSize.Type.Explicit;
			tk2dSpriteFromTexture2.spriteCollectionSize.orthoSize = 0.5f;
			tk2dSpriteFromTexture2.spriteCollectionSize.height = 16f;
			tk2dSpriteFromTexture2.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
			tk2dSpriteFromTexture2.transform.position = GameManager.Instance.PrimaryPlayer.CenterPosition.ToVector3ZUp(1f);
			AdditionalBraveLight additionalBraveLight = tk2dSpriteFromTexture2.gameObject.AddComponent<AdditionalBraveLight>();
			additionalBraveLight.LightIntensity = 1f;
			additionalBraveLight.LightRadius = 20f;
			StartCoroutine(AnimateBulletmanEnding(tk2dSpriteFromTexture2));
		}
		Pixelator.Instance.DoOcclusionLayer = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("ending");
		}
		GameObject instanceQuad = UnityEngine.Object.Instantiate(BulletmanEndingQuad);
		instanceQuad.transform.position = GameManager.Instance.PrimaryPlayer.CenterPosition.ToVector3ZUp();
		animSprite.GetComponent<tk2dSprite>().HeightOffGround = animSprite.transform.position.y + 3f;
		animSprite.GetComponent<tk2dSprite>().UpdateZDepth();
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		GameManager.Instance.MainCameraController.OverridePosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
		Pixelator.Instance.FadeToBlack(1f, true, 0.5f);
		yield return new WaitForSeconds(5.5f);
		Pixelator.Instance.FreezeFrame();
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		float ela = 0f;
		while (ela < ConvictPastController.FREEZE_FRAME_DURATION)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		BraveTime.ClearMultiplier(base.gameObject);
		GameManager.Instance.MainCameraController.SetManualControl(false, false);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		ttcc.ClearDebris();
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		Pixelator.Instance.SetVignettePower(1f);
		yield return StartCoroutine(ttcc.HandleTimeTubeCredits(GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, false, null, -1));
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private IEnumerator AnimateBulletmanEnding(tk2dSpriteFromTexture sft)
	{
		float elapsed = 0f;
		int currentIndex = 0;
		while (true)
		{
			elapsed += BraveTime.DeltaTime;
			if (elapsed > 0.175f)
			{
				elapsed -= 0.175f;
				currentIndex = (currentIndex + 1) % BulletmanEndingFrames.Length;
				sft.Create(sft.spriteCollectionSize, BulletmanEndingFrames[currentIndex], tk2dBaseSprite.Anchor.MiddleCenter);
			}
			yield return null;
		}
	}
}
