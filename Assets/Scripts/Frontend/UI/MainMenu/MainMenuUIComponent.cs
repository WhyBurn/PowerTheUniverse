using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using AstralAvarice.UI.Tooltips;
using System;

public class MainMenuUIComponent : MonoBehaviour
{
    [SerializeField] private DataSet gameData;
    [SerializeField] private UIDocument mainMenuDocument;
    [SerializeField] private MissionUIComponent missionUI;
    [SerializeField] private UIDocument settingsDocument;
    [SerializeField] private UIDocument creditsDocument;
    [SerializeField] private VisualTreeAsset missionButtonPrefab;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Sprite[] missionRankSprites;
	[SerializeField] private TooltipComponent tooltip;

    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Button missionMenuButton = mainMenuDocument.rootVisualElement.Q("MissionSelectButton") as Button;
        missionMenuButton.RegisterCallback<ClickEvent>(OpenMissionPanel);

        Button settingsButton = mainMenuDocument.rootVisualElement.Q<Button>("SettingsButton");
        settingsButton.RegisterCallback<ClickEvent>(OpenSettingsMenu);

        Button creditsButton = mainMenuDocument.rootVisualElement.Q<Button>("CreditsButton");
        creditsButton.RegisterCallback<ClickEvent>(OpenCreditsPage);

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
		
		missionUI.Hide();
		missionUI.OnPlayerClosed.AddListener(MissionUI_OnPlayerClosed);

        settingsDocument.rootVisualElement.style.display = DisplayStyle.None;
        creditsDocument.rootVisualElement.style.display = DisplayStyle.None;

		ResetTimeScale();
	}

	private void MissionUI_OnPlayerClosed()
	{
		OpenMainMenu(null);
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

    public void OpenMissionPanel(ClickEvent click)
    {
		mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
		missionUI.Show();
    }

	private void OpenMainMenu(ClickEvent click)
	{
		mainMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		missionUI.Hide();
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
}
