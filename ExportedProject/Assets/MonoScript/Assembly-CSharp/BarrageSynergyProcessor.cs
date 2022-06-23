using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BarrageSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public BarrageModule Barrage;

	public bool BarrageIsAmbient;

	public float MinBarrageCooldown = 5f;

	public float MaxBarrageCooldown = 5f;

	private Gun m_gun;

	private float m_elapsed;

	private float m_currentCooldown;

	private void Start()
	{
		m_gun = GetComponent<Gun>();
		m_currentCooldown = Random.Range(MinBarrageCooldown, MaxBarrageCooldown);
	}

	private void Update()
	{
		if (Dungeon.IsGenerating || GameManager.IsBossIntro || !BarrageIsAmbient || !m_gun || !(m_gun.CurrentOwner is PlayerController))
		{
			return;
		}
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (playerController.HasActiveBonusSynergy(RequiredSynergy) && playerController.IsInCombat)
		{
			m_elapsed += BraveTime.DeltaTime;
			if (m_elapsed >= m_currentCooldown)
			{
				m_elapsed -= m_currentCooldown;
				m_currentCooldown = Random.Range(MinBarrageCooldown, MaxBarrageCooldown);
				DoAmbientTargetedBarrage(playerController);
			}
		}
	}

	private void DoAmbientTargetedBarrage(PlayerController p)
	{
		List<AIActor> activeEnemies = p.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies != null)
		{
			int index = Random.Range(0, activeEnemies.Count);
			Vector2 normalized = Random.insideUnitCircle.normalized;
			Vector2 startPoint = activeEnemies[index].CenterPosition + -normalized * (Barrage.BarrageLength / 2f);
			if ((bool)activeEnemies[index])
			{
				Barrage.DoBarrage(startPoint, normalized, GameManager.Instance.Dungeon);
			}
		}
	}
}
