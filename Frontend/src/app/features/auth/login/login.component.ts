import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { LoginDto } from '../../../models/login.dto';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

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
  showPassword: boolean = false;
  isLoading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  onSubmit() {
    if (this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData)
      .pipe(
        catchError(err => {
          this.errorMessage = err.error.message || 'Bir hata oluştu.';
          this.snackBar.open(this.errorMessage, 'Kapat', {
            duration: 5000,
            panelClass: ['snackbar-error']
          });
          return of(null);
        }),
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe(response => {
        if (response?.success) {
          localStorage.setItem('token', response.data.token);
          localStorage.setItem('user', JSON.stringify(response.data.user));
          this.snackBar.open('Giriş başarılı!', 'Kapat', {
            duration: 3000,
            panelClass: ['snackbar-success']
          });
          this.router.navigate(['/interviews']);
        } else if (response) { // response null değilse ve success değilse
          this.errorMessage = response.message || 'Bir hata oluştu.';
          this.snackBar.open(this.errorMessage, 'Kapat', {
            duration: 5000,
            panelClass: ['snackbar-error']
          });
        }
      });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

}
