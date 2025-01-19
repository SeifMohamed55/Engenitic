import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CoursesComponent } from './pages/courses/courses.component';
import { GrammarHelpComponent } from './pages/grammar-help/grammar-help.component';
import { VqaComponent } from './pages/vqa/vqa.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ListeningComponent } from './pages/listening/listening.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';

export const routes: Routes = [
    {path : "" , redirectTo : "home" , pathMatch : "full"},
    {path : "home", component : HomeComponent},
    {path : "courses", component : CoursesComponent},
    {path : "grammar", component : GrammarHelpComponent},
    {path : "Q&A", component : VqaComponent},
    {path : "listening", component : ListeningComponent},
    {path : "login", component : LoginComponent},
    {path : "register", component : RegisterComponent},
    {path : "**", component: NotFoundComponent}
];
