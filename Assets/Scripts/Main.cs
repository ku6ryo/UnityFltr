using UnityEngine;
using UnityEngine.UI;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraTexture.didUpdateThisFrame) return;

        var edge = new EdgeDetectionMaskGenerator();
        edge.SetSensitivity(10);

        var crop = new ImageCropProcessor();
        var monoColor = new MonoColorProcessor();

        h.ProcessImage(cameraTexture);
        crop.SetMask(h.texture);
        crop.SetImage(dummyImage);
        crop.Process(cameraTexture, resultRenderTexture);

        edge.Generate(cameraTexture, maskTexture);
        crop.SetImage(maskTexture);
        crop.SetMask(h.texture);
        crop.Process(dummyImage, maskTexture2);

        monoColor.SetMask(maskTexture2);
        monoColor.Process(resultRenderTexture, resultRenderTexture2);

        /*
        var mask = new CircleMaskGenerator();
        mask.SetCenter(new Vector2(cameraImage.width / 2 * Mathf.Sin(Time.time), cameraImage.height / 2));
        mask.SetRadius(Mathf.Sin(Time.time) * 300);
        mask.Generate(cameraImage.width, cameraImage.height, maskRenderTexture);

        var mask2 = new RectangleMaskGenerator();
        mask2.SetCenter(new Vector2(cameraImage.width / 2, cameraImage.height / 2));
        mask2.Generate(cameraImage.width, cameraImage.height, maskRenderTexture);
        var processor = (GaussianBlurProcessor) processors[0];
        processor.SetMask(maskRenderTexture);
        processor.Process(resultRenderTexture, resultRenderTexture2);

        finalImage.texture = resultRenderTexture2; 
        */
        /*
        var processor2 = (MonoColorProcessor) processors[1];
        processor2.SetColor(new Color(1, 1, 0, 1));
        processor2.SetMask(maskTexture)
        processor2.Process(cameraTexture, resultRenderTexture);
        */
        finalImage.texture = maskTexture2; 
    }
    void OnDestroy()
    {
        if (cameraTexture != null) Destroy(cameraTexture);
        if (resultRenderTexture != null) Destroy(resultRenderTexture);
        if (maskTexture != null) Destroy(maskTexture);
        if (maskTexture2 != null) Destroy(maskTexture2);
    }
}
}