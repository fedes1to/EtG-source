using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BossKillCam : TimeInvariantMonoBehaviour
{
	public static bool BossDeathCamRunning;

	public float trackToBossTime = 0.75f;

	public float returnToPlayerTime = 1f;

	protected bool m_isRunning;

	protected Projectile m_projectile;

	protected SpeculativeRigidbody m_bossRigidbody;

	protected CameraController m_camera;

	protected Transform m_cameraTransform;

	protected float m_phaseCountdown;

	protected int m_currentPhase;

	protected bool m_phaseComplete = true;

	protected float m_targetTimeScale = 1f;

	protected bool m_suppressContinuousBulletDestruction;

	protected List<CutsceneMotion> activeMotions = new List<CutsceneMotion>();

	public static GatlingGullOutroDoer hackGatlingGullOutroDoer;

	public void ForceCancelSequence()
	{
		Debug.Log("force ending sequence");
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			playerController.healthHaver.IsVulnerable = true;
			playerController.gameActor.SuppressEffectUpdates = false;
			playerController.ClearInputOverride("bossKillCam");
		}
		m_targetTimeScale = 1f;
		BraveTime.ClearMultiplier(base.gameObject);
		StickyFrictionManager.Instance.FrictionEnabled = true;
		m_isRunning = false;
		BossDeathCamRunning = false;
		GameUIRoot.Instance.EndBossKillCam();
		Object.Destroy(this);
	}

	public void SetPhaseCountdown(float value)
	{
		m_phaseCountdown = value;
	}

	public void TriggerSequence(Projectile p, SpeculativeRigidbody bossSRB)
	{
		m_projectile = p;
		m_bossRigidbody = bossSRB;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			playerController.healthHaver.IsVulnerable = false;
			playerController.gameActor.SuppressEffectUpdates = true;
			playerController.SetInputOverride("bossKillCam");
			playerController.IsOnFire = false;
			playerController.CurrentFireMeterValue = 0f;
			playerController.CurrentPoisonMeterValue = 0f;
			DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(playerController.specRigidbody.UnitCenter, 1f);
			playerController.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(5f);
		}
		m_targetTimeScale = 0.2f;
		StickyFrictionManager.Instance.FrictionEnabled = false;
		m_camera = GameManager.Instance.MainCameraController;
		m_camera.StopTrackingPlayer();
		m_camera.SetManualControl(true, false);
		m_camera.OverridePosition = m_camera.transform.position;
		GenericIntroDoer component = bossSRB.GetComponent<GenericIntroDoer>();
		if ((bool)component && (bool)component.cameraFocus)
		{
			m_camera.RemoveFocusPoint(component.cameraFocus);
		}
		m_cameraTransform = m_camera.transform;
		m_isRunning = true;
		BossDeathCamRunning = true;
		if (m_projectile != null)
		{
			m_currentPhase = 0;
			return;
		}
		m_currentPhase = 1;
		Vector2? overrideKillCamPos = bossSRB.healthHaver.OverrideKillCamPos;
		Vector2 vector = ((!overrideKillCamPos.HasValue) ? bossSRB.UnitCenter : overrideKillCamPos.Value);
		m_suppressContinuousBulletDestruction = bossSRB.healthHaver.SuppressContinuousKillCamBulletDestruction;
		CutsceneMotion cutsceneMotion = new CutsceneMotion(m_cameraTransform, vector, Vector2.Distance(m_cameraTransform.position.XY(), vector) / trackToBossTime);
		cutsceneMotion.camera = m_camera;
		activeMotions.Add(cutsceneMotion);
		m_phaseComplete = false;
	}

	public static void ClearPerLevelData()
	{
		hackGatlingGullOutroDoer = null;
	}

	private void EndSequence()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			playerController.healthHaver.IsVulnerable = true;
			playerController.gameActor.SuppressEffectUpdates = false;
			playerController.ClearInputOverride("bossKillCam");
			playerController.IsOnFire = false;
			playerController.CurrentFireMeterValue = 0f;
			playerController.CurrentPoisonMeterValue = 0f;
			DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(playerController.specRigidbody.UnitCenter, 1f);
		}
		m_targetTimeScale = 1f;
		BraveTime.ClearMultiplier(base.gameObject);
		StickyFrictionManager.Instance.FrictionEnabled = true;
		m_camera.StartTrackingPlayer();
		m_camera.SetManualControl(false);
		m_isRunning = false;
		BossDeathCamRunning = false;
		GameUIRoot.Instance.EndBossKillCam();
		if (hackGatlingGullOutroDoer != null)
		{
			hackGatlingGullOutroDoer.TriggerSequence();
		}
		hackGatlingGullOutroDoer = null;
		Object.Destroy(this);
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (!m_isRunning)
		{
			return;
		}
		if (!m_suppressContinuousBulletDestruction)
		{
			StaticReferenceManager.DestroyAllEnemyProjectiles();
		}
		KillAllEnemies();
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			playerController.healthHaver.IsVulnerable = false;
			playerController.gameActor.SuppressEffectUpdates = true;
			playerController.SetInputOverride("bossKillCam");
			playerController.IsOnFire = false;
			playerController.CurrentFireMeterValue = 0f;
			playerController.CurrentPoisonMeterValue = 0f;
			DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(playerController.specRigidbody.UnitCenter, 1f);
		}
		if (Time.timeScale != m_targetTimeScale)
		{
			float max = ((!(m_targetTimeScale > Time.timeScale)) ? 1f : m_targetTimeScale);
			float min = ((!(m_targetTimeScale > Time.timeScale)) ? m_targetTimeScale : 0f);
			BraveTime.SetTimeScaleMultiplier(Mathf.Clamp(Time.timeScale + (m_targetTimeScale - Time.timeScale) * (realDeltaTime / 0.1f), min, max), base.gameObject);
		}
		for (int j = 0; j < activeMotions.Count; j++)
		{
			CutsceneMotion cutsceneMotion = activeMotions[j];
			Vector2? lerpEnd = cutsceneMotion.lerpEnd;
			Vector2 vector = ((!lerpEnd.HasValue) ? GameManager.Instance.MainCameraController.GetIdealCameraPosition() : cutsceneMotion.lerpEnd.Value);
			float num = Vector2.Distance(vector, cutsceneMotion.lerpStart);
			float num2 = cutsceneMotion.speed * realDeltaTime;
			float num3 = num2 / num;
			cutsceneMotion.lerpProgress = Mathf.Clamp01(cutsceneMotion.lerpProgress + num3);
			float t = cutsceneMotion.lerpProgress;
			if (cutsceneMotion.isSmoothStepped)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			Vector2 vector2 = Vector2.Lerp(cutsceneMotion.lerpStart, vector, t);
			if (cutsceneMotion.camera != null)
			{
				cutsceneMotion.camera.OverridePosition = vector2.ToVector3ZUp(cutsceneMotion.zOffset);
			}
			else
			{
				cutsceneMotion.transform.position = BraveUtility.QuantizeVector(vector2.ToVector3ZUp(cutsceneMotion.zOffset), PhysicsEngine.Instance.PixelsPerUnit);
			}
			if (cutsceneMotion.lerpProgress == 1f)
			{
				activeMotions.RemoveAt(j);
				j--;
				if (activeMotions.Count == 0)
				{
					m_currentPhase++;
					m_phaseComplete = true;
				}
			}
		}
		if (m_currentPhase == 0)
		{
			if (!m_bossRigidbody || m_bossRigidbody.healthHaver.IsDead)
			{
				m_currentPhase += 2;
				m_phaseComplete = true;
			}
			else
			{
				if (!m_projectile || !m_projectile.specRigidbody)
				{
					EndSequence();
					return;
				}
				m_camera.OverridePosition = m_projectile.specRigidbody.UnitCenter.ToVector3ZUp();
			}
		}
		else if (m_currentPhase == 2 && (bool)m_bossRigidbody && m_bossRigidbody.healthHaver.TrackDuringDeath)
		{
			GameManager.Instance.MainCameraController.OverridePosition = m_bossRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		if (m_currentPhase <= 2)
		{
			float magnitude = ((m_currentPhase >= 2) ? Mathf.Clamp01(m_phaseCountdown) : 1f);
			BraveInput.DoSustainedScreenShakeVibration(magnitude);
		}
		if (m_phaseComplete)
		{
			switch (m_currentPhase)
			{
			case 1:
				m_phaseComplete = false;
				break;
			case 2:
				m_targetTimeScale = 1f;
				if ((bool)m_bossRigidbody && (bool)m_bossRigidbody.healthHaver && m_bossRigidbody.healthHaver.OverrideKillCamTime.HasValue)
				{
					m_phaseCountdown = m_bossRigidbody.healthHaver.OverrideKillCamTime.Value;
				}
				else
				{
					m_phaseCountdown = 3f;
				}
				m_phaseComplete = false;
				break;
			case 3:
			{
				GameManager.Instance.MainCameraController.ForceUpdateControllerCameraState(CameraController.ControllerCameraState.FollowPlayer);
				Vector2 coreCurrentBasePosition = m_camera.GetCoreCurrentBasePosition();
				CutsceneMotion cutsceneMotion2 = new CutsceneMotion(m_cameraTransform, null, Vector2.Distance(m_cameraTransform.position.XY(), coreCurrentBasePosition) / returnToPlayerTime);
				cutsceneMotion2.camera = m_camera;
				activeMotions.Add(cutsceneMotion2);
				m_phaseComplete = false;
				break;
			}
			case 4:
				EndSequence();
				return;
			}
		}
		if (m_phaseCountdown > 0f)
		{
			m_phaseCountdown -= realDeltaTime;
			if (m_phaseCountdown <= 0f)
			{
				m_phaseCountdown = 0f;
				m_currentPhase++;
				m_phaseComplete = true;
			}
		}
	}

	private void KillAllEnemies()
	{
		if (!GameManager.Instance.BestActivePlayer)
		{
			return;
		}
		RoomHandler currentRoom = GameManager.Instance.BestActivePlayer.CurrentRoom;
		currentRoom.ClearReinforcementLayers();
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		List<AIActor> list = new List<AIActor>(activeEnemies);
		for (int i = 0; i < list.Count; i++)
		{
			AIActor aIActor = list[i];
			if (!aIActor.PreventAutoKillOnBossDeath)
			{
				SpawnEnemyOnDeath component = aIActor.GetComponent<SpawnEnemyOnDeath>();
				if ((bool)component)
				{
					Object.Destroy(component);
				}
				aIActor.healthHaver.minimumHealth = 0f;
				aIActor.healthHaver.ApplyDamage(10000f, Vector2.zero, "Boss Kill", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
