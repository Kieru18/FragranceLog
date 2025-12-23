import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SharedListDto } from '../models/sharedlist.dto';
import { SharedListPreviewDto } from '../models/sharedlistpreview.dto';

@Injectable({ 
    providedIn: 'root' 
})
export class SharedListService {
  private readonly baseUrl = `${environment.apiUrl}/shared-lists`;

  constructor(private http: HttpClient) {}

  getSharedListPreview(token: string): Observable<SharedListPreviewDto> {
    return this.http.get<SharedListPreviewDto>(
      `${this.baseUrl}/${token}`
    );
  }

  importSharedList(token: string): Observable<number> {
    return this.http.post<number>(
      `${this.baseUrl}/${token}/import`,
      {}
    );
  }
}
