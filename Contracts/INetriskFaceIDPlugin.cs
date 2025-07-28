using SkiaSharp;

namespace Contracts;

public interface INetriskFaceIDPlugin: INetriskPlugin
{
    /// <summary>
    /// Extracts the face region from the provided image.
    /// </summary>
    /// <param name="bitmap">Input image.</param>
    /// <returns>The extracted face bitmap or null if no face detected.</returns>
    public SKBitmap? ExtractFace(SKBitmap bitmap);
    
    /// <summary>
    /// Extracts facial encodings from the provided image.
    /// </summary>
    /// <param name="bitmap">Input image.</param>
    /// <returns>Array of facial encoding floats or null if extraction fails.</returns>
    public float[]? ExtractEncodings(SKBitmap bitmap);

    /// <summary>
    /// Detects spoofing in the provided image.
    /// </summary>
    /// <param name="bitmap">Input image.</param>
    /// <returns>A tuple where 'isSpoof' indicates spoof detection and 'confidence' indicates probability.</returns>
    public (bool isSpoof, float confidence) IsSpoofing(SKBitmap bitmap);
}