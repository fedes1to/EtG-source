using System.Collections.Generic;
using UnityEngine;

public class SpawnGoopBehavior : BasicAttackBehavior
{
	public List<Vector2> roomOffsets;

	public GoopDefinition goopToUse;

	public float goopRadius = 3f;

	public float goopSpeed = 0.5f;

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		Vector2 vector = BraveUtility.RandomElement(roomOffsets);
		Vector2 center = m_aiActor.ParentRoom.area.UnitBottomLeft + vector;
		DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopToUse);
		goopManagerForGoopType.TimedAddGoopCircle(center, goopRadius, goopSpeed);
		UpdateCooldowns();
		return BehaviorResult.SkipAllRemainingBehaviors;
	}
}
