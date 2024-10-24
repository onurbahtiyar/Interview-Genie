import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CreateInterviewComponent } from './create-interview/create-interview.component';
import { InterviewComponent } from './interview/interview.component';
import { InterviewResultComponent } from './interview-result/interview-result.component';
import { InterviewsRoutingModule } from './interviews-routing.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InterviewDetailsComponent } from './interview-details/interview-details.component';
import { BaseChartDirective } from 'ng2-charts';

@NgModule({
  declarations: [
    CreateInterviewComponent,
    InterviewComponent,
    InterviewResultComponent,
    DashboardComponent,
    InterviewDetailsComponent
  ],
  imports: [
    CommonModule,
    InterviewsRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    BaseChartDirective,
  ]
})
export class InterviewsModule { }