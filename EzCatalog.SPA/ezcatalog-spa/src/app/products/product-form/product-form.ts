import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AddProductRequest, ProductsService } from '../../core/api/generated';
import {
  pricePositiveValidator,
  productNameValidator,
  skuValidator,
} from './product-validators';
import { RouterModule } from '@angular/router';

type Currency = AddProductRequest.PriceCurrencyEnum;
const Currency = AddProductRequest.PriceCurrencyEnum;

@Component({
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterModule],
  selector: 'ez-catalog-product-form',
  styleUrl: './product-form.css',
  templateUrl: './product-form.html',
})
export class ProductForm {
  private readonly api = inject(ProductsService);
  protected readonly currencies = Object.values(Currency);
  protected readonly defaults = {
    name: 'New product',
    sku: '',
    priceUnits: '0',
    priceDecimals: '00',
    priceCurrency: Currency.Pln,
  };

  submitting = signal<boolean>(false);
  submitFailed = signal<boolean>(false);

  formGroup = new FormGroup({
    name: new FormControl(this.defaults.name, { nonNullable: true, validators: productNameValidator }),
    sku: new FormControl(this.defaults.sku, { nonNullable: true, validators: skuValidator }),
    priceUnits: new FormControl(this.defaults.priceUnits, { nonNullable: true }),
    priceDecimals: new FormControl(this.defaults.priceDecimals, { nonNullable: true }),
    priceCurrency: new FormControl<Currency>(this.defaults.priceCurrency, { nonNullable: true }),
  }, {
    validators: pricePositiveValidator
  });

  protected keepDigitsOnly(control: FormControl<string>): void {
    const digits = control.value.replace(/\D/g, '');
    if (digits !== control.value) {
      control.setValue(digits);
    }
  }

  protected submit(): void {
    this.submitting.set(true);
    this.submitFailed.set(false);

    const form = this.formGroup.getRawValue();
    const product: AddProductRequest = {
        name: form.name,
        sku: form.sku,
        priceAmount: Number(`${form.priceUnits}.${form.priceDecimals}`),
        priceCurrency: form.priceCurrency,
    };

    this.api
      .addProduct(product)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => this.formGroup.reset(this.defaults),
        error: () => this.submitFailed.set(true),
      });
  }
}
