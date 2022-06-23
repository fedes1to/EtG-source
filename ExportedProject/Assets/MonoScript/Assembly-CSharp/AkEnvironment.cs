using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("Wwise/AkEnvironment")]
[ExecuteInEditMode]
public class AkEnvironment : MonoBehaviour
{
	public class AkEnvironment_CompareByPriority : IComparer<AkEnvironment>
	{
		public virtual int Compare(AkEnvironment a, AkEnvironment b)
		{
			int num = a.priority.CompareTo(b.priority);
			return (num == 0 && a != b) ? 1 : num;
		}
	}

	public class AkEnvironment_CompareBySelectionAlgorithm : AkEnvironment_CompareByPriority
	{
		public override int Compare(AkEnvironment a, AkEnvironment b)
		{
			if (a.isDefault)
			{
				return (!b.isDefault) ? 1 : base.Compare(a, b);
			}
			if (b.isDefault)
			{
				return -1;
			}
			if (a.excludeOthers)
			{
				return (!b.excludeOthers) ? (-1) : base.Compare(a, b);
			}
			return b.excludeOthers ? 1 : base.Compare(a, b);
		}
	}

	public const int MAX_NB_ENVIRONMENTS = 4;

	public static AkEnvironment_CompareByPriority s_compareByPriority = new AkEnvironment_CompareByPriority();

	public static AkEnvironment_CompareBySelectionAlgorithm s_compareBySelectionAlgorithm = new AkEnvironment_CompareBySelectionAlgorithm();

	public bool excludeOthers;

	public bool isDefault;

	public int m_auxBusID;

	private Collider m_Collider;

	public int priority;

	public uint GetAuxBusID()
	{
		return (uint)m_auxBusID;
	}

	public void SetAuxBusID(int in_auxBusID)
	{
		m_auxBusID = in_auxBusID;
	}

	public float GetAuxSendValueForPosition(Vector3 in_position)
	{
		return 1f;
	}

	public Collider GetCollider()
	{
		return m_Collider;
	}

	public void Awake()
	{
		m_Collider = GetComponent<Collider>();
	}
}
