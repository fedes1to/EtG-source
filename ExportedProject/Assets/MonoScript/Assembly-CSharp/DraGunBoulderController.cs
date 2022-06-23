using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraGunBoulderController : BraveBehaviour
{
	public float LifeTime = 1f;

	public tk2dSprite CircleSprite;

	private float m_lifeTime;

	private List<PlayerController> m_cursedPlayers = new List<PlayerController>();

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEntered));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody2.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(HandleTriggerExited));
		if ((bool)CircleSprite)
		{
			tk2dSpriteDefinition currentSpriteDef = CircleSprite.GetCurrentSpriteDef();
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i < currentSpriteDef.uvs.Length; i++)
			{
				vector = Vector2.Min(vector, currentSpriteDef.uvs[i]);
				vector2 = Vector2.Max(vector2, currentSpriteDef.uvs[i]);
			}
			Vector2 vector3 = (vector + vector2) / 2f;
			CircleSprite.renderer.material.SetVector("_WorldCenter", new Vector4(vector3.x, vector3.y, vector3.x - vector.x, vector3.y - vector.y));
		}
		m_lifeTime = 0f;
	}

	private void Update()
	{
		m_lifeTime += BraveTime.DeltaTime;
		if (m_lifeTime >= LifeTime)
		{
			m_lifeTime = 0f;
			GameManager.Instance.Dungeon.StartCoroutine(HandleBreakCR());
		}
		for (int i = 0; i < m_cursedPlayers.Count; i++)
		{
			DoCurse(m_cursedPlayers[i]);
		}
	}

	private IEnumerator HandleBreakCR()
	{
		float elapsed = 0f;
		float duration = 0.3f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			if ((bool)CircleSprite)
			{
				CircleSprite.scale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
			}
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void DoCurse(PlayerController targetPlayer)
	{
		if (!targetPlayer.IsGhost)
		{
			targetPlayer.CurrentStoneGunTimer = Mathf.Max(targetPlayer.CurrentStoneGunTimer, 0.3f);
		}
	}

	private void HandleTriggerExited(SpeculativeRigidbody exitRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		if ((bool)exitRigidbody && (bool)exitRigidbody.gameActor && exitRigidbody.gameActor is PlayerController && m_cursedPlayers.Contains(exitRigidbody.gameActor as PlayerController))
		{
			m_cursedPlayers.Remove(exitRigidbody.gameActor as PlayerController);
		}
	}

	private void HandleTriggerEntered(SpeculativeRigidbody enteredRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2()) == GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(enteredRigidbody.UnitCenter.ToIntVector2()) && enteredRigidbody.gameActor != null && enteredRigidbody.gameActor is PlayerController)
		{
			m_cursedPlayers.Add(enteredRigidbody.gameActor as PlayerController);
		}
	}
}
