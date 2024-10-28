import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import {
  Router,
  NavigationEnd,
  ActivatedRoute,
  ActivatedRouteSnapshot
} from '@angular/router';
import { filter } from 'rxjs/operators';
import { User } from 'src/app/models/user.model';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit {
  user: User = JSON.parse(localStorage.getItem('user') || '{}');
  pageTitle: string = 'Ana Sayfa'; // Varsayılan başlık

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private titleService: Title
  ) { }

  ngOnInit() {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        const childRoute = this.getChild(this.activatedRoute);
        childRoute.data.subscribe(data => {
          this.pageTitle = data['title'] || 'Ana Sayfa';
          this.titleService.setTitle(this.pageTitle);
        });
      });
  }

  getChild(activatedRoute: ActivatedRoute): ActivatedRoute {
    if (activatedRoute.firstChild) {
      return this.getChild(activatedRoute.firstChild);
    } else {
      return activatedRoute;
    }
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/auth/login']);
  }
}