using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Obstacle {

	public List<Line> edges;
	public List<Vector3> vertices;
	public int id = -1;

	public Obstacle() {
		vertices = new List<Vector3> ();
		edges = new List<Line> ();
	}

	public Obstacle(Obstacle obs) {
		edges = new List<Line> ();
		vertices = new List<Vector3> ();

		id = obs.id;

		foreach (Vector3 vec in obs.vertices) {
			vertices.Add (vec);
		}

		foreach (Line line in obs.edges) {
			edges.Add (line);
		}
	}
}
