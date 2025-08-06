// src/app/components/products/product-dialog/product-dialog.component.ts
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductService } from '../../../services/product.service';
import { Product } from '../../../models/product.model';
import { Category } from '../../../models/category.model';
import { Supplier } from '../../../models/supplier.model';

@Component({
  selector: 'app-product-dialog',
  templateUrl: './product-dialog.component.html',
  styleUrls: ['./product-dialog.component.css']
})
export class ProductDialogComponent implements OnInit {
  productForm: FormGroup;
  isEdit = false;
  loading = false;

  constructor(
    private formBuilder: FormBuilder,
    private productService: ProductService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ProductDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {
      product?: Product,
      categories: Category[],
      suppliers: Supplier[]
    }
  ) {
    this.isEdit = !!data.product;
    
    this.productForm = this.formBuilder.group({
      nombre: [data.product?.nombre || '', [Validators.required, Validators.maxLength(200)]],
      descripcion: [data.product?.descripcion || '', [Validators.maxLength(500)]],
      precio: [data.product?.precio || 0, [Validators.required, Validators.min(0)]],
      stock: [data.product?.stock || 0, [Validators.required, Validators.min(0)]],
      stockMinimo: [data.product?.stockMinimo || 5, [Validators.required, Validators.min(1)]],
      codigoBarras: [data.product?.codigoBarras || ''],
      categoriaId: [data.product?.categoriaId || '', [Validators.required]],
      proveedorId: [data.product?.proveedorId || '', [Validators.required]],
      activo: [data.product?.activo ?? true]
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.productForm.valid) {
      this.loading = true;
      const productData = this.productForm.value;

      if (this.isEdit) {
        productData.id = this.data.product!.id;
        this.productService.updateProduct(this.data.product!.id, productData).subscribe({
          next: () => {
            this.loading = false;
            this.snackBar.open('Producto actualizado exitosamente', 'Cerrar', { duration: 3000 });
            this.dialogRef.close(true);
          },
          error: (error) => {
            this.loading = false;
            this.snackBar.open('Error al actualizar producto', 'Cerrar', { duration: 3000 });
          }
        });
      } else {
        this.productService.createProduct(productData).subscribe({
          next: () => {
            this.loading = false;
            this.snackBar.open('Producto creado exitosamente', 'Cerrar', { duration: 3000 });
            this.dialogRef.close(true);
          },
          error: (error) => {
            this.loading = false;
            this.snackBar.open('Error al crear producto', 'Cerrar', { duration: 3000 });
          }
        });
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}

// src/app/components/products/product-dialog/product-dialog.component.html
<h2 mat-dialog-title>{{isEdit ? 'Editar' : 'Nuevo'}} Producto</h2>

<form [formGroup]="productForm" (ngSubmit)="onSubmit()">
  <mat-dialog-content>
    <div class="form-row">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Nombre del Producto</mat-label>
        <input matInput formControlName="nombre" required>
        <mat-error *ngIf="productForm.get('nombre')?.hasError('required')">
          El nombre es requerido
        </mat-error>
      </mat-form-field>
    </div>

    <div class="form-row">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Descripción</mat-label>
        <textarea matInput formControlName="descripcion" rows="3"></textarea>
      </mat-form-field>
    </div>

    <div class="form-row two-columns">
      <mat-form-field appearance="outline">
        <mat-label>Categoría</mat-label>
        <mat-select formControlName="categoriaId" required>
          <mat-option *ngFor="let category of data.categories" [value]="category.id">
            {{category.nombre}}
          </mat-option>
        </mat-select>
        <mat-error *ngIf="productForm.get('categoriaId')?.hasError('required')">
          Seleccione una categoría
        </mat-error>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Proveedor</mat-label>
        <mat-select formControlName="proveedorId" required>
          <mat-option *ngFor="let supplier of data.suppliers" [value]="supplier.id">
            {{supplier.nombre}}
          </mat-option>
        </mat-select>
        <mat-error *ngIf="productForm.get('proveedorId')?.hasError('required')">
          Seleccione un proveedor
        </mat-error>
      </mat-form-field>
    </div>

    <div class="form-row three-columns">
      <mat-form-field appearance="outline">
        <mat-label>Precio</mat-label>
        <input matInput type="number" formControlName="precio" min="0" step="0.01" required>
        <span matSuffix>$</span>
        <mat-error *ngIf="productForm.get('precio')?.hasError('required')">
          El precio es requerido
        </mat-error>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Stock Actual</mat-label>
        <input matInput type="number" formControlName="stock" min="0" required>
        <mat-error *ngIf="productForm.get('stock')?.hasError('required')">
          El stock es requerido
        </mat-error>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Stock Mínimo</mat-label>
        <input matInput type="number" formControlName="stockMinimo" min="1" required>
        <mat-error *ngIf="productForm.get('stockMinimo')?.hasError('required')">
          El stock mínimo es requerido
        </mat-error>
      </mat-form-field>
    </div>

    <div class="form-row">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Código de Barras</mat-label>
        <input matInput formControlName="codigoBarras">
      </mat-form-field>
    </div>

    <div class="form-row">
      <mat-checkbox formControlName="activo">Producto Activo</mat-checkbox>
    </div>
  </mat-dialog-content>

  <mat-dialog-actions align="end">
    <button mat-button (click)="onCancel()" type="button">Cancelar</button>
    <button mat-raised-button color="primary" type="submit" 
            [disabled]="!productForm.valid || loading">
      <mat-progress-spinner *ngIf="loading" diameter="20" class="spinner"></mat-progress-spinner>
      <span *ngIf="!loading">{{isEdit ? 'Actualizar' : 'Crear'}}</span>
    </button>
  </mat-dialog-actions>
</form>

<!-- src/app/components/products/product-dialog/product-dialog.component.css -->
<style>
.form-row {
  margin-bottom: 15px;
}

.full-width {
  width: 100%;
}

.two-columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 15px;
}

.three-columns {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 15px;
}

.spinner {
  margin-right: 10px;
}

mat-dialog-content {
  min-width: 500px;
  padding-top: 20px;
}
</style>

// src/app/components/sales/sales.component.ts
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SaleService } from '../../services/sale.service';
import { Sale } from '../../models/sale.model';
import { SaleDialogComponent } from './sale-dialog/sale-dialog.component';

@Component({
  selector: 'app-sales',
  templateUrl: './sales.component.html',
  styleUrls: ['./sales.component.css']
})
export class SalesComponent implements OnInit {
  displayedColumns: string[] = ['numeroVenta', 'clienteNombre', 'fechaVenta', 'total', 'estado', 'usuarioNombre', 'actions'];
  dataSource = new MatTableDataSource<Sale>();
  loading = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private saleService: SaleService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadSales();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadSales(): void {
    this.loading = true;
    this.saleService.getSales().subscribe({
      next: (sales) => {
        this.dataSource.data = sales;
        this.loading = false;
      },
      error: (error) => {
        this.snackBar.open('Error al cargar ventas', 'Cerrar', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  openSaleDialog(): void {
    const dialogRef = this.dialog.open(SaleDialogComponent, {
      width: '900px',
      maxHeight: '90vh'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadSales();
      }
    });
  }

  viewSaleDetails(sale: Sale): void {
    // Implementar vista de detalles de venta
    console.log('Ver detalles de venta:', sale);
  }

  getStatusColor(estado: string): string {
    switch (estado) {
      case 'Completada':
        return 'success';
      case 'Pendiente':
        return 'warning';
      case 'Cancelada':
        return 'danger';
      default:
        return 'default';
    }
  }
}

// src/app/components/sales/sales.component.html
<div class="sales-container">
  <div class="header-section">
    <h1>Gestión de Ventas</h1>
    <button mat-raised-button color="primary" (click)="openSaleDialog()">
      <mat-icon>add_shopping_cart</mat-icon>
      Nueva Venta
    </button>
  </div>

  <mat-card class="table-card">
    <mat-card-header>
      <mat-card-title>Lista de Ventas</mat-card-title>
    </mat-card-header>
    
    <mat-card-content>
      <div class="filter-section">
        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Buscar ventas</mat-label>
          <input matInput (keyup)="applyFilter($event)" placeholder="Número, cliente, vendedor...">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="dataSource" matSort class="sales-table">
          <!-- Número de Venta Column -->
          <ng-container matColumnDef="numeroVenta">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>N° Venta</th>
            <td mat-cell *matCellDef="let sale">
              <strong>{{sale.numeroVenta}}</strong>
            </td>
          </ng-container>

          <!-- Cliente Column -->
          <ng-container matColumnDef="clienteNombre">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Cliente</th>
            <td mat-cell *matCellDef="let sale">
              <div class="client-info">
                <strong>{{sale.clienteNombre}}</strong>
                <small *ngIf="sale.clienteIdentificacion">{{sale.clienteIdentificacion}}</small>
              </div>
            </td>
          </ng-container>

          <!-- Fecha Column -->
          <ng-container matColumnDef="fechaVenta">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Fecha</th>
            <td mat-cell *matCellDef="let sale">
              {{sale.fechaVenta | date:'dd/MM/yyyy HH:mm'}}
            </td>
          </ng-container>

          <!-- Total Column -->
          <ng-container matColumnDef="total">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Total</th>
            <td mat-cell *matCellDef="let sale">
              <strong>{{sale.total | currency:'USD':'symbol':'1.2-2'}}</strong>
            </td>
          </ng-container>

          <!-- Estado Column -->
          <ng-container matColumnDef="estado">
            <th mat-header-cell *matHeaderCellDef>Estado</th>
            <td mat-cell *matCellDef="let sale">
              <mat-chip-set>
                <mat-chip [class]="'status-' + getStatusColor(sale.estado)">
                  {{sale.estado}}
                </mat-chip>
              </mat-chip-set>
            </td>
          </ng-container>

          <!-- Vendedor Column -->
          <ng-container matColumnDef="usuarioNombre">
            <th mat-header-cell *matHeaderCellDef>Vendedor</th>
            <td mat-cell *matCellDef="let sale">{{sale.usuarioNombre}}</td>
          </ng-container>

          <!-- Actions Column -->
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Acciones</th>
            <td mat-cell *matCellDef="let sale">
              <button mat-icon-button color="primary" 
                      (click)="viewSaleDetails(sale)" 
                      matTooltip="Ver Detalles">
                <mat-icon>visibility</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>

        <mat-paginator [pageSizeOptions]="[10, 25, 50, 100]" 
                       showFirstLastButtons 
                       aria-label="Seleccionar página de ventas">
        </mat-paginator>
      </div>

      <div *ngIf="loading" class="loading-overlay">
        <mat-progress-spinner mode="indeterminate"></mat-progress-spinner>
      </div>
    </mat-card-content>
  </mat-card>
</div>