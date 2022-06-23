using System;
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Finds the Child of a GameObject by Name and/or Tag. Use this to find attach points etc. NOTE: This action will search recursively through all children and return the first match; To find a specific child use Find Child.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class GetChild : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The GameObject to search.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The name of the child to search for.")]
		public FsmString childName;

		[UIHint(UIHint.Tag)]
		[Tooltip("The Tag to search for. If Child Name is set, both name and Tag need to match.")]
		public FsmString withTag;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a GameObject variable.")]
		public FsmGameObject storeResult;

		public override void Reset()
		{
			gameObject = null;
			childName = string.Empty;
			withTag = "Untagged";
			storeResult = null;
		}

		public override void OnEnter()
		{
			storeResult.Value = DoGetChildByName(base.Fsm.GetOwnerDefaultTarget(gameObject), childName.Value, withTag.Value);
			Finish();
		}

		private static GameObject DoGetChildByName(GameObject root, string name, string tag)
		{
			if (root == null)
			{
				return null;
			}
			IEnumerator enumerator = root.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					if (!string.IsNullOrEmpty(name))
					{
						if (transform.name == name)
						{
							if (string.IsNullOrEmpty(tag))
							{
								return transform.gameObject;
							}
							if (transform.tag.Equals(tag))
							{
								return transform.gameObject;
							}
						}
					}
					else if (!string.IsNullOrEmpty(tag) && transform.tag == tag)
					{
						return transform.gameObject;
					}
					GameObject gameObject = DoGetChildByName(transform.gameObject, name, tag);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}

		public override string ErrorCheck()
		{
			if (string.IsNullOrEmpty(childName.Value) && string.IsNullOrEmpty(withTag.Value))
			{
				return "Specify Child Name, Tag, or both.";
			}
			return null;
		}
	}
}
