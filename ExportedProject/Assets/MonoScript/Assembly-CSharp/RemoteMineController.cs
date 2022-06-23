using System.Collections;
using UnityEngine;

public class RemoteMineController : BraveBehaviour
{
	public ExplosionData explosionData;

	[CheckAnimation(null)]
	public string explodeAnimName;

	public void Detonate()
	{
		if (!string.IsNullOrEmpty(explodeAnimName))
		{
			StartCoroutine(DelayDetonateFrame());
			return;
		}
		Exploder.Explode(base.transform.position, explosionData, Vector2.zero);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator DelayDetonateFrame()
	{
		base.spriteAnimator.Play(explodeAnimName);
		yield return new WaitForSeconds(0.05f);
		if (explosionData.damageToPlayer > 0f)
		{
			explosionData.damageToPlayer = 0f;
		}
		Exploder.Explode(base.sprite.WorldCenter.ToVector3ZUp(), explosionData, Vector2.zero);
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
