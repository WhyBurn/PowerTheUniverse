using UnityEngine;
using UnityEngine.UIElements;

public class InspectableDemolish : IInspectable
{
	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = null;
		return PtUUISettings.GetOrCreateSettings().DemolishInspectorUI;
	}
}
