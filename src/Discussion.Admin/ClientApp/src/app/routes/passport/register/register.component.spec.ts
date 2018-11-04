
import {TestModuleMetadata} from '@angular/core/testing';
import {APP_BASE_HREF} from '@angular/common';
import {createComponent, mockedHttpBackend, setUpTestBed} from '@testing/common.spec';

import {UserRegisterComponent} from './register.component';
import {FormControl} from "@angular/forms";
import { cases } from 'jasmine-parameterized';

describe('Component: UserRegister', () => {
  setUpTestBed(<TestModuleMetadata>{
    declarations: [UserRegisterComponent],
    providers: [{ provide: APP_BASE_HREF, useValue: '/' }],
  });

    let registerComp : UserRegisterComponent;
    let usernameCtrl : FormControl;
    let passwordCtrl : FormControl;
    let passwordConfirmCtrl : FormControl;
    let mockedHttp;

    beforeAll(() => {
        mockedHttp = mockedHttpBackend();

        registerComp = createComponent(UserRegisterComponent);
        usernameCtrl = registerComp.username as FormControl;
        passwordCtrl = registerComp.password as FormControl;
        passwordConfirmCtrl = registerComp.confirm as FormControl;
    });


  it('should create user register component', () => {
    expect(registerComp).not.toBeNull();
  });

  it('should register user with valid username and password', () => {
    usernameCtrl.setValue("admin");
    passwordCtrl.setValue("password1");
    passwordConfirmCtrl.setValue("password1");

    registerComp.submit();
    const request = mockedHttp.expectOne(req => {
      expect(req.method).toEqual('POST');
      expect(req.url).toEqual('api/account/register');
      expect(req.body).toEqual({
        userName: 'admin',
        password: 'password1'
      });

      return true;
    });

    request.flush({code: 200, hasSucceeded: true, errors: null, errorMessage: null, result: null});
    mockedHttp.verify();
  });

  it('should show error on server error', () => {
    usernameCtrl.setValue("admin");
    passwordCtrl.setValue("password1");
    passwordConfirmCtrl.setValue("password1");

    registerComp.submit();

    const request = mockedHttp.expectOne(req =>  true);
    request.flush({code: 400, hasSucceeded: false, errors: {username: ['用户名已被占用']}, errorMessage: '用户名已被占用', result: null});
    mockedHttp.verify();

    expect(registerComp.error).toEqual('用户名已被占用');
  });

  it('should show login message 401 response', () => {
    usernameCtrl.setValue("admin");
    passwordCtrl.setValue("password1");
    passwordConfirmCtrl.setValue("password1");

    registerComp.submit();
    const request = mockedHttp.expectOne(req =>  true);
    request.flush({code: 401, hasSucceeded: false, errors: null, errorMessage: null, result: null});
    mockedHttp.verify();

    expect(registerComp.error).toEqual('需要登录后才能继续操作');
  });

  cases([
    "somepassword",
    "111111",
    "password",
    "AAABBB",
    "^&#%&@(#!!",
    "35425325",
    "somepassword"
  ]).it('should validate invalid password', () => {
    passwordCtrl.setValue("somepassword");
    const errors = UserRegisterComponent.checkPasswordRules(passwordCtrl);

    expect(errors).toBeTruthy();
    expect(errors["rules"]).toEqual('请至少包含两种：大写、小写、字符，特殊字符');
  });

  cases([
    "password1",
    "11111a",
    "456!sdakj",
    "AAABBa",
    "^&#%&@(45",
    "3542532L5",
    "Qpassword"
  ]).it('should validate valid password', () => {
    passwordCtrl.setValue("passwo1");
    const errors = UserRegisterComponent.checkPasswordRules(passwordCtrl);

    expect(errors).toBeNull();
  });

});

