import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { PerfumeSuggestionRequestDto } from '../models/perfumesuggestionrequest.dto';

@Injectable({ providedIn: 'root' })
export class PerfumeSuggestionService {
  private readonly baseUrl = `${environment.apiUrl}/perfume-suggestions`;

  constructor(private http: HttpClient) {}

  submit(dto: PerfumeSuggestionRequestDto) {
    return this.http.post(this.baseUrl, dto);
  }
}
