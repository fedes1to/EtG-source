using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BraveDynamicTree
{
	public class b2DynamicTree : RigidbodyContainer
	{
		private int m_root;

		private b2TreeNode[] m_nodes;

		private int m_nodeCount;

		private int m_nodeCapacity;

		private int m_freeList;

		private Stack<int> m_stack = new Stack<int>(256);

		public const int b2_nullNode = -1;

		private const float b2_aabbExtension = 0.1f;

		private const float b2_aabbMultiplier = 2f;

		public b2DynamicTree()
		{
			m_root = -1;
			m_nodeCapacity = 16;
			m_nodeCount = 0;
			m_nodes = new b2TreeNode[m_nodeCapacity];
			for (int i = 0; i < m_nodeCapacity - 1; i++)
			{
				m_nodes[i] = new b2TreeNode();
				m_nodes[i].next = i + 1;
				m_nodes[i].height = -1;
			}
			m_nodes[m_nodeCapacity - 1] = new b2TreeNode();
			m_nodes[m_nodeCapacity - 1].next = -1;
			m_nodes[m_nodeCapacity - 1].height = -1;
			m_freeList = 0;
		}

		[Conditional("BRAVE_INTERNAL")]
		public static void b2Assert(bool mustBeTrue)
		{
			if (!mustBeTrue)
			{
				throw new Exception("Failed assert in b2DynamicTree!");
			}
		}

		public int CreateProxy(b2AABB aabb, SpeculativeRigidbody rigidbody)
		{
			int num = AllocateNode();
			Vector2 vector = new Vector2(0.1f, 0.1f);
			m_nodes[num].fatAabb.lowerBound = aabb.lowerBound - vector;
			m_nodes[num].fatAabb.upperBound = aabb.upperBound + vector;
			m_nodes[num].tightAabb = m_nodes[num].fatAabb;
			m_nodes[num].rigidbody = rigidbody;
			m_nodes[num].height = 0;
			InsertLeaf(num);
			return num;
		}

		public void DestroyProxy(int proxyId)
		{
			RemoveLeaf(proxyId);
			FreeNode(proxyId);
		}

		public bool MoveProxy(int proxyId, b2AABB aabb, Vector2 displacement)
		{
			float num = aabb.lowerBound.x - 0.1f;
			float num2 = aabb.lowerBound.y - 0.1f;
			float num3 = aabb.upperBound.x + 0.1f;
			float num4 = aabb.upperBound.y + 0.1f;
			m_nodes[proxyId].tightAabb = new b2AABB(num, num2, num3, num4);
			if (m_nodes[proxyId].fatAabb.Contains(aabb))
			{
				return false;
			}
			RemoveLeaf(proxyId);
			Vector2 vector = 2f * displacement;
			if (vector.x < 0f)
			{
				num += vector.x;
			}
			else
			{
				num3 += vector.x;
			}
			if (vector.y < 0f)
			{
				num2 += vector.y;
			}
			else
			{
				num4 += vector.y;
			}
			m_nodes[proxyId].fatAabb = new b2AABB(num, num2, num3, num4);
			InsertLeaf(proxyId);
			return true;
		}

		public SpeculativeRigidbody GetSpeculativeRigidbody(int proxyId)
		{
			return m_nodes[proxyId].rigidbody;
		}

		public b2AABB GetFatAABB(int proxyId)
		{
			return m_nodes[proxyId].fatAabb;
		}

		public void Query(b2AABB aabb, Func<SpeculativeRigidbody, bool> callback)
		{
			m_stack.Clear();
			m_stack.Push(m_root);
			while (m_stack.Count > 0)
			{
				int num = m_stack.Pop();
				if (num == -1)
				{
					continue;
				}
				b2TreeNode b2TreeNode2 = m_nodes[num];
				b2AABB fatAabb = b2TreeNode2.fatAabb;
				if (!(aabb.lowerBound.x <= fatAabb.upperBound.x) || !(fatAabb.lowerBound.x <= aabb.upperBound.x) || !(aabb.lowerBound.y <= fatAabb.upperBound.y) || !(fatAabb.lowerBound.y <= aabb.upperBound.y))
				{
					continue;
				}
				if (b2TreeNode2.child1 == -1)
				{
					b2AABB tightAabb = b2TreeNode2.tightAabb;
					if (aabb.lowerBound.x <= tightAabb.upperBound.x && tightAabb.lowerBound.x <= aabb.upperBound.x && aabb.lowerBound.y <= tightAabb.upperBound.y && tightAabb.lowerBound.y <= aabb.upperBound.y && !callback(b2TreeNode2.rigidbody))
					{
						break;
					}
				}
				else
				{
					m_stack.Push(b2TreeNode2.child1);
					m_stack.Push(b2TreeNode2.child2);
				}
			}
		}

		public void RayCast(b2RayCastInput input, Func<b2RayCastInput, SpeculativeRigidbody, float> callback)
		{
			Vector2 p = input.p1;
			Vector2 p2 = input.p2;
			Vector2 a = p2 - p;
			if ((double)a.sqrMagnitude <= 0.0)
			{
				return;
			}
			a.Normalize();
			Vector2 vector = Vector2Extensions.Cross(1f, a);
			Vector2 lhs = vector.Abs();
			float num = input.maxFraction;
			Vector2 rhs = p + num * (p2 - p);
			b2AABB b = default(b2AABB);
			b.lowerBound = Vector2.Min(p, rhs);
			b.upperBound = Vector2.Max(p, rhs);
			m_stack.Clear();
			m_stack.Push(m_root);
			b2RayCastInput arg = default(b2RayCastInput);
			while (m_stack.Count > 0)
			{
				int num2 = m_stack.Pop();
				if (num2 == -1)
				{
					continue;
				}
				b2TreeNode b2TreeNode2 = m_nodes[num2];
				if (!b2AABB.b2TestOverlap(ref b2TreeNode2.fatAabb, ref b))
				{
					continue;
				}
				Vector2 center = b2TreeNode2.fatAabb.GetCenter();
				Vector2 extents = b2TreeNode2.fatAabb.GetExtents();
				float num3 = Mathf.Abs(Vector2.Dot(vector, p - center)) - Vector2.Dot(lhs, extents);
				if (num3 > 0f)
				{
					continue;
				}
				if (b2TreeNode2.IsLeaf())
				{
					arg.p1 = input.p1;
					arg.p2 = input.p2;
					arg.maxFraction = num;
					float num4 = callback(arg, b2TreeNode2.rigidbody);
					if (num4 == 0f)
					{
						break;
					}
					if (num4 > 0f)
					{
						num = num4;
						Vector2 rhs2 = p + num * (p2 - p);
						b.lowerBound = Vector2.Min(p, rhs2);
						b.upperBound = Vector2.Max(p, rhs2);
					}
				}
				else
				{
					m_stack.Push(b2TreeNode2.child1);
					m_stack.Push(b2TreeNode2.child2);
				}
			}
		}

		public void Validate()
		{
			ValidateStructure(m_root);
			ValidateMetrics(m_root);
			int num = 0;
			int num2 = m_freeList;
			while (num2 != -1)
			{
				num2 = m_nodes[num2].next;
				num++;
			}
		}

		public int GetHeight()
		{
			if (m_root == -1)
			{
				return 0;
			}
			return m_nodes[m_root].height;
		}

		public int GetMaxBalance()
		{
			int num = 0;
			for (int i = 0; i < m_nodeCapacity; i++)
			{
				b2TreeNode b2TreeNode2 = m_nodes[i];
				if (b2TreeNode2.height > 1)
				{
					int child = b2TreeNode2.child1;
					int child2 = b2TreeNode2.child2;
					int b = Mathf.Abs(m_nodes[child2].height - m_nodes[child].height);
					num = Mathf.Max(num, b);
				}
			}
			return num;
		}

		public float GetAreaRatio()
		{
			if (m_root == -1)
			{
				return 0f;
			}
			b2TreeNode b2TreeNode2 = m_nodes[m_root];
			float perimeter = b2TreeNode2.fatAabb.GetPerimeter();
			float num = 0f;
			for (int i = 0; i < m_nodeCapacity; i++)
			{
				b2TreeNode b2TreeNode3 = m_nodes[i];
				if (b2TreeNode3.height >= 0)
				{
					num += b2TreeNode3.fatAabb.GetPerimeter();
				}
			}
			return num / perimeter;
		}

		public void RebuildBottomUp()
		{
			int[] array = new int[m_nodeCount];
			int num = 0;
			for (int i = 0; i < m_nodeCapacity; i++)
			{
				if (m_nodes[i].height >= 0)
				{
					if (m_nodes[i].IsLeaf())
					{
						m_nodes[i].parent = -1;
						array[num] = i;
						num++;
					}
					else
					{
						FreeNode(i);
					}
				}
			}
			while (num > 1)
			{
				float num2 = float.MaxValue;
				int num3 = -1;
				int num4 = -1;
				for (int j = 0; j < num; j++)
				{
					b2AABB fatAabb = m_nodes[array[j]].fatAabb;
					for (int k = j + 1; k < num; k++)
					{
						b2AABB fatAabb2 = m_nodes[array[k]].fatAabb;
						b2AABB b2AABB2 = default(b2AABB);
						b2AABB2.Combine(fatAabb, fatAabb2);
						float perimeter = b2AABB2.GetPerimeter();
						if (perimeter < num2)
						{
							num3 = j;
							num4 = k;
							num2 = perimeter;
						}
					}
				}
				int num5 = array[num3];
				int num6 = array[num4];
				b2TreeNode b2TreeNode2 = m_nodes[num5];
				b2TreeNode b2TreeNode3 = m_nodes[num6];
				int num7 = AllocateNode();
				b2TreeNode b2TreeNode4 = m_nodes[num7];
				b2TreeNode4.child1 = num5;
				b2TreeNode4.child2 = num6;
				b2TreeNode4.height = 1 + Mathf.Max(b2TreeNode2.height, b2TreeNode3.height);
				b2TreeNode4.fatAabb.Combine(b2TreeNode2.fatAabb, b2TreeNode3.fatAabb);
				b2TreeNode4.parent = -1;
				b2TreeNode2.parent = num7;
				b2TreeNode3.parent = num7;
				array[num4] = array[num - 1];
				array[num3] = num7;
				num--;
			}
			m_root = array[0];
			Validate();
		}

		public void ShiftOrigin(Vector2 newOrigin)
		{
			for (int i = 0; i < m_nodeCapacity; i++)
			{
				m_nodes[i].fatAabb.lowerBound -= newOrigin;
				m_nodes[i].fatAabb.upperBound -= newOrigin;
			}
		}

		private int AllocateNode()
		{
			if (m_freeList == -1)
			{
				m_nodeCapacity *= 2;
				Array.Resize(ref m_nodes, m_nodeCapacity);
				for (int i = m_nodeCount; i < m_nodeCapacity - 1; i++)
				{
					m_nodes[i] = new b2TreeNode();
					m_nodes[i].next = i + 1;
					m_nodes[i].height = -1;
				}
				m_nodes[m_nodeCapacity - 1] = new b2TreeNode();
				m_nodes[m_nodeCapacity - 1].next = -1;
				m_nodes[m_nodeCapacity - 1].height = -1;
				m_freeList = m_nodeCount;
			}
			int freeList = m_freeList;
			m_freeList = m_nodes[freeList].next;
			m_nodes[freeList].parent = -1;
			m_nodes[freeList].child1 = -1;
			m_nodes[freeList].child2 = -1;
			m_nodes[freeList].height = 0;
			m_nodes[freeList].rigidbody = null;
			m_nodeCount++;
			return freeList;
		}

		private void FreeNode(int nodeId)
		{
			m_nodes[nodeId].next = m_freeList;
			m_nodes[nodeId].height = -1;
			m_freeList = nodeId;
			m_nodeCount--;
		}

		private void InsertLeaf(int leaf)
		{
			if (m_root == -1)
			{
				m_root = leaf;
				m_nodes[m_root].parent = -1;
				return;
			}
			b2AABB fatAabb = m_nodes[leaf].fatAabb;
			int num = m_root;
			while (!m_nodes[num].IsLeaf())
			{
				int child = m_nodes[num].child1;
				int child2 = m_nodes[num].child2;
				float perimeter = m_nodes[num].fatAabb.GetPerimeter();
				b2AABB b2AABB2 = default(b2AABB);
				b2AABB2.Combine(m_nodes[num].fatAabb, fatAabb);
				float perimeter2 = b2AABB2.GetPerimeter();
				float num2 = 2f * perimeter2;
				float num3 = 2f * (perimeter2 - perimeter);
				float num4;
				if (m_nodes[child].IsLeaf())
				{
					b2AABB b2AABB3 = default(b2AABB);
					b2AABB3.Combine(fatAabb, m_nodes[child].fatAabb);
					num4 = b2AABB3.GetPerimeter() + num3;
				}
				else
				{
					b2AABB b2AABB4 = default(b2AABB);
					b2AABB4.Combine(fatAabb, m_nodes[child].fatAabb);
					float perimeter3 = m_nodes[child].fatAabb.GetPerimeter();
					float perimeter4 = b2AABB4.GetPerimeter();
					num4 = perimeter4 - perimeter3 + num3;
				}
				float num5;
				if (m_nodes[child2].IsLeaf())
				{
					b2AABB b2AABB5 = default(b2AABB);
					b2AABB5.Combine(fatAabb, m_nodes[child2].fatAabb);
					num5 = b2AABB5.GetPerimeter() + num3;
				}
				else
				{
					b2AABB b2AABB6 = default(b2AABB);
					b2AABB6.Combine(fatAabb, m_nodes[child2].fatAabb);
					float perimeter5 = m_nodes[child2].fatAabb.GetPerimeter();
					float perimeter6 = b2AABB6.GetPerimeter();
					num5 = perimeter6 - perimeter5 + num3;
				}
				if (num2 < num4 && num2 < num5)
				{
					break;
				}
				num = ((!(num4 < num5)) ? child2 : child);
			}
			int num6 = num;
			int parent = m_nodes[num6].parent;
			int num7 = AllocateNode();
			m_nodes[num7].parent = parent;
			m_nodes[num7].rigidbody = null;
			m_nodes[num7].fatAabb.Combine(fatAabb, m_nodes[num6].fatAabb);
			m_nodes[num7].height = m_nodes[num6].height + 1;
			if (parent != -1)
			{
				if (m_nodes[parent].child1 == num6)
				{
					m_nodes[parent].child1 = num7;
				}
				else
				{
					m_nodes[parent].child2 = num7;
				}
				m_nodes[num7].child1 = num6;
				m_nodes[num7].child2 = leaf;
				m_nodes[num6].parent = num7;
				m_nodes[leaf].parent = num7;
			}
			else
			{
				m_nodes[num7].child1 = num6;
				m_nodes[num7].child2 = leaf;
				m_nodes[num6].parent = num7;
				m_nodes[leaf].parent = num7;
				m_root = num7;
			}
			for (num = m_nodes[leaf].parent; num != -1; num = m_nodes[num].parent)
			{
				num = Balance(num);
				int child3 = m_nodes[num].child1;
				int child4 = m_nodes[num].child2;
				m_nodes[num].height = 1 + Mathf.Max(m_nodes[child3].height, m_nodes[child4].height);
				m_nodes[num].fatAabb.Combine(m_nodes[child3].fatAabb, m_nodes[child4].fatAabb);
			}
		}

		private void RemoveLeaf(int leaf)
		{
			if (leaf == m_root)
			{
				m_root = -1;
				return;
			}
			int parent = m_nodes[leaf].parent;
			int parent2 = m_nodes[parent].parent;
			int num = ((m_nodes[parent].child1 != leaf) ? m_nodes[parent].child1 : m_nodes[parent].child2);
			if (parent2 != -1)
			{
				if (m_nodes[parent2].child1 == parent)
				{
					m_nodes[parent2].child1 = num;
				}
				else
				{
					m_nodes[parent2].child2 = num;
				}
				m_nodes[num].parent = parent2;
				FreeNode(parent);
				int num2;
				for (num2 = parent2; num2 != -1; num2 = m_nodes[num2].parent)
				{
					num2 = Balance(num2);
					int child = m_nodes[num2].child1;
					int child2 = m_nodes[num2].child2;
					m_nodes[num2].fatAabb.Combine(m_nodes[child].fatAabb, m_nodes[child2].fatAabb);
					m_nodes[num2].height = 1 + Mathf.Max(m_nodes[child].height, m_nodes[child2].height);
				}
			}
			else
			{
				m_root = num;
				m_nodes[num].parent = -1;
				FreeNode(parent);
			}
		}

		private int Balance(int iA)
		{
			b2TreeNode b2TreeNode2 = m_nodes[iA];
			if (b2TreeNode2.IsLeaf() || b2TreeNode2.height < 2)
			{
				return iA;
			}
			int child = b2TreeNode2.child1;
			int child2 = b2TreeNode2.child2;
			b2TreeNode b2TreeNode3 = m_nodes[child];
			b2TreeNode b2TreeNode4 = m_nodes[child2];
			int num = b2TreeNode4.height - b2TreeNode3.height;
			if (num > 1)
			{
				int child3 = b2TreeNode4.child1;
				int child4 = b2TreeNode4.child2;
				b2TreeNode b2TreeNode5 = m_nodes[child3];
				b2TreeNode b2TreeNode6 = m_nodes[child4];
				b2TreeNode4.child1 = iA;
				b2TreeNode4.parent = b2TreeNode2.parent;
				b2TreeNode2.parent = child2;
				if (b2TreeNode4.parent != -1)
				{
					if (m_nodes[b2TreeNode4.parent].child1 == iA)
					{
						m_nodes[b2TreeNode4.parent].child1 = child2;
					}
					else
					{
						m_nodes[b2TreeNode4.parent].child2 = child2;
					}
				}
				else
				{
					m_root = child2;
				}
				if (b2TreeNode5.height > b2TreeNode6.height)
				{
					b2TreeNode4.child2 = child3;
					b2TreeNode2.child2 = child4;
					b2TreeNode6.parent = iA;
					b2TreeNode2.fatAabb.Combine(b2TreeNode3.fatAabb, b2TreeNode6.fatAabb);
					b2TreeNode4.fatAabb.Combine(b2TreeNode2.fatAabb, b2TreeNode5.fatAabb);
					b2TreeNode2.height = 1 + Mathf.Max(b2TreeNode3.height, b2TreeNode6.height);
					b2TreeNode4.height = 1 + Mathf.Max(b2TreeNode2.height, b2TreeNode5.height);
				}
				else
				{
					b2TreeNode4.child2 = child4;
					b2TreeNode2.child2 = child3;
					b2TreeNode5.parent = iA;
					b2TreeNode2.fatAabb.Combine(b2TreeNode3.fatAabb, b2TreeNode5.fatAabb);
					b2TreeNode4.fatAabb.Combine(b2TreeNode2.fatAabb, b2TreeNode6.fatAabb);
					b2TreeNode2.height = 1 + Mathf.Max(b2TreeNode3.height, b2TreeNode5.height);
					b2TreeNode4.height = 1 + Mathf.Max(b2TreeNode2.height, b2TreeNode6.height);
				}
				return child2;
			}
			if (num < -1)
			{
				int child5 = b2TreeNode3.child1;
				int child6 = b2TreeNode3.child2;
				b2TreeNode b2TreeNode7 = m_nodes[child5];
				b2TreeNode b2TreeNode8 = m_nodes[child6];
				b2TreeNode3.child1 = iA;
				b2TreeNode3.parent = b2TreeNode2.parent;
				b2TreeNode2.parent = child;
				if (b2TreeNode3.parent != -1)
				{
					if (m_nodes[b2TreeNode3.parent].child1 == iA)
					{
						m_nodes[b2TreeNode3.parent].child1 = child;
					}
					else
					{
						m_nodes[b2TreeNode3.parent].child2 = child;
					}
				}
				else
				{
					m_root = child;
				}
				if (b2TreeNode7.height > b2TreeNode8.height)
				{
					b2TreeNode3.child2 = child5;
					b2TreeNode2.child1 = child6;
					b2TreeNode8.parent = iA;
					b2TreeNode2.fatAabb.Combine(b2TreeNode4.fatAabb, b2TreeNode8.fatAabb);
					b2TreeNode3.fatAabb.Combine(b2TreeNode2.fatAabb, b2TreeNode7.fatAabb);
					b2TreeNode2.height = 1 + Mathf.Max(b2TreeNode4.height, b2TreeNode8.height);
					b2TreeNode3.height = 1 + Mathf.Max(b2TreeNode2.height, b2TreeNode7.height);
				}
				else
				{
					b2TreeNode3.child2 = child6;
					b2TreeNode2.child1 = child5;
					b2TreeNode7.parent = iA;
					b2TreeNode2.fatAabb.Combine(b2TreeNode4.fatAabb, b2TreeNode7.fatAabb);
					b2TreeNode3.fatAabb.Combine(b2TreeNode2.fatAabb, b2TreeNode8.fatAabb);
					b2TreeNode2.height = 1 + Mathf.Max(b2TreeNode4.height, b2TreeNode7.height);
					b2TreeNode3.height = 1 + Mathf.Max(b2TreeNode2.height, b2TreeNode8.height);
				}
				return child;
			}
			return iA;
		}

		private int ComputeHeight(int nodeId)
		{
			b2TreeNode b2TreeNode2 = m_nodes[nodeId];
			if (b2TreeNode2.IsLeaf())
			{
				return 0;
			}
			int a = ComputeHeight(b2TreeNode2.child1);
			int b = ComputeHeight(b2TreeNode2.child2);
			return 1 + Mathf.Max(a, b);
		}

		private int ComputeHeight()
		{
			return ComputeHeight(m_root);
		}

		private void ValidateStructure(int index)
		{
			if (index != -1)
			{
				b2TreeNode b2TreeNode2 = m_nodes[index];
				int child = b2TreeNode2.child1;
				int child2 = b2TreeNode2.child2;
				if (!b2TreeNode2.IsLeaf())
				{
					ValidateStructure(child);
					ValidateStructure(child2);
				}
			}
		}

		private void ValidateMetrics(int index)
		{
			if (index != -1)
			{
				b2TreeNode b2TreeNode2 = m_nodes[index];
				int child = b2TreeNode2.child1;
				int child2 = b2TreeNode2.child2;
				if (!b2TreeNode2.IsLeaf())
				{
					int height = m_nodes[child].height;
					int height2 = m_nodes[child2].height;
					int num = 1 + Mathf.Max(height, height2);
					default(b2AABB).Combine(m_nodes[child].fatAabb, m_nodes[child2].fatAabb);
					ValidateMetrics(child);
					ValidateMetrics(child2);
				}
			}
		}
	}
}
