import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UserProfileDto } from '../models/userprofile.dto';
import { UpdateProfileDto } from '../models/updateprofile.dto';
import { ChangePasswordDto } from '../models/changepassword.dto';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/user`;

  constructor(private http: HttpClient) {}

  getMe() {
    return this.http.get<UserProfileDto>(`${this.baseUrl}/me`);
  }

  updateProfile(dto: UpdateProfileDto) {
    return this.http.put<UserProfileDto>(`${this.baseUrl}/profile`, dto);
  }

  changePassword(dto: ChangePasswordDto) {
    return this.http.put<void>(`${this.baseUrl}/password`, dto);
  }

  deleteAccount() {
    return this.http.delete<void>(`${this.baseUrl}`);
  }
}
