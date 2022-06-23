using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraGunDeathController : BraveBehaviour
{
	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public GameObject skullDebris;

	public GameObject fingerDebris;

	public GameObject neckDebris;

	private DraGunController m_dragunController;

	private tk2dSpriteAnimator m_deathDummy;

	public void Awake()
	{
		m_dragunController = GetComponent<DraGunController>();
		m_deathDummy = base.transform.Find("DeathDummy").GetComponent<tk2dSpriteAnimator>();
	}

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 6.25f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.behaviorSpeculator.enabled = false;
		GameManager.Instance.StartCoroutine(OnDeathExplosionsCR());
		AkSoundEngine.PostEvent("Play_BOSS_dragun_thunder_01", base.gameObject);
	}

	private IEnumerator LerpCrackEmission(float startVal, float targetVal, float duration)
	{
		Material targetMaterial = m_deathDummy.GetComponent<Renderer>().material;
		float ela = 0f;
		while (ela < duration)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t2 = ela / duration;
			t2 *= t2;
			targetMaterial.SetFloat("_CrackAmount", Mathf.Lerp(startVal, targetVal, t2));
			yield return null;
		}
	}

	private IEnumerator LerpCrackColor(Color targetVal, float duration)
	{
		Material targetMaterial = m_deathDummy.GetComponent<Renderer>().material;
		Color startVal = targetMaterial.GetColor("_CrackBaseColor");
		float ela = 0f;
		while (ela < duration)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t2 = ela / duration;
			t2 *= t2;
			targetMaterial.SetColor("_CrackBaseColor", Color.Lerp(startVal, targetVal, t2));
			yield return null;
		}
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
		base.healthHaver.OverrideKillCamPos = base.specRigidbody.UnitCenter + new Vector2(0f, 6f);
		base.aiAnimator.PlayUntilCancelled("heart_burst");
		while (base.aiAnimator.IsPlaying("heart_burst"))
		{
			yield return null;
		}
		base.aiAnimator.EndAnimationIf("heart_burst");
		base.aiAnimator.PlayVfx("heart_burst");
		Pixelator.Instance.FadeToColor(0.75f, Color.white, true);
		yield return new WaitForSeconds(0.3f);
		GameManager.Instance.PreventPausing = true;
		base.aiActor.ToggleRenderers(false);
		m_deathDummy.gameObject.SetActive(true);
		m_deathDummy.GetComponent<Renderer>().enabled = true;
		m_dragunController.IsTransitioning = true;
		m_deathDummy.Play("die");
		StartCoroutine(LerpCrackEmission(1f, 250f, 3f));
		yield return new WaitForSeconds(3f);
		GameManager.Instance.PreventPausing = true;
		StartCoroutine(LerpCrackColor(Color.white, 3f));
		StartCoroutine(LerpCrackEmission(250f, 50000f, 3f));
		yield return new WaitForSeconds(1.5f);
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
		SpawnBones(skullDebris, 1, new Vector2(8f, 6f), new Vector2(-22f, -23f));
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		DraGunRoomPlaceable dragunRoomController = base.aiActor.ParentRoom.GetComponentsAbsoluteInRoom<DraGunRoomPlaceable>()[0];
		dragunRoomController.DraGunKilled = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		UnityEngine.Object.Destroy(base.gameObject);
		yield return new WaitForSeconds(0.75f);
		dragunRoomController.ExtendDeathBridge();
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			GameManager.Instance.AllPlayers[j].ClearInputOverride("DraGunDeathController");
		}
		if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH)
		{
			GameUIRoot.Instance.ShowCoreUI("dragun");
			GameUIRoot.Instance.ToggleLowerPanels(true, false, "dragun");
		}
		yield return null;
		GameManager.Instance.PreventPausing = false;
		for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[k];
			if (!playerController || playerController.passiveItems == null)
			{
				continue;
			}
			for (int l = 0; l < playerController.passiveItems.Count; l++)
			{
				CompanionItem companionItem = playerController.passiveItems[l] as CompanionItem;
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
			for (int m = 0; m < GameManager.Instance.AllPlayers.Length; m++)
			{
				GameManager.Instance.AllPlayers[m].SetInputOverride("game complete");
			}
			Pixelator.Instance.FadeToColor(0.15f, Color.white, true, 0.15f);
			AmmonomiconController.Instance.OpenAmmonomicon(true, true);
		}
		if (GameStatsManager.Instance.IsRainbowRun)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.BOWLER_RAINBOW_RUN_COMPLETE, true);
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
