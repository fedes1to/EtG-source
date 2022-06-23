using Dungeonator;
using UnityEngine;

public class PitParticleKiller : MonoBehaviour
{
	private ParticleSystem.Particle[] m_particleArray;

	private ParticleSystem m_system;

	private Dungeon m_dungeon;

	private Transform m_transform;

	private void Start()
	{
		m_transform = base.transform;
		m_dungeon = GameManager.Instance.Dungeon;
		m_system = GetComponent<ParticleSystem>();
		m_particleArray = new ParticleSystem.Particle[m_system.maxParticles];
	}

	private bool LocalCellSupportsFalling(Vector3 worldPos)
	{
		IntVector2 intVector = worldPos.IntXY(VectorConversions.Floor);
		if (!m_dungeon.data.CheckInBounds(intVector))
		{
			return false;
		}
		CellData cellData = m_dungeon.data[intVector];
		return cellData != null && cellData.type == CellType.PIT && !cellData.fallingPrevented;
	}

	private void LateUpdate()
	{
		int particles = m_system.GetParticles(m_particleArray);
		for (int i = 0; i < particles; i++)
		{
			Vector3 worldPos = m_transform.TransformPoint(m_particleArray[i].position);
			if (LocalCellSupportsFalling(worldPos))
			{
				m_particleArray[i].remainingLifetime = 0f;
			}
		}
		m_system.SetParticles(m_particleArray, particles);
	}
}
