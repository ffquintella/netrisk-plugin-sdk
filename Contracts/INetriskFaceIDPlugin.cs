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
    
    /// <summary>
    /// Checks if the provided color matches the specified key in the image.
    /// </summary>
    /// <param name="colorKey"></param>
    /// <param name="bitmap1"></param>
    /// <returns></returns>
    public (bool isMatch, float confidence)  CheckColorMatch(char colorKey, SKBitmap bitmap1);
    
    /// <summary>
    /// Checks if the provided color matches the specified key in the image, using a double value for color matching.
    /// </summary>
    /// <param name="colorKey"></param>
    /// <param name="bitmap1"></param>
    /// <param name="bitmap2"></param>
    /// <returns></returns>
    public (bool isMatch, float confidence)  CheckColorMatchDouble(char colorKey, SKBitmap bitmap1, SKBitmap bitmap2);
    
    /// <summary>
    /// Checks if the provided color sequence matches the expected sequence in the image sequence.
    /// </summary>
    /// <param name="colorKeys"></param>
    /// <param name="imageSequence"></param>
    /// /// <param name="useDouble">If use double is true the first image on the sequence is used as the off image</param>
    /// <returns></returns>
    public bool CheckColorSequence(char[] colorKeys, List<SKBitmap> imageSequence, bool useDouble = false);
}