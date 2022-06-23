using System;
using UnityEngine;

[Serializable]
public class BloodthirstSettings
{
	public int NumKillsForHealRequiredBase = 5;

	public int NumKillsAddedPerHealthGained = 5;

	public int NumKillsRequiredCap = 50;

	public float Radius = 5f;

	public float DamagePerSecond = 30f;

	[Range(0f, 1f)]
	public float PercentAffected = 0.5f;
}
