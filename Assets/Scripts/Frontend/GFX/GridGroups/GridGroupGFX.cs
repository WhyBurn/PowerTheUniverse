using UnityEngine;

public static class GridGroupGFX
{
	public static Color[] GridGroupColors { get => PtUUISettings.GetOrCreateSettings().GridGroupColors; }

	private const string GRID_GROUP_COLOR_PARAMETER = "_GridGroupColor";

	private const string IS_GRID_ELEMENT_PARAMETER = "_IsGridElement";

	public static Color GridGroupToColor(int gridGroup)
	{
		if (gridGroup < 0)
		{
			Debug.LogWarning($"Asked to pick color for negative grid group {gridGroup}. Using default color.");
			gridGroup = 0;		
		}

		int gridGroupColorIndex = gridGroup % GridGroupColors.Length;
		
		return GridGroupColors[gridGroupColorIndex];
	}

	public static Material SetMaterialGridGroupColor(Material material, int gridGroup)
	{
		material.SetColor(Shader.PropertyToID(GRID_GROUP_COLOR_PARAMETER), GridGroupToColor(gridGroup));
		return material;
	}

	public static Material SetMaterialIsGridElement(Material material, bool isGridGroupElement)
	{
		material.SetFloat(Shader.PropertyToID(IS_GRID_ELEMENT_PARAMETER), isGridGroupElement? 1f : 0f);
		return material;
	}
}
