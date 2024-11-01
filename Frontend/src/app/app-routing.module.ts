import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { LayoutComponent } from './shared/components/layout/layout.component';

const routes: Routes = [
  // Auth Rotaları - LayoutComponent dışında
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'interviews/dashboard', 
        pathMatch: 'full'
      },
      {
        path: 'profile',
        loadChildren: () =>
          import('./features/profile/profile.module').then(m => m.ProfileModule)
      },
      {
        path: 'interviews',
        loadChildren: () =>
          import('./features/interviews/interviews.module').then(
            m => m.InterviewsModule
          )
      },
      { path: '**', redirectTo: 'interviews/dashboard' } 
    ]
  },
  { path: '**', redirectTo: 'interviews/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
