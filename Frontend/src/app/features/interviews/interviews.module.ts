import { NgModule, CUSTOM_ELEMENTS_SCHEMA  } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CreateInterviewComponent } from './create-interview/create-interview.component';
import { InterviewComponent } from './interview/interview.component';
import { InterviewResultComponent } from './interview-result/interview-result.component';
import { InterviewsRoutingModule } from './interviews-routing.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InterviewDetailsComponent } from './interview-details/interview-details.component';
import { BaseChartDirective } from 'ng2-charts';
import { MatTreeModule } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MarkdownModule } from 'ngx-markdown';
import { NgxGraphModule } from '@swimlane/ngx-graph';

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
    MatTreeModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    MatInputModule,
    MarkdownModule.forRoot(),
    NgxGraphModule
  ]
})
export class InterviewsModule { }