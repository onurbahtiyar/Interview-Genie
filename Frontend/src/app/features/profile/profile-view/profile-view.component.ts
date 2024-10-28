import { Component, OnInit } from '@angular/core';
import { ProfileService } from '../../../core/services/profile.service';
import { ProfileDto } from '../../../models/profile.dto';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile-view',
  templateUrl: './profile-view.component.html',
  styleUrls: ['./profile-view.component.scss'],
})
export class ProfileViewComponent implements OnInit {
  profile: ProfileDto | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(private profileService: ProfileService, private router: Router) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.profileService.getProfile().subscribe({
      next: (result) => {
        if (result.success) {
          this.profile = result.data;
        } else {
          this.error = result.message;
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Profil yüklenirken bir hata oluştu.';
        this.isLoading = false;
      },
    });
  }

  navigateToEdit(): void {
    this.router.navigate(['/profile/edit']);
  }
}