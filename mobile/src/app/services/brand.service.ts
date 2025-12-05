import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BrandDictionaryItemDto } from '../models/branddictionaryitem.dto';

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private readonly baseUrl = `${environment.apiUrl}/brands`;

  constructor(private http: HttpClient) {}

  getBrandsDictionary(): Observable<BrandDictionaryItemDto[]> {
    return this.http.get<BrandDictionaryItemDto[]>(`${this.baseUrl}/dictionary`);
  }
}
