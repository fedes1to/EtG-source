using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class StunEnemiesInRoomItem : MonoBehaviour
{
	public float StunDuration = 5f;

	public bool DoChaffParticles = true;

	public bool AllowStealing = true;

	protected void AffectEnemy(AIActor target)
	{
		if ((bool)target && (bool)target.behaviorSpeculator)
		{
			target.behaviorSpeculator.Stun(StunDuration);
		}
	}

	private void Start()
	{
		AkSoundEngine.PostEvent("Play_OBJ_item_throw_01", base.gameObject);
		DebrisObject component = GetComponent<DebrisObject>();
		component.killTranslationOnBounce = false;
		if ((bool)component)
		{
			component.OnGrounded = (Action<DebrisObject>)Delegate.Combine(component.OnGrounded, new Action<DebrisObject>(OnHitGround));
		}
	}

	private void OnHitGround(DebrisObject obj)
	{
		Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.1f);
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		List<AIActor> activeEnemies = absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies != null)
		{
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AffectEnemy(activeEnemies[i]);
			}
		}
		if (DoChaffParticles)
		{
			GlobalSparksDoer.DoRandomParticleBurst(100, absoluteRoom.area.basePosition.ToVector3(), absoluteRoom.area.basePosition.ToVector3() + absoluteRoom.area.dimensions.ToVector3(), Vector3.up / 3f, 180f, 0f, 0.125f, StunDuration, Color.yellow, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
			AkSoundEngine.PostEvent("Play_OBJ_chaff_blast_01", base.gameObject);
		}
		if (AllowStealing)
		{
			List<BaseShopController> allShops = StaticReferenceManager.AllShops;
			for (int j = 0; j < allShops.Count; j++)
			{
				if ((bool)allShops[j] && allShops[j].GetAbsoluteParentRoom() == absoluteRoom)
				{
					allShops[j].SetCapableOfBeingStolenFrom(true, "StunEnemiesInRoomItem", StunDuration);
				}
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
