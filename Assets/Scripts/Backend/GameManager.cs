using UnityEngine;
using System.Collections.Generic;

public class GameManager
{
    private readonly int paymentInterval = 30;
    private readonly int timeTillGameEnd = 10;

    private List<Building> buildings = new List<Building>();
    private int income;
    private int cash;
    private float timePassed;
    private int[] goalTimes;
    private GameController controller;
    private MissionData currentMission;
    private int scienceHeld;
    private int scienceIncome;
    private bool hadEnoughIncomePreviously;
    private int nextGroupId;
    private SortedSet<int> freeGroupIds = new SortedSet<int>();
    private float winningStartTime;
    private float losingStartTime;

    public int Income
    {
        get { return (income); }
    }
    public int Cash
    {
        get { return (cash); }
    }
    public int ScienceHeld
    {
        get { return (scienceHeld); }
    }
    public float TimePassed
    {
        get { return (timePassed); }
    }
    public string MissionName
    {
        get { return (currentMission.missionName); }
    }
    public int TargetIncome
    {
        get { return (currentMission.cashGoal); }
    }
    public bool Winning
    {
        get { return (income > TargetIncome); }
    }
    public bool Losing
    {
        get { return (cash <= 0 && income <= 0); }
    }
    public float WinningStartTime
    {
        get { return (winningStartTime); }
    }

    public GameManager(GameController controller)
    {
        this.controller = controller;
    }

    public void SpendMoney(int spentAmount)
    {
        cash -= spentAmount;
        if (controller != null)
        {
            controller.UpdateCashAndIncome(cash, income);
        }
    }
    public void SpendScience(int spentAmount)
    {
        scienceHeld -= spentAmount;
        if (controller != null)
        {
            controller.UpdateScienceLabels(scienceHeld, scienceIncome);
        }

    }

    //Use this if buildings don't need to be instantiated at start.
    public void StartMission(MissionData mission)
    {
        hadEnoughIncomePreviously = false;
        currentMission = mission;
        cash = mission.startingCash;
        scienceHeld = mission.startingScience;
        buildings = new List<Building>();
        timePassed = 0;
        goalTimes = mission.goalTimes;
        ReassignAllGroups();
        CalculateIncome();
    }
    //Use this if BuildingComponents need a reference to the Building object being used by the manager.
    public void StartMission(MissionData mission, LevelBuilder level)
    {
        hadEnoughIncomePreviously = false;
        currentMission = mission;
        cash = mission.startingCash;
        scienceHeld = mission.startingScience;
        buildings = new List<Building>();
        timePassed = 0;
        goalTimes = mission.goalTimes;

        for (int i = 0; i < level.buildings.Length; ++i)
        {
            Building building = new Building(level.buildings[i].Data);
            building.SetManager(this);
            buildings.Add(building);
            level.buildings[i].SetGameBuilding(building);
        }

        CalculateIncome();
        ReassignAllGroups();
    }

    public void Update(float deltaTime)
    {
        float oldTime = timePassed;
        timePassed += deltaTime;
        int numPayouts = Mathf.FloorToInt(timePassed / paymentInterval) - Mathf.FloorToInt(oldTime / paymentInterval);
        for(int i = 0; i < numPayouts; ++i)
        {
            cash += income;
            scienceHeld += scienceIncome;
            if (controller != null)
            {
                controller.UpdateCashAndIncome(cash, income);
                controller.UpdateScienceLabels(scienceHeld, scienceIncome);
            }
            if (income >= currentMission.cashGoal)
            {
                if (!hadEnoughIncomePreviously)
                {
                    hadEnoughIncomePreviously = true;
                }
                else
                {
                    EndGame();
                    return;
                }
            }
        }
        if(Winning)
        {
            losingStartTime = -1;
            if(winningStartTime <= 0)
            {
                winningStartTime = timePassed;
            }
            if (timePassed > winningStartTime + timeTillGameEnd)
            {
                EndGame();
            }
        }
        else
        {
            winningStartTime = -1;
            if (Losing)
            {
                if(losingStartTime <= 0)
                {
                    losingStartTime = timePassed;
                }
                if(timePassed > losingStartTime + timeTillGameEnd)
                {
                    EndGame();
                }
            }
            else
            {
                losingStartTime = -1;
            }
        }
    }

    public void EndGame()
    {
        CalculateIncome();
        if(income >= currentMission.cashGoal)
        {
            Debug.Log("Earning enough money! Victory!");
        }
        if(controller != null)
        {
            controller.EndGame(Winning, winningStartTime);
        }
    }

    public void AddBuilding(Building building)
    {
        building.BuildingGroup = -1;
        building.SetManager(this);
        buildings.Add(building);
        income -= building.Data.upkeep;
        AdjustIncomeForConnected(building);
        AssignGroupIds(building, GetFreeGroupId());
    }

    public void RemoveBuilding(Building building)
    {
        if(buildings.Remove(building))
        {
            income += building.Data.upkeep;
            for(int i = 0; i < building.NumConnected; ++i)
            {
                building.GetConnectedBuilding(i).RemoveConnection(building);
            }
            for(int i = 0; i < building.NumConnected; ++i)
            {
                AdjustIncomeForConnected(building.GetConnectedBuilding(i));
                AssignGroupIds(building.GetConnectedBuilding(i), GetFreeGroupId());
            }
            if(building.NumConnected == 0)
            {
                if (controller != null)
                {
                    controller.UpdateCashAndIncome(cash, income);
                    controller.UpdateScienceLabels(scienceHeld, scienceIncome);
                }
            }
        }
    }

    public void AddConnection(Building first, Building second)
    {
        first.AddConnection(second);
        second.AddConnection(first);
        AdjustIncomeForConnected(first);
        int groupId = Mathf.Min(first.BuildingGroup, second.BuildingGroup);
        AssignGroupIds(first, groupId);
    }

	public void RemoveConnection(Building first, Building second)
	{
		bool removedOne = first.RemoveConnection(second);
		removedOne = second.RemoveConnection(first) || removedOne;
		if (removedOne)
		{
			AdjustIncomeForConnected(first);
			AdjustIncomeForConnected(second);
            AssignGroupIds(second, GetFreeGroupId());
            AssignGroupIds(first, GetFreeGroupId());
		}

	}

    public void AdjustIncomeForConnected(Building startingBuilding)
    {
        HashSet<Building> buildingsSeen = new HashSet<Building>();
        List<Building> connectedBuildings = new List<Building>();
        List<Building> connectedConsumers = new List<Building>();
        int totalPower = 0;
        buildingsSeen.Add(startingBuilding);
        connectedBuildings.Add(startingBuilding);
        while (connectedBuildings.Count > 0)
        {
            Building building = connectedBuildings[0];
            connectedBuildings.RemoveAt(0);
            if (building.Data.buildingType == BuildingType.PowerProducer)
            {
                totalPower += GetPower(building);
                building.PowerToGive = 0;
			}
			if (building.Data.buildingType == BuildingType.PowerConsumer)
			{
				AddConsumerToSortedList(building, connectedConsumers);
			}
			for (int i = 0; i < building.NumConnected; ++i)
            {
                Building connectedBuilding = building.GetConnectedBuilding(i);
                if (!buildingsSeen.Contains(connectedBuilding))
                {
                    buildingsSeen.Add(connectedBuilding);
					connectedBuildings.Add(connectedBuilding);
                }
            }
        }
        foreach (Building consumer in connectedConsumers)
        {
            if (consumer.IsPowered)
            {
                if (totalPower >= consumer.Data.powerRequired)
                {
                    totalPower -= consumer.Data.powerRequired;
                }
                else
                {
                    income -= consumer.Data.income;
                    scienceIncome -= consumer.Data.scienceIncome;
                    consumer.IsPowered = false;

                }
            }
        }
        foreach (Building consumer in connectedConsumers)
        {
            if (!consumer.IsPowered && totalPower >= consumer.Data.powerRequired && consumer.Data.thrust == 0)
            {
                totalPower -= consumer.Data.powerRequired;
                income += consumer.Data.income;
                scienceIncome += consumer.Data.scienceIncome;
                consumer.IsPowered = true;

            }
        }
        if(controller != null)
        {
            controller.UpdateCashAndIncome(cash, income);
            controller.UpdateScienceLabels(scienceHeld, scienceIncome);
        }
    }

    public void CalculateIncome()
    {
        List<Building> producers = new List<Building>();
        income = 0;
        scienceIncome = 2;

        int totalPower = 0;
        foreach(Building building in buildings)
        {
            income -= building.Data.upkeep;
            if(building.Data.buildingType == BuildingType.PowerConsumer)
            {
                building.IsPowered = false;
            }
            else if(building.Data.buildingType == BuildingType.PowerProducer)
            {
                building.PowerToGive = GetPower(building);
                totalPower += GetPower(building);
                producers.Add(building);
            }
        }

        foreach(Building producer in producers)
        {
            if(producer.PowerToGive > 0)
            {
                AdjustIncomeForConnected(producer);
            }
        }
        if(producers.Count <= 0)
        {
            if (controller != null)
            {
                controller.UpdateCashAndIncome(cash, income);
                controller.UpdateScienceLabels(scienceHeld, scienceIncome);
            }
        }
    }

    private void AddConsumerToSortedList(Building consumerBuilding, List<Building> consumerList)
    {
        float powerToPrice = ((float)consumerBuilding.Data.TotalIncome) / consumerBuilding.Data.powerRequired;
        int max = consumerList.Count;
        int min = 0;
        int index = 0;
        while(max > min)
        {
            index = min + ((max - min) / 2);
            bool lessThanPrev = true;
            bool greaterThanNext = true;
            int previousIndex = index - 1;
            int nextIndex = index;
            if(previousIndex >= 0)
            {
                float prevPowerToPrice = ((float)consumerList[previousIndex].Data.TotalIncome) / consumerList[previousIndex].Data.powerRequired;
                if(prevPowerToPrice < powerToPrice)
                {
                    lessThanPrev = false;
                }
            }
            if(nextIndex < consumerList.Count)
            {
                float nextPowerToPrice = ((float)consumerList[nextIndex].Data.TotalIncome) / consumerList[nextIndex].Data.powerRequired;
                if(nextPowerToPrice > powerToPrice)
                {
                    greaterThanNext = false;
                }
            }
            if(greaterThanNext && lessThanPrev)
            {
                min = index;
                max = index;
            }
            else if(greaterThanNext)
            {
                max = index;
            }    
            else
            {
                min = index + 1;
            }
        }
        consumerList.Insert(min, consumerBuilding);
    }

    public bool EnoughPower(Building checkedBuilding)
    {
        HashSet<Building> buildingsSeen = new HashSet<Building>();
        List<Building> connectedBuildings = new List<Building>();
        List<Building> connectedConsumers = new List<Building>();
        int totalPower = 0;
        buildingsSeen.Add(checkedBuilding);
        connectedBuildings.Add(checkedBuilding);
        while (connectedBuildings.Count > 0)
        {
            Building building = connectedBuildings[0];
            connectedBuildings.RemoveAt(0);
            if (building.Data.buildingType == BuildingType.PowerProducer)
            {
                totalPower += GetPower(building);
                building.PowerToGive = 0;
            }
            if (building.Data.buildingType == BuildingType.PowerConsumer)
            {
                AddConsumerToSortedList(building, connectedConsumers);
            }
            for (int i = 0; i < building.NumConnected; ++i)
            {
                Building connectedBuilding = building.GetConnectedBuilding(i);
                if (!buildingsSeen.Contains(connectedBuilding))
                {
                    buildingsSeen.Add(connectedBuilding);
                    connectedBuildings.Add(connectedBuilding);
                }
            }
        }
        foreach (Building consumer in connectedConsumers)
        {
            if(consumer.IsPowered)
            {
                totalPower -= consumer.Data.powerRequired;
            }
        }
        return (totalPower >= checkedBuilding.Data.powerRequired);
    }

    public bool ToggleBuildingPower(Building building)
    {
        if(building.IsPowered)
        {
            income -= building.Data.income;
            scienceIncome -= building.Data.scienceIncome;
            building.IsPowered = false;
            if (controller != null)
            {
                controller.UpdateCashAndIncome(cash, income);
                controller.UpdateScienceLabels(scienceHeld, scienceIncome);
            }
            return (true);
        }
        else
        {
            if(EnoughPower(building))
            {
                income += building.Data.income;
                scienceIncome += building.Data.scienceIncome;
                building.IsPowered = true;
                if (controller != null)
                {
                    controller.UpdateCashAndIncome(cash, income);
                    controller.UpdateScienceLabels(scienceHeld, scienceIncome);
                }
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }

    public int GetPower(Building building)
    {
        if(building.Data.resourceAmountRequired <= 0)
        {
            return (building.Data.powerProduced);
        }
        return (Mathf.CeilToInt(((float)building.ResourcesProvided) / building.Data.resourceAmountRequired * building.Data.powerProduced));
    }

	/// <summary>
	/// Gets the grid group data for the buildings with
	/// a given group id.
	/// </summary>
	/// <param name="groupId">The id of the grid group.</param>
	/// <returns>Data about the grid group.</returns>
	public GridGroupData GetGroupData(int groupId)
	{
		Building firstBuilding = GetFirstBuildingWithGroupId(groupId);
		return GetGroupData(firstBuilding);
	}

	/// <summary>
	/// Gets the data for the grid group that the passed building belongs to.
	/// </summary>
	/// <param name="groupBuilding">A building that is a member of a grid group.</param>
	/// <returns>Data about the building's grid group.</returns>
	public GridGroupData GetGroupData(Building groupBuilding)
	{
		// TODO: Instead of using DFS, maybe store the grid groups in a dictionary after performing AssignGroupIds
		// e.g. Dictionary<int, GridGroup> gridGroups; Where GridGroup has an member
		// Building[] GridGroup.buildings;
		// Then in here do gridGroups[gridGroupId].
		IEnumerable<Building> connectedBuildings = GetConnectedBuildings(groupBuilding);
		return new GridGroupData(connectedBuildings);
	}

	/// <summary>
	/// Finds a building with the given group id.
	/// Throws an exception if none were found.
	/// </summary>
	/// <param name="groupId">The groupId to search for.</param>
	/// <returns>The first buliding found with that groupId.</returns>
	private Building GetFirstBuildingWithGroupId(int groupId)
	{
		Building foundBuilding = buildings.Find((Building building) => building.BuildingGroup == groupId);

		if (foundBuilding == null)
			throw new System.Exception($"Could not find building with group {groupId}.");

		return foundBuilding;
	}

	/// <summary>
	/// Gets all the buildings that are connected to the passed building, including
	/// the passed building. Uses Depth-first search.
	/// </summary>
	/// <param name="startingBuilding">The building who is connected to other buildings that we want to fetch.</param>
	/// <returns>A set of all the buildings that the passed building is connected to.</returns>
	private HashSet<Building> GetConnectedBuildings(Building startingBuilding)
	{
		// "Visited" in DFS.
		HashSet<Building> buildingsSeen = new HashSet<Building>();
		// "To Visit" in DFS.
		List<Building> connectedBuildings = new List<Building>();
		
		buildingsSeen.Add(startingBuilding);
		connectedBuildings.Add(startingBuilding);
		
		while (connectedBuildings.Count > 0)
		{
			Building building = connectedBuildings[0];
			connectedBuildings.RemoveAt(0);
			
			for (int i = 0; i < building.NumConnected; ++i)
			{
				Building connectedBuilding = building.GetConnectedBuilding(i);
				if (!buildingsSeen.Contains(connectedBuilding))
				{
					buildingsSeen.Add(connectedBuilding);
					connectedBuildings.Add(connectedBuilding);
				}
			}
		}
		return buildingsSeen;
	}

    public void AssignGroupIds(Building groupBuilding, int groupId)
    {
        HashSet<Building> buildingsSeen = new HashSet<Building>();
        List<Building> connectedBuildings = new List<Building>();
        buildingsSeen.Add(groupBuilding);
        connectedBuildings.Add(groupBuilding);
        while (connectedBuildings.Count > 0)
        {
            Building building = connectedBuildings[0];
            connectedBuildings.RemoveAt(0);

            int oldId = building.BuildingGroup;
            if(oldId != groupId)
            {
                building.BuildingGroup = groupId;
                if(oldId >= 0 && !freeGroupIds.Contains(oldId))
                {
                    freeGroupIds.Add(oldId);
                }
            }

            for (int i = 0; i < building.NumConnected; ++i)
            {
                Building connectedBuilding = building.GetConnectedBuilding(i);
                if (!buildingsSeen.Contains(connectedBuilding))
                {
                    buildingsSeen.Add(connectedBuilding);
                    connectedBuildings.Add(connectedBuilding);
                }
            }
        }
    }

    public void ReassignAllGroups()
    {
        freeGroupIds.Clear();
        nextGroupId = 0;
        for(int i = 0; i < buildings.Count; ++i)
        {
            buildings[i].BuildingGroup = -1;
        }
        for(int i = 0; i < buildings.Count; ++i)
        {
            if (buildings[i].BuildingGroup < 0)
            {
                AssignGroupIds(buildings[i], GetFreeGroupId());
            }
        }
    }

    private int GetFreeGroupId()
    {
        int id = nextGroupId;
        if(freeGroupIds.Count > 0)
        {
            id = freeGroupIds.Min;
            freeGroupIds.Remove(id);
        }
        else
        {
            ++nextGroupId;
        }
        return (id);
    }
}
