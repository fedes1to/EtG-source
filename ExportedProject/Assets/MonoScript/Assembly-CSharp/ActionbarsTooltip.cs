using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/Tooltip")]
public class ActionbarsTooltip : MonoBehaviour
{
	private static ActionbarsTooltip _instance;

	private static dfPanel _panel;

	private static dfLabel _name;

	private static dfLabel _info;

	private static Vector2 _cursorOffset;

	public void Start()
	{
		_instance = this;
		_panel = GetComponent<dfPanel>();
		_name = _panel.Find<dfLabel>("lblName");
		_info = _panel.Find<dfLabel>("lblInfo");
		_panel.Hide();
		_panel.IsInteractive = false;
		_panel.IsEnabled = false;
	}

	public void Update()
	{
		if (_panel.IsVisible)
		{
			setPosition(Input.mousePosition);
		}
	}

	public static void Show(SpellDefinition spell)
	{
		if (spell == null)
		{
			Hide();
			return;
		}
		_name.Text = spell.Name;
		_info.Text = spell.Description;
		float num = _info.RelativePosition.y + _info.Size.y;
		_panel.Height = num;
		_cursorOffset = new Vector2(0f, num + 10f);
		_panel.Show();
		_panel.BringToFront();
		_instance.Update();
	}

	public static void Hide()
	{
		_panel.Hide();
		_panel.SendToBack();
	}

	private static void setPosition(Vector2 position)
	{
		position = _panel.GetManager().ScreenToGui(position);
		_panel.RelativePosition = position - _cursorOffset;
	}
}
