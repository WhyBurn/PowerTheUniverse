using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Class that contains
/// </summary>
[Serializable]
public class BuildingSettingEntry: IInspectable
{
	// The asset containing visual information about the building.
	[SerializeField] private BuildingVisuals visualAsset;
	// The asset containing mechanical information about the building.
	[SerializeField] private BuildingData buildingDataAsset;
	// The gameobject to instantiate when making the building.
	[SerializeField] private GameObject prefab;

	// Getters
	public BuildingVisuals VisualAsset
	{
		get { return visualAsset; }
		private set { visualAsset = value; }
	}

	public BuildingData BuildingDataAsset
	{
		get { return buildingDataAsset; }
		private set { buildingDataAsset = value; }
	}

	public GameObject Prefab
	{
		get { return prefab;  }
		private set { prefab = value; }
	}

	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		// NOTE: Circular reference!
		inspectorController = new BuildingButtonInspectorController(this);
		return PtUUISettings.GetOrCreateSettings().BuildingInspectorUI;
	}
}
