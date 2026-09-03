import { AbstractControl, FormArray, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Fails when a `FormArray` holds fewer than `min` controls. */
export function minLengthArray(min: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const length = control instanceof FormArray ? control.length : 0;
    return length >= min ? null : { minLengthArray: { required: min, actual: length } };
  };
}
