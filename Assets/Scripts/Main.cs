using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Fltr {
public class Main : MonoBehaviour
{
    [SerializeField]
    private RawImage finalImage;

    [SerializeField]
    private Texture2D cameraImage;
    private RenderTexture resultRenderTexture;
    private RenderTexture resultRenderTexture2;
    private RenderTexture maskRenderTexture;

    private List<ImageProcessor> processors = new List<ImageProcessor>();
    private HumanSegmentationMaskGenerator h;

    WebCamTexture _webcamTexture;

    // Start is called before the first frame update
    void Start()
    {
        _webcamTexture = new WebCamTexture("", 1920, 1080);
        _webcamTexture.Play();
        h = new HumanSegmentationMaskGenerator();

        processors.Add(new GaussianBlurProcessor());
        processors.Add(new MonoColorProcessor());

        resultRenderTexture = new RenderTexture(cameraImage.width, cameraImage.height, 1, RenderTextureFormat.ARGBFloat);
        resultRenderTexture.enableRandomWrite = true;
        resultRenderTexture.Create();

        resultRenderTexture2 = new RenderTexture(cameraImage.width, cameraImage.height, 0, RenderTextureFormat.ARGBFloat);
        resultRenderTexture2.enableRandomWrite = true;
        resultRenderTexture2.Create();

        maskRenderTexture = new RenderTexture(cameraImage.width, cameraImage.height, 0);
        maskRenderTexture.enableRandomWrite = true;
        maskRenderTexture.Create();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_webcamTexture.didUpdateThisFrame) return;

        var cameraImage = _webcamTexture;

        var mask = new CircleMaskGenerator();
        mask.SetCenter(new Vector2(cameraImage.width / 2 * Mathf.Sin(Time.time), cameraImage.height / 2));
        mask.SetRadius(Mathf.Sin(Time.time) * 300);
        mask.Generate(cameraImage.width, cameraImage.height, maskRenderTexture);
        h.ProcessImage(cameraImage);
        var processor2 = (MonoColorProcessor) processors[1];
        processor2.SetColor(new Color(1, 1, 0, 1));
        processor2.SetMask(h.texture);
        processor2.Process(_webcamTexture, resultRenderTexture);

        var mask2 = new RectangleMaskGenerator();
        mask2.SetCenter(new Vector2(cameraImage.width / 2, cameraImage.height / 2));
        mask2.Generate(cameraImage.width, cameraImage.height, maskRenderTexture);
        var processor = (GaussianBlurProcessor) processors[0];
        processor.SetMask(maskRenderTexture);
        processor.Process(resultRenderTexture, resultRenderTexture2);

        finalImage.texture = resultRenderTexture2; 
    }
    void OnDestroy()
    {
        if (_webcamTexture != null) Destroy(_webcamTexture);
        if (resultRenderTexture != null) Destroy(resultRenderTexture);
        if (maskRenderTexture != null) Destroy(maskRenderTexture);
    }
}
}