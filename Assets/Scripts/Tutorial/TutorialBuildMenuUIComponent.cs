using UnityEngine;
using UnityEngine.UIElements;

public class TutorialBuildMenuUIComponent : BuildMenuUIComponent
{
    [SerializeField] private TutorialGameController tutorialController;

	// When a cable is clicked, only allow entering cable mode if cables are allowed.
	protected override void OnCableClicked()
	{
		if (tutorialController.cablesAreAllowed())
		{
			base.OnCableClicked();
		}
	}

	// Prevent all demolishing in tutorials.
	protected override void OnDemolishClicked()
	{
	}
}
