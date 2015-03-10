using UnityEngine;
using System.Collections;

public class PolyAgent{
	public Vector3 waypoint;
	public Vector3 end;
	public GameObject agent;
	public string id;
	public float R;
	public Model model;
	
	public PolyAgent(string id, Vector3 start, Vector3 end, float r) {
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start;
		R = r;
		this.end = end;
	}
}