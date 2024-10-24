import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { InterviewService } from '../../../core/services/interview.service';
import { CreateInterviewRequest } from '../../../models/create-interview-request.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-interview',
  templateUrl: './create-interview.component.html',
  styleUrls: ['./create-interview.component.scss']
})
export class CreateInterviewComponent implements OnInit {
  interviewForm: FormGroup;
  isSubmitting = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private interviewService: InterviewService,
    private router: Router
  ) {
    this.interviewForm = this.fb.group({
      companyName: ['', Validators.required],
      industry: ['', Validators.required],
      location: ['', Validators.required],
      skills: this.fb.array([this.fb.control('', Validators.required)]),
      description: ['', Validators.required],
      language: ['', Validators.required]
    });
  }

  ngOnInit(): void { }

  get skills(): FormArray {
    return this.interviewForm.get('skills') as FormArray;
  }

  addSkill(): void {
    this.skills.push(this.fb.control('', Validators.required));
  }

  removeSkill(index: number): void {
    if (this.skills.length > 1) {
      this.skills.removeAt(index);
    }
  }

  onSubmit(): void {
    if (this.interviewForm.invalid) {
      this.interviewForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const request: CreateInterviewRequest = {
      ...this.interviewForm.value,
      skills: this.interviewForm.value.skills.filter((skill: string) => skill.trim() !== '')
    };

    this.interviewService.createInterview(request).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.router.navigate(['/interviews', response.id]);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = err.error?.error || 'Görüşme oluşturulurken bir hata oluştu.';
      }
    });
  }
}
