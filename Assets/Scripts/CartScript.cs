using UnityEngine;
using System.Collections;

public class CartScript : MonoBehaviour {
	
	public StartScript theSplineGenerator;
	float distMoved;
	float moveSpeed = 5.0f;
	public float initialTotalEnergy;
	public float mass = 1.0f;
	public float gravity = 9.8f;
	public float currentTotalEnergy;
	
	// Use this for initialization
	void Start () 
	{
		distMoved = 0.0f;
		moveSpeed = 5.0f;
		MoveAlongSpline();
		initialTotalEnergy = (mass * gravity * transform.position.y) + (0.5f * mass * (moveSpeed * moveSpeed));
		currentTotalEnergy = initialTotalEnergy;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float pastTotalEnergy = currentTotalEnergy;
		float totalEnergy = currentTotalEnergy;
		float currentPotentialEnergy;
		float currentKineticEnergy;
		Vector3 startPosition = Camera.main.transform.position; // where did it start
		if (theSplineGenerator != null)
		{
			distMoved += moveSpeed * Time.deltaTime;
			
			if (distMoved >= theSplineGenerator.splineLength)
			{
				distMoved = theSplineGenerator.splineLength;
			}
				
			MoveAlongSpline();
			 totalEnergy = (mass * gravity * transform.position.y) + (0.5f * mass * (moveSpeed * moveSpeed));
			float averageEnergy = (pastTotalEnergy + totalEnergy) / 2.0f;
			 currentTotalEnergy = averageEnergy;
			 currentPotentialEnergy = mass * gravity * transform.position.y;
			 currentKineticEnergy = initialTotalEnergy - currentPotentialEnergy;
			moveSpeed = Mathf.Sqrt((2.0f * currentKineticEnergy) / mass);
		}
	}
	
	void MoveAlongSpline()
	{
		if (theSplineGenerator != null)
		{
			Vector3 newPosition = new Vector3(0,0,0);
			Vector3 newUp = new Vector3(0,1,0);
			Vector3 newForward = new Vector3(0,0,1);
			Quaternion newOrientation = new Quaternion(0,0,0,1);
			
			theSplineGenerator.GetPointAlongSpline(distMoved, out newPosition, out newUp, out newForward);
			//print("MoveAlongSpline() - dist = " + distMoved + ", transform.position = " + transform.position + ", newPosition = " + newPosition);
			transform.position = newPosition;
			newOrientation.SetLookRotation(newForward, newUp);
			transform.rotation = newOrientation;
			
			Camera.main.transform.rotation = newOrientation;
		}
	
	}
}
