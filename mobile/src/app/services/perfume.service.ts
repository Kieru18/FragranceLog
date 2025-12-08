import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PerfumeSearchResponseDto } from '../models/perfumesearchresponse.dto';
import { PerfumeSearchRequestDto } from '../models/perfumesearchrequest.dto';
import { PerfumeDetailsDto } from '../models/perfumedetails.dto';

@Injectable({
  providedIn: 'root'
})
export class PerfumeService {
  private readonly baseUrl = `${environment.apiUrl}/perfumes`;

  constructor(private http: HttpClient) {}

  searchPerfumes(req: PerfumeSearchRequestDto): Observable<PerfumeSearchResponseDto> {
    return this.http.post<PerfumeSearchResponseDto>(
      `${this.baseUrl}/search`,
      req
    );
  }

  getPerfumeDetails(id: number): Observable<PerfumeDetailsDto> {
    return this.http.get<PerfumeDetailsDto>(`${this.baseUrl}/${id}`);
  }
}
