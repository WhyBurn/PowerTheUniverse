using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a menu in the Player Settings window
/// for defining the global building settings.
/// </summary>
public class PtUUISettingsProvider
{
	internal static SerializedObject GetSerializedSettings()
	{
		return new SerializedObject(PtUUISettings.GetOrCreateSettings());
	}

	[SettingsProvider]
	public static SettingsProvider CreateRLSettingsProvider()
	{
		// First parameter is the path in the Settings window.
		// Second parameter is the scope of this setting: it only appears in the Project Settings window.
		var provider = new SettingsProvider("Project/PtU UI", SettingsScope.Project)
		{
			// By default the last token of the path is used as display name if no label is provided.
			label = "UI",
			// Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
			guiHandler = (searchContext) =>
			{
				var settings = GetSerializedSettings();
				EditorGUILayout.PropertyField(settings.FindProperty("defaultInspectorUI"), new GUIContent("Default Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildingInspectorUI"), new GUIContent("Building Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("planetInspectorUI"), new GUIContent("Planet Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("demolishInspectorUI"), new GUIContent("Demolish Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("cableInspectorUI"), new GUIContent("Cable Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("moveInspectorUI"), new GUIContent("Move Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("selectOutlineMaterial"), new GUIContent("Selection Outline Material"));
				EditorGUILayout.PropertyField(settings.FindProperty("demolishOutlineMaterial"), new GUIContent("Demolish Outline Material"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildUISelectedButtonSprite"), new GUIContent("Build UI Button Select Sprite"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildUIDeselectedButtonSprite"), new GUIContent("Build UI Button Deselect Sprite"));
				EditorGUILayout.PropertyField(settings.FindProperty("solarUIData"), new GUIContent("Solar UI"));
				EditorGUILayout.PropertyField(settings.FindProperty("coalUIData"), new GUIContent("Coal UI"));
				EditorGUILayout.PropertyField(settings.FindProperty("windUIData"), new GUIContent("Wind UI"));
				EditorGUILayout.PropertyField(settings.FindProperty("geothermalUIData"), new GUIContent("Geothermal UI"));
				EditorGUILayout.PropertyField(settings.FindProperty("gridGroupColors"), new GUIContent("Grid Group Colors"));
				EditorGUILayout.PropertyField(settings.FindProperty("cableModeVisuals"), new GUIContent("Cable Mode"));
				EditorGUILayout.PropertyField(settings.FindProperty("moveModeVisuals"), new GUIContent("Move Mode"));
				EditorGUILayout.PropertyField(settings.FindProperty("demolishModeVisuals"), new GUIContent("Demolish Mode"));
				EditorGUILayout.PropertyField(settings.FindProperty("highlightClass"), new GUIContent("Highlight Class Name"));
				EditorGUILayout.PropertyField(settings.FindProperty("visualizationSettings"), new GUIContent("Visualization Settings"));
				EditorGUILayout.PropertyField(settings.FindProperty("rankSettings"), new GUIContent("Rank Settings"));
				
				settings.ApplyModifiedPropertiesWithoutUndo();
				settings.Dispose();
			},

			// Populate the search keywords to enable smart search filtering and label highlighting:
			keywords = new HashSet<string>(new[] { "UI"})
		};

		return provider;
	}
}
