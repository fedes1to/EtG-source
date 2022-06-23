using UnityEngine;

public class KillOnRoomUnseal : BraveBehaviour
{
	public void Update()
	{
		if (base.aiActor.enabled && base.behaviorSpeculator.enabled && !base.aiActor.ParentRoom.IsSealed && !base.aiAnimator.IsPlaying("spawn") && !base.aiAnimator.IsPlaying("awaken"))
		{
			base.enabled = false;
			base.healthHaver.PreventAllDamage = false;
			base.healthHaver.ApplyDamage(100000f, Vector2.zero, "Room Clear", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
