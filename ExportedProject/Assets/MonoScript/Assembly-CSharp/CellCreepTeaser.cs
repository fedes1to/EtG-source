using Dungeonator;
using UnityEngine;

public class CellCreepTeaser : MonoBehaviour
{
	public tk2dSpriteAnimator bodySprite;

	public tk2dSprite shadowSprite;

	private bool isPlaying;

	public void Update()
	{
		if (!isPlaying)
		{
			if (!GameManager.Instance.IsPaused && !Dungeon.IsGenerating && !GameManager.Instance.IsLoadingLevel)
			{
				bodySprite.Play();
				isPlaying = true;
			}
			return;
		}
		float alpha = Mathf.InverseLerp(3.75f, 3.17f, bodySprite.ClipTimeSeconds);
		shadowSprite.color = shadowSprite.color.WithAlpha(alpha);
		if (!bodySprite.Playing)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
