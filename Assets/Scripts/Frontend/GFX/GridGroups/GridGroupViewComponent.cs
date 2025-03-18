using UnityEngine;
using AstralAvarice.UI.Tooltips;

public class GridGroupViewComponent : MonoBehaviour
{
	private const string INTENSITY_PARAMETER = "_Intensity";
	[SerializeField] private Material gridGroupViewPostProcessingMaterial;
	[SerializeField] private bool clearOnAwake = true;

	[Header("Tooltips")]
	[SerializeField] private SelectionCursorComponent cursor;
	[SerializeField] private TooltipComponent tooltip;
	[SerializeField] private TooltipLayerFactory_SO tooltipLayerFactory;
	private TooltipLayer currentTooltipLayer = null;
	private GridGroupTooltipUIController tooltipController = new GridGroupTooltipUIController();

	[SerializeField] private GameController gameController;

	public bool IsShowing
	{
		get { return (GetIntensity() != 0); }
	}

	private void Awake()
	{
		if (clearOnAwake)
			Hide();
	}

	public void Toggle()
	{
		if (GetIntensity() == 0)
			Show();
		else
			Hide();
	}

	public void Show()
	{
		SetIntensity(1);
	}

	public void Hide()
	{
		SetIntensity(0);

		if (currentTooltipLayer != null)
		{
			tooltip.Remove(currentTooltipLayer);
			currentTooltipLayer = null;
		}
	}

	private void SetIntensity(float value)
	{
		gridGroupViewPostProcessingMaterial.SetFloat(Shader.PropertyToID(INTENSITY_PARAMETER), value);
	}

	private float GetIntensity()
	{
		return gridGroupViewPostProcessingMaterial.GetFloat(Shader.PropertyToID(INTENSITY_PARAMETER));
	}

	private void Update()
	{
		if (!IsShowing)
			return;
		
		UpdateTooltip();
	}

	/// <summary>
	/// Updates the tooltip to show data about the grid group that the player
	/// is hovering over. Also hides the tooltip if the player isn't hovering
	/// over a grid group
	/// </summary>
	private void UpdateTooltip()
	{
		IGridGroupElement hoveringGridGroupElement = GetHoveringGridGroupElement();

		// If we're not hovering over anything, don't show anything in the tooltip.
		if (hoveringGridGroupElement == null)
		{
			if (currentTooltipLayer != null)
			{
				tooltip.Remove(currentTooltipLayer);
				currentTooltipLayer = null;
			}
			return;
		}

		// If we're hovering over soemthing, update / show the tooltip.
		GridGroupData data = gameController.GetGridGroupData(hoveringGridGroupElement.GridGroup);
		tooltipController.SetData(data);

		// Show the tooltip.
		if (currentTooltipLayer == null)
		{
			currentTooltipLayer = tooltipLayerFactory.MakeLayer(tooltipController);
			tooltip.Add(currentTooltipLayer);
		}
	}

	/// <summary>
	/// Gets the grid group element the player is hovering over.
	/// </summary>
	/// <returns>The grid group element the player is hovering over.</returns>
	private IGridGroupElement GetHoveringGridGroupElement()
	{
		Collider2D gridGroupElementCollider = cursor.FindFirstByPredicate(
			(Collider2D collider) => collider.GetComponentInParent<IGridGroupElement>() != null
			);

		if (gridGroupElementCollider == null)
			return null;

		return gridGroupElementCollider.GetComponentInParent<IGridGroupElement>();
	}

	private void OnDestroy()
	{
		SetIntensity(0);	
	}
}
