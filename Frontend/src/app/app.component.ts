
import { Component, HostListener } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./layouts/navbar/navbar.component";
import { FooterComponent } from "./layouts/footer/footer.component";
import { NgxSpinnerModule } from 'ngx-spinner';

@Component({
  selector: 'app-root',
  imports: [NavbarComponent, FooterComponent, RouterOutlet, NgxSpinnerModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  standalone: true
})
export class AppComponent {
  isVisible: boolean = false;
  scrollHeight: number = 100; 


  handleArrow(event : Event) : void {
      window.scrollTo({
        behavior : 'smooth',
        top : 0
      });
  };

  @HostListener('window:scroll', [])
  onScroll(): void {
    const scrollPosition = window.scrollY || document.documentElement.scrollTop;
    this.isVisible = scrollPosition > this.scrollHeight;
  }



}
