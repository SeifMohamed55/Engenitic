import { CourseComponent } from './components/course/course.component';
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
import { AdminComponent } from './pages/admin/admin.component';
import { StudentEnrolledCoursesComponent } from './components/student-enrolled-courses/student-enrolled-courses.component';
import { InstructorMadeCoursesComponent } from './components/instructor-made-courses/instructor-made-courses.component';
import { InstructorAddCourseComponent } from './pages/instructor-add-course/instructor-add-course.component';
import { InstructorEditCourseComponent } from './pages/instructor-edit-course/instructor-edit-course.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { MainCourseComponent } from './pages/main-course/main-course.component';
import { rolesGuard } from './guards/roles.guard';
import { authRedirectGuardGuard } from './guards/auth-redirect-guard.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  {
    path: 'offered-courses',
    component: CoursesComponent,
    children: [
      { path: '', redirectTo: '1', pathMatch: 'full' },
      { path: ':collectionNumber', component: CourseComponent },
    ],
  },
  { path: 'course-details/:courseId', component: CourseDetailsComponent },
  {
    path: 'main-course/:studentId/:enrollmentId',
    component: MainCourseComponent,
    data: {
      roles: ['student'],
    },
    canActivate: [rolesGuard],
  },
  { path: 'grammar', component: GrammarHelpComponent },
  { path: 'Q&A', component: VqaComponent },
  { path: 'listening', component: ListeningComponent },
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [authRedirectGuardGuard],
  },
  {
    path: 'register',
    component: RegisterComponent,
    canActivate: [authRedirectGuardGuard],
  },
  {
    path: 'profile',
    component: ProfileComponent,
    children: [
      {
        path: 'student',
        data: { roles: ['student'] },
        canActivate: [rolesGuard],
        children: [
          { path: '', redirectTo: '1/1', pathMatch: 'full' },
          {
            path: ':userId/:collectionId',
            component: StudentEnrolledCoursesComponent,
          },
          { path: '**', component: NotFoundComponent },
        ],
      },
      {
        path: 'instructor',
        data: { roles: ['instructor'] },
        canActivate: [rolesGuard],
        children: [
          { path: '', redirectTo: '1/1', pathMatch: 'full' },
          {
            path: ':userId/:collectionId',
            component: InstructorMadeCoursesComponent,
          },
          { path: '**', component: NotFoundComponent },
        ],
      },
      {
        path: 'admin',
        data: { roles: ['admin'] },
        canActivate: [rolesGuard],
        component: AdminComponent,
      },
      { path: '**', component: NotFoundComponent },
    ],
  },
  {
    path: 'courses',
    data: { roles: ['instructor'] },
    canActivate: [rolesGuard],
    children: [
      { path: 'adding/:instructorId', component: InstructorAddCourseComponent },
      {
        path: 'editing/:instructorId/:courseId',
        component: InstructorEditCourseComponent,
      },
      { path: '**', component: NotFoundComponent },
    ],
  },
  { path: 'unauthorized', component: UnothorizedComponent },
  { path: '**', component: NotFoundComponent },
];
