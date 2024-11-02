import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { InterviewService } from '../../../core/services/interview.service';
import { InterviewQuestionResponse } from '../../../models/interview-question-response.model';
import { SubmitAnswerRequest } from '../../../models/submit-answer-request.model';
import { EndInterviewResponse } from '../../../models/end-interview-response.model';

@Component({
  selector: 'app-interview',
  templateUrl: './interview.component.html',
  styleUrls: ['./interview.component.scss']
})
export class InterviewComponent implements OnInit {
  interviewId: string;
  currentQuestion: InterviewQuestionResponse | null = null;
  answer: string = '';
  selectedOption: string = '';
  isLoading = false;
  error: string | null = null;
  isSubmitting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private interviewService: InterviewService
  ) {
    this.interviewId = this.route.snapshot.paramMap.get('id') || '';
  }

  ngOnInit(): void {
    this.loadNextQuestion();
  }

  loadNextQuestion(): void {
    this.isLoading = true;
    this.error = null;
    this.interviewService.getNextQuestion(this.interviewId).subscribe({
      next: (question) => {
        this.currentQuestion = question;
        this.isLoading = false;
        if (!this.currentQuestion || !this.currentQuestion.questionId) {
          // Görüşme bitti, sonuç sayfasına yönlendir
          this.endInterview();
        }
      },
      error: (err) => {
        this.isLoading = false;
        const errorMessage = err.error?.error || 'Soru getirilemedi.';
        if (errorMessage === 'No more questions available.') {
          // Görüşme bitti, sonuç sayfasına yönlendir
          this.endInterview();
        } else {
          this.error = errorMessage;
        }
      }
    });
  }

  submitAnswer(): void {
    if (!this.currentQuestion) {
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const request: SubmitAnswerRequest = {
      questionId: this.currentQuestion.questionId,
      answer: this.getAnswerBasedOnType()
    };

    this.interviewService.submitAnswer(this.interviewId, request).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.selectedOption = '';
        this.answer = '';
        this.loadNextQuestion();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = err.error?.error || 'Cevap gönderilemedi.';
      }
    });
  }

  getAnswerBasedOnType(): string {
    if (this.currentQuestion?.questionType === 'MultipleChoice') {
      return this.selectedOption;
    } else if (this.currentQuestion?.questionType === 'OpenEnded') {
      return this.answer;
    }
    return '';
  }

  isAnswerValid(): boolean {
    if (!this.currentQuestion) {
      return false;
    }
    if (this.currentQuestion.questionType === 'MultipleChoice') {
      return this.selectedOption.trim() !== '';
    } else if (this.currentQuestion.questionType === 'OpenEnded') {
      return this.answer.trim() !== '';
    }
    return false;
  }

  endInterview(): void {
    this.isLoading = true; // Yükleniyor durumunu aktif et
    this.interviewService.endInterview(this.interviewId).subscribe({
      next: (result: EndInterviewResponse) => {
        this.isLoading = false; // Yükleniyor durumunu pasif et (opsiyonel)
        this.router.navigate(['/interviews', this.interviewId, 'result'], { state: { result } });
      },
      error: (err) => {
        this.isLoading = false; // Yükleniyor durumunu pasif et
        this.error = err.error?.error || 'Görüşme sonlandırılamadı.';
      }
    });
  }
}
