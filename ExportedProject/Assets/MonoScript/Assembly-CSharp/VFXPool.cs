using System;
using UnityEngine;

[Serializable]
public class VFXPool
{
	public VFXPoolType type;

	public VFXComplex[] effects;

	private int m_iterator;

	public VFXComplex GetEffect()
	{
		if (effects == null || effects.Length == 0)
		{
			return null;
		}
		switch (type)
		{
		case VFXPoolType.None:
			return null;
		case VFXPoolType.Single:
			return effects[0];
		case VFXPoolType.All:
			return effects[0];
		case VFXPoolType.SequentialGroups:
		{
			VFXComplex result = effects[m_iterator];
			m_iterator = (m_iterator + 1) % effects.Length;
			return result;
		}
		case VFXPoolType.RandomGroups:
			return effects[UnityEngine.Random.Range(0, effects.Length)];
		default:
			Debug.LogWarning("Unknown VFXPoolType " + type);
			return null;
		}
	}

	public void SpawnAtPosition(float xPosition, float yPositionAtGround, float heightOffGround, float zRotation, Transform parent = null, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, bool keepReferences = false, VFXComplex.SpawnMethod spawnMethod = null, bool ignoresPools = false)
	{
		VFXComplex effect = GetEffect();
		if (effect != null)
		{
			effect.SpawnAtPosition(xPosition, yPositionAtGround, heightOffGround, zRotation, parent, sourceNormal, sourceVelocity, keepReferences, spawnMethod, ignoresPools);
		}
	}

	public void SpawnAtPosition(Vector3 position, float zRotation = 0f, Transform parent = null, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, float? heightOffGround = null, bool keepReferences = false, VFXComplex.SpawnMethod spawnMethod = null, tk2dBaseSprite spriteParent = null, bool ignoresPools = false)
	{
		VFXComplex effect = GetEffect();
		if (effect != null)
		{
			effect.SpawnAtPosition(position, zRotation, parent, sourceNormal, sourceVelocity, heightOffGround, keepReferences, spawnMethod, spriteParent, ignoresPools);
		}
	}

	public void SpawnAtTilemapPosition(Vector3 position, float yPositionAtGround, float zRotation, Vector2 sourceNormal, Vector2 sourceVelocity, bool keepReferences = false, VFXComplex.SpawnMethod spawnMethod = null, bool ignoresPools = false)
	{
		VFXComplex effect = GetEffect();
		if (effect != null)
		{
			float heightOffGround = position.y - yPositionAtGround;
			effect.SpawnAtPosition(position.x, yPositionAtGround, heightOffGround, zRotation, null, sourceNormal, sourceVelocity, keepReferences, spawnMethod, ignoresPools);
		}
	}

	public void SpawnAtLocalPosition(Vector3 localPosition, float zRotation, Transform parent, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, bool keepReferences = false, VFXComplex.SpawnMethod spawnMethod = null, bool ignoresPools = false)
	{
		VFXComplex effect = GetEffect();
		if (effect != null)
		{
			effect.SpawnAtLocalPosition(localPosition, zRotation, parent, sourceNormal, sourceVelocity, keepReferences, spawnMethod, ignoresPools);
		}
	}

	public void RemoveDespawnedVfx()
	{
		for (int i = 0; i < effects.Length; i++)
		{
			effects[i].RemoveDespawnedVfx();
		}
	}

	public void DestroyAll()
	{
		for (int i = 0; i < effects.Length; i++)
		{
			effects[i].DestroyAll();
		}
	}

	public void ForEach(Action<GameObject> action)
	{
		for (int i = 0; i < effects.Length; i++)
		{
			effects[i].ForEach(action);
		}
	}

	public void ToggleRenderers(bool value)
	{
		for (int i = 0; i < effects.Length; i++)
		{
			effects[i].ToggleRenderers(value);
		}
	}

	public void SetHeightOffGround(float height)
	{
		for (int i = 0; i < effects.Length; i++)
		{
			effects[i].SetHeightOffGround(height);
		}
	}
}
