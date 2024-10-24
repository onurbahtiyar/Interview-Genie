export interface InterviewQuestionDetail {
  questionId: string;
  questionText: string;
  questionType: string;
  options?: string[];
  userAnswer: string;
  correctAnswer: string;
  isCorrect: boolean;
  topic: string;
}
