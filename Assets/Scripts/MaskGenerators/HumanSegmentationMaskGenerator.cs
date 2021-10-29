using UnityEngine;
using Unity.Barracuda;

namespace Fltr {
  public class HumanSegmentationMaskGenerator : System.IDisposable
  {
        // Segmentation result texutre. Resolution is the same as input texture.
        public RenderTexture texture;

        #region constant number
        // Input and output image size defined by neural network model.
        const int IMAGE_SIZE = 256; 
        // Input image channel defined by neural network model.
        const int IN_CH = 3;
        // Output image channel defined by neural network model.
        const int OUT_CH = 1;
        #endregion

        #region private variables
        Model model;
        IWorker woker;
        ComputeShader preProcessShader;
        ComputeBuffer networkInputBuffer;
        #endregion

        #region public methods
        public HumanSegmentationMaskGenerator() {
            preProcessShader = (ComputeShader) Resources.Load("Shaders/ImageProcessors/HumanSegmentation/Preprocess");
            if (preProcessShader == null) {
              Debug.LogError("pre-process shader not found.");
            }

            networkInputBuffer = new ComputeBuffer(IMAGE_SIZE * IMAGE_SIZE * IN_CH, sizeof(float));
            // Initialize with the resolution of IMAGE_SIZE * IMAGE_SIZE, 
            // but resize it later according to the resolution of the input texture.
            texture = new RenderTexture(IMAGE_SIZE, IMAGE_SIZE, 0, RenderTextureFormat.ARGB32);

            var nnModel = (NNModel) Resources.Load("Shaders/ImageProcessors/HumanSegmentation/segmentation");
            
            // Prepare neural network model.
            model = ModelLoader.Load(nnModel);
            woker = model.CreateWorker();
        }

        public void ProcessImage(Texture inputTexture){
            // Resize `inputTexture` texture to network model image size.
            preProcessShader.SetTexture(0, "_inputTexture", inputTexture);
            preProcessShader.SetBuffer(0, "_output", networkInputBuffer);
            preProcessShader.Dispatch(0, IMAGE_SIZE / 8, IMAGE_SIZE / 8, 1);

            // Execute neural network model.
            var inputTensor = new Tensor(1, IMAGE_SIZE, IMAGE_SIZE, IN_CH, networkInputBuffer);
            woker.Execute(inputTensor);
            inputTensor.Dispose();

            // Get segmentation output as RenderTexture.
            var segTemp = CopyOutputToTempRT("activation_10", IMAGE_SIZE, IMAGE_SIZE, OUT_CH);
            
            if(texture.width != inputTexture.width || texture.height != inputTexture.height){
                // Resize to the same resolution as input texture.
                texture?.Release();
                texture = new RenderTexture(inputTexture.width, inputTexture.height, 0, RenderTextureFormat.ARGB32);
            }

            // Render to segmentation texture to output texture.
            Graphics.Blit(segTemp, texture);
            
            RenderTexture.ReleaseTemporary(segTemp);
        }

        public void Dispose(){
            networkInputBuffer?.Dispose();
            woker?.Dispose();
            texture?.Release();
        }
        #endregion

        RenderTexture CopyOutputToTempRT(string name, int w, int h, int ch)
        {
            var rtFormat = RenderTextureFormat.ARGB32;
            var shape = new TensorShape(1, h, w, ch);
            var rt = RenderTexture.GetTemporary(w, h, 0, rtFormat);
            var tensor = woker.PeekOutput(name).Reshape(shape);
            tensor.ToRenderTexture(rt);
            tensor.Dispose();
            return rt;
        }
  }
}