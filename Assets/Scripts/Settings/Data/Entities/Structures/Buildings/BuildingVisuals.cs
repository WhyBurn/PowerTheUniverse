using UnityEngine;
using AstralAvarice.VisualData;

[CreateAssetMenu(fileName = "BuildingVisuals", menuName = "Visual Info/Building")]
public class BuildingVisuals : ScriptableObject
{
	// Name to show in in-game inspector.
	public string buildingName;
	// Icon to show on buttons.
	public Sprite buildingIcon;
	// Descripton to show in inspector.
	public string buildingDescription;
	// Ghost sprite to use as building cursor.
	public Sprite buildingGhost;
	// Offset of the ghost relative to the building's origin.
	public Vector2 ghostOffset;
	// Scale of the ghost.
	public float ghostScale = 0.1681f;
	// Which categories this building belongs to.
	public BuildCategory[] categories;
}
