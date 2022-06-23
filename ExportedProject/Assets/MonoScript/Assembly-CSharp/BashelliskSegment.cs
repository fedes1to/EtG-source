using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BashelliskSegment : BraveBehaviour
{
	public Transform center;

	public float attachRadius;

	[NonSerialized]
	public BashelliskSegment next;

	[NonSerialized]
	public BashelliskSegment previous;

	[NonSerialized]
	public float PathDist;

	public virtual void UpdatePosition(PooledLinkedList<Vector2> path, LinkedListNode<Vector2> pathNode, float totalPathDist, float thisNodeDist)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
