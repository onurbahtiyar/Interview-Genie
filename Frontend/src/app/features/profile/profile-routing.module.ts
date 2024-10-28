import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProfileViewComponent } from './profile-view/profile-view.component';
import { ProfileEditComponent } from './profile-edit/profile-edit.component';

const routes: Routes = [
  {
    path: 'view',
    component: ProfileViewComponent,
    data: { title: 'Profil' }
  },
  {
    path: 'edit',
    component: ProfileEditComponent,
    data: { title: 'Profil Düzenle' }
  },
  { path: '', redirectTo: 'view', pathMatch: 'full' },
  { path: '**', redirectTo: 'view' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProfileRoutingModule { }
