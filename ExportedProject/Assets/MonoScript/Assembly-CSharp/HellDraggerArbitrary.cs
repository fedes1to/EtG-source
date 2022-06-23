using System.Collections;
using UnityEngine;

public class HellDraggerArbitrary : BraveBehaviour
{
	public GameObject HellDragVFX;

	private IEnumerator HandleGrabbyGrab(PlayerController grabbedPlayer)
	{
		yield return new WaitForSeconds(0.5f);
		grabbedPlayer.IsVisible = false;
	}

	private void GrabPlayer(PlayerController enteredPlayer)
	{
		GameObject gameObject = enteredPlayer.PlayEffectOnActor(HellDragVFX, new Vector3(0f, -0.25f, 0f), false);
		gameObject.transform.position = new Vector3((float)Mathf.RoundToInt(gameObject.transform.position.x + 0.1875f) - 0.1875f, (float)Mathf.RoundToInt(gameObject.transform.position.y - 0.375f) + 0.375f, gameObject.transform.position.z);
		StartCoroutine(HandleGrabbyGrab(enteredPlayer));
	}

	public void Do(PlayerController enteredPlayer)
	{
		if ((bool)enteredPlayer)
		{
			GrabPlayer(enteredPlayer);
		}
	}
}
