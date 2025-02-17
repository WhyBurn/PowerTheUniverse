using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class PlanetResourcesUIComponent : WorldToScreenUIComponent
{
	private PlanetComponent planet;

	private SpecialResourcesContainerUIBinding resourcesBinding;

	protected override void Start()
	{
		base.Start();
		planet = GetComponentInParent<PlanetComponent>();
		resourcesBinding = new SpecialResourcesContainerUIBinding(ui);
	}

	private void Update()
	{
		if (ui == null)
			return;

#if UNITY_EDITOR
		if (SceneManager.GetActiveScene().name == "LevelBuilder")
		{
			Hide();
			return;
		}
#endif

		ResourceType[] availableResourceTypes = GetAvailableResourceTypes();

		if (availableResourceTypes.Length == 0)
		{
			Hide();
			return;
		}

		ShowAndUpdate(availableResourceTypes);
	}

	private ResourceType[] GetAvailableResourceTypes()
	{
		List<ResourceType> resourceTypes = new List<ResourceType>();

		for (int i = 0; i < (int)ResourceType.Resource_Count; ++i)
		{
			if (planet.GetResourceCount((ResourceType)i) > 0)
				resourceTypes.Add((ResourceType)i);
		}

		return resourceTypes.ToArray();
	}

	private void Hide()
	{
		ui.style.display = DisplayStyle.None;
	}

	private void ShowAndUpdate(ResourceType[] availableResourceTypes)
	{
		ui.style.display = DisplayStyle.Flex;

		foreach (var pair in resourcesBinding.ShowResources(GetAvailableResourceTypes()))
		{
			int resourceQuantity = planet.GetAvailableResourceCount(pair.Item1);
			int resourceTotal = planet.GetResourceCount(pair.Item1);

			pair.Item2.SetQuantity(resourceQuantity);
			pair.Item2.SetTotal(resourceTotal);
		}
	}
}
