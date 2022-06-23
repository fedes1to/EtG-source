using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class KingEnemyChallengeModifier : ChallengeModifier
{
	public GameObject KingVFX;

	private bool m_isActive;

	private HealthHaver m_king;

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
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		int num = Random.Range(0, activeEnemies.Count);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (i == num)
			{
				if (IsValidEnemy(activeEnemies[i]))
				{
					Vector2 vector = ((!activeEnemies[i].sprite) ? Vector2.up : (Vector2.up * (activeEnemies[i].sprite.WorldTopCenter.y - activeEnemies[i].sprite.WorldBottomCenter.y)));
					GameObject gameObject = activeEnemies[i].PlayEffectOnActor(KingVFX, vector);
					if ((bool)activeEnemies[i].OverrideBuffEffectPosition)
					{
						Vector3 position = activeEnemies[i].OverrideBuffEffectPosition.position;
						position.x -= gameObject.GetComponent<tk2dSprite>().GetBounds().extents.x;
						gameObject.transform.position = position;
					}
					else if ((bool)activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss)
					{
						Vector3 position2 = activeEnemies[i].specRigidbody.HitboxPixelCollider.UnitTopCenter;
						position2.x -= gameObject.GetComponent<tk2dSprite>().GetBounds().extents.x;
						position2.y += gameObject.GetComponent<tk2dSprite>().GetBounds().extents.y;
						gameObject.transform.position = position2;
					}
					else
					{
						Bounds bounds = activeEnemies[i].sprite.GetBounds();
						Vector3 position3 = activeEnemies[i].transform.position + new Vector3((bounds.max.x + bounds.min.x) / 2f, bounds.max.y, 0f).Quantize(0.0625f);
						position3.y = activeEnemies[i].transform.position.y + activeEnemies[i].sprite.GetUntrimmedBounds().max.y;
						position3.x -= gameObject.GetComponent<tk2dSprite>().GetBounds().extents.x;
						gameObject.transform.position = position3;
					}
					activeEnemies[i].healthHaver.OnDeath += OnKingDeath;
					m_king = activeEnemies[i].healthHaver;
				}
				else
				{
					num++;
				}
			}
			else if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver && !activeEnemies[i].IsMimicEnemy)
			{
				activeEnemies[i].healthHaver.PreventAllDamage = true;
			}
		}
		m_isActive = true;
	}

	private void Update()
	{
		if (m_isActive && (!m_king || m_king.IsDead))
		{
			m_isActive = false;
			OnKingDeath(Vector2.zero);
		}
	}

	private void OnKingDeath(Vector2 obj)
	{
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver)
			{
				activeEnemies[i].healthHaver.PreventAllDamage = false;
			}
		}
	}
}
