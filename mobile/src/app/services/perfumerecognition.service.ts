import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '~/environments/environment';
import { PerfumeRecognitionResultDto } from '../models/perfumerecognitionresult.dto';
import { PerfumeRecognitionRequestDto } from '../models/perfumerecognitionrequest.dto';
import { Observable } from 'rxjs';


@Injectable({ providedIn: 'root' })
export class PerfumeRecognitionService {

  private readonly baseUrl =
    `${environment.apiUrl}/perfume-recognition`;

  constructor(
    private readonly http: HttpClient
  ) {}

  recognize(
    req: PerfumeRecognitionRequestDto
  ): Observable<PerfumeRecognitionResultDto[]> {

    return this.http.post<PerfumeRecognitionResultDto[]>(
      this.baseUrl,
      req
    );
  }
}
