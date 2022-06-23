using System.Collections.Generic;
using UnityEngine;

public class TilemapAnimatorTileManager
{
	public tk2dSpriteCollectionData associatedCollection;

	public int associatedSpriteId;

	public TilesetIndexMetadata associatedMetadata;

	public SimpleTilesetAnimationSequence associatedSequence;

	public SimpleTilesetAnimationSequence loopceptionSequence;

	private bool m_isLoopcepting;

	public IntVector2 worldPosition;

	public TK2DTilemapChunkAnimator animator;

	public List<TilemapAnimatorTileManager> linkedManagers = new List<TilemapAnimatorTileManager>();

	public int startUVIndex;

	public int uvCount;

	private float m_delayRemaining;

	private float m_elapsed;

	private float m_cachedSequenceLength;

	private float m_cachedLoopceptionLength;

	private int m_lastTargetEntry;

	private bool m_triggered;

	private bool m_forceNextUpdate;

	private int m_loopceptionLoopsRemaining;

	public int CurrentFrame
	{
		get
		{
			return m_lastTargetEntry;
		}
	}

	public TilemapAnimatorTileManager(tk2dSpriteCollectionData sourceCollection, int sourceSpriteId, TilesetIndexMetadata metadata, int uvStart, int numUV, tk2dTileMap tilemap)
	{
		associatedCollection = sourceCollection;
		associatedSpriteId = sourceSpriteId;
		associatedMetadata = metadata;
		associatedSequence = associatedCollection.GetAnimationSequence(associatedSpriteId);
		if (associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.LOOPCEPTION)
		{
			loopceptionSequence = tilemap.SpriteCollectionInst.GetAnimationSequence(associatedSequence.loopceptionTarget);
		}
		startUVIndex = uvStart;
		uvCount = numUV;
		m_elapsed = 0f;
		m_cachedSequenceLength = 0f;
		for (int i = 0; i < associatedSequence.entries.Count; i++)
		{
			m_cachedSequenceLength += associatedSequence.entries[i].frameTime;
		}
		if (associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.LOOPCEPTION)
		{
			for (int j = 0; j < loopceptionSequence.entries.Count; j++)
			{
				m_cachedLoopceptionLength += loopceptionSequence.entries[j].frameTime;
			}
		}
		if (associatedSequence.randomStartFrame)
		{
			m_elapsed = Mathf.Lerp(0f, m_cachedSequenceLength, Random.value);
		}
	}

	protected void UpdateChildSectionInternal(Vector2[] refMeshUVs, tk2dTileMap tileMap, int targetEntryIndex)
	{
		SimpleTilesetAnimationSequence simpleTilesetAnimationSequence = ((!m_isLoopcepting) ? associatedSequence : loopceptionSequence);
		m_lastTargetEntry = targetEntryIndex;
		SimpleTilesetAnimationSequenceEntry simpleTilesetAnimationSequenceEntry = simpleTilesetAnimationSequence.entries[targetEntryIndex];
		tk2dSpriteDefinition tk2dSpriteDefinition2 = tileMap.SpriteCollectionInst.spriteDefinitions[simpleTilesetAnimationSequenceEntry.entryIndex];
		for (int i = startUVIndex; i < startUVIndex + uvCount; i++)
		{
			refMeshUVs[i] = tk2dSpriteDefinition2.uvs[i - startUVIndex];
		}
	}

	public void TriggerAnimationSequence()
	{
		if (!m_triggered)
		{
			m_elapsed = 0f;
			m_triggered = true;
		}
	}

	public void UntriggerAnimationSequence()
	{
		if (m_triggered)
		{
			m_elapsed = 0f;
			m_triggered = false;
			m_forceNextUpdate = true;
		}
	}

	public bool UpdateRelevantSection(ref Vector2[] refMeshUVs, Mesh refMesh, tk2dTileMap tileMap, float deltaTime)
	{
		if (!m_forceNextUpdate)
		{
			if (associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.TRIGGERED_ONCE && !m_triggered)
			{
				return false;
			}
			if (associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.TRIGGERED_ONCE && m_elapsed > m_cachedSequenceLength)
			{
				return false;
			}
		}
		m_forceNextUpdate = false;
		float num = deltaTime;
		if (m_delayRemaining > 0f)
		{
			if (deltaTime <= m_delayRemaining)
			{
				m_delayRemaining -= deltaTime;
				return false;
			}
			num = deltaTime - m_delayRemaining;
			m_delayRemaining = 0f;
		}
		if (associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.TRIGGERED_ONCE && !m_triggered)
		{
			m_elapsed = 0f;
		}
		else
		{
			m_elapsed += num;
		}
		if (m_elapsed >= m_cachedSequenceLength && associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.DELAYED_LOOP)
		{
			float num2 = Mathf.Lerp(associatedSequence.loopDelayMin, associatedSequence.loopDelayMax, Random.value);
			m_delayRemaining = num2 - m_elapsed % m_cachedSequenceLength;
			m_elapsed = 0f;
			return false;
		}
		if (associatedSequence.playstyle == SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.LOOPCEPTION)
		{
			if (!m_isLoopcepting && m_elapsed >= m_cachedSequenceLength)
			{
				if (m_loopceptionLoopsRemaining > 0)
				{
					m_loopceptionLoopsRemaining--;
					m_elapsed %= m_cachedSequenceLength;
				}
				else
				{
					m_isLoopcepting = true;
					m_elapsed %= m_cachedSequenceLength;
					m_loopceptionLoopsRemaining = Random.Range(associatedSequence.loopceptionMin, associatedSequence.loopceptionMax);
				}
			}
			else if (m_isLoopcepting && m_elapsed >= m_cachedLoopceptionLength)
			{
				if (m_loopceptionLoopsRemaining > 0)
				{
					m_loopceptionLoopsRemaining--;
					m_elapsed %= m_cachedLoopceptionLength;
				}
				else
				{
					m_isLoopcepting = false;
					m_elapsed %= m_cachedLoopceptionLength;
					m_loopceptionLoopsRemaining = Random.Range(associatedSequence.coreceptionMin, associatedSequence.coreceptionMax);
				}
			}
		}
		else if (associatedSequence.playstyle != SimpleTilesetAnimationSequence.TilesetSequencePlayStyle.TRIGGERED_ONCE)
		{
			m_elapsed %= m_cachedSequenceLength;
		}
		m_elapsed = Mathf.Clamp(m_elapsed, 0f, m_cachedSequenceLength);
		SimpleTilesetAnimationSequence simpleTilesetAnimationSequence = ((!m_isLoopcepting) ? associatedSequence : loopceptionSequence);
		float num3 = 0f;
		int num4 = 0;
		for (num4 = 0; num4 < simpleTilesetAnimationSequence.entries.Count; num4++)
		{
			num3 += simpleTilesetAnimationSequence.entries[num4].frameTime;
			if (num3 >= m_elapsed)
			{
				break;
			}
		}
		if (num4 == m_lastTargetEntry)
		{
			return false;
		}
		m_lastTargetEntry = num4;
		SimpleTilesetAnimationSequenceEntry simpleTilesetAnimationSequenceEntry = simpleTilesetAnimationSequence.entries[num4];
		tk2dSpriteDefinition tk2dSpriteDefinition2 = tileMap.SpriteCollectionInst.spriteDefinitions[simpleTilesetAnimationSequenceEntry.entryIndex];
		for (int i = startUVIndex; i < startUVIndex + uvCount; i++)
		{
			refMeshUVs[i] = tk2dSpriteDefinition2.uvs[i - startUVIndex];
		}
		for (int j = 0; j < linkedManagers.Count; j++)
		{
			linkedManagers[j].UpdateChildSectionInternal(refMeshUVs, tileMap, num4);
		}
		return true;
	}
}
