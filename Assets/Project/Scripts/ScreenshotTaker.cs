using UnityEngine;
using System.IO; // Required for file and directory operations

public class ScreenshotTaker : MonoBehaviour
{
    [Tooltip("The key to press to take the screenshot")]
    public KeyCode screenshotKey = KeyCode.P;

    [Tooltip("The exact width of the screenshot in pixels")]
    public int captureWidth = 4096;
    [Tooltip("The exact height of the screenshot in pixels")]
    public int captureHeight = 4096;

    private Camera mapCamera;

    void Start()
    {
        // Get the camera this script is attached to
        mapCamera = GetComponent<Camera>();
        if (mapCamera == null)
        {
            Debug.LogError("ScreenshotTaker needs to be attached to a Camera GameObject!");
        }
    }

    void Update()
    {
        // Check if the user pressed the screenshot key
        if (Input.GetKeyDown(screenshotKey))
        {
            if (mapCamera != null)
            {
                CaptureAndSave();
            }
        }
    }

    void CaptureAndSave()
    {
        string filename = "BaseMap.png";

        // --- 1. Define the Save Path ---
        // Application.dataPath is your project's "Assets" folder
        string directoryPath = Path.Combine(Application.dataPath, "Project/OutputFiles");

        // Ensure the directory (e.g., "Assets/Project/OutputFiles") exists
        Directory.CreateDirectory(directoryPath);

        // The full path to the file
        string filePath = Path.Combine(directoryPath, filename);

        try
        {
            // --- 2. Create Textures ---
            // Create a RenderTexture with the desired resolution
            RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24);
            mapCamera.targetTexture = rt; // Tell the camera to render to this texture

            // Create a Texture2D to read the pixels into
            Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);

            // --- 3. Render and Read Pixels ---
            mapCamera.Render(); // Force the camera to render a frame
            RenderTexture.active = rt; // Set the RenderTexture as the active one
            screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0); // Read the pixels
            screenShot.Apply(); // Apply the pixel changes

            // --- 4. Clean up ---
            mapCamera.targetTexture = null; // Unhook the camera
            RenderTexture.active = null; // Unhook the active texture
            Destroy(rt); // Destroy the temporary RenderTexture

            // --- 5. Encode and Save ---
            byte[] bytes = screenShot.EncodeToPNG(); // Encode the texture to a PNG
            Destroy(screenShot); // Destroy the temporary Texture2D

            File.WriteAllBytes(filePath, bytes); // Save the PNG to the file path

            // Log a success message
            Debug.Log($"Screenshot saved as {filename} to: {filePath}");
            Debug.LogWarning("IMPORTANT: You must refresh your Unity editor (click back into it) to see the new file in the Project window!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save screenshot: {e.Message}");
        }
    }
}