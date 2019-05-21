using System.Collections.Generic;
using UnityEngine;

public class BuildingState : MonoBehaviour
{
    bool raycastIsHitting = false;
    bool isTransparent = false;

    [SerializeField] Material transparentMaterial;
    [SerializeField] Mesh transparentMesh;

    Material originalMaterial;
    Mesh originalMesh;

    MeshRenderer buildingMeshRenderer;
    MeshFilter buildingMeshFilter;

    List<GameObject> activeChildren = new List<GameObject>();

    void Start()
    {
        buildingMeshFilter = GetComponent<MeshFilter>();
        buildingMeshRenderer = GetComponent<MeshRenderer>();

        originalMaterial = buildingMeshRenderer.material;
        originalMesh = buildingMeshFilter.mesh;

        for (int i = 0; i < transform.childCount; i++)
        {
            activeChildren.Add(transform.GetChild(i).gameObject);
        }
    }
    void MakeTransparent()
    {
        buildingMeshFilter.mesh = transparentMesh;
        buildingMeshRenderer.material = transparentMaterial;

        activeChildren.ForEach(c => c.SetActive(false));
    }

    void RevertToOriginal()
    {
        buildingMeshFilter.mesh = originalMesh;
        buildingMeshRenderer.material = originalMaterial;

        activeChildren.ForEach(c => c.SetActive(true));
    }

    public void HitRay()
    {
        raycastIsHitting = true;
    }

    void Update()
    {
        if (raycastIsHitting && !isTransparent)
        {
            isTransparent = true;
            MakeTransparent();
        }
        else if (!raycastIsHitting && isTransparent)
        {
            RevertToOriginal();
            isTransparent = false;
        }

        raycastIsHitting = false;
    }
}
