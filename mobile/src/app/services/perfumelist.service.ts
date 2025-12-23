import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PerfumeListDto } from '../models/perfumelist.dto';
import { PerfumeListItemDto } from '../models/perfumelistitem.dto';
import { PerfumeListOverviewDto } from '../models/perfumelistoverview.dto';
import { PerfumeListMembershipDto } from '../models/perfumelistmembership.dto';

@Injectable({
  providedIn: 'root'
})
export class PerfumeListService {
  private readonly baseUrl = `${environment.apiUrl}/lists`;

  constructor(private http: HttpClient) {}

  getLists(): Observable<PerfumeListDto[]> {
    return this.http.get<PerfumeListDto[]>(this.baseUrl);
  }

  getListsOverview(): Observable<PerfumeListOverviewDto[]> {
    return this.http.get<PerfumeListOverviewDto[]>(
      `${this.baseUrl}/overview`
    );
  }
  createList(name: string): Observable<PerfumeListDto> {
    return this.http.post<PerfumeListDto>(this.baseUrl, { name });
  }

  renameList(listId: number, name: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${listId}`, { name });
  }

  deleteList(listId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${listId}`);
  }

  getListPerfumes(listId: number): Observable<PerfumeListItemDto[]> {
    return this.http.get<PerfumeListItemDto[]>(
      `${this.baseUrl}/${listId}/perfumes`
    );
  }

  addPerfumeToList(listId: number, perfumeId: number): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/${listId}/perfumes/${perfumeId}`,
      null
    );
  }

  removePerfumeFromList(listId: number, perfumeId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/${listId}/perfumes/${perfumeId}`
    );
  }

  getListsForPerfume(perfumeId: number) {
    return this.http.get<PerfumeListMembershipDto[]>(
      `${this.baseUrl}/for-perfume/${perfumeId}`
    );
  }

  getList(listId: number): Observable<PerfumeListDto> {
    return this.http.get<PerfumeListDto>(
      `${this.baseUrl}/${listId}`
    );
  }
}
