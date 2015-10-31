using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Com.EricReber.InfiniteBackGround
{
	public class InfiniteParallaxBackGround : MonoBehaviour{
		[Tooltip("Camera to used to display this background")]
		public Camera parallaxCamera;
		
		[Tooltip("The elements in the parallax.")]
		public List<GameObject> elements;
		
		[Tooltip("Vertical offset used when positionning the backGround")]
		public float verticalOffset = 0.0f;

		//All the elementsThatCan be displayed on the backGround;
		private List<BackGroundElement> elementsOnScreen = new List<BackGroundElement>();

		private List<PrefabPool> prefabsPool = new List<PrefabPool>();

		//The index of the first (left) element displayed on the screen -1 mean no element on screen
		private int firstElementIndex = -1;

		//The index of the last (right) element displayed on the screen -1 mean no element on screen
		private int lastElementIndex = -1;

		//Retrieve the Camera boundaries in the World Cordinate system using the view port 
		//(Viewport bottom left = 0,0 top right = 1,1
		private float cameraLeftBound{
			get{ return parallaxCamera.ViewportToWorldPoint(Vector3.zero).x;}
		}
		private float cameraRightBound{
			get{ return parallaxCamera.ViewportToWorldPoint(Vector3.right).x;}
		}
		
		//Store the rightest postion displaying a backGround
		private float worldRightBound;

		void Start(){
			//When we start, I add one copy of each prefab in the PrefabPool
			foreach (GameObject obj in elements)
			{
				PrefabPool p = new PrefabPool(obj);
				prefabsPool.Add(p);
			}
			fillUpTheViewPort();	
		}
		
		//This methode is used to add the prefab to the scene so it takes all the view port
		private void fillUpTheViewPort(){
			worldRightBound = cameraLeftBound;
			
			while (worldRightBound<cameraRightBound){
				Vector3 initialPosition = new Vector3(worldRightBound,verticalOffset);
				worldRightBound = DisplayRandomPrefab(initialPosition);
			}

			//When we fillUp the screen for the first time the first element is alway at index 0
			if(firstElementIndex == -1){
				firstElementIndex = 0;
			}

			if(lastElementIndex == -1){
				lastElementIndex = elementsOnScreen.Count;
			}

		}
		
		//Display a Random Prefab and position it at initialPosition return the x coordinate for the right bound
		private float DisplayRandomPrefab(Vector3 initialPosition){
			int randomIndex = 0;
			if(prefabsPool.Count==0){
				Debug.LogError("You need at least one prefab in the elements list.");
			}else if(prefabsPool.Count>1){
				randomIndex = Mathf.Abs(Random.Range(0,prefabsPool.Count));
			}

			//The element that will be rendered
			BackGroundElement renderedObject = prefabsPool[randomIndex].GetAvailableObject();

			renderedObject.DisplayAtPosition(initialPosition);
			elementsOnScreen.Add(renderedObject);

			return renderedObject.rightBound;
		}
		
		void Update()
		{
			//If the camera is rendering something without a background we add one
			if(cameraRightBound>worldRightBound){
				Vector3 initialPosition = new Vector3(worldRightBound,verticalOffset);
				worldRightBound = DisplayRandomPrefab(initialPosition);
				
			}

			//If we have element that are not displayed by the camera any more we disable them
			if(elementsOnScreen[firstElementIndex].rightBound<cameraLeftBound){
				elementsOnScreen[firstElementIndex].Remove();

				if(firstElementIndex<elementsOnScreen.Count){
					firstElementIndex++;
				}else{
					firstElementIndex = 0;
				}
			}
		}
	}

	//
	public class BackGroundElement{
		public GameObject gameObject;
		public float width;
		public float rightBound;

		private Renderer renderer;

		public BackGroundElement(GameObject gObj){
			gameObject = gObj;
			renderer = gameObject.GetComponent<Renderer>();
			width = renderer.bounds.size.x;
		}

		public void DisplayAtPosition(Vector3 pos){
			//We are placing the obect based on his center so we /2.0f the width
			Vector3 newPos = new Vector3(pos.x+width/2.0f,pos.y);
			gameObject.transform.position = newPos;
			gameObject.SetActive(true);
			//We add it to the element on screen

			//We want the right bound, not the center, that's why we don't divide by 2
			rightBound = pos.x+width;
		}

		public void Remove(){
			gameObject.SetActive(false);
		}
	}

	public class PrefabPool{
		private GameObject prefab;
		private List<BackGroundElement> gameObjectPool = new List<BackGroundElement>();

		public PrefabPool(GameObject mainPrefab){
			prefab = mainPrefab;
		}

		public BackGroundElement GetAvailableObject(){
			BackGroundElement backgroundElem =null;
			foreach (BackGroundElement obj in gameObjectPool)
			{
				if(obj.gameObject.activeSelf==false){
					backgroundElem = obj;
					break;
				}
			}

			//if we don't have any available gameObject we need to instansiate a new one
			if(backgroundElem==null){
				GameObject gameObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
				backgroundElem = new BackGroundElement(gameObject);
				gameObjectPool.Add(backgroundElem);
			}
			return backgroundElem;
		}
	}
}
