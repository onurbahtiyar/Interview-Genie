export interface TrainingPlan {
    topic: string;
    steps: TrainingStep[];
}

export interface TrainingStep {
    stepNumber: number;
    title: string;
    description: string;
}