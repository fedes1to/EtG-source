using System.Collections;

public class FoyerCoatRackController : BraveBehaviour
{
	private IEnumerator Start()
	{
		while (Foyer.DoIntroSequence || Foyer.DoMainMenu)
		{
			yield return null;
		}
		GetComponent<tk2dBaseSprite>().HeightOffGround = -1f;
		GetComponent<tk2dBaseSprite>().UpdateZDepth();
		if (0 == 0)
		{
			base.specRigidbody.enabled = false;
			base.gameObject.SetActive(false);
		}
	}
}
