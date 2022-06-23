using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeGameActorEffectData
{
	public GameActor actor;

	public float elapsed;

	public float tickCounter;

	public GameActor.MovementModifier MovementModifier;

	public Action<Vector2> OnActorPreDeath;

	public Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip> OnFlameAnimationCompleted;

	public float onActorVFXTimer;

	public tk2dBaseSprite instanceOverheadVFX;

	public float accumulator;

	public bool destroyVfx;

	public List<Tuple<GameObject, float>> vfxObjects;
}
