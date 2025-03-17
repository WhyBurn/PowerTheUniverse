using UnityEngine;
using System.Collections.Generic;

public struct GridGroupData
{
	public int GroupId { get; private set; }
	public int TotalPowerProduced { get; private set; }
	public int TotalPowerConsumed { get; private set; }

	public GridGroupData(IEnumerable<Building> buildings)
	{
		GroupId = -1;
		TotalPowerProduced = 0;
		TotalPowerConsumed = 0;

		foreach (Building building in buildings)
			RecordBuilding(building);
	}

	public void RecordBuilding(Building building)
	{
		if (GroupId == -1)
			GroupId = building.BuildingGroup;

		TotalPowerProduced += building.Data.powerProduced;
		TotalPowerConsumed += building.Data.powerRequired;
	}
}
