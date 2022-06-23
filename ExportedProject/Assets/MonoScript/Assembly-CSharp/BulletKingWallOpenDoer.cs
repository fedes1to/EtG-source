using System;
using Dungeonator;
using UnityEngine;

public class BulletKingWallOpenDoer : BraveBehaviour
{
	private void Start()
	{
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		absoluteRoomFromPosition.OnEnemiesCleared = (Action)Delegate.Combine(absoluteRoomFromPosition.OnEnemiesCleared, new Action(OnBossKill));
	}

	private void OnBossKill()
	{
		base.specRigidbody.PixelColliders[4].Enabled = false;
		base.specRigidbody.PixelColliders[5].Enabled = false;
		base.spriteAnimator.Play();
		Vector2 unitBottomLeft = base.specRigidbody.PixelColliders[4].UnitBottomLeft;
		Vector2 unitTopRight = base.specRigidbody.PixelColliders[5].UnitTopRight;
		SpawnManager.Instance.ClearRectOfDecals(unitBottomLeft, unitTopRight);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
