import { PerfumeRecognitionConfidence } from '../enums/perfumerecognitionconfidence.enum';

export interface PerfumeRecognitionResultDto {
  perfumeId: number;
  score: number;
  perfumeName: string;
  brandName: string;
  imageUrl?: string;
  confidence: PerfumeRecognitionConfidence;
}
