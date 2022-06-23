using UnityEngine;

public class AmmoRegenSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public float AmmoPerSecond = 0.1f;

	public bool PreventGainWhileFiring = true;

	private Gun m_gun;

	private float m_ammoCounter;

	private float m_gameTimeOnDisable;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if ((bool)m_gun.CurrentOwner && m_gun.OwnerHasSynergy(RequiredSynergy) && (!PreventGainWhileFiring || !m_gun.IsFiring))
		{
			m_ammoCounter += BraveTime.DeltaTime * AmmoPerSecond;
			if (m_ammoCounter > 1f)
			{
				int num = Mathf.FloorToInt(m_ammoCounter);
				m_ammoCounter -= num;
				m_gun.GainAmmo(num);
			}
		}
	}

	private void OnEnable()
	{
		if (m_gameTimeOnDisable > 0f)
		{
			m_ammoCounter += (Time.time - m_gameTimeOnDisable) * AmmoPerSecond;
			m_gameTimeOnDisable = 0f;
		}
	}

	private void OnDisable()
	{
		m_gameTimeOnDisable = Time.time;
	}
}
