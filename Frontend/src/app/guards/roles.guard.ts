import { inject } from '@angular/core';
import { UserService } from './../feature/users/user.service';
import { CanActivateFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

export const rolesGuard: CanActivateFn = (route, state) => {
  const expectedRole = route.data['roles'] as Array<string>;
  const _UserService = inject(UserService);
  const _Router = inject(Router);
  if (expectedRole.includes(_UserService.role.value)) {
    return true;
  }
  _Router.navigate(['/unauthorized']);
  return false;
};
