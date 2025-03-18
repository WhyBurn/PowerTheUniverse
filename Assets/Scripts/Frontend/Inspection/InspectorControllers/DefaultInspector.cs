using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class DefaultInspector : IInspectable
{
	/// <summary>
	/// Sets the default inspector to show the mission name and details.
	/// </summary>
	public class DefaultInspectorController : IInspectorController
	{
		public void ConnectInspectorUI(TemplateContainer inspectorUI)
		{
			Label missionName = inspectorUI.Q<Label>("MissionName");
			missionName.text += Data.selectedMission.missionName;

			Label missionDescription = inspectorUI.Q<Label>("MissionDescription");
			missionDescription.text = Regex.Replace(missionDescription.text, @"\$\d*", "$" + Data.selectedMission.cashGoal.ToString("0.00"));
			missionDescription.text = Regex.Replace(missionDescription.text, @"SECONDS", (Data.selectedMission.goalTimes[0] / 60) + ":" + (Data.selectedMission.goalTimes[0] % 60).ToString("00"));
		}

		public void DisconnectInspectorUI() { }

		public void UpdateUI() { }
	}

	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = new DefaultInspectorController();
		return PtUUISettings.GetOrCreateSettings().DefaultInspectorUI;
	}
}
