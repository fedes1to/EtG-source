using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/MergoBehavior")]
public class GiantPowderSkullMergoBehavior : BasicAttackBehavior
{
	public enum State
	{
		Idle,
		Fading,
		OutAnim,
		Firing,
		Unfading,
		InAnim
	}

	public BulletScriptSelector shootBulletScript;

	public float darknessFadeTime = 1f;

	public float darknessAmount = 0.3f;

	public float playerLightAmount = 0.5f;

	public float fireTime = 8f;

	public float fireMainMidTime = 0.8f;

	public float fireMainDist = 16f;

	public float fireMainDistVariance = 3f;

	[InspectorCategory("Visuals")]
	public string teleportOutAnim;

	[InspectorCategory("Visuals")]
	public string teleportInAnim;

	[InspectorCategory("Visuals")]
	public ParticleSystem roomParticleSystem;

	private State m_state;

	private tk2dBaseSprite m_shadowSprite;

	private ParticleSystem m_mainParticleSystem;

	private ParticleSystem m_trailParticleSystem;

	private Vector2 m_roomMin;

	private Vector2 m_roomMax;

	private float m_timer;

	private float m_mainShotTimer;

	private BulletScriptSource m_shootBulletSource;

	public override void Start()
	{
		base.Start();
		PowderSkullParticleController componentInChildren = m_aiActor.GetComponentInChildren<PowderSkullParticleController>();
		m_mainParticleSystem = componentInChildren.GetComponent<ParticleSystem>();
		m_trailParticleSystem = componentInChildren.RotationChild.GetComponentInChildren<ParticleSystem>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_shadowSprite = m_aiActor.ShadowObject.GetComponent<tk2dSprite>();
		m_state = State.Fading;
		m_aiActor.healthHaver.minimumHealth = 1f;
		m_timer = darknessFadeTime;
		m_aiActor.ParentRoom.BecomeTerrifyingDarkRoom(darknessFadeTime, darknessAmount, playerLightAmount);
		BraveUtility.EnableEmission(m_mainParticleSystem, false);
		BraveUtility.EnableEmission(m_trailParticleSystem, false);
		m_aiActor.ClearPath();
		m_aiActor.knockbackDoer.SetImmobile(true, "CrosshairBehavior");
		m_roomMin = m_aiActor.ParentRoom.area.UnitBottomLeft;
		m_roomMax = m_aiActor.ParentRoom.area.UnitTopRight;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		UpdateRoomParticles();
		if (m_state == State.Fading)
		{
			if (m_timer <= 0f)
			{
				m_state = State.OutAnim;
				m_aiAnimator.PlayUntilCancelled(teleportOutAnim);
				m_aiActor.specRigidbody.enabled = false;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.OutAnim)
		{
			if (!m_aiAnimator.IsPlaying(teleportOutAnim))
			{
				m_state = State.Firing;
				m_timer = fireTime;
				m_mainShotTimer = fireMainMidTime;
				m_shadowSprite.renderer.enabled = false;
				m_aiActor.ToggleRenderers(false);
				roomParticleSystem.GetComponent<Renderer>().enabled = true;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Firing)
		{
			if (m_timer <= 0f)
			{
				m_aiActor.TeleportSomewhere(new IntVector2(5, 5));
				m_state = State.Unfading;
				m_timer = darknessFadeTime;
				RoomHandler parentRoom = m_aiActor.ParentRoom;
				float goalIntensity = darknessAmount;
				parentRoom.EndTerrifyingDarkRoom(1f, goalIntensity, playerLightAmount);
				m_aiActor.ToggleRenderers(true);
				m_aiAnimator.PlayUntilFinished(teleportInAnim);
				m_shadowSprite.renderer.enabled = true;
				m_aiActor.ToggleRenderers(true);
				return ContinuousBehaviorResult.Continue;
			}
			m_mainShotTimer -= m_deltaTime;
			if (m_mainShotTimer < 0f)
			{
				ShootBulletScript();
				m_mainShotTimer += fireMainMidTime;
			}
		}
		else if (m_state == State.Unfading)
		{
			if (!m_aiAnimator.IsPlaying(teleportInAnim) && !m_aiActor.specRigidbody.enabled)
			{
				m_aiActor.specRigidbody.enabled = true;
				BraveUtility.EnableEmission(m_mainParticleSystem, true);
				BraveUtility.EnableEmission(m_trailParticleSystem, true);
			}
			if (m_timer <= 0f && !m_aiAnimator.IsPlaying(teleportInAnim))
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int num = allProjectiles.Count - 1; num >= 0; num--)
		{
			if (allProjectiles[num].Owner is AIActor && allProjectiles[num].name.Contains("cannon", true))
			{
				allProjectiles[num].DieInAir();
			}
		}
		m_aiActor.healthHaver.minimumHealth = 0f;
		m_aiAnimator.EndAnimationIf(teleportInAnim);
		m_aiAnimator.EndAnimationIf(teleportOutAnim);
		m_shadowSprite.renderer.enabled = true;
		m_aiActor.ToggleRenderers(true);
		m_aiActor.specRigidbody.enabled = true;
		BraveUtility.EnableEmission(m_mainParticleSystem, true);
		BraveUtility.EnableEmission(m_trailParticleSystem, true);
		roomParticleSystem.GetComponent<Renderer>().enabled = false;
		RoomHandler parentRoom = m_aiActor.ParentRoom;
		float goalIntensity = darknessAmount;
		parentRoom.EndTerrifyingDarkRoom(1f, goalIntensity, playerLightAmount);
		m_state = State.Idle;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override void OnActorPreDeath()
	{
		if (m_state == State.Fading || m_state == State.Firing)
		{
			RoomHandler parentRoom = m_aiActor.ParentRoom;
			float goalIntensity = darknessAmount;
			parentRoom.EndTerrifyingDarkRoom(1f, goalIntensity, playerLightAmount);
		}
		base.OnActorPreDeath();
	}

	private void ShootBulletScript()
	{
		if (!m_shootBulletSource)
		{
			m_shootBulletSource = new GameObject("Mergo shoot point").AddComponent<BulletScriptSource>();
		}
		m_shootBulletSource.transform.position = RandomShootPoint();
		m_shootBulletSource.BulletManager = m_aiActor.bulletBank;
		m_shootBulletSource.BulletScript = shootBulletScript;
		m_shootBulletSource.Initialize();
	}

	private Vector2 RandomShootPoint()
	{
		Vector2 center = m_aiActor.ParentRoom.area.Center;
		if ((bool)m_aiActor.TargetRigidbody)
		{
			m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		float magnitude = fireMainDist + Random.Range(0f - fireMainDistVariance, fireMainDistVariance);
		List<Vector2> list = new List<Vector2>();
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = 0; i < 36; i++)
		{
			Vector2 item = center + BraveMathCollege.DegreesToVector(i * 10, magnitude);
			if (!data.isWall((int)item.x, (int)item.y) && !data.isTopWall((int)item.x, (int)item.y))
			{
				list.Add(item);
			}
		}
		return BraveUtility.RandomElement(list);
	}

	private void UpdateRoomParticles()
	{
		if (m_state == State.Idle || m_state == State.Unfading)
		{
			return;
		}
		if (m_state == State.Fading && m_timer > darknessFadeTime / 2f)
		{
			float num = (1f - m_timer / darknessFadeTime) * 2f;
			int num2 = Mathf.RoundToInt(200f * m_deltaTime);
			for (int i = 0; i < num2; i++)
			{
				float angle = Random.Range(0, 360);
				float magnitude = Random.Range(num * 15f - 2f, num * 15f);
				Vector3 position = roomParticleSystem.transform.position + (Vector3)BraveMathCollege.DegreesToVector(angle, magnitude);
				position.z = position.y;
				float startLifetime = roomParticleSystem.startLifetime;
				ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
				emitParams.position = position;
				emitParams.velocity = BraveMathCollege.DegreesToVector(Random.Range(0, 360), roomParticleSystem.startSpeed);
				emitParams.startLifetime = startLifetime;
				emitParams.startSize = roomParticleSystem.startSize;
				emitParams.rotation = roomParticleSystem.startRotation;
				emitParams.startColor = roomParticleSystem.startColor;
				ParticleSystem.EmitParams emitParams2 = emitParams;
				roomParticleSystem.Emit(emitParams2, 1);
			}
		}
		else
		{
			int num3 = Mathf.RoundToInt(840f * m_deltaTime);
			for (int j = 0; j < num3; j++)
			{
				Vector3 position2 = BraveUtility.RandomVector2(m_roomMin, m_roomMax, new Vector2(0.5f, 0.5f));
				position2.z = position2.y;
				float startLifetime2 = roomParticleSystem.startLifetime;
				ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
				emitParams.position = position2;
				emitParams.velocity = BraveMathCollege.DegreesToVector(Random.Range(0, 360), roomParticleSystem.startSpeed);
				emitParams.startLifetime = startLifetime2;
				emitParams.startSize = roomParticleSystem.startSize;
				emitParams.rotation = roomParticleSystem.startRotation;
				emitParams.startColor = roomParticleSystem.startColor;
				ParticleSystem.EmitParams emitParams3 = emitParams;
				roomParticleSystem.Emit(emitParams3, 1);
			}
		}
	}
}
