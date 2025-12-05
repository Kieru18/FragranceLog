import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BrandLookupItemDto } from '../models/brandlookupitem.dto';

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private readonly baseUrl = `${environment.apiUrl}/brands`;

  constructor(private http: HttpClient) {}

  getBrands(): Observable<BrandLookupItemDto[]> {
    return this.http.get<BrandLookupItemDto[]>(`${this.baseUrl}/brands/minimal`);
  }
}
