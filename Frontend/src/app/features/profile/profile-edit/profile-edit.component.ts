// src/app/features/profile/profile-edit/profile-edit.component.ts
import { Component, OnInit } from '@angular/core';
import { ProfileService } from '../../../core/services/profile.service';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { CompanyDto } from 'src/app/models/company.dto';
import { LanguageDto } from 'src/app/models/language.dto';
import { ProfileDto } from 'src/app/models/profile.dto';
import { ProjectDto } from 'src/app/models/project.dto';
import { SkillDto } from 'src/app/models/skill.dto';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-profile-edit',
  templateUrl: './profile-edit.component.html',
  styleUrls: ['./profile-edit.component.scss'],
})
export class ProfileEditComponent implements OnInit {
  profile: ProfileDto | null = null;
  isLoading = false;
  error: string | null = null;

  skillsForm: FormGroup;
  languagesForm: FormGroup;
  projectsForm: FormGroup;
  companiesForm: FormGroup;

  allSkills: SkillDto[] = [];
  allLanguages: LanguageDto[] = [];

  constructor(
    private profileService: ProfileService,
    private router: Router,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.skillsForm = this.fb.group({
      skills: [[], Validators.required],
    });

    this.languagesForm = this.fb.group({
      languages: [[], Validators.required],
    });

    this.projectsForm = this.fb.group({
      projects: this.fb.array([]),
    });

    this.companiesForm = this.fb.group({
      companies: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    this.loadAllSkills();
    this.loadAllLanguages();
    this.loadProfile();
  }

  loadAllSkills(): void {
    this.profileService.getAllSkills().subscribe({
      next: (result) => {
        if (result.success) {
          this.allSkills = result.data;
        } else {
          this.showError(result.message || 'Yetenekler yüklenirken bir hata oluştu.');
        }
      },
      error: (err) => {
        this.showError(err.error?.message || 'Yetenekler yüklenirken bir hata oluştu.');
      },
    });
  }

  loadAllLanguages(): void {
    this.profileService.getAllLanguages().subscribe({
      next: (result) => {
        if (result.success) {
          this.allLanguages = result.data;
        } else {
          this.showError(result.message || 'Diller yüklenirken bir hata oluştu.');
        }
      },
      error: (err) => {
        this.showError(err.error?.message || 'Diller yüklenirken bir hata oluştu.');
      },
    });
  }

  loadProfile(): void {
    this.isLoading = true;
    this.profileService.getProfile().subscribe({
      next: (result) => {
        if (result.success) {
          this.profile = result.data;
          this.initializeForms();
        } else {
          this.showError(result.message || 'Profil yüklenirken bir hata oluştu.');
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.showError(err.error?.message || 'Profil yüklenirken bir hata oluştu.');
        this.isLoading = false;
      },
    });
  }

  initializeForms(): void {
    if (!this.profile) return;

    // Yetenekler
    const selectedSkills = this.profile.skills.map((s: SkillDto) => s.name);
    this.skillsForm.patchValue({ skills: selectedSkills });

    // Diller
    const selectedLanguages = this.profile.languages.map((l: LanguageDto) => l.name);
    this.languagesForm.patchValue({ languages: selectedLanguages });

    // Projeler
    const projectsArray = this.projectsForm.get('projects') as FormArray;
    this.profile.projects.forEach((project: ProjectDto) => {
      projectsArray.push(
        this.fb.group({
          id: [project.id],
          title: [project.title, Validators.required],
          description: [project.description],
          skills: [project.skills ? project.skills.map((skill: SkillDto) => skill.name) : []]
        })
      );
    });

    // Şirketler
    const companiesArray = this.companiesForm.get('companies') as FormArray;
    this.profile.companies.forEach((company: CompanyDto) => {
      companiesArray.push(
        this.fb.group({
          id: [company.id],
          companyName: [company.companyName, Validators.required],
          position: [company.position],
          description: [company.description]
        })
      );
    });
  }

  // Yetenekler
  saveSkills(): void {
    if (this.skillsForm.valid && this.profile) {
      const skills = this.skillsForm.value.skills;
      this.profileService.updateSkills(skills).subscribe({
        next: (result) => {
          if (result.success) {
            this.snackBar.open('Yetenekler başarıyla güncellendi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-success']
            });
          } else {
            this.snackBar.open(result.message || 'Yetenekler güncellenemedi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-error']
            });
          }
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Bir hata oluştu.', 'Kapat', {
            duration: 3000,
            panelClass: ['snackbar-error']
          });
        },
      });
    }
  }

  // Diller
  saveLanguages(): void {
    if (this.languagesForm.valid && this.profile) {
      const languages = this.languagesForm.value.languages;
      this.profileService.updateLanguages(languages).subscribe({
        next: (result) => {
          if (result.success) {
            this.snackBar.open('Diller başarıyla güncellendi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-success']
            });
          } else {
            this.snackBar.open(result.message || 'Diller güncellenemedi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-error']
            });
          }
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Bir hata oluştu.', 'Kapat', {
            duration: 3000,
            panelClass: ['snackbar-error']
          });
        },
      });
    }
  }

  // Projeler
  get projects(): FormArray {
    return this.projectsForm.get('projects') as FormArray;
  }

  addProject(): void {
    this.projects.push(
      this.fb.group({
        id: [null],
        title: ['', Validators.required],
        description: [''],
        skills: [[]]
      })
    );
  }

  removeProject(index: number): void {
    this.projects.removeAt(index);
  }

  saveProjects(): void {
    if (this.projectsForm.valid && this.profile) {
      const projects: ProjectDto[] = this.projects.value.map((project: any) => ({
        ...project,
        skills: project.skills.map((skillName: string) => {
          const skill = this.allSkills.find(s => s.name === skillName);
          return skill ? { id: skill.id, name: skill.name } : null;
        }).filter((skill: any) => skill !== null) // null değerleri filtrele
      }));

      this.profileService.addOrUpdateProjects(projects).subscribe({
        next: (result) => {
          if (result.success) {
            this.snackBar.open('Projeler başarıyla güncellendi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-success']
            });
          } else {
            this.snackBar.open(result.message || 'Projeler güncellenemedi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-error']
            });
          }
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Bir hata oluştu.', 'Kapat', {
            duration: 3000,
            panelClass: ['snackbar-error']
          });
        },
      });
    }
  }

  // Şirketler
  get companies(): FormArray {
    return this.companiesForm.get('companies') as FormArray;
  }

  addCompany(): void {
    this.companies.push(
      this.fb.group({
        id: [null],
        companyName: ['', Validators.required],
        position: [''],
        description: ['']
      })
    );
  }

  removeCompany(index: number): void {
    this.companies.removeAt(index);
  }

  saveCompanies(): void {
    if (this.companiesForm.valid && this.profile) {
      const companies: CompanyDto[] = this.companiesForm.value.companies;
      this.profileService.addOrUpdateCompanies(companies).subscribe({
        next: (result) => {
          if (result.success) {
            this.snackBar.open('Şirketler başarıyla güncellendi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-success']
            });
          } else {
            this.snackBar.open(result.message || 'Şirketler güncellenemedi.', 'Kapat', {
              duration: 3000,
              panelClass: ['snackbar-error']
            });
          }
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Bir hata oluştu.', 'Kapat', {
            duration: 3000,
            panelClass: ['snackbar-error']
          });
        },
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/profile']);
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: 3000,
      panelClass: ['snackbar-error']
    });
  }
}
