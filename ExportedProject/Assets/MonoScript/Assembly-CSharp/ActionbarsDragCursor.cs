using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/Drag Cursor")]
public class ActionbarsDragCursor : MonoBehaviour
{
	private static dfSprite _sprite;

	private static Vector2 _cursorOffset;

	public void Start()
	{
		_sprite = GetComponent<dfSprite>();
		_sprite.Hide();
		_sprite.IsInteractive = false;
		_sprite.IsEnabled = false;
	}

	public void Update()
	{
		if (_sprite.IsVisible)
		{
			setPosition(Input.mousePosition);
		}
	}

	public static void Show(dfSprite sprite, Vector2 position, Vector2 offset)
	{
		_cursorOffset = offset;
		setPosition(position);
		_sprite.Size = sprite.Size;
		_sprite.Atlas = sprite.Atlas;
		_sprite.SpriteName = sprite.SpriteName;
		_sprite.IsVisible = true;
		_sprite.BringToFront();
	}

	public static void Hide()
	{
		_sprite.IsVisible = false;
	}

	private static void setPosition(Vector2 position)
	{
		position = _sprite.GetManager().ScreenToGui(position);
		_sprite.RelativePosition = position - _cursorOffset;
	}
}
