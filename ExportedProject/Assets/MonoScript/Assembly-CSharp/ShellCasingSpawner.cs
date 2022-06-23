using UnityEngine;

public class ShellCasingSpawner : BraveBehaviour
{
	public GameObject shellCasing;

	public bool inheritRotationAsDirection;

	public int shellsToLaunch;

	public float minForce = 1f;

	public float maxForce = 2.5f;

	public float angleVariance = 10f;

	private bool m_shouldSpawn;

	public void Start()
	{
		m_shouldSpawn = true;
	}

	public void OnSpawned()
	{
		m_shouldSpawn = true;
	}

	public void Update()
	{
		if (m_shouldSpawn)
		{
			SpawnShells();
			m_shouldSpawn = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void SpawnShells()
	{
		if (GameManager.Options.DebrisQuantity != 0 && GameManager.Options.DebrisQuantity != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			for (int i = 0; i < shellsToLaunch; i++)
			{
				SpawnShellCasingAtPosition(base.transform.position);
			}
		}
	}

	private void SpawnShellCasingAtPosition(Vector3 position)
	{
		if (!(shellCasing != null))
		{
			return;
		}
		float num = ((!inheritRotationAsDirection) ? Random.Range(-180f, 180f) : base.transform.eulerAngles.z);
		GameObject gameObject = SpawnManager.SpawnDebris(shellCasing, position, Quaternion.Euler(0f, 0f, num));
		ShellCasing component = gameObject.GetComponent<ShellCasing>();
		if (component != null)
		{
			component.Trigger();
		}
		DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
		if (component2 != null)
		{
			Vector3 startingForce = BraveMathCollege.DegreesToVector(num + angleVariance * Random.Range(-1f, 1f), Mathf.Lerp(minForce, maxForce, Random.value)).ToVector3ZUp(Random.value * 1.5f + 1f);
			float y = base.transform.position.y;
			float num2 = 0.2f;
			float num3 = component2.transform.position.y - y + Random.value * 0.5f;
			component2.additionalHeightBoost = num2 - num3;
			if (num > 25f && num < 155f)
			{
				component2.additionalHeightBoost += -0.25f;
			}
			else
			{
				component2.additionalHeightBoost += 0.25f;
			}
			component2.Trigger(startingForce, num3);
		}
	}
}
