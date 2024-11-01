import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { RegisterDto } from '../../../models/register.dto';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {

  registerData: RegisterDto = {
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  };

  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  onSubmit() {
    if (this.registerData.password !== this.registerData.confirmPassword) {
      this.errorMessage = 'Şifreler uyuşmuyor.';
      return;
    }

    this.authService.register(this.registerData)
      .pipe(
        catchError(err => {
          this.errorMessage = err.error.message || 'Bir hata oluştu.';
          return of(null);
        })
      )
      .subscribe(response => {
        console.log(response)
        if (response?.success) {
          this.router.navigate(['/auth/login']);
        }else {
          this.errorMessage = response?.message || 'Bir hata oluştu.';
        }
      },
      err => {
        this.errorMessage = err.error.message || 'Bir hata oluştu.';
      }
    );
  }

}