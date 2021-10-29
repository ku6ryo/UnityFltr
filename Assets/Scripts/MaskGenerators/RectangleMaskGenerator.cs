using UnityEngine;

namespace Fltr {
  public class RectangleMaskGenerator {

    private ComputeShader shader;

    private Vector2 size = new Vector2(100, 100);

    private Vector2 center = new Vector2();

    public RectangleMaskGenerator() {
      shader = (ComputeShader) Resources.Load("Shaders/MaskGenerators/Rectangle");
      if (shader == null) {
        Debug.LogError("Shader not found");
      }
    }

    public void SetCenter(Vector2 center)
    {
      this.center = center;
    }

    public void SetSize(Vector2 size)
    {
      this.size = size;
    }

    public void Generate(int width, int height, Texture output) {
        if (shader == null) {
          Debug.LogError("No shader");
        }
        var kernel = shader.FindKernel("CSMain");
        shader.SetFloats("Size", new float[]{ size.x, size.y });
        shader.SetFloats("Center", new float[]{ center.x, center.y });
        shader.SetTexture(kernel, "Result", output);
        shader.Dispatch(
            kernel,
            width / 8,
            height / 8,
            1
        );
    }
  }
}