using UnityEngine;

public abstract class AkSpatialAudioBase : MonoBehaviour
{
	private readonly AkRoom.PriorityList roomPriorityList = new AkRoom.PriorityList();

	protected void SetGameObjectInHighestPriorityRoom()
	{
		ulong highestPriorityRoomID = roomPriorityList.GetHighestPriorityRoomID();
		AkSoundEngine.SetGameObjectInRoom(base.gameObject, highestPriorityRoomID);
	}

	public void EnteredRoom(AkRoom room)
	{
		roomPriorityList.Add(room);
		SetGameObjectInHighestPriorityRoom();
	}

	public void ExitedRoom(AkRoom room)
	{
		roomPriorityList.Remove(room);
		SetGameObjectInHighestPriorityRoom();
	}

	public void SetGameObjectInRoom()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0f);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			AkRoom component = collider.gameObject.GetComponent<AkRoom>();
			if (component != null)
			{
				roomPriorityList.Add(component);
			}
		}
		SetGameObjectInHighestPriorityRoom();
	}
}
