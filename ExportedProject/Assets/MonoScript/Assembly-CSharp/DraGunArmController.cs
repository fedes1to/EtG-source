using System.Collections.Generic;
using UnityEngine;

public class DraGunArmController : BraveBehaviour
{
	public GameObject shoulder;

	public List<GameObject> balls;

	public GameObject hand;

	private int[] offsets = new int[6] { 0, -3, -5, -6, -5, -3 };

	private DraGunController m_body;

	private tk2dBaseSprite shoulderSprite;

	private TileSpriteClipper handSpriteClipper;

	private List<TileSpriteClipper> armSpriteClippers;

	public void Start()
	{
		m_body = base.transform.parent.GetComponent<DraGunController>();
		m_body.specRigidbody.Initialize();
		float unitBottom = m_body.specRigidbody.PrimaryPixelCollider.UnitBottom;
		armSpriteClippers = new List<TileSpriteClipper>(balls.Count);
		for (int i = 0; i < balls.Count; i++)
		{
			tk2dBaseSprite componentInChildren = balls[i].GetComponentInChildren<tk2dBaseSprite>();
			TileSpriteClipper orAddComponent = componentInChildren.gameObject.GetOrAddComponent<TileSpriteClipper>();
			orAddComponent.doOptimize = true;
			orAddComponent.updateEveryFrame = true;
			orAddComponent.clipMode = TileSpriteClipper.ClipMode.ClipBelowY;
			orAddComponent.clipY = unitBottom;
			armSpriteClippers.Add(orAddComponent);
		}
		tk2dBaseSprite componentInChildren2 = hand.GetComponentInChildren<tk2dBaseSprite>();
		handSpriteClipper = componentInChildren2.gameObject.GetOrAddComponent<TileSpriteClipper>();
		handSpriteClipper.doOptimize = true;
		handSpriteClipper.updateEveryFrame = true;
		handSpriteClipper.clipMode = TileSpriteClipper.ClipMode.ClipBelowY;
		handSpriteClipper.clipY = unitBottom;
		handSpriteClipper.enabled = false;
		shoulderSprite = shoulder.GetComponentInChildren<tk2dBaseSprite>();
		m_body.sprite.SpriteChanged += BodySpriteChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ClipArmSprites()
	{
		SetClipArmSprites(true);
	}

	public void UnclipArmSprites()
	{
		SetClipArmSprites(false);
	}

	public void SetClipArmSprites(bool clip)
	{
		for (int i = 0; i < armSpriteClippers.Count; i++)
		{
			armSpriteClippers[i].enabled = clip;
		}
	}

	public void ClipHandSprite()
	{
		SetClipHandSprite(true);
	}

	public void UnclipHandSprite()
	{
		SetClipHandSprite(false);
	}

	public void SetClipHandSprite(bool clip)
	{
		handSpriteClipper.enabled = clip;
	}

	private void BodySpriteChanged(tk2dBaseSprite obj)
	{
		if (m_body.spriteAnimator.CurrentClip != null)
		{
			float num = (float)m_body.spriteAnimator.CurrentFrame / (float)m_body.spriteAnimator.CurrentClip.frames.Length;
			int num2 = Mathf.Min(Mathf.FloorToInt(num * 6f), 5);
			float num3 = PhysicsEngine.PixelToUnit(offsets[num2]);
			shoulderSprite.transform.localPosition = shoulderSprite.transform.localPosition.WithY(num3);
			for (int i = 0; i < armSpriteClippers.Count; i++)
			{
				armSpriteClippers[i].transform.localPosition = shoulderSprite.transform.localPosition.WithY(Mathf.Lerp(num3, 0f, ((float)i + 1f) / ((float)armSpriteClippers.Count + 1f)));
			}
		}
	}
}
