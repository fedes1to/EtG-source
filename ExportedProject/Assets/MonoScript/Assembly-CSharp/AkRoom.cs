using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[AddComponentMenu("Wwise/AkRoom")]
public class AkRoom : MonoBehaviour
{
	public class PriorityList
	{
		private class CompareByPriority : IComparer<AkRoom>
		{
			public virtual int Compare(AkRoom a, AkRoom b)
			{
				int num = a.priority.CompareTo(b.priority);
				if (num == 0 && a != b)
				{
					return 1;
				}
				return -num;
			}
		}

		private static readonly CompareByPriority s_compareByPriority = new CompareByPriority();

		public List<AkRoom> rooms = new List<AkRoom>();

		public ulong GetHighestPriorityRoomID()
		{
			AkRoom highestPriorityRoom = GetHighestPriorityRoom();
			return (!(highestPriorityRoom == null)) ? highestPriorityRoom.GetID() : INVALID_ROOM_ID;
		}

		public AkRoom GetHighestPriorityRoom()
		{
			if (rooms.Count == 0)
			{
				return null;
			}
			return rooms[0];
		}

		public void Add(AkRoom room)
		{
			int num = BinarySearch(room);
			if (num < 0)
			{
				rooms.Insert(~num, room);
			}
		}

		public void Remove(AkRoom room)
		{
			rooms.Remove(room);
		}

		public bool Contains(AkRoom room)
		{
			return BinarySearch(room) >= 0;
		}

		public int BinarySearch(AkRoom room)
		{
			return rooms.BinarySearch(room, s_compareByPriority);
		}
	}

	public static ulong INVALID_ROOM_ID = ulong.MaxValue;

	private static int RoomCount;

	[Tooltip("Higher number has a higher priority")]
	public int priority;

	public AuxBus reverbAuxBus;

	[Range(0f, 1f)]
	public float reverbLevel = 1f;

	[Range(0f, 1f)]
	public float wallOcclusion = 1f;

	public static bool IsSpatialAudioEnabled
	{
		get
		{
			return AkSpatialAudioListener.TheSpatialAudioListener != null && RoomCount > 0;
		}
	}

	public ulong GetID()
	{
		return (ulong)GetInstanceID();
	}

	private void OnEnable()
	{
		AkRoomParams akRoomParams = new AkRoomParams();
		akRoomParams.Up.X = base.transform.up.x;
		akRoomParams.Up.Y = base.transform.up.y;
		akRoomParams.Up.Z = base.transform.up.z;
		akRoomParams.Front.X = base.transform.forward.x;
		akRoomParams.Front.Y = base.transform.forward.y;
		akRoomParams.Front.Z = base.transform.forward.z;
		akRoomParams.ReverbAuxBus = (uint)reverbAuxBus.ID;
		akRoomParams.ReverbLevel = reverbLevel;
		akRoomParams.WallOcclusion = wallOcclusion;
		RoomCount++;
		AkSoundEngine.SetRoom(GetID(), akRoomParams, base.name);
	}

	private void OnDisable()
	{
		RoomCount--;
		AkSoundEngine.RemoveRoom(GetID());
	}

	private void OnTriggerEnter(Collider in_other)
	{
		AkSpatialAudioBase[] componentsInChildren = in_other.GetComponentsInChildren<AkSpatialAudioBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].enabled)
			{
				componentsInChildren[i].EnteredRoom(this);
			}
		}
	}

	private void OnTriggerExit(Collider in_other)
	{
		AkSpatialAudioBase[] componentsInChildren = in_other.GetComponentsInChildren<AkSpatialAudioBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].enabled)
			{
				componentsInChildren[i].ExitedRoom(this);
			}
		}
	}
}
