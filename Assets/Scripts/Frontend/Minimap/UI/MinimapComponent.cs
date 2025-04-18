using System;
using UnityEngine;
using UnityEngine.UIElements;

// In the future, this class may not be needed, as the minimap may be part of
// the UI where it cannot be shown and hidden.
public class MinimapComponent : MonoBehaviour
{
	private const string MINIMAP_CONTAINER_ELEMENT_NAME = "MinimapTemplate";
	private const string MINIMAP_ELEMENT_NAME = "Minimap";
	private const string MINIMAP_LEVEL_BOUNDS_ELEMENT_NAME = "LevelBounds";

	[SerializeField] private MinimapCameraComponent minimapCamera;
	[SerializeField] private CameraMovementComponent mainCamera;
	[SerializeField] private GameController gameController;

	private UIDocument uiDocument;

	private VisualElement minimapContainerElement;
	private VisualElement minimapElement;
	private VisualElement levelBoundsElement;

	/// <summary>
	/// Whether to continue moving the camera whilst the player is dragging accross
	/// the uielement.
	/// </summary>
	private bool continuousCameraMovement = false;

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		minimapContainerElement = uiDocument.rootVisualElement.Q(MINIMAP_CONTAINER_ELEMENT_NAME);
		minimapElement = uiDocument.rootVisualElement.Q(MINIMAP_ELEMENT_NAME);
		levelBoundsElement = minimapElement.Q(MINIMAP_LEVEL_BOUNDS_ELEMENT_NAME);

		minimapCamera.OnResizeForLevelBounds.AddListener(Camera_ResizeToLevelBounds);
	}

	private void Camera_ResizeToLevelBounds()
	{
		SetLevelBounds(gameController.LevelBounds);
	}

	private void SetLevelBounds(Vector2 levelBounds)
	{
		Vector2 bottomLeft = new Vector2(-levelBounds.x / 2, -levelBounds.y / 2);
		Vector2 topRight = -bottomLeft;

		Vector2 bottomLeftRelative = minimapCamera.WorldSpaceToMinimapSpace(bottomLeft);
		Vector2 topRightRelative = minimapCamera.WorldSpaceToMinimapSpace(topRight);

		levelBoundsElement.style.left = new Length(bottomLeftRelative.x * 100, LengthUnit.Percent);
		levelBoundsElement.style.bottom = new Length(bottomLeftRelative.y * 100, LengthUnit.Percent);
		levelBoundsElement.style.right = new Length((1 - topRightRelative.x) * 100, LengthUnit.Percent);
		levelBoundsElement.style.top = new Length((1 - topRightRelative.y) * 100, LengthUnit.Percent);
	}

	private void Start()
	{
		minimapElement.RegisterCallback<MouseMoveEvent>(Minimap_OnMouseMove);
		minimapElement.RegisterCallback<MouseDownEvent>(Minimap_OnMouseDown);
	}

	/// <summary>
	/// Called by the input system.
	/// Necessary as en external call because we want to stop tacking the mouse
	/// on any mouseup event (not just on MouseUp events on the minimapElement).
	/// </summary>
	public void StopTrackingMouseDrag()
	{
		continuousCameraMovement = false;
	}

	private void Minimap_OnMouseMove(MouseMoveEvent evt)
	{
		if (!continuousCameraMovement)
			return;

		Vector2 normalizedPosition = NormalizeLocalMousePosition(evt.localMousePosition);
		MoveMainCameraToMinimapPosition(normalizedPosition);
	}

	private void Minimap_OnMouseDown(MouseDownEvent evt)
	{
		Vector2 normalizedPosition = NormalizeLocalMousePosition(evt.localMousePosition);
		MoveMainCameraToMinimapPosition(normalizedPosition);

		continuousCameraMovement = true;
	}

	private Vector2 NormalizeLocalMousePosition(Vector2 localMousePosition)
	{
		Vector2 normalizedPosition = new Vector2(
			localMousePosition.x / minimapElement.resolvedStyle.width,
			localMousePosition.y / minimapElement.resolvedStyle.height
		);

		return normalizedPosition;
	}

	/// <summary>
	/// Takes a position on the minimap and teleports the main camera to that position.
	/// </summary>
	/// <param name="minimapPosition">Position in the minimap to teleport to. Normalized
	/// vector, where 0,0 is the top left.</param>
	private void MoveMainCameraToMinimapPosition(Vector2 minimapPosition)
	{
		Vector2 viewPortPosition = new Vector2(minimapPosition.x, 1 - minimapPosition.y);

		Vector2 mainCameraPosition = minimapCamera.MinimapSpaceToWorldSpace(viewPortPosition);

		mainCamera.MoveTo(mainCameraPosition);
	}

	public bool GetIsShowing()
	{
		return minimapContainerElement.resolvedStyle.display == DisplayStyle.Flex;
	}

    public void Show()
	{
		minimapContainerElement.style.display = DisplayStyle.Flex;
	}

	public void Hide()
	{
		minimapContainerElement.style.display = DisplayStyle.None;
	}

	public void Toggle()
	{
		if (GetIsShowing())
			Hide();
		else
			Show();
	}
}
