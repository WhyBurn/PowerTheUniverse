using UnityEngine;
using UnityEngine.UIElements;

// Whether to show blue for a standard resource UI or red for lacking resources.
public enum ResourceColorType { STANDARD, LACKING };
// Whether to show the resource UI with a divider "/" and total or without a divider "/" and total.
public enum ResourceDividerType { WITH_DIVIDER, WITHOUT_DIVIDER };

/// <summary>
/// Interface that defines the quantity and total setters for a SpecialResourceUIBinding.
/// Useful in the SpecialResourcesContainerUIBinding which controls the icon of
/// special resource UI elements and doesn't want others to mess with that.
/// </summary>
public interface IRestrictedSpecialResourceUIBinding
{
	void SetQuantity(int quantity);
	void SetTotal(int total);
	void SetColorType(ResourceColorType colorType);
	void SetDividerType(ResourceDividerType dividerType);
}

public class SpecialResourceUIBinding : IRestrictedSpecialResourceUIBinding
{
	private const string STANDARD_SPECIAL_RESOURCE_CLASS = "specialResourceItem";
	private const string LACKING_SPECIAL_RESOURCE_CLASS = "lackingSpecialResourceItem";

	private VisualElement root;
	private VisualElement resourceElement;
	private VisualElement resourceIcon;
	private Label quantityValue;
	private Label quantityDivider;
	private Label quantityTotal;

	public SpecialResourceUIBinding(VisualElement root)
	{
		this.root = root;
		resourceElement = root.Q("SpecialResourceItem");

		resourceIcon = root.Q("ResourceIcon");
		quantityValue = root.Q<Label>("QuantityValue");
		quantityDivider = root.Q<Label>("QuantityDivider");
		quantityTotal = root.Q<Label>("QuantityTotal");
	}

	/// <summary>
	/// Show the entire special resource UI element.
	/// </summary>
	public void Show()
	{
		root.style.display = DisplayStyle.Flex;
	}

	/// <summary>
	/// Hide the entire special resource UI element.
	/// </summary>
	public void Hide()
	{
		root.style.display = DisplayStyle.None;
	}

	/// <summary>
	/// Sets the icon of the special resource UI element to match the passed resource type.
	/// </summary>
	public void SetResourceIcon(ResourceType resourceType)
	{
		var resourceData = PtUUISettings.GetOrCreateSettings().GetSpecialResourceUIData(resourceType);
		SetResourceIcon(resourceData.Icon);
	}

	/// <summary>
	/// Sets the icon of the special resource UI element to be the passed sprite.
	/// </summary>
	private void SetResourceIcon(Sprite icon)
	{
		resourceIcon.style.backgroundImage = new StyleBackground(icon);
	}

	/// <summary>
	/// Sets the quantity of the special resouce UI element to match the passed int.
	/// This is the number that appears before the "/" in the UI element.
	/// </summary>
	public void SetQuantity(int quantity)
	{
		quantityValue.text = quantity.ToString();
	}

	/// <summary>
	/// Sets the total of the special resouce UI element to match the passed int.
	/// This is the number that appears after the "/" in the UI element.
	/// </summary>
	public void SetTotal(int total)
	{
		quantityTotal.text = total.ToString();
	}

	/// <summary>
	/// Sets whether the background for the resource ui is blue or red.
	/// Useful for when a building doesn't have enough resources to power itself,
	/// or when a planet is out of resources.
	/// </summary>
	/// <param name="colorType"></param>
	public void SetColorType(ResourceColorType colorType)
	{
		switch (colorType)
		{
			case ResourceColorType.STANDARD:
				if (!resourceElement.ClassListContains(STANDARD_SPECIAL_RESOURCE_CLASS))
					resourceElement.AddToClassList(STANDARD_SPECIAL_RESOURCE_CLASS);
				if (resourceElement.ClassListContains(LACKING_SPECIAL_RESOURCE_CLASS))
					resourceElement.RemoveFromClassList(LACKING_SPECIAL_RESOURCE_CLASS);
				break;
			case ResourceColorType.LACKING:
				if (!resourceElement.ClassListContains(LACKING_SPECIAL_RESOURCE_CLASS))
					resourceElement.AddToClassList(LACKING_SPECIAL_RESOURCE_CLASS);
				if (resourceElement.ClassListContains(STANDARD_SPECIAL_RESOURCE_CLASS))
					resourceElement.RemoveFromClassList(STANDARD_SPECIAL_RESOURCE_CLASS);
				break;
		}
	}

	/// <summary>
	/// Sets whether the resource ui has a divider "/" and a total.
	/// May be hidden for the build mode buttons which don't have an instance consuming resources.
	/// </summary>
	/// <param name="dividerType"></param>
	public void SetDividerType(ResourceDividerType dividerType)
	{
		DisplayStyle displayStyle = DisplayStyle.Flex;

		switch (dividerType)
		{
			case ResourceDividerType.WITH_DIVIDER:
				displayStyle = DisplayStyle.Flex;
				break;
			case ResourceDividerType.WITHOUT_DIVIDER:
				displayStyle = DisplayStyle.None;
				break;
		}

		quantityDivider.style.display = displayStyle;
		quantityTotal.style.display = displayStyle;
	}

	public void SetAll(
		ResourceType resourceType,
		int quanitity,
		int total,
		ResourceDividerType dividerType = ResourceDividerType.WITH_DIVIDER,
		ResourceColorType colorType = ResourceColorType.STANDARD
		)
	{
		SetResourceIcon(resourceType);
		SetQuantity(quanitity);
		SetTotal(total);
		SetDividerType(dividerType);
		SetColorType(colorType);
	}
}
