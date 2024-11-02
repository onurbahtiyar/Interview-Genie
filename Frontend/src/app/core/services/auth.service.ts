import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RegisterDto } from '../../models/register.dto';
import { LoginDto } from '../../models/login.dto';
import { Observable } from 'rxjs';
import { LoginResponseDto } from '../../models/login-response.dto';
import { User } from '../../models/user.model';
import { environment } from '../../../environments/environment';
import { IDataResult } from 'src/app/models/IDataResult';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

export interface JwtToken {
  exp: number;
  // Diğer alanlar
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private router: Router) { }

  register(registerData: RegisterDto): Observable<IDataResult<User>> {
    return this.http.post<IDataResult<User>>(`${this.apiUrl}/auth/register`, registerData);
  }

  login(loginData: LoginDto): Observable<IDataResult<LoginResponseDto>> {
    return this.http.post<IDataResult<LoginResponseDto>>(`${this.apiUrl}/auth/login`, loginData);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) {
      return true;
    }

    try {
      const decoded = jwtDecode<JwtToken>(token);
      const expiryTime = decoded.exp * 1000;
      return Date.now() > expiryTime;
    } catch (error) {
      return true;
    }
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.router.navigate(['/auth/login']);
  }
}
