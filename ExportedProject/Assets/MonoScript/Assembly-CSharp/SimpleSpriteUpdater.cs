using UnityEngine;

public class SimpleSpriteUpdater : MonoBehaviour
{
	private void Start()
	{
		GetComponent<tk2dBaseSprite>().UpdateZDepth();
	}
}
