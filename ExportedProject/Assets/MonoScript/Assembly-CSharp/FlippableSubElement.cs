using System;
using Dungeonator;
using UnityEngine;

[Serializable]
public class FlippableSubElement
{
	public enum SubElementStyle
	{
		ANIMATOR,
		GOOP
	}

	public SubElementStyle elementStyle;

	public bool isMandatory;

	public bool onlyOneOfThese;

	public float spawnChance = 1f;

	public float flipDelay;

	public bool requiresDirection;

	public DungeonData.Direction requiredDirection;

	[ShowInInspectorIf("elementStyle", 0, false)]
	public tk2dSpriteAnimator targetAnimator;

	[ShowInInspectorIf("elementStyle", 0, false)]
	public string northAnimation;

	[ShowInInspectorIf("elementStyle", 0, false)]
	public string eastAnimation;

	[ShowInInspectorIf("elementStyle", 0, false)]
	public string southAnimation;

	[ShowInInspectorIf("elementStyle", 0, false)]
	public string westAnimation;

	[ShowInInspectorIf("elementStyle", 0, false)]
	public float additionalHeightModification;

	[ShowInInspectorIf("elementStyle", 1, false)]
	public GoopDefinition goopToUse;

	[ShowInInspectorIf("elementStyle", 1, false)]
	public float goopConeLength = 5f;

	[ShowInInspectorIf("elementStyle", 1, false)]
	public float goopConeArc = 45f;

	[ShowInInspectorIf("elementStyle", 1, false)]
	public AnimationCurve goopCurve;

	[ShowInInspectorIf("elementStyle", 1, false)]
	public float goopDuration = 0.5f;

	public void Trigger(DungeonData.Direction flipDirection, tk2dBaseSprite sourceTable)
	{
		if (requiresDirection && requiredDirection != flipDirection)
		{
			return;
		}
		if (elementStyle == SubElementStyle.ANIMATOR)
		{
			targetAnimator.gameObject.SetActive(true);
			string empty = string.Empty;
			switch (flipDirection)
			{
			case DungeonData.Direction.NORTH:
				empty = northAnimation;
				break;
			case DungeonData.Direction.EAST:
				empty = eastAnimation;
				break;
			case DungeonData.Direction.SOUTH:
				empty = southAnimation;
				break;
			case DungeonData.Direction.WEST:
				empty = westAnimation;
				break;
			}
			if (string.IsNullOrEmpty(empty))
			{
				targetAnimator.Play();
			}
			else
			{
				targetAnimator.Play(empty);
			}
			tk2dSpriteAnimator obj = targetAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		}
		else if (elementStyle == SubElementStyle.GOOP)
		{
			DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopToUse);
			Vector2 worldCenter = sourceTable.WorldCenter;
			if (flipDirection == DungeonData.Direction.EAST || flipDirection == DungeonData.Direction.WEST)
			{
				worldCenter += new Vector2(0f, -0.5f);
			}
			goopManagerForGoopType.TimedAddGoopArc(worldCenter, goopConeLength, goopConeArc, DungeonData.GetIntVector2FromDirection(flipDirection).ToVector2(), goopDuration, goopCurve);
		}
	}

	private void AnimationCompleted(tk2dSpriteAnimator source, tk2dSpriteAnimationClip clerp)
	{
		tk2dSpriteAnimator obj = targetAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		source.Sprite.IsPerpendicular = false;
		source.Sprite.HeightOffGround = -1f + additionalHeightModification;
		source.Sprite.UpdateZDepth();
	}
}
