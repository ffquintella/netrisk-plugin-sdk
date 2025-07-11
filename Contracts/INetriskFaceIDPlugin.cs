using SkiaSharp;

namespace Contracts;

public interface INetriskFaceIDPlugin: INetriskPlugin
{
    public SKBitmap? ExtractFace(SKBitmap bitmap);
    
    public float[]? ExtractEncodings(SKBitmap bitmap);

    public (bool, float) IsSpoofing(SKBitmap bitmap);
}