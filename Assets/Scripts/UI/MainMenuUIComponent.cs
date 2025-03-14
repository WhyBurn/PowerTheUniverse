using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIComponent : MonoBehaviour
{
    private static readonly string[] rankStrings = new string[] { "X", "D", "C", "B", "A", "S" };

    [SerializeField] private DataSet gameData;
    [SerializeField] private UIDocument mainMenuDocument;
    [SerializeField] private UIDocument missionSelectDocument;
    [SerializeField] private UIDocument settingsDocument;
    [SerializeField] private UIDocument creditsDocument;
    [SerializeField] private UIDocument mouseHoverDocument;
    [SerializeField] private VisualTreeAsset missionButtonPrefab;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Sprite[] missionRankSprites;

    private VisualElement missionsContent;
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private MissionData hoveredMission;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        missionsContent = missionSelectDocument.rootVisualElement.Q("MissionsContent");
        if(missionsContent == null)
        {
            Debug.Log("No missions content found");
        }
        Button missionMenuButton = mainMenuDocument.rootVisualElement.Q("MissionSelectButton") as Button;
        missionMenuButton.RegisterCallback<ClickEvent>(OpenMissionPanel);

        Button settingsButton = mainMenuDocument.rootVisualElement.Q<Button>("SettingsButton");
        settingsButton.RegisterCallback<ClickEvent>(OpenSettingsMenu);

        Button creditsButton = mainMenuDocument.rootVisualElement.Q<Button>("CreditsButton");
        creditsButton.RegisterCallback<ClickEvent>(OpenCreditsPage);

		Button missionBackButton = missionSelectDocument.rootVisualElement.Q<Button>("BackButton");
		missionBackButton.RegisterCallback<ClickEvent>(OpenMainMenu);

        Button settingsBackButton = settingsDocument.rootVisualElement.Q<Button>("BackButton");
        settingsBackButton.RegisterCallback<ClickEvent>(OpenMainMenu);

        Button creditsBackButton = creditsDocument.rootVisualElement.Q<Button>("BackButton");
        creditsBackButton.RegisterCallback<ClickEvent>(OpenMainMenu);

        masterVolumeSlider = settingsDocument.rootVisualElement.Q<Slider>("MasterSlider");
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0);
        masterVolumeSlider.value = masterVolume;
        audioMixer.SetFloat("Master", masterVolume);
        masterVolumeSlider.RegisterValueChangedCallback<float>(MasterVolumeChanged);

        musicVolumeSlider = settingsDocument.rootVisualElement.Q<Slider>("MusicSlider");
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0);
        musicVolumeSlider.value = musicVolume;
        audioMixer.SetFloat("BGM", musicVolume);
        musicVolumeSlider.RegisterValueChangedCallback<float>(MusicVolumeChanged);

        sfxVolumeSlider = settingsDocument.rootVisualElement.Q<Slider>("SFXSlider");
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0);
        sfxVolumeSlider.value = sfxVolume;
        audioMixer.SetFloat("SFX", sfxVolume);
        sfxVolumeSlider.RegisterValueChangedCallback<float>(SFXVolumeChanged);

        missionSelectDocument.sortingOrder = 0;
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.None;

        settingsDocument.rootVisualElement.style.display = DisplayStyle.None;
        creditsDocument.rootVisualElement.style.display = DisplayStyle.None;

		ResetTimeScale();
	}

	/// <summary>
	/// When we exit a game, the time scale might be wrong due to exiting whilst paused.
	/// This corrects that problem to make sure VFX look correct in the main menu.
	/// </summary>
	private void ResetTimeScale()
	{
		Time.timeScale = 1f;
		Time.fixedDeltaTime = Time.fixedUnscaledDeltaTime;
	}

    public void PopulateMissions()
    {
        missionsContent.Clear();
        for(int i = 0; i < gameData.missionDatas.Length; ++i)
        {
            VisualElement button = CreateMissionButton(gameData.missionDatas[i]);
            missionsContent.Add(button);
        }
    }

    public void OpenMissionPanel(ClickEvent click)
    {
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
		
		missionSelectDocument.sortingOrder = 2;

        PopulateMissions();
    }

    public void StartMission(ClickEvent click, MissionData mission)
    {
        Debug.Log("Start mission " + mission.name);
        Data.selectedMission = mission;
        if(mission.tutorialScene != "")
        {
            SceneManager.LoadScene(mission.tutorialScene);
        }
        else
        {
            SceneManager.LoadScene("MainGame");
        }
    }

	private void OpenMainMenu(ClickEvent click)
	{
		mainMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.None;
        settingsDocument.rootVisualElement.style.display = DisplayStyle.None;
        creditsDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void OpenSettingsMenu(ClickEvent click)
    {
        settingsDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void OpenCreditsPage(ClickEvent click)
    {
        creditsDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void MasterVolumeChanged(ChangeEvent<float> change)
    {
        float newValue = change.newValue;
        if (newValue < -29f)
        {
            newValue = -80f;
        }
        audioMixer.SetFloat("Master", newValue);
        PlayerPrefs.SetFloat("MasterVolume", newValue);
    }

    private void MusicVolumeChanged(ChangeEvent<float> change)
    {
        float newValue = change.newValue;
        if (newValue < -49f)
        {
            newValue = -80f;
        }
        audioMixer.SetFloat("BGM", newValue);
        PlayerPrefs.SetFloat("MusicVolume", newValue);
    }

    private void SFXVolumeChanged(ChangeEvent<float> change)
    {
        float newValue = change.newValue;
        if (newValue < -49f)
        {
            newValue = -80f;
        }
        audioMixer.SetFloat("SFX", newValue);
        PlayerPrefs.SetFloat("SFXVolume", newValue);
    }

    private VisualElement CreateMissionButton(MissionData mission)
    {
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
            check.style.backgroundImage = new StyleBackground(missionRankSprites[missionCompletionStatus]);
            check.style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleToFit);
        }
        return (missionButtonElement);
    }

    private void MissionButtonOnHoverStart(PointerEnterEvent evt, MissionData mission)
    {
		/// TODO: Notify Tooltip.
        //tooltipLabel.style.display = DisplayStyle.Flex;
        //tooltipLabel.text = GetMissionTooltipText(mission);
        hoveredMission = mission;
    }

    private void MissionButtonOnHoverEnd(PointerLeaveEvent evt, MissionData mission)
    {
        if(hoveredMission == mission)
        {
            hoveredMission = null;
            // TODO: Notify Tooltip
			//tooltipLabel.style.display = DisplayStyle.None;
        }
    }

    private string GetMissionTooltipText(MissionData mission)
    {
        if(mission.hasPrereq)
        {
            if(PlayerPrefs.GetInt(mission.prereqMission, -1) < mission.prereqRank)
            {
                return ("Complete mission " + mission.prereqMission + " with rank " + rankStrings[mission.prereqRank] + " or better to unlock.");
            }
        }
        float bestTime = PlayerPrefs.GetFloat(mission.missionName + "Time", -1);
        if (bestTime > 0)
        {
            string timeText = Mathf.FloorToInt(bestTime / 60).ToString("00");
            timeText += ":" + (bestTime % 60).ToString("00.0");
            return ("Best time: " + timeText);
        }
        else
        {
            return ("Not completed");
        }
    }
}
