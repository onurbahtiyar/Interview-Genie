import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { InterviewService } from '../../../core/services/interview.service';
import { InterviewDetails } from '../../../models/interview-details.model';

import { Chart, ChartOptions, ChartType, registerables } from 'chart.js';
import { InterviewQuestionDetail } from 'src/app/models/interview-question-detail.model';
import { ProfileComment } from 'src/app/models/profilecomment.dto';
import { NestedTreeControl } from '@angular/cdk/tree';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { LearningTreeDto } from 'src/app/models/learning-tree.dto';

import { TrainingService } from '../../../core/services/training.service';
import { TrainingPlan } from 'src/app/models/training-plan.dto';

Chart.register(...registerables);

@Component({
  selector: 'app-interview-details',
  templateUrl: './interview-details.component.html',
  styleUrls: ['./interview-details.component.scss'],
})
export class InterviewDetailsComponent implements OnInit {

  @ViewChild('chatHistoryContainer', { static: false }) chatHistoryContainer!: ElementRef;

  sessionId: string = '';
  isLoading = false;
  error: string | null = null;

  questions: InterviewQuestionDetail[] = [];
  parsedProfileComment: ProfileComment | null = null;
  profileComment: string = '';

  treeControl = new NestedTreeControl<LearningTreeDto>(node => node.SubTopics);
  dataSource = new MatTreeNestedDataSource<LearningTreeDto>();

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

  // Modal ile ilgili değişkenler
  showModal: boolean = false;
  selectedTopic: string = '';
  trainingPlan: TrainingPlan | null = null;
  chatHistory: { sender: 'user' | 'bot'; message: string }[] = [];
  userMessage: string = '';
  isChatLoading: boolean = false;
  isTrainingLoading: boolean = false;

  showHelpModal: boolean = false;
  helpStepTitle: string = '';
  helpStepDescription: string = '';
  helpUserMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private interviewService: InterviewService,
    private trainingService: TrainingService
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
        this.profileComment = data.profileComment;

        // ProfileComment JSON stringini ayrıştırma
        this.parseProfileComment();
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
          backgroundColor: 'rgba(72, 187, 120, 0.6)',
        },
        {
          data: incorrectData,
          label: 'Yanlış Cevaplar',
          backgroundColor: 'rgba(248, 113, 113, 0.6)',
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
          backgroundColor: 'rgba(96, 165, 250, 0.2)',
          borderColor: 'rgba(96, 165, 250, 1)',
          pointBackgroundColor: 'rgba(96, 165, 250, 1)',
        },
      ],
    };
  }

  isAnswerCorrect(question: InterviewQuestionDetail): boolean {
    return question.isCorrect;
  }

  parseProfileComment(): void {
    try {
      if (this.profileComment) {
        this.parsedProfileComment = typeof this.profileComment === 'string'
          ? JSON.parse(this.profileComment) as ProfileComment
          : this.profileComment as ProfileComment;
        console.log('Parsed Profile Comment:', this.parsedProfileComment);
        this.dataSource.data = this.parsedProfileComment?.LearningTree || [];
      } else {
        this.parsedProfileComment = null;
        this.dataSource.data = [];
      }
    } catch (error) {
      console.error('Profile comment parsing error:', error);
      this.parsedProfileComment = null;
      this.dataSource.data = [];
    }
  }

  hasChild = (_: number, node: LearningTreeDto) => !!node.SubTopics && node.SubTopics.length > 0;

  // Modal açma fonksiyonu
  openTrainingModal(topic: string): void {
    this.selectedTopic = topic;
    this.showModal = true;
    this.loadTrainingPlan(topic);
  }

  closeModal(): void {
    this.showModal = false;
    this.trainingPlan = null;
    this.chatHistory = [];
    this.userMessage = '';
  }

  loadTrainingPlan(topic: string): void {
    this.isTrainingLoading = true;
    this.trainingService.getTrainingPlan(topic, 'Türkçe').subscribe({
      next: (plan: TrainingPlan) => {
        this.trainingPlan = plan;
        this.isTrainingLoading = false;
      },
      error: (err) => {
        console.error('Eğitim planı yüklenemedi:', err);
        this.isTrainingLoading = false;
      },
    });
  }

  requestHelp(step: any): void {
    this.helpStepTitle = step.title;
    this.helpStepDescription = step.description;
    this.helpUserMessage = '';
    this.showHelpModal = true;
  }

  // Yardım modalını kapatmak için
  closeHelpModal(): void {
    this.showHelpModal = false;
  }

  // Kullanıcının yardım isteğini gönderme metodu
  submitHelpRequest(): void {
    if (this.helpUserMessage.trim()) {
      const combinedMessage = `Merhaba, ${this.helpStepTitle}. ${this.helpStepDescription}. ${this.helpUserMessage}`;
      this.userMessage = combinedMessage;
      this.showHelpModal = false;
      this.sendMessage();
    }
  }

  sendMessage(): void {
    if (!this.userMessage.trim()) return;

    // Kullanıcı mesajını ekle
    this.chatHistory.push({ sender: 'user', message: this.userMessage });
    const messageToSend = this.userMessage;
    this.userMessage = '';
    this.isChatLoading = true;

    // Kaydırma
    this.scrollToBottom();

    // Bot cevabını al
    this.trainingService
      .getChatbotResponse(this.selectedTopic, messageToSend, 'Türkçe')
      .subscribe({
        next: (data: any) => {
          this.isChatLoading = false;
          // Yazma efekti
          this.simulateTypingEffect(data.response);
        },
        error: (err) => {
          this.isChatLoading = false;

          this.chatHistory.push({
            sender: 'bot',
            message: 'Bir hata oluştu, lütfen tekrar deneyin.',
          });
          this.scrollToBottom();
        },
      });
  }

  scrollToBottom(): void {
    try {
      setTimeout(() => {
        if (this.chatHistoryContainer) {
          this.chatHistoryContainer.nativeElement.scrollTop = this.chatHistoryContainer.nativeElement.scrollHeight;
        }
      }, 100);
    } catch (err) {
      console.error('Scroll to bottom failed:', err);
    }
  }

  simulateTypingEffect(fullText: string): void {
    let index = 0;

    // Boş bir bot mesajı ekle
    this.chatHistory.push({ sender: 'bot', message: '' });
    const lastIndex = this.chatHistory.length - 1;

    const interval = setInterval(() => {
      if (index <= fullText.length) {
        const currentText = fullText.substring(0, index);
        // Son bot mesajını güncelle
        this.chatHistory[lastIndex].message = currentText;
        index++;
        this.scrollToBottom();
      } else {
        clearInterval(interval);
      }
    }, 1);
  }
  toggleNode(node: LearningTreeDto): void {
    node.expanded = !node.expanded;
  }

  getDifficultyClass(level: string): string {
    switch(level.toLowerCase()) {
      case 'kolay':
        return 'easy';
      case 'orta':
        return 'medium';
      case 'zor':
        return 'hard';
      default:
        return '';
    }
  }
  
  // Öncelik seviyesine göre sınıf döndüren fonksiyon
  getImportanceClass(importance: string): string {
    switch(importance.toLowerCase()) {
      case 'düşük':
        return 'low';
      case 'orta':
        return 'medium';
      case 'yüksek':
        return 'high';
      default:
        return '';
    }
  }

  // Zorluk seviyesine göre emoji döndüren fonksiyon
  getDifficultyEmoji(level: string): string {
    switch(level.toLowerCase()) {
      case 'kolay':
        return '😃';
      case 'orta':
        return '😐';
      case 'zor':
        return '😓';
      default:
        return '';
    }
  }

  // Öncelik seviyesine göre emoji döndüren fonksiyon
  getImportanceEmoji(importance: string): string {
    switch(importance.toLowerCase()) {
      case 'düşük':
        return '✅';
      case 'orta':
        return '⚡️';
      case 'yüksek':
        return '📌';
      default:
        return '';
    }
  }
  
}
