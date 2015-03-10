using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolyNode {
	public List<Vector3> vertices;
	public List<Vector3> neighbours;
	public Vector3 pos;

	public PolyNode(){
		vertices = new List<Vector3>();
		neighbours = new List<Vector3> ();
	}
}
