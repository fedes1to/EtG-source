using System;
using UnityEngine;

public class PickupIdentifierAttribute : PropertyAttribute, DatabaseIdentifierAttribute
{
	public Type PickupType;

	public PickupIdentifierAttribute()
	{
		PickupType = typeof(PickupObject);
	}

	public PickupIdentifierAttribute(Type type)
	{
		PickupType = type;
	}
}
