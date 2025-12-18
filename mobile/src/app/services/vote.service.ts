import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

import { SetGenderVoteRequestDto } from '../models/setgendervoterequest.dto';
import { SetSillageVoteRequestDto } from '../models/setsillagevoterequest.dto';
import { SetLongevityVoteRequestDto } from '../models/setlongevityvoterequest.dto';
import { SetSeasonVoteRequestDto } from '../models/setseasonvoterequest.dto';
import { SetDaytimeVoteRequestDto } from '../models/setdaytimevoterequest.dto';

@Injectable({
  providedIn: 'root'
})
export class VoteService {
  private readonly baseUrl = `${environment.apiUrl}/perfumes`;

  constructor(private http: HttpClient) {}

  setGenderVote(
    perfumeId: number,
    req: SetGenderVoteRequestDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/${perfumeId}/votes/gender`,
      req
    );
  }

  setSillageVote(
    perfumeId: number,
    req: SetSillageVoteRequestDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/${perfumeId}/votes/sillage`,
      req
    );
  }

  setLongevityVote(
    perfumeId: number,
    req: SetLongevityVoteRequestDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/${perfumeId}/votes/longevity`,
      req
    );
  }

  setSeasonVote(
    perfumeId: number,
    req: SetSeasonVoteRequestDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/${perfumeId}/votes/season`,
      req
    );
  }

  setDaytimeVote(
    perfumeId: number,
    req: SetDaytimeVoteRequestDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/${perfumeId}/votes/daytime`,
      req
    );
  }

  deleteGenderVote(perfumeId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${perfumeId}/votes/gender`);
  }

  deleteLongevityVote(perfumeId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${perfumeId}/votes/longevity`);
  }

  deleteSillageVote(perfumeId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${perfumeId}/votes/sillage`);
  }

  deleteSeasonVote(perfumeId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${perfumeId}/votes/season`);
  }

  deleteDaytimeVote(perfumeId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${perfumeId}/votes/daytime`);
  }
}
