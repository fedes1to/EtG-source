using System;
using System.Collections;
using UnityEngine;

public class AdvancedDraGunDeathController : BraveBehaviour
{
	public GameObject fingerDebris;

	public GameObject neckDebris;

	private DraGunController m_dragunController;

	private tk2dSpriteAnimator m_roarDummy;

	public void Awake()
	{
		m_dragunController = GetComponent<DraGunController>();
		m_roarDummy = base.aiActor.transform.Find("RoarDummy").GetComponent<tk2dSpriteAnimator>();
	}

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 16.5f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.behaviorSpeculator.enabled = false;
		GameManager.Instance.StartCoroutine(OnDeathExplosionsCR());
	}

	private IEnumerator OnDeathExplosionsCR()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("DraGunDeathController");
		}
		GameManager.Instance.PreventPausing = true;
		GameUIRoot.Instance.HideCoreUI("dragun");
		GameUIRoot.Instance.ToggleLowerPanels(false, false, "dragun");
		Pixelator.Instance.FadeToColor(0.5f, Color.white, true);
		base.healthHaver.OverrideKillCamPos = base.specRigidbody.UnitCenter + new Vector2(0f, 6f);
		base.aiActor.ToggleRenderers(false);
		m_dragunController.head.OverrideDesiredPosition = base.aiActor.transform.position + new Vector3(3.63f, 11.8f);
		m_roarDummy.gameObject.SetActive(true);
		m_roarDummy.GetComponent<Renderer>().enabled = true;
		m_roarDummy.sprite.usesOverrideMaterial = false;
		m_roarDummy.Play("death");
		base.aiAnimator.PlayVfx("roar_shake");
		while (m_roarDummy.IsPlaying("death"))
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		GameManager.Instance.PreventPausing = true;
		Animation leftArm = m_dragunController.leftArm.GetComponent<Animation>();
		AIAnimator leftHand = m_dragunController.leftArm.transform.Find("LeftHand").GetComponent<AIAnimator>();
		Animation rightArm = m_dragunController.rightArm.GetComponent<Animation>();
		AIAnimator rightHand = m_dragunController.rightArm.transform.Find("RightHand").GetComponent<AIAnimator>();
		AIAnimator head = m_dragunController.head.aiAnimator;
		Renderer[] componentsInChildren = m_dragunController.leftArm.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = true;
		}
		leftArm.Play("DraGunLeftAdvancedDeath");
		leftHand.spriteAnimator.enabled = true;
		leftHand.PlayUntilCancelled("predeath");
		Renderer[] componentsInChildren2 = m_dragunController.rightArm.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer2 in componentsInChildren2)
		{
			renderer2.enabled = true;
		}
		rightArm.Play("DraGunRightAdvancedDeath");
		rightHand.spriteAnimator.enabled = true;
		rightHand.PlayUntilCancelled("predeath");
		m_dragunController.head.renderer.enabled = true;
		head.spriteAnimator.enabled = true;
		head.PlayUntilCancelled("predeath");
		head.LockFacingDirection = true;
		m_roarDummy.sprite.SetSprite("dragun_gold_death_body_001");
		leftArm.transform.Find("LeftArm (5)").GetComponentInChildren<Renderer>().enabled = false;
		leftArm.transform.Find("LeftArm (6)").GetComponentInChildren<Renderer>().enabled = false;
		rightArm.transform.Find("RightArm (5)").GetComponentInChildren<Renderer>().enabled = false;
		rightArm.transform.Find("RightArm (6)").GetComponentInChildren<Renderer>().enabled = false;
		StartCoroutine(ExplodeHand(leftHand, 180f));
		yield return new WaitForSeconds(2f);
		StartCoroutine(ExplodeHand(rightHand, 0f));
		yield return new WaitForSeconds(1.25f);
		StartCoroutine(ExplodeBall(leftArm, "LeftArm (4)", 180f, 0.5f));
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(ExplodeBall(rightArm, "RightArm (4)", 0f, 0.5f));
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(ExplodeBall(leftArm, "LeftArm (3)", 180f, 0.4f));
		yield return new WaitForSeconds(0.4f);
		StartCoroutine(ExplodeBall(rightArm, "RightArm (3)", 0f, 0.4f));
		yield return new WaitForSeconds(0.4f);
		StartCoroutine(ExplodeBall(leftArm, "LeftArm (2)", 180f, 0.3f));
		yield return new WaitForSeconds(0.3f);
		StartCoroutine(ExplodeBall(rightArm, "RightArm (2)", 0f, 0.3f));
		yield return new WaitForSeconds(0.3f);
		StartCoroutine(ExplodeBall(leftArm, "LeftArm (1)", 180f, 0.2f));
		yield return new WaitForSeconds(0.2f);
		StartCoroutine(ExplodeBall(rightArm, "RightArm (1)", 0f, 0.9f));
		yield return new WaitForSeconds(0.9f);
		StartCoroutine(ExplodeShoulder(leftArm, "LeftShoulder", 180f, 0.9f));
		yield return new WaitForSeconds(0.9f);
		StartCoroutine(ExplodeShoulder(rightArm, "RightShoulder", -90f, 1f));
		yield return new WaitForSeconds(1f);
		head.sprite.usesOverrideMaterial = false;
		head.PlayUntilCancelled("death");
		Vector2 shardPos = head.sprite.WorldCenter;
		yield return new WaitForSeconds(0.7f);
		m_roarDummy.Play("death_body_head_explode");
		base.aiAnimator.PlayVfx("death_big_shake");
		SpawnShardsOnDeath[] componentsInChildren3 = m_dragunController.head.GetComponentsInChildren<SpawnShardsOnDeath>();
		foreach (SpawnShardsOnDeath spawnShardsOnDeath in componentsInChildren3)
		{
			spawnShardsOnDeath.HandleShardSpawns(Vector2.zero, shardPos);
		}
		yield return new WaitForSeconds(1.67f);
		base.aiAnimator.PlayVfx("death_slow_shake");
		yield return new WaitForSeconds(0.66f);
		StartCoroutine(FadeBodyCR(1.33f));
		yield return new WaitForSeconds(0.67f);
		head.renderer.enabled = false;
		Pixelator.Instance.FadeToColor(0.5f, Color.white);
		yield return new WaitForSeconds(0.75f);
		m_dragunController.ModifyCamera(false);
		m_dragunController.BlockPitTiles(false);
		yield return new WaitForSeconds(0.75f);
		m_dragunController.IsTransitioning = false;
		SpawnBones(fingerDebris, UnityEngine.Random.Range(3, 6), new Vector2(2f, 4f), new Vector3(-24f, -15f));
		SpawnBones(fingerDebris, UnityEngine.Random.Range(3, 6), new Vector2(24f, 4f), new Vector3(-2f, -15f));
		SpawnBones(neckDebris, UnityEngine.Random.Range(1, 3), new Vector2(2f, 4f), new Vector3(-24f, -15f));
		SpawnBones(neckDebris, UnityEngine.Random.Range(1, 3), new Vector2(24f, 4f), new Vector3(-2f, -15f));
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		DraGunRoomPlaceable dragunRoomController = base.aiActor.ParentRoom.GetComponentsAbsoluteInRoom<DraGunRoomPlaceable>()[0];
		dragunRoomController.DraGunKilled = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		UnityEngine.Object.Destroy(base.gameObject);
		yield return new WaitForSeconds(0.75f);
		dragunRoomController.ExtendDeathBridge();
		for (int m = 0; m < GameManager.Instance.AllPlayers.Length; m++)
		{
			GameManager.Instance.AllPlayers[m].ClearInputOverride("DraGunDeathController");
		}
		if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH)
		{
			GameUIRoot.Instance.ShowCoreUI("dragun");
			GameUIRoot.Instance.ToggleLowerPanels(true, false, "dragun");
		}
		yield return null;
		GameManager.Instance.PreventPausing = false;
		for (int n = 0; n < GameManager.Instance.AllPlayers.Length; n++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[n];
			if (!playerController || playerController.passiveItems == null)
			{
				continue;
			}
			for (int num = 0; num < playerController.passiveItems.Count; num++)
			{
				CompanionItem companionItem = playerController.passiveItems[num] as CompanionItem;
				if (!companionItem || !companionItem.ExtantCompanion)
				{
					continue;
				}
				GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_SUNLIGHT_SPEAR_UNLOCK, true);
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_DRAGUN_WITH_TURTLE))
				{
					CompanionController component = companionItem.ExtantCompanion.GetComponent<CompanionController>();
					if ((bool)component && component.companionID == CompanionController.CompanionIdentifier.SUPER_SPACE_TURTLE)
					{
						GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_DRAGUN_WITH_TURTLE, true);
					}
				}
			}
		}
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
		{
			GameManager.Instance.MainCameraController.SetManualControl(true);
			GameStatsManager.Instance.SetFlag(GungeonFlags.SHERPA_BOSSRUSH_COMPLETE, true);
			GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_BOSSRUSH, true);
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			for (int num2 = 0; num2 < GameManager.Instance.AllPlayers.Length; num2++)
			{
				GameManager.Instance.AllPlayers[num2].SetInputOverride("game complete");
			}
			Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
			AmmonomiconController.Instance.OpenAmmonomicon(true, true);
		}
		if (GameStatsManager.Instance.IsRainbowRun)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.BOWLER_RAINBOW_RUN_COMPLETE, true);
		}
	}

	private IEnumerator ExplodeHand(AIAnimator hand, float headDirection)
	{
		AIAnimator headAnimator = m_dragunController.head.aiAnimator;
		headAnimator.FacingDirection = headDirection;
		headAnimator.EndAnimation();
		headAnimator.PlayUntilCancelled("predeath");
		hand.sprite.usesOverrideMaterial = false;
		hand.PlayUntilCancelled("death");
		yield return new WaitForSeconds(0.75f);
		base.aiAnimator.PlayVfx("death_small_shake");
		SpawnShardsOnDeath[] componentsInChildren = hand.GetComponentsInChildren<SpawnShardsOnDeath>();
		foreach (SpawnShardsOnDeath spawnShardsOnDeath in componentsInChildren)
		{
			spawnShardsOnDeath.HandleShardSpawns(Vector2.zero, hand.sprite.WorldCenter);
		}
	}

	private IEnumerator ExplodeBall(Animation arm, string ballName, float headDirection, float postDelay)
	{
		tk2dSprite ballSprite = arm.transform.Find(ballName).GetComponentInChildren<tk2dSprite>();
		ballSprite.spriteAnimator.enabled = true;
		ballSprite.usesOverrideMaterial = false;
		ballSprite.spriteAnimator.Play("arm_death_explode");
		yield return new WaitForSeconds(0.47f);
		base.aiAnimator.PlayVfx("death_small_shake");
		AIAnimator headAnimator = m_dragunController.head.aiAnimator;
		headAnimator.FacingDirection = headDirection;
		headAnimator.EndAnimation();
		if (postDelay < 0.3f)
		{
			string text = "predeath";
			float warpClipDuration = postDelay - 0.05f;
			headAnimator.PlayUntilCancelled(text, false, null, warpClipDuration);
		}
		else
		{
			headAnimator.PlayUntilCancelled("predeath");
		}
	}

	private IEnumerator ExplodeShoulder(Animation arm, string ballName, float headDirection, float postDelay)
	{
		tk2dSprite ballSprite = arm.transform.Find(ballName).GetComponentInChildren<tk2dSprite>();
		ballSprite.spriteAnimator.enabled = true;
		ballSprite.usesOverrideMaterial = false;
		ballSprite.spriteAnimator.Play((!(headDirection > 0f)) ? "death_shoulder_left_explode" : "death_shoulder_right_explode");
		yield return new WaitForSeconds(0.42f);
		base.aiAnimator.PlayVfx("death_small_shake");
		AIAnimator headAnimator = m_dragunController.head.aiAnimator;
		headAnimator.FacingDirection = headDirection;
		headAnimator.EndAnimation();
		if (postDelay < 0.3f)
		{
			string text = "predeath";
			float warpClipDuration = postDelay - 0.05f;
			headAnimator.PlayUntilCancelled(text, false, null, warpClipDuration);
		}
		else
		{
			headAnimator.PlayUntilCancelled("predeath");
		}
	}

	private IEnumerator FadeBodyCR(float duration)
	{
		float timer = 0f;
		while (timer < duration)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			m_roarDummy.sprite.renderer.material.SetColor("_OverrideColor", new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, Mathf.Clamp01(timer / duration))));
		}
	}

	private void SpawnBones(GameObject bonePrefab, int count, Vector2 min, Vector2 max)
	{
		Vector2 min2 = base.aiActor.ParentRoom.area.basePosition.ToVector2() + min + new Vector2(0f, DraGunRoomPlaceable.HallHeight);
		Vector2 max2 = base.aiActor.ParentRoom.area.basePosition.ToVector2() + base.aiActor.ParentRoom.area.dimensions.ToVector2() + max;
		for (int i = 0; i < count; i++)
		{
			Vector2 vector = BraveUtility.RandomVector2(min2, max2);
			GameObject gameObject = SpawnManager.SpawnVFX(bonePrefab, vector, Quaternion.identity);
			if ((bool)gameObject)
			{
				gameObject.transform.parent = SpawnManager.Instance.VFX;
				DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
				orAddComponent.decayOnBounce = 0.5f;
				orAddComponent.bounceCount = 1;
				orAddComponent.canRotate = true;
				float angle = UnityEngine.Random.Range(-80f, -100f);
				Vector2 vector2 = BraveMathCollege.DegreesToVector(angle) * UnityEngine.Random.Range(0.1f, 3f);
				Vector3 startingForce = new Vector3(vector2.x, (!(vector2.y < 0f)) ? 0f : vector2.y, (!(vector2.y > 0f)) ? 0f : vector2.y);
				if ((bool)orAddComponent.minorBreakable)
				{
					orAddComponent.minorBreakable.enabled = true;
				}
				orAddComponent.Trigger(startingForce, UnityEngine.Random.Range(1f, 2f));
				if ((bool)orAddComponent.specRigidbody)
				{
					orAddComponent.OnGrounded = (Action<DebrisObject>)Delegate.Combine(orAddComponent.OnGrounded, new Action<DebrisObject>(HandleComplexDebris));
				}
			}
		}
	}

	private void HandleComplexDebris(DebrisObject debrisObject)
	{
		GameManager.Instance.StartCoroutine(DelayedSpriteFixer(debrisObject.sprite));
		SpeculativeRigidbody speculativeRigidbody = debrisObject.specRigidbody;
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(speculativeRigidbody);
		UnityEngine.Object.Destroy(debrisObject);
		speculativeRigidbody.RegenerateCache();
	}

	private IEnumerator DelayedSpriteFixer(tk2dBaseSprite sprite)
	{
		yield return null;
		sprite.HeightOffGround = -1f;
		sprite.IsPerpendicular = true;
		sprite.UpdateZDepth();
	}
}
