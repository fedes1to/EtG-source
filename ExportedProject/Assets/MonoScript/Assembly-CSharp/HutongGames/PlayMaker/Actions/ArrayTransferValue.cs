using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Transfer a value from one array to another, basically a copy/cut paste action on steroids.")]
	[NoActionTargets]
	[ActionCategory(ActionCategory.Array)]
	public class ArrayTransferValue : FsmStateAction
	{
		public enum ArrayTransferType
		{
			Copy,
			Cut,
			nullify
		}

		public enum ArrayPasteType
		{
			AsFirstItem,
			AsLastItem,
			InsertAtSameIndex,
			ReplaceAtSameIndex
		}

		[RequiredField]
		[Tooltip("The Array Variable source.")]
		[UIHint(UIHint.Variable)]
		public FsmArray arraySource;

		[Tooltip("The Array Variable target.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmArray arrayTarget;

		[Tooltip("The index to transfer.")]
		[MatchFieldType("array")]
		public FsmInt indexToTransfer;

		[ObjectType(typeof(ArrayTransferType))]
		[ActionSection("Transfer Options")]
		public FsmEnum copyType;

		[ObjectType(typeof(ArrayPasteType))]
		public FsmEnum pasteType;

		[ActionSection("Result")]
		[Tooltip("Event sent if this array source does not contains that element (described below)")]
		public FsmEvent indexOutOfRange;

		public override void Reset()
		{
			arraySource = null;
			arrayTarget = null;
			indexToTransfer = null;
			copyType = ArrayTransferType.Copy;
			pasteType = ArrayPasteType.AsLastItem;
		}

		public override void OnEnter()
		{
			DoTransferValue();
			Finish();
		}

		private void DoTransferValue()
		{
			if (arraySource.IsNone || arrayTarget.IsNone)
			{
				return;
			}
			int value = indexToTransfer.Value;
			if (value < 0 || value >= arraySource.Length)
			{
				base.Fsm.Event(indexOutOfRange);
				return;
			}
			object obj = arraySource.Values[value];
			if ((ArrayTransferType)(object)copyType.Value == ArrayTransferType.Cut)
			{
				List<object> list = new List<object>(arraySource.Values);
				list.RemoveAt(value);
				arraySource.Values = list.ToArray();
			}
			else if ((ArrayTransferType)(object)copyType.Value == ArrayTransferType.nullify)
			{
				arraySource.Values.SetValue(null, value);
			}
			if ((ArrayPasteType)(object)pasteType.Value == ArrayPasteType.AsFirstItem)
			{
				List<object> list2 = new List<object>(arrayTarget.Values);
				list2.Insert(0, obj);
				arrayTarget.Values = list2.ToArray();
			}
			else if ((ArrayPasteType)(object)pasteType.Value == ArrayPasteType.AsLastItem)
			{
				arrayTarget.Resize(arrayTarget.Length + 1);
				arrayTarget.Set(arrayTarget.Length - 1, obj);
			}
			else if ((ArrayPasteType)(object)pasteType.Value == ArrayPasteType.InsertAtSameIndex)
			{
				if (value >= arrayTarget.Length)
				{
					base.Fsm.Event(indexOutOfRange);
				}
				List<object> list3 = new List<object>(arrayTarget.Values);
				list3.Insert(value, obj);
				arrayTarget.Values = list3.ToArray();
			}
			else if ((ArrayPasteType)(object)pasteType.Value == ArrayPasteType.ReplaceAtSameIndex)
			{
				if (value >= arrayTarget.Length)
				{
					base.Fsm.Event(indexOutOfRange);
				}
				else
				{
					arrayTarget.Set(value, obj);
				}
			}
		}
	}
}
