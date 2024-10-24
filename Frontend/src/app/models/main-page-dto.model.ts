import { InterviewSessionDto } from "./interview-session-dto.model";
import { InterviewSummaryDto } from "./interview-summary-dto.model";

export interface MainPageDto {
    pastSessions: InterviewSessionDto[];
    summary: InterviewSummaryDto;
  }