using UnityEngine;

public class TutorialBuildSubMenuUIComponent : BuildSubMenuUIComponent
{
	[SerializeField] private TutorialGameController tutorialGameController;

	// Only allow the construction of buildings specified in the tutorial.
	protected override void BuildingButton_OnClick(int buttonId)
	{
		BuildingSettingEntry building = DisplayingBuildingsList[buttonId];

		if (tutorialGameController.buildingAllowed(building.BuildingDataAsset))
		{
			base.BuildingButton_OnClick(buttonId);
		}
	}
}
