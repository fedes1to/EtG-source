using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class MegalichDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public GameObject shellCasing;

	private InfinilichDeathController m_infinilich;

	private bool m_challengesSuppressed;

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
		List<AIActor> allActors = StaticReferenceManager.AllEnemies;
		for (int i = 0; i < allActors.Count; i++)
		{
			if ((bool)allActors[i])
			{
				InfinilichDeathController component = allActors[i].GetComponent<InfinilichDeathController>();
				if ((bool)component)
				{
					m_infinilich = component;
					break;
				}
			}
		}
		RoomHandler megalichRoom = base.aiActor.ParentRoom;
		RoomHandler infinilichRoom = m_infinilich.aiActor.ParentRoom;
		infinilichRoom.AddDarkSoulsRoomResetDependency(megalichRoom);
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 3.5f;
	}

	protected override void OnDestroy()
	{
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE && m_challengesSuppressed)
		{
			ChallengeManager.Instance.SuppressChallengeStart = false;
			m_challengesSuppressed = false;
		}
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.aiAnimator.PlayUntilCancelled("death");
		base.aiAnimator.StopVfx("double_pound");
		base.aiAnimator.StopVfx("left_pound");
		base.aiAnimator.StopVfx("right_pound");
		GameManager.Instance.StartCoroutine(OnDeathExplosionsCR());
		GameManager.Instance.StartCoroutine(OnDeathCR());
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		for (int i = 0; i < explosionCount; i++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.2f, 0.2f));
			GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(explosionMidDelay);
		}
	}

	private bool IsAnyPlayerFalling()
	{
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			if ((bool)playerController && playerController.healthHaver.IsAlive && playerController.IsFalling)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator OnDeathCR()
	{
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.ForceStop();
		}
		SuperReaperController.PreventShooting = true;
		yield return new WaitForSeconds(2f);
		while (IsAnyPlayerFalling())
		{
			yield return null;
		}
		Pixelator.Instance.FadeToColor(0.75f, Color.white);
		Minimap.Instance.TemporarilyPreventMinimap = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		yield return new WaitForSeconds(3f);
		MegalichIntroDoer introDoer = GetComponent<MegalichIntroDoer>();
		introDoer.ModifyCamera(false);
		introDoer.BlockPitTiles(false);
		yield return new WaitForSeconds(0.75f);
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		if ((bool)base.aiActor)
		{
			Object.Destroy(base.aiActor);
		}
		if ((bool)base.healthHaver)
		{
			Object.Destroy(base.healthHaver);
		}
		if ((bool)base.behaviorSpeculator)
		{
			Object.Destroy(base.behaviorSpeculator);
		}
		if ((bool)base.aiAnimator.ChildAnimator)
		{
			Object.Destroy(base.aiAnimator.ChildAnimator.gameObject);
		}
		if ((bool)base.aiAnimator)
		{
			Object.Destroy(base.aiAnimator);
		}
		if ((bool)base.specRigidbody)
		{
			Object.Destroy(base.specRigidbody);
		}
		RegenerateCache();
		Minimap.Instance.TemporarilyPreventMinimap = true;
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.SuppressChallengeStart = true;
			m_challengesSuppressed = true;
		}
		AIActor infinilich = m_infinilich.GetComponent<AIActor>();
		RoomHandler infinilichRoom = GameManager.Instance.Dungeon.data.rooms.Find((RoomHandler r) => r.GetRoomName() == "LichRoom03");
		int numPlayers = GameManager.Instance.AllPlayers.Length;
		infinilich.visibilityManager.SuppressPlayerEnteredRoom = true;
		for (int i = 0; i < numPlayers; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("lich transition");
		}
		while (IsAnyPlayerFalling())
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		TimeTubeCreditsController.AcquireTunnelInstanceInAdvance();
		TimeTubeCreditsController.AcquirePastDioramaInAdvance();
		yield return null;
		PlayerController player = GameManager.Instance.PrimaryPlayer;
		Vector2 targetPoint = infinilichRoom.area.Center + new Vector2(0f, -5f);
		if ((bool)player)
		{
			player.WarpToPoint(targetPoint);
			player.DoSpinfallSpawn(0.5f);
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
			if ((bool)otherPlayer)
			{
				otherPlayer.ReuniteWithOtherPlayer(player);
				otherPlayer.DoSpinfallSpawn(0.5f);
			}
		}
		m_infinilich.GetComponent<InfinilichIntroDoer>().ModifyWorld(true);
		Vector2 idealCameraPosition = infinilich.GetComponent<GenericIntroDoer>().BossCenter;
		CameraController camera = GameManager.Instance.MainCameraController;
		camera.SetManualControl(true, false);
		camera.OverridePosition = idealCameraPosition;
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		yield return new WaitForSeconds(0.4f);
		Vector2 center = infinilich.specRigidbody.UnitCenter + new Vector2(0f, 10f);
		for (int j = 0; j < 150; j++)
		{
			SpawnShellCasingAtPosition(center + Random.insideUnitCircle.Scale(2f, 1f) * 5f);
		}
		yield return new WaitForSeconds(2f);
		for (int k = 0; k < numPlayers; k++)
		{
			GameManager.Instance.AllPlayers[k].ClearInputOverride("lich transition");
		}
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.SuppressChallengeStart = false;
			m_challengesSuppressed = false;
			ChallengeManager.Instance.EnteredCombat();
		}
		infinilich.visibilityManager.ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		Minimap.Instance.TemporarilyPreventMinimap = false;
		infinilich.GetComponent<GenericIntroDoer>().TriggerSequence(player);
		Object.Destroy(base.gameObject);
	}

	private void SpawnShellCasingAtPosition(Vector3 position)
	{
		if (shellCasing != null)
		{
			float num = Random.Range(-100f, -80f);
			GameObject gameObject = SpawnManager.SpawnDebris(shellCasing, position, Quaternion.Euler(0f, 0f, num));
			ShellCasing component = gameObject.GetComponent<ShellCasing>();
			if (component != null)
			{
				component.Trigger();
			}
			DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
			if (component2 != null)
			{
				Vector3 startingForce = BraveMathCollege.DegreesToVector(num, Random.Range(0.5f, 1f)).ToVector3ZUp(Random.value * 1.5f + 1f);
				component2.Trigger(startingForce, Random.Range(8f, 10f));
			}
		}
	}
}
