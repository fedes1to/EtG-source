using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Bashellisk/ShootBehavior")]
public class BashelliskShootBehavior : BasicAttackBehavior
{
	public enum SegmentOrder
	{
		Sequence = 10,
		Random = 20,
		RandomSequential = 30
	}

	public BashelliskBodyController.ShootDirection shootDirection;

	public BulletScriptSelector BulletScript;

	public SegmentOrder segmentOrder = SegmentOrder.Sequence;

	[InspectorShowIf("ShowSegmentCount")]
	public int SegmentCount;

	[InspectorShowIf("ShowSegmentPercentage")]
	public float SegmentPercentage;

	public float SegmentDelay;

	public bool StopDuring;

	public bool WaitForAllSegmentsFree;

	private BashelliskHeadController m_bashellisk;

	private PooledLinkedList<BashelliskBodyController> m_segments = new PooledLinkedList<BashelliskBodyController>();

	private LinkedListNode<BashelliskBodyController> m_nextSegmentNode;

	private bool m_waitingToStart;

	private float m_segmentDelayTimer;

	private bool ShowSegmentCount()
	{
		return SegmentPercentage <= 0f;
	}

	private bool ShowSegmentPercentage()
	{
		return SegmentCount <= 0;
	}

	public override void Start()
	{
		base.Start();
		m_bashellisk = m_aiActor.GetComponent<BashelliskHeadController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_segmentDelayTimer -= m_deltaTime * m_behaviorSpeculator.CooldownScale;
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
		SelectSegments();
		m_waitingToStart = false;
		if (WaitForAllSegmentsFree)
		{
			m_waitingToStart = true;
			m_segmentDelayTimer = 0f;
		}
		else if (SegmentDelay <= 0f)
		{
			while (m_nextSegmentNode != null)
			{
				FireNextSegment();
			}
		}
		else
		{
			FireNextSegment();
			m_segmentDelayTimer = SegmentDelay;
		}
		m_updateEveryFrame = true;
		return (!StopDuring) ? BehaviorResult.RunContinuousInClass : BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_waitingToStart)
		{
			for (LinkedListNode<BashelliskBodyController> linkedListNode = m_segments.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (linkedListNode.Value.IsShooting)
				{
					return ContinuousBehaviorResult.Continue;
				}
			}
			m_segmentDelayTimer = 0f;
			m_waitingToStart = false;
		}
		while (m_segmentDelayTimer <= 0f && m_nextSegmentNode != null)
		{
			m_segmentDelayTimer += SegmentDelay;
			FireNextSegment();
		}
		if (m_nextSegmentNode == null)
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void SelectSegments()
	{
		int b = SegmentCount;
		if (SegmentCount <= 0)
		{
			b = Mathf.RoundToInt(SegmentPercentage * (float)(m_bashellisk.Body.Count - 1));
		}
		b = Mathf.Max(1, b);
		m_segments.Clear();
		for (LinkedListNode<BashelliskSegment> next = m_bashellisk.Body.First.Next; next != null; next = next.Next)
		{
			m_segments.AddLast(next.Value as BashelliskBodyController);
		}
		if (segmentOrder == SegmentOrder.Sequence)
		{
			while (m_segments.Count > b)
			{
				m_segments.RemoveLast();
			}
		}
		else if (segmentOrder == SegmentOrder.Random)
		{
			for (int i = 0; i < b; i++)
			{
				int index = Random.Range(i, m_segments.Count);
				LinkedListNode<BashelliskBodyController> byIndexSlow = m_segments.GetByIndexSlow(index);
				m_segments.Remove(byIndexSlow, false);
				m_segments.AddFirst(byIndexSlow);
			}
			while (m_segments.Count > b)
			{
				m_segments.RemoveLast();
			}
		}
		else if (segmentOrder == SegmentOrder.RandomSequential)
		{
			while (m_segments.Count > b)
			{
				int index2 = Random.Range(0, m_segments.Count);
				m_segments.Remove(m_segments.GetByIndexSlow(index2), true);
			}
		}
		for (LinkedListNode<BashelliskBodyController> linkedListNode = m_segments.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			linkedListNode.Value.shootDirection = shootDirection;
			linkedListNode.Value.UpdateShootDirection();
		}
		m_nextSegmentNode = m_segments.First;
	}

	private void FireNextSegment()
	{
		m_nextSegmentNode.Value.Fire(BulletScript);
		m_nextSegmentNode = m_nextSegmentNode.Next;
	}
}
