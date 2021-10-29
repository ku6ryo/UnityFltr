using UnityEngine;

namespace Fltr {
  public class CircleMaskGenerator {

    private ComputeShader shader;

    private float radius = 100;

    private Vector2 center = new Vector2();

    public CircleMaskGenerator() {
      shader = (ComputeShader) Resources.Load("Shaders/MaskGenerators/Circle");
      if (shader == null) {
        Debug.LogError("Shader not found");
      }
    }

    public void SetCenter(Vector2 center)
    {
      this.center = center;
    }

    public void SetRadius(float r)
    {
      radius = r;
    }

    public void Generate(int width, int height, Texture output) {
        if (shader == null) {
          Debug.LogError("No shader");
        }
        var kernel = shader.FindKernel("CSMain");
        shader.SetFloat("Radius", radius);
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