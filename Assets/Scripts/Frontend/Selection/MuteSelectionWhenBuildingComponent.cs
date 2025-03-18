using System;
using UnityEngine;

public class MuteSelectionWhenBuildingComponent : MonoBehaviour
{
	[SerializeField] private SelectionComponent selection;
	[SerializeField] private BuildManagerComponent buildManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		buildManager.OnStateChanged.AddListener(BuildManager_OnStateChanged);   
    }

	/// <summary>
	/// When we enter a build state other than "NONE", mute the selection.
	/// Otherwise, unmute the selection.
	/// </summary>
	/// <param name="oldState"></param>
	/// <param name="newState"></param>
	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		if (newState == null)
		{
			selection.UnMute();
			return;
		}

		switch(newState.GetStateType())
		{
			case BuildStateType.NONE:
				selection.UnMute();
				break;
			default:
				selection.Mute();
				break;
		}
	}
}
