import { Routes } from '@angular/router';
import { rolesGuard } from './guards/roles.guard';
import { authRedirectGuardGuard } from './guards/auth-redirect-guard.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  {
    path: 'home',
    loadComponent: () =>
      import('./pages/home/home.component').then((m) => m.HomeComponent),
  },

  {
    path: 'policy',
    loadComponent: () =>
      import('./pages/policy/policy.component').then((m) => m.PolicyComponent),
  },

  {
    path: 'redirection-page',
    loadComponent: () =>
      import('./pages/redirection-page/redirection-page.component').then(
        (m) => m.RedirectionPageComponent
      ),
    data: { roles: ['student', 'instructor', 'admin'] },
    canActivate: [rolesGuard],
  },

  {
    path: 'offered-courses',
    loadComponent: () =>
      import('./pages/courses/courses.component').then(
        (m) => m.CoursesComponent
      ),
    children: [
      { path: '', redirectTo: '1', pathMatch: 'full' },
      {
        path: ':collectionNumber',
        loadComponent: () =>
          import('./components/course/course.component').then(
            (m) => m.CourseComponent
          ),
      },
    ],
  },

  {
    path: 'course-details/:courseId',
    loadComponent: () =>
      import('./pages/course-details/course-details.component').then(
        (m) => m.CourseDetailsComponent
      ),
  },

  {
    path: 'main-course/:studentId/:enrollmentId/:courseId',
    loadComponent: () =>
      import('./pages/main-course/main-course.component').then(
        (m) => m.MainCourseComponent
      ),
    data: { roles: ['student'] },
    canActivate: [rolesGuard],
  },

  {
    path: 'grammar',
    loadComponent: () =>
      import('./pages/grammar-help/grammar-help.component').then(
        (m) => m.GrammarHelpComponent
      ),
  },

  {
    path: 'Q&A',
    loadComponent: () =>
      import('./pages/vqa/vqa.component').then((m) => m.VqaComponent),
  },

  {
    path: 'listening',
    loadComponent: () =>
      import('./pages/listening/listening.component').then(
        (m) => m.ListeningComponent
      ),
  },

  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent),
    canActivate: [authRedirectGuardGuard],
  },

  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register/register.component').then(
        (m) => m.RegisterComponent
      ),
    canActivate: [authRedirectGuardGuard],
  },

  {
    path: 'profile',
    loadComponent: () =>
      import('./pages/profile/profile.component').then(
        (m) => m.ProfileComponent
      ),
    children: [
      {
        path: 'student',
        data: { roles: ['student'] },
        canActivate: [rolesGuard],
        children: [
          { path: '', redirectTo: '1/1', pathMatch: 'full' },
          {
            path: ':userId/:collectionId',
            loadComponent: () =>
              import(
                './components/student-enrolled-courses/student-enrolled-courses.component'
              ).then((m) => m.StudentEnrolledCoursesComponent),
          },
          {
            path: '**',
            loadComponent: () =>
              import('./pages/not-found/not-found.component').then(
                (m) => m.NotFoundComponent
              ),
          },
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
            loadComponent: () =>
              import(
                './components/instructor-made-courses/instructor-made-courses.component'
              ).then((m) => m.InstructorMadeCoursesComponent),
          },
          {
            path: '**',
            loadComponent: () =>
              import('./pages/not-found/not-found.component').then(
                (m) => m.NotFoundComponent
              ),
          },
        ],
      },
      {
        path: 'admin',
        data: { roles: ['admin'] },
        canActivate: [rolesGuard],
        children: [
          {
            path: '',
            redirectTo: '1/1',
            pathMatch: 'full',
          },
          {
            path: ':userId/:userCollectionId',
            loadComponent: () =>
              import('./pages/admin/admin.component').then(
                (m) => m.AdminComponent
              ),
          },
          {
            path: '**',
            loadComponent: () =>
              import('./pages/not-found/not-found.component').then(
                (m) => m.NotFoundComponent
              ),
          },
        ],
      },
      {
        path: '**',
        loadComponent: () =>
          import('./pages/not-found/not-found.component').then(
            (m) => m.NotFoundComponent
          ),
      },
    ],
  },

  {
    path: 'courses',
    data: { roles: ['instructor'] },
    canActivate: [rolesGuard],
    children: [
      {
        path: 'adding/:instructorId',
        loadComponent: () =>
          import(
            './pages/instructor-add-course/instructor-add-course.component'
          ).then((m) => m.InstructorAddCourseComponent),
      },
      {
        path: 'editing/:instructorId/:courseId',
        loadComponent: () =>
          import(
            './pages/instructor-edit-course/instructor-edit-course.component'
          ).then((m) => m.InstructorEditCourseComponent),
      },
      {
        path: '**',
        loadComponent: () =>
          import('./pages/not-found/not-found.component').then(
            (m) => m.NotFoundComponent
          ),
      },
    ],
  },

  {
    path: 'forget-password',
    loadComponent: () =>
      import('./pages/forget-password/forget-password.component').then(
        (m) => m.ForgetPasswordComponent
      ),
    canActivate: [authRedirectGuardGuard],
  },

  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./pages/unothorized/unothorized.component').then(
        (m) => m.UnothorizedComponent
      ),
  },
  {
    path: 'confirm-password',
    loadComponent: () =>
      import('./pages/confirm-password/confirm-password.component').then(
        (m) => m.ConfirmPasswordComponent
      ),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found.component').then(
        (m) => m.NotFoundComponent
      ),
  },
];
