using System;
using UnityEngine;

public class ArtfulDodgerBumperController : DungeonPlaceableBehaviour
{
	public enum DiagonalDirection
	{
		None,
		NorthEast,
		SouthEast,
		SouthWest,
		NorthWest
	}

	[Header("Bumper Data")]
	public tk2dBaseSprite mySprite;

	public bool StopsGameProjectileBounces;

	public bool DestroyBumperOnGameCollision;

	public DiagonalDirection diagonalDirection;

	public VFXPool BumperPopVFX;

	public string hitAnimation = string.Empty;

	[ShowInInspectorIf("DestroyBumperOnGameCollision", false)]
	public string breakAnimation = string.Empty;

	public string idleAnimation = string.Empty;

	private bool m_canDestroy;

	private void Start()
	{
		tk2dSpriteAnimator obj = mySprite.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		if (diagonalDirection != 0)
		{
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.ReflectProjectilesNormalGenerator = (Func<Vector2, Vector2, Vector2>)Delegate.Combine(speculativeRigidbody2.ReflectProjectilesNormalGenerator, new Func<Vector2, Vector2, Vector2>(ReflectNormalGenerator));
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)base.gameObject)
		{
			tk2dSpriteAnimator obj = mySprite.spriteAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
			if (diagonalDirection != 0)
			{
				SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
				speculativeRigidbody2.ReflectProjectilesNormalGenerator = (Func<Vector2, Vector2, Vector2>)Delegate.Remove(speculativeRigidbody2.ReflectProjectilesNormalGenerator, new Func<Vector2, Vector2, Vector2>(ReflectNormalGenerator));
			}
		}
		base.OnDestroy();
	}

	private void AnimationCompleted(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if (DestroyBumperOnGameCollision && clip.name == breakAnimation && m_canDestroy)
		{
			BumperPopVFX.SpawnAtPosition(base.gameObject.transform.position, 0f, null, null, null, 1f);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (clip.name == hitAnimation)
		{
			mySprite.spriteAnimator.Play(idleAnimation);
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (rigidbodyCollision.OtherRigidbody.projectile != null)
		{
			Projectile projectile = rigidbodyCollision.OtherRigidbody.projectile;
			m_canDestroy = projectile.name.StartsWith("ArtfulDodger");
			mySprite.spriteAnimator.Play((!m_canDestroy || !DestroyBumperOnGameCollision) ? hitAnimation : breakAnimation);
			if (StopsGameProjectileBounces)
			{
				projectile.DieInAir();
			}
		}
	}

	private Vector2 ReflectNormalGenerator(Vector2 contact, Vector2 normal)
	{
		switch (diagonalDirection)
		{
		case DiagonalDirection.NorthEast:
			if (normal.x > 0.5f || normal.y > 0.5f)
			{
				return new Vector2(1f, 1f).normalized;
			}
			break;
		case DiagonalDirection.SouthEast:
			if (normal.x > 0.5f || normal.y < -0.5f)
			{
				return new Vector2(1f, -1f).normalized;
			}
			break;
		case DiagonalDirection.SouthWest:
			if (normal.x < -0.5f || normal.y < -0.5f)
			{
				return new Vector2(-1f, -1f).normalized;
			}
			break;
		case DiagonalDirection.NorthWest:
			if (normal.x < -0.5f || normal.y > 0.5f)
			{
				return new Vector2(-1f, 1f).normalized;
			}
			break;
		}
		return normal;
	}
}
