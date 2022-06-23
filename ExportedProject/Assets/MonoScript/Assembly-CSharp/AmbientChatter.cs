using System.Collections;
using UnityEngine;

public class AmbientChatter : MonoBehaviour
{
	public float MinTimeBetweenChatter = 10f;

	public float MaxTimeBetweenChatter = 20f;

	public float ChatterDuration = 5f;

	public string ChatterStringKey;

	public Transform SpeakPoint;

	public bool WanderInRadius;

	public float WanderRadius = 3f;

	private Transform m_transform;

	private Vector3 m_startPosition;

	private void Start()
	{
		m_transform = base.transform;
		m_startPosition = m_transform.position;
		if (WanderInRadius)
		{
			StartCoroutine(HandleWander());
		}
		StartCoroutine(HandleAmbientChatter());
	}

	private IEnumerator HandleWander()
	{
		Vector2 currentTargetPosition = m_startPosition.XY() + Random.insideUnitCircle * WanderRadius;
		while (true)
		{
			if (Vector2.Distance(currentTargetPosition, m_transform.position.XY()) < 0.25f)
			{
				yield return new WaitForSeconds(Random.Range(0f, 1f));
				currentTargetPosition = m_startPosition.XY() + Random.insideUnitCircle * WanderRadius;
			}
			m_transform.position = Vector3.MoveTowards(m_transform.position, currentTargetPosition.ToVector3ZUp(m_transform.position.z), 3f * BraveTime.DeltaTime);
			yield return null;
		}
	}

	private IEnumerator HandleAmbientChatter()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(MinTimeBetweenChatter, MaxTimeBetweenChatter));
			TextBoxManager.ShowTextBox(SpeakPoint.position, base.transform, ChatterDuration, StringTableManager.GetString(ChatterStringKey), string.Empty, false);
		}
	}
}
