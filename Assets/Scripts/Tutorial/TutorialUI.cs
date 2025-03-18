using UnityEngine;
using UnityEngine.UIElements;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset[] tutorialTrees;
    private int currentlyShown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentlyShown = -1;
        ShowNextElement();
    }

    public void ShowNextElement()
    {
        ++currentlyShown;
        if(currentlyShown < tutorialTrees.Length)
        {
            document.visualTreeAsset = tutorialTrees[currentlyShown];
        }
        else
        {
            document.visualTreeAsset = null;
        }
    }
}
