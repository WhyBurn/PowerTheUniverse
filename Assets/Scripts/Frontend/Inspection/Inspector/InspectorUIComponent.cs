using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using werignac.Utils;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using AstralAvarice.Utils.Layers;

[RequireComponent(typeof(UIDocument))]
public class InspectorUIComponent : MonoBehaviour
{
	private UIDocument uiDocument;
	
	private Button collapseButton;
	private VisualElement collapsableInspectorContent;
	private VisualElement inspectorUIContainer;

	[SerializeField] private Sprite collapseButtonMenuOpened;
	[SerializeField] private Sprite collapseButtonMenuClosed;

	private LayerContainer<InspectorLayer> inspectorLayers = new LayerContainer<InspectorLayer>();


	private InspectorLayer currentInspectorLayer;
	private IInspectorController currentController;

	// Whether to reassess the inspector layers on LateUpdate.
	bool markedForUIUpdate = false;

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();

		// If there nothing else to inspect, by default show the default inspector.
		inspectorLayers.Add(new InspectorLayer(new DefaultInspector(), InspectorLayerType.DEFAULT));
		MarkForUIUpdate();

		inspectorLayers.OnTopLayerChanged.AddListener(InspectorLayer_OnTopLayerChanged);
	}

	private void InspectorLayer_OnTopLayerChanged(InspectorLayer oldTop, InspectorLayer newTop)
	{
		MarkForUIUpdate();
	}

	private void MarkForUIUpdate()
	{
		markedForUIUpdate = true;
	}

	private void Start()
	{
		collapseButton = uiDocument.rootVisualElement.Q<Button>("CollapseButton");
		collapsableInspectorContent = uiDocument.rootVisualElement.Q("CollapsableContainer");
		inspectorUIContainer = uiDocument.rootVisualElement.Q("ScrolledContent");

		collapseButton.RegisterCallback<ClickEvent>(CollapseButton_OnClick);
	}

	public InspectorLayer AddLayer(IInspectable inspectable, InspectorLayerType layer)
	{
		return AddLayer(inspectable, (int)layer);
	}

	public InspectorLayer AddLayer(IInspectable inspectable, int layer)
	{
		InspectorLayer newLayerObj = new InspectorLayer(inspectable, (InspectorLayerType) layer);
		inspectorLayers.Add(newLayerObj);
		return newLayerObj;
	}

	public bool RemoveLayer(InspectorLayer toRemove)
	{
		return inspectorLayers.Remove(toRemove);
	}

	private void CollapseButton_OnClick(ClickEvent evt)
	{
		switch (collapsableInspectorContent.style.display.value)
		{
			case DisplayStyle.None:
				collapsableInspectorContent.style.display = DisplayStyle.Flex;
				collapseButton.style.backgroundImage = new StyleBackground(collapseButtonMenuOpened);
				break;
			case DisplayStyle.Flex:
				collapsableInspectorContent.style.display = DisplayStyle.None;
				collapseButton.style.backgroundImage = new StyleBackground(collapseButtonMenuClosed);
				break;
		}
	}

	private void LateUpdate()
	{
		if (markedForUIUpdate)
			UpdateTopmostInspectorLayer();
		markedForUIUpdate = false;

		if (currentController != null)
			currentController.UpdateUI();
	}

	private void UpdateTopmostInspectorLayer()
	{
		// Assume topmostLayer is never null.
		InspectorLayer topmostLayer = inspectorLayers.Max;

		// If the topmost inspectable has not changed, don't change the UI.
		if (currentInspectorLayer != null &&
			topmostLayer.inspectable == currentInspectorLayer.inspectable &&
			topmostLayer.LayerType == currentInspectorLayer.LayerType)
			return;

		// Only instantiate new ui if there was a change in the object that
		// is shown in the inspector. Going from hovering to selecting the same object
		// should not trigger a change in UI.
		if (currentInspectorLayer == null || topmostLayer.inspectable != currentInspectorLayer.inspectable)
		{
			// Remove old controller.
			if (currentController != null)
				currentController.DisconnectInspectorUI();
			// Remove old UI.
			if (inspectorUIContainer.childCount > 0)
				inspectorUIContainer.RemoveAt(0);

			// Create new inspector UI and controller.
			var inspectorAsset = topmostLayer.inspectable.GetInspectorElement(out IInspectorController newInspectorController);
			var inspectorInstance = inspectorAsset.Instantiate();
			if (newInspectorController != null)
				newInspectorController.ConnectInspectorUI(inspectorInstance);

			// Put the new inspector in the inspector contianer.
			inspectorUIContainer.Add(inspectorInstance);
		
			currentController = newInspectorController;
		}

		currentInspectorLayer = topmostLayer;
	}
}
