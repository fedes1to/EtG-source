using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/Message Scroller")]
public class MessageDisplay : MonoBehaviour
{
	private class MessageInfo
	{
		public dfLabel label;

		public float startTime;
	}

	private const float TIME_BEFORE_FADE = 3f;

	private const float FADE_TIME = 2f;

	private List<MessageInfo> messages = new List<MessageInfo>();

	private dfLabel lblTemplate;

	public void AddMessage(string text)
	{
		if (!(lblTemplate == null))
		{
			for (int i = 0; i < messages.Count; i++)
			{
				dfLabel label = messages[i].label;
				label.RelativePosition += new Vector3(0f, 0f - label.Height);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(lblTemplate.gameObject);
			gameObject.transform.parent = base.transform;
			gameObject.transform.position = lblTemplate.transform.position;
			gameObject.name = "Message" + messages.Count;
			dfLabel component = gameObject.GetComponent<dfLabel>();
			component.Text = text;
			component.IsVisible = true;
			messages.Add(new MessageInfo
			{
				label = component,
				startTime = Time.realtimeSinceStartup
			});
		}
	}

	public void onSpellActivated(SpellDefinition spell)
	{
		AddMessage("You cast " + spell.Name);
	}

	private void OnClick(dfControl sender, dfMouseEventArgs args)
	{
		AddMessage("New test message added to the list at " + DateTime.Now);
		args.Use();
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		lblTemplate = GetComponentInChildren<dfLabel>();
		lblTemplate.IsVisible = false;
	}

	private void Update()
	{
		for (int num = messages.Count - 1; num >= 0; num--)
		{
			MessageInfo messageInfo = messages[num];
			float num2 = Time.realtimeSinceStartup - messageInfo.startTime;
			if (!(num2 < 3f))
			{
				if (num2 >= 5f)
				{
					messages.RemoveAt(num);
					UnityEngine.Object.Destroy(messageInfo.label.gameObject);
				}
				else
				{
					float opacity = 1f - (num2 - 3f) / 2f;
					messageInfo.label.Opacity = opacity;
				}
			}
		}
	}
}
