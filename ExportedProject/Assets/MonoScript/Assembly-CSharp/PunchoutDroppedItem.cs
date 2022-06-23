using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchoutDroppedItem : MonoBehaviour
{
	private static int[] s_randomIndices;

	private static int s_indicesIndex;

	public List<Vector2> targetOffsets;

	public float airTime;

	public float airHeight;

	public float airTime2;

	public float airHeight2;

	public float motionStartPercent = 0.75f;

	public float motionMultiplier = 4f;

	public float gravityMultiplier = 6f;

	private Vector2 m_offset;

	private Vector2 m_startPosition;

	private Vector2 m_targetPosition;

	public void Init(bool isLeft)
	{
		if (s_randomIndices == null || s_randomIndices.Length != targetOffsets.Count)
		{
			s_randomIndices = new int[targetOffsets.Count];
			for (int i = 0; i < s_randomIndices.Length; i++)
			{
				s_randomIndices[i] = i;
			}
			BraveUtility.RandomizeArray(s_randomIndices);
			s_indicesIndex = 0;
		}
		m_offset = base.transform.position.XY() - GetComponent<tk2dBaseSprite>().WorldBottomCenter;
		m_startPosition = base.transform.position;
		m_targetPosition = m_startPosition + targetOffsets[s_randomIndices[s_indicesIndex]].Scale((!isLeft) ? 1 : (-1), 1f);
		s_indicesIndex = (s_indicesIndex + 1) % s_randomIndices.Length;
		StartCoroutine(MoveCR());
		AkSoundEngine.PostEvent("Play_OBJ_coin_spill_01", base.gameObject);
	}

	private IEnumerator MoveCR()
	{
		tk2dSprite sprite = GetComponent<tk2dSprite>();
		sprite.HeightOffGround = 8f;
		sprite.UpdateZDepth();
		float timer = 0f;
		while (timer < airTime)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			float t = Mathf.Clamp01(timer / airTime);
			Vector2 pos = Vector2.Lerp(m_startPosition, m_targetPosition, t);
			pos.y += Mathf.Sin(t * (float)Math.PI) * airHeight;
			base.transform.position = pos + m_offset;
			sprite.UpdateZDepth();
		}
		Vector2 deltaPos = m_targetPosition - m_startPosition;
		m_startPosition = m_targetPosition;
		m_targetPosition = m_startPosition + deltaPos.normalized * 2f;
		timer = 0f;
		while (timer < airTime2)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			float t2 = Mathf.Clamp01(timer / airTime2);
			Vector2 pos2 = Vector2.Lerp(m_startPosition, m_targetPosition, t2);
			pos2.y += Mathf.Sin(t2 * (float)Math.PI) * airHeight2;
			base.transform.position = pos2 + m_offset;
			sprite.UpdateZDepth();
		}
		sprite.renderer.sortingLayerName = "Background";
		sprite.SortingOrder = 1;
		sprite.HeightOffGround = -2f;
		sprite.UpdateZDepth();
		Vector2 midPos = Vector2.Lerp(m_startPosition, m_targetPosition, motionStartPercent);
		midPos.y += Mathf.Sin(motionStartPercent * (float)Math.PI) * airHeight2;
		Vector2 endPos = m_targetPosition;
		Vector2 velocity = (endPos - midPos).normalized * motionMultiplier;
		timer = 0f;
		while (timer < 1.25f)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			Vector2 pos3 = endPos + timer * velocity;
			pos3.y -= Mathf.Sin(Mathf.Clamp(timer * 0.5f, 0f, 0.5f) * (float)Math.PI) * gravityMultiplier;
			base.transform.position = pos3 + m_offset;
			sprite.UpdateZDepth();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
