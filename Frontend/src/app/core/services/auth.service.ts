import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RegisterDto } from '../../models/register.dto';
import { LoginDto } from '../../models/login.dto';
import { Observable } from 'rxjs';
import { LoginResponseDto } from '../../models/login-response.dto';
import { User } from '../../models/user.model';
import { environment } from '../../../environments/environment';
import { IDataResult } from 'src/app/models/IDataResult';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  register(registerData: RegisterDto): Observable<IDataResult<User>> {
    return this.http.post<IDataResult<User>>(`${this.apiUrl}/auth/register`, registerData);
  }

  login(loginData: LoginDto): Observable<IDataResult<LoginResponseDto>> {
    return this.http.post<IDataResult<LoginResponseDto>>(`${this.apiUrl}/auth/login`, loginData);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

}
