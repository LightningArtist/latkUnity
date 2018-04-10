using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulation {

    public float lowerAngle;

    List<Vertex> vertices;
    List<Polygon> triangles;

    public class Vertex {
		public Vector3 pos;
		public Vector3 normal;
		public Vector2 uv;
		public int type;

		public Vertex(Vector3 p, int t) {
			this.pos = p;
			this.type = t;
		}

		public Vertex(Vector3 p, int t, Vector3 n, Vector2 u) {
			this.pos = p;
			this.type = t;
			this.normal = n;
			this.uv = u;
		}
	}

	public class Polygon {
		public List<int> indexVertice;

		public Polygon() { 
			this.indexVertice = new List<int>(); 
		}

		public Polygon(int[] indexVertices) {
			this.indexVertice = new List<int>();
			for (int i=0; i<indexVertices.Length; i++) {
				this.indexVertice.Add(indexVertices[i]);	
			}
		}
	}

	public Triangulation() {
		this.vertices = new List<Vertex>();
		this.triangles = new List<Polygon>();
		this.lowerAngle = 3f;
	}

	public Triangulation(float lower_Angle) {
		this.vertices = new List<Vertex>();
		this.triangles = new List<Polygon>();
		this.lowerAngle = lower_Angle;
	}

	public Triangulation(Mesh mesh) {
		int i;
		this.vertices = new List<Vertex>();
		this.lowerAngle = 3f;

        for (i=0; i<mesh.vertices.Length; i++) {
			this.Add(mesh.vertices[i],0,mesh.normals[i],mesh.uv[i]);
		}

		this.triangles = new List<Polygon>();

        for (i=0; i<mesh.triangles.Length; i+=3) {
			this.triangles.Add(new Polygon(new int[3]{ mesh.triangles[i * 3] , mesh.triangles[i * 3+1] , mesh.triangles[i * 3+2]}));
		}
	}

	public void Add(Vector3 p) { 
		this.Add(p, 0); 
	}

	public void Add(Vector3 p, int t) { 
		this.vertices.Add(new Vertex(p, t)); 
	}

	public void Add(Vector3 p, int t, Vector3 n, Vector2 u) { 
		this.vertices.Add(new Vertex(p, t, n, u)); 
	}

	public void AddPointOnTriangle(Vector3 pos, int onTriangle) {
		if (onTriangle < 0 || onTriangle >= this.triangles.Count) return;
		this.vertices.Add(new Vertex(pos, 1, normalCoords(onTriangle), uvCoords(pos,onTriangle)));
		this.triangles[onTriangle].indexVertice.Add(this.vertices.Count-1);
	}


	public void AddTriangles(Vertex[] vertices, Polygon[] polygons) {
		int head = this.vertices.Count;
		int i, w;

        for (i=0; i<vertices.Length;i++) {
			this.vertices.AddRange(vertices);
		}

		for (i=0; i<polygons.Length;i++) {
			for (w=0; w<polygons[i].indexVertice.Count; w++) {
				polygons[i].indexVertice[w] += head;
			}
		}

		this.triangles.AddRange(polygons);
	}

	Vector3 normalCoords(int onTriangle) {
		return ((this.vertices[this.triangles[onTriangle].indexVertice[0]].normal + this.vertices[this.triangles[onTriangle].indexVertice[1]].normal + this.vertices[this.triangles[onTriangle].indexVertice[2]].normal) / 3f).normalized;
	}

	void invertNormals() { 
		for (int i=0; i<this.vertices.Count; i++) {
			this.vertices[i].normal *= -1f; 
		}
	}

	Vector2 uvCoords(Vector3 point, int onTriangle) {
		// http://answers.unity3d.com/questions/383804/calculate-uv-coordinates-of-3d-point-on-plane-of-m.html
		// ... interpolate (extrapolate?) points outside the triangle, a more general approach must be used: the "sign" of each
		// area must be taken into account, which produces correct results for points inside or outside the triangle. In order 
		// to calculate the area "signs", we can use (guess what?) dot products - like this:

		// triangle points
		Vector3 p1 = this.vertices[this.triangles[onTriangle].indexVertice[0]].pos;
		Vector3 p2 = this.vertices[this.triangles[onTriangle].indexVertice[1]].pos;
		Vector3 p3 = this.vertices[this.triangles[onTriangle].indexVertice[2]].pos;

		// calculate vectors from point f to vertices p1, p2 and p3:
		Vector3 f1 = p1-point; //p1-f;
		Vector3 f2 = p2-point; //p2-f;
		Vector3 f3 = p3-point; //p3-f;

		// calculate the areas (parameters order is essential in this case):
		Vector3 va  = Vector3.Cross(p1-p2, p1-p3); // main triangle cross product
		Vector3 va1 = Vector3.Cross(f2, f3); // p1's triangle cross product
		Vector3 va2 = Vector3.Cross(f3, f1); // p2's triangle cross product
		Vector3 va3 = Vector3.Cross(f1, f2); // p3's triangle cross product
		float     a = va.magnitude; // main triangle area

		// calculate barycentric coordinates with sign:
		float a1 = va1.magnitude/a * Mathf.Sign(Vector3.Dot(va, va1));
		float a2 = va2.magnitude/a * Mathf.Sign(Vector3.Dot(va, va2));
		float a3 = va3.magnitude/a * Mathf.Sign(Vector3.Dot(va, va3));

		// find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
		Vector2 uv1=this.vertices[this.triangles[onTriangle].indexVertice[0]].uv;
		Vector2 uv2=this.vertices[this.triangles[onTriangle].indexVertice[1]].uv;
		Vector2 uv3=this.vertices[this.triangles[onTriangle].indexVertice[2]].uv;

		return uv1 * a1 + uv2 * a2 + uv3 * a3;
	}


	public Vector3[] Calculate() {
		if (this.vertices.Count == 0) return new Vector3[1];

		Vector3[] result;
		int i;

		if (this.vertices.Count <= 3) {
			result = new Vector3[vertices.Count];
			for (i=0; i<vertices.Count; i++) {
				result[i]=vertices[i].pos;
			}
			return result;
		}

		List<int[]> triangles = new List<int[]>();
		int[] triangle;
		int w,x;
		float circumsphereRadius;
		Vector3 a, b, c, ac, ab, abXac, toCircumsphereCenter, ccs;

		//All Combinations without repetition, some vertice of different type, only one vertice different of type 0
		for (i=0; i<vertices.Count-2; i++) { 
			for (w=i+1; w<vertices.Count-1; w++) { 
				for (x=w+1; x<vertices.Count; x++) {
					if (vertices[i].type == vertices[w].type && vertices[i].type == vertices[x].type) continue; // Same type
					if (Vector3.Angle(vertices[w].pos-vertices[i].pos, vertices[x].pos-vertices[i].pos) < this.lowerAngle) continue; // Remove triangles with angle near to 180º
					triangle = new int[3]{i, w, x};
					triangles.Add(triangle);
				} 
			} 
		}

		//Delaunay Condition
		for (i=triangles.Count-1; i>=0; i--) {
			//Points
			triangle = triangles[i];
			a = vertices[triangle[0]].pos;
			b = vertices[triangle[1]].pos;
			c = vertices[triangle[2]].pos;

			//Circumcenter 3Dpoints
			//http://gamedev.stackexchange.com/questions/60630/how-do-i-find-the-circumcenter-of-a-triangle-in-3d
			ac = c - a;
			ab = b - a;
			abXac = Vector3.Cross(ab, ac);				
			// this is the vector from a TO the circumsphere center
			toCircumsphereCenter = (Vector3.Cross(abXac, ab) * ac.sqrMagnitude + Vector3.Cross(ac, abXac) * ab.sqrMagnitude) / (2f * abXac.sqrMagnitude);				
			// The 3 space coords of the circumsphere center then:
			ccs = a + toCircumsphereCenter ; // now this is the actual 3space location
			// The three vertices A, B, C of the triangle ABC are the same distance from the circumcenter ccs.
			circumsphereRadius = toCircumsphereCenter.magnitude;
			// As defined by the Delaunay condition, circumcircle is empty if it contains no other vertices besides the three that define.
			for (w=0; w<vertices.Count; w++) {
				if (w != triangle[0] && w != triangle[1] && w != triangle[2]) {
					if (Vector3.Distance(vertices[w].pos,ccs) <= circumsphereRadius) break;
				}
			}
			// If it's not empty, remove.
			if (w != vertices.Count) triangles.RemoveAt(i);
		}

		result = new Vector3[triangles.Count * 3];
		for (i=0; i<triangles.Count; i++) {
			triangle = triangles[i];
			result[i * 3] = vertices[triangle[0]].pos;
			result[i * 3+1] = vertices[triangle[1]].pos;
			result[i * 3+2] = vertices[triangle[2]].pos;
		}

		return result;
	}

}