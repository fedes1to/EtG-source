using System.Collections.Generic;
using UnityEngine;

public class ProjectileVolleyData : ScriptableObject
{
	public List<ProjectileModule> projectiles;

	public bool UsesBeamRotationLimiter;

	public float BeamRotationDegreesPerSecond = 30f;

	public bool ModulesAreTiers;

	public bool UsesShotgunStyleVelocityRandomizer;

	public float DecreaseFinalSpeedPercentMin = -5f;

	public float IncreaseFinalSpeedPercentMax = 5f;

	public float GetVolleySpeedMod()
	{
		if (UsesShotgunStyleVelocityRandomizer)
		{
			return 1f + Random.Range(DecreaseFinalSpeedPercentMin, IncreaseFinalSpeedPercentMax) / 100f;
		}
		return 1f;
	}

	public void InitializeFrom(ProjectileVolleyData source)
	{
		projectiles = new List<ProjectileModule>();
		UsesShotgunStyleVelocityRandomizer = source.UsesShotgunStyleVelocityRandomizer;
		DecreaseFinalSpeedPercentMin = source.DecreaseFinalSpeedPercentMin;
		IncreaseFinalSpeedPercentMax = source.IncreaseFinalSpeedPercentMax;
		for (int i = 0; i < source.projectiles.Count; i++)
		{
			projectiles.Add(ProjectileModule.CreateClone(source.projectiles[i]));
		}
		UsesBeamRotationLimiter = source.UsesBeamRotationLimiter;
		BeamRotationDegreesPerSecond = source.BeamRotationDegreesPerSecond;
		ModulesAreTiers = source.ModulesAreTiers;
	}
}
