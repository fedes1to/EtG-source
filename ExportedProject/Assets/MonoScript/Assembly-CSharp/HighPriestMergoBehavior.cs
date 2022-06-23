using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/HighPriest/MergoBehavior")]
public class HighPriestMergoBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		OutAnim,
		Fading,
		Firing,
		Unfading,
		InAnim
	}

	public BulletScriptSelector shootBulletScript;

	public BulletScriptSelector wallBulletScript;

	public float darknessFadeTime = 1f;

	public float fireTime = 8f;

	public float fireMainMidTime = 0.8f;

	public float fireMainDist = 16f;

	public float fireMainDistVariance = 3f;

	public float fireWallMidTime = 0.5f;

	[InspectorCategory("Visuals")]
	public string teleportOutAnim;

	[InspectorCategory("Visuals")]
	public string teleportInAnim;

	private const float c_wallBuffer = 5f;

	private State m_state;

	private tk2dBaseSprite m_shadowSprite;

	private float m_timer;

	private float m_mainShotTimer;

	private float m_wallShotTimer;

	private List<BulletScriptSource> m_shootBulletSources = new List<BulletScriptSource>();

	private BulletScriptSource m_wallBulletSource;

	public override void Start()
	{
		base.Start();
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
		m_state = State.OutAnim;
		if (!m_aiActor.IsBlackPhantom)
		{
			m_aiActor.sprite.usesOverrideMaterial = true;
			m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
		}
		SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
		m_aiAnimator.PlayUntilCancelled(teleportOutAnim);
		m_aiActor.specRigidbody.enabled = false;
		m_aiActor.ClearPath();
		m_aiActor.knockbackDoer.SetImmobile(true, "CrosshairBehavior");
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.OutAnim)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(Mathf.Lerp(1f, 0f, m_aiAnimator.CurrentClipProgress));
			if (!m_aiAnimator.IsPlaying(teleportOutAnim))
			{
				m_state = State.Fading;
				m_aiActor.ToggleRenderers(false);
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(0f);
				m_timer = darknessFadeTime;
				m_aiActor.ParentRoom.BecomeTerrifyingDarkRoom();
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Fading)
		{
			if (m_timer <= 0f)
			{
				m_state = State.Firing;
				m_timer = fireTime;
				m_mainShotTimer = fireMainMidTime;
				m_wallShotTimer = fireWallMidTime;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Firing)
		{
			if (m_timer <= 0f)
			{
				m_state = State.Unfading;
				m_timer = darknessFadeTime;
				m_aiActor.ParentRoom.EndTerrifyingDarkRoom();
				return ContinuousBehaviorResult.Continue;
			}
			m_mainShotTimer -= m_deltaTime;
			if (m_mainShotTimer < 0f)
			{
				ShootBulletScript();
				m_mainShotTimer += fireMainMidTime;
			}
			m_wallShotTimer -= m_deltaTime;
			if (m_wallShotTimer < 0f)
			{
				ShootWallBulletScript();
				m_wallShotTimer += fireWallMidTime;
			}
		}
		else if (m_state == State.Unfading)
		{
			if (m_timer <= 0f)
			{
				m_state = State.InAnim;
				m_aiActor.ToggleRenderers(true);
				m_aiAnimator.PlayUntilFinished(teleportInAnim);
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.InAnim)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(Mathf.Lerp(0f, 1f, m_aiAnimator.CurrentClipProgress));
			if (!m_aiAnimator.IsPlaying(teleportInAnim))
			{
				if (!m_aiActor.IsBlackPhantom)
				{
					m_aiActor.sprite.usesOverrideMaterial = false;
					m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitCutoutUber");
				}
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
				m_aiActor.specRigidbody.enabled = true;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiAnimator.EndAnimationIf(teleportInAnim);
		m_aiAnimator.EndAnimationIf(teleportOutAnim);
		m_aiActor.ToggleRenderers(true);
		m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		if (!m_aiActor.IsBlackPhantom)
		{
			m_aiActor.sprite.usesOverrideMaterial = false;
			m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitCutoutUber");
		}
		SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
		m_aiActor.ParentRoom.EndTerrifyingDarkRoom();
		m_aiActor.specRigidbody.enabled = true;
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
			m_aiActor.ParentRoom.EndTerrifyingDarkRoom();
		}
		base.OnActorPreDeath();
	}

	private void ShootBulletScript()
	{
		BulletScriptSource bulletScriptSource = null;
		for (int i = 0; i < m_shootBulletSources.Count; i++)
		{
			if (m_shootBulletSources[i].IsEnded)
			{
				bulletScriptSource = m_shootBulletSources[i];
				break;
			}
		}
		if (bulletScriptSource == null)
		{
			bulletScriptSource = new GameObject("Mergo shoot point").AddComponent<BulletScriptSource>();
			m_shootBulletSources.Add(bulletScriptSource);
		}
		bulletScriptSource.transform.position = RandomShootPoint();
		bulletScriptSource.BulletManager = m_aiActor.bulletBank;
		bulletScriptSource.BulletScript = shootBulletScript;
		bulletScriptSource.Initialize();
	}

	private void ShootWallBulletScript()
	{
		float rotation;
		Vector2 vector = RandomWallPoint(out rotation);
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (!playerController || playerController.healthHaver.IsDead || Vector2.Distance(vector, playerController.CenterPosition) < 8f)
			{
				return;
			}
		}
		if (!m_wallBulletSource)
		{
			m_wallBulletSource = new GameObject("Mergo wall shoot point").AddComponent<BulletScriptSource>();
		}
		m_wallBulletSource.transform.position = vector;
		m_wallBulletSource.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
		m_wallBulletSource.BulletManager = m_aiActor.bulletBank;
		m_wallBulletSource.BulletScript = wallBulletScript;
		m_wallBulletSource.Initialize();
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

	private Vector2 RandomWallPoint(out float rotation)
	{
		float num = 4f;
		CellArea area = m_aiActor.ParentRoom.area;
		Vector2 vector = area.basePosition.ToVector2() + new Vector2(0.5f, 1.5f);
		Vector2 vector2 = (area.basePosition + area.dimensions).ToVector2() - new Vector2(0.5f, 0.5f);
		if (BraveUtility.RandomBool())
		{
			if (BraveUtility.RandomBool())
			{
				rotation = -90f;
				return new Vector2(Random.Range(vector.x + 5f, vector2.x - 5f), vector2.y + num + 2f);
			}
			rotation = 90f;
			return new Vector2(Random.Range(vector.x + 5f, vector2.x - 5f), vector.y - num);
		}
		if (BraveUtility.RandomBool())
		{
			rotation = 0f;
			return new Vector2(vector.x - num, Random.Range(vector.y + 5f, vector2.y - 5f));
		}
		rotation = 180f;
		return new Vector2(vector2.x + num, Random.Range(vector.y + 5f, vector2.y - 5f));
	}
}
