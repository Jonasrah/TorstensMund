using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
 
public class DeviceCameraController : MonoBehaviour
{
    public RawImage image;
    public RectTransform imageParent;
    public AspectRatioFitter imageFitter;
    private Renderer[] teethRend;
    public Color teethColor;

    // Device cameras
    WebCamDevice frontCameraDevice;
    WebCamDevice backCameraDevice;
    WebCamDevice activeCameraDevice;

    WebCamTexture frontCameraTexture;
    WebCamTexture backCameraTexture;
    WebCamTexture activeCameraTexture;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);
    [HideInInspector]public Color avgColor;


    void Start()
    {
        Application.targetFrameRate = 60;

        teethRend = new Renderer[GameObject.FindGameObjectsWithTag("Teeth").Length];
        for (int i = 0; i < teethRend.Length; i++) {
            teethRend[i] = GameObject.FindGameObjectsWithTag("Teeth")[i].GetComponent<Renderer>();
        }
        // Check for device cameras
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No devices cameras found");
            return;
        }

        // Get the device's cameras and create WebCamTextures with them
        frontCameraDevice = WebCamTexture.devices.Last();
        backCameraDevice = WebCamTexture.devices.First();

        frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
        backCameraTexture = new WebCamTexture(backCameraDevice.name, 1920, 1080, 30);

        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;

        // Set the camera to use by default
        SetActiveCamera(backCameraTexture);
    }

    // Set the device camera to use and start it
    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }
            
        activeCameraTexture = cameraToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device => 
            device.name == cameraToUse.deviceName);

        image.material.SetTexture("_MainTex", activeCameraTexture);
        //image.material.mainTexture = activeCameraTexture;

        activeCameraTexture.Play();
    }

    // Switch between the device's front and back camera
    /* public void SwitchCamera()
    {
        SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ? 
            backCameraTexture : frontCameraTexture);
    } */
        
    // Make adjustments to image every frame to be safe, since Unity isn't 
    // guaranteed to report correct data as soon as device camera is started
    void Update()
    {
        // Skip making adjustment for incorrect camera data
        if (activeCameraTexture.width < 100)
        {
            //Debug.Log(activeCameraTexture.width);
            return;
        }

        // Rotate image to show correct orientation 
        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        image.transform.localEulerAngles = rotationVector;

        // Set AspectRatioFitter's ratio
        float videoRatio = 
            (float)activeCameraTexture.width / (float)activeCameraTexture.height;
        imageFitter.aspectRatio = videoRatio;

        // Unflip if vertically flipped
        image.uvRect = 
            activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

        // Mirror front-facing camera's image horizontally to look more natural
        imageParent.localScale = 
            !activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;
        

        for (int i = 0; i < teethRend.Length; i++) {
            teethRend[i].material.color = Color.Lerp(teethRend[i].material.color, teethColor, Time.deltaTime);
        }
    }

    public Color GetAvgColor() {
        var colors = activeCameraTexture.GetPixels ((int)(activeCameraTexture.width/2), (int)(activeCameraTexture.height/2), (int)(activeCameraTexture.width * 0.2f), (int)(activeCameraTexture.height * 0.2f)); // no need to look at all of them
			
        float r = 0f, g = 0f, b = 0f;

        for (int i = 0; i < colors.Length; i++) {
            r += colors[i].r;
            g += colors[i].g;
            b += colors[i].b;
        }

        avgColor = new Color(r/colors.Length,g/colors.Length,b/colors.Length);

        //For testing
        for (int i = 0; i < teethRend.Length; i++) {
            teethRend[i].material.color = avgColor * 1.5f;
        }

        return avgColor;
    }
}