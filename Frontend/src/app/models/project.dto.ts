import { SkillDto } from "./skill.dto";

export interface ProjectDto {
    id?: string;
    title: string;
    description: string;
    skills: SkillDto[];
  }