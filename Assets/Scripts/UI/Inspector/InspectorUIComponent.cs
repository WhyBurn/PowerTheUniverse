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
	// In ascending order of precedence:
	// DEFAULT: Nothing is being hovered over. No build mode is set.
	// HOVER: The player is hovering over an inspectable object. No build mode is set.
	// SELECT: The player clicked on an inspectable object. No build mode is set.
	// BUILD_MODE: The player is in a particular build mode.
	// UI_HOVER: The player is hovering over some UI.

	// Layers that cannot be used by outside scripts.
	private const int LOCKED_LAYERS = (int) (InspectorLayerType.DEFAULT | InspectorLayerType.HOVER | InspectorLayerType.SELECT);

	private UIDocument uiDocument;
	
	private Button collapseButton;
	private VisualElement collapsableInspectorContent;
	private VisualElement inspectorUIContainer;

	[SerializeField] private Sprite collapseButtonMenuOpened;
	[SerializeField] private Sprite collapseButtonMenuClosed;

	[SerializeField] private GameController gameController;
	[SerializeField] private SelectionCursorComponent selectionCursor;

	private SortedSet<InspectorLayer> activeInspectorLayers = new SortedSet<InspectorLayer>();

	// Only one inpectable component can be in the activeInspectorLayers set at a time.
	// Inspectable components are gameobjects that have an associated inspector UI.
	// Inspectable components show their UI when they are hovered over or selected.
	private InspectorLayer currentInspectableComponentLayer = null;
	private IInspectableComponent CurrentInspectableComponent { get => currentInspectableComponentLayer == null ? null : currentInspectableComponentLayer.inspectable as IInspectableComponent; }
	private bool isCurrentInspectableComponentSelected = false;

	private InspectorLayer currentInspectorLayer;
	private IInspectorController currentController;

	// Whether to reassess the inspector layers on LateUpdate.
	bool markedForUIUpdate = false;

	[HideInInspector] public UnityEvent<IInspectableComponent> OnHoverEnter = new UnityEvent<IInspectableComponent>();
	[HideInInspector] public UnityEvent<IInspectableComponent> OnHoverExit = new UnityEvent<IInspectableComponent>();
	[HideInInspector] public UnityEvent<IInspectableComponent> OnSelectStart = new UnityEvent<IInspectableComponent>();
	[HideInInspector] public UnityEvent<IInspectableComponent> OnSelectEnd = new UnityEvent<IInspectableComponent>();


	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();

		// If there nothing else to inspect, by default show the default inspector.
		activeInspectorLayers.Add(new InspectorLayer(new DefaultInspector(), InspectorLayerType.DEFAULT));
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
		if ((LOCKED_LAYERS & layer) != 0)
			throw new ArgumentException($"Could not externally create an inspector layer from layer {layer}. This layer is managed by the InspectorUIComponent itself.");

		InspectorLayer newLayerObj = new InspectorLayer(inspectable, (InspectorLayerType) layer);
		activeInspectorLayers.Add(newLayerObj);
		MarkForUIUpdate();
		return newLayerObj;
	}

	public bool RemoveLayer(InspectorLayer toRemove)
	{
		if ((LOCKED_LAYERS & (int) toRemove.LayerType) != 0)
			throw new ArgumentException($"Could not externally remove an inspector layer from layer {toRemove.LayerType}. This layer is managed by the InspectorUIComponent itself.");

		bool didRemove = activeInspectorLayers.Remove(toRemove);
		if(didRemove)
        {
			MarkForUIUpdate();
        }
		return didRemove;
	}

	private void CollapseButton_OnClick(ClickEvent evt)
	{
		// TODO: Change collapse button graphic.

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

	private void Update()
	{
		// Check if the selected / hovered layer has been destroyed.
		ValidateInspectorComponentIntegrity();

		// If we are in a build state, don't look for objects to hover or select.
		bool isInBuildState = BuildManagerComponent.Instance.IsInBuildState();
		// If nothing is selected, look for gameobjects to hover over.
		if (!isInBuildState && !isCurrentInspectableComponentSelected)
		{
			IInspectableComponent hovering = GetHoveringInspectable();

			// Remove the old item we were hovering over.
			if (currentInspectableComponentLayer != null)
				activeInspectorLayers.Remove(currentInspectableComponentLayer);

			// Add the new item we are hovering over.
			if (hovering != null)
			{
				InspectorLayer newLayer = new InspectorLayer(hovering, InspectorLayerType.HOVER);
				activeInspectorLayers.Add(newLayer);
				currentInspectableComponentLayer = newLayer;
			}

			// Must call after updating inspector layers.
			MarkForUIUpdate();
		}
	}

	/// <summary>
	/// Called by InputController. Tries to select an inpectable under the cursor.
	/// Should only be called whilst out of build mode and not hover over UI.
	/// </summary>
	public void TrySelect()
	{
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (BuildManagerComponent.Instance.IsInBuildState())
			return;

		IInspectableComponent toSelect = GetHoveringInspectable();
		if (toSelect == null)
			return;

		// Remove old select or hover layer.
		if (currentInspectableComponentLayer != null)
		{
			activeInspectorLayers.Remove(currentInspectableComponentLayer);
		}

		InspectorLayer selectLayer = new InspectorLayer(toSelect, InspectorLayerType.SELECT);
		isCurrentInspectableComponentSelected = true;
		activeInspectorLayers.Add(selectLayer);
		currentInspectableComponentLayer = selectLayer;
		MarkForUIUpdate();
	}

	public void FreeSelect()
	{
		// If we're not in selecting anything, there is nothing to deselect.
		if (!isCurrentInspectableComponentSelected)
			return;

		activeInspectorLayers.Remove(currentInspectableComponentLayer);
		currentInspectableComponentLayer = null;
		isCurrentInspectableComponentSelected = false;

		MarkForUIUpdate();
	}

	private IInspectableComponent GetHoveringInspectable()
	{
		List<Collider2D> allHoveringColliders = new List<Collider2D>(selectionCursor.GetHovering());
		Collider2D foundInspectableCollider = allHoveringColliders.Find((Collider2D collider) => { return collider.TryGetComponentInParent<IInspectableComponent>(out IInspectableComponent _); });

		IInspectableComponent foundInspectable = foundInspectableCollider == null ? null : foundInspectableCollider.GetComponentInParent<IInspectableComponent>();
		return foundInspectable;
	}

	private void LateUpdate()
	{
		if (markedForUIUpdate)
			UpdateTopmostInspectorLayer();
		markedForUIUpdate = false;

		if (currentController != null)
			currentController.UpdateUI();
	}

	/// <summary>
	/// Catches when selected / hovered GameObjects are destroyed.
	/// </summary>
	private void ValidateInspectorComponentIntegrity()
	{
		// Need the "as Component" otherwise the wrong == is used.
		if (currentInspectableComponentLayer != null && currentInspectableComponentLayer.inspectable as Component == null)
		{
			activeInspectorLayers.Remove(currentInspectableComponentLayer);
			currentInspectableComponentLayer = null;
			isCurrentInspectableComponentSelected = false;

			MarkForUIUpdate();
		}
	}

	private void UpdateTopmostInspectorLayer()
	{
		// Assume topmostLayer is never null.
		InspectorLayer topmostLayer = activeInspectorLayers.Max;

		// If the topmost inspectable has not changed, don't change the UI.
		if (currentInspectorLayer != null &&
			topmostLayer.inspectable == currentInspectorLayer.inspectable &&
			topmostLayer.LayerType == currentInspectorLayer.LayerType)
			return;

		if (currentInspectorLayer != null)
		{
			// If the old layer was hover or select, we need to clear hover and select.
			
			switch (currentInspectorLayer.LayerType)
			{
				case InspectorLayerType.HOVER:
					(currentInspectorLayer.inspectable as IInspectableComponent).OnHoverExit();
					OnHoverExit?.Invoke(currentInspectorLayer.inspectable as IInspectableComponent);
					break;
				case InspectorLayerType.SELECT:
					(currentInspectorLayer.inspectable as IInspectableComponent).OnSelectEnd();
					OnSelectEnd?.Invoke(currentInspectorLayer.inspectable as IInspectableComponent);
					break;
			}
		}

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

		// If the new layer is hover or select, we need to show that we're now hovering or selecting.
		switch(topmostLayer.LayerType)
		{
			case InspectorLayerType.HOVER:
				(topmostLayer.inspectable as IInspectableComponent).OnHoverEnter();
				OnHoverEnter?.Invoke(topmostLayer.inspectable as IInspectableComponent);
				break;
			case InspectorLayerType.SELECT:
				(topmostLayer.inspectable as IInspectableComponent).OnSelectStart();
				OnSelectStart?.Invoke(topmostLayer.inspectable as IInspectableComponent);
				// If the player selected something and the inspector was closed, open it.
				if (collapsableInspectorContent.style.display == DisplayStyle.None)
					CollapseButton_OnClick(null);
				break;
		}

		currentInspectorLayer = topmostLayer;
	}
}
