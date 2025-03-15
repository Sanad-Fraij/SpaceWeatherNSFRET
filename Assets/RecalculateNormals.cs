using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RecalculateNormals : MonoBehaviour
{
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != null)
        {
            Mesh mesh = mf.mesh;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents(); // Optional: For tangent recalculation
            Debug.Log("Normals and tangents recalculated for: " + gameObject.name);
        }
    }
}