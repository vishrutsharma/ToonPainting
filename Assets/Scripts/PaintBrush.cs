using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode
{
    POLISHING,
    PAINTING
}
public class PaintBrush : MonoBehaviour
{

    public List<GameObject> selectedBrushObjects;
    public Slider rotationSlider;
    public Slider sampleSlider;
    public Slider strengthSlider;
    public Slider brightnessSlider;
    public GameObject colorCanvas;
    public GameMode gameModeType;
    public List<GameObject> paintableObjects;
    public Material colorPainterMat;
    public Material dustRemovalMat;
    public GameObject brush;
    public int resolution = 512;
    public Texture2D whiteMapDust;
    public Texture2D whiteMapPaint;
    public float brushSize;
    public Texture2D brushTexture;
    public List<Texture2D> colorBrushTextures;
    private Texture2D desiredTexture;
    Vector2 stored;

    public static Dictionary<Collider, RenderTexture> paintTextures = new Dictionary<Collider, RenderTexture>();
    void Start()
    {
        SwitchGameMode();
        sampleSlider.value = paintableObjects[0].GetComponent<Renderer>().material.GetFloat("_SampleStrength");
        strengthSlider.value = paintableObjects[0].GetComponent<Renderer>().material.GetFloat("_Strength");
        brightnessSlider.value = paintableObjects[0].GetComponent<Renderer>().material.GetFloat("_Brightness");
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.DrawRay(transform.position, Vector3.down * 20f, Color.magenta);
            RaycastHit hit;
            // if (Physics.Raycast(transform.position, Vector3.down, out hit))
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) // delete previous and uncomment for mouse painting
            {
                Collider coll = hit.collider;
                if (coll != null)
                {

                    if (!paintTextures.ContainsKey(coll)) // if there is already paint on the material, add to that
                    {
                        Renderer rend = hit.transform.GetComponent<Renderer>();
                        paintTextures.Add(coll, getWhiteRT());
                        rend.material.SetTexture("_PaintMap", paintTextures[coll]);
                    }
                    if (stored != hit.lightmapCoord) // stop drawing on the same point
                    {
                        stored = hit.lightmapCoord;
                        Vector2 pixelUV = hit.lightmapCoord;
                        pixelUV.y *= resolution;
                        pixelUV.x *= resolution;
                        DrawTexture(paintTextures[coll], pixelUV.x, pixelUV.y);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (gameModeType == GameMode.POLISHING)
            {

                StopAllCoroutines();
            }
        }

    }

    void DrawTexture(RenderTexture rt, float posX, float posY)
    {

        RenderTexture.active = rt; // activate rendertexture for drawtexture;
        GL.PushMatrix();                       // save matrixes
        GL.LoadPixelMatrix(0, resolution, resolution, 0);      // setup matrix for correct size

        // draw brushtexture
        Graphics.DrawTexture(new Rect(posX - desiredTexture.width / brushSize, (rt.height - posY) - desiredTexture.height / brushSize, desiredTexture.width / (brushSize * 0.5f), desiredTexture.height / (brushSize * 0.5f)), desiredTexture);
        GL.PopMatrix();
        RenderTexture.active = null;// turn off rendertexture

    }

    RenderTexture getWhiteRT()
    {
        RenderTexture rt = new RenderTexture(resolution, resolution, 32);
        Graphics.Blit(gameModeType == GameMode.POLISHING ? whiteMapDust : whiteMapPaint, rt);
        return rt;
    }

    private void SwitchGameMode()
    {
        if (gameModeType == GameMode.POLISHING)
        {
            colorCanvas.SetActive(false);
            desiredTexture = brushTexture;
            foreach (GameObject ob in paintableObjects)
            {
                ob.GetComponent<Renderer>().material = dustRemovalMat;
            }
        }
        else
        {
            colorCanvas.SetActive(true);
            foreach (GameObject ob in paintableObjects)
            {
                ob.GetComponent<Renderer>().material = colorPainterMat;
            }
            desiredTexture = colorBrushTextures[0];
        }
    }

    public void OnColorSelect(int index)
    {
        if (gameModeType == GameMode.PAINTING)
        {
            desiredTexture = colorBrushTextures[index];
            foreach (GameObject g in selectedBrushObjects)
            {
                g.SetActive(false);
            }
            selectedBrushObjects[index].SetActive(true);
        }

    }

    public void ToggleGameMode()
    {
        gameModeType = gameModeType == GameMode.POLISHING ? GameMode.PAINTING : GameMode.POLISHING;
        paintTextures.Clear();
        SwitchGameMode();
    }

    void CreateClearTexture()
    {
        // whiteMap = new Texture2D(1, 1);
        // whiteMap.SetPixel(0, 0, Color.white);
        // whiteMap.Apply();
    }

    public void OnStrengthValueChanged()
    {
        foreach (GameObject ob in paintableObjects)
        {
            ob.GetComponent<Renderer>().material.SetFloat("_Strength", strengthSlider.value);
        }
    }

    public void OnSampleValueChanged()
    {
        foreach (GameObject ob in paintableObjects)
        {
            ob.GetComponent<Renderer>().material.SetFloat("_SampleStrength", sampleSlider.value);
        }
    }

    public void OnBrightnessValueChanged()
    {
        foreach (GameObject ob in paintableObjects)
        {
            ob.GetComponent<Renderer>().material.SetFloat("_Brightness", brightnessSlider.value);
        }
    }

    public void OnRotationValueChanged()
    {
        foreach (GameObject ob in paintableObjects)
        {
            ob.GetComponent<Rotator>().rotationSpeed = rotationSlider.value * 100;
        }
    }
}