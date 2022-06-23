using System;
using UnityEngine;

public class dfFollowObjectSorter : MonoBehaviour
{
	private class FollowSortRecord : IComparable<FollowSortRecord>
	{
		public float distance;

		public dfFollowObject follow;

		public dfControl control;

		public FollowSortRecord(dfFollowObject follow)
		{
			this.follow = follow;
			control = follow.GetComponent<dfControl>();
		}

		public int CompareTo(FollowSortRecord other)
		{
			return other.distance.CompareTo(distance);
		}
	}

	private static dfFollowObjectSorter _instance;

	private static dfList<FollowSortRecord> list = new dfList<FollowSortRecord>();

	public static dfFollowObjectSorter Instance
	{
		get
		{
			lock (typeof(dfFollowObjectSorter))
			{
				if (_instance == null && Application.isPlaying)
				{
					GameObject gameObject = new GameObject("Follow Object Sorter");
					_instance = gameObject.AddComponent<dfFollowObjectSorter>();
					list.Clear();
				}
				return _instance;
			}
		}
	}

	public static void Register(dfFollowObject follow)
	{
		if (Application.isPlaying)
		{
			Instance.register(follow);
		}
	}

	public static void Unregister(dfFollowObject follow)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].follow == follow)
			{
				list.RemoveAt(i);
				break;
			}
		}
	}

	public void Update()
	{
		int num = int.MaxValue;
		for (int i = 0; i < list.Count; i++)
		{
			FollowSortRecord followSortRecord = list[i];
			if ((bool)followSortRecord.follow.attach)
			{
				followSortRecord.distance = getDistance(followSortRecord.follow);
				if (followSortRecord.control.ZOrder < num)
				{
					num = followSortRecord.control.ZOrder;
				}
			}
		}
		list.Sort();
		for (int j = 0; j < list.Count; j++)
		{
			dfControl control = list[j].control;
			control.ZOrder = num++;
		}
	}

	private void register(dfFollowObject follow)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].follow == follow)
			{
				return;
			}
		}
		list.Add(new FollowSortRecord(follow));
	}

	private float getDistance(dfFollowObject follow)
	{
		return (follow.mainCamera.transform.position - follow.attach.transform.position).sqrMagnitude;
	}
}
