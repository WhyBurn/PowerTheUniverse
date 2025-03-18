using UnityEngine;
using UnityEngine.UIElements;

public interface IInspectable
{
	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController);

}
