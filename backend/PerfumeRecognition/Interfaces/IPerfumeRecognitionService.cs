using PerfumeRecognition.Models;

namespace PerfumeRecognition.Interfaces;

public interface IPerfumeRecognitionService
{
    IReadOnlyList<RecognitionResult> Recognize(
        string imagePath,
        int topK);
}
