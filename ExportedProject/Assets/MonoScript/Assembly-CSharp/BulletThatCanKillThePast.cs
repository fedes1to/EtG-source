using UnityEngine;

public class BulletThatCanKillThePast : PassiveItem
{
	private void Start()
	{
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
		if (!m_pickedUp)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		}
		Shader.SetGlobalFloat("_MapActive", 0f);
	}

	protected override void Update()
	{
		base.Update();
		if (!m_pickedUp && base.gameObject.layer != LayerMask.NameToLayer("Unpixelated"))
		{
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		SimpleSpriteRotator[] componentsInChildren = GetComponentsInChildren<SimpleSpriteRotator>();
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
		GameManager.Instance.PrimaryPlayer.PastAccessible = true;
		Shader.SetGlobalFloat("_MapActive", 1f);
		base.Pickup(player);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<BulletThatCanKillThePast>().m_pickedUpThisRun = true;
		debrisObject.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
