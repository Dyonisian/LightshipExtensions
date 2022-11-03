using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakePhoto : MonoBehaviour
{
    [SerializeField]
    Text debugText;
    [SerializeField]
    GameObject photoUI;
    float timerDelay = 0.0f;
    [SerializeField]
    Text timerText;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator CaptureScreenshot()
    {
        yield return new WaitForSeconds(timerDelay);
        yield return new WaitForEndOfFrame();

        Texture2D screenImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //Get Image from screen
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();

        //NativeGallery.Permission NativeGallery.SaveImageToGallery(Texture2D image, string album, string filename, MediaSaveCallback callback = null);
        //byte[] imageBytes = screenImage.EncodeToJPG();


        NativeGallery.Permission per = NativeGallery.SaveImageToGallery(screenImage, "Recent", "FannyStella");
        if (debugText)
        {
            debugText.gameObject.SetActive(true);
            debugText.transform.parent.gameObject.SetActive(true);
            debugText.transform.parent.parent.gameObject.SetActive(true);
            debugText.text = per.ToString();
        }
        Destroy(screenImage);
        //Convert to png       
    }
    /// <summary>
    /// Call this function to take a photo
    /// </summary>
    public void TakeAPhoto()
    {
        StopCoroutine(CaptureScreenshot());
        StartCoroutine(CaptureScreenshot());
    }
    public void ToggleMenu()
    {
        photoUI.SetActive(!photoUI.activeSelf);
    }
    /// <summary>
    /// Use this to set a timer delay for taking a photo 
    /// </summary>
    /// <param name="x">Delay in seconds</param>
    public void ChangeTimer(float x)
    {
        timerDelay += x;
        if (timerDelay < 0.0f)
            timerDelay = 0.0f;
        timerText.text = "Timer: " + timerDelay + " sec";
    }
}
