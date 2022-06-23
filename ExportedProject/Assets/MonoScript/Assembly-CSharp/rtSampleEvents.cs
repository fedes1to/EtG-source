using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Rich Text/Rich Text Events")]
public class rtSampleEvents : MonoBehaviour
{
	public void OnLinkClicked(dfRichTextLabel sender, dfMarkupTagAnchor tag)
	{
		string hRef = tag.HRef;
		if (hRef.ToLowerInvariant().StartsWith("http:"))
		{
			Application.OpenURL(hRef);
		}
	}
}
