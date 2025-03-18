using System;
using UnityEngine;

/// <summary>
/// A component that creates inspector layers for build states
/// when the build state changes.
/// </summary>
public class BuildStateInspectorComponent : MonoBehaviour
{
	[SerializeField] private BuildManagerComponent buildManager;
	[SerializeField] private InspectorUIComponent inspector;

	private InspectorLayer activeBuildStateLayer;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		buildManager.OnStateChanged.AddListener(BuildManager_OnStateChanged);
    }

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		// Remove the layer that was last being used for the build state.
		if (activeBuildStateLayer != null)
			inspector.RemoveLayer(activeBuildStateLayer);

		IInspectable inspectable = null;
		InspectorLayerType layerType = InspectorLayerType.BUILD_STATE;

		switch (newState.GetStateType())
		{
			case BuildStateType.BUILDING:
			case BuildStateType.BUILDING_CHAINED:
				BuildingBuildState buildingBuildState = newState as BuildingBuildState;
				inspectable = buildingBuildState;
				break;
			case BuildStateType.CABLE:
				inspectable = new InspectableCable();
				break;
			case BuildStateType.DEMOLISH:
				inspectable = new InspectableDemolish();
				break;
			case BuildStateType.MOVE:
				inspectable = new InspectableMove();
				break;
			default:
				// If we don't recognize the build state, then
				// don't add a layer.
				activeBuildStateLayer = null;
				return;
		}

		// Add the inspector for the new state.

		// Store a handle to the layer so that when the build state changes again,
		// we can remove the old build state inspector.
		activeBuildStateLayer = inspector.AddLayer(inspectable, layerType);
	}
}
