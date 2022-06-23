using Dungeonator;
using UnityEngine;

public class UnlockPlayableBulletManBehavior : BehaviorBase
{
	private float m_aloneElapsed;

	public override void Start()
	{
		base.Start();
		if ((bool)m_aiActor && !m_aiActor.sprite)
		{
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		if (m_aiActor.ParentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) == 1)
		{
			m_aloneElapsed += BraveTime.DeltaTime;
			if (m_aloneElapsed > 3f)
			{
				GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Teleport_Beam");
				GameObject gameObject = Object.Instantiate(original);
				gameObject.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(m_aiActor.specRigidbody.UnitBottomCenter + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
				Debug.Log("Setting a SEEN_SECRET_BULLETMAN flag!");
				GameStatsManager.Instance.SetNextFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_01, GungeonFlags.SECRET_BULLETMAN_SEEN_02, GungeonFlags.SECRET_BULLETMAN_SEEN_03, GungeonFlags.SECRET_BULLETMAN_SEEN_04, GungeonFlags.SECRET_BULLETMAN_SEEN_05);
				Object.Destroy(m_gameObject);
			}
		}
		return base.Update();
	}
}
