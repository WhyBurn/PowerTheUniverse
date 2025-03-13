using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManagerComponent : MonoBehaviour
{
    public static InputManagerComponent Instance { get; private set; }
	private InputAction acceptAction;
	private InputAction panAction;
	private InputAction cancelAction;
	private InputAction zoomAction;
	private InputAction incrementGameSpeedAction;
	private InputAction decrementGameSpeedAction;
	private InputAction redistributeElectricityAction;
	private InputAction pauseAction;
	private InputAction toggleMinimapAction;
	private InputAction directionalPanAction;
	private InputAction increaseDirectionalPanSpeed;
	private InputAction decreaseDirectionalPanSpeed;
	private InputAction toggleGridGroupViewAction;


	[SerializeField] private CameraMovementComponent cameraMovementComponent;
	[SerializeField] private SelectionCursorComponent selectionCursor;
	[SerializeField] private SelectionComponent selection;
	[SerializeField] private GameController gameController;
	[SerializeField] private PauseManager pauseManager;
	[SerializeField] private MinimapComponent minimap;
	[SerializeField] private GridGroupViewComponent gridGroupView;

	private void Awake()
	{
		// Singleton enforcement.
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		
		Instance = this;
	}

	private void Start()
	{
		acceptAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Accept"];
		panAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Pan"];
		cancelAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Cancel"];
		zoomAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Zoom"];
		incrementGameSpeedAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["IncrementGameSpeed"];
		decrementGameSpeedAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["DecrementGameSpeed"];
		redistributeElectricityAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["RedistributeElectricity"];
		pauseAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Pause"];
		toggleMinimapAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["ToggleMinimap"];
		directionalPanAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["DirectionalPan"];
		increaseDirectionalPanSpeed = PlayerInputSingletonComponent.Instance.Input.currentActionMap["IncreaseDirectionalPanSpeed"];
		decreaseDirectionalPanSpeed = PlayerInputSingletonComponent.Instance.Input.currentActionMap["DecreaseDirectionalPanSpeed"];
		toggleGridGroupViewAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["ToggleGridGroupView"];

		StartCoroutine(RefreshInputComponent());
	}

	// From https://discussions.unity.com/t/no-ui-input-actions-after-scene-load/814381/2
	private IEnumerator RefreshInputComponent()
	{
		yield return new WaitForEndOfFrame();
		PlayerInputSingletonComponent.Instance.Input.enabled = false;
		yield return new WaitForEndOfFrame();
		PlayerInputSingletonComponent.Instance.Input.enabled = true;
	}

	private void Update()
	{
		bool startInBuildState = BuildManagerComponent.Instance.IsInBuildState();

		UpdateSelectionCursor();
		UpdateBuildManager();
		if (! startInBuildState)
			UpdateSelection();
		UpdateCameraMovement();
		UpdateGameController();
		UpdatePauseManager();
		UpdateMinimap();
		UpdateGridGroupView();
	}

	private void UpdateSelectionCursor()
	{
		// Don't look for objects under the cursor whilst paused.
		if (gameController.GamePaused)
			return;

		// Convert the mouse position to a position in world space.
		Vector2 mousePositionWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		selectionCursor.SetPosition(mousePositionWorldSpace);
		selectionCursor.QueryHovering();
	}

	private void UpdateBuildManager()
	{
		// Don't take input whilst the game is paused.
		if (gameController.GamePaused)
			return;

		// If the player was clicking on UI, ignore the input.
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}

		if (acceptAction.WasPerformedThisFrame())
		{
			// If we're currently building, try to place something.
			if (BuildManagerComponent.Instance.IsInBuildState())
				BuildManagerComponent.Instance.SetPlace();
		}

		if (cancelAction.WasPerformedThisFrame())
		{
			if (BuildManagerComponent.Instance.IsInBuildState())
				BuildManagerComponent.Instance.CancelBuildState();
		}
	}

	private void UpdateSelection()
	{
		// Don't take input whilst the game is paused.
		if (gameController.GamePaused)
			return;

		// If the player was clicking on UI, ignore the input.
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		// Objects are not selectable whilst in a build mode.
		if (BuildManagerComponent.Instance.IsInBuildState())
			return;

		// Select the hovered object.
		if (acceptAction.WasPerformedThisFrame())
		{
			selection.CursorSelectThisUpdate();
		}

		// Get rid of the selected object (if there is one).
		if (cancelAction.WasPerformedThisFrame())
		{
			selection.ClearSelection();
		}
	}

	private void UpdateCameraMovement()
	{
		// Don't take input whilst the game is paused.
		if (gameController.GamePaused)
			return;

		if (cameraMovementComponent == null)
			return;
		
		

		cameraMovementComponent.SetHoverInput(Input.mousePosition);
		cameraMovementComponent.SetPanningInput(panAction.IsPressed());


		CameraMovementComponent.PanSpeedModifier panSpeedModifier = CameraMovementComponent.PanSpeedModifier.NONE;

		if (increaseDirectionalPanSpeed.IsPressed() && !decreaseDirectionalPanSpeed.IsPressed())
			panSpeedModifier = CameraMovementComponent.PanSpeedModifier.MULTIPLY;
		if (decreaseDirectionalPanSpeed.IsPressed() && !increaseDirectionalPanSpeed.IsPressed())
			panSpeedModifier = CameraMovementComponent.PanSpeedModifier.DIVIDE;

		Vector2 directionalPan = Vector2.ClampMagnitude(directionalPanAction.ReadValue<Vector2>(), 1);
		cameraMovementComponent.SetDirectionalPanInput(directionalPan, panSpeedModifier);
		
		// User may scroll to read build menu, don't zoom when this is the case.
		if (! EventSystem.current.IsPointerOverGameObject())
			cameraMovementComponent.SetZoomInput(zoomAction.ReadValue<float>());
	}

	private void UpdateGameController()
	{
		// Don't take input whilst the game is paused.
		if (gameController.GamePaused)
			return;

		if (incrementGameSpeedAction.WasPerformedThisFrame())
		{
			gameController.IncrementGameSpeed();
		}

		if (decrementGameSpeedAction.WasPerformedThisFrame())
		{
			gameController.DecrementGameSpeed();
		}

		if (redistributeElectricityAction.WasPerformedThisFrame())
		{
			gameController.RecomputeIncome();
		}
	}

	private void UpdatePauseManager()
	{
		// Don't block handling this input whilst paused.
		// If the user presses esc whilst in the pause menu, we should unpause.

		if (pauseManager == null)
		{
			Debug.LogError("InputManager is missing PauseManager reference.");
		}
		if (pauseAction.WasPerformedThisFrame())
		{
			if (gameController.GamePaused)
			{
				pauseManager.UnpauseGame();
			}
			else
			{
				pauseManager.PauseGame();
			}
			
		}
	}

	private void UpdateMinimap()
	{
		// Don't take input whilst the game is paused.
		if (gameController.GamePaused)
			return;

		if (minimap == null)
			return;

		if (toggleMinimapAction.WasPerformedThisFrame())
		{
			minimap.Toggle();
		}
	}

	private void UpdateGridGroupView()
	{
		if (gameController.GamePaused)
			return;

		if (gridGroupView == null)
			return;

		if (toggleGridGroupViewAction.WasPerformedThisFrame())
		{
			gridGroupView.Toggle();
		}
	}
}
