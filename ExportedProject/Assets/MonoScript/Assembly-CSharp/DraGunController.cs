using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DraGunController : BraveBehaviour
{
	public bool isAdvanced;

	public DraGunHeadController head;

	public GameObject neck;

	public GameObject wings;

	public GameObject headShootPoint;

	public GameObject leftArm;

	public GameObject rightArm;

	public float NearDeathTriggerHealth = 150f;

	public GameObject skyRocket;

	public GameObject skyBoulder;

	public AIActor AdvancedDraGunPrefab;

	[NonSerialized]
	public Vector2 SpotlightVelocity;

	[NonSerialized]
	public float SpotlightRadius = 3f;

	public float SpotlightShrink;

	public GameObject SpotlightSprite;

	[Header("For Brents")]
	public Material SpotlightMaterial;

	public Material PitCausticsMaterial;

	private float m_elapsedFlap;

	private bool m_isFlapping;

	private float m_babyCheckTimer;

	private AdditionalBraveLight m_spotlight;

	private GameObject m_spotlightSprite;

	private float m_elapsedSpotlight;

	private bool m_isMotionRestricted;

	private tk2dSpriteAnimator m_wingsAnimator;

	private EmbersController m_embers;

	private tk2dSpriteAnimator m_transitionDummy;

	private bool m_hasDoneIntro;

	private int m_minPlayerY;

	private int m_maxPlayerY;

	public float? OverrideTargetX { get; set; }

	public bool TrackPlayerWithHead { get; set; }

	public bool IsNearDeath { get; set; }

	public bool IsTransitioning { get; set; }

	public bool SpotlightEnabled { get; set; }

	public Vector2 SpotlightPos { get; set; }

	public float SpotlightSpeed { get; set; }

	public float SpotlightSmoothTime { get; set; }

	public bool HasDoneIntro
	{
		get
		{
			return m_hasDoneIntro;
		}
		set
		{
			if (!m_hasDoneIntro && value)
			{
				RestrictMotion(true);
			}
			m_hasDoneIntro = value;
		}
	}

	public bool HasConvertedBaby { get; set; }

	public void Start()
	{
		TrackPlayerWithHead = true;
		base.specRigidbody.Initialize();
		float unitBottom = base.specRigidbody.PrimaryPixelCollider.UnitBottom;
		TileSpriteClipper[] componentsInChildren = GetComponentsInChildren<TileSpriteClipper>(true);
		foreach (TileSpriteClipper tileSpriteClipper in componentsInChildren)
		{
			tileSpriteClipper.clipMode = TileSpriteClipper.ClipMode.ClipBelowY;
			tileSpriteClipper.clipY = unitBottom;
		}
		if (!isAdvanced)
		{
			m_transitionDummy = base.transform.Find("TransitionDummy").GetComponent<tk2dSpriteAnimator>();
			base.healthHaver.minimumHealth = NearDeathTriggerHealth;
			base.healthHaver.OnPreDeath += OnPreDeath;
		}
		if ((bool)wings && (bool)wings.GetComponent<tk2dSpriteAnimator>())
		{
			m_wingsAnimator = wings.GetComponent<tk2dSpriteAnimator>();
		}
		TrailRenderer[] componentsInChildren2 = head.GetComponentsInChildren<TrailRenderer>(true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].sortingLayerName = "Foreground";
		}
	}

	private void HandleFlaps()
	{
		if (GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.HIGH)
		{
			return;
		}
		if (!m_embers)
		{
			m_embers = GlobalSparksDoer.GetEmbersController();
		}
		if (m_isFlapping)
		{
			m_elapsedFlap += BraveTime.DeltaTime * 2f;
		}
		if (!m_isFlapping)
		{
			m_elapsedFlap -= BraveTime.DeltaTime / 3f;
		}
		m_elapsedFlap = Mathf.Clamp01(m_elapsedFlap);
		if (m_elapsedFlap <= 0f)
		{
			m_embers.AdditionalVortices.Clear();
			return;
		}
		Vector4 vector = new Vector4(wings.transform.position.x + 6f, wings.transform.position.y + 5f, 15f * m_elapsedFlap, -11f * m_elapsedFlap);
		Vector4 vector2 = new Vector4(wings.transform.position.x + 22f, wings.transform.position.y + 5f, 15f * m_elapsedFlap, 11f * m_elapsedFlap);
		if (m_embers.AdditionalVortices.Count < 1)
		{
			m_embers.AdditionalVortices.Add(vector);
		}
		else
		{
			m_embers.AdditionalVortices[0] = vector;
		}
		if (m_embers.AdditionalVortices.Count < 2)
		{
			m_embers.AdditionalVortices.Add(vector2);
		}
		else
		{
			m_embers.AdditionalVortices[1] = vector2;
		}
	}

	public void Update()
	{
		if (OverrideTargetX.HasValue)
		{
			head.TargetX = base.specRigidbody.HitboxPixelCollider.UnitCenter.x + OverrideTargetX.Value;
		}
		else if ((bool)base.aiActor.TargetRigidbody)
		{
			head.TargetX = base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox).x;
		}
		head.UpdateHead();
		if ((bool)m_wingsAnimator)
		{
			if (!IsTransitioning)
			{
				if (m_wingsAnimator.IsPlaying("wing_flap"))
				{
					m_isFlapping = true;
				}
				else
				{
					m_isFlapping = false;
				}
			}
			HandleFlaps();
		}
		if (!isAdvanced && !IsNearDeath && base.healthHaver.GetCurrentHealth() <= NearDeathTriggerHealth)
		{
			StartCoroutine(ConvertToNearDeath());
		}
		if (SpotlightEnabled)
		{
			m_elapsedSpotlight += BraveTime.DeltaTime;
			if ((bool)base.aiActor.TargetRigidbody)
			{
				Vector2 unitCenter = base.aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
				SpotlightPos = Vector2.SmoothDamp(SpotlightPos, unitCenter, ref SpotlightVelocity, SpotlightSmoothTime, SpotlightSpeed, BraveTime.DeltaTime);
			}
			Vector2 vector = headShootPoint.transform.position;
			float num = (SpotlightPos - vector).ToAngle();
			if (!m_spotlight)
			{
				GameObject gameObject = new GameObject("dragunSpotlight");
				m_spotlight = gameObject.AddComponent<AdditionalBraveLight>();
				m_spotlight.CustomLightMaterial = SpotlightMaterial;
				m_spotlight.UsesCustomMaterial = true;
				m_spotlight.LightColor = new Color(1f, 0.286274523f, 0.419607848f);
				m_spotlightSprite = UnityEngine.Object.Instantiate(SpotlightSprite);
				m_spotlightSprite.transform.parent = gameObject.transform;
				m_spotlightSprite.transform.localPosition = Vector3.zero;
			}
			else if (!m_spotlight.gameObject.activeSelf)
			{
				m_spotlight.gameObject.SetActive(true);
			}
			float num2 = 5f;
			float b = ((GameManager.Options.LightingQuality != 0) ? 92 : 50);
			m_spotlight.LightIntensity = Mathf.Lerp(0f, b, Mathf.Clamp01(m_elapsedSpotlight / num2));
			m_spotlight.LightRadius = SpotlightRadius * 2f + 1.25f;
			m_spotlight.CustomLightMaterial.SetVector("_LightOrigin", new Vector4(vector.x, vector.y, 0f, 0f));
			m_spotlight.transform.position = SpotlightPos.ToVector3ZisY();
			m_spotlightSprite.transform.localScale = new Vector3(SpotlightShrink, SpotlightShrink, 1f);
		}
		else
		{
			m_elapsedSpotlight = 0f;
			if ((bool)m_spotlight && m_spotlight.gameObject.activeSelf)
			{
				m_spotlight.gameObject.SetActive(false);
			}
		}
		if (isAdvanced || !base.aiActor.HasBeenEngaged || HasConvertedBaby)
		{
			return;
		}
		m_babyCheckTimer -= BraveTime.DeltaTime;
		if (m_babyCheckTimer <= 0f)
		{
			BabyDragunController babyDragunController = UnityEngine.Object.FindObjectOfType<BabyDragunController>();
			if ((bool)babyDragunController)
			{
				babyDragunController.BecomeEnemy(this);
				HasConvertedBaby = true;
			}
			m_babyCheckTimer = 1f;
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)m_embers)
		{
			m_embers.AdditionalVortices.Clear();
		}
		if ((bool)PitCausticsMaterial)
		{
			PitCausticsMaterial.SetFloat("_LightCausticPower", 4f);
			PitCausticsMaterial.SetFloat("_ValueMaximum", 50f);
			PitCausticsMaterial.SetFloat("_ValueMinimum", 0f);
		}
		if ((bool)m_spotlight)
		{
			UnityEngine.Object.Destroy(m_spotlight.gameObject);
		}
		RestrictMotion(false);
		ModifyCamera(false);
		BlockPitTiles(false);
		SilencerInstance.s_MaxRadiusLimiter = null;
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath -= OnPreDeath;
		}
		base.OnDestroy();
	}

	private void OnPreDeath(Vector2 finalDirection)
	{
		RestrictMotion(false);
	}

	private void PlayerMovementRestrictor(SpeculativeRigidbody playerSpecRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		if (pixelOffset.y < prevPixelOffset.y)
		{
			int num = playerSpecRigidbody.PixelColliders[0].MinY + pixelOffset.y;
			if (num < m_minPlayerY)
			{
				validLocation = false;
			}
		}
		if (pixelOffset.y > prevPixelOffset.y)
		{
			int num2 = playerSpecRigidbody.PixelColliders[0].MaxY + pixelOffset.y;
			if (num2 >= m_maxPlayerY)
			{
				validLocation = false;
			}
		}
	}

	public void RestrictMotion(bool value)
	{
		if (m_isMotionRestricted == value)
		{
			return;
		}
		if (value)
		{
			if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach)
			{
				return;
			}
			m_minPlayerY = (base.aiActor.ParentRoom.area.basePosition.y + DraGunRoomPlaceable.HallHeight) * 16 + 8;
			m_maxPlayerY = (base.aiActor.ParentRoom.area.basePosition.y + base.aiActor.ParentRoom.area.dimensions.y - 1) * 16;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				SpeculativeRigidbody speculativeRigidbody = GameManager.Instance.AllPlayers[i].specRigidbody;
				speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
			}
		}
		else
		{
			if (!GameManager.HasInstance || GameManager.IsReturningToBreach)
			{
				return;
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController)
				{
					SpeculativeRigidbody speculativeRigidbody2 = playerController.specRigidbody;
					speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
				}
			}
		}
		m_isMotionRestricted = value;
	}

	public void ModifyCamera(bool value)
	{
		if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach)
		{
			return;
		}
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		if ((bool)mainCameraController)
		{
			if (value)
			{
				mainCameraController.OverrideZoomScale = 0.75f;
				mainCameraController.LockToRoom = true;
				mainCameraController.AddFocusPoint(head.gameObject);
			}
			else
			{
				mainCameraController.OverrideZoomScale = 1f;
				mainCameraController.LockToRoom = false;
				mainCameraController.RemoveFocusPoint(head.gameObject);
			}
		}
	}

	public void BlockPitTiles(bool value)
	{
		if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach || GameManager.Instance.Dungeon == null)
		{
			return;
		}
		IntVector2 basePosition = base.aiActor.ParentRoom.area.basePosition;
		IntVector2 intVector = base.aiActor.ParentRoom.area.basePosition + base.aiActor.ParentRoom.area.dimensions - IntVector2.One;
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = basePosition.x; i <= intVector.x; i++)
		{
			for (int j = basePosition.y; j <= intVector.y; j++)
			{
				CellData cellData = data[i, j];
				if (cellData != null && cellData.type == CellType.PIT)
				{
					cellData.IsPlayerInaccessible = value;
				}
			}
		}
	}

	public bool MaybeConvertToGold()
	{
		if (HasConvertedBaby)
		{
			StartCoroutine(ConvertToGold());
			return true;
		}
		return false;
	}

	private IEnumerator ConvertToNearDeath()
	{
		base.healthHaver.IsVulnerable = false;
		IsNearDeath = true;
		Pixelator.Instance.FadeToColor(0.01f, Color.white);
		base.behaviorSpeculator.InterruptAndDisable();
		StaticReferenceManager.DestroyAllEnemyProjectiles();
		yield return null;
		StaticReferenceManager.DestroyAllEnemyProjectiles();
		List<AIActor> enemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].name.Contains("knife", true))
			{
				enemies[i].healthHaver.ApplyDamage(1000f, Vector2.zero, "Dragun Near-Death", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
			}
		}
		GameManager.Instance.PreventPausing = true;
		yield return new WaitForSeconds(0.5f);
		StaticReferenceManager.DestroyAllEnemyProjectiles();
		base.aiActor.ToggleRenderers(false);
		head.OverrideDesiredPosition = base.transform.position + new Vector3(3.63f, 10.8f);
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		GameManager.Instance.DungeonMusicController.SwitchToDragunTwo();
		m_transitionDummy.gameObject.SetActive(true);
		m_transitionDummy.GetComponent<Renderer>().enabled = true;
		m_transitionDummy.Play("hit_react");
		while (m_transitionDummy.IsPlaying("hit_react"))
		{
			yield return null;
		}
		float roarElapsed = 0f;
		IsTransitioning = true;
		m_transitionDummy.Play("roar");
		base.aiActor.GetComponent<DragunCracktonMap>().ConvertToCrackton();
		while (m_transitionDummy.IsPlaying("roar"))
		{
			roarElapsed += GameManager.INVARIANT_DELTA_TIME;
			if (roarElapsed > 0.5f)
			{
				m_isFlapping = true;
			}
			yield return null;
		}
		m_isFlapping = false;
		IsTransitioning = false;
		m_transitionDummy.Play("blank");
		m_transitionDummy.gameObject.SetActive(false);
		base.aiActor.ToggleRenderers(true);
		head.OverrideDesiredPosition = null;
		base.healthHaver.minimumHealth = 0f;
		base.healthHaver.DamageableColliders = new List<PixelCollider>();
		base.healthHaver.DamageableColliders.Add(base.aiActor.specRigidbody.PixelColliders[1]);
		base.behaviorSpeculator.enabled = true;
		GameManager.Instance.PreventPausing = false;
	}

	private IEnumerator ConvertToGold()
	{
		base.healthHaver.IsVulnerable = false;
		base.aiAnimator.PlayVfx("heart_heal");
		if (GameManager.HasInstance && (bool)GameManager.Instance.MainCameraController)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.OverridePosition = base.specRigidbody.UnitCenter + new Vector2(0f, 4f);
			mainCameraController.SetManualControl(true);
		}
		GameUIRoot.Instance.HideCoreUI("dragun_transition");
		GameUIRoot.Instance.ToggleLowerPanels(false, false, "dragun_transition");
		yield return new WaitForSeconds(3f);
		base.behaviorSpeculator.InterruptAndDisable();
		StaticReferenceManager.DestroyAllEnemyProjectiles();
		yield return null;
		StaticReferenceManager.DestroyAllEnemyProjectiles();
		List<AIActor> enemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].name.Contains("knife", true))
			{
				enemies[i].healthHaver.ApplyDamage(1000f, Vector2.zero, "Dragun Near-Death", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
			}
		}
		GameManager.Instance.PreventPausing = true;
		StaticReferenceManager.DestroyAllEnemyProjectiles();
		base.aiActor.ToggleRenderers(false);
		head.OverrideDesiredPosition = base.transform.position + new Vector3(3.63f, 10.8f);
		GameManager.Instance.DungeonMusicController.SwitchToDragunTwo();
		GameManager.Instance.PreventPausing = false;
		base.aiAnimator.StopVfx("heart_heal");
		AIActor advancedDraGun = AIActor.Spawn(AdvancedDraGunPrefab, base.specRigidbody.UnitBottomLeft, base.aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Default, false);
		advancedDraGun.transform.position = base.transform.position;
		advancedDraGun.specRigidbody.Reinitialize();
		base.healthHaver.EndBossState(false);
		UnityEngine.Object.Destroy(base.gameObject);
		GameUIRoot.Instance.ShowCoreUI("dragun_transition");
		GameUIRoot.Instance.ToggleLowerPanels(true, true, "dragun_transition");
		advancedDraGun.GetComponent<ObjectVisibilityManager>().ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
		advancedDraGun.GetComponent<GenericIntroDoer>().TriggerSequence(GameManager.Instance.BestActivePlayer);
		advancedDraGun.GetComponent<DragunCracktonMap>().PreGold();
		if (HasConvertedBaby)
		{
			BabyDragunController babyDragunController = UnityEngine.Object.FindObjectOfType<BabyDragunController>();
			if ((bool)babyDragunController)
			{
				UnityEngine.Object.Destroy(babyDragunController.gameObject);
			}
		}
	}

	public void HandleDarkRoomEffects(bool enabling, float duration)
	{
		StartCoroutine(HandleDarkRoomEffectsCR(enabling, duration));
	}

	private IEnumerator HandleDarkRoomEffectsCR(bool enabling, float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t2 = Mathf.Clamp01(elapsed / duration);
			if (!enabling)
			{
				t2 = 1f - t2;
			}
			t2 = Mathf.Pow(t2, 4f);
			PitCausticsMaterial.SetFloat("_LightCausticPower", Mathf.Lerp(4f, 80f, t2));
			PitCausticsMaterial.SetFloat("_ValueMaximum", Mathf.Lerp(50f, 5000f, t2));
			PitCausticsMaterial.SetFloat("_ValueMinimum", Mathf.Lerp(0f, 0.5f, t2));
			yield return null;
		}
		PitCausticsMaterial.SetFloat("_LightCausticPower", (!enabling) ? 4 : 80);
		PitCausticsMaterial.SetFloat("_ValueMaximum", (!enabling) ? 50 : 5000);
		PitCausticsMaterial.SetFloat("_ValueMinimum", (!enabling) ? 0f : 0.5f);
	}
}
