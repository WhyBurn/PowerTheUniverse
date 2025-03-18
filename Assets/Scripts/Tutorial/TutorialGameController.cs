using UnityEngine;
using werignac.Utils;

public class TutorialGameController : GameController
{
    enum TutorialStateChangeCondition { click = 0, building = 1, cable = 2, planetDestroyed = 3, cableFour = 4, selectedHouse = 5, selectedThruster = 6, clickFour = 7 };
    enum TutorialAllowedAction { none = 0, cable = 1, pylon = 2, fission = 3, lab = 4, coal = 5 };

    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private MissionData tutorialMission;
    [SerializeField] private TutorialStateChangeCondition[] stateChangeConditions;
    [SerializeField] private TutorialAllowedAction[] allowedActions;
    [SerializeField] private int[] gameSpeedOverrides;

    private int currentTutorialState = 0;
    private bool advanceAtEndOfNextUpdate = false;
    private int numCables = 0;

    public bool cablesAreAllowed()
    {
        return (allowedActions[currentTutorialState] == TutorialAllowedAction.cable);
    }
    public bool buildingAllowed(BuildingData building)
    {
        if(allowedActions[currentTutorialState] == TutorialAllowedAction.pylon)
        {
            return (building.buildingName == "Pylon");
        }
        else if (allowedActions[currentTutorialState] == TutorialAllowedAction.fission)
        {
            return (building.buildingName == "Fission Plant");
        }
        else if (allowedActions[currentTutorialState] == TutorialAllowedAction.lab)
        {
            return (building.buildingName == "Small Lab");
        }
        else if (allowedActions[currentTutorialState] == TutorialAllowedAction.coal)
        {
            return (building.buildingName == "Small Coal Plant");
        }
        return (false);
    }

    private void AdvanceToNextState()
    {
        ++currentTutorialState;
        if (currentTutorialState >= stateChangeConditions.Length)
        {
            EndGame(true, 0);
            ReturnToMenu();
        }
        else
        {
            BuildManagerComponent.Instance.SetNoneState();
            tutorialUI.ShowNextElement();
        }
    }

    protected override void Start()
    {
        if(tutorialMission != null)
        {
            Data.selectedMission = tutorialMission;
        }
        base.Start();
    }

    protected override void CollectInitialGameObjects()
    {
        base.CollectInitialGameObjects();
        foreach(BuildingComponent building in Buildings)
        {
            if (building.Data.buildingType == BuildingType.PowerConsumer && building.BackendBuilding.IsPowered)
            {
                building.BackendBuilding.TogglePower();
            }
        }
    }
    protected override void Update()
    {
        base.Update();
        if (gameSpeedOverrides[currentTutorialState] >= 0)
        {
            gameSpeed = gameSpeedOverrides[currentTutorialState];
        }
        if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.click && !GamePaused && Input.GetMouseButtonDown(0))
        {
            advanceAtEndOfNextUpdate = true;
        }
        if(stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.clickFour && !GamePaused && Input.GetMouseButtonDown(0))
        {
            ++numCables;
            if(numCables >= 4)
            {
                advanceAtEndOfNextUpdate = true;
            }
        }
        if(advanceAtEndOfNextUpdate)
        {
            AdvanceToNextState();
            advanceAtEndOfNextUpdate = false;
            numCables = 0;
        }
    }

    public override void BuildManager_OnBuildResovle(BuildResolve resolution)
    {
        base.BuildManager_OnBuildResovle(resolution);

        if (resolution.successfullyPlacedBuilding)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.building)
            {
                advanceAtEndOfNextUpdate = true;
            }
        }

        if (resolution.successfullyPlacedCable)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.cable)
            {
                advanceAtEndOfNextUpdate = true;
            }
            else if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.cableFour)
            {
                ++numCables;
                if(numCables >= 4)
                {
                    advanceAtEndOfNextUpdate = true;
                }
            }
        }
    }

    protected override void Planet_OnDestroyed(PlanetComponent planetComponent)
    {
        base.Planet_OnDestroyed(planetComponent);
        if (currentTutorialState < stateChangeConditions.Length && stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.planetDestroyed)
        {
            advanceAtEndOfNextUpdate = true;
        }
    }

    protected override void RegisterBuilding(BuildingComponent buildingComponent)
    {
        base.RegisterBuilding(buildingComponent);
        buildingComponent.OnBuildingSelected.AddListener(delegate { BuildingSelected(buildingComponent); });
    }

    public void BuildingSelected(BuildingComponent building)
    {
        if (currentTutorialState < stateChangeConditions.Length)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.selectedHouse && building.Data.name.Contains("House"))
            {
                advanceAtEndOfNextUpdate = true;
            }
            else if(stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.selectedThruster && building.Data.name.Contains("Thruster"))
            {
                advanceAtEndOfNextUpdate = true;
            }
        }
        
    }
}
