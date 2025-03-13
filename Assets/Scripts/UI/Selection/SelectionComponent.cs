using UnityEngine;
using werignac.Utils;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

public class SelectionComponent : MonoBehaviour
{
	private struct SelectionChanges
	{
		// Reference to an object that is no longer being hovered over.
		public IInspectableComponent stopHovering;
		// Whether we need to make a call to the currentHover to notify that it's been hovered.
		public bool startHovering;
		// Reference to an object that is no longer being selected.
		public IInspectableComponent stopSelection;
		// Whether we need to make a call to the currentSelection to notify that it's been selected.
		public bool startSelection;

		// If the player has stopped hovering over something, notify the previoulsy hovered object.
		// If we're also stopping a selection, there's no need notify about stopping a hover because we have already
		// notified the object when the selection started, or this object was only hovered over whilst another
		// object was selected.
		public bool BroadcastStopHovering { get => stopHovering != null && stopSelection == null; }
		// Only notify that hovering has started if we're not also starting a selection.
		public bool BroadcastStartHovering { get => startHovering && ! startSelection; }
		// Notify a selection has stopped if there was a previously selected object.
		public bool BroadcastStopSelection { get => stopSelection != null; }
		public bool BroadcastStartSelection { get => startSelection; }

		// Get whether there were any changes at all.
		public bool AnyChange { get =>
				BroadcastStopHovering ||
				BroadcastStartHovering ||
				BroadcastStopSelection ||
				BroadcastStartSelection;
		}
	}

	[SerializeField] private SelectionCursorComponent cursor;
	[SerializeField] InspectorUIComponent inspector;

	private IInspectableComponent currentHover = null;
	private IInspectableComponent currentSelection = null;

	/// <summary>
	/// The current layer being shown in the inspector.
	/// </summary>
	private InspectorLayer activeInspectorLayer = null;
	
	/// <summary>
	/// Whether visuals about the selection should be hidden.
	/// This does not change the references stored in currentHover
	/// and currentSelection, which is different from ClearSelection().
	/// </summary>
	private bool muted = false;

	/// <summary>
	/// Whether to perform a selection this update.
	/// </summary>
	private bool cursorSelectThisUpdate = false;

	// Events
	
	// Invoked when an object starts being hovered for selection.
	// Skipped if hovering & selecting on the same update.
	[HideInInspector] public UnityEvent<IInspectableComponent> OnHoverEnter = new UnityEvent<IInspectableComponent>();
	// Invoked when an object stops being hovered for selection.
	[HideInInspector] public UnityEvent<IInspectableComponent> OnHoverExit = new UnityEvent<IInspectableComponent>();
	// Invoked when an object is selected.
	[HideInInspector] public UnityEvent<IInspectableComponent> OnSelectStart = new UnityEvent<IInspectableComponent>();
	// Invoked when an object is no longer selected.
	[HideInInspector] public UnityEvent<IInspectableComponent> OnSelectEnd = new UnityEvent<IInspectableComponent>();

	/// <summary>
	/// Called by the input manager.
	/// Performs a selection when UpdateSelection is called.
	/// </summary>
	public void CursorSelectThisUpdate()
	{
		cursorSelectThisUpdate = true;
	}

	private void Update()
	{
		SelectionChanges changes = new SelectionChanges();

		// Check for hovering or selected objects being destroyed.
		ValidateHoverAndSelection(ref changes);

		UpdateHover(ref changes);
		UpdateSelect(ref changes);

		if (changes.AnyChange)
			BroadcastChange(changes);
	}


	/// <summary>
	/// Updates currentHover based on the state of the
	/// cursor.
	/// </summary>
	/// <param name="changes">Struct storing changes made to selections. Modified by this function.</param>
	private void UpdateHover(ref SelectionChanges changes)
	{
		// If the cursor is over UI, no gameobjects are being hovered over.
		if (EventSystem.current.IsPointerOverGameObject())
		{
			if (currentHover == null)
				return;
			
			// Only broadcast hover changes whilst there is no selection.
			if (currentSelection == null)
				changes.stopHovering = currentHover;

			currentHover = null;
			return;
		}
		
		// See what the cursor is hovering over (may be null).
		IInspectableComponent newHover = GetCursorInspectable();
		if (currentHover != newHover)
		{
			Debug.Log($"Old Hover was {currentHover} new hover is {newHover}.");

			// Only broadcast hover changes whilst there is no selection.
			if (currentSelection == null)
			{
				changes.stopHovering = currentHover;
				changes.startHovering = newHover != null;
			}

			currentHover = newHover;
		}
	}

	/// <summary>
	/// Updates currentSelection if selectThisUpdate was called.
	/// </summary>
	/// <returns>Whether currentSelection was changed.</returns>
	private void UpdateSelect(ref SelectionChanges changes)
	{
		// If there was no selection input, don't change the selection.
		if (!cursorSelectThisUpdate)
			return;

		// Otherwise, take what we were hovering over and make that the
		// new selection.
		IInspectableComponent priorSelection = currentSelection;
		// Make a selection without broadcasting. Since this is in update,
		// it is assumed broadcasting will take place after the call to this
		// function.
		bool selectionChanged = Select(currentHover, false);

		// Store changes.
		if (selectionChanged)
		{
			changes.stopSelection = priorSelection;
			changes.startSelection = currentSelection != null;

			// If we were previously hovering over an object for more than
			// one update, that hovered object got a notification about hovering starting.
			// Now we need to tell it that hovering has stopped.
			if (currentSelection != null && priorSelection == null && !changes.startHovering)
			{
				changes.stopHovering = currentHover;
			}

			// NOTE: At this point, currentSelection = currentHover. If currentSelection is now null,
			// we don't need to send any notifications for starting to hover over an object because
			// currentHover is also null.
			// This is not the case for when Select is called externally because we don't know where the
			// cursor is then / what it might be hovering over.
		}

		// Reset cursorSelectThisUpdate.
		cursorSelectThisUpdate = false;
	}

	/// <summary>
	/// Function for selecting objects externally.
	/// </summary>
	/// <param name="toSelect">Object to select.</param>
	public void Select(IInspectableComponent toSelect)
	{
		Select(IsDestroyed(toSelect) ? null : toSelect, true);
	}

	/// <summary>
	/// Function for clearing the selection externally.
	/// </summary>
	public void ClearSelection()
	{
		Select(null);
	}

	/// <summary>
	/// Function for selecting objecs internally.
	/// </summary>
	/// <param name="toSelect">Object to select.</param>
	/// <param name="broadcastChange">Whether to make OnHoverEnd, OnSelectionStart, and inspector layer changes immediately.</param>
	/// <returns> Whether this is a new selection.</returns>
	private bool Select(IInspectableComponent toSelect, bool broadcastChange)
	{
		IInspectableComponent priorSelection = currentSelection;
		bool hadPriorSelection = priorSelection != null;

		if (toSelect == currentSelection)
			return false;

		currentSelection = toSelect;

		if (broadcastChange)
		{
			BroadcastChange(new SelectionChanges
			{
				// If we had something previously selected, no updates need to be made to
				// currentHover.
				// If we had nothing selected, and currentHover != null, then we need to notify
				// currentHover that it's no longer being hovered over for selection.
				stopHovering = hadPriorSelection ? null : currentHover,
				// Notify of hovering if we're hovering over something.
				startHovering = currentHover != null,
				// If we had something previously selected, we need to notify it that it is
				// no longer being selected.
				stopSelection = priorSelection,
				startSelection = toSelect != null
			});
		}

		return true;
	}

	/// <summary>
	/// Notify object that their selection status has changed.
	/// Also, update the inspector.
	/// </summary>
	/// <param name="changes">The changes that determine how objects should be notified.</param>
	private void BroadcastChange(SelectionChanges changes)
	{
		// Tell the relavant IInspectableComponents that their
		// statuses have changed.
		// Note that if currentHover or currentSelection are null,
		// their boolean checks should be false. But, currentHover and currentSelection
		// can be destroyed components that are normally considered null.
		if (changes.BroadcastStopHovering)
		{
			Debug.Log($"Stopped hovering on {changes.stopHovering}.");
			if (! IsDestroyed(changes.stopHovering))
				changes.stopHovering.OnHoverExit();
			OnHoverExit?.Invoke(changes.stopHovering);
		}
		if (changes.BroadcastStartHovering)
		{
			Debug.Log($"Started hovering on {currentHover}.");
			if (! IsDestroyed(currentHover))
				currentHover.OnHoverEnter();
			OnHoverEnter?.Invoke(currentHover);
		}
		if (changes.BroadcastStopSelection)
		{
			Debug.Log($"Stopped selecting {changes.stopSelection}.");
			if (! IsDestroyed(changes.stopSelection))
				changes.stopSelection.OnSelectEnd();
			OnSelectEnd?.Invoke(changes.stopSelection);
		}
		if (changes.BroadcastStartSelection)
		{
			Debug.Log($"Started selecting {currentSelection}.");
			if (! IsDestroyed(currentSelection))
				currentSelection.OnSelectStart();
			OnSelectStart?.Invoke(currentSelection);
		}

		// Assumed at this point that some meaningful change as occurred.

		// Update the inspector to reflect the newly selected / hovered object.
		UpdateInspector();
	}

	/// <summary>
	/// Updates the inspector to use a layer of the selected or hovered object.
	/// </summary>
	private void UpdateInspector()
	{
		// Remove the old layer.
		if (activeInspectorLayer != null)
			inspector.RemoveLayer(activeInspectorLayer);

		// Since we removed the layer, there is now no active inspector layer.
		activeInspectorLayer = null;

		// Selections take precedence over hovering for the inspector.
		if (currentSelection != null)
		{
			activeInspectorLayer = inspector.AddLayer(currentSelection, InspectorLayerType.SELECT);
			return;
		}

		if (currentHover != null)
		{
			activeInspectorLayer = inspector.AddLayer(currentHover, InspectorLayerType.SELECT);
		}
	}

	private void ValidateHoverAndSelection(ref SelectionChanges changes)
	{
		if (IsDestroyed(currentHover))
		{
			if (currentSelection != null)
				changes.stopHovering = currentHover;
			currentHover = null;
		}

		if (IsDestroyed(currentSelection))
		{
			changes.stopSelection = currentSelection;
			currentSelection = null;
		}
	}

	/// <summary>
	/// Gets whether an inspectable component has been destroyed
	/// (not standard null).
	/// </summary>
	/// <param name="component">The component to check.</param>
	/// <returns>Whether the component has been destroyed.</returns>
	private static bool IsDestroyed(IInspectableComponent component)
	{
		return component != null && (component as Component) == null;
	}

	/// <summary>
	/// Get the inspectable component under the cursor.
	/// If there is none, return null.
	/// </summary>
	/// <returns>An inspectable component under the cursor or null.</returns>
	private IInspectableComponent GetCursorInspectable()
	{
		Collider2D foundInspectableCollider = cursor.FindFirstByPredicate(
			(Collider2D collider) =>
			{
				return collider.TryGetComponentInParent<IInspectableComponent>(out IInspectableComponent _);
			}
		);

		return foundInspectableCollider == null ? null : foundInspectableCollider.GetComponentInParent<IInspectableComponent>();
	}
}
