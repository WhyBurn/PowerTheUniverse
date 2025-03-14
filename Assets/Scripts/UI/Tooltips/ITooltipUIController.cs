using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.UI.Tooltip
{
	/// <summary>
	/// Interface for objects that control what is displayed in the
	/// ui for a tooltip.
	/// </summary>
	public interface ITooltipUIController
	{
		/// <summary>
		/// Bind to the passed ui. Find references to elements that will display
		/// information relevant to the hovered object.
		/// </summary>
		/// <param name="ui">The instance of ui to bind to.</param>
		public void Bind(VisualElement ui);
		
		/// <summary>
		/// Unbind to whatever ui the controller was previously bound to.
		/// </summary>
		public void UnBind();
	}
}
