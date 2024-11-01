import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { TrainingPlan } from 'src/app/models/training-plan.dto';

@Injectable({
  providedIn: 'root',
})
export class TrainingService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTrainingPlan(topic: string, language: string): Observable<TrainingPlan> {
    return this.http.post<TrainingPlan>(`${this.apiUrl}/Trainings/GenerateTrainingPlan`, {
      topic,
      language,
    });
  }

  getChatbotResponse(topic: string, userQuestion: string, language: string): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/Trainings/Chatbot`, {
      topic,
      userQuestion,
      language,
    });
  }
}
