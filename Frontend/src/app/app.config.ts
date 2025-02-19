import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { BrowserAnimationsModule, provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';
import { headerInterceptor } from './auth/header.interceptor';
import { NgxSpinnerModule, provideSpinnerConfig } from 'ngx-spinner';
import { loaderInterceptor } from './auth/loader.interceptor';


export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimations(), // Provide animations
    provideToastr(), // Provide Toastr for notifications
    provideHttpClient(
      withInterceptors([headerInterceptor, loaderInterceptor]) // Add interceptors
    ),
    importProvidersFrom(NgxSpinnerModule.forRoot()), // Import and configure NgxSpinnerModule
    provideZoneChangeDetection({ eventCoalescing: true }), // Enable zone change detection
    provideRouter(routes), // Provide routes
    provideClientHydration(withEventReplay()),
  ],
};
