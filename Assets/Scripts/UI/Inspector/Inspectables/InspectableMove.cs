using UnityEngine;
using UnityEngine.UIElements;

public class InspectableMove : IInspectable
{
	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = null;
		return PtUUISettings.GetOrCreateSettings().MoveInspectorUI;
	}
}
