using System;
using System.Collections.Generic;

public class DwarfEventListener : BraveBehaviour, IEventTriggerable
{
	[Serializable]
	public class Pair
	{
		public int eventIndex;

		public string playmakerEvent;
	}

	public List<Pair> events;

	public Action<int> OnTrigger;

	public void Trigger(int index)
	{
		if (OnTrigger != null)
		{
			OnTrigger(index);
		}
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i].eventIndex == index)
			{
				SendPlaymakerEvent(events[i].playmakerEvent);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
