using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Collections.Generic;
using werignac.Utils;
using System;
using UnityEngine.InputSystem;

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
	//[SerializeField] private UIDocument gridPowerDocument;
	//[SerializeField] private GridGroupViewComponent gridGroupView;
	[SerializeField] private Camera mainCamera;


    private Label cashLabel;
    private Label incomeLabel;
	private Label scienceLabel;
	private Label scienceIncomeLabel;
	private Label timeLabel;
	private Label timeScaleLabel;
	private Label gridPowerLabel;
	/// <summary>
	/// Warning: Changing this variable directly instead of GameSpeed will not update Time.timeScale.
	/// </summary>
	protected int gameSpeed;
	private bool gameEnded = false;
	private bool gamePaused = false;
	private int prePauseGameSpeed = 1;
	private float goalFixedDeltaTime;

	[HideInInspector] public UnityEvent OnLevelLoad = new UnityEvent();

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

	// Set the gameSpeed property along with Time.timeScale.
	public int GameSpeed {
		get => gameSpeed;

		protected set
		{
			gameSpeed = value;
			Time.timeScale = value;
			// Adjust Time.fixedDeltaTime to perform the same number of FixedDeltaTime updates per frame.
			Time.fixedDeltaTime = goalFixedDeltaTime * Time.timeScale;
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
        gameManager = new GameManager(this);
        cashLabel = statsDocument.rootVisualElement.Q("Cash") as Label;
        incomeLabel = statsDocument.rootVisualElement.Q("Income") as Label;
		scienceLabel = statsDocument.rootVisualElement.Q("Science") as Label;
		scienceIncomeLabel = statsDocument.rootVisualElement.Q("ScienceIncome") as Label;
		timeLabel = statsDocument.rootVisualElement.Q("Time") as Label;
		timeScaleLabel = statsDocument.rootVisualElement.Q<Label>("TimeScale");
		//gridPowerLabel = gridPowerDocument.rootVisualElement.Q<Label>("GridPowerLabel");

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
			cableComponent.SetDemolishable(true);
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
			MovePlanets();
		}
    }

	// Called on every fixed update. Due to time scaling, this may be called multiple times in
	// a single update.
    private void MovePlanets()
	{
		if (Planets.Count > 1 && GameSpeed > 0)
		{
			List<Vector2> planetTranslations = new List<Vector2>();
			for (int i = 0; i < Planets.Count; ++i)
			{
				planetTranslations.Add(new Vector2(0, 0));
			}
			for (int i = 0; i < Planets.Count; ++i)
			{
				PlanetComponent planet = Planets[i];
				float planetMass = planet.GetTotalMass() / 25f;
				for (int p = 0; p < Planets.Count; ++p)
				{
					if (p != i)
					{
						PlanetComponent other = Planets[p];
						Vector2 distance = planet.transform.position - other.transform.position;
						if (distance.magnitude < planetMass)
						{
							planetTranslations[p] += distance.normalized * planetMass / distance.magnitude / other.GetTotalMass() * GameSpeedFixedDeltaTime;
						}
					}
				}
				for(int c = 0; c < planet.BuildingContainer.childCount; ++c)
                {
					BuildingComponent building = planet.BuildingContainer.GetChild(c).gameObject.GetComponent<BuildingComponent>();
					if(building != null && building.BackendBuilding.IsPowered &&  building.Data.thrust != 0)
                    {
						Vector3 movement = building.transform.up.normalized * building.Data.thrust / planetMass * GameSpeedFixedDeltaTime * -1;
						planetTranslations[i] += new Vector2(movement.x, movement.y);
                    }
                }
			}
			for (int i = 0; i < Planets.Count; ++i)
			{
				if(planetTranslations[i].magnitude < 0.00000001f && Planets[i].PlanetVelocity.magnitude > 0.0001f)
                {
					planetTranslations[i] = Planets[i].PlanetVelocity * -1;
					if(planetTranslations[i].magnitude > GameSpeedFixedDeltaTime)
                    {
						planetTranslations[i] = planetTranslations[i].normalized * GameSpeedFixedDeltaTime;
                    }
                }
				Rigidbody2D body = Planets[i].gameObject.GetComponent<Rigidbody2D>();
				if (body != null)
                {
					Planets[i].PlanetVelocity += planetTranslations[i];
                    body.MovePosition(body.position + (Planets[i].PlanetVelocity * GameSpeedFixedDeltaTime));
                }
			}
			UpdatePlanetsSolar();
			CheckCableSnap();
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

    public void EndGame(bool victory, float victoryTime)
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
			float previousTime = PlayerPrefs.GetFloat(gameManager.MissionName + "Time", -1);
            if (previousTime <= 0 || previousTime > victoryTime)
            {
				PlayerPrefs.SetFloat(gameManager.MissionName + "Time", victoryTime);
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
		for(int i = 0; i < Planets.Count; ++i)
        {
			PlanetComponent planet = Planets[i];
			int solarAmount = planet.SolarOutput;

			for(int p = 0; p < Planets.Count; ++p)
            {
				if(p != i)
                {
					PlanetComponent other = Planets[p];
					solarAmount += ComputeSolarEnergyForPlanet(planet.transform.position, other.transform.position, other.SolarOutput);
                }
            }
			planet.SetResourceCount(ResourceType.Solar, solarAmount);
        }
		UpdateBuildingResources();
    }

	/// <summary>
	/// Computes the amount of solar energy a planet gains from being near a particular star (a normal planet can also
	/// be used: the output is always 0).
	/// </summary>
	/// <param name="planetPosition">The position in world space of the planet.</param>
	/// <param name="starPosition">The position in world space of the potential star (a planet that has 0 solar output may still be used).</param>
	/// <param name="starSolarOutput">The solar output of the potential star.</param>
	/// <returns></returns>
	public static int ComputeSolarEnergyForPlanet(Vector3 planetPosition, Vector3 starPosition, float starSolarOutput)
	{
		return Mathf.Max(0, Mathf.CeilToInt(starSolarOutput - Vector2.Distance(starPosition, planetPosition)));
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

	public void CheckCableSnap()
    {
		for(int i = 0; i < Cables.Count; ++i)
        {
			if(Cables[i].Length > GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength)
            {
				Cables[i].Demolish();
            }
			else
			{
                CableComponent.GetBoxFromPoints(
                    Cables[i].End.CableConnectionTransform.position,
                    Cables[i].Start.CableConnectionTransform.position,
                    out Vector2 center,
                    out Vector2 size,
                    out float angle
                    );

                List<Collider2D> cableOverlaps = new List<Collider2D>(Physics2D.OverlapBoxAll(center, size, angle));
                int badOverlapIndex = cableOverlaps.FindIndex((Collider2D collider) =>
                {
                    return !IsValidCableOverlap(collider, Cables[i].Start, Cables[i].End);
                });
                bool noOverlapsAlongCable = badOverlapIndex == -1;
				if(noOverlapsAlongCable)
                {
					Cables[i].CableOverlapTime = 0;
                }
				else
				{
					// Make GameSpeedFixedDeltaTime?
					Cables[i].CableOverlapTime += Time.fixedDeltaTime;
					if (Cables[i].CableOverlapTime > 0.5f)
					{
						Cables[i].Demolish();
					}
				}
            }
        }
    }

    /// <summary>
    /// Determines whether a cable can overlap over the given collider.
    /// Cables can only overlap over buildings that they connect to or other
    /// Cables that share the same builing connections.
    /// </summary>
    private bool IsValidCableOverlap(Collider2D overlapping, BuildingComponent startBuilding, BuildingComponent endBuildling)
    {
        if (overlapping.TryGetComponentInParent(out BuildingComponent overlapBuilding))
        {
            return (overlapBuilding == startBuilding) || (overlapBuilding == endBuildling);
        }

        if (overlapping.TryGetComponentInParent(out CableComponent overlapCable))
        {
            bool startIsConnectingBuilding = (overlapCable.Start == startBuilding) || (overlapCable.Start == endBuildling);
            bool endIsConnectingBuilding = (overlapCable.End == startBuilding) || (overlapCable.End == endBuildling);

            return startIsConnectingBuilding || endIsConnectingBuilding;
        }

        // TODO: Detect other cables?
        return false;
    }

	public void ReloadScene()
    {
		SceneManager.LoadScene("MainGame");
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
