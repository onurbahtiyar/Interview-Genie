export interface InterviewSessionDto {
    id: string;
    userId: string;
    companyName: string;
    isActive: boolean;
    startedAt: string;
    endedAt?: string;
    totalQuestions: number;
    correctAnswers: number;
    incorrectAnswers: number;
  }