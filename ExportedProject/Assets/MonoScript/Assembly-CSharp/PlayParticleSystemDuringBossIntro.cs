using UnityEngine;

public class PlayParticleSystemDuringBossIntro : MonoBehaviour
{
	private bool m_isSimulating;

	private ParticleSystem m_particleSystem;

	public void Start()
	{
		m_particleSystem = GetComponent<ParticleSystem>();
	}

	public void Update()
	{
		if (GameManager.IsBossIntro)
		{
			m_particleSystem.Simulate(GameManager.INVARIANT_DELTA_TIME, true, false);
			m_isSimulating = true;
		}
		else if (m_isSimulating)
		{
			m_particleSystem.Play();
			m_isSimulating = false;
		}
	}
}
