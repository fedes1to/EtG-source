using System.Collections;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/Spell Slot")]
[ExecuteInEditMode]
public class SpellSlot : MonoBehaviour
{
	[SerializeField]
	protected string spellName = string.Empty;

	[SerializeField]
	protected int slotNumber;

	[SerializeField]
	protected bool isActionSlot;

	private bool isSpellActive;

	public bool IsActionSlot
	{
		get
		{
			return isActionSlot;
		}
		set
		{
			isActionSlot = value;
			refresh();
		}
	}

	public string Spell
	{
		get
		{
			return spellName;
		}
		set
		{
			spellName = value;
			refresh();
		}
	}

	public int SlotNumber
	{
		get
		{
			return slotNumber;
		}
		set
		{
			slotNumber = value;
			refresh();
		}
	}

	private void OnEnable()
	{
		refresh();
	}

	private void Start()
	{
		refresh();
	}

	private void Update()
	{
		if (IsActionSlot && !string.IsNullOrEmpty(Spell) && Input.GetKeyDown((KeyCode)(slotNumber + 48)))
		{
			castSpell();
		}
	}

	public void onSpellActivated(SpellDefinition spell)
	{
		if (!(spell.Name != Spell))
		{
			StartCoroutine(showCooldown());
		}
	}

	private void OnDoubleClick()
	{
		if (!isSpellActive && !string.IsNullOrEmpty(Spell))
		{
			castSpell();
		}
	}

	private void OnDragStart(dfControl source, dfDragEventArgs args)
	{
		if (!allowDrag(args))
		{
			return;
		}
		if (string.IsNullOrEmpty(Spell))
		{
			args.State = dfDragDropState.Denied;
		}
		else
		{
			dfSprite dfSprite2 = GetComponent<dfControl>().Find("Icon") as dfSprite;
			Ray ray = dfSprite2.GetCamera().ScreenPointToRay(Input.mousePosition);
			Vector2 position = Vector2.zero;
			if (!dfSprite2.GetHitPosition(ray, out position))
			{
				return;
			}
			ActionbarsDragCursor.Show(dfSprite2, Input.mousePosition, position);
			if (IsActionSlot)
			{
				dfSprite2.SpriteName = string.Empty;
			}
			args.State = dfDragDropState.Dragging;
			args.Data = this;
		}
		args.Use();
	}

	private void OnDragEnd(dfControl source, dfDragEventArgs args)
	{
		ActionbarsDragCursor.Hide();
		if (isActionSlot)
		{
			if (args.State == dfDragDropState.CancelledNoTarget)
			{
				Spell = string.Empty;
			}
			refresh();
		}
	}

	private void OnDragDrop(dfControl source, dfDragEventArgs args)
	{
		if (allowDrop(args))
		{
			args.State = dfDragDropState.Dropped;
			SpellSlot spellSlot = args.Data as SpellSlot;
			string spell = spellName;
			Spell = spellSlot.Spell;
			if (spellSlot.IsActionSlot)
			{
				spellSlot.Spell = spell;
			}
		}
		else
		{
			args.State = dfDragDropState.Denied;
		}
		args.Use();
	}

	private bool allowDrag(dfDragEventArgs args)
	{
		return !isSpellActive && !string.IsNullOrEmpty(spellName);
	}

	private bool allowDrop(dfDragEventArgs args)
	{
		if (isSpellActive)
		{
			return false;
		}
		SpellSlot spellSlot = args.Data as SpellSlot;
		return spellSlot != null && IsActionSlot;
	}

	private IEnumerator showCooldown()
	{
		isSpellActive = true;
		SpellDefinition assignedSpell = SpellDefinition.FindByName(Spell);
		dfSprite sprite = GetComponent<dfControl>().Find("CoolDown") as dfSprite;
		sprite.IsVisible = true;
		float startTime = Time.realtimeSinceStartup;
		float endTime = startTime + assignedSpell.Recharge;
		while (Time.realtimeSinceStartup < endTime)
		{
			float elapsed = Time.realtimeSinceStartup - startTime;
			float lerp = (sprite.FillAmount = 1f - elapsed / assignedSpell.Recharge);
			yield return null;
		}
		sprite.FillAmount = 1f;
		sprite.IsVisible = false;
		isSpellActive = false;
	}

	private void castSpell()
	{
		ActionBarViewModel actionBarViewModel = Object.FindObjectsOfType(typeof(ActionBarViewModel)).FirstOrDefault() as ActionBarViewModel;
		if (actionBarViewModel != null)
		{
			actionBarViewModel.CastSpell(Spell);
		}
	}

	private void refresh()
	{
		SpellDefinition spellDefinition = SpellDefinition.FindByName(Spell);
		dfSprite dfSprite2 = GetComponent<dfControl>().Find<dfSprite>("Icon");
		dfSprite2.SpriteName = ((spellDefinition == null) ? string.Empty : spellDefinition.Icon);
		dfButton componentInChildren = GetComponentInChildren<dfButton>();
		componentInChildren.IsVisible = IsActionSlot;
		componentInChildren.Text = slotNumber.ToString();
	}
}
