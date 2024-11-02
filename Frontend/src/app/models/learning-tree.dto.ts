export interface LearningTreeDto{
    Topic: string;
    DifficultyLevel: string;
    Importance: string;
    SubTopics?: LearningTreeDto[];
    expanded?: boolean;
}