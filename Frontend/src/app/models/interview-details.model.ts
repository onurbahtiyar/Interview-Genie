import { InterviewQuestionDetail } from './interview-question-detail.model';

export interface InterviewDetails {
  sessionId: string;
  startedAt: string;
  endedAt?: string;
  questions: InterviewQuestionDetail[];
}
