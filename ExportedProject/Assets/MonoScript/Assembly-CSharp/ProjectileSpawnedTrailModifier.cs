using UnityEngine;

public class ProjectileSpawnedTrailModifier : MonoBehaviour
{
	public GameObject TrailPrefab;

	public string spawnAudioEvent;

	public Transform InFlightSpawnTransform;

	public Vector3 WorldSpaceSpawnOffset;

	public float SpawnPeriod;

	private float m_elapsed;

	private Projectile m_projectile;

	private SpeculativeRigidbody m_srb;

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		m_srb = GetComponent<SpeculativeRigidbody>();
	}

	private void Update()
	{
		m_elapsed += BraveTime.DeltaTime;
		if (m_elapsed > SpawnPeriod)
		{
			if ((bool)InFlightSpawnTransform)
			{
				m_elapsed -= SpawnPeriod;
				SpawnManager.SpawnVFX(TrailPrefab, InFlightSpawnTransform.position + WorldSpaceSpawnOffset, Quaternion.identity);
			}
			else
			{
				m_elapsed -= SpawnPeriod;
				SpawnManager.SpawnVFX(TrailPrefab, m_srb.UnitCenter.ToVector3ZisY() + WorldSpaceSpawnOffset, Quaternion.identity);
			}
			if (!string.IsNullOrEmpty(spawnAudioEvent))
			{
				AkSoundEngine.PostEvent(spawnAudioEvent, base.gameObject);
			}
		}
	}
}
