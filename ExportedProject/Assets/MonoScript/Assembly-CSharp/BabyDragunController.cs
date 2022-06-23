using System;
using System.Collections.Generic;
using UnityEngine;

public class BabyDragunController : MonoBehaviour
{
	public List<Transform> Segments;

	private List<tk2dSprite> SegmentSprites = new List<tk2dSprite>();

	public AIAnimator HeadAnimator;

	private SpeculativeRigidbody m_srb;

	private Vector2 m_lastBasePosition;

	private Vector2 m_lastVelocityAvg;

	private List<BabyDragunSegment> m_segmentData = new List<BabyDragunSegment>();

	public float SinWave1Multiplier = 1f;

	public float SinTimeMultiplier = 1f;

	public float SinAmplitude = 0.5f;

	[Header("Enemy Stats")]
	public float EnemyBaseSpeed = 4.5f;

	public float EnemyFastSpeed = 6f;

	private float m_concernTimer;

	private PooledLinkedList<Vector2> m_path;

	private int m_lastChangedFacingFrame = -100;

	private float m_pathSegmentLength;

	private AIBulletBank m_bulletBank;

	private BehaviorSpeculator m_behaviorSpeculator;

	public bool IsEnemy { get; set; }

	public Vector2 EnemyTargetPos { get; set; }

	public float EnemySpeed { get; set; }

	public DraGunController Parent { get; set; }

	public AutoAimTarget ParentHeart { get; set; }

	private void Start()
	{
		m_srb = GetComponent<SpeculativeRigidbody>();
		SpeculativeRigidbody srb = m_srb;
		srb.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(srb.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(OnPostRigidbodyMovement));
		m_lastBasePosition = base.transform.position.XY();
		m_path = new PooledLinkedList<Vector2>();
		m_path.AddLast(m_lastBasePosition);
		m_path.AddLast(m_lastBasePosition);
		float num = 0f;
		for (int i = 0; i < Segments.Count; i++)
		{
			BabyDragunSegment item = default(BabyDragunSegment);
			item.lastPosition = Segments[i].position.XY();
			item.initialStartingDistance = ((i != 0) ? (m_segmentData[m_segmentData.Count - 1].lastPosition - item.lastPosition).magnitude : (m_lastBasePosition - item.lastPosition).magnitude);
			item.pathDist = num;
			num += item.initialStartingDistance;
			m_segmentData.Add(item);
			SegmentSprites.Add(Segments[i].GetComponent<tk2dSprite>());
		}
	}

	public void Update()
	{
		if (!IsEnemy || !(BraveTime.DeltaTime > 0f))
		{
			return;
		}
		Vector2 enemyTargetPos = EnemyTargetPos;
		if ((bool)Parent.head && Parent.head.transform.position.x + 5f > enemyTargetPos.x)
		{
			enemyTargetPos.x = Parent.head.transform.position.x + 5f;
		}
		Vector2 vector = enemyTargetPos + new Vector2(1f, 0f).Rotate(Time.realtimeSinceStartup / 3f * 360f).Scale(3f, 1.5f);
		if (ParentHeart.enabled)
		{
			if (m_concernTimer <= 0f)
			{
				EnemySpeed = EnemyFastSpeed;
				EnemyTargetPos += new Vector2(-5.5f, -3.4f);
				m_behaviorSpeculator.InterruptAndDisable();
			}
			m_concernTimer += BraveTime.DeltaTime;
			vector = EnemyTargetPos + new Vector2(1.5f, 0f).Rotate(Time.realtimeSinceStartup / 2f * -360f);
		}
		Vector2 vector2 = vector - base.transform.position.XY();
		if (vector2.magnitude < EnemySpeed * BraveTime.DeltaTime)
		{
			m_srb.Velocity = vector2 / BraveTime.DeltaTime;
		}
		else
		{
			m_srb.Velocity = vector2.normalized * EnemySpeed;
		}
	}

	private void UpdatePath(Vector2 newPosition)
	{
		float num = Vector2.Distance(newPosition, m_path.First.Value);
		for (int i = 0; i < m_segmentData.Count; i++)
		{
			BabyDragunSegment value = m_segmentData[i];
			value.pathDist += num;
			m_segmentData[i] = value;
		}
		if (m_pathSegmentLength > 0.05f)
		{
			m_path.AddFirst(newPosition);
		}
		else
		{
			m_path.First.Value = newPosition;
		}
		m_pathSegmentLength = Vector2.Distance(m_path.First.Value, m_path.First.Next.Value);
	}

	private float GetPerp(float totalPathDist, int segmentIndex)
	{
		float b = Mathf.Sin(totalPathDist * SinWave1Multiplier + Time.time * SinTimeMultiplier) * SinAmplitude;
		float t = 1f - (float)segmentIndex / (1f * (float)Segments.Count);
		return Mathf.Lerp(0f, b, t);
	}

	private void UpdatePathSegment(LinkedListNode<Vector2> pathNode, float totalPathDist, float thisNodeDist, int segmentIndex)
	{
		BabyDragunSegment value = m_segmentData[segmentIndex];
		float num = ((segmentIndex > 0) ? m_segmentData[segmentIndex - 1].pathDist : 0f);
		Transform transform = Segments[segmentIndex];
		float num2 = value.pathDist - num;
		float num3 = value.initialStartingDistance;
		bool flag = false;
		if (num2 < num3)
		{
			num3 = num2;
		}
		float num4 = 0f - thisNodeDist;
		while (pathNode.Next != null)
		{
			float num5 = Vector2.Distance(pathNode.Next.Value, pathNode.Value);
			if (num4 + num5 >= num3)
			{
				float num6 = num3 - num4;
				if (!flag)
				{
					Vector2 vector = Vector2.Lerp(pathNode.Value, pathNode.Next.Value, num6 / num5);
					transform.position = vector;
					SegmentSprites[segmentIndex].UpdateZDepth();
				}
				value.pathDist = totalPathDist + num3;
				if (segmentIndex + 1 < m_segmentData.Count)
				{
					UpdatePathSegment(pathNode, totalPathDist + num3, num6, segmentIndex + 1);
				}
				else
				{
					while (m_path.Last != pathNode.Next)
					{
						m_path.RemoveLast();
					}
				}
				m_segmentData[segmentIndex] = value;
				return;
			}
			num4 += num5;
			pathNode = pathNode.Next;
		}
		m_segmentData[segmentIndex] = value;
	}

	public void OnPostRigidbodyMovement(SpeculativeRigidbody s, Vector2 v, IntVector2 iv)
	{
		if (base.enabled)
		{
			Vector2 vector = base.transform.position.XY();
			UpdatePath(vector);
			UpdatePathSegment(m_path.First, 0f, 0f, 0);
			m_lastBasePosition = vector;
			m_lastVelocityAvg = BraveMathCollege.MovingAverage(m_lastVelocityAvg, m_srb.Velocity, 8);
			if (float.IsNaN(m_lastVelocityAvg.x) || float.IsNaN(m_lastVelocityAvg.y))
			{
				m_lastVelocityAvg = Vector2.zero;
			}
			HeadAnimator.LockFacingDirection = true;
			if (Time.frameCount - m_lastChangedFacingFrame >= 3)
			{
				HeadAnimator.FacingDirection = m_lastVelocityAvg.ToAngle();
				m_lastChangedFacingFrame = Time.frameCount;
			}
		}
	}

	public void BecomeEnemy(DraGunController draGunController)
	{
		if (IsEnemy)
		{
			return;
		}
		PlayerOrbital component = GetComponent<PlayerOrbital>();
		component.DecoupleBabyDragun();
		m_srb.CollideWithOthers = false;
		IsEnemy = true;
		EnemyTargetPos = draGunController.transform.position + new Vector3(10f, 6f);
		EnemySpeed = EnemyBaseSpeed;
		Parent = UnityEngine.Object.FindObjectOfType<DraGunController>();
		ParentHeart = Parent.GetComponentsInChildren<AutoAimTarget>(true)[0];
		tk2dBaseSprite[] componentsInChildren = GetComponentsInChildren<tk2dBaseSprite>();
		tk2dBaseSprite[] array = componentsInChildren;
		foreach (tk2dBaseSprite tk2dBaseSprite2 in array)
		{
			if (!tk2dBaseSprite2.IsOutlineSprite)
			{
				tk2dBaseSprite2.HeightOffGround += 1f;
			}
		}
		m_bulletBank = GetComponent<AIBulletBank>();
		m_bulletBank.ActorName = Parent.aiActor.GetActorName();
		m_bulletBank.enabled = true;
		m_behaviorSpeculator = GetComponent<BehaviorSpeculator>();
		m_behaviorSpeculator.enabled = true;
		m_behaviorSpeculator.AttackCooldown = 5f;
	}
}
