using System;
using System.Collections;
using UnityEngine;

public class DebrisMelter : BraveBehaviour
{
	public float delay;

	public float meltTime;

	public bool doesGoop;

	[ShowInInspectorIf("doesGoop", false)]
	public GoopDefinition goop;

	[ShowInInspectorIf("doesGoop", false)]
	public float goopRadius = 1f;

	public void Start()
	{
		DebrisObject debrisObject = base.debris;
		debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, new Action<DebrisObject>(OnGrounded));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnGrounded(DebrisObject debrisObject)
	{
		StartCoroutine(DoMeltCR());
	}

	private IEnumerator DoMeltCR()
	{
		yield return new WaitForSeconds(delay);
		if (doesGoop)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goop).TimedAddGoopCircle(base.sprite.WorldCenter, goopRadius, meltTime);
		}
		for (float timer = meltTime; timer > 0f; timer -= BraveTime.DeltaTime)
		{
			base.transform.localScale = Vector3.one * (timer / meltTime);
			yield return null;
		}
		SpawnManager.Despawn(base.gameObject);
	}
}
