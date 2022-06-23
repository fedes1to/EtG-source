using System.Collections;
using UnityEngine;

public class SpriteAnimatorStopper : MonoBehaviour
{
	public float duration = 10f;

	private tk2dSpriteAnimator animator;

	private IEnumerator Start()
	{
		animator = GetComponent<tk2dSpriteAnimator>();
		yield return new WaitForSeconds(duration);
		animator.Stop();
		Object.Destroy(this);
	}
}
