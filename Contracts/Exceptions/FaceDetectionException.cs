namespace Contracts.Exceptions;

public class FaceDetectionException(string message, Exception? innerException = null) : Exception(message, innerException);