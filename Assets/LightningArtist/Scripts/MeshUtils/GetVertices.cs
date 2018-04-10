using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GetVertices : MonoBehaviour {

    public bool alwaysUpdateSource = false;

    public enum Source { MESH, MESH_FILTER, SKINNED_MESH };
    public Source source = Source.MESH_FILTER;
    public enum Positions { WORLD, LOCAL };
    public Positions positions = Positions.WORLD;

    public Mesh mesh;
    public MeshFilter meshFilter;
    public SkinnedMeshRenderer skinnedMesh;

    public Vector3 origin = Vector3.zero;

    void Update() {
        if (alwaysUpdateSource) {
            getSource();
        }
    }

    public List<Vector3> getSource() {
        List<Vector3> points = new List<Vector3>();

        if (source == Source.MESH_FILTER) {
            mesh = meshFilter.mesh;
            origin = meshFilter.transform.position;
        } else if (source == Source.SKINNED_MESH) {
            mesh = new Mesh();
            skinnedMesh.BakeMesh(mesh);
            origin = skinnedMesh.transform.position;
        }

        if (positions == Positions.LOCAL) {
            points = mesh.vertices.ToList();
        } else if (positions == Positions.WORLD) {
            for (int i = 0; i < mesh.vertices.Length; i++) {
                points.Add(mesh.vertices[i] + origin);
            }
        }
        return points;
    }

}
