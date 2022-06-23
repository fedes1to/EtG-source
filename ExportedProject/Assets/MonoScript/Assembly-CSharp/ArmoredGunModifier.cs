using UnityEngine;

public class ArmoredGunModifier : MonoBehaviour
{
	[PickupIdentifier]
	public int ArmoredId = -1;

	[PickupIdentifier]
	public int UnarmoredId = -1;

	[CheckAnimation(null)]
	public string ArmorUpAnimation;

	[CheckAnimation(null)]
	public string ArmorLostAnimation;

	private Gun m_gun;

	private bool m_armored = true;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		if (ArmoredId < 0)
		{
			ArmoredId = PickupObjectDatabase.GetById(UnarmoredId).GetComponent<ArmoredGunModifier>().ArmoredId;
		}
		if (UnarmoredId < 0)
		{
			UnarmoredId = PickupObjectDatabase.GetById(ArmoredId).GetComponent<ArmoredGunModifier>().UnarmoredId;
		}
	}

	private void Update()
	{
		if ((bool)m_gun && !m_gun.CurrentOwner)
		{
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			if ((bool)bestActivePlayer)
			{
				if ((bool)bestActivePlayer.healthHaver && bestActivePlayer.healthHaver.Armor > 0f)
				{
					Gun gun = PickupObjectDatabase.GetById(ArmoredId) as Gun;
					m_gun.sprite.SetSprite(gun.sprite.spriteId);
				}
				else
				{
					Gun gun2 = PickupObjectDatabase.GetById(UnarmoredId) as Gun;
					m_gun.sprite.SetSprite(gun2.sprite.spriteId);
				}
			}
		}
		else if ((bool)m_gun && (bool)m_gun.CurrentOwner && (bool)m_gun.CurrentOwner.healthHaver)
		{
			float num = m_gun.CurrentOwner.healthHaver.Armor;
			if (m_gun.OwnerHasSynergy(CustomSynergyType.NANOARMOR))
			{
				num = 20f;
			}
			if (m_armored && num <= 0f)
			{
				BecomeUnarmored();
			}
			else if (!m_armored && num > 0f)
			{
				BecomeArmored();
			}
		}
	}

	private void BecomeArmored()
	{
		if (!m_armored)
		{
			m_armored = true;
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(ArmoredId) as Gun);
			m_gun.spriteAnimator.Play(ArmorUpAnimation);
		}
	}

	private void BecomeUnarmored()
	{
		if (m_armored)
		{
			m_armored = false;
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(UnarmoredId) as Gun);
			m_gun.spriteAnimator.Play(ArmorLostAnimation);
		}
	}
}
