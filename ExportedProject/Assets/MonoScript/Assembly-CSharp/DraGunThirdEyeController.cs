using System.Collections.Generic;
using UnityEngine;

public class DraGunThirdEyeController : MonoBehaviour
{
	public GameObject IntroDummy;

	public GameObject IntroAttachPoint;

	public GameObject RoarDummy;

	public GameObject RoarAttachPoint;

	public GameObject AttachPoint;

	public List<ParticleSystem> particleSystems;

	private List<Renderer> m_particleRenderers;

	public void Awake()
	{
		m_particleRenderers = new List<Renderer>(particleSystems.Count);
		for (int i = 0; i < particleSystems.Count; i++)
		{
			m_particleRenderers.Add(particleSystems[i].GetComponent<Renderer>());
		}
	}

	public void LateUpdate()
	{
		GameObject gameObject = AttachPoint;
		if (IntroDummy.activeSelf)
		{
			gameObject = IntroAttachPoint;
		}
		else if (RoarDummy.activeSelf)
		{
			gameObject = RoarAttachPoint;
		}
		for (int i = 0; i < particleSystems.Count; i++)
		{
			m_particleRenderers[i].enabled = true;
			if (gameObject.activeSelf)
			{
				particleSystems[i].enableEmission = true;
				base.transform.position = gameObject.transform.position;
			}
			else
			{
				particleSystems[i].enableEmission = false;
			}
			if (GameManager.IsBossIntro)
			{
				particleSystems[i].Simulate(GameManager.INVARIANT_DELTA_TIME, true, false);
			}
			else if (particleSystems[i].isPaused)
			{
				particleSystems[i].Play();
			}
		}
	}
}
