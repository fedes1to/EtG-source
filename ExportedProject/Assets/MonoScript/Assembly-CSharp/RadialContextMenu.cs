using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Menus/Radial Context Menu Helper")]
public class RadialContextMenu : MonoBehaviour
{
	public dfRadialMenu contextMenu;

	public void Start()
	{
		contextMenu.MenuClosed += delegate(dfRadialMenu menu)
		{
			menu.host.Hide();
		};
	}

	public void OnMouseDown(dfControl control, dfMouseEventArgs args)
	{
		if (!args.Used && args.Buttons == dfMouseButtons.Middle)
		{
			if (contextMenu.IsOpen)
			{
				contextMenu.Close();
				return;
			}
			args.Use();
			Vector2 hitPosition = control.GetHitPosition(args);
			dfControl host = contextMenu.host;
			host.RelativePosition = hitPosition - host.Size * 0.5f;
			host.BringToFront();
			host.Show();
			host.Focus();
			contextMenu.Open();
		}
	}
}
