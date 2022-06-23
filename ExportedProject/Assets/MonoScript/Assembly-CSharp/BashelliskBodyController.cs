using System;
using System.Collections.Generic;
using UnityEngine;

public class BashelliskBodyController : BashelliskSegment
{
	public enum ShootDirection
	{
		NextSegment,
		Head,
		Average
	}

	private enum BodyState
	{
		Idle,
		Intro,
		Shooting,
		Outro
	}

	public GameObject shootPoint;

	private BashelliskHeadController m_head;

	private BodyState m_state;

	private BulletScriptSelector m_bulletScript;

	private BulletScriptSource m_bulletSource;

	public ShootDirection shootDirection { get; set; }

	public bool IsShooting
	{
		get
		{
			return State != BodyState.Idle;
		}
	}

	public float BaseShootDirection { get; private set; }

	public bool IsBroken { get; set; }

	private BodyState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	public void Start()
	{
		if ((bool)base.majorBreakable)
		{
			MajorBreakable obj = base.majorBreakable;
			obj.OnBreak = (Action)Delegate.Combine(obj.OnBreak, new Action(OnBreak));
		}
	}

	public void Init(BashelliskHeadController headController)
	{
		m_head = headController;
		base.healthHaver = m_head.healthHaver;
		base.aiActor = m_head.aiActor;
		base.aiActor.healthHaver.bodySprites.Add(base.sprite);
	}

	public void Update()
	{
		UpdateState();
	}

	protected override void OnDestroy()
	{
		MajorBreakable obj = base.majorBreakable;
		obj.OnBreak = (Action)Delegate.Remove(obj.OnBreak, new Action(OnBreak));
		base.OnDestroy();
	}

	public void Fire(BulletScriptSelector bulletScript)
	{
		if (!IsBroken)
		{
			m_bulletScript = bulletScript;
			State = BodyState.Intro;
		}
	}

	public void UpdateShootDirection()
	{
		float z = 0f;
		if (shootDirection == ShootDirection.NextSegment)
		{
			z = (previous.center.position - center.position).XY().ToAngle();
		}
		else if (shootDirection == ShootDirection.Head)
		{
			z = m_head.aiAnimator.FacingDirection;
		}
		else if (shootDirection == ShootDirection.Average)
		{
			float prevAverage = 0f;
			int prevCount = 0;
			BraveMathCollege.WeightedAverage(m_head.aiAnimator.FacingDirection, ref prevAverage, ref prevCount);
			for (LinkedListNode<BashelliskSegment> linkedListNode = m_head.Body.First.Next; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				BraveMathCollege.WeightedAverage(((BashelliskBodyController)linkedListNode.Value).BaseShootDirection, ref prevAverage, ref prevCount);
			}
			z = prevAverage;
		}
		shootPoint.transform.rotation = Quaternion.Euler(0f, 0f, z);
	}

	public void OnBreak()
	{
		IsBroken = true;
		if (m_state != 0)
		{
			m_state = BodyState.Idle;
			if ((bool)m_bulletSource)
			{
				m_bulletSource.ForceStop();
			}
		}
		base.aiAnimator.SetBaseAnim("broken");
		base.aiAnimator.EndAnimation();
	}

	public override void UpdatePosition(PooledLinkedList<Vector2> path, LinkedListNode<Vector2> pathNode, float totalPathDist, float thisNodeDist)
	{
		float num = PathDist - previous.PathDist;
		float num2 = previous.attachRadius + attachRadius;
		bool flag = false;
		if (num < num2)
		{
			num2 = num;
			flag = true;
		}
		float num3 = 0f - thisNodeDist;
		while (pathNode.Next != null)
		{
			float num4 = Vector2.Distance(pathNode.Next.Value, pathNode.Value);
			if (num3 + num4 >= num2)
			{
				float num5 = num2 - num3;
				if (!flag)
				{
					Vector2 vector = Vector2.Lerp(pathNode.Value, pathNode.Next.Value, num5 / num4);
					base.transform.position = (Vector3)vector - center.localPosition;
					base.specRigidbody.Reinitialize();
				}
				base.sprite.UpdateZDepth();
				BaseShootDirection = (previous.center.position - center.position).XY().ToAngle();
				PathDist = totalPathDist + num2;
				if ((bool)next)
				{
					next.UpdatePosition(path, pathNode, totalPathDist + num2, num5);
				}
				else
				{
					while (path.Last != pathNode.Next)
					{
						path.RemoveLast();
					}
				}
				UpdateShootDirection();
				break;
			}
			num3 += num4;
			pathNode = pathNode.Next;
		}
	}

	private void BeginState(BodyState state)
	{
		switch (state)
		{
		case BodyState.Intro:
			base.aiAnimator.PlayUntilCancelled("gun_out");
			break;
		case BodyState.Shooting:
			if (!m_bulletSource)
			{
				m_bulletSource = shootPoint.GetOrAddComponent<BulletScriptSource>();
			}
			m_bulletSource.BulletManager = m_head.bulletBank;
			m_bulletSource.BulletScript = m_bulletScript;
			m_bulletSource.Initialize();
			break;
		case BodyState.Outro:
			base.aiAnimator.PlayUntilFinished("gun_in");
			break;
		}
	}

	private void UpdateState()
	{
		if (State == BodyState.Intro)
		{
			if (!base.aiAnimator.IsPlaying("gun_out"))
			{
				State = BodyState.Shooting;
			}
		}
		else if (State == BodyState.Shooting)
		{
			if (m_bulletSource.IsEnded)
			{
				State = BodyState.Outro;
			}
		}
		else if (State == BodyState.Outro && !base.aiAnimator.IsPlaying("gun_in"))
		{
			State = BodyState.Idle;
		}
	}

	private void EndState(BodyState state)
	{
	}
}
