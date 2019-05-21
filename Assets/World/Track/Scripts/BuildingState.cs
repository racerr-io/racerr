using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class BuildingState : MonoBehaviour
{
    bool raycastIsHitting = false;
    bool isTransparent = false;

    [SerializeField] Material transparentMaterial;
    [SerializeField] Mesh transparentMesh;

    Material originalMaterial;
    Mesh originalMesh;

    MeshFilter buildingMeshFilter;
    MeshRenderer buildingMeshRenderer;
    
    List<GameObject> activeChildren = new List<GameObject>();

    /// <summary>
    /// Saves original mesh renderer and filter. Also keeps track of currently active children.
    /// </summary>
    void Start()
    {
        buildingMeshFilter = GetComponent<MeshFilter>();
        buildingMeshRenderer = GetComponent<MeshRenderer>();

        originalMesh = buildingMeshFilter.mesh;
        originalMaterial = buildingMeshRenderer.material;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.activeSelf)
            {
                activeChildren.Add(child);
            }
        }
    }

    /// <summary>
    /// Called every frame to update the transparent state of building.
    /// </summary>
    void Update()
    {
        if (raycastIsHitting && !isTransparent) // We have a ray hitting, need to make building transparent
        {
            isTransparent = true;
            MakeTransparent();
        }
        else if (!raycastIsHitting && isTransparent) // There is no ray, but building is transparent, make building back to original
        {
            RevertToOriginal();
            isTransparent = false;
        }

        raycastIsHitting = false; // Set to false, but BuildingTransparencyRaycaster should set to true on each frame before Update() called again
    }

    /// <summary>
    /// Make object transparent and active children inactive.
    /// </summary>
    void MakeTransparent()
    {
        buildingMeshFilter.mesh = transparentMesh;
        buildingMeshRenderer.material = transparentMaterial;

        activeChildren.ForEach(c => c.SetActive(false));
    }

    /// <summary>
    /// Reverts object back to 
    /// </summary>
    void RevertToOriginal()
    {
        buildingMeshFilter.mesh = originalMesh;
        buildingMeshRenderer.material = originalMaterial;

        activeChildren.ForEach(c => c.SetActive(true));
    }

    /// <summary>
    /// Sets raycastIsHitting a building to be true.
    /// </summary>
    public void HitRay()
    {
        raycastIsHitting = true;
    }
}