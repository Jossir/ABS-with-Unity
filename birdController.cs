using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boundary 
{
	public float xMin, xMax, zMin, zMax;


}

public class birdController : MonoBehaviour {
	public Rigidbody rb;
	public Boundary boundary;
	public float speed = 3.0f;
	public float slowDownSeperate = 1.0f;

	public float vision = 1.5f;
	public float minimumSeperation=0.4f;
	public float maxAlignTurn =2.5f;
	public float maxCohereTurn = 1.5f;
	public float maxSeperateTurn = 0.75f;



	public bool catchUP = false;
	protected List<Rigidbody> nearbyBirds = new List<Rigidbody>();

	private Quaternion flockDirection;
	private Vector3 flockPos;

	private float closestBirdDinstance;
	private Rigidbody closestBird;

	private Quaternion lookRotation;



	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		speed = speed + (Random.Range (0, 3)/4) - (Random.Range (0, 3)/4);

	}
	
	// Update is called once per frame
	private float elapsed = 0f;
	void Update () {
		
		elapsed += Time.deltaTime;
		float waitTime = 4f/60f;//1min
		if (elapsed >= waitTime) {
			elapsed = elapsed % waitTime;
			//call methodes here
			NearbyBirds (rb.transform.position, vision);//populate nearby birds and update other variables
			if (nearbyBirds.Count > 0) {
				AverageQuaternion (nearbyBirds);//sets flock direction
				AveragePosition(nearbyBirds);//sets flock avg position
			}

			bool s = false;
			if (nearbyBirds.Count > 0)//only turn if nearby birds around 
			{

				//Collider[] hitColliders = Physics.OverlapSphere(rb.transform.position, minimumSeperation);

				if (closestBirdDinstance <= minimumSeperation) {

					rb.transform.rotation = (Quaternion.RotateTowards (rb.transform.rotation, closestBird.rotation, -maxSeperateTurn));//seperate
					s=true;
				} else {
					s = false;
					rb.transform.rotation = Quaternion.RotateTowards (rb.transform.rotation, flockDirection, maxAlignTurn);//align

					rb.transform.rotation = Quaternion.RotateTowards (rb.transform.rotation, lookRotation, maxCohereTurn);//cohere

				}
			}


			UpdateBoundry ();
			int birdsCloseBy = nearbyBirds.Count;
			if (catchUP) {
				if (birdsCloseBy < 1) {
					rb.velocity = transform.forward * speed * 1.2f;
				} else if (birdsCloseBy < 2) {
					rb.velocity = transform.forward * speed * 1.15f;
				} else if (birdsCloseBy < 3) {
					rb.velocity = transform.forward * speed * 1.08f;
				} else if (birdsCloseBy < 4) {
					rb.velocity = transform.forward * speed * 1.04f;
				} else if (birdsCloseBy < 5) {
					rb.velocity = transform.forward * speed * 1.02f;
				} else {
					rb.velocity = transform.forward * speed;
				}
			}
			if (s) {
				rb.velocity = transform.forward * speed * slowDownSeperate;
			} 
			//else if (nearbyBirds.Count > 0) {
			//			rb.velocity = transform.forward * speed * 1.1f;
			//		} else {
			//			rb.velocity = transform.forward * speed;
			//		}
			//float newSpeed = speed+(nearbyBirds.Count/5);
			//rb.velocity = transform.forward * newSpeed;


			rb.velocity = transform.forward * speed;



		}






	}
	void updateFlockPosDirection()
	{
		Vector3 direction = (flockPos - transform.position).normalized;
		lookRotation = Quaternion.LookRotation(direction);

	}

	void FixedUpdate()
	{
		//rb.position = new Vector3 (Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax),0.0f,Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax));


	}


	void UpdateBoundry()
	{
		if (rb.position.x < boundary.xMin || rb.position.x > boundary.xMax || rb.position.z < boundary.zMin || rb.position.z > boundary.zMax) {//check if out of boundry

			if (rb.position.x < 0) {
				if (rb.position.z < 0) {// -x, -z -> x, z
					rb.position = new Vector3 (Mathf.Abs (rb.position.x), 0, Mathf.Abs (rb.position.z));
				} else {// -x, z -> x, -z
					rb.position = new Vector3 (Mathf.Abs (rb.position.x), 0, -1 * rb.position.z);
				}
			} else {
				if (rb.position.z < 0) {// x, -z -> -x, z
					rb.position = new Vector3 (-1 * (rb.position.x), 0, Mathf.Abs (rb.position.z));

				} else {// x, z -> -x, -z
					rb.position = new Vector3 (-1 * (rb.position.x), 0, -1 * (rb.position.z));
				}

			}


		}

	}

	void NearbyBirds(Vector3 center, float radius)//populate nearby birds - stiore in nearbyBirds list
	{
		nearbyBirds.Clear ();

		//get avg vision direction and positions
		Collider[] hitColliders = Physics.OverlapSphere(center, radius);

		closestBirdDinstance = 1000f;

		bool found = false;
		int i = 0;
		float minDistance = 1000f;
		Collider bird = new Collider();
		while (i < hitColliders.Length) {
			if (hitColliders [i].tag == "Bird"&&!hitColliders[i].attachedRigidbody.Equals(rb)) {
				//float distance = Vector3.Distance(rb.position, hitColliders[i].ClosestPoint);//go through all nearby bird objects
				float distance = Vector3.Distance (center, hitColliders [i].attachedRigidbody.position);

				if (minDistance > distance) {
					found = true;
					minDistance = distance;
					bird = hitColliders [i];
				}




				nearbyBirds.Add (hitColliders [i].attachedRigidbody);
				//hitColliders [i].attachedRigidbody.transform.rotation
			}
			i++;
		}

		if (found) {
			closestBirdDinstance = minDistance;
			closestBird = bird.attachedRigidbody;
		}
		 
	}

	//average multiple roataions
	void AverageQuaternion(List<Rigidbody> qArray){
		Quaternion qAvg = qArray[0].rotation;
		float weight;
		for (int i = 1 ; i < qArray.Count; i++)
		{
			weight = 1.0f / (float)(i+1);
			qAvg = Quaternion.Slerp(qAvg, qArray[i].rotation, weight);
		}
		flockDirection= qAvg;
	}

	//avrage position
	void AveragePosition(List<Rigidbody> qArray)
	{
		float x = 0f;
		float y = 0f;
		float z = 0f;

		foreach (Rigidbody rb in qArray)
		{
			x += rb.position.x;
			y += rb.position.y;
			z += rb.position.z;
		}
		flockPos = new Vector3(x / qArray.Count, y / qArray.Count, z / qArray.Count);

		updateFlockPosDirection ();
	}



//	void OnTriggerEnter(Collider other)
//	{
//
//
//		//float step = maxSeperateTurn * Time.deltaTime;
//
//		//rotates togeather
//		//rb.transform.rotation = (Quaternion.RotateTowards(rb.transform.rotation, other.transform.rotation, -12));
//
//		if (other.tag == ("Bird")) {//seperate
//			rb.transform.rotation = (Quaternion.RotateTowards (rb.transform.rotation, other.transform.rotation, -maxSeperateTurn));//seperate
//		} else {//align and cohere
//			if (other.tag == ("Vision")) {
//				rb.transform.rotation = Quaternion.RotateTowards (rb.transform.rotation, flockDirection, maxAlignTurn);//align
//				//rb.transform.rotation = Quaternion.RotateTowards(rb.position, AveragePosition(nearbyBirds),maxCohereTurn);//cohesion
//				//Vector3 targetDir = target.position - transform.position;
//				//float step = speed * Time.deltaTime;
//				//Vector3 newDir = Vector3.RotateTowards(transform.forward, AveragePosition(nearbyBirds), maxCohereTurn, 0.0F);
//				//Debug.DrawRay(transform.position, newDir, Color.red);
//				//Vector3 newDir = AveragePosition(nearbyBirds) - rb.transform.position;
//				//rb.transform.position = Vector3.RotateTowards (rb.transform.position, newDir, maxCohereTurn, 0.0F);
//				//transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards (rb.transform.position, newDir, maxCohereTurn*0.01745329252f, 0.0F));
//
//				rb.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(rb.transform.position, AveragePosition(nearbyBirds), maxCohereTurn*0.01745329252f, 0.0F));
//			}
//		}
//
//	}






}
