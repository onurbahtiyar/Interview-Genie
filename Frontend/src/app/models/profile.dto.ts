import { CompanyDto } from "./company.dto";
import { LanguageDto } from "./language.dto";
import { ProjectDto } from "./project.dto";
import { SkillDto } from "./skill.dto";

export interface ProfileDto {
  userId: string;
  skills: SkillDto[];
  languages: LanguageDto[];
  projects: ProjectDto[];
  companies: CompanyDto[];
}