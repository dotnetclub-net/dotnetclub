
import {TestModuleMetadata} from '@angular/core/testing';
import {APP_BASE_HREF} from '@angular/common';
import {createComponent, dummyValidToken, getService, mockedHttpBackend, setUpTestBed} from '@testing/common.spec';

import {UserLoginComponent} from './login.component';
import {FormControl} from "@angular/forms";
import {StartupService} from "@core/startup/startup.service";
import {ITokenModel, JWTInterceptor, TokenService} from "@delon/auth";
import {HTTP_INTERCEPTORS} from "@angular/common/http";

describe('Component: UserLogin', () => {
  setUpTestBed(<TestModuleMetadata>{
    declarations: [UserLoginComponent],
    providers: [
      TokenService,
      StartupService,
      { provide: HTTP_INTERCEPTORS, useClass: JWTInterceptor, multi: true },
      { provide: APP_BASE_HREF, useValue: '/' }
    ],
  });

    let loginComp : UserLoginComponent;
    let usernameCtrl : FormControl;
    let passwordCtrl : FormControl;
    let mockedHttp;

    beforeEach(() => {
        mockedHttp = mockedHttpBackend();

        loginComp = createComponent(UserLoginComponent);
        usernameCtrl = loginComp.userName as FormControl;
        passwordCtrl = loginComp.password as FormControl;
    });


  it('should create user login component', () => {
    expect(loginComp).not.toBeNull();
  });

  it('should post credential to login and set token', () => {
    usernameCtrl.setValue("admin");
    passwordCtrl.setValue("password1");

    loginComp.submit();

    const request = mockedHttp.expectOne(req =>  {
      expect(req.method).toEqual('POST');
      expect(req.url).toEqual('api/account/signin');
      expect(req.body).toEqual({
        userName: 'admin',
        password: 'password1'
      });
      return true;
    });

    request.flush({
        code: 200,
        hasSucceeded: true,
        result: {
          id: 2,
          token: dummyValidToken,
          expiresInSeconds: 7200
        },
        errors: null,
      errorMessage: null
    });

    mockedHttp.expectOne(req => {
      expect(req.url).toEqual('api/account/user');
      expect(req.headers.get('Authorization')).toEqual(`Bearer ${dummyValidToken}`);
      return true;
    });

    mockedHttp.verify();

    const tokenObj:ITokenModel = getService(TokenService).get();
    expect(tokenObj).not.toBeNull();
    expect(tokenObj.token).toEqual(dummyValidToken);

  });

  it('should show error on server error', () => {
    usernameCtrl.setValue("admin");
    passwordCtrl.setValue("password1");

    loginComp.submit();

    const request = mockedHttp.expectOne('api/account/signin');
    request.flush({code: 400, hasSucceeded: false, errors: {username: ['用户名或密码错误']}, errorMessage: '用户名或密码错误', result: null});
    mockedHttp.verify();

    expect(loginComp.error).toEqual('用户名或密码错误');
  });
});

