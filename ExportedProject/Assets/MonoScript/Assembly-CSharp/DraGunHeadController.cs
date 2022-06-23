using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraGunHeadController : BraveBehaviour
{
	public List<DraGunNeckPieceController> neckPieces;

	public float moveTime = 1f;

	public float maxSpeed = 3f;

	public float overrideMoveTime = 0.5f;

	public float overrideMaxSpeed = 9f;

	private bool m_initialized;

	private Vector2 m_startingHeadPosition;

	private Vector2 m_currentVelocity;

	public float? TargetX { get; set; }

	public Vector2? OverrideDesiredPosition { get; set; }

	public bool ReachedOverridePosition
	{
		get
		{
			return OverrideDesiredPosition.HasValue && Vector2.Distance(base.transform.position.XY(), OverrideDesiredPosition.Value) < 0.5f;
		}
	}

	public IEnumerator Start()
	{
		yield return null;
		m_initialized = true;
		m_startingHeadPosition = base.transform.position;
	}

	public void UpdateHead()
	{
		if (!m_initialized)
		{
			return;
		}
		Vector2 current = base.transform.position.XY();
		Vector2 target = new Vector2(current.x, m_startingHeadPosition.y);
		if (OverrideDesiredPosition.HasValue)
		{
			target = OverrideDesiredPosition.Value;
		}
		else
		{
			if (TargetX.HasValue)
			{
				target.x = TargetX.Value;
			}
			target.y = m_startingHeadPosition.y + Mathf.Sin(Time.timeSinceLevelLoad * ((float)Math.PI * 2f) / 1.5f) * 1.5f;
		}
		Vector2 vector = ((!OverrideDesiredPosition.HasValue) ? Vector2.SmoothDamp(current, target, ref m_currentVelocity, moveTime, maxSpeed, BraveTime.DeltaTime) : Vector2.SmoothDamp(current, target, ref m_currentVelocity, overrideMoveTime, overrideMaxSpeed, BraveTime.DeltaTime));
		base.transform.position = vector;
		Vector2 vector2 = vector - m_startingHeadPosition;
		Vector2 vector3 = vector2;
		if (!OverrideDesiredPosition.HasValue)
		{
			if (Mathf.Abs(vector3.x) > 6f)
			{
				vector3.x = Math.Sign(vector3.x) * 6;
			}
			if (vector3.y < -5f)
			{
				vector3.y = -5f;
			}
			if (vector3.y > 4f)
			{
				vector3.y = 4f;
			}
		}
		if (vector3 != vector2)
		{
			base.transform.position = m_startingHeadPosition + vector3;
			vector2 = vector3;
		}
		for (int i = 0; i < neckPieces.Count; i++)
		{
			neckPieces[i].UpdateHeadDelta(vector2);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void TriggerAnimationEvent(string eventInfo)
	{
		base.aiActor.behaviorSpeculator.TriggerAnimationEvent(eventInfo);
	}
}
