using System;

[Serializable]
public class RoomEventDefinition
{
	public RoomEventTriggerCondition condition;

	public RoomEventTriggerAction action;

	public RoomEventDefinition(RoomEventTriggerCondition c, RoomEventTriggerAction a)
	{
		condition = c;
		action = a;
	}
}
