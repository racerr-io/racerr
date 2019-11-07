using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Track
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class BuildingState : MonoBehaviour
    {
        bool raycastIsHitting = false;
        bool isTransparent = false;

        [SerializeField] [Range(0, 1)] float transparency = 0.2f;
        [SerializeField] Mesh transparentMesh;
        [SerializeField] Shader transparentShader;
        Mesh originalMesh;
        Shader originalShader;
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
            originalShader = buildingMeshRenderer.material.shader;

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
                StartCoroutine(MakeTransparent());
            }
            else if (!raycastIsHitting && isTransparent) // There is no ray, but building is transparent, make building back to original
            {
                isTransparent = false;
                StartCoroutine(RevertToOriginal());
            }

            raycastIsHitting = false; // Set to false, but BuildingTransparencyRaycaster should set to true on each frame before Update() called again
        }

        /// <summary>
        /// Make object transparent and active children inactive.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        IEnumerator MakeTransparent()
        {
            buildingMeshFilter.mesh = transparentMesh;
            buildingMeshRenderer.material.shader = transparentShader;
            buildingMeshRenderer.material.SetFloat("_Glossiness", 0);
            activeChildren.ForEach(go => go.SetActive(false));
            yield return FadeTransparency(buildingMeshRenderer.material, transparency, 0.2f);
        }

        /// <summary>
        /// Revert back to the original state.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        IEnumerator RevertToOriginal()
        {
            buildingMeshFilter.mesh = originalMesh;
            yield return FadeTransparency(buildingMeshRenderer.material, 1f, 0.1f);
            buildingMeshRenderer.material.shader = originalShader;
            activeChildren.ForEach(go => go.SetActive(true));
        }

        /// <summary>
        /// Fade transparency of material over time.
        /// </summary>
        /// <param name="newAlphaValue">New transparency level.</param>
        /// <param name="time">How fast to fade.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        IEnumerator FadeTransparency(Material material, float newAlphaValue, float time)
        {
            Color colour = material.color;
            float alpha = material.color.a;
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
            {
                Color newColor = new Color(colour.r, colour.g, colour.b, Mathf.Lerp(alpha, newAlphaValue, t));
                material.color = newColor;
                yield return null;
            }
        }

        /// <summary>
        /// Sets raycastIsHitting a building to be true.
        /// </summary>
        public void HitRay()
        {
            raycastIsHitting = true;
        }
    }
}