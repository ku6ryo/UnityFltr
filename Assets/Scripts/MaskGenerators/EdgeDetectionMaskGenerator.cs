using UnityEngine;

namespace Fltr {
  public class EdgeDetectionMaskGenerator {

    private ComputeShader shader;

    private float sensitivity = 1;

    public EdgeDetectionMaskGenerator() {
      shader = (ComputeShader) Resources.Load("Shaders/MaskGenerators/EdgeDetection");
      if (shader == null) {
        Debug.LogError("Shader not found");
      }
    }

    public void SetSensitivity(float s) {
      sensitivity = s;
    }

    public void Generate(Texture input, Texture output) {
        if (shader == null) {
          Debug.LogError("No shader");
        }
        var kernel = shader.FindKernel("CSMain");
        shader.SetTexture(kernel, "Input", input);
        shader.SetTexture(kernel, "Result", output);
        shader.SetFloat("Sensitivity", sensitivity);
        shader.Dispatch(
            kernel,
            input.width / 8,
            input.height / 8,
            1
        );
    }
  }
}