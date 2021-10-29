using UnityEngine;

namespace Fltr {

public class MonoColorProcessor : ImageProcessor {

    private ComputeShader shader;

    private Texture mask;

    private Color color = new Color(1, 0, 0, 0);

    public MonoColorProcessor() {
      shader = (ComputeShader) Resources.Load("Shaders/ImageProcessors/MonoColor");
      if (shader == null) {
        Debug.LogError("Shader not found");
      }
      SetMask(new Texture2D(100, 100, TextureFormat.RGBA32, 0, true));
    }

    public void SetMask(Texture tex)
    {
      mask = tex;
    }

    public void SetColor(Color color) {
      this.color = color;
    }

    public override void Process(Texture input, Texture output) {
        if (shader == null) {
          Debug.LogError("No shader");
        }
        var kernel = shader.FindKernel("CSMain");
        shader.SetTexture(kernel, "Input", input);
        shader.SetTexture(kernel, "Result", output);
        shader.SetTexture(kernel, "Mask", mask);
        shader.SetFloats("Color", new float[]{ color.r, color.g, color.b, color.a });
        shader.Dispatch(
            kernel,
            input.width / 8,
            input.height / 8,
            1
        );
    }
  }
}