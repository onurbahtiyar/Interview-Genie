import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { InterviewService } from '../../../core/services/interview.service';
import { InterviewDetails } from '../../../models/interview-details.model';

import { Chart, ChartOptions, ChartType, registerables } from 'chart.js';
import { InterviewQuestionDetail } from 'src/app/models/interview-question-detail.model';

Chart.register(...registerables);

@Component({
  selector: 'app-interview-details',
  templateUrl: './interview-details.component.html',
  styleUrls: ['./interview-details.component.scss'],
})
export class InterviewDetailsComponent implements OnInit {
  sessionId: string = '';
  isLoading = false;
  error: string | null = null;

  questions: InterviewQuestionDetail[] = [];

  // Genel istatistikler
  totalQuestions: number = 0;
  totalCorrect: number = 0;
  totalIncorrect: number = 0;
  successRate: number = 0;

  // Bar Grafiği (Konulara Göre Performans)
  barChartData: any;
  barChartLabels: string[] = [];
  barChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { stacked: true },
      y: { stacked: true, beginAtZero: true },
    },
  };
  barChartType: ChartType = 'bar';
  barChartLegend = true;

  // Radar Grafiği (Konulara Göre Yetkinlik)
  radarChartData: any;
  radarChartLabels: string[] = [];
  radarChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      r: {
        beginAtZero: true,
        max: 100,
      },
    },
  };
  radarChartType: ChartType = 'radar';

  constructor(
    private route: ActivatedRoute,
    private interviewService: InterviewService
  ) {}

  ngOnInit(): void {
    this.sessionId = this.route.snapshot.paramMap.get('id') || '';
    this.loadInterviewDetails();
  }

  loadInterviewDetails(): void {
    this.isLoading = true;
    this.error = null;

    this.interviewService.getInterviewDetails(this.sessionId).subscribe({
      next: (data: InterviewDetails) => {
        this.questions = data.questions;
        this.calculateStatistics();
        this.processBarChartData();
        this.processRadarChartData();
        this.isLoading = false;
      },
      error: (err) => {
        this.error = err.error?.error || 'Detaylar yüklenemedi.';
        this.isLoading = false;
      },
    });
  }

  calculateStatistics(): void {
    this.totalQuestions = this.questions.length;
    this.totalCorrect = this.questions.filter((q) => q.isCorrect).length;
    this.totalIncorrect = this.totalQuestions - this.totalCorrect;
    this.successRate = (this.totalCorrect / this.totalQuestions) * 100;
  }

  processBarChartData(): void {
    const topicStats: { [topic: string]: { correct: number; incorrect: number } } = {};

    this.questions.forEach((question) => {
      const isCorrect = question.isCorrect;

      if (!topicStats[question.topic]) {
        topicStats[question.topic] = { correct: 0, incorrect: 0 };
      }

      if (isCorrect) {
        topicStats[question.topic].correct += 1;
      } else {
        topicStats[question.topic].incorrect += 1;
      }
    });

    this.barChartLabels = Object.keys(topicStats);
    const correctData = this.barChartLabels.map((topic) => topicStats[topic].correct);
    const incorrectData = this.barChartLabels.map((topic) => topicStats[topic].incorrect);

    this.barChartData = {
      labels: this.barChartLabels,
      datasets: [
        {
          data: correctData,
          label: 'Doğru Cevaplar',
          backgroundColor: 'green',
        },
        {
          data: incorrectData,
          label: 'Yanlış Cevaplar',
          backgroundColor: 'red',
        },
      ],
    };
  }

  processRadarChartData(): void {
    const topicProficiency: { [topic: string]: number } = {};

    this.barChartLabels.forEach((topic) => {
      const correct = this.questions.filter(
        (q) => q.topic === topic && q.isCorrect
      ).length;
      const total = this.questions.filter((q) => q.topic === topic).length;
      const proficiency = (correct / total) * 100;
      topicProficiency[topic] = proficiency;
    });

    this.radarChartLabels = this.barChartLabels;
    const proficiencyData = this.radarChartLabels.map((topic) => topicProficiency[topic]);

    this.radarChartData = {
      labels: this.radarChartLabels,
      datasets: [
        {
          data: proficiencyData,
          label: 'Yetkinlik (%)',
          backgroundColor: 'rgba(54, 162, 235, 0.2)',
          borderColor: 'rgba(54, 162, 235, 1)',
          pointBackgroundColor: 'rgba(54, 162, 235, 1)',
        },
      ],
    };
  }

  isAnswerCorrect(question: InterviewQuestionDetail): boolean {
    return question.isCorrect;
  }
}
