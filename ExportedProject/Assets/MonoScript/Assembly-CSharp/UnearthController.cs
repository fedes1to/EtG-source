using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnearthController : BraveBehaviour
{
	private enum UnearthState
	{
		Idle,
		Unearth,
		Finished
	}

	public string triggerAnim;

	public List<GameObject> dirtVfx;

	public int dirtCount;

	public List<GameObject> dustVfx;

	public float dustMidDelay = 0.05f;

	public Vector2 dustOffset;

	public Vector2 dustDimensions;

	private UnearthState m_state;

	private void Update()
	{
		if (m_state == UnearthState.Idle)
		{
			if (base.aiAnimator.IsPlaying(triggerAnim))
			{
				m_state = UnearthState.Unearth;
				StartCoroutine(DirtCR());
				StartCoroutine(PuffCR());
			}
		}
		else if (m_state == UnearthState.Unearth && !base.aiAnimator.IsPlaying(triggerAnim))
		{
			m_state = UnearthState.Finished;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private IEnumerator DirtCR()
	{
		List<GameObject> dirtObjs = new List<GameObject>();
		Vector2 minPos = base.specRigidbody.UnitBottomLeft;
		Vector2 maxPos = base.specRigidbody.UnitBottomRight;
		for (int i = 0; i < dirtCount; i++)
		{
			GameObject prefab = BraveUtility.RandomElement(dirtVfx);
			Vector2 vector = BraveUtility.RandomVector2(minPos, maxPos, new Vector2(0.125f, 0f));
			GameObject gameObject = SpawnManager.SpawnVFX(prefab, vector, Quaternion.identity);
			dirtObjs.Add(gameObject);
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			if ((bool)component)
			{
				base.sprite.AttachRenderer(component);
				component.HeightOffGround = 0.1f;
				component.UpdateZDepth();
			}
		}
		while (m_state == UnearthState.Unearth)
		{
			yield return null;
		}
		for (int j = 0; j < dirtObjs.Count; j++)
		{
			SpawnManager.Despawn(dirtObjs[j]);
		}
	}

	private IEnumerator PuffCR()
	{
		Vector2 minPos = base.specRigidbody.UnitBottomLeft + dustOffset;
		Vector2 maxPos = base.specRigidbody.UnitBottomLeft + dustOffset + dustDimensions;
		float intraTimer = 0f;
		while (m_state == UnearthState.Unearth)
		{
			for (; intraTimer <= 0f; intraTimer += dustMidDelay)
			{
				GameObject prefab = BraveUtility.RandomElement(dustVfx);
				Vector2 vector = BraveUtility.RandomVector2(minPos, maxPos);
				GameObject gameObject = SpawnManager.SpawnVFX(prefab, vector, Quaternion.identity);
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				if ((bool)component)
				{
					base.sprite.AttachRenderer(component);
					component.HeightOffGround = 0.1f;
					component.UpdateZDepth();
				}
			}
			yield return null;
			intraTimer -= BraveTime.DeltaTime;
		}
	}
}
