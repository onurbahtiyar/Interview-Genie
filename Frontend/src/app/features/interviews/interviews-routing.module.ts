import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateInterviewComponent } from './create-interview/create-interview.component';
import { InterviewComponent } from './interview/interview.component';
import { InterviewResultComponent } from './interview-result/interview-result.component';
import { AuthGuard } from '../../core/guards/auth.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InterviewDetailsComponent } from './interview-details/interview-details.component';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'create', component: CreateInterviewComponent },
  { path: ':id', component: InterviewComponent },
  { path: ':id/result', component: InterviewResultComponent },
  { path: ':id/details', component: InterviewDetailsComponent },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InterviewsRoutingModule { }