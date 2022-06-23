using System.Collections.Generic;
using FullInspector;
using UnityEngine;

public class DashMoveBehavior : MovementBehaviorBase
{
	[InspectorCategory("Conditions")]
	public float Cooldown = 1f;

	[InspectorCategory("Conditions")]
	public bool HealthModifiesCooldown;

	[InspectorCategory("Conditions")]
	[InspectorShowIf("HealthModifiesCooldown")]
	public float NoHealthCooldown = 1f;

	[InspectorShowIf("HealthModifiesCooldown")]
	[InspectorCategory("Conditions")]
	public float FullHealthCooldown = 1f;

	[InspectorCategory("Conditions")]
	public float InitialCooldown;

	[InspectorCategory("Conditions")]
	public float GlobalCooldown;

	public float dashDistance;

	public float dashTime;

	[InspectorCategory("Visuals")]
	public bool enableShadowTrail;

	protected float m_cooldownTimer;

	protected float m_dashTimer;

	public override void Start()
	{
		base.Start();
		m_cooldownTimer = InitialCooldown;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_cooldownTimer, true);
		DecrementTimer(ref m_dashTimer);
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
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		float num = dashDistance / dashTime;
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = num * GetDashDirection();
		m_updateEveryFrame = true;
		m_dashTimer = dashTime;
		if (enableShadowTrail)
		{
			m_aiActor.GetComponent<AfterImageTrailController>().spawnShadows = true;
		}
		AkSoundEngine.PostEvent("Play_ENM_highpriest_dash_01", GameManager.Instance.PrimaryPlayer.gameObject);
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (enableShadowTrail && m_dashTimer <= 0.1f)
		{
			m_aiActor.GetComponent<AfterImageTrailController>().spawnShadows = false;
		}
		return (m_dashTimer <= 0f) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
		m_aiActor.BehaviorOverridesVelocity = false;
		if (enableShadowTrail)
		{
			m_aiActor.GetComponent<AfterImageTrailController>().spawnShadows = false;
		}
		RefreshCooldowns();
	}

	public bool IsReady()
	{
		return m_cooldownTimer == 0f;
	}

	public void RefreshCooldowns()
	{
		if (HealthModifiesCooldown)
		{
			m_cooldownTimer = Mathf.Lerp(NoHealthCooldown, FullHealthCooldown, m_aiActor.healthHaver.GetCurrentHealthPercentage());
		}
		else
		{
			m_cooldownTimer = Cooldown;
		}
		if (GlobalCooldown > 0f)
		{
			m_aiActor.behaviorSpeculator.GlobalCooldown = GlobalCooldown;
		}
	}

	private Vector2 GetDashDirection()
	{
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 normalized = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.Ground) - unitCenter).normalized;
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < 2; i++)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			Vector2 vector = normalized.Rotate((i != 0) ? (-90) : 90);
			RaycastResult result;
			flag = PhysicsEngine.Instance.Raycast(unitCenter, vector, 3f, out result, true, true, int.MaxValue, CollisionLayer.EnemyCollider, false, null, m_aiActor.specRigidbody);
			RaycastResult.Pool.Free(ref result);
			for (float num = 0.25f; num <= dashDistance; num += 0.25f)
			{
				if (flag2)
				{
					break;
				}
				if (flag)
				{
					break;
				}
				Vector2 vector2 = unitCenter + num * vector;
				if (!GameManager.Instance.Dungeon.CellExists(vector2))
				{
					flag2 = true;
				}
				else if (GameManager.Instance.Dungeon.ShouldReallyFall(vector2))
				{
					flag2 = true;
				}
			}
			for (float num = 0.25f; num <= dashDistance; num += 0.25f)
			{
				if (flag2)
				{
					break;
				}
				if (flag3)
				{
					break;
				}
				if (flag)
				{
					break;
				}
				IntVector2 intVector = (unitCenter + num * vector).ToIntVector2(VectorConversions.Floor);
				if (!GameManager.Instance.Dungeon.CellExists(intVector))
				{
					flag3 = true;
				}
				else if (GameManager.Instance.Dungeon.data[intVector].isExitCell)
				{
					flag3 = true;
				}
			}
			if (!flag && !flag2 && !flag3)
			{
				list.Add(vector);
			}
		}
		if (list.Count > 0)
		{
			return BraveUtility.RandomElement(list).normalized;
		}
		return normalized.Rotate(BraveUtility.RandomSign() * 90f).normalized;
	}
}
