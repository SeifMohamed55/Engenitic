import { CourseComponent } from './pages/course/course.component';
import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CoursesComponent } from './pages/courses/courses.component';
import { GrammarHelpComponent } from './pages/grammar-help/grammar-help.component';
import { VqaComponent } from './pages/vqa/vqa.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ListeningComponent } from './pages/listening/listening.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { CourseDetailsComponent } from './pages/course-details/course-details.component';
import { UnothorizedComponent } from './pages/unothorized/unothorized.component';
import { StudentComponent } from './pages/student/student.component';
import { InstructorComponent } from './pages/instructor/instructor.component';
import { AdminComponent } from './pages/admin/admin.component';

export const routes: Routes = [
    {path : "" , redirectTo : "home" , pathMatch : "full"},
    {path : "home", component : HomeComponent},
    {path : "courses", component : CoursesComponent, children : [
        {path : '', redirectTo : '1', pathMatch : 'full'},
        {path : ':collection', component : CourseComponent },
    ]},
    {path : 'course/:id' , component : CourseDetailsComponent},
    {path : "grammar", component : GrammarHelpComponent},
    {path : "Q&A", component : VqaComponent},
    {path : "listening", component : ListeningComponent},
    {path : "login", component : LoginComponent},
    {path : "register", component : RegisterComponent},
    {path : "profile" , children : [
        {path : 'student', component : StudentComponent},
        {path : 'instructor', component : InstructorComponent},
        {path : 'admin', component : AdminComponent},
        {path : '**' , component : NotFoundComponent}
    ]},
    {path : "unauthorized", component : UnothorizedComponent},
    {path : "**", component: NotFoundComponent}
];
