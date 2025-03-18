using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.UI.Tooltips
{
	public class UIDocumentTooltipController : ITooltipUIController
	{
		private const string TOOLTIP_LABEL_ELEMENT_NAME = "TooltipLabel";

		private string tooltipText;
		private Label labelElement;

		public void SetText(string tooltipText)
		{
			this.tooltipText = tooltipText;

			if (labelElement != null)
				labelElement.text = tooltipText;
		}

		public void Bind(VisualElement ui)
		{
			labelElement = ui.Q<Label>(TOOLTIP_LABEL_ELEMENT_NAME);

			Debug.Assert(labelElement != null, $"Missing child {TOOLTIP_LABEL_ELEMENT_NAME} from tooltip VisualElement {labelElement}.");

			labelElement.text = tooltipText;
		}

		public void UnBind()
		{
			labelElement = null;
		}
	}
}
