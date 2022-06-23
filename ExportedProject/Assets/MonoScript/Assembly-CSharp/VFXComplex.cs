using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VFXComplex
{
	public delegate GameObject SpawnMethod(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools);

	public VFXObject[] effects;

	private List<GameObject> m_spawnedObjects = new List<GameObject>();

	public void SpawnAtPosition(float xPosition, float yPositionAtGround, float heightOffGround, float zRotation = 0f, Transform parent = null, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, bool keepReferences = false, SpawnMethod spawnMethod = null, bool ignoresPools = false)
	{
		Vector3 position = new Vector3(xPosition, yPositionAtGround + heightOffGround, yPositionAtGround - heightOffGround);
		Action<VFXObject, tk2dSprite> vfxSpriteManipulator = delegate(VFXObject effect, tk2dSprite vfxSprite)
		{
			vfxSprite.HeightOffGround = 2f * heightOffGround;
			vfxSprite.UpdateZDepth();
		};
		InternalSpawnAtLocation(position, zRotation, parent, sourceNormal, sourceVelocity, vfxSpriteManipulator, keepReferences, spawnMethod, ignoresPools);
	}

	public void SpawnAtPosition(Vector3 position, float zRotation = 0f, Transform parent = null, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, float? heightOffGround = null, bool keepReferences = false, SpawnMethod spawnMethod = null, tk2dBaseSprite spriteParent = null, bool ignoresPools = false)
	{
		Action<VFXObject, tk2dSprite> vfxSpriteManipulator = delegate(VFXObject effect, tk2dSprite vfxSprite)
		{
			if (spriteParent != null)
			{
				spriteParent.AttachRenderer(vfxSprite);
				vfxSprite.HeightOffGround = 0.05f;
				vfxSprite.UpdateZDepth();
			}
			else if (vfxSprite.Collection != null)
			{
				DepthLookupManager.ProcessRenderer(vfxSprite.renderer);
				if (Mathf.Abs(zRotation) > 90f)
				{
					vfxSprite.FlipY = true;
				}
				if (heightOffGround.HasValue)
				{
					vfxSprite.HeightOffGround = heightOffGround.Value;
				}
				else if (effect.usesZHeight)
				{
					vfxSprite.HeightOffGround = effect.zHeight;
				}
				else
				{
					vfxSprite.HeightOffGround = 0.9f;
				}
				vfxSprite.UpdateZDepth();
			}
		};
		InternalSpawnAtLocation(position, zRotation, parent, sourceNormal, sourceVelocity, vfxSpriteManipulator, keepReferences, spawnMethod, ignoresPools);
	}

	public void SpawnAtLocalPosition(Vector3 localPosition, float zRotation, Transform parent, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, bool keepReferences = false, SpawnMethod spawnMethod = null, bool ignoresPools = false)
	{
		Vector3 position = parent.transform.position + localPosition;
		Action<VFXObject, tk2dSprite> vfxSpriteManipulator = delegate(VFXObject effect, tk2dSprite vfxSprite)
		{
			if (effect.usesZHeight)
			{
				vfxSprite.HeightOffGround = effect.zHeight;
			}
			else if (effect.orphaned && !vfxSprite.IsPerpendicular)
			{
				vfxSprite.HeightOffGround = 0f;
			}
			else
			{
				vfxSprite.HeightOffGround = 0.9f;
			}
			vfxSprite.UpdateZDepth();
		};
		InternalSpawnAtLocation(position, zRotation, parent, sourceNormal, sourceVelocity, vfxSpriteManipulator, keepReferences, spawnMethod, ignoresPools);
	}

	public void RemoveDespawnedVfx()
	{
		for (int num = m_spawnedObjects.Count - 1; num >= 0; num--)
		{
			if (!m_spawnedObjects[num] || !m_spawnedObjects[num].activeSelf)
			{
				m_spawnedObjects.RemoveAt(num);
			}
		}
	}

	public void DestroyAll()
	{
		for (int i = 0; i < m_spawnedObjects.Count; i++)
		{
			if ((bool)m_spawnedObjects[i])
			{
				if ((bool)SpawnManager.Instance)
				{
					m_spawnedObjects[i].transform.parent = SpawnManager.Instance.VFX;
				}
				SpawnManager.Despawn(m_spawnedObjects[i]);
			}
		}
		m_spawnedObjects.Clear();
	}

	public void ForEach(Action<GameObject> action)
	{
		for (int i = 0; i < m_spawnedObjects.Count; i++)
		{
			if ((bool)m_spawnedObjects[i])
			{
				action(m_spawnedObjects[i]);
			}
		}
	}

	public void ToggleRenderers(bool value)
	{
		for (int i = 0; i < m_spawnedObjects.Count; i++)
		{
			if ((bool)m_spawnedObjects[i])
			{
				Renderer[] componentsInChildren = m_spawnedObjects[i].GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = value;
				}
			}
		}
	}

	public void SetHeightOffGround(float height)
	{
		for (int i = 0; i < m_spawnedObjects.Count; i++)
		{
			if (!m_spawnedObjects[i])
			{
				continue;
			}
			tk2dBaseSprite[] componentsInChildren = m_spawnedObjects[i].GetComponentsInChildren<tk2dBaseSprite>();
			foreach (tk2dBaseSprite tk2dBaseSprite2 in componentsInChildren)
			{
				tk2dBaseSprite2.HeightOffGround = height;
				if (tk2dBaseSprite2.attachParent == null)
				{
					tk2dBaseSprite2.UpdateZDepth();
				}
			}
		}
	}

	protected void HandleDebris(GameObject vfx, float heightOffGround, Vector2? sourceNormal, Vector2? sourceVelocity)
	{
		DebrisObject component = vfx.GetComponent<DebrisObject>();
		if (component != null && sourceNormal.HasValue && sourceVelocity.HasValue)
		{
			if (!sourceNormal.HasValue)
			{
				Debug.LogWarning("Trying to create debris for an effect with no normal.");
			}
			if (!sourceVelocity.HasValue)
			{
				Debug.LogWarning("Trying to create debris for an effect with no velocity.");
			}
			tk2dBaseSprite sprite = component.sprite;
			sprite.IsPerpendicular = false;
			sprite.usesOverrideMaterial = true;
			Bounds bounds = sprite.GetBounds();
			component.transform.position = component.transform.position + new Vector3(BraveMathCollege.ActualSign(sourceNormal.Value.x) * bounds.size.x, 0f, 0f);
			float z = Mathf.Atan2(sourceVelocity.Value.y, sourceVelocity.Value.x) * 57.29578f;
			vfx.transform.localRotation = Quaternion.Euler(0f, 0f, z);
			Vector2 normalized = BraveMathCollege.ReflectVectorAcrossNormal(sourceVelocity.Value, sourceNormal.Value).normalized;
			float z2 = UnityEngine.Random.Range(-20f, 20f);
			normalized = Quaternion.Euler(0f, 0f, z2) * normalized;
			component.Trigger(normalized.ToVector3ZUp(0.1f), heightOffGround + 1f);
		}
	}

	protected void HandleAttachment(tk2dSprite vfxSprite, Transform parent)
	{
		if (parent != null && parent.parent != null)
		{
			tk2dSprite componentInChildren = parent.parent.GetComponentInChildren<tk2dSprite>();
			if (vfxSprite != null && componentInChildren != null)
			{
				componentInChildren.AttachRenderer(vfxSprite);
			}
		}
	}

	protected void InternalSpawnAtLocation(Vector3 position, float zRotation, Transform parent, Vector2? sourceNormal, Vector2? sourceVelocity, Action<VFXObject, tk2dSprite> vfxSpriteManipulator, bool keepReferences, SpawnMethod spawnMethod, bool ignoresPools)
	{
		if (spawnMethod == null)
		{
			spawnMethod = SpawnManager.SpawnVFX;
		}
		m_spawnedObjects.RemoveAll((GameObject go) => !go);
		for (int i = 0; i < effects.Length; i++)
		{
			if (effects[i].effect == null)
			{
				continue;
			}
			if (effects[i].alignment == VFXAlignment.NormalAligned && sourceNormal.HasValue)
			{
				zRotation = Mathf.Atan2(sourceNormal.Value.y, sourceNormal.Value.x) * 57.29578f;
			}
			if (effects[i].alignment == VFXAlignment.VelocityAligned && sourceVelocity.HasValue)
			{
				zRotation = Mathf.Atan2(sourceVelocity.Value.y, sourceVelocity.Value.x) * 57.29578f + 180f;
			}
			Vector3 position2 = position.Quantize(0.0625f);
			GameObject gameObject = spawnMethod(effects[i].effect, position2, Quaternion.identity, ignoresPools);
			if (!gameObject)
			{
				continue;
			}
			if (keepReferences && !effects[i].persistsOnDeath)
			{
				m_spawnedObjects.Add(gameObject);
			}
			tk2dSprite componentInChildren = gameObject.GetComponentInChildren<tk2dSprite>();
			if (componentInChildren != null)
			{
				vfxSpriteManipulator(effects[i], componentInChildren);
				if (effects[i].usesZHeight)
				{
					componentInChildren.HeightOffGround = effects[i].zHeight;
					componentInChildren.UpdateZDepth();
				}
			}
			if ((bool)gameObject.GetComponent<ParticleSystem>())
			{
				ParticleKiller component = gameObject.GetComponent<ParticleKiller>();
				if ((bool)component && component.overrideXRotation)
				{
					gameObject.transform.localRotation = Quaternion.Euler(component.xRotation, 0f, 0f);
				}
				else
				{
					gameObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
				}
				gameObject.transform.position = gameObject.transform.position.WithZ(gameObject.transform.position.y);
			}
			else
			{
				gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
			}
			HandleDebris(gameObject, 0.5f, sourceNormal, sourceVelocity);
			if (!(parent != null))
			{
				continue;
			}
			if (!effects[i].orphaned)
			{
				gameObject.transform.parent = parent;
				gameObject.transform.localScale = Vector3.one;
				PersistentVFXManagerBehaviour persistentVFXManagerBehaviour = parent.GetComponentInChildren<PersistentVFXManagerBehaviour>() ?? parent.GetComponentInParent<PersistentVFXManagerBehaviour>();
				if (persistentVFXManagerBehaviour != null && !gameObject.GetComponent<SpriteAnimatorKiller>())
				{
					if (effects[i].destructible)
					{
						persistentVFXManagerBehaviour.AttachDestructibleVFX(gameObject);
					}
					else
					{
						persistentVFXManagerBehaviour.AttachPersistentVFX(gameObject);
					}
				}
				if (effects[i].attached)
				{
					HandleAttachment(componentInChildren, parent);
				}
			}
			else
			{
				gameObject.transform.localScale = parent.localScale;
			}
		}
	}
}
