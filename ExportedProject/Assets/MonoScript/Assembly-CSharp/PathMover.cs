using System;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class PathMover : BraveBehaviour
{
	public bool Paused;

	[NonSerialized]
	protected float m_pathSpeed = 1f;

	[FormerlySerializedAs("PathSpeed")]
	public float OriginalPathSpeed = 1f;

	public float AdditionalNodeDelay;

	[NonSerialized]
	public Vector2 nodeOffset;

	[NonSerialized]
	public SerializedPath Path;

	[NonSerialized]
	public RoomHandler RoomHandler;

	[NonSerialized]
	public int PathStartNode;

	[NonSerialized]
	public bool IsUsingAlternateTargets;

	[NonSerialized]
	public bool ForceCornerDelayHack;

	public Action<Vector2, Vector2, bool> OnNodeReached;

	private Vector2 prevMotionVec = Vector2.zero;

	private int m_currentIndex;

	private int m_currentIndexDelta = 1;

	public float AbsPathSpeed
	{
		get
		{
			return Mathf.Abs(PathSpeed);
		}
	}

	public float PathSpeed
	{
		get
		{
			return m_pathSpeed;
		}
		set
		{
			bool flag = false;
			if (Mathf.Sign(value) != Mathf.Sign(m_pathSpeed))
			{
				flag = true;
			}
			m_pathSpeed = value;
			if (flag)
			{
				HandleDirectionFlip();
			}
		}
	}

	public int PreviousIndex
	{
		get
		{
			return (m_currentIndex + m_currentIndexDelta * -1 + Path.nodes.Count) % Path.nodes.Count;
		}
	}

	public int CurrentIndex
	{
		get
		{
			return m_currentIndex;
		}
	}

	public static SerializedPath GetRoomPath(RoomHandler room, int pathIndex)
	{
		return room.area.runtimePrototypeData.paths[pathIndex];
	}

	private void Awake()
	{
		m_pathSpeed = OriginalPathSpeed;
	}

	public void Start()
	{
		if (Path == null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		if (Path != null && Path.overrideSpeed != -1f)
		{
			PathSpeed = Path.overrideSpeed;
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPathTargetReached = (Action)Delegate.Combine(speculativeRigidbody.OnPathTargetReached, new Action(PathTargetReached));
		m_currentIndex = PathStartNode;
		if ((bool)base.talkDoer)
		{
			Paused = true;
			PathToNextNode();
		}
		else
		{
			base.transform.position = RoomHandler.area.basePosition.ToVector2() + Path.nodes[PathStartNode].RoomPosition + nodeOffset;
			base.specRigidbody.Reinitialize();
			PathTargetReached();
		}
	}

	public void Update()
	{
		base.specRigidbody.PathSpeed = ((!Paused) ? Mathf.Abs(PathSpeed) : 0f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected float GetTotalPathLength()
	{
		float num = 0f;
		for (int i = 0; i < Path.nodes.Count - 1; i++)
		{
			num += (Path.nodes[i + 1].RoomPosition - Path.nodes[i].RoomPosition).magnitude;
		}
		if (Path.wrapMode == SerializedPath.SerializedPathWrapMode.Loop)
		{
			num += (Path.nodes[0].RoomPosition - Path.nodes[Path.nodes.Count - 1].RoomPosition).magnitude;
		}
		return num;
	}

	public Vector2 GetPositionOfNode(int nodeIndex)
	{
		return Path.nodes[nodeIndex].RoomPosition + RoomHandler.area.basePosition.ToVector2() + nodeOffset;
	}

	public float GetParametrizedPathPosition()
	{
		float totalPathLength = GetTotalPathLength();
		float num = 0f;
		if (PathSpeed >= 0f)
		{
			int num2 = ((m_currentIndex != 0) ? m_currentIndex : Path.nodes.Count);
			for (int i = 0; i < num2 - 1; i++)
			{
				num += (Path.nodes[i + 1].RoomPosition - Path.nodes[i].RoomPosition).magnitude;
			}
			int index = (m_currentIndex + Path.nodes.Count - 1) % Path.nodes.Count;
			num += Vector2.Distance(Path.nodes[index].RoomPosition + RoomHandler.area.basePosition.ToVector2() + nodeOffset, base.specRigidbody.Position.UnitPosition);
		}
		else
		{
			for (int j = 0; j < m_currentIndex; j++)
			{
				num += (Path.nodes[j + 1].RoomPosition - Path.nodes[j].RoomPosition).magnitude;
			}
			num += Vector2.Distance(Path.nodes[m_currentIndex].RoomPosition + RoomHandler.area.basePosition.ToVector2() + nodeOffset, base.specRigidbody.Position.UnitPosition);
		}
		return num / totalPathLength;
	}

	private void PathTargetReached()
	{
		if (ForceCornerDelayHack && Vector2.Distance(Path.nodes[m_currentIndex].RoomPosition + RoomHandler.area.basePosition.ToVector2() + nodeOffset, base.specRigidbody.Position.UnitPosition) > 0.1f)
		{
			base.specRigidbody.Velocity = Vector2.zero;
			return;
		}
		SerializedPathNode serializedPathNode = Path.nodes[m_currentIndex];
		Vector2 vector = Path.nodes[(m_currentIndex - 1 + Path.nodes.Count) % Path.nodes.Count].position.ToVector2();
		Vector2 vector2 = Path.nodes[m_currentIndex].position.ToVector2();
		bool flag = UpdatePathIndex();
		if (OnNodeReached != null)
		{
			bool flag2 = flag;
			Vector2 vector3 = Vector2.zero;
			if (flag2)
			{
				vector3 = Path.nodes[m_currentIndex].position.ToVector2();
			}
			if (prevMotionVec == Vector2.zero)
			{
				prevMotionVec = vector2 - vector;
			}
			Vector2 vector4 = vector3 - vector2;
			OnNodeReached(prevMotionVec, vector4, flag2);
			if (vector4 != Vector2.zero)
			{
				prevMotionVec = vector3 - vector2;
			}
		}
		if (!flag)
		{
			base.specRigidbody.PathMode = false;
			base.specRigidbody.Velocity = Vector2.zero;
		}
		else if (serializedPathNode.delayTime == 0f && AdditionalNodeDelay == 0f)
		{
			PathToNextNode();
		}
		else
		{
			base.specRigidbody.PathMode = false;
			base.specRigidbody.Velocity = Vector2.zero;
			Invoke("PathToNextNode", serializedPathNode.delayTime + AdditionalNodeDelay);
		}
	}

	public void WarpToStart()
	{
		m_currentIndex = PathStartNode;
		base.transform.position = RoomHandler.area.basePosition.ToVector2() + Path.nodes[PathStartNode].RoomPosition + nodeOffset;
		base.specRigidbody.Reinitialize();
		PathTargetReached();
	}

	public void WarpToNearestPoint(Vector2 targetPoint)
	{
		Vector2 vector = Vector2.zero;
		float num = float.MaxValue;
		int currentIndex = -1;
		for (int i = 0; i < Path.nodes.Count - 1; i++)
		{
			Vector2 vector2 = Path.nodes[i].RoomPosition + nodeOffset + RoomHandler.area.basePosition.ToVector2();
			Vector2 vector3 = Path.nodes[i + 1].RoomPosition + nodeOffset + RoomHandler.area.basePosition.ToVector2();
			Vector2 vector4 = BraveMathCollege.ClosestPointOnLineSegment(targetPoint, vector2, vector3);
			float num2 = Vector2.Distance(vector4, targetPoint);
			if (num2 < 1f)
			{
				Vector2 vector5 = ((!(Vector2.Distance(vector2, vector4) < Vector2.Distance(vector3, vector4))) ? vector2 : vector3);
				Vector2 vector6 = vector5 - vector4;
				if (vector6.magnitude > 1f)
				{
					vector4 += vector6.normalized;
				}
			}
			if (num2 < num)
			{
				num = num2;
				currentIndex = i + 1;
				vector = vector4;
			}
		}
		if (Path.wrapMode == SerializedPath.SerializedPathWrapMode.Loop)
		{
			Vector2 vector7 = Path.nodes[Path.nodes.Count - 1].RoomPosition + nodeOffset + RoomHandler.area.basePosition.ToVector2();
			Vector2 vector8 = Path.nodes[0].RoomPosition + nodeOffset + RoomHandler.area.basePosition.ToVector2();
			Vector2 vector9 = BraveMathCollege.ClosestPointOnLineSegment(targetPoint, vector7, vector8);
			float num3 = Vector2.Distance(vector9, targetPoint);
			if (num3 < 1f)
			{
				Vector2 vector10 = ((!(Vector2.Distance(vector7, vector9) < Vector2.Distance(vector8, vector9))) ? vector7 : vector8);
				Vector2 vector11 = vector10 - vector9;
				if (vector11.magnitude > 1f)
				{
					vector9 += vector11.normalized;
				}
			}
			if (num3 < num)
			{
				num = num3;
				currentIndex = 0;
				vector = vector9;
			}
		}
		m_currentIndex = currentIndex;
		base.transform.position = vector.ToVector3ZUp();
		base.specRigidbody.Reinitialize();
		PathToNextNode();
	}

	public void ForcePathToNextNode()
	{
		PathToNextNode();
	}

	protected void HandleDirectionFlip()
	{
		m_currentIndexDelta *= -1;
		switch (Path.wrapMode)
		{
		case SerializedPath.SerializedPathWrapMode.Loop:
			m_currentIndex = (m_currentIndex + Path.nodes.Count + m_currentIndexDelta) % Path.nodes.Count;
			break;
		case SerializedPath.SerializedPathWrapMode.Once:
			m_currentIndex = (m_currentIndex + Path.nodes.Count + m_currentIndexDelta) % Path.nodes.Count;
			break;
		case SerializedPath.SerializedPathWrapMode.PingPong:
			if (m_currentIndex + m_currentIndexDelta < 0 || m_currentIndex + m_currentIndexDelta >= Path.nodes.Count)
			{
				m_currentIndexDelta *= -1;
			}
			m_currentIndex += m_currentIndexDelta;
			break;
		default:
			m_currentIndex = (m_currentIndex + Path.nodes.Count + m_currentIndexDelta) % Path.nodes.Count;
			break;
		}
		PathToNextNode();
	}

	private void PathToNextNode()
	{
		SerializedPathNode serializedPathNode = Path.nodes[m_currentIndex];
		base.specRigidbody.PathMode = true;
		base.specRigidbody.PathTarget = PhysicsEngine.UnitToPixel(RoomHandler.area.basePosition.ToVector2() + nodeOffset + serializedPathNode.RoomPosition);
		base.specRigidbody.PathSpeed = ((!Paused) ? Mathf.Abs(PathSpeed) : 0f);
	}

	public Vector2 GetCurrentTargetPosition()
	{
		return GetPositionOfNode(m_currentIndex);
	}

	public Vector2 GetPreviousSourcePosition()
	{
		return GetPositionOfNode((m_currentIndex + m_currentIndexDelta * -2 + Path.nodes.Count) % Path.nodes.Count);
	}

	public Vector2 GetPreviousTargetPosition()
	{
		return GetPositionOfNode((m_currentIndex + m_currentIndexDelta * -1 + Path.nodes.Count) % Path.nodes.Count);
	}

	public Vector2 GetNextTargetPosition()
	{
		return GetNextTargetRoomPosition() + nodeOffset + RoomHandler.area.basePosition.ToVector2();
	}

	private Vector2 GetNextTargetRoomPosition()
	{
		SerializedPath path = Path;
		int currentIndex = m_currentIndex;
		if (IsUsingAlternateTargets && Path.nodes[m_currentIndex].UsesAlternateTarget)
		{
			SerializedPathNode serializedPathNode = Path.nodes[m_currentIndex];
			path = GetRoomPath(RoomHandler, serializedPathNode.AlternateTargetPathIndex);
			currentIndex = serializedPathNode.AlternateTargetNodeIndex;
			return path.nodes[currentIndex].RoomPosition;
		}
		if (Path.wrapMode == SerializedPath.SerializedPathWrapMode.Once)
		{
			currentIndex += m_currentIndexDelta;
			if (currentIndex >= Path.nodes.Count)
			{
				if (m_currentIndex >= Path.nodes.Count)
				{
					m_currentIndex = 0;
				}
				return Path.nodes[m_currentIndex].RoomPosition;
			}
		}
		else if (Path.wrapMode == SerializedPath.SerializedPathWrapMode.Loop)
		{
			currentIndex = (currentIndex + Path.nodes.Count + m_currentIndexDelta) % Path.nodes.Count;
		}
		else
		{
			if (Path.wrapMode != 0)
			{
				Debug.LogError("Unsupported path type " + Path.wrapMode);
				return Path.nodes[m_currentIndex].RoomPosition;
			}
			currentIndex += m_currentIndexDelta;
			if (currentIndex < 0 || currentIndex >= Path.nodes.Count)
			{
				m_currentIndexDelta *= -1;
				currentIndex += m_currentIndexDelta * 2;
			}
		}
		for (; currentIndex < 0; currentIndex += Path.nodes.Count)
		{
		}
		currentIndex %= Path.nodes.Count;
		return Path.nodes[currentIndex].RoomPosition;
	}

	private bool UpdatePathIndex()
	{
		if (IsUsingAlternateTargets && Path.nodes[m_currentIndex].UsesAlternateTarget)
		{
			SerializedPathNode serializedPathNode = Path.nodes[m_currentIndex];
			Path = GetRoomPath(RoomHandler, serializedPathNode.AlternateTargetPathIndex);
			m_currentIndex = serializedPathNode.AlternateTargetNodeIndex;
			return true;
		}
		if (Path.wrapMode == SerializedPath.SerializedPathWrapMode.Once)
		{
			m_currentIndex += m_currentIndexDelta;
			if (m_currentIndex >= Path.nodes.Count || m_currentIndex < 0)
			{
				return false;
			}
		}
		else if (Path.wrapMode == SerializedPath.SerializedPathWrapMode.Loop)
		{
			m_currentIndex = (m_currentIndex + m_currentIndexDelta + Path.nodes.Count) % Path.nodes.Count;
		}
		else
		{
			if (Path.wrapMode != 0)
			{
				Debug.LogError("Unsupported path type " + Path.wrapMode);
				return false;
			}
			if (m_currentIndex == 0)
			{
				m_currentIndex = 1;
				m_currentIndexDelta = 1;
			}
			else if (m_currentIndex == Path.nodes.Count - 1)
			{
				m_currentIndex = Path.nodes.Count - 2;
				m_currentIndexDelta = -1;
			}
			else
			{
				m_currentIndex += m_currentIndexDelta;
				if (m_currentIndex < 0 || m_currentIndex >= Path.nodes.Count)
				{
					m_currentIndexDelta *= -1;
					m_currentIndex += m_currentIndexDelta * 2;
				}
			}
		}
		return true;
	}
}
