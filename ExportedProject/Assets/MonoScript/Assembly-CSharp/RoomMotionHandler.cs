using System.Collections;
using Dungeonator;
using UnityEngine;

public class RoomMotionHandler : MonoBehaviour
{
	private float c_roomSpeed = 3f;

	private Transform m_transform;

	private float m_zOffset;

	private IntVector2 currentCellPosition;

	private bool m_isMoving;

	public void Initialize(RoomHandler parentRoom)
	{
		m_transform = base.transform;
		m_zOffset = m_transform.position.z - m_transform.position.y;
		currentCellPosition = parentRoom.area.basePosition;
	}

	public void TriggerMoveTo(IntVector2 targetPosition)
	{
		if (!m_isMoving && !(targetPosition == currentCellPosition))
		{
			StartCoroutine(HandleMove(targetPosition));
		}
	}

	private IEnumerator HandleMove(IntVector2 targetPosition)
	{
		m_isMoving = true;
		IntVector2 startPosition = currentCellPosition;
		IntVector2 movementVector = targetPosition - startPosition;
		Vector3 worldStartPosition = m_transform.position;
		Vector3 worldEndPosition2 = m_transform.position + movementVector.ToVector3();
		worldEndPosition2 = worldEndPosition2.WithZ(worldEndPosition2.y + m_zOffset);
		float distanceToTravel = IntVector2.ManhattanDistance(startPosition, targetPosition);
		float timeToTravel = distanceToTravel / c_roomSpeed;
		float elapsed = 0f;
		while (elapsed < timeToTravel)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / timeToTravel));
			m_transform.position = Vector3.Lerp(worldStartPosition, worldEndPosition2, t);
			currentCellPosition = m_transform.position.IntXY(VectorConversions.Floor);
			yield return null;
		}
		m_transform.position = worldEndPosition2;
		currentCellPosition = targetPosition;
		m_isMoving = false;
	}
}
