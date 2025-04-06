using UnityEngine;
using UnityEngine.Events;
using werignac.Utils;
using AstralAvarice.Frontend;
using System;

public class CableComponent : MonoBehaviour, IDemolishable, IGridGroupElement
{
	[SerializeField] private BuildingComponent startBuilding;
	[SerializeField] private BuildingComponent endBuilding;

	[SerializeField] private LineRenderer lineRenderer;

	[SerializeField] private BoxCollider2D boxCollider;
	[SerializeField] private bool isPlayerDemolishable = false;

	public float CableOverlapTime { get; set; }

	// Events
	[HideInInspector] public UnityEvent OnCableMoved = new UnityEvent(); // Invoked when the cable is moved.
	[HideInInspector] public UnityEvent<CableComponent> OnCableDemolished = new UnityEvent<CableComponent>();
	[HideInInspector] public UnityEvent OnCableHoverStartForDemolish = new UnityEvent();
	[HideInInspector] public UnityEvent OnCableHoverEndForDemolish = new UnityEvent();
	[HideInInspector] public UnityEvent<int> OnGridGroupChanged { get; private set; } = new UnityEvent<int>();

	// Getters
	public float Length
	{
		get
		{
			return Vector2.Distance(
				startBuilding.CableConnectionTransform.position,
				endBuilding.CableConnectionTransform.position
				);
		}
	}

	public BuildingComponent Start
	{
		get => startBuilding;
	}

	public BuildingComponent End
	{
		get => endBuilding;
	}

	/// <summary>
	/// Which independent power grid this cable belongs to.
	/// </summary>
	public int GridGroup
	{
		get => (Start) ? Start.GridGroup : (End) ? End.GridGroup : -1;
	}

	private void Awake()
	{
#if UNITY_EDITOR
		if (lineRenderer == null)
			lineRenderer = GetComponentInChildren<LineRenderer>();

#endif
	}

	public void SetAttachedBuildings(BuildingComponent start, BuildingComponent end)
	{
		if (start == null)
			throw new System.NullReferenceException("Tried to set cable start to a null BuildingComponent");

		if (end == null)
			throw new System.NullReferenceException("Tried to set cable end to a null BuildingComponent");

		startBuilding = start;
		endBuilding = end;

		// When buildings are destroyed, we should destroy the cable as well.
		startBuilding.OnBuildingDemolished.AddListener(Building_OnDestroy);
		endBuilding.OnBuildingDemolished.AddListener(Building_OnDestroy);

		// When the buildings move, we should move the cable as well.
		startBuilding.GetComponent<CableConnectionPointMonitorComponent>().onConnectionPositionHasChanged.AddListener(Building_OnMove);
		endBuilding.GetComponent<CableConnectionPointMonitorComponent>().onConnectionPositionHasChanged.AddListener(Building_OnMove);

		// When either of the buildings change grid groups, notify others of the change to the grid group for the cable
		// via chained calls.
		// TODO: Prevent double calls to cables. This callback will always be called twice.
		startBuilding.OnGridGroupChanged.AddListener(OnGridGroupChanged.Invoke);
		endBuilding.OnGridGroupChanged.AddListener(OnGridGroupChanged.Invoke);
	}

	private void Building_OnMove()
	{
		// Invokes LateUpdate.
		enabled = true;
	}

	public static void GetBoxFromPoints(
		Vector2 point1,
		Vector2 point2,
		out Vector2 center,
		out Vector2 size,
		out float angle
		)
	{
		Vector2 difference = point1 - point2;
		// Get the middle of the two points.
		center = (point1 + point2) / 2;
		// Length of the cable + constant cable width.
		size = new Vector2(Vector2.Distance(point1, point2), GlobalBuildingSettings.GetOrCreateSettings().CableWidth);

		angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
	}

	private void LateUpdate()
	{
		UpdateLineRenderer();

		// Move the box collider to match the area the cable is taking up.
		GetBoxFromPoints(
			startBuilding.CableConnectionTransform.position,
			endBuilding.CableConnectionTransform.position,
			out Vector2 center,
			out Vector2 size,
			out float angle
			);

		boxCollider.transform.position = center;
		boxCollider.transform.localEulerAngles = Vector3.forward * angle;
		boxCollider.size = size;

		OnCableMoved.Invoke();

		// Disable the script so that we don't keep calling LateUpdate
		// until one of the buildings has moved.
		enabled = false;
	}

	public void UpdateLineRenderer()
	{
		Vector3[] positions = new Vector3[]
		{
			startBuilding.CableConnectionTransform.position,
			endBuilding.CableConnectionTransform.position
		};

		// TODO: Handle cable breaks.

		lineRenderer.SetPositions(positions);
	}

	// TODO: Use for breaks when objects move.
	private bool IsValidOverlap(Collider2D overlapping)
	{
		if (overlapping.TryGetComponentInParent(out BuildingComponent overllapingBuilding))
		{
			return (overllapingBuilding == Start) || (overllapingBuilding == End);
		}

		if (overlapping.TryGetComponentInParent(out CableComponent overlappingCable))
		{
			bool startIsConnectingBuilding = (overlappingCable.Start == Start) || (overlappingCable.Start == Start);
			bool endIsConnectingBuilding = (overlappingCable.End == Start) || (overlappingCable.End == End);

			return startIsConnectingBuilding || endIsConnectingBuilding;
		}

		return false;
	}

	private void Building_OnDestroy(BuildingComponent _)
	{
		Demolish();
	}

	public void Demolish()
	{
		OnCableDemolished?.Invoke(this);

		GameObject cableDestructionVFXObject = Instantiate(GlobalBuildingSettings.GetOrCreateSettings().CableDestructionVFXPrefab);
		CableDestructionVFXComponent cableDestructionVFXComponent = cableDestructionVFXObject.GetComponent<CableDestructionVFXComponent>();
		cableDestructionVFXComponent.RunVFX(this);


		Destroy(gameObject);
	}

	public void HoverDemolishStart()
	{
		OnCableHoverStartForDemolish?.Invoke();
	}

	public void HoverDemolishEnd()
	{
		OnCableHoverEndForDemolish?.Invoke();
	}

	public void SetDemolishable(bool isDemolishable)
	{
		isPlayerDemolishable = isDemolishable;
	}

	public bool Demolishable()
	{
		return isPlayerDemolishable;
	}
}
