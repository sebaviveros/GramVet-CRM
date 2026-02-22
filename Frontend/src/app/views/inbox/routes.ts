import { Routes } from '@angular/router';
import { InboxComponent } from './inbox.component';

export const routes: Routes = [
  {
    path: '',
    component: InboxComponent,
    data: {
      title: 'Buzón'
    }
  }
];