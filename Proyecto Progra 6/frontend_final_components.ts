// src/app/components/sales/sale-dialog/sale-dialog.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, map, startWith } from 'rxjs';
import { SaleService } from '../../../services/sale.service';
import { ProductService } from '../../../services/product.service';
import { Product } from '../../../models/product.model';
import { CreateSale, CreateSaleDetail } from '../../../models/sale.model';

@Component({
  selector: 'app-sale-dialog',
  templateUrl: './sale-dialog.component.html',
  styleUrls: ['./sale-dialog.component.css']
})
export class SaleDialogComponent implements OnInit {
  saleForm: FormGroup;
  products: Product[] = [];
  filteredProducts: Observable<Product[]> | undefined;
  loading = false;
  total = 0;
  subtotal = 0;
  impuesto = 0;
  
  constructor(
    private formBuilder: FormBuilder,
    private saleService: SaleService,
    private productService: ProductService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<SaleDialogComponent>
  ) {
    this.saleForm = this.formBuilder.group({
      clienteNombre: ['', [Validators.required]],
      clienteIdentificacion: [''],
      clienteTelefono: [''],
      descuento: [0, [Validators.min(0)]],
      detallesVenta: this.formBuilder.array([])
    });
  }

  ngOnInit(): void {
    this.loadProducts();
    this.addSaleDetail();
  }

  get detallesVenta(): FormArray {
    return this.saleForm.get('detallesVenta') as FormArray;
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe({
      next: (products) => {
        this.products = products.filter(p => p.activo && p.stock > 0);
      },
      error: (error) => {
        this.snackBar.open('Error al cargar productos', 'Cerrar', { duration: 3000 });
      }
    });
  }

  addSaleDetail(): void {
    const detailGroup = this.formBuilder.group({
      productoId: ['', [Validators.required]],
      cantidad: [1, [Validators.required, Validators.min(1)]],
      descuento: [0, [Validators.min(0)]]
    });

    this.detallesVenta.push(detailGroup);
    this.updateTotal();
  }

  removeSaleDetail(index: number): void {
    this.detallesVenta.removeAt(index);
    this.updateTotal();
  }

  getProduct(productId: number): Product | undefined {
    return this.products.find(p => p.id === productId);
  }

  getMaxQuantity(productId: number): number {
    const product = this.getProduct(productId);
    return product ? product.stock : 0;
  }

  updateTotal(): void {
    let subtotal = 0;
    
    this.detallesVenta.controls.forEach(control => {
      const productoId = control.get('productoId')?.value;
      const cantidad = control.get('cantidad')?.value || 0;
      const descuento = control.get('descuento')?.value || 0;
      
      if (productoId) {
        const product = this.getProduct(productoId);
        if (product) {
          subtotal += (product.precio * cantidad) - descuento;
        }
      }
    });

    const descuentoGeneral = this.saleForm.get('descuento')?.value || 0;
    this.subtotal = subtotal;
    this.impuesto = this.subtotal * 0.13; // 13% IVA
    this.total = this.subtotal + this.impuesto - descuentoGeneral;
  }

  onSubmit(): void {
    if (this.saleForm.valid && this.detallesVenta.length > 0) {
      this.loading = true;
      
      const saleData: CreateSale = {
        clienteNombre: this.saleForm.get('clienteNombre')?.value,
        clienteIdentificacion: this.saleForm.get('clienteIdentificacion')?.value,
        clienteTelefono: this.saleForm.get('clienteTelefono')?.value,
        descuento: this.saleForm.get('descuento')?.value || 0,
        detallesVenta: this.detallesVenta.value
      };

      this.saleService.createSale(saleData).subscribe({
        next: () => {
          this.loading = false;
          this.snackBar.open('Venta registrada exitosamente', 'Cerrar', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.loading = false;
          this.snackBar.open('Error al registrar venta: ' + error.error.message, 'Cerrar', { duration: 5000 });
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}

// src/app/components/sales/sale-dialog/sale-dialog.component.html
<h2 mat-dialog-title>Nueva Venta</h2>

<form [formGroup]="saleForm" (ngSubmit)="onSubmit()">
  <mat-dialog-content>
    <!-- Cliente Information -->
    <div class="section">
      <h3>Información del Cliente</h3>
      <div class="form-row two-columns">
        <mat-form-field appearance="outline">
          <mat-label>Nombre del Cliente</mat-label>
          <input matInput formControlName="clienteNombre" required>
          <mat-error>El nombre del cliente es requerido</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Identificación</mat-label>
          <input matInput formControlName="clienteIdentificacion">
        </mat-form-field>
      </div>

      <div class="form-row">
        <mat-form-field appearance="outline" class="half-width">
          <mat-label>Teléfono</mat-label>
          <input matInput formControlName="clienteTelefono">
        </mat-form-field>
      </div>
    </div>

    <!-- Products Section -->
    <div class="section">
      <div class="section-header">
        <h3>Productos</h3>
        <button mat-icon-button color="primary" type="button" (click)="addSaleDetail()">
          <mat-icon>add</mat-icon>
        </button>
      </div>

      <div formArrayName="detallesVenta" class="products-container">
        <div *ngFor="let detail of detallesVenta.controls; let i = index" 
             [formGroupName]="i" class="product-row">
          
          <mat-form-field appearance="outline" class="product-select">
            <mat-label>Producto</mat-label>
            <mat-select formControlName="productoId" (selectionChange)="updateTotal()">
              <mat-option *ngFor="let product of products" [value]="product.id">
                {{product.nombre}} - {{product.precio | currency:'USD':'symbol':'1.2-2'}} (Stock: {{product.stock}})
              </mat-option>
            </mat-select>
            <mat-error>Seleccione un producto</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="quantity-field">
            <mat-label>Cantidad</mat-label>
            <input matInput type="number" formControlName="cantidad" 
                   [max]="getMaxQuantity(detail.get('productoId')?.value)"
                   min="1" (input)="updateTotal()">
            <mat-error>Cantidad inválida</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="discount-field">
            <mat-label>Descuento</mat-label>
            <input matInput type="number" formControlName="descuento" 
                   min="0" step="0.01" (input)="updateTotal()">
            <span matSuffix>$</span>
          </mat-form-field>

          <div class="product-total">
            <strong>{{getProduct(detail.get('productoId')?.value)?.precio * detail.get('cantidad')?.value - detail.get('descuento')?.value | currency:'USD':'symbol':'1.2-2'}}</strong>
          </div>

          <button mat-icon-button color="warn" type="button" 
                  (click)="removeSaleDetail(i)"
                  [disabled]="detallesVenta.length <= 1">
            <mat-icon>delete</mat-icon>
          </button>
        </div>
      </div>
    </div>

    <!-- Totals Section -->
    <div class="section totals-section">
      <div class="totals-grid">
        <div class="form-row">
          <mat-form-field appearance="outline" class="discount-field">
            <mat-label>Descuento General</mat-label>
            <input matInput type="number" formControlName="descuento" 
                   min="0" step="0.01" (input)="updateTotal()">
            <span matSuffix>$</span>
          </mat-form-field>
        </div>

        <div class="totals-summary">
          <div class="total-line">
            <span>Subtotal:</span>
            <strong>{{subtotal | currency:'USD':'symbol':'1.2-2'}}</strong>
          </div>
          <div class="total-line">
            <span>IVA (13%):</span>
            <strong>{{impuesto | currency:'USD':'symbol':'1.2-2'}}</strong>
          </div>
          <div class="total-line">
            <span>Descuento:</span>
            <strong>-{{saleForm.get('descuento')?.value | currency:'USD':'symbol':'1.2-2'}}</strong>
          </div>
          <div class="total-line final-total">
            <span>Total:</span>
            <strong>{{total | currency:'USD':'symbol':'1.2-2'}}</strong>
          </div>
        </div>
      </div>
    </div>
  </mat-dialog-content>

  <mat-dialog-actions align="end">
    <button mat-button (click)="onCancel()" type="button">Cancelar</button>
    <button mat-raised-button color="primary" type="submit" 
            [disabled]="!saleForm.valid || loading || detallesVenta.length === 0">
      <mat-progress-spinner *ngIf="loading" diameter="20" class="spinner"></mat-progress-spinner>
      <span *ngIf="!loading">Registrar Venta</span>
    </button>
  </mat-dialog-actions>
</form>

<!-- src/app/components/sales/sale-dialog/sale-dialog.component.css -->
<style>
.section {
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e0e0e0;
}

.section:last-child {
  border-bottom: none;
}

.section h3 {
  margin-bottom: 16px;
  color: #333;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.form-row {
  margin-bottom: 16px;
}

.two-columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.half-width {
  width: 50%;
}

.products-container {
  max-height: 300px;
  overflow-y: auto;
}

.product-row {
  display: grid;
  grid-template-columns: 2fr 100px 120px 100px 40px;
  gap: 12px;
  align-items: center;
  margin-bottom: 16px;
  padding: 16px;
  background-color: #f9f9f9;
  border-radius: 8px;
}

.product-select {
  min-width: 250px;
}

.quantity-field, .discount-field {
  width: 100%;
}

.product-total {
  text-align: center;
  color: #667eea;
  font-weight: 500;
}

.totals-section {
  background-color: #f5f5f5;
  padding: 20px;
  border-radius: 8px;
  margin-top: 20px;
}

.totals-grid {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.totals-summary {
  min-width: 250px;
}

.total-line {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  padding: 4px 0;
}

.final-total {
  border-top: 2px solid #667eea;
  margin-top: 8px;
  padding-top: 8px;
  font-size: 1.1em;
}

.final-total span,
.final-total strong {
  color: #667eea;
}

.spinner {
  margin-right: 8px;
}

mat-dialog-content {
  max-width: 800px;
  min-height: 500px;
}
</style>

// src/app/components/categories/categories.component.ts
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/category.model';

@Component({
  selector: 'app-categories',
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.css']
})
export class CategoriesComponent implements OnInit {
  displayedColumns: string[] = ['icono', 'nombre', 'descripcion', 'fechaCreacion', 'actions'];
  dataSource = new MatTableDataSource<Category>();
  loading = false;

  constructor(
    private categoryService: CategoryService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getCategories().subscribe({
      next: (categories) => {
        this.dataSource.data = categories;
        this.loading = false;
      },
      error: (error) => {
        this.snackBar.open('Error al cargar categorías', 'Cerrar', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  deleteCategory(id: number): void {
    if (confirm('¿Está seguro de eliminar esta categoría?')) {
      this.categoryService.deleteCategory(id).subscribe({
        next: () => {
          this.snackBar.open('Categoría eliminada exitosamente', 'Cerrar', { duration: 3000 });
          this.loadCategories();
        },
        error: (error) => {
          this.snackBar.open('Error al eliminar categoría', 'Cerrar', { duration: 3000 });
        }
      });
    }
  }
}

// package.json
{
  "name": "ferreteria-frontend",
  "version": "0.0.0",
  "scripts": {
    "ng": "ng",
    "start": "ng serve",
    "build": "ng build",
    "watch": "ng build --watch --configuration development",
    "test": "ng test"
  },
  "private": true,
  "dependencies": {
    "@angular/animations": "^15.0.0",
    "@angular/cdk": "^15.0.0",
    "@angular/common": "^15.0.0",
    "@angular/compiler": "^15.0.0",
    "@angular/core": "^15.0.0",
    "@angular/forms": "^15.0.0",
    "@angular/material": "^15.0.0",
    "@angular/platform-browser": "^15.0.0",
    "@angular/platform-browser-dynamic": "^15.0.0",
    "@angular/router": "^15.0.0",
    "chart.js": "^4.2.1",
    "ng2-charts": "^4.1.1",
    "rxjs": "~7.5.0",
    "tslib": "^2.3.0",
    "zone.js": "~0.12.0"
  },
  "devDependencies": {
    "@angular-devkit/build-angular": "^15.0.0",
    "@angular/cli": "~15.0.0",
    "@angular/compiler-cli": "^15.0.0",
    "@types/jasmine": "~4.3.0",
    "@types/node": "^18.7.0",
    "jasmine-core": "~4.5.0",
    "karma": "~6.4.0",
    "karma-chrome-headless": "~3.1.0",
    "karma-coverage": "~2.2.0",
    "karma-jasmine": "~5.1.0",
    "karma-jasmine-html-reporter": "~2.0.0",
    "typescript": "~4.8.0"
  }
}

// FerreriaAPI.csproj
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.25" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.25" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.25" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.25" />
    <PackageReference Include="AutoMapper" Version="12.0.1" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="CrystalReports.Engine" Version="13.0.4000" />
  </ItemGroup>
</Project>

// README.md
# 🔧 Sistema de Inventario y Ventas - Ferretería Nacional

Sistema integral de gestión para ferreterías desarrollado con **Angular 15+** y **ASP.NET Core 6+**.

## 🚀 Características

- ✅ **Autenticación JWT** - Sistema seguro de login
- ✅ **Gestión de Productos** - CRUD completo con categorías y proveedores
- ✅ **Sistema de Ventas** - Punto de venta con carrito intuitivo
- ✅ **Dashboard Ejecutivo** - KPIs y gráficos en tiempo real
- ✅ **Control de Inventario** - Alertas de stock mínimo
- ✅ **Reportes PDF** - Crystal Reports integrado
- ✅ **Diseño Responsivo** - Angular Material UI
- ✅ **API RESTful** - Documentación con Swagger

## 🛠️ Tecnologías

### Backend
- **ASP.NET Core 6**
- **Entity Framework Core**
- **SQL Server**
- **JWT Authentication**
- **Crystal Reports**
- **AutoMapper**

### Frontend
- **Angular 15+**
- **Angular Material**
- **Chart.js**
- **TypeScript**
- **RxJS**

## 📦 Instalación

### Prerrequisitos
```bash
• .NET 6 SDK
• Node.js 16+
• SQL Server Express
• Angular CLI 15+
```

### 1. Clonar el Repositorio
```bash
git clone https://github.com/tu-usuario/ferreteria-system.git
cd ferreteria-system
```

### 2. Configurar Backend
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### 3. Configurar Frontend
```bash
cd frontend
npm install
ng serve
```

## 🌐 Acceso
- **Frontend**: http://localhost:4200
- **API**: https://localhost:5001/api
- **Swagger**: https://localhost:5001/swagger

## 👥 Cuentas Demo
- **Admin**: admin@ferreteria.com / admin123
- **Vendedor**: vendedor@ferreteria.com / vendedor123

## 📊 Funcionalidades

### Dashboard
- KPIs de ventas y inventario
- Gráficos de tendencias
- Productos más vendidos
- Alertas de stock bajo

### Gestión de Productos
- CRUD completo
- Categorización
- Control de stock
- Códigos de barras
- Imágenes de productos

### Sistema de Ventas
- Punto de venta intuitivo
- Búsqueda de productos
- Carrito de compras
- Facturación automática
- Historial de ventas

### Reportes
- Ventas diarias/mensuales
- Inventario detallado
- Productos más vendidos
- Exportación a PDF

## 🗃️ Base de Datos

### Entidades Principales
- **Users** - Usuarios del sistema
- **Products** - Catálogo de productos
- **Categories** - Categorías de productos
- **Suppliers** - Proveedores
- **Sales** - Registro de ventas
- **SaleDetails** - Detalles de venta

## 🔐 Seguridad
- Autenticación JWT
- Autorización por roles
- Validaciones de entrada
- Encriptación de contraseñas
- CORS configurado

## 📱 Responsive Design
- Mobile-first approach
- Optimizado para tablets
- Interfaz intuitiva
- Navegación fluida

## 🤝 Contribuir
1. Fork el proyecto
2. Crea una rama feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia
Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## 📞 Soporte
Para soporte técnico, contacta a: support@ferreteria-system.com

---
**Desarrollado con ❤️ para Ferreterías de Costa Rica**