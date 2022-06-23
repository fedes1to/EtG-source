using System;
using UnityEngine;

[Serializable]
public class AkGameObjPositionOffsetData
{
	public bool KeepMe;

	public Vector3 positionOffset;

	public AkGameObjPositionOffsetData(bool IReallyWantToBeConstructed = false)
	{
		KeepMe = IReallyWantToBeConstructed;
	}
}
