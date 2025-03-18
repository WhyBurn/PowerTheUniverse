using UnityEngine;


namespace AstralAvarice.Tutorial
{
	/// <summary>
	/// Interface to be implemented by UI element bindings. Used to show / hide highlights
	/// in the tutorial.
	/// </summary>
	public interface IHighlightable
	{
		protected static string HIGHLIGHT_CLASS { get => PtUUISettings.GetOrCreateSettings().HighlightClass; }

		/// <summary>
		/// Show a halo around the UI element. Used in the tutorial.
		/// </summary>
		public void ShowHighlight();

		/// <summary>
		/// Hide a halo around the UI element.
		/// </summary>
		public void HideHighlight();
	}
}
