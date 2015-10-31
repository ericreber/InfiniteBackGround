using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Com.EricReber.InfiniteBackGround
{
	public class InfiniteBackGround : MonoBehaviour{
		[Tooltip("Camera to used to display this background")]
		public Camera parallaxCamera;
		
		[Tooltip("The elements in the parallax.")]
		public List<GameObject> elements;
		
		[Tooltip("Vertical offset used when positionning the backGround")]
		public float verticalOffset = 0.0f;

		//All the elementsThatCan be displayed on the backGround;
		private List<BackGroundElement> elementsOnScreen = new List<BackGroundElement>();

		private List<PrefabPool> prefabsPool = new List<PrefabPool>();

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
		private float worldLeftBound;

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
			worldLeftBound = cameraLeftBound;
			while (worldRightBound<cameraRightBound){
				Vector3 initialPosition = new Vector3(worldRightBound,verticalOffset);
				worldRightBound = DisplayRandomPrefab(initialPosition,BackGroundInsertionDirection.Right).rightBound;
			}
		}
		
		//Display a Random Prefab and position it at initialPosition return the x coordinate for the right bound
		private BackGroundElement DisplayRandomPrefab(Vector3 initialPosition,BackGroundInsertionDirection direction){
			int randomIndex = 0;
			if(prefabsPool.Count==0){
				Debug.LogError("You need at least one prefab in the elements list.");
			}else if(prefabsPool.Count>1){
				randomIndex = Mathf.Abs(Random.Range(0,prefabsPool.Count));
			}

			//The element that will be rendered
			BackGroundElement renderedObject = prefabsPool[randomIndex].GetAvailableObject();

			renderedObject.DisplayAtPosition(initialPosition,direction);

			if(direction == BackGroundInsertionDirection.Right){
				elementsOnScreen.Add(renderedObject);
			}else{
				elementsOnScreen.Insert(0,renderedObject);
			}

			return renderedObject;
		}
		private void RemovePrefab(PrefabRank rank){
			//we can only remove the first or the last prefab
			if(rank == PrefabRank.First){
				//Move the world left border
				worldLeftBound+=elementsOnScreen[0].width;
				//Disable the gameObject
				elementsOnScreen[0].Remove();
				//Remove it from the onScreen list
				elementsOnScreen.RemoveAt(0);
			}else{
				//Move the world Right border
				worldRightBound-=elementsOnScreen[elementsOnScreen.Count-1].width;
				//Disable the gameObject
				elementsOnScreen[elementsOnScreen.Count-1].Remove();
				//Remove it from the onScreen list
				elementsOnScreen.RemoveAt(elementsOnScreen.Count-1);
			}
		}
		void Update()
		{
			//If the camera is rendering something without a background we add one
			//Check on the right
			if(cameraRightBound>worldRightBound){
				Vector3 initialPosition = new Vector3(worldRightBound,verticalOffset);
				worldRightBound = DisplayRandomPrefab(initialPosition,BackGroundInsertionDirection.Right).rightBound;	
				Trace("add right");
			}
			//Check on the left
			if(cameraLeftBound<worldLeftBound){
				Vector3 initialPosition = new Vector3(worldLeftBound,verticalOffset);
				worldLeftBound = DisplayRandomPrefab(initialPosition,BackGroundInsertionDirection.Left).leftBound;	
				Trace("add left");
			}

			//If we have element that are not displayed by the camera any more we disable them
			//Check on the left
			if(elementsOnScreen[0].rightBound<cameraLeftBound){
				RemovePrefab(PrefabRank.First);
				Trace("Remove left");
			}
			//Check on the Right
			if(elementsOnScreen[elementsOnScreen.Count-1].leftBound>cameraRightBound){
				RemovePrefab(PrefabRank.Last);
				Trace("Remove Right");
			}
		}

		public void Trace(string header){
			string s0 = (header);
			string s1 = ("World Left Bound = "+worldLeftBound+ " World Right Bound = "+worldRightBound);
			string s2 = ("Camera Left Bound = "+cameraLeftBound+ " Camera Right Bound = "+cameraRightBound);
			string s3 = ("First Element = "+elementsOnScreen[0].leftBound+ " Last Element = "+elementsOnScreen[elementsOnScreen.Count-1].rightBound);
			Debug.Log(s0+"\n"+s1+"\n"+s2+"\n"+s3);
		}
	}

	public enum BackGroundInsertionDirection{
		Left,
		Right
	}

	public enum PrefabRank{
		First,
		Last,
	}

	//
	public class BackGroundElement{
		public GameObject gameObject;
		public float width;
		public float rightBound;
		public float leftBound{
			get{ return rightBound-width;}
		}

		private Renderer renderer;

		public BackGroundElement(GameObject gObj){
			gameObject = gObj;
			renderer = gameObject.GetComponent<Renderer>();
			width = renderer.bounds.size.x;
		}

		public void DisplayAtPosition(Vector3 pos,BackGroundInsertionDirection direction){
			//We are placing the obect based on his center so we /2.0f the width

			int directionFactor;
			if(direction == BackGroundInsertionDirection.Right){
				directionFactor = 1 ;
			}else{
				directionFactor = -1;
			}
			Vector3 newPos = new Vector3(pos.x+((width/2.0f)*directionFactor),pos.y);;
			gameObject.transform.position = newPos;
			gameObject.SetActive(true);
			//We add it to the element on screen

			//We want the right bound, not the center, that's why we don't divide by 2
			if(direction == BackGroundInsertionDirection.Right){
				rightBound = pos.x+(width*directionFactor);
			}else{
				rightBound = pos.x;
			}

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
