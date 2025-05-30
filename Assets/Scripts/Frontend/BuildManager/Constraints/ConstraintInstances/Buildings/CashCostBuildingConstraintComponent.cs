using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class CashCostBuildingConstraintComponent : BuildingConstraintComponent
	{
		[SerializeField] private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			int buildingCashCost = state.buildState.toBuild.BuildingDataAsset.cost;
			int cashAfterPurchase = gameController.Cash - (state.priorCashCosts + buildingCashCost);
			bool sufficientCash = cashAfterPurchase >= 0;

			string postfix = "";
			BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;
			bool constraintTriggered = false;

			if (!sufficientCash)
			{
				postfix = $" (Missing ${Mathf.Abs(cashAfterPurchase)})";
				warningType = BuildWarning.WarningType.FATAL;
				constraintTriggered = true;
			}

			BuildWarning warning = new BuildWarning($"Cost ${buildingCashCost}{postfix}.", warningType);

			return new ConstraintQueryResult(constraintTriggered, warning);
		}
	}
}
