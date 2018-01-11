using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class Main : MonoBehaviour 
{
	[SerializeField] Animator animator;
	[SerializeField] RawImage cameraFeed;
	[SerializeField] Camera snapCamera;
	[SerializeField] GameObject chewModel;
	[SerializeField] Rigidbody followSphere;
	private Renderer snapshotModelRenderer;
	[SerializeField] AudioSource ahhAudioSource;
	[SerializeField] AudioSource chewAudioSource;
	[SerializeField] AudioSource reactionAudioSource;

	[System.Serializable]
	public class FoodColor 
	{
		public Color color;
		public string name;
		public AudioClip[] clips;
	}

	[SerializeField] FoodColor[] foodColors;

	void Awake()
	{
		StartCoroutine(Sequence());
	}

	IEnumerator Sequence() 
	{
		WebCamDevice[] devices = WebCamTexture.devices;
		string camDevice = WebCamTexture.devices.First().name;
		WebCamTexture webCamTexture = new WebCamTexture(camDevice, 1280, 720, 30);
		webCamTexture.Play();

		while (true)
		{
			while (!Input.GetMouseButton(0))
			{
				yield return null;
			}

			reactionAudioSource.Stop();

			animator.SetBool("IsOpen", true);

			while (Input.GetMouseButton(0))
			{
				yield return null;
			}

			chewAudioSource.Play();

			StartCoroutine(Snapshot());

			animator.SetBool("IsOpen", false);

            HSBColor hsb;

            float[] diffs = new float[foodColors.Length];
			Color avgColor = cameraFeed.gameObject.GetComponent<DeviceCameraController>().GetAvgColor();

            for(int i = 0; i < foodColors.Length; i++)
            {
            	diffs[i] = 0f;

            	var hsbTarget = new HSBColor(foodColors[i].color);
				
				diffs[i] += Mathf.Abs (HSBColor.Distance (HSBColor.FromColor(avgColor), hsbTarget));
	            
            }

            int minIndex = 0;
            float min = diffs[0];

            for (int i = 1; i < foodColors.Length; i++)
            {
//            	Debug.Log(i + " " + diffs[i]);
            	if (diffs[i] < min)
            	{
            		minIndex = i;
            		min = diffs[i];
            	}
            }

			//Debug.Log(foodColors[minIndex].name);

			while (chewAudioSource.isPlaying)
			{
				yield return null;
			}

			Destroy(GameObject.FindGameObjectWithTag("ChewBits"));

            var foodColor = foodColors[minIndex];
            reactionAudioSource.PlayOneShot(foodColor.clips[Random.Range(0, foodColor.clips.Length)]);
		}
	}

	IEnumerator Snapshot() {

		//enable model
		GameObject chewClone = Instantiate(chewModel, new Vector3(-0.615f, -0.425f, 0.118f), Quaternion.Euler(0,90,0));

		//Get the right camera
		Camera mainCam = Camera.main;
		mainCam.enabled = false;
		snapCamera.enabled = true;

		// wait for graphics to render
		yield return new WaitForEndOfFrame();
	
		// create a texture to pass to encoding
		Texture2D texture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
	
		// assign new texture to variable
		snapshotModelRenderer = chewClone.GetComponentInChildren<Renderer>();
		snapshotModelRenderer.material.SetTexture("_MainTex", texture);
	
		// put buffer into texture
		texture.ReadPixels(new Rect(0,0, Screen.width, Screen.height), 0, 0);
		texture.Apply();
		
		//Turn camera back on
		mainCam.enabled = true;
		snapCamera.enabled = false;
	}
	
}
