using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class NightclubLightController : MonoBehaviour
{
	private static List<Vector2> m_registeredPositions;

	public float LightChangePeriod = 1f;

	public float MotionMaxSpeed = 5f;

	public Rect ValidMotionRect;

	private float m_elapsed;

	private Transform m_transform;

	private ShadowSystem m_light;

	private SceneLightManager m_colors;

	private RoomHandler m_parentRoom;

	private Material m_lightMaterial;

	private int m_colorID;

	private int m_positionIndex;

	private bool m_inMotion;

	private IEnumerator Start()
	{
		if (m_registeredPositions == null)
		{
			m_registeredPositions = new List<Vector2>();
		}
		m_registeredPositions.Clear();
		yield return null;
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		m_transform = base.transform.parent;
		m_light = GetComponent<ShadowSystem>();
		m_colors = GetComponent<SceneLightManager>();
		m_registeredPositions.Add(m_transform.position.XY());
		m_positionIndex = m_registeredPositions.Count - 1;
		m_lightMaterial = m_light.renderer.sharedMaterial;
		m_colorID = Shader.PropertyToID("_TintColor");
	}

	private void Update()
	{
		if (!(m_light == null))
		{
			m_elapsed += ((!GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
			if (m_elapsed > LightChangePeriod)
			{
				m_elapsed -= LightChangePeriod;
				m_lightMaterial.SetColor(m_colorID, m_colors.validColors[Random.Range(0, m_colors.validColors.Length)]);
			}
			if (!m_inMotion)
			{
				StartCoroutine(HandleMotion());
			}
		}
	}

	private bool CheckPositionValid(Vector2 targetPosition)
	{
		bool result = true;
		for (int i = 0; i < m_registeredPositions.Count; i++)
		{
			if (Vector2.Distance(m_registeredPositions[i], targetPosition) < 3f)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private IEnumerator HandleMotion()
	{
		m_inMotion = true;
		Vector2 currentPosition = m_transform.position.XY();
		Vector2 targetPosition = new Vector2(Random.Range(ValidMotionRect.xMin, ValidMotionRect.xMax), Random.Range(ValidMotionRect.yMin, ValidMotionRect.yMax));
		targetPosition += m_parentRoom.area.basePosition.ToVector2();
		for (; !CheckPositionValid(targetPosition); targetPosition += m_parentRoom.area.basePosition.ToVector2())
		{
			targetPosition.Set(Random.Range(ValidMotionRect.xMin, ValidMotionRect.xMax), Random.Range(ValidMotionRect.yMin, ValidMotionRect.yMax));
		}
		m_registeredPositions[m_positionIndex] = targetPosition;
		float elapsed = 0f;
		float duration = Vector2.Distance(targetPosition, currentPosition) / MotionMaxSpeed;
		while (elapsed < duration)
		{
			m_elapsed += ((!GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
			Vector2 setPosition = Vector2.Lerp(t: Mathf.SmoothStep(0f, 1f, elapsed / duration), a: currentPosition, b: targetPosition);
			m_transform.position = setPosition.ToVector3ZUp(setPosition.y);
			yield return null;
		}
		yield return new WaitForSeconds(Random.Range(0.5f, 3f));
		m_inMotion = false;
	}
}
