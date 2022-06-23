using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Quit On Click")]
public class QuitOnClick : MonoBehaviour
{
	private void OnClick()
	{
		Application.Quit();
	}
}
