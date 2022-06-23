using System;
using Dungeonator;
using UnityEngine;

public class KillOnRoomClear : BraveBehaviour
{
	[CheckAnimation(null)]
	public string overrideDeathAnim;

	public bool preventExplodeOnDeath;

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
		if (!string.IsNullOrEmpty(overrideDeathAnim) && (bool)base.aiAnimator)
		{
			bool flag = false;
			for (int i = 0; i < base.aiAnimator.OtherAnimations.Count; i++)
			{
				if (base.aiAnimator.OtherAnimations[i].name == "death")
				{
					base.aiAnimator.OtherAnimations[i].anim.Type = DirectionalAnimation.DirectionType.Single;
					base.aiAnimator.OtherAnimations[i].anim.Prefix = overrideDeathAnim;
					flag = true;
				}
			}
			if (!flag)
			{
				AIAnimator.NamedDirectionalAnimation namedDirectionalAnimation = new AIAnimator.NamedDirectionalAnimation();
				namedDirectionalAnimation.name = "death";
				namedDirectionalAnimation.anim = new DirectionalAnimation();
				namedDirectionalAnimation.anim.Type = DirectionalAnimation.DirectionType.Single;
				namedDirectionalAnimation.anim.Prefix = overrideDeathAnim;
				namedDirectionalAnimation.anim.Flipped = new DirectionalAnimation.FlipType[1];
				base.aiAnimator.OtherAnimations.Add(namedDirectionalAnimation);
			}
		}
		if (preventExplodeOnDeath)
		{
			ExplodeOnDeath component = GetComponent<ExplodeOnDeath>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		base.healthHaver.PreventAllDamage = false;
		base.healthHaver.ApplyDamage(100000f, Vector2.zero, "Death on Room Claer", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
	}
}
