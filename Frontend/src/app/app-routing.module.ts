import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { LayoutComponent } from './shared/components/layout/layout.component';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        redirectTo: '/interviews/dashboard',
        pathMatch: 'full'
      },
      {
        path: 'auth',
        loadChildren: () =>
          import('./features/auth/auth.module').then(m => m.AuthModule)
      },
      {
        path: 'profile',
        loadChildren: () =>
          import('./features/profile/profile.module').then(m => m.ProfileModule),
        canActivate: [AuthGuard]
      },
      {
        path: 'interviews',
        loadChildren: () =>
          import('./features/interviews/interviews.module').then(
            m => m.InterviewsModule
          ),
        canActivate: [AuthGuard]
      },
      { path: '**', redirectTo: '/interviews/dashboard' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
