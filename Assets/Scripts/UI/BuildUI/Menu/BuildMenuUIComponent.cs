using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.VisualData;
using System.Collections.Generic;
using System;

public class BuildMenuUIComponent : MonoBehaviour
{
	private UIDocument uiDocument;

	// TODO: Implement.
	/// <summary>
	/// All the buildings that can be placed by the player in this level.
	/// </summary>
	private BuildingSettingEntry[] AllBuildings { get; set; }

	private BuildSubMenuUIComponent submenu;
	[SerializeField] private GameController gameController;
	[SerializeField] private InspectorUIComponent inspector;

	[SerializeField] private VisualTreeAsset menuButtonTemplate;

	private VisualElement menuContainer;
	private VisualElement categoryButtonsContainer;
	private VisualElement buildModeButtonsContainer;
	private List<BuildUIMenuElementBinding<int>> buttonBindings = new List<BuildUIMenuElementBinding<int>>();

	private IBuildUIMenuElement[] displayingMenuElements;

	private Dictionary<int, InspectorLayer> activeLayers = new Dictionary<int, InspectorLayer>();

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		submenu = GetComponent<BuildSubMenuUIComponent>();
		gameController.OnLevelLoad?.AddListener(GameController_OnLevelLoad);
	}

	private void GameController_OnLevelLoad()
	{
		AllBuildings = BuildManagerComponent.Instance.PlaceableBuildings;

		Initialize();
	}

	/// <summary>
	/// Find all the relevant visual elements in the UI document, instantiate
	/// buttons etc.
	/// </summary>
	private void Initialize()
	{
		submenu.Initialize();
		submenu.Hide();
		FetchVisualElements();
		InitializeMenuElements();
		RegisterExternalEvents();
	}

	private void FetchVisualElements()
	{
		menuContainer = uiDocument.rootVisualElement.Q("MenuContainer");
		categoryButtonsContainer = menuContainer.Q("BuildingCategoriesContainer");
		categoryButtonsContainer.Clear();
		buildModeButtonsContainer = menuContainer.Q("BuildModeContainer");
		buildModeButtonsContainer.Clear();
	}

	private void InitializeMenuElements()
	{
		displayingMenuElements = GatherMenuElements();

		foreach (IBuildUIMenuElement toDisplay in displayingMenuElements)
			AddMenuElement(toDisplay);
	}

	private void AddMenuElement(IBuildUIMenuElement toAdd)
	{
		// Create a new instance of a button to display the element.
		TemplateContainer buttonInstance = menuButtonTemplate.Instantiate();

		// Add the button to the appropriate container.
		if (toAdd is BuildCategory)
			categoryButtonsContainer.Add(buttonInstance);
		else
			buildModeButtonsContainer.Add(buttonInstance);

		// Bind the button to the element.
		int newBindingId = buttonBindings.Count;
		BuildUIMenuElementBinding<int> buttonBinding = new BuildUIMenuElementBinding<int>(buttonInstance, newBindingId);
		buttonBindings.Add(buttonBinding);
		buttonBinding.Display(toAdd);

		buttonBinding.OnClick.AddListener(MenuElement_OnClick);
		buttonBinding.OnHoverStart.AddListener(MenuElement_OnHoverStart);
		buttonBinding.OnHoverEnd.AddListener(MenuElement_OnHoverEnd);
	}

	private void MenuElement_OnClick(int id)
	{
		IBuildUIMenuElement clickedElement = displayingMenuElements[id];
		
		// Multiplex the element that was clicked.
		if (clickedElement is BuildModeVisuals)
		{
			// The element is a build mode.
			BuildModeVisuals clickedMode = clickedElement as BuildModeVisuals;
			PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();

			if (uiSettings.CableModeVisuals == clickedMode)
				OnCableClicked();
			if (uiSettings.MoveModeVisuals == clickedMode)
				OnMoveClicked();
			if (uiSettings.DemolishModeVisuals == clickedMode)
				OnDemolishClicked();
		}
		else
		{
			// The element is a building category.
			BuildCategory clickedCategory = clickedElement as BuildCategory;
			OnCategoryClicked(clickedCategory, id);
		}
	}

	protected virtual void OnCableClicked()
	{
		BuildManagerComponent.Instance.SetCableState();
	}

	protected virtual void OnMoveClicked()
	{
		BuildManagerComponent.Instance.SetMoveState(null);
		submenu.Hide();
	}

	protected virtual void OnDemolishClicked()
	{
		BuildManagerComponent.Instance.SetDemolishState();
		submenu.Hide();
	}

	private void OnCategoryClicked(BuildCategory clickedCategory, int id)
	{
		var submenuBuildings = GatherBuildingsOfCategory(clickedCategory);

		submenu.Display(submenuBuildings);
		submenu.Show();

		// Set all the building bindings to not be selected.
		foreach (BuildUIMenuElementBinding<int> binding in FindBindingsForBuildCategories())
			binding.HideSelectUI();

		// Set the binding that was clicked to be selected.
		buttonBindings[id].ShowSelectUI();
	}

	private void MenuElement_OnHoverStart(int id)
	{
		IBuildUIMenuElement element = displayingMenuElements[id];
		if (!(element is IInspectable))
			return;

		var layer = inspector.AddLayer(element as IInspectable, InspectorLayerType.UI_HOVER);

		activeLayers[id] = layer;
	}

	private void MenuElement_OnHoverEnd(int id)
	{
		IBuildUIMenuElement element = displayingMenuElements[id];
		if (!(element is IInspectable))
			return;

		if (!activeLayers.ContainsKey(id))
			return;

		inspector.RemoveLayer(activeLayers[id]);
		activeLayers.Remove(id);
	}

	/// <summary>
	/// Registers this component to events invoked by other classes.
	/// </summary>
	private void RegisterExternalEvents()
	{
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);
		submenu.OnHide.AddListener(Submenu_OnHide);
	}

	private void Submenu_OnHide()
	{
		// Set all the building bindings to not be selected.
		foreach (BuildUIMenuElementBinding<int> binding in FindBindingsForBuildCategories())
			binding.HideSelectUI();
	}

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		foreach (BuildUIMenuElementBinding<int> binding in FindBindingsForBuildState(oldState))
			binding.HideSelectUI();

		foreach (BuildUIMenuElementBinding<int> binding in FindBindingsForBuildState(newState))
			binding.ShowSelectUI();
	}

	/// <summary>
	/// Find all the bingins that belong to build categories.
	/// </summary>
	private IEnumerable<BuildUIMenuElementBinding<int>> FindBindingsForBuildCategories()
	{
		for (int i = 0; i < displayingMenuElements.Length; i++)
		{
			IBuildUIMenuElement element = displayingMenuElements[i];

			if (element is BuildCategory)
				yield return buttonBindings[i];
		}
	}

	/// <summary>
	/// Find all the bindings that belong to a particular build state.
	/// </summary>
	/// <param name="state">The state that the buildings belong to.</param>
	private IEnumerable<BuildUIMenuElementBinding<int>> FindBindingsForBuildState(BuildState state)
	{
		if (state == null)
			yield break;

		PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();

		// Find the state type of the passed state.
		bool isCableState = (state.GetStateType() & BuildStateType.CABLE) > 0;
		bool isDemolishState = state.GetStateType() == BuildStateType.DEMOLISH;
		bool isMoveState = state.GetStateType() == BuildStateType.MOVE;

		// Go through all the display elements and bindings.
		for (int i = 0; i < displayingMenuElements.Length; i++)
		{
			IBuildUIMenuElement element = displayingMenuElements[i];

			// If the element is for building categories, it is not for a build state. Ignore it.
			if (!(element is BuildModeVisuals))
				continue;

			BuildModeVisuals visuals = element as BuildModeVisuals;
			BuildUIMenuElementBinding<int> binding = buttonBindings[i];
			
			// Get the properties of the binding's build mode.
			bool isCableBinding = uiSettings.CableModeVisuals == visuals;
			bool isMoveBinding = uiSettings.MoveModeVisuals == visuals;
			bool isDemolishBinding = uiSettings.DemolishModeVisuals == visuals;

			// Compare properties.
			bool matchingState = (isCableState && isCableBinding) || (isMoveState && isMoveBinding) || (isDemolishState && isDemolishBinding);

			// Return if the properties match.
			if (matchingState)
				yield return binding;
		}
	}

	/// <summary>
	/// Gathers all the build menu elements that should be displayed in the lower left
	/// of the screen.
	/// </summary>
	/// <returns>A list of build menu elements sorted by priority.</returns>
	private IBuildUIMenuElement[] GatherMenuElements()
	{
		List<IBuildUIMenuElement> elementsList = new List<IBuildUIMenuElement>();

		// Get the elements that correspond to "static" modes (cables, move, demolish).
		elementsList.AddRange(GatherBuildModeMenuUIElements());

		// Get the elements that correspond to building types (generators, pylons, utilities, labs).
		elementsList.AddRange(GatherBuildingMenuUIElements());

		// Sort by priority.
		elementsList.Sort(new BuildUIMenuElementPriorityComparer());
		
		return elementsList.ToArray();
	}

	/// <summary>
	/// Get the menu elements for build modes other than building build mode.
	/// </summary>
	private IEnumerable<IBuildUIMenuElement> GatherBuildModeMenuUIElements()
	{
		PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();
		yield return uiSettings.CableModeVisuals;
		yield return uiSettings.MoveModeVisuals;
		yield return uiSettings.DemolishModeVisuals;
	}

	/// <summary>
	/// Get the menu elements for the different building categories.
	/// </summary>
	private IEnumerable<IBuildUIMenuElement> GatherBuildingMenuUIElements()
	{
		HashSet<BuildCategory> uniqueCategories = new HashSet<BuildCategory>();

		// Collect all unique categories.
		foreach (BuildingSettingEntry building in AllBuildings)
			uniqueCategories.UnionWith(building.VisualAsset.categories);

		return uniqueCategories;
	}

	private IEnumerable<BuildingSettingEntry> GatherBuildingsOfCategory(BuildCategory category)
	{
		// Find all buildings with the specified category.
		return System.Array.FindAll(
			AllBuildings,
			(BuildingSettingEntry building) => System.Array.IndexOf(building.VisualAsset.categories, category) >= 0
		);
	}
}
