using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RuntimeMeshMaterialFixer : MonoBehaviour
{
    void Start()
    {
        FixMeshMaterialMismatch();
    }

    // Call this to fix the mesh anytime you need it (e.g., after instantiating an object)
    public void FixMeshMaterialMismatch()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            Debug.LogWarning($"Missing MeshFilter or MeshRenderer on '{gameObject.name}'");
            return;
        }

        Mesh mesh = meshFilter.mesh; // Runtime instance of the mesh

        if (mesh == null)
        {
            Debug.LogWarning($"No mesh assigned on '{gameObject.name}'");
            return;
        }

        int subMeshCount = mesh.subMeshCount;
        Material[] materials = meshRenderer.materials; // This gets a copy of materials (runtime)

        Debug.Log($"[Runtime] '{gameObject.name}': Submeshes = {subMeshCount}, Materials = {materials.Length}");

        if (materials.Length != subMeshCount)
        {
            Material[] newMaterials = new Material[subMeshCount];

            for (int i = 0; i < subMeshCount; i++)
            {
                if (i < materials.Length)
                {
                    newMaterials[i] = materials[i]; // Keep existing material
                }
                else
                {
                    // If not enough materials, assign a default material (using built-in Standard shader)
                    newMaterials[i] = new Material(Shader.Find("Standard"));
                }
            }

            meshRenderer.materials = newMaterials;

            Debug.Log($"[Runtime] ✔️ Fixed '{gameObject.name}': Materials now match submesh count ({subMeshCount})");
        }
        else
        {
            Debug.Log($"[Runtime] ✅ '{gameObject.name}' materials already match submeshes");
        }
    }
}