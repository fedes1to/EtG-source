using UnityEngine;

public class JetpackItem : PlayerItem
{
	public GameObject prefabToAttachToPlayer;

	private GameObject instanceJetpack;

	private tk2dSprite instanceJetpackSprite;

	protected override void DoEffect(PlayerController user)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			PreventCooldownBar = true;
			AkSoundEngine.PostEvent("Play_OBJ_jetpack_start_01", base.gameObject);
			base.IsCurrentlyActive = true;
			user.SetIsFlying(true, "jetpack");
			instanceJetpack = user.RegisterAttachedObject(prefabToAttachToPlayer, "jetpack");
			instanceJetpackSprite = instanceJetpack.GetComponent<tk2dSprite>();
		}
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Stop_OBJ_jetpack_loop_01", base.gameObject);
		base.IsCurrentlyActive = false;
		user.SetIsFlying(false, "jetpack");
		user.DeregisterAttachedObject(instanceJetpack);
		instanceJetpackSprite = null;
		user.stats.RecalculateStats(user);
	}

	public override void Update()
	{
		base.Update();
		if (base.IsCurrentlyActive)
		{
			DeadlyDeadlyGoopManager.IgniteGoopsCircle(instanceJetpackSprite.WorldBottomCenter, 0.5f);
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				DoActiveEffect(LastOwner);
			}
		}
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			DoActiveEffect(user);
		}
	}

	public override void OnItemSwitched(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			DoActiveEffect(user);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
