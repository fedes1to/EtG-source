using System;
using Dungeonator;

public class RoomSealDisabler : BraveBehaviour
{
	public bool MatchRoomState = true;

	private void Start()
	{
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		absoluteRoom.OnSealChanged = (Action<bool>)Delegate.Combine(absoluteRoom.OnSealChanged, new Action<bool>(HandleSealStateChanged));
		HandleSealStateChanged(false);
	}

	private void HandleSealStateChanged(bool isSealed)
	{
		if (MatchRoomState)
		{
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.enabled = isSealed;
			}
			base.gameObject.SetActive(isSealed);
		}
		else
		{
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.enabled = !isSealed;
			}
			base.gameObject.SetActive(!isSealed);
		}
	}
}
