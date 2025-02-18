import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./layouts/navbar/navbar.component";
import { FooterComponent } from "./layouts/footer/footer.component";
import { NgxSpinnerModule } from 'ngx-spinner';

@Component({
  selector: 'app-root',
  imports: [NavbarComponent, FooterComponent, RouterOutlet, NgxSpinnerModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  
  
  handleArrow(event : Event) : void{
    const arrowUp = event.target as HTMLButtonElement;
    arrowUp.addEventListener('click', ()=>{
      window.scrollTo({
        behavior : 'smooth',
        top : 0
      });
    });
  };

}
