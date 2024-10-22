import { Component, HostBinding } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'Frontend';

  isDarkMode = false;

  @HostBinding('class.dark') get darkModeClass() {
    return this.isDarkMode;
  }

  ngOnInit() {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.isDarkMode = prefersDark;
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
  }
}
