using UnityEngine;

public class RationItem : PlayerItem
{
	public float healingAmount = 2f;

	public GameObject healVFX;

	protected override void DoEffect(PlayerController user)
	{
		user.healthHaver.ApplyHealing(healingAmount);
		AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
		if (healVFX != null)
		{
			user.PlayEffectOnActor(healVFX, Vector3.zero);
		}
	}

	public void DoHealOnDeath(PlayerController user)
	{
		DoEffect(user);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
