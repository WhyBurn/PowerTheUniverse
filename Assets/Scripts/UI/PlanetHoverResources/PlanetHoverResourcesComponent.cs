using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

// IMPORTANT BUG: USING SETACTIVE(false) ON THE GAMEOBJECT WILL MAKE IT SO THAT YOU CANNOT UPDATE UI DOCUMENT ELEMENTS!!!
// DON'T EVER DO THIS FOR ANY UIDOCUMENT. USE style.display = DisplayStyles.None!!!
public class PlanetHoverResourcesComponent : WorldToScreenUIComponent
{
	[SerializeField] private SelectionComponent selection;
	private SpecialResourcesContainerUIBinding resourcesBinding;

	private PlanetComponent currentPlanet = null;

	private bool IsShowingPlanetResources { get => currentPlanet != null; }

	// Used for managing visual bug with moving UI documents on the same frame that they're shown.
	private bool hidThisFrame = false;
	

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		UIDocument specialResourcesUIDocument = uiDocument;
		resourcesBinding = new SpecialResourcesContainerUIBinding(specialResourcesUIDocument.rootVisualElement);

		// When selections and hovers are made / done, update the resources container and show / hide.
		selection.OnHoverEnter.AddListener(Inspector_OnHoverEnter);
		selection.OnSelectStart.AddListener(Inspector_OnSelectStart);
		selection.OnHoverExit.AddListener(Inspector_OnHoverExit);
		selection.OnSelectEnd.AddListener(Inspector_OnSelectEnd);
	}

	protected override void Start()
	{
		base.Start();
		Hide();
	}

	private void Inspector_OnHoverEnter(IInspectableComponent potentialPlanet)
	{
		CheckForPlanetAndBind(potentialPlanet);
	}

	private void Inspector_OnSelectStart(IInspectableComponent potentialPlanet)
	{
		CheckForPlanetAndBind(potentialPlanet);
	}

	private void Inspector_OnHoverExit(IInspectableComponent potentialPlanet)
	{
		// If we're exiting a hover, there's no point in checking if the hover is the planet we're currently bound to.
		// Either the last hover was the planet we're bound to, or we're not currently bound to anything.
		if (IsShowingPlanetResources)
			HideAndUnbindFromPlanet();
	}

	private void Inspector_OnSelectEnd(IInspectableComponent potentialPlanet)
	{
		// If a selection has ended, there's no point in checking if the selection is the planet we're currently bound to.
		// Either the last selection was the planet we're bound to, or we're not currently bound to anything.
		if (IsShowingPlanetResources)
			HideAndUnbindFromPlanet();
	}

	private void CheckForPlanetAndBind(IInspectableComponent potentialPlanet)
	{
		if (potentialPlanet is PlanetComponent)
		{
			BindToPlanet(potentialPlanet as PlanetComponent);
		}
	}

	private void BindToPlanet(PlanetComponent toBind)
	{
		currentPlanet = toBind;
		currentPlanet.OnPlanetDemolished.AddListener(BoundPlanet_OnDemolish);
		UpdateAll();

		// Because of a problem with ui documents moving on the same frame that they're shown,
		// if we weren't hidden just this frame, then we want to delay the showing of the resource
		// ui so that it's in place before rendering.
		if (hidThisFrame)
			Show();
		else
			StartCoroutine(DelayedShow());
	}

	private IEnumerator DelayedShow()
	{
		yield return new WaitForEndOfFrame();
		if (IsShowingPlanetResources)
			Show();
	}

	private void BoundPlanet_OnDemolish(PlanetComponent arg0)
	{
		HideAndUnbindFromPlanet();
	}

	private void HideAndUnbindFromPlanet()
	{
		Hide();
		if (currentPlanet != null)
			currentPlanet.OnPlanetDemolished.RemoveListener(BoundPlanet_OnDemolish);
		currentPlanet = null;
	}

	private void Show()
	{
		ui.style.display = DisplayStyle.Flex;
	}

	private void Hide()
	{
		ui.style.display = DisplayStyle.None;
		hidThisFrame = IsShowingPlanetResources;
	}

	private void Update()
	{
		if (IsShowingPlanetResources)		
			UpdateAll();

		if (hidThisFrame)
			hidThisFrame = false;
	}

	private void UpdateAll()
	{
		UpdatePosition();
		UpdateResourcesContainer();
		UpdateUIPosition();
	}

	private void UpdatePosition()
	{
		transform.position = currentPlanet.transform.position;
	}

	private void UpdateResourcesContainer()
	{
		resourcesBinding.ShowPlanetResources(currentPlanet);
	}
}
