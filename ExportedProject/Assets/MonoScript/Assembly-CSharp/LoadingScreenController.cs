using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
	public tk2dBaseSprite DEBUG_SPRITE;

	public dfSprite ItemBowlSprite;

	public dfSprite ItemDescriptionBox;

	public dfLabel ItemNameLabel;

	public dfLabel ItemDescriptionLabel;

	public tk2dBaseSprite ItemSprite;

	private void Start()
	{
		ItemDescriptionLabel.SizeChanged += OnDescriptionLabelSizeChanged;
		ItemSprite.ignoresTiltworldDepth = true;
	}

	private void Update()
	{
	}

	private void OnDescriptionLabelSizeChanged(dfControl control, Vector2 value)
	{
		if (control == ItemDescriptionLabel)
		{
			Vector2 vector = ItemDescriptionLabel.Font.ObtainRenderer().MeasureString(ItemDescriptionLabel.Text);
			float num = (float)Mathf.CeilToInt(vector.x / ItemDescriptionLabel.Size.x) * vector.y;
			ItemDescriptionBox.Size = new Vector2(ItemDescriptionBox.Size.x, num + 66f);
			Vector2 vector2 = ItemNameLabel.Font.ObtainRenderer().MeasureString(ItemNameLabel.Text);
			ItemDescriptionBox.Size = new Vector2(Mathf.Max(vector2.x, ItemDescriptionBox.Size.x), ItemDescriptionBox.Size.y);
		}
	}

	public void ChangeToNewItem(tk2dBaseSprite sourceSprite, JournalEntry entry)
	{
	}
}
