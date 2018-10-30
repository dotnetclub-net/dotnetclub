// from: https://github.com/angular/angular/issues/12409

import { TestBed, TestModuleMetadata } from '@angular/core/testing';
import { Type, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import {
  SettingsService,
  MenuService,
  ScrollService,
  _HttpClient,
} from '@delon/theme';
import { DelonAuthModule } from '@delon/auth';
import { SharedModule } from '@shared/shared.module';


import { DelonModule } from '../app/delon.module';
import {HttpClientTestingModule, HttpTestingController} from "@angular/common/http/testing";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";

const resetTestingModule = TestBed.resetTestingModule,
  preventAngularFromResetting = () =>
    (TestBed.resetTestingModule = () => TestBed);
const allowAngularToReset = () =>
  (TestBed.resetTestingModule = resetTestingModule);

export const setUpTestBed = (moduleDef: TestModuleMetadata) => {
  beforeAll(done =>
    (async () => {
      resetTestingModule();
      preventAngularFromResetting();

      // region: schemas
      if (!moduleDef.schemas) {
        moduleDef.schemas = [];
      }
      moduleDef.schemas.push(CUSTOM_ELEMENTS_SCHEMA);
      // endregion

      // region: imports
      if (!moduleDef.imports) {
        moduleDef.imports = [];
      }

      moduleDef.imports.push(HttpClientTestingModule);
      moduleDef.imports.push(RouterTestingModule);
      moduleDef.imports.push(HttpClientModule);
      moduleDef.imports.push(DelonModule);
      moduleDef.imports.push(SharedModule);
      moduleDef.imports.push(ReactiveFormsModule);
      moduleDef.imports.push(FormsModule);
      // auth
      moduleDef.imports.push(DelonAuthModule.forRoot());
      // endregion

      // region: providers
      if (!moduleDef.providers) {
        moduleDef.providers = [];
      }
      // load full services
      [SettingsService, MenuService, ScrollService, _HttpClient].forEach(
        (item: any) => {
          if (moduleDef.providers.includes(item)) {
            return;
          }
          moduleDef.providers.push(item);
        },
      );
      // endregion

      TestBed.configureTestingModule(moduleDef);
      await TestBed.compileComponents();

      // prevent Angular from resetting testing module
      TestBed.resetTestingModule = () => TestBed;
    })()
      .then(done)
      .catch(done.fail));

  afterAll(() => allowAngularToReset());
};

/**
 * get service instance
 */
export const getService = <T>(type: Type<T>): T => <T>TestBed.get(type);

export const createComponent = <T>(type: Type<T>): T => TestBed.createComponent(type).debugElement.componentInstance;

export const mockedHttpBackend = (): HttpTestingController => TestBed.get(HttpTestingController);

