using UnityEngine;

public class BashelliskBodyPickupController : BraveBehaviour
{
	public Transform center;

	public AIActorBuffEffect buffEffect;

	public void Awake()
	{
		base.aiActor.PreventBlackPhantom = true;
	}

	public void Update()
	{
		ShootBehavior shootBehavior = base.aiActor.behaviorSpeculator.AttackBehaviors[0] as ShootBehavior;
		if (base.aiActor.CanTargetEnemies)
		{
			shootBehavior.Cooldown = 0.15f;
			shootBehavior.BulletName = "fast";
		}
		else
		{
			shootBehavior.Cooldown = 1.5f;
			shootBehavior.BulletName = "default";
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
