// src/app/app.component.ts
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: '<router-outlet></router-outlet>',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'ferreteria-frontend';
}

// src/app/components/auth/login/login.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  hidePassword = true;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.loading = true;
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          this.loading = false;
          this.snackBar.open('Login exitoso', 'Cerrar', { duration: 3000 });
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.loading = false;
          this.snackBar.open('Email o contraseña incorrectos', 'Cerrar', { duration: 3000 });
        }
      });
    }
  }
}

// src/app/components/auth/login/login.component.html
<div class="login-container">
  <mat-card class="login-card">
    <mat-card-header>
      <div class="login-header">
        <mat-icon class="login-icon">store</mat-icon>
        <mat-card-title>Ferretería Nacional</mat-card-title>
        <mat-card-subtitle>Sistema de Inventario y Ventas</mat-card-subtitle>
      </div>
    </mat-card-header>
    
    <mat-card-content>
      <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" required>
          <mat-icon matSuffix>email</mat-icon>
          <mat-error *ngIf="loginForm.get('email')?.hasError('required')">
            El email es requerido
          </mat-error>
          <mat-error *ngIf="loginForm.get('email')?.hasError('email')">
            Email inválido
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Contraseña</mat-label>
          <input matInput [type]="hidePassword ? 'password' : 'text'" formControlName="password" required>
          <button mat-icon-button matSuffix (click)="hidePassword = !hidePassword" type="button">
            <mat-icon>{{hidePassword ? 'visibility_off' : 'visibility'}}</mat-icon>
          </button>
          <mat-error *ngIf="loginForm.get('password')?.hasError('required')">
            La contraseña es requerida
          </mat-error>
        </mat-form-field>

        <button mat-raised-button color="primary" type="submit" 
                [disabled]="!loginForm.valid || loading" class="full-width login-button">
          <mat-progress-spinner *ngIf="loading" diameter="20" class="spinner"></mat-progress-spinner>
          <span *ngIf="!loading">Iniciar Sesión</span>
        </button>
      </form>

      <div class="demo-accounts">
        <p><strong>Cuentas de Demostración:</strong></p>
        <p>Admin: admin@ferreteria.com / admin123</p>
        <p>Vendedor: vendedor@ferreteria.com / vendedor123</p>
      </div>
    </mat-card-content>
  </mat-card>
</div>

<!-- src/app/components/auth/login/login.component.css -->
<style>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.login-card {
  width: 100%;
  max-width: 400px;
  padding: 20px;
}

.login-header {
  text-align: center;
  margin-bottom: 20px;
}

.login-icon {
  font-size: 48px;
  height: 48px;
  width: 48px;
  color: #667eea;
  margin-bottom: 10px;
}

.full-width {
  width: 100%;
  margin-bottom: 15px;
}

.login-button {
  height: 48px;
  font-size: 16px;
  margin-top: 20px;
}

.spinner {
  margin-right: 10px;
}

.demo-accounts {
  margin-top: 20px;
  padding: 15px;
  background-color: #f5f5f5;
  border-radius: 8px;
  font-size: 12px;
}
</style>

// src/app/components/layout/layout.component.ts
import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit {
  currentUser: User | null = null;
  sidenavOpened = true;

  menuItems = [
    { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
    { icon: 'inventory', label: 'Productos', route: '/products' },
    { icon: 'shopping_cart', label: 'Ventas', route: '/sales' },
    { icon: 'category', label: 'Categorías', route: '/categories' },
    { icon: 'business', label: 'Proveedores', route: '/suppliers' }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  logout(): void {
    this.authService.logout();
  }

  toggleSidenav(): void {
    this.sidenavOpened = !this.sidenavOpened;
  }
}

// src/app/components/layout/layout.component.html
<mat-sidenav-container class="sidenav-container">
  <mat-sidenav #drawer class="sidenav" fixedInViewport
               [attr.role]="'navigation'"
               [mode]="'side'"
               [opened]="sidenavOpened">
    <div class="sidenav-header">
      <mat-icon class="logo-icon">store</mat-icon>
      <h2>Ferretería</h2>
    </div>
    
    <mat-nav-list>
      <a mat-list-item *ngFor="let item of menuItems" 
         [routerLink]="item.route" 
         routerLinkActive="active-link">
        <mat-icon matListIcon>{{item.icon}}</mat-icon>
        <span>{{item.label}}</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary" class="toolbar">
      <button type="button" 
              aria-label="Toggle sidenav" 
              mat-icon-button 
              (click)="toggleSidenav()">
        <mat-icon aria-label="Side nav toggle icon">menu</mat-icon>
      </button>
      
      <span class="toolbar-spacer"></span>
      
      <button mat-button [matMenuTriggerFor]="userMenu" class="user-button">
        <mat-icon>account_circle</mat-icon>
        <span class="user-name">{{currentUser?.nombre}}</span>
        <mat-icon>arrow_drop_down</mat-icon>
      </button>
      
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item disabled>
          <mat-icon>person</mat-icon>
          <span>{{currentUser?.rol}}</span>
        </button>
        <mat-divider></mat-divider>
        <button mat-menu-item (click)="logout()">
          <mat-icon>logout</mat-icon>
          <span>Cerrar Sesión</span>
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="main-content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>

<!-- src/app/components/layout/layout.component.css -->
<style>
.sidenav-container {
  height: 100vh;
}

.sidenav {
  width: 260px;
  background-color: #fafafa;
}

.sidenav-header {
  display: flex;
  align-items: center;
  padding: 20px 16px;
  background-color: #667eea;
  color: white;
  margin-bottom: 20px;
}

.logo-icon {
  font-size: 32px;
  height: 32px;
  width: 32px;
  margin-right: 12px;
}

.toolbar {
  position: sticky;
  top: 0;
  z-index: 1000;
}

.toolbar-spacer {
  flex: 1 1 auto;
}

.user-button {
  display: flex;
  align-items: center;
}

.user-name {
  margin: 0 8px;
}

.main-content {
  padding: 20px;
  min-height: calc(100vh - 64px);
  background-color: #f5f5f5;
}

.active-link {
  background-color: rgba(103, 126, 234, 0.1) !important;
  color: #667eea !important;
}

.active-link mat-icon {
  color: #667eea !important;
}
</style>

// src/app/components/dashboard/dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  kpis: any = {};
  ventasPorMes: any[] = [];
  productosMasVendidos: any[] = [];
  productosStockBajo: any[] = [];
  loading = true;

  constructor(
    private dashboardService: DashboardService,
    private productService: ProductService
  ) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;

    // Cargar KPIs
    this.dashboardService.getKPIs().subscribe({
      next: (kpis) => {
        this.kpis = kpis;
      },
      error: (error) => {
        console.error('Error loading KPIs:', error);
      }
    });

    // Cargar ventas por mes
    this.dashboardService.getVentasPorMes().subscribe({
      next: (ventas) => {
        this.ventasPorMes = ventas;
      },
      error: (error) => {
        console.error('Error loading sales by month:', error);
      }
    });

    // Cargar productos más vendidos
    this.dashboardService.getProductosMasVendidos().subscribe({
      next: (productos) => {
        this.productosMasVendidos = productos;
      },
      error: (error) => {
        console.error('Error loading top products:', error);
      }
    });

    // Cargar productos con stock bajo
    this.productService.getLowStockProducts().subscribe({
      next: (productos) => {
        this.productosStockBajo = productos;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading low stock products:', error);
        this.loading = false;
      }
    });
  }
}