using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class PlanetInspectorController : IInspectorController
{
	private Label subheading;
    private VisualElement icon;
	private VisualElement placeable;
	private Label placeableText;
    private VisualElement baseMass;
    private Label baseMassValue;
    private VisualElement totalMass;
    private Label totalMassValue;
    private VisualElement solarOutput;
    private Label solarOutputValue;
	private SpecialResourcesContainerUIBinding resourcesBinding;

    private PlanetComponent displayingPlanet;

    public PlanetInspectorController(PlanetComponent planet)
    {
        displayingPlanet = planet;
    }

	public void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		subheading = inspectorUI.Q<Label>("Subheading");
		icon = inspectorUI.Q("Icon");
		placeable = inspectorUI.Q("Placeable");
		placeableText = inspectorUI.Q<Label>("PlaceableLabel");
		baseMass = inspectorUI.Q("BaseMass");
		baseMassValue = inspectorUI.Q<Label>("BaseMassValue");
        totalMass = inspectorUI.Q("TotalMass");
        totalMassValue = inspectorUI.Q<Label>("TotalMassValue");
        solarOutput = inspectorUI.Q("SolarOutput");
        solarOutputValue = inspectorUI.Q<Label>("SolarOutputValue");
		resourcesBinding = new SpecialResourcesContainerUIBinding(inspectorUI.Q("SpecialResourcesContainer"));
    }

	public void DisconnectInspectorUI()
	{

	}
	public void UpdateUI()
	{

        subheading.text = displayingPlanet.PlanetName;

        if (displayingPlanet.CanPlaceBuildings)
        {
            placeableText.text = "Building Possible";
        }
        else
        {
            placeableText.text = "Impossible to Build";
        }

        baseMassValue.text = displayingPlanet.Mass.ToString();
        totalMassValue.text = displayingPlanet.GetTotalMass().ToString();

        int s = Mathf.FloorToInt(displayingPlanet.SolarOutput - (displayingPlanet.transform.localScale.x / 2));
        if (s > 0)
        {
            solarOutputValue.text = s.ToString();
        }
        else
        {
            solarOutput.style.display = DisplayStyle.None;
        }

		resourcesBinding.ShowPlanetResources(displayingPlanet);
    }
}
