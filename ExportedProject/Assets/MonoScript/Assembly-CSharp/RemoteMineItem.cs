using UnityEngine;

public class RemoteMineItem : PlayerItem
{
	public GameObject objectToSpawn;

	public string detonatorSprite = "c4_transmitter_001";

	protected RemoteMineController m_extantEffect;

	protected int m_originalSprite;

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_mine_set_01", base.gameObject);
		GameObject gameObject = Object.Instantiate(objectToSpawn, user.specRigidbody.UnitCenter, Quaternion.identity);
		m_originalSprite = base.sprite.spriteId;
		base.sprite.SetSprite(detonatorSprite);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		m_extantEffect = gameObject.GetComponent<RemoteMineController>();
		if (component != null)
		{
			component.PlaceAtPositionByAnchor(user.specRigidbody.UnitCenter.ToVector3ZUp(component.transform.position.z), tk2dBaseSprite.Anchor.MiddleCenter);
		}
		m_isCurrentlyActive = true;
	}

	public override void Update()
	{
		if (m_extantEffect == null)
		{
			base.Update();
		}
		else if (TimeTubeCreditsController.IsTimeTubing)
		{
			Object.Destroy(m_extantEffect.gameObject);
			m_extantEffect = null;
		}
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (m_isCurrentlyActive)
		{
			DoActiveEffect(user);
		}
		base.OnPreDrop(user);
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		if (m_extantEffect != null)
		{
			m_extantEffect.Detonate();
			m_extantEffect = null;
		}
		base.sprite.SetSprite(m_originalSprite);
		m_isCurrentlyActive = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
