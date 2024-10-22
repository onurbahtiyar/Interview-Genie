import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { LoginDto } from '../../../models/login.dto';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  loginData: LoginDto = {
    username: '',
    password: ''
  };

  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  onSubmit() {
    this.authService.login(this.loginData)
      .pipe(
        catchError(err => {
          this.errorMessage = err.error.message || 'Bir hata oluştu.';
          return of(null);
        })
      )
      .subscribe(response => {
        if (response?.success) {
          localStorage.setItem('token', response.data.token);
          this.router.navigate(['/dashboard']);
        } else {
          this.errorMessage = response?.message || 'Bir hata oluştu.';
        }
      },
      err => {
        this.errorMessage = err.error.message || 'Bir hata oluştu.';
      });
  }

}
