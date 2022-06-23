using System;
using UnityEngine;

public class DFMaxScaler : MonoBehaviour
{
	private dfControl m_control;

	private void Start()
	{
		m_control = GetComponent<dfControl>();
		dfControl control = m_control;
		control.ResolutionChangedPostLayout = (Action<dfControl, Vector3, Vector3>)Delegate.Combine(control.ResolutionChangedPostLayout, new Action<dfControl, Vector3, Vector3>(m_control_SizeChanged));
		m_control_SizeChanged(m_control, Vector3.zero, Vector3.zero);
	}

	private void m_control_SizeChanged(dfControl control, Vector3 pre, Vector3 post)
	{
		if (!(control is dfSprite))
		{
			return;
		}
		dfSprite dfSprite2 = control as dfSprite;
		dfGUIManager manager = control.GetManager();
		Vector2 screenSize = manager.GetScreenSize();
		Vector2 sizeInPixels = dfSprite2.SpriteInfo.sizeInPixels;
		float num = screenSize.x / screenSize.y;
		float num2 = sizeInPixels.x / sizeInPixels.y;
		dfSprite2.Anchor = dfAnchorStyle.None;
		if (num > num2)
		{
			Vector2 vector = new Vector2(screenSize.y * num2, screenSize.y);
			if (dfSprite2.Size != vector)
			{
				dfSprite2.Size = vector;
				dfSprite2.RelativePosition = new Vector3((screenSize.x - vector.x) / 2f, 0f, dfSprite2.RelativePosition.z);
			}
		}
		else
		{
			Vector2 vector2 = new Vector2(screenSize.x, screenSize.x / num2);
			if (dfSprite2.Size != vector2)
			{
				dfSprite2.Size = vector2;
				dfSprite2.RelativePosition = new Vector3(0f, (screenSize.y - vector2.y) / 2f, dfSprite2.RelativePosition.z);
			}
		}
	}
}
