using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.Utils.Layers;

namespace AstralAvarice.UI.Tooltip
{
	/// <summary>
	/// Interface for interacting with tooltip layers.
	/// </summary>
	public interface ITooltipLayer
	{
		/// <summary>
		/// The UI to show in this layer.
		/// </summary>
		public VisualTreeAsset UIAsset { get; }

		/// <summary>
		/// An optional crontroller object that updates the UI.
		/// Must be notified by the TooltipComponent when the UI gets
		/// destroyed / when a new tooltip is used.
		/// </summary>
		public ITooltipUIController UIController { get; }
	}

	/// <summary>
	/// A TooltipLayer can be seen as a request to the TooltipComponent to display
	/// certain ui content.
	/// </summary>
	public class TooltipLayer : Layer, ITooltipLayer
	{
		private VisualTreeAsset uiAsset;
		private ITooltipUIController uiController;

		public VisualTreeAsset UIAsset => uiAsset;
		public ITooltipUIController UIController => uiController;

		public TooltipLayer(int priority, VisualTreeAsset uiAsset, ITooltipUIController uiController = null) :
			base(priority)
		{
			this.uiAsset = uiAsset;
			// It is assumed that the UIController has all the information it needs to update
			// the ui, except for a reference to the ui itself.
			this.uiController = uiController;
		}
	}

	public static class TooltipLayerExtentions
	{
		public static bool HasUIController(this ITooltipLayer tooltipLayer)
		{
			return tooltipLayer.UIController != null;
		}
	}
}
