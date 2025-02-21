import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UserService } from '../../feature/users/user.service';

@Component({
  selector: 'app-footer',
  imports: [RouterModule, CommonModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})
export class FooterComponent implements OnInit {

  constructor (
    private _UserService:UserService
  ){}

  registered : string | null = null;

  ngOnInit() : void {
      this._UserService.registered.subscribe(data=>{
        this.registered = data;
      })
  }
}
