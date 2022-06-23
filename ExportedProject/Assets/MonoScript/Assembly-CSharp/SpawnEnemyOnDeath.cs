using System;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SpawnEnemyOnDeath : OnDeathBehavior
{
	public enum EnemySelection
	{
		All = 10,
		Random = 20
	}

	public enum SpawnPosition
	{
		InsideCollider = 0,
		ScreenEdge = 1,
		InsideRadius = 20
	}

	public float chanceToSpawn = 1f;

	public string spawnVfx;

	[Header("Enemies to Spawn")]
	public EnemySelection enemySelection = EnemySelection.All;

	[EnemyIdentifier]
	public string[] enemyGuidsToSpawn;

	[ShowInInspectorIf("ShowRandomPrams", true)]
	public int minSpawnCount = 1;

	[ShowInInspectorIf("ShowRandomPrams", true)]
	public int maxSpawnCount = 1;

	[FormerlySerializedAs("spawnType")]
	[Header("Placement")]
	public SpawnPosition spawnPosition;

	[ShowInInspectorIf("ShowInsideColliderParams", true)]
	public int extraPixelWidth;

	[ShowInInspectorIf("ShowInsideRadiusParams", true)]
	public float spawnRadius = 1f;

	[Header("Spawn Parameters")]
	public float guaranteedSpawnGenerations;

	public string spawnAnim = "awaken";

	public bool spawnsCanDropLoot = true;

	public bool DoNormalReinforcement;

	private bool m_hasTriggered;

	private bool ShowRandomPrams()
	{
		return enemySelection == EnemySelection.Random;
	}

	private bool ShowInsideColliderParams()
	{
		return spawnPosition == SpawnPosition.InsideCollider;
	}

	private bool ShowInsideRadiusParams()
	{
		return spawnPosition == SpawnPosition.InsideRadius;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnTrigger(Vector2 damageDirection)
	{
		if (m_hasTriggered)
		{
			return;
		}
		m_hasTriggered = true;
		if (guaranteedSpawnGenerations <= 0f && chanceToSpawn < 1f && UnityEngine.Random.value > chanceToSpawn)
		{
			return;
		}
		if (!string.IsNullOrEmpty(spawnVfx))
		{
			base.aiAnimator.PlayVfx(spawnVfx);
		}
		string[] array = null;
		if (enemySelection == EnemySelection.All)
		{
			array = enemyGuidsToSpawn;
		}
		else if (enemySelection == EnemySelection.Random)
		{
			array = new string[UnityEngine.Random.Range(minSpawnCount, maxSpawnCount)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = BraveUtility.RandomElement(enemyGuidsToSpawn);
			}
		}
		SpawnEnemies(array);
	}

	public void ManuallyTrigger(Vector2 damageDirection)
	{
		OnTrigger(damageDirection);
	}

	private void SpawnEnemies(string[] selectedEnemyGuids)
	{
		if (spawnPosition == SpawnPosition.InsideCollider)
		{
			IntVector2 pos = base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
			if (base.aiActor.IsFalling || GameManager.Instance.Dungeon.CellIsPit(base.specRigidbody.UnitCenter.ToVector3ZUp()))
			{
				return;
			}
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(pos);
			List<SpeculativeRigidbody> list = new List<SpeculativeRigidbody>();
			list.Add(base.specRigidbody);
			Vector2 unitBottomLeft = base.specRigidbody.UnitBottomLeft;
			for (int i = 0; i < selectedEnemyGuids.Length; i++)
			{
				AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(selectedEnemyGuids[i]);
				AIActor aIActor = AIActor.Spawn(orLoadByGuid, base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), roomFromPosition);
				if (base.aiActor.IsBlackPhantom)
				{
					aIActor.ForceBlackPhantom = true;
				}
				if ((bool)aIActor)
				{
					aIActor.specRigidbody.Initialize();
					Vector2 vector = unitBottomLeft - (aIActor.specRigidbody.UnitBottomLeft - aIActor.transform.position.XY());
					Vector2 b = vector + new Vector2(Mathf.Max(0f, base.specRigidbody.UnitDimensions.x - aIActor.specRigidbody.UnitDimensions.x), 0f);
					aIActor.transform.position = Vector2.Lerp(vector, b, (selectedEnemyGuids.Length != 1) ? ((float)i / ((float)selectedEnemyGuids.Length - 1f)) : 0f);
					aIActor.specRigidbody.Reinitialize();
					vector -= new Vector2(PhysicsEngine.PixelToUnit(extraPixelWidth), 0f);
					b += new Vector2(PhysicsEngine.PixelToUnit(extraPixelWidth), 0f);
					Vector2 vector2 = Vector2.Lerp(vector, b, (selectedEnemyGuids.Length != 1) ? ((float)i / ((float)selectedEnemyGuids.Length - 1f)) : 0.5f);
					IntVector2 intVector = PhysicsEngine.UnitToPixel(vector2 - aIActor.transform.position.XY());
					CollisionData result = null;
					if (PhysicsEngine.Instance.RigidbodyCastWithIgnores(aIActor.specRigidbody, intVector, out result, true, true, null, false, list.ToArray()))
					{
						intVector = result.NewPixelsToMove;
					}
					CollisionData.Pool.Free(ref result);
					aIActor.transform.position += (Vector3)PhysicsEngine.PixelToUnit(intVector);
					aIActor.specRigidbody.Reinitialize();
					if (i == 0)
					{
						aIActor.aiAnimator.FacingDirection = 180f;
					}
					else if (i == selectedEnemyGuids.Length - 1)
					{
						aIActor.aiAnimator.FacingDirection = 0f;
					}
					HandleSpawn(aIActor);
					list.Add(aIActor.specRigidbody);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				for (int k = 0; k < list.Count; k++)
				{
					if (j != k)
					{
						list[j].RegisterGhostCollisionException(list[k]);
					}
				}
			}
		}
		else if (spawnPosition == SpawnPosition.ScreenEdge)
		{
			for (int l = 0; l < selectedEnemyGuids.Length; l++)
			{
				AIActor orLoadByGuid2 = EnemyDatabase.GetOrLoadByGuid(selectedEnemyGuids[l]);
				AIActor spawnedActor = AIActor.Spawn(orLoadByGuid2, base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), base.aiActor.ParentRoom);
				if (!spawnedActor)
				{
					continue;
				}
				Vector2 cameraBottomLeft = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
				Vector2 cameraTopRight = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
				IntVector2 bottomLeft = cameraBottomLeft.ToIntVector2(VectorConversions.Ceil);
				IntVector2 topRight = cameraTopRight.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
				CellValidator cellValidator = delegate(IntVector2 c)
				{
					for (int num2 = 0; num2 < spawnedActor.Clearance.x; num2++)
					{
						for (int num3 = 0; num3 < spawnedActor.Clearance.y; num3++)
						{
							if (GameManager.Instance.Dungeon.data.isTopWall(c.x + num2, c.y + num3))
							{
								return false;
							}
							if (GameManager.Instance.Dungeon.data[c.x + num2, c.y + num3].isExitCell)
							{
								return false;
							}
						}
					}
					return (c.x >= bottomLeft.x && c.y >= bottomLeft.y && c.x + spawnedActor.Clearance.x - 1 <= topRight.x && c.y + spawnedActor.Clearance.y - 1 <= topRight.y) ? true : false;
				};
				Func<IntVector2, float> cellWeightFinder = delegate(IntVector2 c)
				{
					float a = float.MaxValue;
					a = Mathf.Min(a, (float)c.x - cameraBottomLeft.x);
					a = Mathf.Min(a, (float)c.y - cameraBottomLeft.y);
					a = Mathf.Min(a, cameraTopRight.x - (float)c.x + (float)spawnedActor.Clearance.x);
					return Mathf.Min(a, cameraTopRight.y - (float)c.y + (float)spawnedActor.Clearance.y);
				};
				Vector2 vector3 = spawnedActor.specRigidbody.UnitCenter - spawnedActor.transform.position.XY();
				IntVector2? randomWeightedAvailableCell = spawnedActor.ParentRoom.GetRandomWeightedAvailableCell(spawnedActor.Clearance, spawnedActor.PathableTiles, false, cellValidator, cellWeightFinder, 0.25f);
				if (!randomWeightedAvailableCell.HasValue)
				{
					Debug.LogError("Screen Edge Spawn FAILED!", spawnedActor);
					UnityEngine.Object.Destroy(spawnedActor);
				}
				else
				{
					spawnedActor.transform.position = Pathfinder.GetClearanceOffset(randomWeightedAvailableCell.Value, spawnedActor.Clearance) - vector3;
					spawnedActor.specRigidbody.Reinitialize();
					HandleSpawn(spawnedActor);
				}
			}
		}
		else if (spawnPosition == SpawnPosition.InsideRadius)
		{
			Vector2 unitCenter = base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			List<SpeculativeRigidbody> list2 = new List<SpeculativeRigidbody>();
			list2.Add(base.specRigidbody);
			for (int m = 0; m < selectedEnemyGuids.Length; m++)
			{
				Vector2 vector4 = unitCenter + UnityEngine.Random.insideUnitCircle * spawnRadius;
				if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST && SceneManager.GetActiveScene().name == "fs_robot")
				{
					RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
					Vector2 lhs = entrance.area.basePosition.ToVector2() + new Vector2(7f, 7f);
					Vector2 lhs2 = entrance.area.basePosition.ToVector2() + new Vector2(38f, 36f);
					vector4 = Vector2.Min(lhs2, Vector2.Max(lhs, vector4));
				}
				AIActor orLoadByGuid3 = EnemyDatabase.GetOrLoadByGuid(selectedEnemyGuids[m]);
				AIActor aIActor2 = AIActor.Spawn(orLoadByGuid3, unitCenter.ToIntVector2(VectorConversions.Floor), base.aiActor.ParentRoom, true);
				if ((bool)aIActor2)
				{
					aIActor2.specRigidbody.Initialize();
					Vector2 unit = vector4 - aIActor2.specRigidbody.GetUnitCenter(ColliderType.HitBox);
					aIActor2.specRigidbody.ImpartedPixelsToMove = PhysicsEngine.UnitToPixel(unit);
					HandleSpawn(aIActor2);
					list2.Add(aIActor2.specRigidbody);
				}
			}
			for (int n = 0; n < list2.Count; n++)
			{
				for (int num = 0; num < list2.Count; num++)
				{
					if (n != num)
					{
						list2[n].RegisterGhostCollisionException(list2[num]);
					}
				}
			}
		}
		else
		{
			Debug.LogError("Unknown spawn type: " + spawnPosition);
		}
	}

	private void HandleSpawn(AIActor spawnedActor)
	{
		if (!string.IsNullOrEmpty(spawnAnim))
		{
			spawnedActor.aiAnimator.PlayUntilFinished(spawnAnim);
		}
		SpawnEnemyOnDeath component = spawnedActor.GetComponent<SpawnEnemyOnDeath>();
		if ((bool)component)
		{
			component.guaranteedSpawnGenerations = Mathf.Max(0f, guaranteedSpawnGenerations - 1f);
		}
		if (!spawnsCanDropLoot)
		{
			spawnedActor.CanDropCurrency = false;
			spawnedActor.CanDropItems = false;
		}
		if (DoNormalReinforcement)
		{
			spawnedActor.HandleReinforcementFallIntoRoom(0.1f);
		}
	}
}
