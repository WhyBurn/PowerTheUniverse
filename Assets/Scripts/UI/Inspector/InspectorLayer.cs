using UnityEngine;
using AstralAvarice.Utils.Layers;

// In ascending order of precedence:
// DEFAULT: Nothing is being hovered over. No build mode is set.
// HOVER: The player is hovering over an inspectable object. No build mode is set.
// SELECT: The player clicked on an inspectable object. No build mode is set.
// BUILD_MODE: The player is in a particular build mode.
// UI_HOVER: The player is hovering over some UI.
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
