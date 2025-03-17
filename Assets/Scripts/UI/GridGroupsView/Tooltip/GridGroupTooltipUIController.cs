using UnityEngine;
using AstralAvarice.UI.Tooltips;
using UnityEngine.UIElements;

public class GridGroupTooltipUIController : ITooltipUIController
{
	private const string GROUP_COLOR_ELEMENT_NAME = "GroupColor";
	private const string POWER_PRODUCED_LABEL_ELEMENT_NAME = "PowerProducedLabel";
	private const string POWER_CONSUMED_LABEL_ELEMENT_NAME = "PowerConsumedLabel";

	private GridGroupData dataToDisplay;

	private VisualElement ui;
	private VisualElement groupColorElement;
	private Label powerProducedLabelElement;
	private Label powerConsumedLabelElement;

	public void SetData(GridGroupData dataToDisplay)
	{
		if (dataToDisplay.Equals(this.dataToDisplay))
			return;

		this.dataToDisplay = dataToDisplay;

		if (ui != null)
			UpdateUI();
	}

	public void Bind(VisualElement ui)
	{
		this.ui = ui;

		groupColorElement = ui.Q(GROUP_COLOR_ELEMENT_NAME);
		powerProducedLabelElement = ui.Q<Label>(POWER_PRODUCED_LABEL_ELEMENT_NAME);
		powerConsumedLabelElement = ui.Q<Label>(POWER_CONSUMED_LABEL_ELEMENT_NAME);

		// Assume dataToDisplay is already set.
		UpdateUI();
	}

	public void UnBind()
	{
		ui = null;

		groupColorElement = null;
		powerProducedLabelElement = null;
		powerConsumedLabelElement = null;
	}

	private void UpdateUI()
	{
		Color groupColor = GridGroupGFX.GridGroupToColor(dataToDisplay.GroupId);
		groupColor.a = 1;
		groupColorElement.style.backgroundColor = groupColor;
		powerProducedLabelElement.text = dataToDisplay.TotalPowerProduced.ToString();
		powerConsumedLabelElement.text = dataToDisplay.TotalPowerConsumed.ToString();
	}
}
