using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ProximityMine : BraveBehaviour
{
	public enum ExplosiveStyle
	{
		PROXIMITY,
		TIMED
	}

	public ExplosionData explosionData;

	public ExplosiveStyle explosionStyle;

	[ShowInInspectorIf("explosionStyle", 0, false)]
	public float detectRadius = 2.5f;

	public float explosionDelay = 0.3f;

	public bool usesCustomExplosionDelay;

	[ShowInInspectorIf("usesCustomExplosionDelay", false)]
	public float customExplosionDelay = 0.1f;

	[CheckAnimation(null)]
	public string deployAnimName;

	[CheckAnimation(null)]
	public string idleAnimName;

	[CheckAnimation(null)]
	public string explodeAnimName;

	[Header("Homing")]
	public bool MovesTowardEnemies;

	public bool HomingTriggeredOnSynergy;

	[LongNumericEnum]
	public CustomSynergyType TriggerSynergy;

	public float HomingRadius = 5f;

	public float HomingSpeed = 3f;

	public float HomingDelay = 5f;

	protected bool m_triggered;

	protected bool m_disarmed;

	private void TransitionToIdle(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if (idleAnimName != null && !animator.IsPlaying(explodeAnimName))
		{
			animator.Play(idleAnimName);
		}
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToIdle));
	}

	private void Update()
	{
		if (!MovesTowardEnemies && HomingTriggeredOnSynergy && GameManager.Instance.PrimaryPlayer.HasActiveBonusSynergy(TriggerSynergy))
		{
			MovesTowardEnemies = true;
		}
		if (!MovesTowardEnemies)
		{
			return;
		}
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		float nearestDistance = float.MaxValue;
		AIActor nearestEnemy = absoluteRoom.GetNearestEnemy(base.sprite.WorldCenter, out nearestDistance);
		if ((bool)nearestEnemy && nearestDistance < HomingRadius)
		{
			Vector2 centerPosition = nearestEnemy.CenterPosition;
			Vector2 normalized = (centerPosition - base.sprite.WorldCenter).normalized;
			if ((bool)base.debris)
			{
				base.debris.ApplyFrameVelocity(normalized * HomingSpeed);
			}
			else
			{
				base.transform.position = base.transform.position + normalized.ToVector3ZisY() * HomingSpeed * BraveTime.DeltaTime;
			}
		}
	}

	private IEnumerator Start()
	{
		if (!string.IsNullOrEmpty(deployAnimName))
		{
			base.spriteAnimator.Play(deployAnimName);
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TransitionToIdle));
		}
		else if (!string.IsNullOrEmpty(idleAnimName))
		{
			base.spriteAnimator.Play(idleAnimName);
		}
		if (explosionStyle == ExplosiveStyle.PROXIMITY)
		{
			Vector2 position = base.transform.position.XY();
			List<AIActor> allActors = StaticReferenceManager.AllEnemies;
			AkSoundEngine.PostEvent("Play_OBJ_mine_set_01", base.gameObject);
			while (!m_triggered)
			{
				if (MovesTowardEnemies)
				{
					position = base.transform.position.XY();
				}
				if (!GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor)).HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
				{
					m_triggered = true;
					m_disarmed = true;
					break;
				}
				bool shouldContinue = false;
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					if ((bool)GameManager.Instance.AllPlayers[i] && !GameManager.Instance.AllPlayers[i].IsGhost)
					{
						float num = Vector2.SqrMagnitude(position - GameManager.Instance.AllPlayers[i].specRigidbody.UnitCenter);
						if (num < detectRadius * detectRadius)
						{
							shouldContinue = true;
							break;
						}
					}
				}
				if (shouldContinue)
				{
					yield return null;
					continue;
				}
				for (int j = 0; j < allActors.Count; j++)
				{
					AIActor aIActor = allActors[j];
					if (aIActor.IsNormalEnemy && aIActor.gameObject.activeSelf && aIActor.HasBeenEngaged && !aIActor.healthHaver.IsDead)
					{
						float num2 = Vector2.SqrMagnitude(position - aIActor.specRigidbody.UnitCenter);
						if (num2 < detectRadius * detectRadius)
						{
							m_triggered = true;
							break;
						}
					}
				}
				yield return null;
			}
		}
		else if (explosionStyle == ExplosiveStyle.TIMED)
		{
			yield return new WaitForSeconds(explosionDelay);
			if (MovesTowardEnemies && HomingDelay > explosionDelay)
			{
				yield return new WaitForSeconds(HomingDelay - explosionDelay);
			}
		}
		if (!m_disarmed)
		{
			if (!string.IsNullOrEmpty(explodeAnimName))
			{
				base.spriteAnimator.Play(explodeAnimName);
				if (usesCustomExplosionDelay)
				{
					yield return new WaitForSeconds(customExplosionDelay);
				}
				else
				{
					tk2dSpriteAnimationClip clip = base.spriteAnimator.GetClipByName(explodeAnimName);
					yield return new WaitForSeconds((float)clip.frames.Length / clip.fps);
				}
			}
			Exploder.Explode(base.sprite.WorldCenter.ToVector3ZUp(), explosionData, Vector2.zero);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			base.spriteAnimator.StopAndResetFrame();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
