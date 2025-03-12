using UnityEngine;
using UnityEngine.UIElements;

public class BuildingInstanceInspectorController : BuildingInspectorController
{
	private BuildingComponent displayingBuilding;

	private string BuildingName
	{
		get
		{
			if (displayingBuilding.BuildingVisuals != null)
			{
				return displayingBuilding.BuildingVisuals.buildingName;
			}
			return displayingBuilding.Data.buildingName;
		}
	}

	private Sprite BuildingIcon
	{
		get
		{
			if (displayingBuilding.BuildingVisuals != null)
				return displayingBuilding.BuildingVisuals.buildingIcon;

			return null;
		}
	}

	private bool BuildingHasIncome
	{
		get => displayingBuilding.Data.income > 0;
	}

	private int BuildingIncome
	{
		get => displayingBuilding.Data.income;
	}

	private bool BuildingProducesPower
	{
		get => displayingBuilding.Data.powerProduced > 0;
	}

	private int BuildingProducedPower
	{
		get => displayingBuilding.Data.powerProduced;
	}

	private bool BuildingConsumesElectricity
	{
		get => displayingBuilding.Data.powerRequired > 0;
	}

	private int BuildingConsumedElectricity
	{
		get => displayingBuilding.Data.powerRequired;
	}

	private bool BuildingHasUpkeep
	{
		get => displayingBuilding.Data.upkeep > 0;
	}

	private int BuildingUpkeep
	{
		get => displayingBuilding.Data.upkeep;
	}

	private int BuildingConnectionsConsumed
	{
		get => displayingBuilding.BackendBuilding.NumConnected;
	}

	private int BuildingConnectionsTotal
	{
		get => displayingBuilding.Data.maxPowerLines;
	}

	private int BuildingMass
	{
		get => displayingBuilding.Data.mass;
	}

	private bool BuildingIsPowered
	{
		get => displayingBuilding.BackendBuilding.IsPowered;
    }

    private int BuildingScienceIncome
    {
        get => displayingBuilding.Data.scienceIncome;
    }

    private string BuildingDescription
	{
		get
		{
			if (displayingBuilding.BuildingVisuals != null)
				return displayingBuilding.BuildingVisuals.buildingDescription
		;
			return "";
		}
	}

	public BuildingInstanceInspectorController(BuildingComponent toDisplay)
	{
		displayingBuilding = toDisplay;
	}

	public override void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		base.ConnectInspectorUI(inspectorUI);

		RegisterListeners();
	}

	private void RegisterListeners()
	{
		PoweredButton.RegisterCallback<ClickEvent>(PoweredButton_OnClick);
	}

	private void UnregisterListeners()
	{
		PoweredButton.UnregisterCallback<ClickEvent>(PoweredButton_OnClick);
	}

	private void PoweredButton_OnClick(ClickEvent evt)
	{
		displayingBuilding.BackendBuilding.TogglePower();
	}

	public override void UpdateUI()
	{
		Title.text = BuildingName;
		Icon.style.backgroundImage = new StyleBackground(BuildingIcon);

		Cost?.Hide();
		AdvancedMaterialsCost?.Hide();

		if (BuildingHasIncome)
		{
			// Assumed income has a divider.
			// TODO: Change to be accurate based on missing special resources.
			Income.SetValue("$" + (BuildingIncome * (BuildingIsPowered ? 1 : 0)).ToString());
			Income.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITH_DIVIDER);
			Income.SetTotal(BuildingIncome.ToString());
		}
		else
			Income?.Hide();

		if (BuildingProducesPower)
		{
			ElectricityProduction?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
			ElectricityProduction?.SetValue(BuildingProducedPower.ToString());
		}
		else
			ElectricityProduction?.Hide();


        if (BuildingScienceIncome > 0)
        {
			AdvancedMaterialsProduction?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
            AdvancedMaterialsProduction?.SetValue(BuildingScienceIncome.ToString());
        }
        else
            AdvancedMaterialsProduction?.Hide();


		if (BuildingConsumesElectricity)
		{
			ElectricityConsumption?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
			ElectricityConsumption?.SetValue(BuildingConsumedElectricity.ToString());
		}
		else
			ElectricityConsumption?.Hide();

		if (BuildingHasUpkeep)
		{
			Upkeep?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
			Upkeep?.SetValue(BuildingUpkeep.ToString());
		}
		else
			Upkeep?.Hide();

		Connections?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITH_DIVIDER);
		Connections?.SetValue(BuildingConnectionsConsumed.ToString());
		Connections?.SetTotal(BuildingConnectionsTotal.ToString());

		Mass?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
		Mass?.SetValue(BuildingMass.ToString());

		ResourcesBinding.ShowBuildingInstanceResources(displayingBuilding);

        if (BuildingConsumesElectricity)
			PoweredCheckMark.style.display = BuildingIsPowered ? DisplayStyle.Flex : DisplayStyle.None;
		else
			Settings.style.display = DisplayStyle.None;

		Description.text = BuildingDescription;
	}

	public override void DisconnectInspectorUI()
	{
		UnregisterListeners();
	}
}

