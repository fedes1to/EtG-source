using System;
using Dungeonator;

public class MakeVulnerableOnRoomClear : BraveBehaviour
{
	[CheckDirectionalAnimation(null)]
	public string vulnerableAnim;

	public bool disableBehaviors = true;

	public void Start()
	{
		RoomHandler parentRoom = base.aiActor.ParentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom.OnEnemiesCleared, new Action(RoomCleared));
	}

	protected override void OnDestroy()
	{
		if ((bool)base.aiActor && base.aiActor.ParentRoom != null)
		{
			RoomHandler parentRoom = base.aiActor.ParentRoom;
			parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(RoomCleared));
		}
		base.OnDestroy();
	}

	private void RoomCleared()
	{
		if (base.healthHaver.PreventAllDamage)
		{
			base.healthHaver.PreventAllDamage = false;
			if (!string.IsNullOrEmpty(vulnerableAnim))
			{
				base.aiAnimator.PlayUntilCancelled(vulnerableAnim, true);
			}
			if (disableBehaviors)
			{
				base.aiActor.enabled = false;
				base.aiActor.IsHarmlessEnemy = true;
				base.behaviorSpeculator.InterruptAndDisable();
			}
			base.aiActor.CollisionDamage = 0f;
			base.aiActor.CollisionKnockbackStrength = 0f;
		}
	}
}
