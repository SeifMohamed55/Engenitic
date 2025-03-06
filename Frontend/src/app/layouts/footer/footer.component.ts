import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-footer',
  imports: [RouterModule, CommonModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})
export class FooterComponent implements OnInit, OnDestroy {

  constructor (
    private _UserService:UserService
  ){}

  private destroy$ = new Subject<void>();
  registered : string | null = null;

  ngOnInit() : void {
      this._UserService.registered.pipe(takeUntil(this.destroy$)).subscribe(data=>{
        this.registered = data;
      })
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  };
}
