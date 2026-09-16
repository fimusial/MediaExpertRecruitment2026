import { AbstractControl, ValidationErrors } from '@angular/forms';

const SKU_MIN_LENGTH = 4;
const SKU_MAX_LENGTH = 32;
const SKU_PATTERN = /^[A-Z0-9]+(-[A-Z0-9]+)*$/;
const NAME_MIN_LENGTH = 1;
const NAME_MAX_LENGTH = 500;
const CONTROL_CHARACTER = /\p{Cc}/u;

export function skuValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';

  if (!value.trim()) {
    return { message: 'SKU cannot be empty.' };
  }

  const normalized = value.trim().toUpperCase();
  if (normalized.length < SKU_MIN_LENGTH || normalized.length > SKU_MAX_LENGTH) {
    return {
      message: `Normalized SKU must be between ${SKU_MIN_LENGTH} and ${SKU_MAX_LENGTH} characters.`,
    };
  }

  if (!SKU_PATTERN.test(normalized)) {
    return {
      message:
        'SKU may contain only letters, digits and single hyphens, and cannot start or end with a hyphen.',
    };
  }

  return null;
}

export function productNameValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';

  if (!value.trim()) {
    return { message: 'Product name cannot be empty.' };
  }

  if (CONTROL_CHARACTER.test(value)) {
    return { message: 'Product name cannot contain control characters.' };
  }

  const normalized = value.trim();
  if (normalized.length < NAME_MIN_LENGTH || normalized.length > NAME_MAX_LENGTH) {
    return {
      message: `Normalized product name must be between ${NAME_MIN_LENGTH} and ${NAME_MAX_LENGTH} characters.`,
    };
  }

  return null;
}

export function pricePositiveValidator(group: AbstractControl): ValidationErrors | null {
  const units: string = group.get('priceUnits')?.value ?? '';
  const decimals: string = group.get('priceDecimals')?.value ?? '';

  if (Number(`${units}.${decimals}`) > 0) {
    return null;
  }

  return {
    priceError: {
        message: 'Product price must be positive.'
    }
  };
}
