import { Component, OnInit, HostListener } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { User } from 'src/app/models/user.model';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent implements OnInit {
  user: User = JSON.parse(localStorage.getItem('user') || '{}');
  pageTitle: string = 'Interview Genie';
  isSidebarOpen = false;
  isDropdownOpen = false;
  currentYear: number = new Date().getFullYear();

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private titleService: Title
  ) {}

  ngOnInit() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        const childRoute = this.getChild(this.activatedRoute);
        childRoute.data.subscribe((data) => {
          this.pageTitle = data['title'] || 'Ana Sayfa';
          this.titleService.setTitle(this.pageTitle);
        });
      });
  }

  getChild(activatedRoute: ActivatedRoute): ActivatedRoute {
    return activatedRoute.firstChild ? this.getChild(activatedRoute.firstChild) : activatedRoute;
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar() {
    this.isSidebarOpen = false;
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  handleOutsideClick(event: Event) {
    const target = event.target as HTMLElement;
    if (
      !target.closest('#user-dropdown') &&
      !target.closest('#dropdown-button')
    ) {
      this.isDropdownOpen = false;
    }
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.router.navigate(['/auth/login']);
  }
}
