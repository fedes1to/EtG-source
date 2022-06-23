using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class HangingObjectController : DungeonPlaceableBehaviour, IPlayerInteractable, IEventTriggerable, IPlaceConfigurable
{
	public tk2dSprite objectSprite;

	public tk2dSpriteAnimator AdditionalChainDownAnimator;

	public bool destroyOnFinish = true;

	public GameObject[] additionalDestroyObjects;

	public bool hasSubSprites;

	public tk2dSprite[] subSprites;

	public float startingHeight = 5f;

	public bool DoExplosion = true;

	public ExplosionData explosionData;

	public Vector3 explosionOffset;

	public GameObject replacementRangeEffect;

	public GameObject triggerObjectPrefab;

	public bool DoesTriggerShake;

	public ScreenShakeSettings TriggerShake;

	public SpeculativeRigidbody EnableRigidbodyPostFall;

	public bool MakeMajorBreakableAfterwards;

	protected Transform m_objectTransform;

	protected Vector3 m_cachedStartingPosition;

	protected float m_currentHeight;

	protected bool m_hasFallen;

	protected RoomHandler m_room;

	protected MinorBreakable TriggerObjectBreakable;

	protected tk2dSpriteAnimator TriggerObjectAnimator;

	protected tk2dSpriteAnimator TriggerChainAnimator;

	private bool m_subsidiary;

	private float GRAVITY_ACCELERATION = -10f;

	private int subspritesFalling;

	private HashSet<float> m_usedDepths;

	private void Start()
	{
		m_currentHeight = startingHeight;
		m_objectTransform = objectSprite.transform;
		m_cachedStartingPosition = m_objectTransform.position;
		objectSprite.HeightOffGround = m_currentHeight + 1f;
		m_objectTransform.position = m_cachedStartingPosition + new Vector3(0f, m_currentHeight, 0f);
		objectSprite.UpdateZDepth();
		if (!(triggerObjectPrefab != null))
		{
			return;
		}
		RoomEventTriggerArea eventTriggerAreaFromObject = m_room.GetEventTriggerAreaFromObject(this);
		if (eventTriggerAreaFromObject == null)
		{
			return;
		}
		if (eventTriggerAreaFromObject.tempDataObject != null)
		{
			m_subsidiary = true;
			GameObject tempDataObject = eventTriggerAreaFromObject.tempDataObject;
			TriggerObjectBreakable = tempDataObject.GetComponentInChildren<MinorBreakable>();
			TriggerObjectBreakable.OnlyBreaksOnScreen = true;
			MinorBreakable triggerObjectBreakable = TriggerObjectBreakable;
			triggerObjectBreakable.OnBreak = (Action)Delegate.Combine(triggerObjectBreakable.OnBreak, new Action(TriggerFallBroken));
			tk2dSpriteAnimator component = TriggerObjectBreakable.GetComponent<tk2dSpriteAnimator>();
			component.OnPlayAnimationCalled = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(component.OnPlayAnimationCalled, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(SubTriggerAnim));
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(triggerObjectPrefab, eventTriggerAreaFromObject.initialPosition.ToVector3(eventTriggerAreaFromObject.initialPosition.y), Quaternion.identity);
		TriggerObjectBreakable = gameObject.GetComponentInChildren<MinorBreakable>();
		TriggerObjectBreakable.OnlyBreaksOnScreen = true;
		MinorBreakable triggerObjectBreakable2 = TriggerObjectBreakable;
		triggerObjectBreakable2.OnBreak = (Action)Delegate.Combine(triggerObjectBreakable2.OnBreak, new Action(TriggerFallBroken));
		TriggerObjectAnimator = TriggerObjectBreakable.GetComponent<tk2dSpriteAnimator>();
		if (TriggerObjectBreakable.transform.childCount > 0)
		{
			TriggerChainAnimator = TriggerObjectBreakable.transform.GetChild(0).GetComponent<tk2dSpriteAnimator>();
		}
		eventTriggerAreaFromObject.tempDataObject = gameObject;
		if ((bool)TriggerObjectBreakable && (bool)TriggerObjectBreakable.sprite)
		{
			TriggerObjectBreakable.sprite.IsPerpendicular = true;
			TriggerObjectBreakable.sprite.UpdateZDepth();
		}
	}

	private void SubTriggerAnim(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		m_subsidiary = true;
		Interact(GameManager.Instance.BestActivePlayer);
	}

	public void Trigger(int index)
	{
		if (triggerObjectPrefab == null)
		{
			TriggerFallBroken();
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON && room.RoomVisualSubtype == 6)
		{
			room.RoomVisualSubtype = ((!(UnityEngine.Random.value > 0.5f)) ? 3 : 0);
			for (int i = 0; i < room.Cells.Count; i++)
			{
				room.UpdateCellVisualData(room.Cells[i].x, room.Cells[i].y);
			}
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (m_hasFallen)
		{
			return 1000f;
		}
		if (TriggerObjectAnimator == null)
		{
			return 1000f;
		}
		tk2dBaseSprite tk2dBaseSprite2 = TriggerObjectAnimator.Sprite;
		return Vector2.Distance(point, tk2dBaseSprite2.WorldCenter) / 2f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(TriggerObjectAnimator.Sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(TriggerObjectAnimator.Sprite);
		}
	}

	private IEnumerator HandleSubSpriteFall(tk2dSprite targetSprite, float adjustedStartHeight)
	{
		if (m_usedDepths == null)
		{
			m_usedDepths = new HashSet<float>();
		}
		subspritesFalling++;
		targetSprite.transform.parent = targetSprite.transform.parent.parent;
		float curHeight2 = m_currentHeight + adjustedStartHeight;
		float curVel = UnityEngine.Random.Range(0, -3);
		float cachedStartY = targetSprite.transform.localPosition.y - m_currentHeight;
		Vector3 startPos = targetSprite.transform.position.WithY(targetSprite.transform.position.y - m_currentHeight);
		while (curHeight2 > 0f)
		{
			curVel += GRAVITY_ACCELERATION * BraveTime.DeltaTime;
			curHeight2 += curVel * BraveTime.DeltaTime;
			curHeight2 = (targetSprite.HeightOffGround = Mathf.Max(0f, curHeight2));
			targetSprite.transform.position = startPos + new Vector3(0f, curHeight2, 0f);
			targetSprite.UpdateZDepth();
			yield return null;
			if (!this)
			{
				yield break;
			}
		}
		float finalTargetHeight;
		for (finalTargetHeight = -1.5f + cachedStartY; m_usedDepths.Contains(targetSprite.transform.position.y + targetSprite.transform.position.y - finalTargetHeight); finalTargetHeight += 0.0625f)
		{
		}
		m_usedDepths.Add(targetSprite.transform.position.y + targetSprite.transform.position.y - finalTargetHeight);
		targetSprite.HeightOffGround = finalTargetHeight;
		targetSprite.UpdateZDepth();
		subspritesFalling--;
	}

	private IEnumerator Fall()
	{
		if (DoesTriggerShake && !m_subsidiary)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(TriggerShake, null);
			AkSoundEngine.PostEvent("Play_WPN_grenade_blast_01", base.gameObject);
		}
		if (!objectSprite.gameObject.activeSelf)
		{
			objectSprite.gameObject.SetActive(true);
		}
		if ((bool)objectSprite && (bool)objectSprite.GetComponent<MinorBreakable>())
		{
			objectSprite.GetComponent<MinorBreakable>().enabled = true;
		}
		if (TriggerObjectAnimator != null)
		{
			TriggerObjectAnimator.Play();
		}
		if (TriggerChainAnimator != null)
		{
			TriggerChainAnimator.PlayAndDestroyObject(string.Empty);
		}
		if (AdditionalChainDownAnimator != null)
		{
			AdditionalChainDownAnimator.Play();
		}
		RelatedObjects related = ((!(TriggerChainAnimator != null)) ? null : TriggerChainAnimator.GetComponent<RelatedObjects>());
		if (related != null)
		{
			for (int i = 0; i < related.relatedObjects.Length; i++)
			{
				related.relatedObjects[i].SetActive(true);
			}
		}
		float m_currentVelocity = 0f;
		if (hasSubSprites)
		{
			for (int j = 0; j < subSprites.Length; j++)
			{
				StartCoroutine(HandleSubSpriteFall(subSprites[j], subSprites[j].transform.localPosition.y + 2f + UnityEngine.Random.Range(-1f, 1.5f)));
			}
		}
		bool hasDisabledParticles = false;
		while (m_currentHeight > 0f)
		{
			m_currentVelocity += GRAVITY_ACCELERATION * BraveTime.DeltaTime;
			m_currentHeight += m_currentVelocity * BraveTime.DeltaTime;
			m_currentHeight = Mathf.Max(0f, m_currentHeight);
			objectSprite.HeightOffGround = m_currentHeight;
			if ((bool)m_objectTransform)
			{
				m_objectTransform.position = m_cachedStartingPosition + new Vector3(0f, m_currentHeight, 0f);
			}
			if (m_currentHeight < 5f && !hasDisabledParticles)
			{
				hasDisabledParticles = true;
				for (int k = 0; k < additionalDestroyObjects.Length; k++)
				{
					if ((bool)additionalDestroyObjects[k] && (bool)additionalDestroyObjects[k].GetComponent<ParticleSystem>())
					{
						BraveUtility.EnableEmission(additionalDestroyObjects[k].GetComponent<ParticleSystem>(), false);
						additionalDestroyObjects[k] = null;
					}
				}
			}
			if ((bool)objectSprite)
			{
				objectSprite.UpdateZDepth();
			}
			yield return null;
			if (!this)
			{
				yield break;
			}
		}
		if (hasSubSprites)
		{
			AkSoundEngine.PostEvent("Play_OBJ_boulder_crash_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		if (!objectSprite)
		{
			yield break;
		}
		MinorBreakable breakable = objectSprite.GetComponent<MinorBreakable>();
		if (breakable != null)
		{
			breakable.Break();
		}
		if (DoExplosion)
		{
			if (m_subsidiary)
			{
				explosionData.doScreenShake = false;
			}
			explosionData.overrideRangeIndicatorEffect = replacementRangeEffect;
			Exploder.Explode(objectSprite.WorldCenter.ToVector3ZUp() + explosionOffset, explosionData, Vector2.zero, null, true);
		}
		while (subspritesFalling > 0)
		{
			yield return null;
			if (!this)
			{
				yield break;
			}
		}
		if (destroyOnFinish)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if ((bool)objectSprite)
		{
			objectSprite.HeightOffGround = ((!hasSubSprites) ? (-1.5f) : (-1.625f));
			objectSprite.UpdateZDepth();
		}
		for (int l = 0; l < additionalDestroyObjects.Length; l++)
		{
			if ((bool)additionalDestroyObjects[l])
			{
				UnityEngine.Object.Destroy(additionalDestroyObjects[l]);
			}
		}
		if ((bool)EnableRigidbodyPostFall)
		{
			EnableRigidbodyPostFall.enabled = true;
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(EnableRigidbodyPostFall);
			EnableRigidbodyPostFall.FlagCellsOccupied();
		}
		yield return new WaitForSeconds(0.25f);
		if ((bool)this && MakeMajorBreakableAfterwards)
		{
			MajorBreakable component = objectSprite.GetComponent<MajorBreakable>();
			if ((bool)EnableRigidbodyPostFall)
			{
				EnableRigidbodyPostFall.majorBreakable = component;
			}
			component.enabled = true;
			component.OnBreak = (Action)Delegate.Combine(component.OnBreak, new Action(HandleSubspritesLaunch));
		}
	}

	private void HandleSubspritesLaunch()
	{
		if ((bool)EnableRigidbodyPostFall)
		{
			EnableRigidbodyPostFall.enabled = false;
		}
		for (int i = 0; i < subSprites.Length; i++)
		{
			if ((bool)subSprites[i])
			{
				MinorBreakable component = subSprites[i].GetComponent<MinorBreakable>();
				if ((bool)component)
				{
					component.enabled = true;
				}
				DebrisObject orAddComponent = subSprites[i].gameObject.GetOrAddComponent<DebrisObject>();
				AkSoundEngine.PostEvent("Play_OBJ_boulder_break_01", base.gameObject);
				orAddComponent.angularVelocity = 45f;
				orAddComponent.angularVelocityVariance = 20f;
				orAddComponent.decayOnBounce = 0.5f;
				orAddComponent.bounceCount = 1;
				orAddComponent.canRotate = true;
				orAddComponent.shouldUseSRBMotion = true;
				orAddComponent.AssignFinalWorldDepth(-0.5f);
				orAddComponent.sprite = subSprites[i];
				orAddComponent.Trigger((subSprites[i].WorldCenter - objectSprite.WorldCenter).ToVector3ZUp(0.5f) * UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(1, 2));
			}
		}
	}

	public void Interact(PlayerController interactor)
	{
		TriggerFallInteracted();
	}

	protected void TriggerFallInteracted()
	{
		if (m_hasFallen)
		{
			return;
		}
		m_hasFallen = true;
		if ((bool)TriggerObjectBreakable)
		{
			if ((bool)TriggerObjectBreakable.specRigidbody)
			{
				TriggerObjectBreakable.specRigidbody.enabled = false;
			}
			TriggerObjectBreakable.CleanupCallbacks();
			UnityEngine.Object.Destroy(TriggerObjectBreakable);
		}
		m_room.DeregisterInteractable(this);
		StartCoroutine(Fall());
	}

	protected void TriggerFallBroken()
	{
		if (!m_hasFallen)
		{
			m_hasFallen = true;
			m_room.DeregisterInteractable(this);
			StartCoroutine(Fall());
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
