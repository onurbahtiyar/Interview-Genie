import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { IDataResult } from '../../models/IDataResult';
import { IResult } from '../../models/IResult';
import { environment } from '../../../environments/environment';
import { CompanyDto } from 'src/app/models/company.dto';
import { ProjectDto } from 'src/app/models/project.dto';
import { ProfileDto } from 'src/app/models/profile.dto';
import { SkillDto } from 'src/app/models/skill.dto';
import { LanguageDto } from 'src/app/models/language.dto';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<IDataResult<ProfileDto>> {
    return this.http.get<IDataResult<ProfileDto>>(`${this.apiUrl}/profile`);
  }

  updateSkills(skills: string[]): Observable<IResult> {
    return this.http.post<IResult>(`${this.apiUrl}/profile/skills`, skills);
  }

  updateLanguages(languages: string[]): Observable<IResult> {
    return this.http.post<IResult>(`${this.apiUrl}/profile/languages`, languages);
  }

  addOrUpdateProjects(projects: ProjectDto[]): Observable<IResult> {
    return this.http.post<IResult>(`${this.apiUrl}/profile/projects`, projects);
  }

  addOrUpdateCompanies(companies: CompanyDto[]): Observable<IResult> {
    return this.http.post<IResult>(`${this.apiUrl}/profile/companies`, companies);
  }

    // Mevcut yetenekleri alır
    getAllSkills(): Observable<IDataResult<SkillDto[]>> {
      return this.http.get<IDataResult<SkillDto[]>>(`${this.apiUrl}/profile/skills/all`);
    }
  
    // Mevcut dilleri alır
    getAllLanguages(): Observable<IDataResult<LanguageDto[]>> {
      return this.http.get<IDataResult<LanguageDto[]>>(`${this.apiUrl}/profile/languages/all`);
    }
  
    // Mevcut teknolojileri alır
    getAllTechnologies(): Observable<IDataResult<string[]>> {
      return this.http.get<IDataResult<string[]>>(`${this.apiUrl}/profile/technologies`);
    }
}