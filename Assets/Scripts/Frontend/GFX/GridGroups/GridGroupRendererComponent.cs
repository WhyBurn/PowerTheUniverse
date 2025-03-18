using UnityEngine;
using UnityEngine.SceneManagement;

public class GridGroupRendererComponent : MonoBehaviour
{
	private IGridGroupElement gridGroupElement;

	[SerializeField] protected Renderer[] renderers;

	private void Awake()
	{
		gridGroupElement = GetComponentInParent<IGridGroupElement>();
	}

	private void LateUpdate()
	{
		if (SceneManager.GetActiveScene().name == "LevelBuilder")
			return;
		
		SetRendererMaterials();
	}

	protected virtual void SetRendererMaterials()
	{
		int gridGroup = GetGridGroup();

		foreach(Renderer renderer in renderers)
		{
			// Note .material and note .sharedMaterial for separate parameters.
			renderer.material = GridGroupGFX.SetMaterialGridGroupColor(renderer.material, gridGroup);
		}
	}

	protected int GetGridGroup()
	{
		return gridGroupElement.GridGroup;
	}
}
