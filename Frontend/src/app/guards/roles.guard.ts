import { inject } from '@angular/core';
import { UserService } from './../feature/users/user.service';
import { CanActivateFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { filter, map, take, tap } from 'rxjs';

export const rolesGuard: CanActivateFn = (route, state) => {
  const expectedRole = route.data['roles'] as Array<string>;
  const _UserService = inject(UserService);
  const _Router = inject(Router);

  if(!expectedRole.includes(_UserService.role.value)){
    _UserService.redirectReason.next('unothorized');
    _Router.navigate(['/unothorized']);
    return false;
  }
  return true;
};
