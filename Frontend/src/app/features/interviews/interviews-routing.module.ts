import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateInterviewComponent } from './create-interview/create-interview.component';
import { InterviewComponent } from './interview/interview.component';
import { InterviewResultComponent } from './interview-result/interview-result.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { InterviewDetailsComponent } from './interview-details/interview-details.component';

const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent,
    data: { title: 'Ana Sayfa' }
  },
  {
    path: 'create-interview',
    component: CreateInterviewComponent,
    data: { title: 'Mülakat Oluştur' }
  },
  {
    path: ':id',
    component: InterviewComponent,
    data: { title: 'Mülakat' }
  },
  {
    path: ':id/result',
    component: InterviewResultComponent,
    data: { title: 'Mülakat Sonucu' }
  },
  {
    path: ':id/details',
    component: InterviewDetailsComponent,
    data: { title: 'Mülakat Detayları' }
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InterviewsRoutingModule { }
