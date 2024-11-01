import { AreaOfImprovement } from "./areaffimprovement.dto";
import { LearningTreeDto } from "./learning-tree.dto";

export interface ProfileComment {
  AreasOfImprovement: AreaOfImprovement[];
  LearningTree: LearningTreeDto[];
  OverallFeedback: string;
}