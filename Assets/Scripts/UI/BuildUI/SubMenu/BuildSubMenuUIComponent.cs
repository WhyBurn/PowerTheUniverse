using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;
using AstralAvarice.Tutorial;

public class BuildSubMenuUIComponent : MonoBehaviour
{
	protected class BuildSubMenuButtonBinding : BuildingButtonBinding<int>, IHighlightable
	{

		private VisualElement rootElement;

		public BuildSubMenuButtonBinding(VisualElement buttonElement, int identifier)
			: base(buttonElement, identifier)
		{
			rootElement = buttonElement;
		}

		protected override string GetTitleName() => "BuildingName";
		protected override string GetSpecialResourcesContainerName() => "ConsumedResources";
		protected override string GetAdvancedMaterialsProductionName() => "ScienceProduction";

		public void ShowSelectUI()
		{
			button.style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUISelectedButtonSprite);
		}

		public void HideSelectUI()
		{
			button.style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUIDeselectedButtonSprite);
		}

		public void Show()
		{
			rootElement.style.display = DisplayStyle.Flex;
		}

		public void Hide()
		{
			rootElement.style.display = DisplayStyle.None;
		}

		/// <summary>
		/// Show a halo around the button, used to guide player's attention in the tutorial.
		/// </summary>
		public void ShowHighlight()
		{
			rootElement.AddToClassList(IHighlightable.HIGHLIGHT_CLASS);
		}

		/// <summary>
		/// Hide a halo around the button.
		/// </summary>
		public void HideHighlight()
		{
			rootElement.RemoveFromClassList(IHighlightable.HIGHLIGHT_CLASS);
		}
	}

	private class GameControllerCurrencyWrapper : BuildingButtonBinding<int>.ICurrencyCount
	{
		private GameController gameController;

		public GameControllerCurrencyWrapper(GameController gameController)
		{
			this.gameController = gameController;
		}

		public int Cash => gameController.Cash;

		public int AdvancedMaterials => gameController.HeldScience;
	}

	private UIDocument uiDocument;
	[SerializeField] private VisualTreeAsset buildingButtonTemplate;
	[SerializeField] private InspectorUIComponent inspector;
	[SerializeField] private GameController gameController;
	private GameControllerCurrencyWrapper wrappedGameController;

	private VisualElement rootElement;
	private VisualElement buildButtonContainer;
	private Button closeButtonElement;

	private BuildingSettingEntry[] displayingBuildingsList;
	private List<BuildSubMenuButtonBinding> buildButtonBindings = new List<BuildSubMenuButtonBinding>();

	private Dictionary<int, InspectorLayer> inspectorLayers = new Dictionary<int, InspectorLayer>();

	[HideInInspector] public UnityEvent OnShow = new UnityEvent();
	[HideInInspector] public UnityEvent OnHide = new UnityEvent();

	protected BuildingSettingEntry[] DisplayingBuildingsList { get => displayingBuildingsList; }

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		wrappedGameController = new GameControllerCurrencyWrapper(gameController);
	}

	public void Initialize()
	{
		FetchVisualElements();
		// Listen to events to change the selected building to add.
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	private void FetchVisualElements()
	{
		rootElement = uiDocument.rootVisualElement.Q("SubMenuContainer");
		buildButtonContainer = rootElement.Q("ButtonsContainer");
		buildButtonContainer.Clear();
		closeButtonElement = rootElement.Q<Button>("CloseButton");
		closeButtonElement.RegisterCallback<ClickEvent>(CloseButton_OnClick);

		// Prevent elements in the unity scroller from interfering with building placement.
		rootElement.Q("unity-content-and-vertical-scroll-container").pickingMode = PickingMode.Ignore;
		rootElement.Q("unity-content-container").pickingMode = PickingMode.Ignore;
	}

	protected virtual void CloseButton_OnClick(ClickEvent evt)
	{
		Hide();
	}

	internal void Display(IEnumerable<BuildingSettingEntry> submenuBuildings)
	{
		displayingBuildingsList = new List<BuildingSettingEntry>(submenuBuildings).ToArray();

		// Re-use buttons that already exist for the first buildings to display.
		for (int i = 0; i < displayingBuildingsList.Length && i < buildButtonBindings.Count; i++)
		{
			BuildSubMenuButtonBinding binding = buildButtonBindings[i];
			BuildingSettingEntry building = displayingBuildingsList[i];
			binding.Display(building, wrappedGameController);
			binding.Show();
		}

		// Hide buttons if there are more than what is needed.
		for (int i = displayingBuildingsList.Length; i < buildButtonBindings.Count; i++)
		{
			BuildSubMenuButtonBinding binding = buildButtonBindings[i];
			binding.Hide();
		}

		// Add buttons if we are missing them.
		for (int i = buildButtonBindings.Count; i < displayingBuildingsList.Length; i++)
		{
			BuildingSettingEntry buildingSettingEntry = displayingBuildingsList[i];
			AddBuildingButton(buildingSettingEntry, i);
		}

		// Set all buttons to be not selected.
		foreach (BuildSubMenuButtonBinding binding in buildButtonBindings)
			binding.HideSelectUI();

		// If we are in a building build mode, select the button that is mapped to the building being built
		// if there is one.
		GetBindingForBuildState(BuildManagerComponent.Instance.State)?.ShowSelectUI();

		// TODO: Refresh inspector layers when we change the displaying buildings.

	}

	private void AddBuildingButton(BuildingSettingEntry toAdd, int id)
	{
		VisualElement buildButtonElement = CreateButton();
		buildButtonContainer.Add(buildButtonElement);

		BuildSubMenuButtonBinding binding = new BuildSubMenuButtonBinding(buildButtonElement, id);
		binding.Display(toAdd, wrappedGameController);
		binding.OnClick.AddListener(BuildingButton_OnClick);
		binding.OnHoverStart.AddListener(BuildingButton_OnHoverStart);
		binding.OnHoverEnd.AddListener(BuildingButton_OnHoverEnd);

		buildButtonBindings.Add(binding);
	}

	private BuildSubMenuButtonBinding ButtonIdToBinding(int buttonId)
	{
		if (buttonId < 0)
			return null;
		return buildButtonBindings[buttonId];
	}

	private BuildingSettingEntry ButtonIdToSettingEntry(int buttonId)
	{
		return displayingBuildingsList[buttonId];
	}

	private int SettingEntryToButtonId(BuildingSettingEntry buildingSettingEntry)
	{
		return Array.FindIndex(displayingBuildingsList, (BuildingSettingEntry toCompare) => buildingSettingEntry == toCompare);
	}

	private void BuildingButton_OnHoverStart(int buttonId)
	{
		var buildingSettings = ButtonIdToSettingEntry(buttonId);

		if (!inspectorLayers.ContainsKey(buttonId))
		{
			InspectorLayer newLayer = inspector.AddLayer(buildingSettings, InspectorLayerType.UI_HOVER);
			inspectorLayers.Add(buttonId, newLayer);
		}
	}

	private void BuildingButton_OnHoverEnd(int buttonId)
	{
		if (inspectorLayers.TryGetValue(buttonId, out var layer))
		{
			inspector.RemoveLayer(layer);
			inspectorLayers.Remove(buttonId);
		}
	}

	protected virtual void BuildingButton_OnClick(int buttonId)
	{
		var toBuild = ButtonIdToSettingEntry(buttonId);
		BuildManagerComponent.Instance.SetBuildState(toBuild);
	}

	private VisualElement CreateButton()
	{
		VisualElement button = buildingButtonTemplate.Instantiate(); 
		return button;
	}

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		// If the old state was a build state, unselect the corresponding button.
		GetBindingForBuildState(oldState)?.HideSelectUI();

		// If the new state is a build state, select the corresponding building button button.
		GetBindingForBuildState(newState)?.ShowSelectUI();
	}

	private BuildSubMenuButtonBinding GetBindingForBuildState(BuildState state)
	{
		// State must not be null and must not be none.
		if (state != null && state.GetStateType() != BuildStateType.NONE)
		{
			// State must be a building build state.
			if ((state.GetStateType() & BuildStateType.BUILDING) != 0)
			{
				// There may be a button of this building being displayed.
				BuildingBuildState buildingBuildState = state as BuildingBuildState;

				int buttonId = SettingEntryToButtonId(buildingBuildState.toBuild);
				return ButtonIdToBinding(buttonId);
			}
		}

		// The state does not involve placing a building.
		return null;
	}



		/// <summary>
		/// Keep updating the bindings' affordability.
		/// </summary>
		private void Update()
	{
		foreach(var binding in buildButtonBindings)
		{
			binding.UpdateUI();
		}
	}

	/// <summary>
	/// Called when the player clicks on a building category.
	/// </summary>
	public void Show()
	{
		rootElement.style.display = DisplayStyle.Flex;
		OnShow?.Invoke();
	}

	/// <summary>
	/// Called when the player moves off of a building category.
	/// </summary>
	public void Hide()
	{
		rootElement.style.display = DisplayStyle.None;
		OnHide?.Invoke();
	}
}
