using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class DemonWallDeathController : BraveBehaviour
{
	public GameObject deathEyes;

	public GameObject deathOil;

	private bool m_isDying;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		IntVector2 intVector = (base.specRigidbody.HitboxPixelCollider.UnitBottomLeft + new Vector2(0f, -1f)).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = base.specRigidbody.HitboxPixelCollider.UnitTopRight.ToIntVector2(VectorConversions.Ceil);
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			if (i == (intVector2.x + intVector.x) / 2 || i == (intVector2.x + intVector.x) / 2 - 1)
			{
				continue;
			}
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				if (data.CheckInBoundsAndValid(new IntVector2(i, j)))
				{
					CellData cellData = data[i, j];
					if (cellData.type == CellType.FLOOR)
					{
						cellData.isOccupied = true;
					}
				}
			}
		}
		base.aiActor.ParentRoom.OverrideBossPedestalLocation = base.specRigidbody.UnitCenter.ToIntVector2() + new IntVector2(-1, 7);
		StartCoroutine(OnDeathAnimationCR());
	}

	private IEnumerator OnDeathAnimationCR()
	{
		m_isDying = true;
		base.aiAnimator.EndAnimation();
		deathEyes.SetActive(true);
		tk2dSpriteAnimator deathEyesAnimator = deathEyes.GetComponent<tk2dSpriteAnimator>();
		deathEyesAnimator.Play();
		while (deathEyesAnimator.IsPlaying(deathEyesAnimator.DefaultClip))
		{
			yield return null;
		}
		deathEyes.SetActive(false);
		base.aiAnimator.PlayUntilCancelled("death");
		while (base.aiAnimator.IsPlaying("death"))
		{
			yield return null;
		}
		tk2dSpriteAnimator deathOilAnimator = deathOil.GetComponent<tk2dSpriteAnimator>();
		while (deathOilAnimator.IsPlaying(deathOilAnimator.DefaultClip))
		{
			yield return null;
		}
		deathOil.SetActive(false);
		base.sprite.HeightOffGround = -1.5f;
		base.sprite.UpdateZDepth();
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
		{
			base.specRigidbody.PixelColliders[i].Enabled = !base.specRigidbody.PixelColliders[i].Enabled;
		}
		base.specRigidbody.Reinitialize();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		base.specRigidbody.CanBePushed = false;
		if ((bool)base.aiActor)
		{
			UnityEngine.Object.Destroy(base.aiActor);
		}
		if ((bool)base.healthHaver)
		{
			UnityEngine.Object.Destroy(base.healthHaver);
		}
		if ((bool)base.behaviorSpeculator)
		{
			UnityEngine.Object.Destroy(base.behaviorSpeculator);
		}
		RegenerateCache();
		m_isDying = false;
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_isDying && clip.GetFrame(frame).eventInfo == "oil")
		{
			deathOil.SetActive(true);
			tk2dSpriteAnimator component = deathOil.GetComponent<tk2dSpriteAnimator>();
			component.Play();
		}
	}
}
