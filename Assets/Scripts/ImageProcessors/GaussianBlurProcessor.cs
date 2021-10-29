using UnityEngine;

namespace Fltr {
  public class GaussianBlurProcessor: ImageProcessor {

    private ComputeShader shader;

    private Texture mask;

    public GaussianBlurProcessor() {
      shader = (ComputeShader) Resources.Load("Shaders/ImageProcessors/GaussianBlur");
      if (shader == null) {
        Debug.LogError("Shader not found");
      }
      SetMask(new Texture2D(100, 100, TextureFormat.RGBA32, 0, true));
    }

    public void SetMask(Texture tex)
    {
      mask = tex;
    }

    public override void Process(Texture input, Texture output) {
        if (shader == null) {
          Debug.LogError("No shader");
        }
        var kernel = shader.FindKernel("CSMain");
        shader.SetTexture(kernel, "Input", input);
        shader.SetTexture(kernel, "Result", output);
        shader.SetTexture(kernel, "Mask", mask);
        shader.Dispatch(
            kernel,
            input.width / 8,
            input.height / 8,
            1
        );
    }
  }
}