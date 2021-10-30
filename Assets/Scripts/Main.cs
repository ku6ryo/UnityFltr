using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazeFace;
using Unity.Barracuda;

namespace Fltr {
public class Main : MonoBehaviour
{
    [SerializeField]
    private RawImage finalImage;

    [SerializeField]
    private Texture2D dummyImage;
    private RenderTexture resultRenderTexture;
    private RenderTexture resultRenderTexture2;
    private RenderTexture maskTexture;
    private RenderTexture maskTexture2;
    private HumanSegmentationMaskGenerator h;
    WebCamTexture cameraTexture;

    FaceDetector faceDetector;

    [SerializeField]
    RenderTexture videoRenderTexture;

    // Start is called before the first frame update
    void Start()
    {
        int resolutionX = 1920;
        int resolutionY = 1080;
        cameraTexture = new WebCamTexture("", resolutionX, resolutionY);
        cameraTexture.Play();
        h = new HumanSegmentationMaskGenerator();

        resultRenderTexture = new RenderTexture(resolutionX, resolutionY, 1, RenderTextureFormat.ARGBFloat);
        resultRenderTexture.enableRandomWrite = true;
        resultRenderTexture.Create();

        resultRenderTexture2 = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGBFloat);
        resultRenderTexture2.enableRandomWrite = true;
        resultRenderTexture2.Create();

        maskTexture = new RenderTexture(resolutionX, resolutionY, 0);
        maskTexture.enableRandomWrite = true;
        maskTexture.Create();

        maskTexture2 = new RenderTexture(resolutionX, resolutionY, 0);
        maskTexture2.enableRandomWrite = true;
        maskTexture2.Create();

        faceDetector = new FaceDetector();
    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraTexture.didUpdateThisFrame) return;

        var edge = new EdgeDetectionMaskGenerator();
        edge.SetSensitivity(10);

        var crop = new ImageCropProcessor();
        var monoColor = new MonoColorProcessor();
        var circleMask = new CircleMaskGenerator();

        h.ProcessImage(cameraTexture);
        crop.SetMask(h.texture);
        crop.SetImage(videoRenderTexture);
        crop.Process(cameraTexture, resultRenderTexture);

        edge.Generate(cameraTexture, maskTexture);
        crop.SetImage(maskTexture);
        crop.SetMask(h.texture);
        crop.Process(dummyImage, maskTexture2);

        monoColor.SetMask(maskTexture2);
        monoColor.Process(resultRenderTexture, resultRenderTexture2);

        faceDetector.ProcessImage(cameraTexture);
        Vector2 center = new Vector2();
        foreach (var dct in faceDetector.Detections) {
          Debug.Log("a");
          center = new Vector2(
              dct.center.x * cameraTexture.width,
              dct.center.y * cameraTexture.height
          );
        }
        Debug.Log("-------");
        circleMask.SetCenter(center);
        circleMask.SetRadius(100);
        circleMask.Generate(cameraTexture.width, cameraTexture.height, maskTexture);
        crop.SetMask(maskTexture);
        crop.SetImage(dummyImage);
        crop.Process(resultRenderTexture2, resultRenderTexture);
        /*
        monoColor.SetColor(new Color(0, 1, 1, 1));
        monoColor.SetMask(maskTexture);
        monoColor.Process(resultRenderTexture2, resultRenderTexture);
        */
        finalImage.texture = resultRenderTexture;
    }
    void OnDestroy()
    {
        if (cameraTexture != null) Destroy(cameraTexture);
        if (resultRenderTexture != null) Destroy(resultRenderTexture);
        if (resultRenderTexture2 != null) Destroy(resultRenderTexture2);
        if (maskTexture != null) Destroy(maskTexture);
        if (maskTexture2 != null) Destroy(maskTexture2);
    }
}
}