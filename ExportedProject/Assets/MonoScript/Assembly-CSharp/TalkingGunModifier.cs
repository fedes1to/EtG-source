using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class TalkingGunModifier : MonoBehaviour, IGunInheritable
{
	public Transform talkPoint;

	public int roomsToRankUp = 10;

	public float ChanceToGainFriendship = 0.5f;

	private Gun m_gun;

	private int m_friendship;

	private int m_enmityCounter;

	private int m_begrudgingCounter;

	private int m_friendCounter;

	private PlayerController m_owner;

	private float m_destroyTimer;

	private void Start()
	{
		m_gun = GetComponent<Gun>();
		m_gun.AddAdditionalFlipTransform(talkPoint);
		Gun gun = m_gun;
		gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, new Action<Projectile>(PostprocessFriendship));
		Gun gun2 = m_gun;
		gun2.OnInitializedWithOwner = (Action<GameActor>)Delegate.Combine(gun2.OnInitializedWithOwner, new Action<GameActor>(OnGunReinitialized));
		if (m_gun.CurrentOwner != null)
		{
			OnGunReinitialized(m_gun.CurrentOwner);
		}
		Gun gun3 = m_gun;
		gun3.OnDropped = (Action)Delegate.Combine(gun3.OnDropped, new Action(HandleDropped));
	}

	private void OnGunReinitialized(GameActor newOwner)
	{
		m_owner = m_gun.CurrentOwner as PlayerController;
		m_owner.OnRoomClearEvent += HandleRoomCleared;
	}

	private void HandleDropped()
	{
		if ((bool)m_owner)
		{
			m_owner.OnRoomClearEvent -= HandleRoomCleared;
		}
		m_owner = null;
	}

	private void PostprocessFriendship(Projectile obj)
	{
		BounceProjModifier component = obj.GetComponent<BounceProjModifier>();
		PierceProjModifier component2 = obj.GetComponent<PierceProjModifier>();
		if (m_friendship < roomsToRankUp)
		{
			return;
		}
		if (m_friendship < roomsToRankUp * 2)
		{
			obj.baseData.damage += 3f;
			obj.baseData.speed += 6f;
			if ((bool)component)
			{
				obj.GetComponent<BounceProjModifier>().numberOfBounces += 2;
			}
			if ((bool)component2)
			{
				obj.GetComponent<PierceProjModifier>().penetration += 3;
			}
		}
		else
		{
			obj.baseData.damage += 6f;
			obj.baseData.speed += 6f;
			if ((bool)component2)
			{
				obj.GetComponent<PierceProjModifier>().BeastModeLevel = PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE;
			}
			HomingModifier homingModifier = obj.gameObject.AddComponent<HomingModifier>();
			homingModifier.HomingRadius = 8f;
			homingModifier.AngularVelocity = 360f;
		}
	}

	private void ClearTextBoxForReal()
	{
		TextBoxManager.ClearTextBox(talkPoint);
		if (!talkPoint || talkPoint.childCount <= 0)
		{
			return;
		}
		for (int num = talkPoint.childCount - 1; num >= 0; num--)
		{
			Transform child = talkPoint.GetChild(num);
			if ((bool)child)
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
	}

	private void Update()
	{
		if ((bool)m_gun && (bool)m_gun.sprite)
		{
			talkPoint.transform.localPosition = new Vector3(0.875f, (!m_gun.sprite.FlipY) ? 1.3125f : (-1.3125f), 0f);
			if ((bool)m_owner && m_owner.CurrentRoom != null && m_owner.CurrentRoom.IsSealed && m_owner.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All) && talkPoint.childCount > 0)
			{
				if (m_destroyTimer < 0.25f)
				{
					m_destroyTimer += BraveTime.DeltaTime;
				}
				else
				{
					ClearTextBoxForReal();
				}
			}
			else
			{
				m_destroyTimer = 0f;
			}
		}
		talkPoint.rotation = Quaternion.identity;
	}

	private IEnumerator HandleDelayedTalk(PlayerController obj)
	{
		yield return new WaitForSeconds(1f);
		if (!obj.IsInCombat && base.gameObject.activeSelf)
		{
			if (m_friendship < roomsToRankUp)
			{
				DoAmbientTalk(talkPoint, Vector3.zero, "#MASKGUN_ROOMCLEAR_ENMITY", 4f, m_enmityCounter);
				m_enmityCounter++;
			}
			else if (m_friendship < roomsToRankUp * 2)
			{
				DoAmbientTalk(talkPoint, Vector3.zero, "#MASKGUN_ROOMCLEAR_BEGRUDGING", 4f, m_begrudgingCounter);
				m_begrudgingCounter++;
			}
			else
			{
				DoAmbientTalk(talkPoint, Vector3.zero, "#MASKGUN_ROOMCLEAR_FRIENDS", 4f, m_friendCounter);
				m_friendCounter++;
			}
		}
	}

	private void HandleRoomCleared(PlayerController obj)
	{
		if ((bool)this && base.gameObject.activeSelf && m_gun.CurrentOwner != null && UnityEngine.Random.value < ChanceToGainFriendship)
		{
			obj.StartCoroutine(HandleDelayedTalk(obj));
			m_friendship++;
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_owner)
		{
			m_owner.OnRoomClearEvent -= HandleRoomCleared;
		}
	}

	private void OnDisable()
	{
		ClearTextBoxForReal();
	}

	public void DoAmbientTalk(Transform baseTransform, Vector3 offset, string stringKey, float duration, int index)
	{
		TextBoxManager.ShowTextBox(baseTransform.position + offset, baseTransform, duration, StringTableManager.GetStringSequential(stringKey, ref index, true), string.Empty, false);
	}

	public void InheritData(Gun sourceGun)
	{
		TalkingGunModifier component = sourceGun.GetComponent<TalkingGunModifier>();
		if ((bool)component)
		{
			m_friendship = component.m_friendship;
			m_enmityCounter = component.m_enmityCounter;
			m_begrudgingCounter = component.m_begrudgingCounter;
			m_friendCounter = component.m_friendCounter;
		}
	}

	public void MidGameSerialize(List<object> data, int dataIndex)
	{
		data.Add(m_friendship);
		data.Add(m_enmityCounter);
		data.Add(m_begrudgingCounter);
		data.Add(m_friendCounter);
	}

	public void MidGameDeserialize(List<object> data, ref int dataIndex)
	{
		m_friendship = (int)data[dataIndex];
		m_enmityCounter = (int)data[dataIndex + 1];
		m_begrudgingCounter = (int)data[dataIndex + 2];
		m_friendCounter = (int)data[dataIndex + 3];
		dataIndex += 4;
	}
}
