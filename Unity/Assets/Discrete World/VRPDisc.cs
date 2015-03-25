using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class R{
	public List<Node> v;
}

public class Vertex {
	public Node v;
	public List<int> pNeighborhood;

	public Vertex(Node v) {
		this.v = v;
		this.pNeighborhood = new List<int> ();
	}
}

public class VRPDisc : MonoBehaviour {

	List<Node> V;
	List<int> A;	
	Grid grid;
	List<Node> W; 

	void Start() {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();

		V = new List<Node> ();

		/*
		V.Add (W [1, 1]);
		V.Add (W [1, 2]);
		V.Add (W [1, 3]);
		V.Add (W [1, 4]);
		V.Add (W [1, 5]);
		V.Add (W [1, 6]);
		
		foreach (Node v in V) {
			print (v.gridPosY - 1);
		}

		print ("INSERTINGI NEW V");
		InsertionI (4, 0, 2, W [1, 7]);

		foreach (Node v in V) {
			print (v.gridPosY - 1);
		}
		*/
		/*
		V.Add (W [1, 1]);
		V.Add (W [1, 2]);
		V.Add (W [1, 3]);
		V.Add (W [1, 4]);
		V.Add (W [1, 5]);
		V.Add (W [1, 6]);
		V.Add (W [1, 7]);
		V.Add (W [1, 8]);

		foreach (Node v in V) {
			print (v.gridPosY - 1);
		}
		
		print ("INSERTINGII NEW V");
		InsertionII (7, 0, 4, 3, W [1, 9]);

		foreach (Node v in V) {
			print (v.gridPosY - 1);
		}
		*/
		/*
		// test1
		W = new List<Node> ();
		W.Add (grid.grid [1, 1]);
		W.Add (grid.grid [1, 2]);
		W.Add (grid.grid [1, 3]);
		W.Add (grid.grid [1, 4]);
		W.Add (grid.grid [1, 5]);
		W.Add (grid.grid [1, 6]);
		W.Add (grid.grid [1, 7]);

		List<Vertex> T = new List<Vertex> ();
		for (int i = 0; i < W.Count - 1; i++)
			T.Add (new Vertex(W [i]));
		
		foreach (Vertex x in T) {
			print ("[" + x.v.gridPosX + ", " + (x.v.gridPosY - 1) + "]");
		}

		print ("------------");
		List<Vertex> T2 = Insertion1 (0, 2, new Vertex (W [W.Count - 1]), T);
		foreach (Vertex v in T2) {
			print (v.v.gridPosY - 1);
		}

		foreach(Vertex v in T2) {
			int vi = -1;
			for(int i = 0; i < T2.Count; i++) {
				if(T2[i] == v) {
					vi = i;
					break;
				}
			}
			v.pNeighborhood = pNeighborhood(vi, 8, T);
		}

		print ("************");
		List<Vertex> T3 = Unstring1 (1, T2);
		foreach (Vertex v in T3) {
			print (v.v.gridPosY - 1);
		}
*/

		//Test 2
		W = new List<Node> ();
		W.Add (grid.grid [1, 1]);
		W.Add (grid.grid [1, 2]);
		W.Add (grid.grid [1, 3]);
		W.Add (grid.grid [1, 4]);
		W.Add (grid.grid [1, 5]);
		W.Add (grid.grid [1, 6]);
		W.Add (grid.grid [1, 7]);
		W.Add (grid.grid [1, 8]);
		W.Add (grid.grid [1, 9]);
/*
		List<Vertex> T = new List<Vertex> ();
		for (int i = 0; i < W.Count - 1; i++)
			T.Add (new Vertex(W [i]));

		foreach (Vertex x in T) {
			print ("[" + x.v.gridPosX + ", " + (x.v.gridPosY - 1) + "]");
		}

		print ("------------");
		List<Vertex> T2 = Insertion2 (0, 4, new Vertex (W [W.Count - 1]), T);
		foreach (Vertex v in T2) {
			print (v.v.gridPosY - 1);
		}

		foreach(Vertex v in T2) {
			int vi = -1;
			for(int i = 0; i < T2.Count; i++) {
				if(T2[i] == v) {
					vi = i;
					break;
				}
			}
			v.pNeighborhood = pNeighborhood(vi, 8, T);
		}

		print ("************");
		List<Vertex> T3 = Unstring2 (1, T2);
		foreach (Vertex v in T3) {
			print (v.v.gridPosY - 1);
		}
		*/
		W = new List<Node> ();
		

		W.Add (grid.grid [1, 1]);
		W.Add (grid.grid [1, 2]);
		W.Add (grid.grid [1, 3]);
		W.Add (grid.grid [1, 4]);
		W.Add (grid.grid [1, 5]);
		W.Add (grid.grid [1, 6]);
		W.Add (grid.grid [1, 7]);
		W.Add (grid.grid [1, 8]);
		W.Add (grid.grid [1, 9]);
		W.Add (grid.grid [1, 10]);
		W.Add (grid.grid [1, 11]);
		W.Add (grid.grid [1, 12]);
		W.Add (grid.grid [1, 13]);

		//Geni (W);
		List<Vertex> vv = GeniUs (W);
		print ("new");
		foreach (Vertex v in vv) {
			print (v.v.gridPosY - 1);
		}
	}
	
	public VRPDisc() {
	}

	public List<R> Search(List<Node> W, int q, R p1, int p2, int thetaMin, int thetaMax, float g, int h, int nMax) {
		// Initialization
		return null;
	}

	private List<int> pNeighborhood(int v, int p, List<Vertex> T) {
		List<int> ret = new List<int> ();
		if (T.Count < p) {
			for(int i = 0; i < T.Count; i++)
				ret.Add (i);
			return ret;
		}

		int tMin = mod ((v - (p / 2)), T.Count);
		for (int i = 0; i < p; i++) {
			ret.Add (tMin);
			tMin = mod ((tMin + 1), T.Count);
		}

		return ret;
	}

	public List<Vertex> Geni(List<Node> W) {
		int pos = 3;
		int p = 8;

		List<Vertex> T = new List<Vertex> ();
		T.Add (new Vertex (W [0]));
		T.Add (new Vertex (W [1]));
		T.Add (new Vertex (W [2]));

		while (T.Count != W.Count) {
			
			print (T.Count);
			foreach(Vertex v in T) {
				int vi = -1;
				for(int i = 0; i < T.Count; i++) {
					if(T[i] == v) {
						vi = i;
						break;
					}
				}
				v.pNeighborhood = pNeighborhood(vi, p, T);
			}

			if(pos >= W.Count)
				break;
			Vertex nV = new Vertex(W[pos]);
			pos++;

			float bestCost = -1;
			List<Vertex> bestPT = new List<Vertex> ();
			for(int i = 0; i < T.Count; i++) {
				for(int j = 0; j < T[i].pNeighborhood.Count; j++) {
					if(j <= i)
						continue;

					float cost;
					List<Vertex> pT1 = Insertion1(i, j, nV, T);
					List<Vertex> pT2 = Insertion2(i, j, nV, T);
					if(pT1 == null && pT2 == null) {
						continue;
					}
					else if(pT1 != null && pT2 == null) {
						cost = TourCost (pT1);
						if(bestCost == -1 || cost < bestCost) {
							bestCost = cost;
							bestPT = pT1;
						}
					}
					else if(pT1 == null && pT2 != null) {
						cost = TourCost (pT2);
						if(bestCost == -1 || cost < bestCost) {
							bestCost = cost;
							bestPT = pT2;
						}
					}
					else {
						float c1 = TourCost (pT1);
						float c2 = TourCost (pT2);
						cost = c1 < c2 ? c1 : c2;
						if(bestCost == -1 || cost < bestCost) {
							bestCost = cost;
							bestPT = c1 < c2 ? pT1 : pT2;
						}
					}
				}
			}

			/*
			List<Vertex> lastPT = new List<Vertex> (T);
			lastPT.Add (nV);
			float lastCost = TourCost (lastPT);
			if(lastCost < bestCost) {
				bestPT = lastPT;
			}
			*/

			if(bestPT.Count == 0) {
				T.Add (nV);
			}
			else {
				T = bestPT;
			}
		}


		foreach (Vertex v in T) {
			print (v.v.gridPosY - 1);
		}


		return T;
	}

	public List<Vertex> GeniUs(List<Node> W) {
		List<Vertex> T = Geni (W);
		List<Vertex> TStar = T;
		float costz = TourCost (T);
		float costStar = costz;
		int t = 0;

		while(t < T.Count) {
			Vertex rmVertex = T[t];
			List<Vertex> u1 = Unstring1 (t, T);
			List<Vertex> u2 = Unstring2 (t, T);
			bool improvement = false;
			foreach(int neigh in T[t].pNeighborhood) {
				print ("Checking " + neigh + " of " + t);
				List<Vertex> tU11 = Insertion1 (t, neigh, rmVertex, u1);
				List<Vertex> tU12 = Insertion2 (t, neigh, rmVertex, u1);
				List<Vertex> tU21 = Insertion1 (t, neigh, rmVertex, u2);
				List<Vertex> tU22 = Insertion2 (t, neigh, rmVertex, u2);
				
				float zPrim = -1;
				int index = -1;
				List<Vertex>[] proc = {tU11, tU12, tU21, tU22};
				for(int j = 0; j < 4; j++) {
					if(proc[j] != null) {
						float newCost = TourCost (proc[j]);
						if(zPrim == -1 || newCost < zPrim) {
							zPrim = newCost;
							index = j;
							improvement = true;
							
                            print ("hej");
						}
					}
				}

				if(index != -1) {
					List<Vertex> TPrim = proc[index];
					T = TPrim;
					costz = zPrim;

					foreach(Vertex v in T) {
						int vi = -1;
						for(int i = 0; i < T.Count; i++) {
							if(T[i] == v) {
								vi = i;
                                break;
                            }
                        }
                        v.pNeighborhood = pNeighborhood(vi, 8, T);
					}

					if(costz < costStar) {
						TStar = T;
						costStar = costz;
						t = 1;
					}
					else if(costz >= costStar) {
						t = t + 1;
					}
				}
			}
			if(!improvement) {
				t = t + 1;
            }
        }
        return TStar;
	}
	
	float TourCost(List<Vertex> T) {
		float cost = 0;
		for(int i = 0; i < T.Count-1; i++) {
			cost = cost + Cost (T[i].v, T[i+1].v);
		}
		return cost;
	}

	float Cost(Node A, Node B) {
		return Mathf.Abs (A.gridPosX - B.gridPosX) + Mathf.Abs (A.gridPosY - B.gridPosY);
	}

	private List<Vertex> Insertion1(int i, int j, Vertex v, List<Vertex> T) {
		try{
			if (T == null)
				return null;
			if (T.Count < 6 || mod (j + 2, T.Count) != mod (i - 2, T.Count)) {
				return null;
			}
			
			int k = j + 2;
			if (k + 1 >= T.Count)
				return null;
			
			List<Vertex> V = new List<Vertex> (T);
			if (!(V[k] != V[i] && V[k] != V[j])) {
				return null;
			}
			
			Vertex i1 = V [i + 1];
            Vertex j1 = V [j + 1];
            Vertex k1 = V [k + 1];
            V [i + 1] = v;
            V [j + 1] = i1;
            V [k + 1] = j1;
            V.Insert (k + 2, k1);
            
            return V;
        }
		catch (System.ArgumentOutOfRangeException e){
            return null;
		}

	}

	private List<Vertex> Insertion2(int i, int j, Vertex v, List<Vertex> T) {
		try{
			if (T == null)
				return null;
			if (T.Count < 8 || mod (i - 1, T.Count) != mod (j + 3, T.Count))
				return null;
			
			int k = j + 3;
			int l = j - 1;
			
			List<Vertex> V = new List<Vertex> (T);
			if (!(V [k] != V [j] && V [k] != V [j + 1] && V [l] != V [i] && V [l] != V [i + 1])) {
				return null;
			}
			
			List<Vertex> cV = new List<Vertex> (T);
			for (int x = i; x < k; x++) {
				V.Remove (V [i + 1]);
			}
			
			V.Insert (i + 1, v);
			V.Insert (i + 2, cV [j]);
			V.Insert (i + 3, cV [l]);
            V.Insert (i + 4, cV [j + 1]);
            V.Insert (i + 5, cV [k - 1]);
            V.Insert (i + 6, cV [l - 1]);
            V.Insert (i + 7, cV [i + 1]);
            V.Insert (i + 8, cV [k]);
            
            return V;
		}
		catch (System.ArgumentOutOfRangeException e){
			return null;
		}
	}

	private List<Vertex> Unstring1(int i, List<Vertex> T) {
		if (T == null)
			return null;
		if (T.Count <= i + 1 || i - 1 < 0) {
			print (i + " will result in out of bounds");
			return null;
		}

		List<Vertex> best = null;
		float bestCost = -1;
		List<int> vj = T[i+1].pNeighborhood;
		List<int> vk = T[i-1].pNeighborhood;
		for (int J = 0; J < vj.Count; J++) {
			for(int K = 0; K < vk.Count; K++) {
				if(i + 1 < vk[K] && vk[K] < vj[J] - 1) {
					//print (i + 1 + " < " + vk[K] + " < " + (vj[J] - 1));

					List<Vertex> V = new List<Vertex> (T);
					try{
					for(int x = vj[J] + 1; x >= i - 1; x--) {
						V.RemoveAt (x);
					}
					}
					catch (System.ArgumentOutOfRangeException e){
						continue;
					}

					V.Insert (i - 1, T[i-1]);
					V.Insert (i, T[vk[K]]);
					V.Insert (i + 1, T[i+1]);
					V.Insert (i + 2, T[vj[J]]);
					V.Insert (i + 3, T[vk[K]+1]);
					V.Insert (i + 4, T[vj[J]+1]);

					
					try{
					if(vk[K] - (i + 1) > 2)
						V.Reverse (i + 1, vk[K] - (i + 1));
					if(vj[J] - (vk[K]+1) > 2)
						V.Reverse (vk[K]+1, vj[J] - (vk[K]+1));

					float cost = TourCost (V);
					if(best == null || cost < bestCost) {
						bestCost = cost;
						best = V;
					}
					}
					catch (System.ArgumentException e){
						continue;
                    }
                }
            }
        }
        
        return best == null ? null : best;
	}

	private List<Vertex> Unstring2(int i, List<Vertex> T) {
		if (T == null)
			return null;
		if (T.Count <= i + 1 || i - 1 < 0) {
			print (i + " will result in out of bounds");
			return null;
		}

		List<int> vj = T [i + 1].pNeighborhood;
		List<int> vk = T [i - 1].pNeighborhood;
		List<Vertex> best = null;
		float bestCost = -1;
		for (int J = 0; J < vj.Count; J++) {
			for(int K = 0; K < vk.Count; K++) {
				//print ((vj[J]+1) + " < " + vk[K] + " < " + mod ((i - 2), T.Count));
				if(vj[J]+1 < vk[K] && vk[K] < mod(i-2,T.Count)) {
					List<int> vl = T[vk[K]+1].pNeighborhood;
					for(int L = 0; L < vl.Count; L++) {
						if(vj[J] < vl[L] && vl[L] < vk[K]-1) {
							if(vj[J] == 0)
								continue;

							//print (vj[J] + " < " + vl[L] + " < " + (vk[K]-1));
							List<Vertex> V = new List<Vertex> (T);
							try{
							for(int x = i + 7; x > i-2; x--) {
								V.RemoveAt (x);
							}
							}
							catch (System.ArgumentOutOfRangeException e){
								return null;
                            }
                            
                            V.Insert (i-1, T[i-1]);
							V.Insert (i, T[vk[K]]);
							V.Insert (i+1, T[vl[L]+1]);
							V.Insert (i+2, T[vj[J]-1]);
							V.Insert (i+3, T[i+1]);
							V.Insert (i+4, T[vj[J]]);
							V.Insert (i+5, T[vl[L]]);
							V.Insert (i+6, T[vk[K]+1]);

							if(vj[J] - 1 - (i+1) > 2)
								V.Reverse (i+1, vj[J] - 1 - (i+1));
							if(vk[K] - (vl[L]+1) > 2){
								V.Reverse (vl[L]+1, vk[K] - (vl[L]+1));
							}

							float cost = TourCost (V);
							if(best == null || cost < bestCost) {
								bestCost = cost;
								best = V;
							}
						}
					}
				}
			}
		}
		if (best == null)
			return new List<Vertex> ();
		else 
			return best;
		return best == null ? null : best;
	}

	int mod(int x, int m) {
		return (x%m + m)%m;
	}
}
