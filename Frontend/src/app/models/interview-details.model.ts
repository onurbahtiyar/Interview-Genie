import { InterviewQuestionDetail } from './interview-question-detail.model';

export interface InterviewDetails {
  sessionId: string;
  profileComment: string;
  questions: InterviewQuestionDetail[];
}
