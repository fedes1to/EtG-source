using UnityEngine;

public class SkyRocketChallengeModifier : ChallengeModifier
{
	public GameObject Rocket;

	public float TimeBetweenRockets = 3f;

	private float m_elapsedSinceRocket;

	private int m_spawnedRockets;

	private void Update()
	{
		m_elapsedSinceRocket += BraveTime.DeltaTime;
		if (m_elapsedSinceRocket > TimeBetweenRockets)
		{
			m_elapsedSinceRocket = 0f;
			FireRocket();
		}
		if (m_spawnedRockets > 0 && (BossKillCam.BossDeathCamRunning || GameManager.Instance.PreventPausing))
		{
			Cleanup();
		}
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void FireRocket()
	{
		if (!BossKillCam.BossDeathCamRunning && !GameManager.Instance.PreventPausing)
		{
			PlayerController randomActivePlayer = GameManager.Instance.GetRandomActivePlayer();
			SkyRocket component = SpawnManager.SpawnProjectile(Rocket, Vector3.zero, Quaternion.identity).GetComponent<SkyRocket>();
			component.Target = randomActivePlayer.specRigidbody;
			tk2dSprite componentInChildren = component.GetComponentInChildren<tk2dSprite>();
			component.transform.position = component.transform.position.WithY(component.transform.position.y - componentInChildren.transform.localPosition.y);
			m_spawnedRockets++;
		}
	}

	public void Cleanup()
	{
		m_spawnedRockets = 0;
		SkyRocket[] array = Object.FindObjectsOfType<SkyRocket>();
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i])
			{
				array[i].DieInAir();
			}
		}
	}
}
