import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login';
import { RegisterComponent } from './auth/register/register';
import { MeetingListComponent } from './meetings/meeting-list/meeting-list';


export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'meetings', component: MeetingListComponent },
    { path: '', redirectTo: '/login', pathMatch: 'full' }
];
