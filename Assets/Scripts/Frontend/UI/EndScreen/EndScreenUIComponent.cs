using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using AstralAvarice.Utils;
using System.Collections;

namespace AstralAvarice.Frontend
{
    public class EndScreenUIComponent : MonoBehaviour
    {
		private const string MENU_ELEMENT_NAME = "MenuLayout";
		private const string MAIN_MENU_BUTTON_ELEMENT_NAME = "MainMenuButton";
		private const string PLAY_AGAIN_BUTTON_ELEMENT_NAME = "PlayAgainButton";
		private const string FREE_PLAY_BUTTON_ELEMENT_NAME = "FreePlayButton";
		private const string TIME_BAR_ELEMENT_NAME = "TimeBar";
		private const string TIME_LABEL_ELEMENT_NAME = "TimeLabel";
		private const string RANK_RICH_TEXT_LABEL_ELEMENT_NAME = "RankRichTextLabel";

		private const string RANK_RICH_TEXT_INIT_CLASS = "rankInit";
		private const string MENU_INIT_CLASS = "menuInit";

		private const string PLAY_AGAIN_BUTTON_ON_LOSE_TEXT = "Try Again";

		[SerializeField] private UIDocument uiDocument;

		[HideInInspector] public UnityEvent OnMainMenuButtonClicked = new UnityEvent();
		[HideInInspector] public UnityEvent OnPlayAgainButtonClicked = new UnityEvent();
		[HideInInspector] public UnityEvent OnFreePlayButtonClicked = new UnityEvent();

		/// <summary>
		/// Container for most of the UI elements in the end game screen.
		/// </summary>
		private VisualElement menuElement;
		private Button mainMenuButtonElement;
		private Button playAgainButtonElement;
		private Button freePlayButtonElement;
		private TimeBarBinding timeBar;
		private Label timeLabel;
		private Label rankRichTextLabel;

		/// <summary>
		/// The time the player took to end the game.
		/// </summary>
		private int endGameTime;

		private Coroutine startupSequence;

		public void Initialize(int[] missionRankTimes)
		{
			menuElement = uiDocument.rootVisualElement.Q(MENU_ELEMENT_NAME);

			mainMenuButtonElement = uiDocument.rootVisualElement.Q<Button>(MAIN_MENU_BUTTON_ELEMENT_NAME);
			playAgainButtonElement = uiDocument.rootVisualElement.Q<Button>(PLAY_AGAIN_BUTTON_ELEMENT_NAME);
			freePlayButtonElement = uiDocument.rootVisualElement.Q<Button>(FREE_PLAY_BUTTON_ELEMENT_NAME);

			mainMenuButtonElement.RegisterCallback<ClickEvent>(MainMenuButton_OnClick);
			playAgainButtonElement.RegisterCallback<ClickEvent>(PlayAgainButton_OnClick);
			freePlayButtonElement.RegisterCallback<ClickEvent>(FreePlayButton_OnClick);

			InitializeTimeBar(missionRankTimes);

			timeLabel = uiDocument.rootVisualElement.Q<Label>(TIME_LABEL_ELEMENT_NAME);
			rankRichTextLabel = uiDocument.rootVisualElement.Q<Label>(RANK_RICH_TEXT_LABEL_ELEMENT_NAME);
		}

		private void InitializeTimeBar(int[] missionRankTimes)
		{
			// Assumes the first time is the shortest.
			int shortestTime = missionRankTimes[0];

			// Assumes the last time is the longest.
			int longestTime = missionRankTimes[missionRankTimes.Length - 1];

			// Add more time for the D rank.
			int longestTimeDisplay = Mathf.CeilToInt(shortestTime + (longestTime - shortestTime) * 1.3f);

			timeBar = new TimeBarBinding(uiDocument.rootVisualElement.Q(TIME_BAR_ELEMENT_NAME),
				shortestTime,
				longestTimeDisplay);

			RankUIData[] rankSettings = PtUUISettings.GetOrCreateSettings().RankSettings;

			// Don't include x-rank on the time bar.
			for (int i = rankSettings.Length - 1; i >= 1; i--)
			{
				int rankTimeIndex = rankSettings.Length - i - 1;

				if (rankTimeIndex > missionRankTimes.Length)
				{
					timeBar.HideTick(i);
					continue;
				}

				// Last rank is handled differently from the other ranks.
				if (rankTimeIndex == missionRankTimes.Length)
				{
					timeBar.SetTick(i, -1);
					continue;
				}

				int rankTime = missionRankTimes[rankTimeIndex];
				timeBar.SetTick(i, rankTime);
			}
		}

		private void FreePlayButton_OnClick(ClickEvent evt)
		{
			OnFreePlayButtonClicked.Invoke();
		}

		private void PlayAgainButton_OnClick(ClickEvent evt)
		{
			OnPlayAgainButtonClicked.Invoke();
		}

		private void MainMenuButton_OnClick(ClickEvent evt)
		{
			OnMainMenuButtonClicked.Invoke();
		}

		public void Show(int endGameTime, int endGameRankId)
		{
			uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

			this.endGameTime = endGameTime;

			RankUIData endGameRank = PtUUISettings.GetOrCreateSettings().RankSettings[endGameRankId];
			rankRichTextLabel.text = endGameRank.GetRichTextName(2);
			rankRichTextLabel.style.visibility = Visibility.Hidden;
			rankRichTextLabel.AddToClassList(RANK_RICH_TEXT_INIT_CLASS);

			timeBar.SetProgress(-2); // -2 so that the first tick doesn't glow.

			startupSequence = StartCoroutine(StartupRoutine());
		}

		public void Hide()
		{
			uiDocument.rootVisualElement.style.display = DisplayStyle.None;
		}

		private void SetButtonsEnabled(bool enabled)
		{
			// Show all buttons.
			mainMenuButtonElement.parent.style.visibility = enabled ? Visibility.Visible : Visibility.Hidden;

			if (this.endGameTime < 0)
			{
				playAgainButtonElement.text = PLAY_AGAIN_BUTTON_ON_LOSE_TEXT;
				freePlayButtonElement.style.display = DisplayStyle.None; // Hide the free play button on a loss.
			}
		}

		private IEnumerator StartupRoutine()
		{
			// Hide the buttons until the startup routine has finished.
			SetButtonsEnabled(false);

			yield return SlideMenu();

			yield return new WaitForSecondsRealtime(1f);

			yield return FillTimeBar();

			yield return new WaitForSecondsRealtime(1f);

			yield return ShowRankRichTextLabel();

			yield return new WaitForSecondsRealtime(1f);

			// Show the buttons again.
			SetButtonsEnabled(true);
		}

		private IEnumerator SlideMenu()
		{
			menuElement.RemoveFromClassList(MENU_INIT_CLASS);
			yield return new WaitForSecondsRealtime(menuElement.resolvedStyle.GetTotalTransitionDuration());
		}

		private IEnumerator FillTimeBar()
		{
			if (endGameTime >= 0)
			{
				// TODO: Put in settings.
				float animationDuration = 3f;

				float animationStartTime = Time.realtimeSinceStartup;

				while (Time.realtimeSinceStartup < animationDuration + animationStartTime)
				{
					yield return new WaitForEndOfFrame();

					float animationTime = Time.realtimeSinceStartup - animationStartTime;
					float animationProgress = animationTime / animationDuration;
					// The seconds to show on-screen.
					int interpSeconds = (int)Mathf.Lerp(timeBar.Max, endGameTime, 1 - Mathf.Pow(animationProgress - 1, 2));

					if (endGameTime > timeBar.Max)
						interpSeconds = endGameTime;

					timeBar.SetProgress(interpSeconds);
					timeLabel.text = UIUtils.SecondsToTime(interpSeconds);
				}
			}
		}

		private IEnumerator ShowRankRichTextLabel()
		{
			rankRichTextLabel.style.visibility = Visibility.Visible;
			rankRichTextLabel.RemoveFromClassList(RANK_RICH_TEXT_INIT_CLASS);

			yield return new WaitForSecondsRealtime(2f);
		}
    }
}
