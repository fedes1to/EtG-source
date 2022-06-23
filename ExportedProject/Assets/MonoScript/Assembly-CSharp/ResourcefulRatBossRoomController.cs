using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class ResourcefulRatBossRoomController : MonoBehaviour
{
	public tk2dSpriteAnimator grateAnimator;

	private RoomHandler m_ratRoom;

	private bool m_isRoomSealed;

	public IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		GameManager.Instance.Dungeon.StartCoroutine(LateStart());
	}

	public IEnumerator LateStart()
	{
		yield return null;
		ResourcefulRatController rat = UnityEngine.Object.FindObjectOfType<ResourcefulRatController>();
		MetalGearRatController metalGearRat = UnityEngine.Object.FindObjectOfType<MetalGearRatController>();
		m_ratRoom = rat.aiActor.ParentRoom;
		RoomHandler metalGearRatRoom = ((!metalGearRat) ? null : metalGearRat.aiActor.ParentRoom);
		if (metalGearRatRoom != null)
		{
			m_ratRoom.TargetPitfallRoom = metalGearRatRoom;
			m_ratRoom.ForcePitfallForFliers = true;
			RoomHandler ratRoom = m_ratRoom;
			ratRoom.OnTargetPitfallRoom = (Action)Delegate.Combine(ratRoom.OnTargetPitfallRoom, new Action(PitfallIntoMetalGearRatRoom));
			RoomHandler ratRoom2 = m_ratRoom;
			ratRoom2.OnDarkSoulsReset = (Action)Delegate.Combine(ratRoom2.OnDarkSoulsReset, new Action(OnDarkSoulsReset));
			metalGearRatRoom.AddDarkSoulsRoomResetDependency(m_ratRoom);
		}
		EnablePitfalls(false);
	}

	public void Update()
	{
		if (!GameManager.HasInstance || !m_isRoomSealed)
		{
			return;
		}
		PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
		if ((bool)bestActivePlayer)
		{
			CellArea area = m_ratRoom.area;
			Vector2 unitCenter = bestActivePlayer.specRigidbody.GetUnitCenter(ColliderType.Ground);
			Vector2 min = area.UnitBottomLeft + new Vector2(-2f, -8f);
			Vector2 max = area.UnitTopRight + new Vector2(2f, 2f);
			if (!unitCenter.IsWithin(min, max))
			{
				SpecialSealRoom(false);
			}
		}
	}

	public void OpenGrate()
	{
		SpecialSealRoom(true);
		grateAnimator.Play("rat_grate");
	}

	public void OnDarkSoulsReset()
	{
		SpecialSealRoom(false);
		grateAnimator.StopAndResetFrameToDefault();
		EnablePitfalls(false);
	}

	private void SpecialSealRoom(bool seal)
	{
		m_isRoomSealed = seal;
		m_ratRoom.npcSealState = (seal ? RoomHandler.NPCSealState.SealAll : RoomHandler.NPCSealState.SealNone);
		foreach (DungeonDoorController connectedDoor in m_ratRoom.connectedDoors)
		{
			connectedDoor.KeepBossDoorSealed = seal;
		}
	}

	public void EnablePitfalls(bool value)
	{
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		IntVector2 intVector = absoluteRoom.area.basePosition + new IntVector2(16, 10);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				IntVector2 key = intVector + new IntVector2(i, j);
				GameManager.Instance.Dungeon.data[key].fallingPrevented = !value;
			}
		}
	}

	private void PitfallIntoMetalGearRatRoom()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			playerController.IsOnFire = false;
			playerController.CurrentFireMeterValue = 0f;
			playerController.CurrentPoisonMeterValue = 0f;
		}
		SpecialSealRoom(false);
		GameManager.Instance.StartCoroutine(DoMetalGearPitfall());
	}

	private IEnumerator DoMetalGearPitfall()
	{
		BossKillCam extantCam = UnityEngine.Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		int numPlayers = GameManager.Instance.AllPlayers.Length;
		AIActor metalGearRat = StaticReferenceManager.AllEnemies.Find((AIActor e) => (bool)e.healthHaver && e.healthHaver.IsBoss && (bool)e.GetComponent<MetalGearRatIntroDoer>());
		if (metalGearRat == null)
		{
			yield break;
		}
		metalGearRat.GetComponent<GenericIntroDoer>().triggerType = GenericIntroDoer.TriggerType.BossTriggerZone;
		metalGearRat.visibilityManager.SuppressPlayerEnteredRoom = true;
		metalGearRat.aiAnimator.PlayUntilCancelled("intro_idle");
		CameraController camera = GameManager.Instance.MainCameraController;
		camera.SetZoomScaleImmediate(0.66f);
		camera.LockToRoom = true;
		for (int i = 0; i < numPlayers; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("lich transition");
		}
		metalGearRat.specRigidbody.Initialize();
		Vector2 idealCameraPosition = metalGearRat.GetComponent<GenericIntroDoer>().BossCenter;
		camera.SetManualControl(true, false);
		camera.OverridePosition = idealCameraPosition + new Vector2(0f, 4f);
		Minimap.Instance.TemporarilyPreventMinimap = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		metalGearRat.GetComponent<MetalGearRatIntroDoer>().Init();
		Pixelator.Instance.FadeToBlack(0.5f, true);
		metalGearRat.visibilityManager.ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			playerController.DoSpinfallSpawn(3f);
			playerController.WarpFollowersToPlayer();
		}
		float timer = 0f;
		float duration = 3f;
		while (timer < duration)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			if ((bool)camera)
			{
				camera.OverridePosition = idealCameraPosition + new Vector2(0f, Mathf.SmoothStep(4f, 0f, timer / duration));
			}
		}
		yield return new WaitForSeconds(2.5f);
		if (GameManager.HasInstance)
		{
			for (int k = 0; k < numPlayers; k++)
			{
				GameManager.Instance.AllPlayers[k].ClearInputOverride("lich transition");
			}
		}
		GenericIntroDoer metalGearIntroDoer = metalGearRat.GetComponent<GenericIntroDoer>();
		metalGearIntroDoer.SkipWalkIn();
		if ((bool)metalGearRat)
		{
			metalGearIntroDoer.TriggerSequence(GameManager.Instance.BestActivePlayer);
		}
	}
}
