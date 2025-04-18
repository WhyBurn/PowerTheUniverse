using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Collections.Generic;
using werignac.Utils;
using System;
using UnityEngine.InputSystem;
using AstralAvarice.Frontend;

public class GameController : MonoBehaviour
{
    private GameObject levelObject;
    private GameManager gameManager;
    [SerializeField] private UIDocument statsDocument;
	[SerializeField] private AudioSource sfxAudio;
	[SerializeField] private AudioClip buildClip;
	[SerializeField] private AudioClip demolishClip;
	[SerializeField] private AudioClip cableConnectClip;
	[SerializeField] private DataSet gameDataSet;
	[SerializeField] private UIDocument victoryDocument;
	[SerializeField] private UIDocument defeatDocument;
	[SerializeField] protected Camera mainCamera;

	[SerializeField] private ComputePlanetVelocitiesEvent computePlanetVelocitiesEvent;
	[SerializeField] private ComputePlanetSolarEnergyEvent computePlanetSolarEnergyEvent;

	private Label cashLabel;
    private Label incomeLabel;
	private Label scienceLabel;
	private Label scienceIncomeLabel;
	private Label timeLabel;
	private Label timeScaleLabel;
	/// <summary>
	/// Warning: Changing this variable directly instead of GameSpeed will not update Time.timeScale.
	/// </summary>
	protected int gameSpeed;
	/// <summary>
	/// False at the start of the game. Set to true when the player makes their first change
	/// to the game state (i.e. changing the game speed, placing a building, etc.).
	/// </summary>
	private bool gameStarted = false;
	private bool gameEnded = false;
	private bool gamePaused = false;
	private int prePauseGameSpeed = 1;
	private float goalFixedDeltaTime;

	[HideInInspector] public UnityEvent OnLevelLoad = new UnityEvent();
	[HideInInspector] public UnityEvent OnGameStart = new UnityEvent();

	// Refercnes to in-game objects.
	public List<PlanetComponent> Planets { get; private set; } = new List<PlanetComponent>();
	public List<BuildingComponent> Buildings { get; private set; } = new List<BuildingComponent>();
	public List<CableComponent> Cables { get; private set; } = new List<CableComponent>();

	// Getters
	public Vector2 LevelBounds { get; private set; }

    public int Cash
    {
        get { return (gameManager.Cash); }
    }
	public int HeldScience
    {
        get { return (gameManager.ScienceHeld); }
    }
	public bool GamePaused
	{
		get { return (gamePaused); }
	}
	public int Income
    {
        get { return (gameManager.Income); }
    }
	public int TargetIncome
	{
		get { return (gameManager.TargetIncome); }
	}
	public bool Winning
	{
		get { return (gameManager.Winning); }
	}
	public bool Losing
	{
		get { return (gameManager.Losing); }
	}
	public int TotalIncome
    {
        get { return (gameManager.TotalIncome); }
    }

	// Set the gameSpeed property along with Time.timeScale.
	public int GameSpeed {
		get => gameSpeed;

		protected set
		{
			gameSpeed = value;
			Time.timeScale = value;
			// Adjust Time.fixedDeltaTime to perform the same number of FixedDeltaTime updates per frame.
			Time.fixedDeltaTime = goalFixedDeltaTime * Time.timeScale;

			// Game Speed is usually changed due to player action.
			// If the game speed turns positive and we haven't started the game yet, the game has now started.
			if (!gameStarted && value > 0)
			{
				gameStarted = true;
				OnGameStart?.Invoke();
			}
		}
	}

	// Deltatime considering gameSpeed, since in rare cases, this is different from Time.fixedDeltaTime.
	private float GameSpeedFixedDeltaTime
	{
		get
		{
			return Time.fixedDeltaTime;
		}
	}

	private void Awake()
	{
		goalFixedDeltaTime = Time.fixedDeltaTime;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	protected virtual void Start()
    {
		GameSpeed = 0;
        gameManager = new GameManager();

		gameManager.OnGameEnd.AddListener(EndGame);
		gameManager.OnUpdatedCashAndCashIncome.AddListener(UpdateCashAndIncome);
		gameManager.OnUpdatedScienceAndScienceIncome.AddListener(UpdateScienceLabels);

        cashLabel = statsDocument.rootVisualElement.Q("Cash") as Label;
        incomeLabel = statsDocument.rootVisualElement.Q("Income") as Label;
		scienceLabel = statsDocument.rootVisualElement.Q("Science") as Label;
		scienceIncomeLabel = statsDocument.rootVisualElement.Q("ScienceIncome") as Label;
		timeLabel = statsDocument.rootVisualElement.Q("Time") as Label;
		timeScaleLabel = statsDocument.rootVisualElement.Q<Label>("TimeScale");

		if(defeatDocument != null)
        {
			defeatDocument.rootVisualElement.style.display = DisplayStyle.None;
			Button defeatMainMenuButton = defeatDocument.rootVisualElement.Q<Button>("MainMenuButton");
			defeatMainMenuButton.RegisterCallback<ClickEvent>(MainMenuClicked);
			Button defeatTryAgainButton = defeatDocument.rootVisualElement.Q<Button>("TryAgainButton");
			defeatTryAgainButton.RegisterCallback<ClickEvent>(TryAgainClicked);
        }
		if(victoryDocument != null)
        {
			victoryDocument.rootVisualElement.style.display = DisplayStyle.None;
			Button victoryMainMenuButton = victoryDocument.rootVisualElement.Q<Button>("MainMenuButton");
			victoryMainMenuButton.RegisterCallback<ClickEvent>(MainMenuClicked);
			Button victoryPlayAgainButton = victoryDocument.rootVisualElement.Q<Button>("PlayAgainButton");
			victoryPlayAgainButton.RegisterCallback<ClickEvent>(TryAgainClicked);
        }

		if (Data.selectedMission == null)
		{
			Data.selectedMission = gameDataSet.missionDatas[0];
		}
		if (Data.selectedMission != null)
        {
            levelObject = Instantiate<GameObject>(Resources.Load<GameObject>("Levels/" + Data.selectedMission.name));
            gameManager.StartMission(Data.selectedMission);

			LevelBounds = levelObject.GetComponent<LevelBuilder>().levelDimentions;

			CollectInitialGameObjects();

			OnLevelLoad?.Invoke();
        }
    }

	protected virtual void CollectInitialGameObjects()
	{
		foreach(PlanetComponent planet in WerignacUtils.GetComponentsInActiveScene<PlanetComponent>())
		{
			RegisterPlanet(planet);
		}
		
		foreach (BuildingComponent buildingComponent in WerignacUtils.GetComponentsInActiveScene<BuildingComponent>())
		{
			RegisterBuilding(buildingComponent);
		}

		foreach(CableComponent cableComponent in WerignacUtils.GetComponentsInActiveScene<CableComponent>())
        {
            cableComponent.SetAttachedBuildings(cableComponent.Start, cableComponent.End);
            RegisterCable(cableComponent);
		}

		UpdatePlanetsSolar();
		gameManager.CalculateIncome();
	}

	// Update is called once per frame
	protected virtual void Update()
    {
		if (gameEnded)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
			{
				ReturnToMenu();
			}
		}
		else
		{
			// Replaced gameSpeed with Time.timeScale, so no longer need to multiply by gameSpeed.
			gameManager.Update(GameSpeed * Time.unscaledDeltaTime);

			string timeText = Mathf.FloorToInt(gameManager.TimePassed / 60).ToString("00");
			timeText += ":" + Mathf.FloorToInt((gameManager.TimePassed % 60)).ToString("00");
			timeLabel.text = timeText;

			timeScaleLabel.text = "X" + GameSpeed;
		}
	}

	/// <summary>
	/// Invoked by input manager and status UI prior to update.
	/// </summary>
	public void IncrementGameSpeed()
	{
		if (!gameEnded && GameSpeed < 5)
			++GameSpeed;
	}

	/// <summary>
	/// Invoked by input manager and status UI prior to update.
	/// </summary>
	public void DecrementGameSpeed()
	{
		if (!gameEnded && GameSpeed > 1)
			--GameSpeed;
	}

	public void PauseGame()
	{
		if(!gameEnded && !gamePaused)
		{
			gamePaused = true;
			prePauseGameSpeed = GameSpeed;
			GameSpeed = 0;
		}
	}
	public void UnpauseGame()
	{
        if (!gameEnded && gamePaused)
        {
			gamePaused = false;
			GameSpeed = prePauseGameSpeed;
        }
    }

	/// <summary>
	/// Invoked by input manager and status UI prior to update.
	/// </summary>
	public void RecomputeIncome()
	{
		if (! gameEnded)
			gameManager.CalculateIncome();
	}

	void FixedUpdate()
    {
		if (!gameEnded && !gamePaused)
		{
			if (!(Planets.Count > 1 && GameSpeed > 0))
				return;

			ComputeAllPlanetVelocities();
			ApplyAllPlanetVelocities();
			UpdatePlanetsSolar();
		}
    }

	/// <summary>
	/// Calls the static event that all objects that apply forces listen to.
	/// </summary>
	private void ComputeAllPlanetVelocities()
	{
		computePlanetVelocitiesEvent.Invoke(GameSpeedFixedDeltaTime);
	}

	/// <summary>
	/// Move all the planets in accordance with their current velocities.
	/// </summary>
	private void ApplyAllPlanetVelocities()
	{
		// Apply translations and account for game speed.
		foreach (PlanetComponent planet in Planets)
		{
			planet.SumVelocity(GameSpeedFixedDeltaTime); // Sum up accumulated velocities. Also decelerate if necessary.
			planet.ApplyVelocity(GameSpeedFixedDeltaTime); // Move based on velocity.
		}
	}

    public void UpdateCashAndIncome(int newCash, int newIncome)
    {
        cashLabel.text = "" + newCash;
        incomeLabel.text = "(";
        if(newIncome > 0)
        {
            incomeLabel.text += "+";
            incomeLabel.style.color = new StyleColor(new Color(0, 0.9f, 0));
        }
        else if(newIncome == 0)
        {
            incomeLabel.style.color = new StyleColor(new Color(1, 1, 1));
        }
        else
        {
            incomeLabel.style.color = new StyleColor(new Color(0.9f, 0, 0));
        }
        incomeLabel.text += newIncome + ")";
        
    }
	public void UpdateScienceLabels(int newScience, int newIncome)
	{
		scienceLabel.text = "" + newScience;
		scienceIncomeLabel.text = "(";
		if (newIncome > 0)
		{
			scienceIncomeLabel.text += "+";
			scienceIncomeLabel.style.color = new StyleColor(new Color(0, 0.9f, 0));
		}
		else if (newIncome == 0)
		{
			scienceIncomeLabel.style.color = new StyleColor(new Color(1, 1, 1));
		}
		else
		{
			scienceIncomeLabel.style.color = new StyleColor(new Color(0.9f, 0, 0));
		}
		scienceIncomeLabel.text += newIncome + ")";
	}

    public void EndGame(bool victory, int victoryTime)
    {
        Debug.Log("Game has ended");
		gameEnded = true;
		if(victory)
        {
			int rank = GetRank(victoryTime);
			if (PlayerPrefs.GetInt(gameManager.MissionName, -1) < rank)
			{
				PlayerPrefs.SetInt(gameManager.MissionName, rank);
			}
			int previousTime = PlayerPrefs.GetInt(gameManager.MissionName + "Time", -1);
			Debug.Log(previousTime + "," + victoryTime);
            if (previousTime <= 0 || previousTime > victoryTime)
            {
				PlayerPrefs.SetInt(gameManager.MissionName + "Time", victoryTime);
            }
			if (victoryDocument != null)
			{
				victoryDocument.rootVisualElement.style.display = DisplayStyle.Flex;
			}
        }
		else
        {
            if (PlayerPrefs.GetInt(gameManager.MissionName, -1) < 0)
            {
                PlayerPrefs.SetInt(gameManager.MissionName, 0);
            }
            if (defeatDocument != null)
			{
				defeatDocument.rootVisualElement.style.display = DisplayStyle.Flex;
			}
        }
    }

	public void ReturnToMenu()
    {
		SceneManager.LoadScene("MainMenu");
    }

	public virtual void BuildManager_OnBuildResovle(BuildResolve resolution)
	{
		if(GameSpeed == 0 && resolution.TriedAnything())
		{
			GameSpeed = 1;
		}
		if (resolution.successfullyPlacedBuilding)
		{
			RegisterBuilding(resolution.builtBuilding);
			gameManager.SpendMoney(resolution.builtBuilding.Data.cost);
			gameManager.SpendScience(resolution.builtBuilding.Data.scienceCost);
			sfxAudio.clip = buildClip;
			sfxAudio.Play();
		}

		if (resolution.successfullyPlacedCable)
		{
			RegisterCable(resolution.builtCable);
			gameManager.SpendMoney(Mathf.CeilToInt(resolution.builtCable.Length * Data.cableCostMultiplier));
			if(!sfxAudio.isPlaying)
			{
                sfxAudio.clip = cableConnectClip;
                sfxAudio.Play();
            }
		}

		if (resolution.triedDemolishBuilding && resolution.demolishTarget != null)
		{
			BuildingComponent demolishedBuilding = resolution.demolishTarget as BuildingComponent;
			if (demolishedBuilding != null)
			{
				gameManager.SpendMoney(demolishedBuilding.Data.cost / -2);
			}
		}
	}

	protected virtual void RegisterBuilding(BuildingComponent buildingComponent)
	{
		Buildings.Add(buildingComponent);
		
		Building building = new Building(buildingComponent.Data);
		buildingComponent.SetGameBuilding(building);
		gameManager.AddBuilding(building);

		buildingComponent.OnBuildingDemolished.AddListener(Building_OnDestroy);
	}

	private void Building_OnDestroy(BuildingComponent buildingComponent)
	{
		Buildings.Remove(buildingComponent);
		gameManager.RemoveBuilding(buildingComponent.BackendBuilding);
		if (sfxAudio != null && sfxAudio.gameObject != null)
		{
			sfxAudio.clip = demolishClip;
			sfxAudio.Play();
		}
	}

	private void RegisterCable(CableComponent cableComponent)
	{
		Cables.Add(cableComponent);

		gameManager.AddConnection(cableComponent.Start.BackendBuilding, cableComponent.End.BackendBuilding);

		cableComponent.OnCableDemolished.AddListener(Cable_OnDestroyed);
	}

	private void Cable_OnDestroyed(CableComponent cableComponent)
	{
		Cables.Remove(cableComponent);
		gameManager.RemoveConnection(cableComponent.Start.BackendBuilding, cableComponent.End.BackendBuilding);
        if (sfxAudio != null && sfxAudio.gameObject != null)
        {
            sfxAudio.clip = demolishClip;
            sfxAudio.Play();
        }
    }

	private void RegisterPlanet(PlanetComponent planetComponent)
	{
		Planets.Add(planetComponent);

		planetComponent.OnPlanetDemolished.AddListener(Planet_OnDestroyed);
	}

    protected virtual void Planet_OnDestroyed(PlanetComponent planetComponent)
    {
        Planets.Remove(planetComponent);
    }

    public void UpdatePlanetsSolar()
    {
		computePlanetSolarEnergyEvent.Invoke();
		foreach (PlanetComponent planet in Planets)
			planet.ApplySolar();
		UpdateBuildingResources();
    }

	public void UpdateBuildingResources()
	{
		List<Building> buildingsChanged = new List<Building>();
		for (int i = 0; i < Planets.Count; ++i)
		{
			PlanetComponent planet = Planets[i];
			int[] totalResources = new int[(int)ResourceType.Resource_Count];
			for(int r = 0; r < totalResources.Length; ++r)
            {
				totalResources[r] = planet.GetResourceCount((ResourceType)r);
            }				
			for(int b = 0; b < planet.BuildingContainer.childCount; ++b)
            {
				BuildingComponent building = planet.BuildingContainer.GetChild(b).gameObject.GetComponent<BuildingComponent>();
				if(building != null)
                {
					if(building.Data.requiredResource != ResourceType.Resource_Count)
                    {
						int previousResources = building.BackendBuilding.ResourcesProvided;
						building.BackendBuilding.ResourcesProvided = Mathf.Min(totalResources[(int)building.Data.requiredResource], building.BackendBuilding.Data.resourceAmountRequired);
						totalResources[(int)building.Data.requiredResource] = Mathf.Max(0, totalResources[(int)building.Data.requiredResource] - building.BackendBuilding.Data.resourceAmountRequired);
						if(previousResources != building.BackendBuilding.ResourcesProvided)
						{
							buildingsChanged.Add(building.BackendBuilding);
						}
                    }
                }
            }
		}
		foreach(Building b in buildingsChanged)
		{
			gameManager.AdjustIncomeForConnected(b);
		}
	}

	public void ReloadScene()
    {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

	private void MainMenuClicked(ClickEvent click)
    {
		ReturnToMenu();
    }
	private void TryAgainClicked(ClickEvent click)
    {
		ReloadScene();
    }

	public List<CableComponent> GetConnectedCables(BuildingComponent building)
	{
		List<CableComponent> connectedCables = new List<CableComponent>();
		foreach(CableComponent cable in Cables)
		{
			if(cable.Start == building || cable.End == building)
			{
				connectedCables.Add(cable);
			}
		}
		return (connectedCables);
	}

	
	// When destroyed, reset time to its normal state.
	private void OnDestroy()
	{
		Time.timeScale = 1;
		Time.fixedDeltaTime = goalFixedDeltaTime;
	}

	public int GetRank(float time)
    {
		return (Data.selectedMission.GetRank(time));
    }
	public int GetRank()
	{
		if(Losing)
		{
			return (0);
		}
		else if(Winning)
		{
			return (GetRank(gameManager.WinningStartTime));
		}
		else
		{
			return (GetRank(gameManager.TimePassed));
		}
	}

	public Vector2 GetMousePosition()
	{
		Vector2 position = Mouse.current.position.ReadValue();
		Vector2 adjustedPosition = new Vector2(position.x * 1080 / Screen.height, position.y * 1080 / Screen.height);
		return (adjustedPosition);
    }

	public GridGroupData GetGridGroupData(int gridGroupId)
	{
		return gameManager.GetGroupData(gridGroupId);
	}
}
