import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { EndInterviewResponse } from '../../../models/end-interview-response.model';

@Component({
  selector: 'app-interview-result',
  templateUrl: './interview-result.component.html',
  styleUrls: ['./interview-result.component.scss']
})
export class InterviewResultComponent implements OnInit {
  result: EndInterviewResponse | null = null;

  constructor(public router: Router) {
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras.state as { result: EndInterviewResponse };
    if (state && state.result) {
      this.result = state.result;
    } else {
      // Eğer sonuç yoksa, kullanıcıyı ana sayfaya yönlendir
      this.router.navigate(['/dashboard']);
    }
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
  
  ngOnInit(): void { }
}