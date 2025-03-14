using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AstralAvarice.UI.Tooltip
{
	public class UIDocumentTooltipsComponent : MonoBehaviour
	{
		[SerializeField] private TooltipComponent tooltip;

		/// <summary>
		/// Reference to a  UI document with a reference to the panel
		/// to check.
		/// </summary>
		[SerializeField] private UIDocument panelDocument;

		private IRuntimePanel panel;

		private UIDocumentTooltipController uiController = new UIDocumentTooltipController();

		private TooltipLayer activeLayer = null;

		[SerializeField] private TooltipLayerFactory_SO layerFactory;


		private void Start()
		{
			panel = panelDocument.runtimePanel;
		}

		private void Update()
		{
			Vector2 screenPosition = GetScreenPosition();

			VisualElement hoveringElement = panel.Pick(screenPosition);

			if (TryGetTooltipText(hoveringElement, out string tooltipText))
			{
				if (activeLayer == null)
				{
					activeLayer = layerFactory.MakeLayer(uiController);
					tooltip.Add(activeLayer);
				}

				uiController.SetText(tooltipText);
			}
			else
			{
				if (activeLayer != null)
				{
					tooltip.Remove(activeLayer);
					activeLayer = null;
				}
			}
		}

		private Vector2 GetScreenPosition()
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();
			return new Vector2(mousePosition.x, Screen.height - mousePosition.y);
		}

		private bool TryGetTooltipText(VisualElement element, out string tooltipText)
		{
			if (element == null)
			{
				tooltipText = null;
				return false;
			}

			if (element.tooltip != null && element.tooltip.Length > 0)
			{
				tooltipText = element.tooltip;
				return true;
			}

			return TryGetTooltipText(element.parent, out tooltipText);
		}
	}
}
