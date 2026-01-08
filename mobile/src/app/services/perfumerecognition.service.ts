import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PerfumeRecognitionResultDto } from '../models/perfumerecognitionresult.dto';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PerfumeRecognitionService {

  private readonly baseUrl = `${environment.apiUrl}/perfume-recognition`;

  constructor(
    private readonly http: HttpClient
  ) { }

  recognize(
    image: File,
    topK: number = 3
  ): Observable<PerfumeRecognitionResultDto[]> {

    const formData = new FormData();
    formData.append('image', image);

    const params = new HttpParams()
      .set('topK', topK.toString());

    return this.http.post<PerfumeRecognitionResultDto[]>(
      this.baseUrl,
      formData,
      { params }
    );
  }
}
