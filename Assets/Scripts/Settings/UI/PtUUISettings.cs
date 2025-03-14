using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UIElements;
using AstralAvarice.VisualData;
using AstralAvarice.Visualization;

/// <summary>
/// Stores references to all the buildings that can be built by the player.
/// Set in the project settings.
/// </summary>
public class PtUUISettings : ScriptableObject
{
	// Place to save settings.
	public const string uiSettingsResourcesPath = "Settings/GlobalUISettings";
	public const string uiSettingsPath = "Assets/Resources/Settings/GlobalUISettings.asset";

	[SerializeField] private VisualTreeAsset defaultInspectorUI;
	// The buildings that can be placed by the player.
	[SerializeField] private VisualTreeAsset buildingInspectorUI;
	[SerializeField] private VisualTreeAsset planetInspectorUI;
	[SerializeField] private VisualTreeAsset demolishInspectorUI;
	[SerializeField] private VisualTreeAsset cableInspectorUI;
	[SerializeField] private VisualTreeAsset moveInspectorUI;

	[Header("Selection and Outlines")]
	[SerializeField] private Material selectOutlineMaterial;
	[SerializeField] private Material demolishOutlineMaterial;

	[Header("Build UI Indicators")]
	[SerializeField] private Sprite buildUISelectedButtonSprite;
	[SerializeField] private Sprite buildUIDeselectedButtonSprite;

	[Header("Special Resources")]
	[SerializeField] private SpecialResourceUIData solarUIData;
	[SerializeField] private SpecialResourceUIData coalUIData;
	[SerializeField] private SpecialResourceUIData windUIData;
	[SerializeField] private SpecialResourceUIData geothermalUIData;

	[Header("Grid Group View")]
	[SerializeField, ColorUsage(false, true)] private Color[] gridGroupColors;

	[Header("Build Mode Visuals")]
	[SerializeField] private BuildModeVisuals cableModeVisuals;
	[SerializeField] private BuildModeVisuals moveModeVisuals;
	[SerializeField] private BuildModeVisuals demolishModeVisuals;

	[Header("Tutorial")]
	[SerializeField, Tooltip("Name of the class that is used to highlight certain UI elements with a halo. Used in the tutorial.")]
	private string highlightClass;

	[Header("Visualization")]
	[SerializeField, Tooltip("Reference to the UI settings for visualization options (e.g. gravity fields & solar fields).")]
	private VisualizationUISettings_SO visualizationSettings;

	[Header("Ranks")]
	[SerializeField, Tooltip("Mapping of rank ids to rank visuals.")]
	private RankUIData[] rankSettings;

	// Getters
	public VisualTreeAsset DefaultInspectorUI
	{
		get => defaultInspectorUI;
	}
	public VisualTreeAsset BuildingInspectorUI
	{
		get => buildingInspectorUI;
	}
	public VisualTreeAsset PlanetInspectorUI
	{
		get => planetInspectorUI;
	}

	public VisualTreeAsset DemolishInspectorUI
	{
		get => demolishInspectorUI;
	}

	public VisualTreeAsset CableInspectorUI
	{
		get => cableInspectorUI;
	}

	public VisualTreeAsset MoveInspectorUI
    {
		get => moveInspectorUI;
    }

	public Material SelectOutlineMaterial
	{
		get => selectOutlineMaterial;
	}

	public Material DemolishOutlineMaterial
	{
		get => demolishOutlineMaterial;
	}

	public Sprite BuildUISelectedButtonSprite { get => buildUISelectedButtonSprite; }
	public Sprite BuildUIDeselectedButtonSprite { get => buildUIDeselectedButtonSprite; }

	public SpecialResourceUIData SolarUIData { get => solarUIData; }
	public SpecialResourceUIData CoalUIData { get => coalUIData; }
	public SpecialResourceUIData WindUIData { get => windUIData; }
	public SpecialResourceUIData GeothermalUIData { get => geothermalUIData; }

	public Color[] GridGroupColors { get => gridGroupColors; }

	public BuildModeVisuals CableModeVisuals { get => cableModeVisuals; }
	public BuildModeVisuals MoveModeVisuals { get => moveModeVisuals; }
	public BuildModeVisuals DemolishModeVisuals { get => demolishModeVisuals; }

	public string HighlightClass { get => highlightClass; }

	public VisualizationUISettings_SO VisualizationSettings { get => visualizationSettings; }

	public RankUIData[] RankSettings => rankSettings;

	public SpecialResourceUIData GetSpecialResourceUIData(ResourceType resourceType)
	{
		SpecialResourceUIData uiData = null;

		switch(resourceType)
		{
			case ResourceType.Solar:
				uiData = SolarUIData;
				break;
			case ResourceType.Coal:
				uiData = CoalUIData;
				break;
			case ResourceType.Wind:
				uiData = WindUIData;
				break;
			case ResourceType.Thermal:
				uiData = GeothermalUIData;
				break;
			default:
				throw new System.ArgumentException($"Resource type {resourceType} has no UI Data.");
		}

		if (uiData == null)
			Debug.LogWarning($"Special resource UI of type {resourceType} is null.");

		return uiData;
	}


	/// <summary>
	/// Gets a singleton GlobalBuildingSettings reference. If there is already a
	/// GlobalBuildingSettings asset, use that. Otherwise, create a new GlobalBuildingSettings object.
	/// Creates an asset for the GlobalBuildingSettings if it doesn't exist, and we're in editor.
	/// </summary>
	/// <returns>The singleton GlobalBuildingSettings.</returns>
	public static PtUUISettings GetOrCreateSettings()
	{
		PtUUISettings settings;
#if UNITY_EDITOR
		settings = AssetDatabase.LoadAssetAtPath<PtUUISettings>(uiSettingsPath);
#else
		settings = Resources.Load<PtUUISettings>(uiSettingsResourcesPath);
#endif
		if (settings == null)
		{
			settings = ScriptableObject.CreateInstance<PtUUISettings>();
				
			// Initialize fields here.

#if UNITY_EDITOR
			AssetDatabase.CreateAsset(settings, uiSettingsPath);
			AssetDatabase.SaveAssets();
#endif
		}
		return settings;
	}

}

