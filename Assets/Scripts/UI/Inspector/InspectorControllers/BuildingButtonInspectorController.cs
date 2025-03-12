using UnityEngine;
using UnityEngine.UIElements;

public class BuildingButtonInspectorController : BuildingInspectorController
{
	private BuildingSettingEntry buildSettings;

	private string BuildingName
	{
		get => buildSettings.VisualAsset.buildingName;
	}

	private Sprite BuildingIcon
	{
		get => buildSettings.VisualAsset.buildingIcon;
	}

	private int BuildingCost
	{
		get => buildSettings.BuildingDataAsset.cost;
	}

	private int BuildingScienceCost
	{
		get => buildSettings.BuildingDataAsset.scienceCost;
	}

	private bool BuildingHasIncome
	{
		get => buildSettings.BuildingDataAsset.income > 0;
	}

	private int BuildingIncome
	{
		get => buildSettings.BuildingDataAsset.income;
	}

	private bool BuildingProducesPower
	{
		get => buildSettings.BuildingDataAsset.powerProduced > 0;
	}

	private int BuildingProducedPower
	{
		get => buildSettings.BuildingDataAsset.powerProduced;
	}

	private bool BuildingConsumesElectricity
	{
		get => buildSettings.BuildingDataAsset.powerRequired > 0;
	}

	private int BuildingConsumedElectricity
	{
		get => buildSettings.BuildingDataAsset.powerRequired;
	}

	private bool BuildingHasUpkeep
	{
		get => buildSettings.BuildingDataAsset.upkeep > 0;
	}

	private int BuildingUpkeep
	{
		get => buildSettings.BuildingDataAsset.upkeep;
	}

	private int BuildingConnectionsTotal
	{
		get => buildSettings.BuildingDataAsset.maxPowerLines;
	}

	private int BuildingMass
	{
		get => buildSettings.BuildingDataAsset.mass;
	}

	private string BuildingDescription
	{
		get => buildSettings.VisualAsset.buildingDescription;
	}

	private int BuildingScienceIncome
	{
		get => buildSettings.BuildingDataAsset.scienceIncome;
	}

	public BuildingButtonInspectorController(BuildingSettingEntry toDisplay)
	{
		buildSettings = toDisplay;
	}

	public override void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		base.ConnectInspectorUI(inspectorUI);
	}

	public override void UpdateUI()
	{
		Title.text = BuildingName;
		Icon.style.backgroundImage = new StyleBackground(BuildingIcon);

		Cost?.SetValue("$" + BuildingCost.ToString());

		if (BuildingScienceCost > 0)
		{
			AdvancedMaterialsCost?.SetValue(BuildingScienceCost.ToString());
		}
		else
		{
			AdvancedMaterialsCost?.Hide();
		}

		if (BuildingHasIncome)
		{
			Income?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
			Income?.SetValue("$" + BuildingIncome.ToString());
		}
		else
			Income.Hide();

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
		{
			AdvancedMaterialsProduction?.Hide();
		}

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

		Connections?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
		Connections?.SetValue(BuildingConnectionsTotal.ToString());

		Mass?.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
		Mass?.SetValue(BuildingMass.ToString());

		ResourcesBinding.ShowBuildingTypeResources(buildSettings.BuildingDataAsset);

		Settings.style.display = DisplayStyle.None;

		Description.text = BuildingDescription;
	}

	public override void DisconnectInspectorUI()
	{
	}
}
