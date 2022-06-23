using System;
using UnityEngine;

public class BurstVariatorGunModifier : MonoBehaviour
{
	public int NumDiceRolls = 2;

	public int DiceMin = 1;

	public int DiceMax = 6;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnPostFired, new Action<PlayerController, Gun>(PostFired));
	}

	private int DiceRoll()
	{
		return UnityEngine.Random.Range(DiceMin, DiceMax + 1);
	}

	private void PostFired(PlayerController arg1, Gun arg2)
	{
		if (arg2.MidBurstFire)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < NumDiceRolls; i++)
		{
			num += DiceRoll();
		}
		if (arg2.RawSourceVolley != null)
		{
			for (int j = 0; j < arg2.RawSourceVolley.projectiles.Count; j++)
			{
				if (arg2.RawSourceVolley.projectiles[j].shootStyle == ProjectileModule.ShootStyle.Burst)
				{
					arg2.RawSourceVolley.projectiles[j].burstShotCount = num;
				}
			}
		}
		else if (arg2.singleModule.shootStyle == ProjectileModule.ShootStyle.Burst)
		{
			arg2.singleModule.burstShotCount = num;
		}
		if (!(arg2.modifiedVolley != null))
		{
			return;
		}
		for (int k = 0; k < arg2.modifiedVolley.projectiles.Count; k++)
		{
			if (arg2.modifiedVolley.projectiles[k].shootStyle == ProjectileModule.ShootStyle.Burst)
			{
				arg2.modifiedVolley.projectiles[k].burstShotCount = num;
			}
		}
	}
}
