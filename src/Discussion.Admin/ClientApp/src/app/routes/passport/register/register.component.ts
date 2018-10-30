import {Component, NgZone, OnDestroy} from '@angular/core';
import { Router } from '@angular/router';
import {
  FormGroup,
  FormBuilder,
  Validators,
  FormControl,
} from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd';
import {_HttpClient} from '@delon/theme';
import { ApiResponse } from '../../../api-response';

@Component({
  selector: 'passport-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.less'],
})
export class UserRegisterComponent implements OnDestroy {
  form: FormGroup;
  error = '';
  loading = false;
  visible = false;
  status = 'poor';
  progress = 0;
  passwordProgressMap = {
    ok: 'success',
    pass: 'normal',
    poor: 'exception',
  };


  constructor(
    fb: FormBuilder,
    public msg: NzMessageService,
    private ngZone: NgZone,
    private router: Router,
    private httpClient: _HttpClient
  ) {
      this.form = fb.group({
      username: [null, [
        Validators.required,
        Validators.minLength(3),
        Validators.pattern(/^[a-zA-Z0-9\-_]+$/)]],
      password: [
        null,
        [
          Validators.required,
          Validators.minLength(6),
          UserRegisterComponent.checkPasswordRules.bind(this),
          UserRegisterComponent.updatePasswordHint.bind(this),
        ],
      ],
      confirm: [
        null,
        [
          Validators.required,
          Validators.minLength(6),
          UserRegisterComponent.passwordEquar,
        ],
      ],
    });
  }

  static checkPasswordRules(control: FormControl) {
    if (!control || !control.value) {
      return null;
    }

    const val = control.value;
    const numbers = /\d/;
    const lower = /[a-z]/;
    const upper = /[A-Z]/;
    const symbols = /[\x20,<.>/?;:'"[{\]}\\|`~!@#$%^&*()\-_=+]/;

    let counter = 0;
    if (numbers.test(val)) {
      counter++;
    }

    if (lower.test(val)) {
      counter++;
    }

    if (upper.test(val)) {
      counter++;
    }

    if (symbols.test(val)) {
      counter++;
    }

    return counter >= 2 ? null : {
      'rules': '请至少包含两种：大写、小写、字符，特殊字符'
    };
  }


  static updatePasswordHint(control: FormControl) {
    if (!control) return null;
    const self: any = this;
    self.visible = !!control.value;

    if (!control.value) {
      return null;
    }

    if (control.value.length > 9) {
      self.status = 'ok';
    } else if (control.value.length > 5) {
      self.status = 'pass';
    } else {
      self.status = 'poor';
    }

    if (self.visible) {
      self.progress = Math.min(control.value.length * 10, 100);
    }
  }

  static passwordEquar(control: FormControl) {
    if (!control || !control.parent) return null;
    if (control.value !== control.parent.get('password').value) {
      return { equar: true };
    }
    return null;
  }

  // region: fields

  get username() {
    return this.form.controls.username;
  }
  get password() {
    return this.form.controls.password;
  }
  get confirm() {
    return this.form.controls.confirm;
  }

  // endregion

  submit() {
    this.error = '';
    for (const i of Object.keys(this.form.controls)) {
      this.form.controls[i].markAsDirty();
      this.form.controls[i].updateValueAndValidity();
    }

    if (this.form.invalid) {
      return;
    }


    this.loading = true;
    this.httpClient.post<ApiResponse>('api/account/register',
      {
        userName: this.form.controls.username.value,
        password: this.form.controls.password.value
      })
      .subscribe(res => {
        this.loading = false;
        if (res.hasSucceeded) {
          this.ngZone.run(() => this.router.navigate(['/passport/login'])).then();
        } else {
          this.error = res.errorMessage;
        }
      });
  }

  ngOnDestroy(): void {
  }
}
