using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageTrailController : BraveBehaviour
{
	private class Shadow
	{
		public float timer;

		public tk2dSprite sprite;
	}

	public bool spawnShadows = true;

	public float shadowTimeDelay = 0.1f;

	public float shadowLifetime = 0.6f;

	public float minTranslation = 0.2f;

	public float maxEmission = 800f;

	public float minEmission = 100f;

	public float targetHeight = -2f;

	public Color dashColor = new Color(1f, 0f, 1f, 1f);

	public Shader OptionalImageShader;

	public bool UseTargetLayer;

	public string TargetLayer;

	[NonSerialized]
	public Shader OverrideImageShader;

	private readonly LinkedList<Shadow> m_activeShadows = new LinkedList<Shadow>();

	private readonly LinkedList<Shadow> m_inactiveShadows = new LinkedList<Shadow>();

	private float m_spawnTimer;

	private Vector2 m_lastSpawnPosition;

	private bool m_previousFrameSpawnShadows;

	public void Start()
	{
		if (OptionalImageShader != null)
		{
			OverrideImageShader = OptionalImageShader;
		}
		if (base.transform.parent != null && base.transform.parent.GetComponent<Projectile>() != null)
		{
			base.transform.parent.GetComponent<Projectile>().OnDestruction += projectile_OnDestruction;
		}
		m_lastSpawnPosition = base.transform.position;
	}

	private void projectile_OnDestruction(Projectile source)
	{
		if (m_activeShadows.Count > 0)
		{
			GameManager.Instance.StartCoroutine(HandleDeathShadowCleanup());
		}
	}

	public void LateUpdate()
	{
		if (spawnShadows && !m_previousFrameSpawnShadows)
		{
			m_spawnTimer = shadowTimeDelay;
		}
		m_previousFrameSpawnShadows = spawnShadows;
		LinkedListNode<Shadow> linkedListNode = m_activeShadows.First;
		while (linkedListNode != null)
		{
			LinkedListNode<Shadow> next = linkedListNode.Next;
			linkedListNode.Value.timer -= BraveTime.DeltaTime;
			if (linkedListNode.Value.timer <= 0f)
			{
				m_activeShadows.Remove(linkedListNode);
				m_inactiveShadows.AddLast(linkedListNode);
				if ((bool)linkedListNode.Value.sprite)
				{
					linkedListNode.Value.sprite.renderer.enabled = false;
				}
			}
			else if ((bool)linkedListNode.Value.sprite)
			{
				float num = linkedListNode.Value.timer / shadowLifetime;
				Material sharedMaterial = linkedListNode.Value.sprite.renderer.sharedMaterial;
				sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(maxEmission, minEmission, num));
				sharedMaterial.SetFloat("_Opacity", num);
			}
			linkedListNode = next;
		}
		if (spawnShadows)
		{
			if (m_spawnTimer > 0f)
			{
				m_spawnTimer -= BraveTime.DeltaTime;
			}
			if (m_spawnTimer <= 0f && Vector2.Distance(m_lastSpawnPosition, base.transform.position) > minTranslation)
			{
				SpawnNewShadow();
				m_spawnTimer += shadowTimeDelay;
				m_lastSpawnPosition = base.transform.position;
			}
		}
	}

	private IEnumerator HandleDeathShadowCleanup()
	{
		while (m_activeShadows.Count > 0)
		{
			LinkedListNode<Shadow> node = m_activeShadows.First;
			while (node != null)
			{
				LinkedListNode<Shadow> next = node.Next;
				node.Value.timer -= BraveTime.DeltaTime;
				if (node.Value.timer <= 0f)
				{
					m_activeShadows.Remove(node);
					m_inactiveShadows.AddLast(node);
					if ((bool)node.Value.sprite)
					{
						node.Value.sprite.renderer.enabled = false;
					}
				}
				else if ((bool)node.Value.sprite)
				{
					float num = node.Value.timer / shadowLifetime;
					Material sharedMaterial = node.Value.sprite.renderer.sharedMaterial;
					sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(maxEmission, minEmission, num));
					sharedMaterial.SetFloat("_Opacity", num);
				}
				node = next;
			}
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void SpawnNewShadow()
	{
		if (m_inactiveShadows.Count == 0)
		{
			CreateInactiveShadow();
		}
		LinkedListNode<Shadow> first = m_inactiveShadows.First;
		tk2dSprite tk2dSprite2 = first.Value.sprite;
		m_inactiveShadows.RemoveFirst();
		if ((bool)tk2dSprite2 && (bool)tk2dSprite2.renderer)
		{
			first.Value.timer = shadowLifetime;
			tk2dSprite2.SetSprite(base.sprite.Collection, base.sprite.spriteId);
			tk2dSprite2.transform.position = base.sprite.transform.position;
			tk2dSprite2.transform.rotation = base.sprite.transform.rotation;
			tk2dSprite2.scale = base.sprite.scale;
			tk2dSprite2.usesOverrideMaterial = true;
			tk2dSprite2.IsPerpendicular = true;
			if ((bool)tk2dSprite2.renderer)
			{
				tk2dSprite2.renderer.enabled = true;
				tk2dSprite2.renderer.material.shader = OverrideImageShader ?? ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage");
				tk2dSprite2.renderer.sharedMaterial.SetFloat("_EmissivePower", minEmission);
				tk2dSprite2.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
				tk2dSprite2.renderer.sharedMaterial.SetColor("_DashColor", dashColor);
			}
			tk2dSprite2.HeightOffGround = targetHeight;
			tk2dSprite2.UpdateZDepth();
			m_activeShadows.AddLast(first);
		}
	}

	private void CreateInactiveShadow()
	{
		GameObject gameObject = new GameObject("after image");
		if (UseTargetLayer)
		{
			gameObject.layer = LayerMask.NameToLayer(TargetLayer);
		}
		tk2dSprite tk2dSprite2 = gameObject.AddComponent<tk2dSprite>();
		gameObject.transform.parent = SpawnManager.Instance.VFX;
		m_inactiveShadows.AddLast(new Shadow
		{
			timer = shadowLifetime,
			sprite = tk2dSprite2
		});
	}
}
