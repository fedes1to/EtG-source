using System.Collections;
using UnityEngine;

public class ShrinkEnemiesInRoomItem : AffectEnemiesInRoomItem
{
	public Vector2 TargetScale;

	public float ShrinkTime = 0.1f;

	public float HoldTime = 3f;

	public float RegrowTime = 3f;

	public float DamageMultiplier = 2f;

	public bool DepixelatesTargets;

	protected override void AffectEnemy(AIActor target)
	{
		target.StartCoroutine(HandleShrink(target));
	}

	private IEnumerator HandleShrink(AIActor target)
	{
		AkSoundEngine.PostEvent("Play_OBJ_lightning_flash_01", base.gameObject);
		float elapsed3 = 0f;
		Vector2 startScale = target.EnemyScale;
		int cachedLayer = target.gameObject.layer;
		int cachedOutlineLayer = cachedLayer;
		if (DepixelatesTargets)
		{
			target.gameObject.layer = LayerMask.NameToLayer("Unpixelated");
			cachedOutlineLayer = SpriteOutlineManager.ChangeOutlineLayer(target.sprite, LayerMask.NameToLayer("Unpixelated"));
		}
		target.ClearPath();
		DazedBehavior db = new DazedBehavior
		{
			PointReachedPauseTime = 0.5f,
			PathInterval = 0.5f
		};
		if ((bool)target.knockbackDoer)
		{
			target.knockbackDoer.weight /= 3f;
		}
		if ((bool)target.healthHaver)
		{
			target.healthHaver.AllDamageMultiplier *= DamageMultiplier;
		}
		target.behaviorSpeculator.OverrideBehaviors.Insert(0, db);
		target.behaviorSpeculator.RefreshBehaviors();
		m_isCurrentlyActive = true;
		while (elapsed3 < ShrinkTime)
		{
			elapsed3 += target.LocalDeltaTime;
			target.EnemyScale = Vector2.Lerp(startScale, TargetScale, elapsed3 / ShrinkTime);
			yield return null;
		}
		elapsed3 = 0f;
		while (elapsed3 < HoldTime)
		{
			m_activeElapsed = elapsed3;
			m_activeDuration = HoldTime;
			elapsed3 += target.LocalDeltaTime;
			yield return null;
		}
		elapsed3 = 0f;
		while (elapsed3 < RegrowTime)
		{
			elapsed3 += target.LocalDeltaTime;
			target.EnemyScale = Vector2.Lerp(TargetScale, startScale, elapsed3 / RegrowTime);
			yield return null;
		}
		if ((bool)target.knockbackDoer)
		{
			target.knockbackDoer.weight *= 3f;
		}
		if ((bool)target.healthHaver)
		{
			target.healthHaver.AllDamageMultiplier /= DamageMultiplier;
		}
		target.behaviorSpeculator.OverrideBehaviors.Remove(db);
		target.behaviorSpeculator.RefreshBehaviors();
		m_isCurrentlyActive = false;
		if (DepixelatesTargets)
		{
			target.gameObject.layer = cachedLayer;
			SpriteOutlineManager.ChangeOutlineLayer(target.sprite, cachedOutlineLayer);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
