using UnityEngine;
using UnityEngine.UIElements;

public class InspectableCable : IInspectable
{
	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = null;
		return PtUUISettings.GetOrCreateSettings().CableInspectorUI;
	}
}
