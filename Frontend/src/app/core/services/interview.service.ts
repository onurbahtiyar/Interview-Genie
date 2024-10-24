import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateInterviewRequest } from '../../models/create-interview-request.model';
import { SubmitAnswerRequest } from '../../models/submit-answer-request.model';
import { InterviewQuestionResponse } from '../../models/interview-question-response.model';
import { EndInterviewResponse } from '../../models/end-interview-response.model';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MainPageDto } from 'src/app/models/main-page-dto.model';
import { InterviewDetails } from 'src/app/models/interview-details.model';

@Injectable({
  providedIn: 'root'
})
export class InterviewService {
  private apiUrl = `${environment.apiUrl}/interviews`;

  constructor(private http: HttpClient) { }

  createInterview(request: CreateInterviewRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}`, request);
  }

  getNextQuestion(interviewId: string): Observable<InterviewQuestionResponse> {
    return this.http.get<InterviewQuestionResponse>(`${this.apiUrl}/${interviewId}/NextQuestion`);
  }

  submitAnswer(interviewId: string, request: SubmitAnswerRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${interviewId}/SubmitAnswer`, request);
  }

  endInterview(interviewId: string): Observable<EndInterviewResponse> {
    return this.http.post<EndInterviewResponse>(`${this.apiUrl}/${interviewId}/End`, {});
  }

  getMainPageData(): Observable<MainPageDto> {
    return this.http.get<MainPageDto>(`${this.apiUrl}/main`);
  }

  getInterviewDetails(interviewId: string): Observable<InterviewDetails> {
    return this.http.get<InterviewDetails>(`${this.apiUrl}/${interviewId}/details`);
  }

}
