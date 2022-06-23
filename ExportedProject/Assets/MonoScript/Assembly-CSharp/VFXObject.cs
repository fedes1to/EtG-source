using System;
using UnityEngine;

[Serializable]
public class VFXObject
{
	public GameObject effect;

	public bool orphaned;

	public bool attached = true;

	public bool persistsOnDeath;

	public bool usesZHeight;

	public float zHeight;

	public VFXAlignment alignment;

	[HideInInspector]
	public bool destructible;
}
