using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartScript : MonoBehaviour {
	
	public NodeScript firstNode;
	public GameObject trackTiePrefab;
	
	public float splineLength = 0.0f;
	
	class SplineNode
	{
		public Vector3	up;
		public Vector3	forward;
		public Vector3	position;
		public float 	distFromStartOfSpline;
		
		public SplineNode()
		{
			up = new Vector3(0,0,0);
			forward = new Vector3(0,0,0);
			position = new Vector3(0,0,0);
			distFromStartOfSpline = 0.0f;
		}
	};
	
	List<SplineNode> nodeList = new List<SplineNode>();
	
	// Use this for initialization
	void Start () 
	{
		BuildSpline();
		GenerateTrack();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	void GenerateTrack()
	{
		float dist = 0.0f;
		Vector3 pos = new Vector3(0,0,0);
		Vector3 up = new Vector3(0,1,0);
		Vector3 forward = new Vector3(0,0,1);
		Quaternion orientation = new Quaternion(0,0,0,1);
		
		while (dist < splineLength)
		{
			GetPointAlongSpline(dist, out pos, out up, out forward);
			orientation.SetLookRotation(forward, up);
			if (trackTiePrefab != null)
			{
				Instantiate(trackTiePrefab, pos, orientation);
			}
			
			// Making a lot of spheres is a good way of debugging the spline
			//GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = pos;
			
			dist += 5.0f;
		}
	}
	
	public void GetPointAlongSpline(float dist, out Vector3 position, out Vector3 up, out Vector3 forward)
	{
		int lowNodeIndex = 0;
		int maxNodeIndex = nodeList.Count-1;
		Vector3 vecFromNodeToNode = new Vector3(0,0,0);		//work variable
		
		print("GetPointAlongSpline - dist = " + dist + ", nodeList.Count = " + nodeList.Count);
		print("nodeList[maxNodeIndex].distFromStartOfSpline = " + nodeList[maxNodeIndex].distFromStartOfSpline);
		
		//check for start or end
		if (dist < 0.0f)
		{
			position = nodeList[0].position;
			up = nodeList[0].up;
			forward = nodeList[0].forward;
			return;
		}
		else if (dist > nodeList[maxNodeIndex].distFromStartOfSpline)
		{
			//allow for distances off of the end of the spline, but they're just in the line between the last two points
			vecFromNodeToNode = nodeList[maxNodeIndex].position - nodeList[maxNodeIndex-1].position;
			float distRemaining = dist - nodeList[maxNodeIndex].distFromStartOfSpline;
			position = nodeList[maxNodeIndex].position + vecFromNodeToNode.normalized * distRemaining;
			
			up = nodeList[maxNodeIndex].up;
			forward = nodeList[maxNodeIndex].forward;
			return;
		}
		
		//do a binary search to find the nodes that dist falls between
		int sanity = 0;
		while (lowNodeIndex < maxNodeIndex - 1)
		{
			int midIndex = (maxNodeIndex + lowNodeIndex) / 2;
			
			if (dist < nodeList[midIndex].distFromStartOfSpline)
			{
				maxNodeIndex = midIndex;
			}
			else if (dist >= nodeList[midIndex].distFromStartOfSpline)
			{
				lowNodeIndex = midIndex;
			}
			//print("lowNodeIndex = " + lowNodeIndex + ", maxNodeIndex = " + maxNodeIndex);
			
			if (sanity > 100)
			{
				Debug.Break();
				break;
			}
			sanity++;
		}
		
		//generate our 3 interpolation points
		Vector3 pt0Position = new Vector3(0,0,0);
		Vector3 pt0Up = new Vector3(0,1,0);
		Vector3 pt0Forward = new Vector3(0,0,1);
		
		Vector3 pt1Position = new Vector3(0,0,0);
		Vector3 pt1Up = new Vector3(0,1,0);
		Vector3 pt1Forward = new Vector3(0,0,1);
		
		Vector3 pt2Position = new Vector3(0,0,0);
		Vector3 pt2Up = new Vector3(0,1,0);
		Vector3 pt2Forward = new Vector3(0,0,1);
		
		float distFromPos0ToPos1 = nodeList[maxNodeIndex].distFromStartOfSpline - nodeList[lowNodeIndex].distFromStartOfSpline;
		float ratio = (dist - nodeList[lowNodeIndex].distFromStartOfSpline) / distFromPos0ToPos1;
		float t = 0.0f;
		if (ratio < 0.0f || ratio > 1.0f)
		{
			print("Sanity check FAILED! Dist = " + dist + ", ratio = " + ratio);
			Debug.Break();
		}
		else if (ratio > 0.5f)
		{
			//I'm at the start of the Bezier curve surrounding nodeList[maxNodeIndex]
			//Calculate "t" (the interpolation value used to generate the Bezier curve), which is between 0 & 1... 
			//but this case will always be between 0 & 0.5
			t = ratio - 0.5f;
			
			//pt0 is the midpoint between maxNodeIndex and the last node
			pt0Position = (nodeList[maxNodeIndex-1].position + nodeList[maxNodeIndex].position) * 0.5f;
			pt0Up = (nodeList[maxNodeIndex-1].up + nodeList[maxNodeIndex].up) * 0.5f;
			pt0Forward = (nodeList[maxNodeIndex-1].forward + nodeList[maxNodeIndex].forward) * 0.5f;
			
			//pt1 is just maxNodeIndex
			pt1Position = nodeList[maxNodeIndex].position;
			pt1Up = nodeList[maxNodeIndex].up;
			pt1Forward = nodeList[maxNodeIndex].forward;
			
			//pt2 is the midpoint between maxNodeIndex and the next node
			if (maxNodeIndex == nodeList.Count-1)
			{
				//special case... I need to generate a point that exists AFTER the end of the spline
				pt2Position = (nodeList[maxNodeIndex].position - nodeList[maxNodeIndex-1].position).normalized * (distFromPos0ToPos1 / 2.0f);
				pt2Position += nodeList[maxNodeIndex].position;
				pt2Up = nodeList[maxNodeIndex].up;
				pt2Forward = nodeList[maxNodeIndex].forward;
			}
			else
			{
				pt2Position = (nodeList[maxNodeIndex].position + nodeList[maxNodeIndex+1].position) * 0.5f;
				pt2Up = (nodeList[maxNodeIndex].up + nodeList[maxNodeIndex+1].up) * 0.5f;
				pt2Forward = (nodeList[maxNodeIndex].forward + nodeList[maxNodeIndex+1].forward) * 0.5f;
			}
		}
		else
		{
			//I'm at the end of the Bezier curve surrounding nodeList[lowNodeIndex]
			//Calculate "t" (the interpolation value used to generate the Bezier curve), which is between 0 & 1... 
			//but this case will always be between 0.5 & 1.0
			t = ratio + 0.5f;
			
			//pt0 is the midpoint between lowNodeIndex and the last node
			if (lowNodeIndex == 0)
			{
				//special case... I need to generate a point that exists BEFORE the start of the spline
				pt0Position = (nodeList[lowNodeIndex].position - nodeList[lowNodeIndex+1].position).normalized * (distFromPos0ToPos1 / 2.0f);
				pt0Position += nodeList[lowNodeIndex].position;
				pt0Up = nodeList[lowNodeIndex].up;
				pt0Forward = nodeList[lowNodeIndex].forward;
			}
			else
			{
				pt0Position = (nodeList[lowNodeIndex].position + nodeList[lowNodeIndex-1].position) * 0.5f;
				pt0Up = (nodeList[lowNodeIndex].up + nodeList[lowNodeIndex-1].up) * 0.5f;
				pt0Forward = (nodeList[lowNodeIndex].forward + nodeList[lowNodeIndex-1].forward) * 0.5f;
			}			
				
			//pt1 is just lowNodeIndex
			pt1Position = nodeList[lowNodeIndex].position;
			pt1Up = nodeList[lowNodeIndex].up;
			pt1Forward = nodeList[lowNodeIndex].forward;			
			
			//pt2 is the midpoint between lowNodeIndex and the next node
			pt2Position = (nodeList[lowNodeIndex].position + nodeList[lowNodeIndex+1].position) * 0.5f;
			pt2Up = (nodeList[lowNodeIndex].up + nodeList[lowNodeIndex+1].up) * 0.5f;
			pt2Forward = (nodeList[lowNodeIndex].forward + nodeList[lowNodeIndex+1].forward) * 0.5f;
		}
		
		//Special work variables - S0 and S1
		//	S0 is a point interpolated between pt0 and pt1
		//	S1 is a point interpolated between pt1 and pt2
		Vector3 S0Position = new Vector3(0,0,0);
		Vector3 S0Up = new Vector3(0,1,0);
		Vector3 S0Forward = new Vector3(0,0,1);
		
		Vector3 S1Position = new Vector3(0,0,0);
		Vector3 S1Up = new Vector3(0,1,0);
		Vector3 S1Forward = new Vector3(0,0,1);
		
		//Get S0
		InterpBetweenValues(t, ref pt0Position, ref pt1Position,
		                       ref pt0Up,       ref pt1Up,
		                       ref pt0Forward,  ref pt1Forward,
		                       out S0Position, out S0Up, out S0Forward);
		//Get S1
		InterpBetweenValues(t, ref pt1Position, ref pt2Position,
		                       ref pt1Up,       ref pt2Up,
		                       ref pt1Forward,  ref pt2Forward,
		                       out S1Position, out S1Up, out S1Forward);
		
		
		//Use S0 & S1 to get the points along the spline
		InterpBetweenValues(t, ref S0Position, ref S1Position,
		                       ref S0Up,       ref S1Up,
		                       ref S0Forward,  ref S1Forward,
		                       out position, out up, out forward);
		
		
		//Debug, test case
		/*InterpBetweenValues(ratio, ref nodeList[lowNodeIndex].position, ref nodeList[maxNodeIndex].position,
		                    	   ref nodeList[lowNodeIndex].up,       ref nodeList[maxNodeIndex].up,
		                           ref nodeList[lowNodeIndex].forward,  ref nodeList[maxNodeIndex].forward,
		                    	   out position, out up, out forward);*/
		
		//print("dist = " + dist + ", ratio = " + ratio + ", t = " + t + ", position = " + position);
	}
	
	
	void InterpBetweenValues(float ratio, ref Vector3 pos0, ref Vector3 pos1,
	                         			  ref Vector3 up0, ref Vector3 up1,
	                         			  ref Vector3 forward0, ref Vector3 forward1,
	                         			  out Vector3 resultPosition, out Vector3 resultUp, out Vector3 resultForward)
	{		
		resultPosition = ((pos1 - pos0) * ratio) + pos0;
		resultUp = ((up1 - up0) * ratio) + up0;
		resultForward = ((forward1 - forward0) * ratio) + forward0;
	}
	
	void BuildSpline()
	{
		if (firstNode != null && firstNode.nextNode != null)
		{
			NodeScript curNode = firstNode;
			Vector3 vecFromNodeToNode = new Vector3(0,0,0);
			float distAlongSpline = 0.0f;
			while (curNode != null)
			{
				//print("\ncurNode = " + curNode.name + ", distAlongSpline = " + distAlongSpline);
				
				SplineNode newSplineNode = new SplineNode();
				newSplineNode.distFromStartOfSpline = distAlongSpline;
				newSplineNode.up = curNode.transform.up;
				newSplineNode.forward = curNode.transform.forward;
				newSplineNode.position = curNode.transform.position;
				
				curNode.renderer.enabled = false;
				
				nodeList.Add(newSplineNode);
				//print("nodeList.Count = " + nodeList.Count);
				
				//look ahead
				if (curNode.nextNode == null)
				{
					//print("curNode.nextNode = None");
				}
				else
				{
					//print("curNode.nextNode = " + curNode.nextNode.name);
					
					//sanity check... make sure we're not building a loop!
					foreach (SplineNode node in nodeList)
					{
						if (Mathf.Approximately((node.position - curNode.nextNode.transform.position).sqrMagnitude, 0.0f))
						{
							print("Infinite Node Loop Detected!  Halting execution!");
							Debug.Break();
							break;
						}
					}
					
					vecFromNodeToNode = curNode.nextNode.transform.position - curNode.transform.position;
					distAlongSpline += vecFromNodeToNode.magnitude;
				}
				curNode = curNode.nextNode;
			}
			
			splineLength = distAlongSpline;
			//print("splineLength = " + splineLength + ", nodeList.Count = " + nodeList.Count);
			
		}
	}
}
