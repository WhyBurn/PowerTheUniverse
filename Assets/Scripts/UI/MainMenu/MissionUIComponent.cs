using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.UI.Tooltips;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

public class MissionUIComponent : MonoBehaviour
{
	private const string MISSIONS_CONTENT_ELEMENT_NAME = "MissionsContent";
	private const string BACK_BUTTON_ELEMENT_NAME = "BackButton";

	[SerializeField] private DataSet gameData;
	[SerializeField] private UIDocument missionSelectDocument;
	[SerializeField] private VisualTreeAsset missionButtonPrefab;
	[SerializeField] private TooltipComponent tooltip;

	private VisualElement missionsContent;
	private Button missionBackButton;

	private TooltipLayer tooltipLayer = null;
	private MissionTooltipController tooltipController = new MissionTooltipController();
	[SerializeField] private TooltipLayerFactory_SO tooltipLayerFactory;

	[HideInInspector] public UnityEvent OnPlayerClosed = new UnityEvent();

	private void Start()
	{
		FindUIElements();
		PopulateMissions();
	}

	private void FindUIElements()
	{
		missionsContent = missionSelectDocument.rootVisualElement.Q(MISSIONS_CONTENT_ELEMENT_NAME);
		Debug.Assert(missionsContent != null, $"Missing {MISSIONS_CONTENT_ELEMENT_NAME} in mission UI.");

		missionBackButton = missionSelectDocument.rootVisualElement.Q<Button>(BACK_BUTTON_ELEMENT_NAME);
		Debug.Assert(missionBackButton != null, $"Missing {BACK_BUTTON_ELEMENT_NAME} in mission UI.");

		missionBackButton.RegisterCallback<ClickEvent>(BackButton_OnClick);
	}

	private void BackButton_OnClick(ClickEvent evt)
	{
		Hide();
		OnPlayerClosed?.Invoke();
	}

	private void PopulateMissions()
	{
		missionsContent.Clear();
		for (int i = 0; i < gameData.missionDatas.Length; ++i)
		{
			VisualElement button = CreateMissionButton(gameData.missionDatas[i]);
			missionsContent.Add(button);
		}
	}

	public void Show()
	{
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		missionSelectDocument.sortingOrder = 2;
	}

	public void Hide()
	{
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.None;
	}

	public void StartMission(ClickEvent click, MissionData mission)
	{
		Debug.Log("Start mission " + mission.name);
		Data.selectedMission = mission;
		if (mission.tutorialScene != "")
		{
			SceneManager.LoadScene(mission.tutorialScene);
		}
		else
		{
			SceneManager.LoadScene("MainGame");
		}
	}

	private VisualElement CreateMissionButton(MissionData mission)
	{
		PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();

		string missionName = mission.missionName;
		VisualElement missionButtonElement = missionButtonPrefab.Instantiate();
		Button missionButton = missionButtonElement.Q<Button>("MissionButton");
		VisualElement check = missionButtonElement.Q("Check");
		missionButton.text = missionName;
		bool needsPrereq = false;
		if (mission.hasPrereq)
		{
			if (PlayerPrefs.GetInt(mission.prereqMission, -1) < mission.prereqRank)
			{
				missionButton.style.unityBackgroundImageTintColor = Color.gray;
				needsPrereq = true;
			}
		}
		if (!needsPrereq)
		{
			missionButton.RegisterCallback<ClickEvent, MissionData>(StartMission, mission);
		}
		missionButton.RegisterCallback<PointerEnterEvent, MissionData>(MissionButtonOnHoverStart, mission);
		missionButton.RegisterCallback<PointerLeaveEvent, MissionData>(MissionButtonOnHoverEnd, mission);
		int missionCompletionStatus = PlayerPrefs.GetInt(missionName, -1);
		if (missionCompletionStatus < 0)
		{
			check.style.display = DisplayStyle.None;
		}
		else
		{
			check.style.display = DisplayStyle.Flex;
			Sprite rankSprite = uiSettings.RankSettings[missionCompletionStatus].icon;
			check.style.backgroundImage = new StyleBackground(rankSprite);
			check.style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleToFit);
		}
		return (missionButtonElement);
	}

	private void MissionButtonOnHoverStart(PointerEnterEvent evt, MissionData mission)
	{
		// Remove the old layer.
		if (tooltipLayer != null)
		{
			tooltip.Remove(tooltipLayer);
			tooltipLayer = null;
		}

		// Add the new layer.
		tooltipLayer = tooltipLayerFactory.MakeLayer(tooltipController);
		tooltipController.SetMission(mission);
		tooltip.Add(tooltipLayer);
	}

	private void MissionButtonOnHoverEnd(PointerLeaveEvent evt, MissionData mission)
	{
		if (tooltipController.DisplayingMission == mission)
		{
			tooltip.Remove(tooltipLayer);
			tooltipLayer = null;
		}
	}
}
