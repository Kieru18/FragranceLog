import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GroupDictionaryItemDto } from '../models/groupdictionaryitem.dto';

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  private readonly baseUrl = `${environment.apiUrl}/groups`;

  constructor(private http: HttpClient) {}

  getGroupsDictionary(): Observable<GroupDictionaryItemDto[]> {
    return this.http.get<GroupDictionaryItemDto[]>(`${this.baseUrl}/dictionary`);
  }
}
