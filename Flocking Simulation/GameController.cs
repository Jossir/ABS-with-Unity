using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {



	public GameObject bird;
	public int numOfBirds;
	//public List<Sprite> sprite = new List<Sprite>();

	public Vector3 spawnValues;




	// Use this for initialization
	void Start () {
		spawnBirds();

	}
	
	// Update is called once per frame
	void Update () {
//		if (Input.GetKeyDown(KeyCode.R)) {//restart level
//			Application.LoadLevel(Application.loadedLevel);
//		}
	}

	void spawnBirds(){

		for (int i = 0; i < numOfBirds; i++) {
			

			//SpriteRenderer renderer = bird.GetComponentInChildren<SpriteRenderer>();
			//int r = Random.Range (0, sprite.Count);
			//renderer.sprite = sprite[r];

			Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x),spawnValues.y,Random.Range(-spawnValues.z,spawnValues.z));
			Quaternion spawnRotation = Quaternion.Euler(0,Random.Range(0, 359),0);
			Instantiate (bird, spawnPosition, spawnRotation);
		}


	}
}
