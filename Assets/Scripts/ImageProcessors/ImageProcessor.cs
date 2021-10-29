using UnityEngine;

namespace Fltr {
  public abstract class ImageProcessor
  {
    public abstract void Process(Texture input, Texture output);
  }
}