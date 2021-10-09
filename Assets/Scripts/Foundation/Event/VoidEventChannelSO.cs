using UnityEngine;
using UnityEngine.Events;
using UnityTemplateProjects.Foundation.Base;

namespace UnityTemplateProjects.Foundation.Event
{
	[CreateAssetMenu(menuName = "Events/Void Event Channel")]
	public class VoidEventChannelSO : DescriptionBaseSO
	{
		public UnityAction OnEventRaised;

		public void RaiseEvent()
		{
			if (OnEventRaised != null)
				OnEventRaised.Invoke();
		}
	}
}