
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

    beforeAll(() => {
        registerComp = createComponent(UserRegisterComponent);
    });


  it('should create user register component', () => {
    expect(registerComp).not.toBeNull();
  });

  it('should register user with valid username and password', () => {
    const mockedHttp = mockedHttpBackend();

    const usernameCtrl = registerComp.form.controls['username'] as FormControl;
    const passwordCtrl = registerComp.form.controls['password'] as FormControl;
    const passwordConfirmCtrl = registerComp.form.controls['confirm'] as FormControl;
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

  cases([
    "somepassword",
    "111111",
    "password",
    "AAABBB",
    "^&#%&@(#!!",
    "35425325",
    "somepassword"
  ]).it('should validate invalid password', () => {
    const passwordCtrl = registerComp.form.controls['password'] as FormControl;
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
    const passwordCtrl = registerComp.form.controls['password'] as FormControl;
    passwordCtrl.setValue("passwo1");
    const errors = UserRegisterComponent.checkPasswordRules(passwordCtrl);

    expect(errors).toBeNull();
  });

});

