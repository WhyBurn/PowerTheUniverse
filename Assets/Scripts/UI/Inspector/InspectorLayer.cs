using UnityEngine;
using AstralAvarice.Utils.Layers;

public enum InspectorLayerType { DEFAULT = 1, HOVER = 2, SELECT = 4, BUILD_STATE = 8, UI_HOVER = 16 }

public class InspectorLayer : Layer
{
	public readonly IInspectable inspectable;
	public InspectorLayerType LayerType { get => (InspectorLayerType)priority; }

	public InspectorLayer(IInspectable inspectable, InspectorLayerType layer) :
		base((int)layer)
	{
		this.inspectable = inspectable;
	}
}
