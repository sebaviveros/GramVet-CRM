import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { staffGuard } from './core/guards/staff.guard';

export const routes: Routes = [


  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },


  {
    path: 'login',
    loadComponent: () =>
      import('./views/pages/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Login Page'
    }
  },


  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout').then(m => m.DefaultLayoutComponent),
    data: {
      title: 'Home'
    },
    children: [
      {
        path: 'users',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./modules/users/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'tags',
        canActivate: [staffGuard],
        loadComponent: () =>
          import('./modules/tags/tags/tags.component').then(m => m.TagsComponent)
      },
      {
        path: 'macros',
        canActivate: [staffGuard],
        loadComponent: () =>
          import('./modules/macros/macros/macros.component').then(m => m.MacrosComponent)
      },
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./views/dashboard/routes').then((m) => m.routes)
      },
      {
        path: 'inbox',
        loadChildren: () =>
          import('./views/inbox/routes').then((m) => m.routes)
      }
    ]
  },

  {
    path: '404',
    loadComponent: () =>
      import('./views/pages/page404/page404.component').then(m => m.Page404Component),
    data: {
      title: 'Page 404'
    }
  },
  {
    path: '500',
    loadComponent: () =>
      import('./views/pages/page500/page500.component').then(m => m.Page500Component),
    data: {
      title: 'Page 500'
    }
  },


  {
    path: '**',
    redirectTo: 'login'
  }
];