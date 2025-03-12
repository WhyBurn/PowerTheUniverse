using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class SpecialResourcesContainerUIBinding
{
	private VisualElement specialResourcesContainer;

	private Dictionary<ResourceType, SpecialResourceUIBinding> specialResourcesBindings;

	public SpecialResourcesContainerUIBinding(VisualElement specialResourcesContainer)
	{
		this.specialResourcesContainer = specialResourcesContainer;

		specialResourcesBindings = new Dictionary<ResourceType, SpecialResourceUIBinding>
	{
		{ResourceType.Solar, new SpecialResourceUIBinding(specialResourcesContainer.Q("SolarResource")) },
		{ResourceType.Coal, new SpecialResourceUIBinding(specialResourcesContainer.Q("CoalResource")) },
		{ResourceType.Wind, new SpecialResourceUIBinding(specialResourcesContainer.Q("WindResource")) },
		{ResourceType.Thermal, new SpecialResourceUIBinding(specialResourcesContainer.Q("GeothermalResource")) },
	};

		InitializeIcons();
	}

	private void InitializeIcons()
	{
		foreach (ResourceType resourceType in specialResourcesBindings.Keys)
		{
			var binding = specialResourcesBindings[resourceType];
			binding.SetResourceIcon(resourceType);
		}
	}

	public void Show()
	{
		specialResourcesContainer.style.display = DisplayStyle.Flex;
	}

	public void Hide()
	{
		specialResourcesContainer.style.display = DisplayStyle.None;
	}

	/// <summary>
	/// Shows all the passed resources. Hides all others.
	/// </summary>
	/// <param name="resourcesToShow">An array of all the resources to show.</param>
	/// <returns>A resource type that will be shown and the binding associated with that resource. In the order of resourcestoShow. This is used to set quantities and totals.</returns>
	public IEnumerable<System.Tuple<ResourceType, IRestrictedSpecialResourceUIBinding>> ShowResources(ResourceType[] resourcesToShow)
	{
		// Show all the types that should be shown.
		foreach (ResourceType typeToShow in resourcesToShow)
		{
			if (!specialResourcesBindings.ContainsKey(typeToShow))
			{
				Debug.LogWarning($"Tried to show special resource {typeToShow} in container, but this type does not exist in the container. Skipping.");
				continue;
			}

			var toShow = specialResourcesBindings[typeToShow];
			toShow.Show();

			yield return new System.Tuple<ResourceType, IRestrictedSpecialResourceUIBinding>(typeToShow, toShow);
		}

		// Hide all the types that should be hidden.
		foreach (ResourceType potentialTypeToHide in specialResourcesBindings.Keys)
		{
			if (System.Array.IndexOf(resourcesToShow, potentialTypeToHide) >= 0)
				continue;

			specialResourcesBindings[potentialTypeToHide].Hide();
		}
	}


	/// <summary>
	/// Shows the resources for a planet.
	/// Does not take into account whether the planet is a star or not.
	/// </summary>
	/// <param name="planet">The planet to show resources for.</param>
	public void ShowPlanetResources(PlanetComponent planet)
	{
		foreach (var pair in ShowResources(GetPlanetAvailableResourceTypes(planet)))
		{
			int resourceQuantity = planet.GetAvailableResourceCount(pair.Item1);
			int resourceTotal = planet.GetResourceCount(pair.Item1);

			pair.Item2.SetDividerType(ResourceDividerType.WITH_DIVIDER);
			pair.Item2.SetColorType((resourceQuantity > 0) ? ResourceColorType.STANDARD : ResourceColorType.LACKING);
			pair.Item2.SetQuantity(resourceQuantity);
			pair.Item2.SetTotal(resourceTotal);
		}
	}

	/// <summary>
	/// Gets which resources a planet has as an array.
	/// </summary>
	private static ResourceType[] GetPlanetAvailableResourceTypes(PlanetComponent planet)
	{
		List<ResourceType> resourceTypes = new List<ResourceType>();

		for (int i = 0; i < (int)ResourceType.Resource_Count; ++i)
		{
			if (planet.GetResourceCount((ResourceType)i) > 0)
				resourceTypes.Add((ResourceType)i);
		}

		return resourceTypes.ToArray();
	}

	/// <summary>
	/// Show the resources a building type uses.
	/// No divider since these are not instances.
	/// </summary>
	public void ShowBuildingTypeResources(BuildingData buildingData)
	{
		foreach (var pair in ShowResources(GetBuildingConsumedResourceTypes(buildingData)))
		{
			int resourceQuantity = buildingData.resourceAmountRequired;

			pair.Item2.SetDividerType(ResourceDividerType.WITHOUT_DIVIDER);
			pair.Item2.SetColorType(ResourceColorType.STANDARD);
			pair.Item2.SetQuantity(resourceQuantity);
		}
	}

	/// <summary>
	/// Show the resources a building instance uses.
	/// With divider since these are instances that may be missing resources.
	/// </summary>
	public void ShowBuildingInstanceResources(BuildingComponent building)
	{
		foreach (var pair in ShowResources(GetBuildingConsumedResourceTypes(building.Data)))
		{
			int resourceQuantity = building.BackendBuilding.ResourcesProvided;
			int resourceTotal = building.Data.resourceAmountRequired;

			pair.Item2.SetDividerType(ResourceDividerType.WITH_DIVIDER);
			pair.Item2.SetColorType((resourceQuantity == resourceTotal) ? ResourceColorType.STANDARD : ResourceColorType.LACKING);
			pair.Item2.SetQuantity(resourceQuantity);
			pair.Item2.SetTotal(resourceTotal);
		}
	}

	/// <summary>
	/// Get which resource (if any) a building consumes in an array.
	/// </summary>
	private static ResourceType[] GetBuildingConsumedResourceTypes(BuildingData buildingData)
	{
		if (buildingData.requiredResource == ResourceType.Resource_Count)
			return new ResourceType[0];
		else
			return new ResourceType[] { buildingData.requiredResource };
	}
}
