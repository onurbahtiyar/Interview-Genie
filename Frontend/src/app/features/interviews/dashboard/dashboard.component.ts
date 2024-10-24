import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { InterviewService } from '../../../core/services/interview.service';
import { InterviewSessionDto } from 'src/app/models/interview-session-dto.model';
import { MainPageDto } from 'src/app/models/main-page-dto.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  isLoading = false;
  error: string | null = null;
  pastSessions: InterviewSessionDto[] = [];
  summary: any = {};

  constructor(private interviewService: InterviewService, private router: Router) { }

  ngOnInit(): void {
    this.loadMainPageData();
  }

  loadMainPageData(): void {
    this.isLoading = true;
    this.error = null;
    this.interviewService.getMainPageData().subscribe({
      next: (data: MainPageDto) => {
        this.pastSessions = data.pastSessions;
        this.summary = data.summary;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = err.error?.error || 'Main page data could not be loaded.';
        this.isLoading = false;
      }
    });
  }

  navigateToCreateInterview(): void {
    this.router.navigate(['/interviews/create']);
  }

  viewDetails(sessionId: string): void {
    this.router.navigate(['/interviews', sessionId, 'details']);
  }
}
