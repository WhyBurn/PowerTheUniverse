using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.UI.Tooltips
{
	public class MissionTooltipController : ITooltipUIController
	{
		private const string MISSION_LABEL_ELEMENT_NAME = "TooltipLabel";

		private MissionData displayingMission = null;
		private Label missionLabel = null;

		public MissionData DisplayingMission { get => displayingMission; }

		public void SetMission(MissionData displayingMission)
		{
			this.displayingMission = displayingMission;

			if (displayingMission == null)
				throw new System.ArgumentNullException("Cannot call MissionTooltipController.SetMission() with a null argument.");

			if (missionLabel != null)
				missionLabel.text = GetMissionTooltipText(displayingMission);
		}

		public void Bind(VisualElement ui)
		{
			missionLabel = ui.Q<Label>(MISSION_LABEL_ELEMENT_NAME);

			Debug.Assert(missionLabel != null, $"Could not find element {MISSION_LABEL_ELEMENT_NAME} in ui element {ui}.");

			if (displayingMission != null)
				missionLabel.text = GetMissionTooltipText(displayingMission);
		}

		public void UnBind()
		{
			missionLabel = null;
		}

		private static string GetMissionTooltipText(MissionData mission)
		{
			PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();

			if (mission.hasPrereq)
			{
				if (PlayerPrefs.GetInt(mission.prereqMission, -1) < mission.prereqRank)
				{
					return ("Complete mission " + mission.prereqMission + " with rank " + uiSettings.RankSettings[mission.prereqRank].name + " or better to unlock.");
				}
			}
			float bestTime = PlayerPrefs.GetFloat(mission.missionName + "Time", -1);
			if (bestTime > 0)
			{
				string timeText = Mathf.FloorToInt(bestTime / 60).ToString("00");
				timeText += ":" + (bestTime % 60).ToString("00.0");
				return ("Best time: " + timeText);
			}
			else if(bestTime == 0)
            {
				return ("Complete");
            }
			else
			{
				return ("Not completed");
			}
		}
	}
}
