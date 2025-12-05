import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GroupLookupItemDto } from '../models/grouplookupitem.dto';

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  private readonly baseUrl = `${environment.apiUrl}/groups`;

  constructor(private http: HttpClient) {}

  getGroups(): Observable<GroupLookupItemDto[]> {
    return this.http.get<GroupLookupItemDto[]>(`${this.baseUrl}/groups/minimal`);
  }
}
