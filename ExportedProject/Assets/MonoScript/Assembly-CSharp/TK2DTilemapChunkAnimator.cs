using System.Collections.Generic;
using UnityEngine;

public class TK2DTilemapChunkAnimator : MonoBehaviour
{
	public static Dictionary<IntVector2, List<TilemapAnimatorTileManager>> PositionToAnimatorMap = new Dictionary<IntVector2, List<TilemapAnimatorTileManager>>(new IntVector2EqualityComparer());

	private List<TilemapAnimatorTileManager> m_tiles;

	private Mesh m_refMesh;

	private tk2dTileMap m_refTilemap;

	private Vector2[] m_currentUVs;

	public void Initialize(List<TilemapAnimatorTileManager> tiles, Mesh refMesh, tk2dTileMap refTilemap)
	{
		m_tiles = tiles;
		for (int i = 0; i < m_tiles.Count; i++)
		{
			m_tiles[i].animator = this;
		}
		m_refMesh = refMesh;
		m_refTilemap = refTilemap;
		m_currentUVs = m_refMesh.uv;
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < m_tiles.Count; i++)
		{
			if (m_tiles[i].UpdateRelevantSection(ref m_currentUVs, m_refMesh, m_refTilemap, BraveTime.DeltaTime))
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_refMesh.uv = m_currentUVs;
		}
	}
}
