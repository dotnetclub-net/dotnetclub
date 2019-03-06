import { _HttpClient } from '@delon/theme';
import {Component, OnDestroy, Inject, Optional, NgZone} from '@angular/core';
import { Router } from '@angular/router';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  TokenService,
  DA_SERVICE_TOKEN,
} from '@delon/auth';
import { StartupService } from '@core/startup/startup.service';
import {ApiResponse} from '../../../api-response';

@Component({
  selector: 'passport-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.less']
})
export class UserLoginComponent implements OnDestroy {
  form: FormGroup;
  error = '';
  loading = false;

  constructor(
    fb: FormBuilder,
    private router: Router,
    private ngZone: NgZone,
    @Inject(DA_SERVICE_TOKEN) private tokenService: TokenService,
    private startupSrv: StartupService,
    private http: _HttpClient) {

    this.form = fb.group({
      userName: [null, [Validators.required, Validators.minLength(3)]],
      password: [null, Validators.required]
    });
  }

  get userName() {
    return this.form.controls.userName;
  }
  get password() {
    return this.form.controls.password;
  }

  submit() {
    this.error = '';
    this.userName.markAsDirty();
    this.userName.updateValueAndValidity();

    this.password.markAsDirty();
    this.password.updateValueAndValidity();

    if (this.userName.invalid || this.password.invalid) return;

    this.http.post('api/account/signin', {
      userName: this.userName.value,
      password: this.password.value
    }).subscribe((res: ApiResponse) => {
      this.loading = false;
      if (!res.hasSucceeded) {
        if (!res.errorMessage) {
          res.errorMessage = '服务器返回错误：' + res.code;
        }

        this.error = res.errorMessage;
        return;
      }

      this.tokenService.set({
        token: res.result.token,
      });
      this.startupSrv.load().then(() =>
        this.ngZone.run(() =>
          this.router.navigate(['/'])
        ).then());
    });
    this.loading = this.http.loading;
  }

  ngOnDestroy(): void {
  }
}
