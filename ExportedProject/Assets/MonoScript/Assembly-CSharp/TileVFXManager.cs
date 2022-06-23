using System.Collections.Generic;
using UnityEngine;

public class TileVFXManager : MonoBehaviour
{
	private static TileVFXManager m_instance;

	private List<IntVector2> m_registeredCells = new List<IntVector2>();

	private List<TilesetIndexMetadata> m_registeredMetadata = new List<TilesetIndexMetadata>();

	private List<RuntimeTileVFXData> m_runtimeData = new List<RuntimeTileVFXData>();

	private Vector2 m_frameCameraPosition;

	public static TileVFXManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = GameManager.Instance.Dungeon.gameObject.GetOrAddComponent<TileVFXManager>();
			}
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public void RegisterCellVFX(IntVector2 cellPosition, TilesetIndexMetadata metadata)
	{
		if (m_registeredCells.Contains(cellPosition))
		{
			Debug.Log("registering a cell twice!!!!!!");
			return;
		}
		m_registeredCells.Add(cellPosition);
		m_registeredMetadata.Add(metadata);
		RuntimeTileVFXData item = default(RuntimeTileVFXData);
		if (metadata.tileVFXPlaystyle == TilesetIndexMetadata.VFXPlaystyle.TIMED_REPEAT)
		{
			item.cooldownRemaining = Random.Range(0f, Random.Range(metadata.tileVFXDelayTime - metadata.tileVFXDelayVariance, metadata.tileVFXDelayTime + metadata.tileVFXDelayVariance));
		}
		m_runtimeData.Add(item);
	}

	private void CreateVFX(IntVector2 cellPosition, TilesetIndexMetadata cellMetadata, RuntimeTileVFXData runtimeData, bool ignoreCulling = false)
	{
		Vector3 vector = (cellPosition.ToVector2() + cellMetadata.tileVFXOffset).ToVector3ZUp();
		vector.z = vector.y;
		if (ignoreCulling)
		{
			SpawnManager.SpawnVFX(cellMetadata.tileVFXPrefab, vector, Quaternion.identity);
			return;
		}
		Vector2 vector2 = m_frameCameraPosition - vector.XY();
		vector2.y *= 1.7f;
		if (!(vector2.sqrMagnitude > 420f))
		{
			SpawnManager.SpawnVFX(cellMetadata.tileVFXPrefab, vector, Quaternion.identity);
		}
	}

	private void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		m_frameCameraPosition = GameManager.Instance.MainCameraController.transform.PositionVector2();
		if (m_registeredCells.Count != m_registeredMetadata.Count || m_registeredCells.Count != m_runtimeData.Count)
		{
			Debug.LogError("MISMATCH IN TILE VFX MANAGER, THIS IS NOT GOOD.");
			return;
		}
		for (int i = 0; i < m_registeredCells.Count; i++)
		{
			IntVector2 intVector = m_registeredCells[i];
			TilesetIndexMetadata tilesetIndexMetadata = m_registeredMetadata[i];
			RuntimeTileVFXData runtimeTileVFXData = m_runtimeData[i];
			if (tilesetIndexMetadata.tileVFXPlaystyle == TilesetIndexMetadata.VFXPlaystyle.CONTINUOUS)
			{
				if (!runtimeTileVFXData.vfxHasEverBeenInstantiated)
				{
					CreateVFX(intVector, tilesetIndexMetadata, runtimeTileVFXData, true);
					runtimeTileVFXData.vfxHasEverBeenInstantiated = true;
				}
			}
			else if (tilesetIndexMetadata.tileVFXPlaystyle == TilesetIndexMetadata.VFXPlaystyle.TIMED_REPEAT)
			{
				runtimeTileVFXData.cooldownRemaining = Mathf.Max(0f, runtimeTileVFXData.cooldownRemaining - BraveTime.DeltaTime);
				if (runtimeTileVFXData.cooldownRemaining <= 0f)
				{
					CreateVFX(intVector, tilesetIndexMetadata, runtimeTileVFXData);
					runtimeTileVFXData.vfxHasEverBeenInstantiated = true;
					runtimeTileVFXData.cooldownRemaining = Random.Range(tilesetIndexMetadata.tileVFXDelayTime - tilesetIndexMetadata.tileVFXDelayVariance, tilesetIndexMetadata.tileVFXDelayTime + tilesetIndexMetadata.tileVFXDelayVariance);
				}
			}
			else if (tilesetIndexMetadata.tileVFXPlaystyle == TilesetIndexMetadata.VFXPlaystyle.ON_ANIMATION_FRAME && TK2DTilemapChunkAnimator.PositionToAnimatorMap.ContainsKey(intVector))
			{
				for (int j = 0; j < TK2DTilemapChunkAnimator.PositionToAnimatorMap[intVector].Count; j++)
				{
					if (TK2DTilemapChunkAnimator.PositionToAnimatorMap[intVector][j].associatedMetadata != tilesetIndexMetadata)
					{
						continue;
					}
					TilemapAnimatorTileManager tilemapAnimatorTileManager = TK2DTilemapChunkAnimator.PositionToAnimatorMap[intVector][j];
					if (tilemapAnimatorTileManager.CurrentFrame == tilesetIndexMetadata.tileVFXAnimFrame)
					{
						if (!runtimeTileVFXData.vfxHasEverBeenInstantiated)
						{
							CreateVFX(intVector, tilesetIndexMetadata, runtimeTileVFXData);
							runtimeTileVFXData.vfxHasEverBeenInstantiated = true;
						}
					}
					else
					{
						runtimeTileVFXData.vfxHasEverBeenInstantiated = false;
					}
				}
			}
			m_runtimeData[i] = runtimeTileVFXData;
		}
	}
}
