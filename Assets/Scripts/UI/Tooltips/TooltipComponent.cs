using UnityEngine;
using AstralAvarice.Utils.Layers;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System;
using UnityEngine.InputSystem;

namespace AstralAvarice.UI.Tooltip
{
	public class TooltipComponent : MonoBehaviour
	{
		private const string TOOLTIP_CONTAINER_ELEMENT_NAME = "TooltipContainer";

		/// <summary>
		/// Class used to store information about replacing the contents
		/// of a tooltip.
		/// </summary>
		private class LayerReplacement
		{
			public TooltipLayer oldLayer;
			public TooltipLayer newLayer;
		}

		private LayerContainer<TooltipLayer> tooltipLayers = new LayerContainer<TooltipLayer>();

		/// <summary>
		/// Null when the ui element for the tooltip doesn't need to be replaced.
		/// Not null for when the ui element for the tooltip needs to be replaced on LateUpdate.
		/// </summary>
		private LayerReplacement replacement = null;

		/// <summary>
		/// UI document containing the tooltip to be moved and updated with information.
		/// </summary>
		[SerializeField] private UIDocument uiDocument;

		/// <summary>
		/// The element that moves to follow the position of the mouse on screen.
		/// </summary>
		private VisualElement movingElement;

		private VisualElement tooltipContainer;

		/// <summary>
		/// Displacement between the mouse and the corner of the tooltip.
		/// Both values should be positive.
		/// </summary>
		[SerializeField] private Vector2 margin;
		
		private void Awake()
		{
			tooltipLayers.OnTopLayerChanged.AddListener(TooltipLayers_OnTopLayerChanged);
		}

		private void Start()
		{
			// Get a reference to the element we will be manipulating to follow the mouse.
			movingElement = uiDocument.rootVisualElement;
			// The position should be absolue, not relative to other elements at the same level with the same parent (though,
			// there shouldn't be any).
			movingElement.style.position = Position.Absolute;

			tooltipContainer = movingElement.Q(TOOLTIP_CONTAINER_ELEMENT_NAME);

			// Hide by default.
			Hide();
		}

		private void TooltipLayers_OnTopLayerChanged(TooltipLayer oldLayer, TooltipLayer newLayer)
		{
			StoreReplacement(oldLayer, newLayer);
		}

		/// <summary>
		/// Stores a layer replacement until LateUpdate.
		/// </summary>
		/// <param name="oldLayer">The old layer being replaced.</param>
		/// <param name="newLayer">The new layer being used.</param>
		private void StoreReplacement(TooltipLayer oldLayer, TooltipLayer newLayer)
		{
			// If this is the first replacement for this update, simply create
			// a new replacement object.
			if (replacement == null)
			{
				replacement = new LayerReplacement
				{
					oldLayer = oldLayer,
					newLayer = newLayer
				};

				return;
			}

			// If this is not the first replacement for this update, chain with
			// the existing replacement.

			Debug.Assert(replacement.newLayer == oldLayer, $"Chained layer replacement failed in tooltip. Expected layer {replacement.newLayer}, got {oldLayer}.");
			
			// Skip the passed oldLayer since it was just set to the be new layer
			// previously in the same update.
			replacement.newLayer = newLayer;
		}

		private void LateUpdate()
		{
			ProcessReplacement();
			UpdatePosition();
		}

		private void ProcessReplacement()
		{
			// If no replacements need to be made, don't do anything.
			if (replacement == null)
				return;
			
			// Remove and disconnect the old layer's controller.
			if (replacement.oldLayer != null)
			{
				if (replacement.oldLayer.HasUIController())
					replacement.oldLayer.UIController.UnBind();
			}

			// Add and connect the new layer.
			if (replacement.newLayer != null)
			{
				// Only replace the ui elements if the assets are different.
				if (replacement.oldLayer == null ||
					replacement.oldLayer.UIAsset != replacement.newLayer.UIAsset)
				{
					tooltipContainer.Clear();
					TemplateContainer newUI = replacement.newLayer.UIAsset.Instantiate();
					tooltipContainer.Add(newUI);
				}

				// If there is a new controller, bind it to the ui.
				if (replacement.newLayer.HasUIController())
					replacement.newLayer.UIController.Bind(tooltipContainer);

				Show();
			}
			else // If there are no layers, there is nothing to show.
			{
				tooltipContainer.Clear();
				Hide();
			}

			// Reset the replacement.
			replacement = null;
		}

		private void UpdatePosition()
		{
			Vector2 documentPosition = GetMouseDocumentPosition();
			documentPosition = ApplyMarginDirections(documentPosition, out bool aboveMouse, out bool leftOfMouse);
			SetPosition(documentPosition, aboveMouse, leftOfMouse);
		}

		private Vector2 GetMouseDocumentPosition()
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();
			return new Vector2(mousePosition.x, Screen.height - mousePosition.y);
		}

		private Vector2 ApplyMarginDirections(Vector2 documentPosition, out bool aboveMouse, out bool leftOfMouse)
		{
			Vector2 positionWithMargin = documentPosition + margin;
			aboveMouse = false;
			leftOfMouse = false;

			// If the top is over halway down the screen, check whether we would run off the screen and move above.
			bool pastVerticalHalf = documentPosition.y > Screen.height / 2;
			bool bottomOverflow = positionWithMargin.y + movingElement.resolvedStyle.height > Screen.height;
			if (pastVerticalHalf && bottomOverflow)
			{
				positionWithMargin.y = documentPosition.y - margin.y - movingElement.resolvedStyle.height;
			}

			// If the left is over halway down the screen, check whether we would run off the screen and move to the left.
			bool pastHorizontalHalf = documentPosition.x > Screen.width / 2;
			bool rightOverflow = positionWithMargin.x + movingElement.resolvedStyle.width > Screen.width;
			if (pastHorizontalHalf && rightOverflow)
			{
				positionWithMargin.x = documentPosition.x - margin.x - movingElement.resolvedStyle.width;
			}

			return positionWithMargin;
		}

		private void SetPosition(Vector2 position, bool aboveMouse, bool leftOfMouse)
		{
			movingElement.style.top = (aboveMouse) ? StyleKeyword.Auto : position.y;
			movingElement.style.bottom = (aboveMouse) ? position.y : StyleKeyword.Auto;
			movingElement.style.left = (leftOfMouse) ? StyleKeyword.Auto : position.x;
			movingElement.style.right = (leftOfMouse) ? position.x : StyleKeyword.Auto;
		}

		private void Hide()
		{
			movingElement.style.display = DisplayStyle.None;
		}

		private void Show()
		{
			movingElement.style.display = DisplayStyle.Flex;
		}

		public void Add(TooltipLayer layer)
		{
			tooltipLayers.Add(layer);
		}

		public void Remove(TooltipLayer layer)
		{
			tooltipLayers.Remove(layer);
		}
	}
}
