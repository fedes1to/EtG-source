using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BestForLastChallengeModifier : ChallengeModifier
{
	public GameObject KingVFX;

	private bool m_isActive;

	private HealthHaver m_king;

	private RoomHandler m_room;

	private List<AIActor> m_activeEnemies = new List<AIActor>();

	private GameObject m_instanceVFX;

	private bool IsValidEnemy(AIActor testEnemy)
	{
		if (!testEnemy || testEnemy.IsHarmlessEnemy)
		{
			return false;
		}
		if ((bool)testEnemy.healthHaver && testEnemy.healthHaver.PreventAllDamage)
		{
			return false;
		}
		if ((bool)testEnemy.GetComponent<ExplodeOnDeath>() && !testEnemy.IsSignatureEnemy)
		{
			return false;
		}
		return true;
	}

	private void Start()
	{
		(m_room = GameManager.Instance.PrimaryPlayer.CurrentRoom).GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear, ref m_activeEnemies);
		int num = Random.Range(0, m_activeEnemies.Count);
		for (int i = 0; i < m_activeEnemies.Count; i++)
		{
			if (i == num)
			{
				if (IsValidEnemy(m_activeEnemies[i]))
				{
					Vector2 vector = ((!m_activeEnemies[i].sprite) ? Vector2.up : (Vector2.up * (m_activeEnemies[i].sprite.WorldTopCenter.y - m_activeEnemies[i].sprite.WorldBottomCenter.y)));
					GameObject gameObject = m_activeEnemies[i].PlayEffectOnActor(KingVFX, vector);
					Bounds bounds = m_activeEnemies[i].sprite.GetBounds();
					Vector3 position = m_activeEnemies[i].transform.position + new Vector3((bounds.max.x + bounds.min.x) / 2f, bounds.max.y, 0f).Quantize(0.0625f);
					position.y = m_activeEnemies[i].transform.position.y + m_activeEnemies[i].sprite.GetUntrimmedBounds().max.y;
					position.x -= gameObject.GetComponent<tk2dSprite>().GetBounds().extents.x;
					position.y -= gameObject.GetComponent<tk2dSprite>().GetBounds().extents.y;
					gameObject.transform.position = position;
					m_instanceVFX = gameObject;
					ChallengeManager.Instance.StartCoroutine(DelayedSpawnIcon());
					m_activeEnemies[i].healthHaver.PreventAllDamage = true;
					m_king = m_activeEnemies[i].healthHaver;
				}
				else
				{
					num++;
				}
			}
		}
		m_isActive = true;
	}

	private IEnumerator DelayedSpawnIcon()
	{
		Bounds vfxBounds = m_instanceVFX.GetComponent<tk2dSprite>().GetBounds();
		float elapsed = 0f;
		float duration = 0.25f;
		if ((bool)m_king && (bool)m_king.specRigidbody)
		{
			while ((bool)m_king && m_king.specRigidbody.HitboxPixelCollider.UnitDimensions.x <= 0f)
			{
				yield return null;
			}
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			if ((bool)m_instanceVFX && (bool)m_king)
			{
				m_instanceVFX.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
				if ((bool)m_king.specRigidbody && m_king.specRigidbody.HitboxPixelCollider != null && m_king.specRigidbody.HitboxPixelCollider.ColliderGenerationMode != PixelCollider.PixelColliderGeneration.BagelCollider)
				{
					Vector3 position = m_king.specRigidbody.HitboxPixelCollider.UnitTopCenter;
					position.x -= vfxBounds.extents.x;
					position.y += vfxBounds.extents.y;
					m_instanceVFX.transform.position = position;
				}
				else
				{
					Bounds bounds = m_king.sprite.GetBounds();
					Vector3 position2 = m_king.transform.position + new Vector3((bounds.max.x + bounds.min.x) / 2f, bounds.max.y, 0f).Quantize(0.0625f);
					position2.y = m_king.transform.position.y + m_king.sprite.GetBounds().max.y;
					position2.x -= vfxBounds.extents.x;
					position2.y -= vfxBounds.extents.y;
					m_instanceVFX.transform.position = position2;
				}
			}
			yield return null;
		}
	}

	private void Update()
	{
		if (!m_isActive)
		{
			return;
		}
		m_room.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear, ref m_activeEnemies);
		if ((bool)m_king && m_activeEnemies.Count == 1)
		{
			m_king.PreventAllDamage = false;
			if ((bool)m_instanceVFX)
			{
				m_instanceVFX.GetComponent<tk2dSprite>().SetSprite("lastmanstanding_check_001");
			}
		}
	}
}
