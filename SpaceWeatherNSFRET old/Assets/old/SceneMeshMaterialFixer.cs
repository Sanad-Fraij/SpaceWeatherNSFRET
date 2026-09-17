using UnityEngine;

public class SceneMeshMaterialFixer : MonoBehaviour
{
    // Runs automatically at scene start
    void Start()
    {
        FixAllMeshesInScene();
    }

    // Scans and fixes all meshes in the active scene
    public void FixAllMeshesInScene()
    {
        int fixedObjects = 0;

        // Find all MeshRenderers in the scene, including inactive ones
        MeshRenderer[] meshRenderers = GameObject.FindObjectsOfType<MeshRenderer>(true);

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();

            if (meshFilter == null)
                continue; // Skip if no MeshFilter

            Mesh mesh = meshFilter.sharedMesh; // sharedMesh works with prefabs and instances

            if (mesh == null)
                continue;

            int subMeshCount = mesh.subMeshCount;
            Material[] materials = meshRenderer.sharedMaterials; // SharedMaterials matches prefab instances

            // If the materials don't match the submeshes, fix it
            if (materials.Length != subMeshCount)
            {
                Material[] newMaterials = new Material[subMeshCount];

                for (int i = 0; i < subMeshCount; i++)
                {
                    if (i < materials.Length)
                    {
                        newMaterials[i] = materials[i]; // Keep existing
                    }
                    else
                    {
                        // Replace with default material (you can assign your own default later)
                        newMaterials[i] = new Material(Shader.Find("Standard"));
                    }
                }

                meshRenderer.sharedMaterials = newMaterials; // SharedMaterials makes prefab overrides clean
                fixedObjects++;

                Debug.Log($"✔️ Fixed '{meshRenderer.gameObject.name}': Submeshes = {subMeshCount}, Materials = {newMaterials.Length}");
            }
        }

        Debug.Log($"✅ Scene Mesh Material Fix complete. Fixed {fixedObjects} objects!");
    }
}
