using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeaderFollower : MonoBehaviour{
	public List<LeaderFollower> children;
	LeaderFollower parent = null;
	PolyAgent agent;
	List<Vector3> leaderWP; // for leader only
	List<Line> obstacles = new List<Line> ();
	public static int totChildren = 5;
	public static int dist = 10; // How close to their leader they want to be
	public bool empty = true;
	bool doubleChild = false;

	// Choose model
	string modelType = "point";
	string moveType = "differential";

	void Start() {
		List<Vector3> start = new List<Vector3> ();
		start.Add (new Vector3 (100, 1, 100));
		start.Add (new Vector3 (200, 1, 100));
		start.Add (Vector3.zero);
		start.Add (new Vector3 (0, 1, -100));

		List<Model> models = new List<Model> ();
		for (int i = 0; i < totChildren; i++) {
			if(modelType == "car") {
				if(moveType == "dynamic")
					models.Add (gameObject.AddComponent<DynamicCarModel> ());
				if(moveType == "kinematic")
					models.Add (gameObject.AddComponent<KinematicCarModel> ());
			}
			if(modelType == "point") {
				if(moveType == "dynamic")
					models.Add (gameObject.AddComponent<DynamicPointModel> ());
				if(moveType == "kinematic")
					models.Add (gameObject.AddComponent<KinematicPointModel> ());
				if(moveType == "differential")
					models.Add (gameObject.AddComponent<DifferentialDriveModel> ());
			}
		}
		
		LeaderFollower leader = new LeaderFollower ("leader", Vector3.zero, null, start);
		leader.doubleChild = true;
		leader.CreateChildren (models);
		if(modelType == "car") {
			if(moveType == "dynamic")
				leader.SetModel (gameObject.AddComponent<DynamicCarModel> ());
			if(moveType == "kinematic")
				leader.SetModel (gameObject.AddComponent<KinematicCarModel> ());
		}
		if(modelType == "point") {
			if(moveType == "dynamic")
				leader.SetModel (gameObject.AddComponent<DynamicPointModel> ());
			if(moveType == "kinematic")
				leader.SetModel (gameObject.AddComponent<KinematicPointModel> ());
			if(moveType == "differential")
				leader.SetModel (gameObject.AddComponent<DifferentialDriveModel> ());
		}

		leader.MoveLeader ();
		StartCoroutine(MoveCoroutine (leader));

	}

	public static List<Vector3> debugwp = new List<Vector3> ();
	IEnumerator MoveCoroutine(LeaderFollower leader){
		while (true) {
			leader.MoveChildren ();
			yield return new WaitForSeconds (1);
			leader.StopChildren ();
            debugwp.Clear ();

			if(Vector3.Distance (leader.agent.agent.transform.position, leader.leaderWP[leader.leaderWP.Count-1]) < 4)
				leader.MoveLeader();
		}
    }


	// Note: Children can still spawn on each other. 
	// Some kind of agent collision needs to be added
	int numChildren = 1;
	public void CreateChildren(List<Model> model) {
		int index = 0;
		Queue<LeaderFollower> Q = new Queue<LeaderFollower> ();
		LeaderFollower v = this;
		Q.Enqueue (v);
		while (Q.Count > 0) {
			v = Q.Dequeue ();
			Vector3 parentPos = v.agent.agent.transform.position;
            
            for(int i = 0; i < v.children.Count; i++) {
				if(!v.doubleChild && i == 1) 
					break;
				if(v.children[i] == null) {
					if(totChildren > 0) {
						Vector3 startPos = Vector3.zero;

                        v.children[i] = new LeaderFollower("child " + numChildren++, startPos, v);
						v.children[i].SetModel(model[index++]);
						v.children[i].empty = false;
						if(i == 1)
							v.children[i].doubleChild = true;
						totChildren--;
						Q.Enqueue(v.children[i]);
					}
				}
			}
		}

	}

	public LeaderFollower() {

	}

	public LeaderFollower(string agentId, Vector3 agentPos, 
	                      LeaderFollower parent, List<Vector3> wp = null) {
		agent = new PolyAgent (agentId, agentPos, Vector3.zero, 5, modelType);
		agent.agent.renderer.material.color = Color.black;
		leaderWP = wp;
		children = new List<LeaderFollower>();
		children.Add (new LeaderFollower());
		children.Add (new LeaderFollower());
		this.parent = parent;
	}

	public void SetModel(Model model) {
		agent.model = model;
    }
    
	public void MoveChildren() { 
		Queue<LeaderFollower> Q = new Queue<LeaderFollower> ();
		LeaderFollower v = this;
		Q.Enqueue (v);
		Dictionary<PolyAgent, bool> visited = new Dictionary<PolyAgent, bool> ();
		visited [v.agent] = true;
		while (Q.Count > 0) {
			v = Q.Dequeue ();
			int i = 0;
			Vector3 parentPos = v.agent.agent.transform.position;
			foreach(LeaderFollower child in v.children) {
				if(!child.empty) {
					if(child.agent != null) {
						if(!visited.ContainsKey (child.agent) || !visited[child.agent]) {
							Q.Enqueue (child);
							visited[child.agent] = true;

							Vector3 dir = v.agent.agent.transform.forward;
							parentPos = parentPos - dir * dist/2;
                            List<Vector3> path = new List<Vector3> ();
							Vector3 left = Vector3.Cross (dir, Vector3.up);
                            if(i == 0){
								path.Add (parentPos + left * dist);
							}
							else if(i == 1) {			
								Vector3 right = -left;
                                path.Add (parentPos + right * dist);
							}
							debugwp.Add (path[0]);
                            

							child.agent.model.SetPath (path, child.agent, child.obstacles);
							child.agent.model.StartCoroutineMove ();
							child.agent.agent.renderer.material.color = Color.yellow;
                        }
                    }
                }
				i++;
            }
        }
    }

	public void StopChildren() { 
		Queue<LeaderFollower> Q = new Queue<LeaderFollower> ();
		LeaderFollower v = this;
		Q.Enqueue (v);
		Dictionary<PolyAgent, bool> visited = new Dictionary<PolyAgent, bool> ();
		visited [v.agent] = true;
		while (Q.Count > 0) {
			v = Q.Dequeue ();
			foreach(LeaderFollower child in v.children) {
				if(!child.empty) {
					if(child.agent != null) {
						if(!visited.ContainsKey (child.agent) || !visited[child.agent]) {
							Q.Enqueue (child);
							visited[child.agent] = true;
				
							child.agent.model.StopCoroutineMove();
                        }
                    }
                }
            }
        }
    }
    
    public void MoveLeader() {
        if (parent == null && leaderWP != null) {
			agent.model.SetPath (leaderWP, agent, obstacles);
			agent.model.StartCoroutineMove ();
		} 
	}

	void OnDrawGizmos() {
		foreach(Vector3 v in debugwp) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (v, 2);
		}
	}
}
