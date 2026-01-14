using PerfumeRecognition.Models;

namespace PerfumeRecognition.Services;

public interface IPerfumeRecognitionService
{
    IReadOnlyList<RecognitionResult> Recognize(
        string imagePath,
        int topK);
}
