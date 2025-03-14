using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.UI.Tooltip
{
	[CreateAssetMenu(fileName = "TooltipLayerFactory", menuName = "Tooltips/Layers/Factory")]
	public class TooltipLayerFactory_SO : ScriptableObject
	{
		[SerializeField] private int priority;
		[SerializeField] private VisualTreeAsset uiAsset;

		public TooltipLayer MakeLayer(ITooltipUIController controller = null)
		{
			return new TooltipLayer(priority, uiAsset, controller);
		}
	}
}
