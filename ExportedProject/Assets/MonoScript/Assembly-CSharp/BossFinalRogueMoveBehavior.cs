using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRogue/MoveBehavior")]
public class BossFinalRogueMoveBehavior : MovementBehaviorBase
{
	public Vector2 maxMoveSpeed = new Vector2(3f, 0f);

	public Vector2 moveAcceleration = new Vector2(2f, 0f);

	public float ramMultiplier = 3f;

	public float minPlayerDist = 5f;

	public float maxPlayerDist = 12f;

	public float minYHeight;

	public float maxYHeight;

	private Vector2 m_targetCenter;

	private float? m_centerX;

	public override void Start()
	{
		base.Start();
		m_updateEveryFrame = true;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_aiActor.BehaviorOverridesVelocity = true;
	}

	public override BehaviorResult Update()
	{
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		if (!m_centerX.HasValue)
		{
			m_centerX = m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter.x;
		}
		Vector2 unitCenter = m_aiActor.TargetRigidbody.UnitCenter;
		Vector2 zero = Vector2.zero;
		if (m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter.x < m_centerX.Value - 2f)
		{
			zero.x = 1f;
		}
		else if (m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter.x > m_centerX.Value + 2f)
		{
			zero.x = -1f;
		}
		float num = m_aiActor.specRigidbody.HitboxPixelCollider.UnitBottom - unitCenter.y;
		bool useRamingSpeed = false;
		if (num < -1.5f)
		{
			if (unitCenter.x < m_aiActor.specRigidbody.HitboxPixelCollider.UnitLeft)
			{
				useRamingSpeed = true;
				zero.x = -1f;
			}
			else if (unitCenter.x > m_aiActor.specRigidbody.HitboxPixelCollider.UnitRight)
			{
				useRamingSpeed = true;
				zero.x = 1f;
			}
		}
		m_aiActor.BehaviorVelocity.x = RamMoveTowards(m_aiActor.BehaviorVelocity.x, zero.x * maxMoveSpeed.x, moveAcceleration.x * m_deltaTime, useRamingSpeed);
		m_aiActor.BehaviorVelocity.y = 0f;
		return BehaviorResult.Continue;
	}

	private float RamMoveTowards(float current, float target, float maxDelta, bool useRamingSpeed)
	{
		float num = target;
		float num2 = maxDelta;
		if (useRamingSpeed)
		{
			num = target * ramMultiplier;
			num2 = maxDelta * ramMultiplier;
		}
		if ((num < 0f && (current < num || current >= 0f)) || (num > 0f && (current > num || current <= 0f)))
		{
			num2 = maxDelta * ramMultiplier;
		}
		if (Mathf.Abs(num - current) <= num2)
		{
			return num;
		}
		return current + Mathf.Sign(num - current) * num2;
	}
}
