using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class LichDeathController : BraveBehaviour
{
	public GameObject HellDragVFX;

	public ScreenShakeSettings hellDragScreenShake;

	public ScreenShakeSettings dualLichShake1;

	public ScreenShakeSettings dualLichShake2;

	public GameObject explosionVfx;

	private float explosionMidDelay = 0.1f;

	private int explosionCount = 55;

	public GameObject bigExplosionVfx;

	private float bigExplosionMidDelay = 0.2f;

	private int bigExplosionCount = 15;

	private MegalichDeathController m_megalich;

	private bool m_challengesSuppressed;

	public bool IsDoubleFinalDeath { get; set; }

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
				MegalichDeathController component = allActors[i].GetComponent<MegalichDeathController>();
				if ((bool)component)
				{
					m_megalich = component;
					break;
				}
			}
		}
		RoomHandler lichRoom = base.aiActor.ParentRoom;
		RoomHandler megalichRoom = m_megalich.aiActor.ParentRoom;
		megalichRoom.AddDarkSoulsRoomResetDependency(lichRoom);
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
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
		if (LichIntroDoer.DoubleLich)
		{
			IsDoubleFinalDeath = true;
			foreach (AIActor activeEnemy in base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
			{
				if (activeEnemy.healthHaver.IsBoss && !activeEnemy.healthHaver.IsSubboss && activeEnemy != base.aiActor && activeEnemy.healthHaver.IsAlive)
				{
					IsDoubleFinalDeath = false;
					break;
				}
			}
		}
		if (!LichIntroDoer.DoubleLich || IsDoubleFinalDeath)
		{
			AkSoundEngine.PostEvent("Play_MUS_Lich_Transition_01", GameManager.Instance.gameObject);
		}
		if (IsDoubleFinalDeath)
		{
			base.aiAnimator.PlayUntilCancelled("death_real", true);
			base.healthHaver.OverrideKillCamTime = 11.5f;
			GameManager.Instance.StartCoroutine(HandleDoubleLichPostDeathCR());
			GameManager.Instance.StartCoroutine(HandleDoubleLichExtraExplosionsCR());
		}
		else
		{
			base.aiAnimator.PlayUntilCancelled("death", true);
			GameManager.Instance.StartCoroutine(HandlePostDeathCR());
		}
	}

	private IEnumerator HandlePostDeathCR()
	{
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.ForceStop();
		}
		tk2dBaseSprite shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		while (base.aiAnimator.IsPlaying("death"))
		{
			shadowSprite.color = shadowSprite.color.WithAlpha(1f - base.aiAnimator.CurrentClipProgress);
			float progress = base.aiAnimator.CurrentClipProgress;
			if (progress < 0.4f)
			{
				GlobalSparksDoer.DoRandomParticleBurst((int)(200f * Time.deltaTime), base.transform.position + new Vector3(4.5f, 4.5f), base.transform.position + new Vector3(5f, 5.5f), new Vector3(0f, 1f, 0f), 90f, 0.75f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			}
			yield return null;
		}
		base.renderer.enabled = false;
		shadowSprite.color = shadowSprite.color.WithAlpha(0f);
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
		if (LichIntroDoer.DoubleLich)
		{
			yield break;
		}
		yield return new WaitForSeconds(5f);
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(hellDragScreenShake, this);
		yield return new WaitForSeconds(3f);
		SuperReaperController.PreventShooting = true;
		yield return new WaitForSeconds(1f);
		List<HellDraggerArbitrary> arbitraryGrabbers = new List<HellDraggerArbitrary>();
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && playerController.healthHaver.IsAlive)
			{
				playerController.CurrentInputState = PlayerInputState.NoInput;
				HellDraggerArbitrary component = Object.Instantiate(HellDragVFX).GetComponent<HellDraggerArbitrary>();
				component.Do(playerController);
				arbitraryGrabbers.Add(component);
			}
		}
		yield return new WaitForSeconds(1f);
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		yield return new WaitForSeconds(1.5f);
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.SuppressChallengeStart = true;
			m_challengesSuppressed = true;
		}
		AIActor megalich = m_megalich.aiActor;
		RoomHandler megalichRoom = GameManager.Instance.Dungeon.data.rooms.Find((RoomHandler r) => r.GetRoomName() == "LichRoom02");
		int numPlayers = GameManager.Instance.AllPlayers.Length;
		megalich.visibilityManager.SuppressPlayerEnteredRoom = true;
		Pixelator.Instance.FadeToBlack(0.1f);
		yield return new WaitForSeconds(0.1f);
		for (int j = 0; j < arbitraryGrabbers.Count; j++)
		{
			Object.Destroy(arbitraryGrabbers[j].gameObject);
		}
		CameraController camera = GameManager.Instance.MainCameraController;
		camera.SetZoomScaleImmediate(0.75f);
		camera.LockToRoom = true;
		for (int k = 0; k < numPlayers; k++)
		{
			GameManager.Instance.AllPlayers[k].SetInputOverride("lich transition");
		}
		PlayerController player = GameManager.Instance.PrimaryPlayer;
		Vector2 targetPoint = megalichRoom.area.basePosition.ToVector2() + new Vector2(19.5f, 5f);
		if ((bool)player)
		{
			player.WarpToPoint(targetPoint);
			player.DoSpinfallSpawn(3f);
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
			if ((bool)otherPlayer)
			{
				otherPlayer.ReuniteWithOtherPlayer(player);
				otherPlayer.DoSpinfallSpawn(3f);
			}
		}
		Vector2 idealCameraPosition = megalich.GetComponent<GenericIntroDoer>().BossCenter;
		camera.SetManualControl(true, false);
		camera.OverridePosition = idealCameraPosition + new Vector2(0f, 4f);
		Pixelator.Instance.FadeToBlack(0.5f, true);
		float timer = 0f;
		float duration = 3f;
		while (timer < duration)
		{
			yield return null;
			timer += Time.deltaTime;
			camera.OverridePosition = idealCameraPosition + new Vector2(0f, Mathf.SmoothStep(4f, 0f, timer / duration));
		}
		yield return new WaitForSeconds(2.5f);
		for (int l = 0; l < numPlayers; l++)
		{
			GameManager.Instance.AllPlayers[l].ClearInputOverride("lich transition");
		}
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			ChallengeManager.Instance.SuppressChallengeStart = false;
			m_challengesSuppressed = false;
			ChallengeManager.Instance.EnteredCombat();
		}
		megalich.visibilityManager.ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		megalich.GetComponent<GenericIntroDoer>().TriggerSequence(player);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator HandleDoubleLichExtraExplosionsCR()
	{
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		for (int i = 0; i < explosionCount; i++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			GameObject vfxObj = SpawnManager.SpawnVFX(position: BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.2f, 0.2f)), prefab: explosionVfx, rotation: Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(explosionMidDelay);
		}
	}

	private IEnumerator HandleDoubleLichPostDeathCR()
	{
		SuperReaperController.PreventShooting = true;
		GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLINGER_PAST_KILLED, true);
		GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLINGER_UNLOCKED, true);
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(dualLichShake1, this);
		yield return new WaitForSeconds((float)explosionCount * explosionMidDelay - (float)bigExplosionCount * bigExplosionMidDelay);
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(dualLichShake2, this);
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		for (int i = 0; i < bigExplosionCount; i++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			GameObject vfxObj = SpawnManager.SpawnVFX(position: BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.2f, 0.2f)), prefab: bigExplosionVfx, rotation: Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(bigExplosionMidDelay);
		}
		Pixelator.Instance.DoMinimap = false;
		BossKillCam extantCam = Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		yield return new WaitForSeconds(1f);
		Vector2 lichCenter = base.aiAnimator.sprite.WorldCenter;
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		GameManager.Instance.MainCameraController.DoScreenShake(1.25f, 8f, 0.5f, 0.75f, null);
		GameObject gameObject = SpawnManager.SpawnVFX(bigExplosionVfx, collider.UnitCenter, Quaternion.identity);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.HeightOffGround = 0.8f;
		base.sprite.AttachRenderer(component);
		base.sprite.UpdateZDepth();
		yield return new WaitForSeconds(0.15f);
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
		Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
		yield return new WaitForSeconds(0.15f);
		for (int j = 0; j < StaticReferenceManager.AllDebris.Count; j++)
		{
			if ((bool)StaticReferenceManager.AllDebris[j])
			{
				Vector2 flatPoint = StaticReferenceManager.AllDebris[j].transform.position.XY();
				if (GameManager.Instance.MainCameraController.PointIsVisible(flatPoint))
				{
					StaticReferenceManager.AllDebris[j].TriggerDestruction();
				}
			}
		}
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		TimeTubeCreditsController ttcc = new TimeTubeCreditsController();
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.25f);
		yield return new WaitForSeconds(0.4f);
		yield return GameManager.Instance.StartCoroutine(ttcc.HandleTimeTubeCredits(lichCenter, false, null, -1, true));
		ttcc.CleanupLich();
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		BraveCameraUtility.OverrideAspect = 1.77777779f;
		yield return GameManager.Instance.StartCoroutine(HandlePastBeingShot());
	}

	private IEnumerator HandlePastBeingShot()
	{
		Minimap.Instance.TemporarilyPreventMinimap = true;
		Pixelator.Instance.LerpToLetterbox(1f, 2.5f);
		yield return new WaitForSeconds(7.5f);
		TitleDioramaController tdc = Object.FindObjectOfType<TitleDioramaController>();
		float elapsed = 0f;
		float duration = 10f;
		Transform targetXform = tdc.PastIslandSprite.transform.parent;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			tdc.SkyRenderer.material.SetFloat("_SkyBoost", Mathf.Lerp(0.88f, 0f, t));
			tdc.SkyRenderer.material.SetColor("_OverrideColor", Color.Lerp(new Color(1f, 0.55f, 0.2f, 1f), new Color(0.05f, 0.08f, 0.15f, 1f), t));
			tdc.SkyRenderer.material.SetFloat("_CurvePower", Mathf.Lerp(0.3f, -0.25f, t));
			tdc.SkyRenderer.material.SetFloat("_DitherCohesionFactor", Mathf.Lerp(0.3f, 1f, t));
			tdc.SkyRenderer.material.SetFloat("_StepValue", Mathf.Lerp(0.2f, 0.01f, t));
			targetXform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0f, -60f, 0f), BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t));
			yield return null;
		}
		AmmonomiconController.Instance.OpenAmmonomicon(true, true);
	}

	private IEnumerator HandleSplashBody(PlayerController sourcePlayer, bool isPrimaryPlayer, TitleDioramaController diorama)
	{
		AkSoundEngine.PostEvent("Play_CHR_forever_fall_01", GameManager.Instance.gameObject);
		if (!sourcePlayer.healthHaver.IsDead)
		{
			GameObject timefallCorpseInstance = (GameObject)Object.Instantiate(BraveResources.Load("Global Prefabs/TimefallCorpse"), sourcePlayer.sprite.transform.position, Quaternion.identity);
			timefallCorpseInstance.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
			tk2dSpriteAnimator targetTimefallAnimator = timefallCorpseInstance.GetComponent<tk2dSpriteAnimator>();
			SpriteOutlineManager.AddOutlineToSprite(targetTimefallAnimator.Sprite, Color.black);
			tk2dSpriteAnimation timefallSpecificLibrary = (targetTimefallAnimator.Library = (targetTimefallAnimator.Library = ((!(sourcePlayer is PlayerSpaceshipController)) ? sourcePlayer.sprite.spriteAnimator.Library : (sourcePlayer as PlayerSpaceshipController).TimefallCorpseLibrary)));
			tk2dSpriteAnimationClip clip = targetTimefallAnimator.GetClipByName("timefall");
			if (clip != null)
			{
				targetTimefallAnimator.Play("timefall");
			}
			float elapsed = 0f;
			float duration = 3f;
			while (elapsed < duration)
			{
				elapsed += BraveTime.DeltaTime;
				Vector3 startPoint = diorama.VFX_Splash.transform.position + new Vector3(-8f, 40f, 0f);
				Vector3 endPoint = diorama.VFX_Splash.GetComponent<tk2dBaseSprite>().WorldCenter.ToVector3ZUp(startPoint.z);
				targetTimefallAnimator.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(elapsed / duration));
				timefallCorpseInstance.transform.localScale = Vector3.Lerp(Vector3.one * 1.25f, new Vector3(0.125f, 0.125f, 0.125f), Mathf.Clamp01(elapsed / duration));
				yield return null;
			}
			AkSoundEngine.PostEvent("Play_CHR_final_splash_01", GameManager.Instance.gameObject);
			diorama.VFX_Splash.SetActive(true);
			diorama.VFX_Splash.GetComponent<tk2dSpriteAnimator>().PlayAndDisableObject(string.Empty);
			diorama.VFX_Splash.GetComponent<tk2dSprite>().UpdateZDepth();
			Object.Destroy(timefallCorpseInstance);
		}
	}
}
