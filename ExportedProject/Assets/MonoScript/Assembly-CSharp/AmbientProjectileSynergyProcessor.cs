using Dungeonator;
using UnityEngine;

public class AmbientProjectileSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public float TimeBetweenAmbientProjectiles = 5f;

	public bool ActiveEvenWithoutEnemies;

	public bool UsesRadius;

	public float Radius = 5f;

	public RadialBurstInterface Ambience;

	private Gun m_gun;

	private float m_elapsed;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (!m_gun || !(m_gun.CurrentOwner is PlayerController))
		{
			return;
		}
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (!playerController.HasActiveBonusSynergy(SynergyToCheck) || (!ActiveEvenWithoutEnemies && (playerController.CurrentRoom == null || !playerController.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))))
		{
			return;
		}
		m_elapsed += BraveTime.DeltaTime;
		if (UsesRadius)
		{
			float nearestDistance = float.MaxValue;
			playerController.CurrentRoom.GetNearestEnemy(playerController.CenterPosition, out nearestDistance);
			if (nearestDistance > Radius)
			{
				return;
			}
		}
		if (m_elapsed > TimeBetweenAmbientProjectiles)
		{
			m_elapsed = 0f;
			Ambience.DoBurst(playerController);
		}
	}
}
