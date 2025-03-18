using UnityEngine;
using UnityEngine.UIElements;

public interface IInspectableComponent : IInspectable
{
	/// <summary>
	/// Mouse hovers over this inspectable object and shows the object's inspector UI.
	/// Override this function to apply select VFX.
	/// </summary>
	public void OnHoverEnter();
	/// <summary>
	/// Mouse stops hovering over this inspectable object and stops showing the object's inspector UI.
	/// Override this function to stop applying select VFX.
	/// </summary>
	public void OnHoverExit();
	/// <summary>
	/// User has selected this object for inspection. The object's inspector UI will be shown until the
	/// object is deselected.
	/// Override this function to apply select VFX.
	/// </summary>
	public void OnSelectStart();
	/// <summary>
	/// User has stopped selecting this object for inspection. The object's inspector UI may not be shown (depends
	/// on if the user is still hovering over the object).
	/// Override this function to stop applying select VFX.
	/// </summary>
	public void OnSelectEnd();
	/// <summary>
	/// Returns the VisualTreeAsset with the contents to be displayed in the inspector.
	/// Optionally outs an InspectorController that updates the VisualTreeAsset and accepts input
	/// from the VisualTreeAsset.
	/// </summary>
	/// <param name="inspectorController">Null if no interaction with the Inspector UI is needed.
	/// Otherwise, an object that periodically updates the state of the VisualElement, and accepts input
	/// from the VisualElement.</param>
	/// <returns>A VisualTreeAsset to show in the Inspector.</returns>
}
