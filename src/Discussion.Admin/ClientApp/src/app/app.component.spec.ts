import { TestBed, TestModuleMetadata } from '@angular/core/testing';
import { APP_BASE_HREF } from '@angular/common';
import {mockedHttpBackend, setUpTestBed} from '@testing/common.spec';

import { AppComponent } from './app.component';
import {HttpClient} from "@angular/common/http";
import {_HttpClient} from "@delon/theme";
import {Observable} from "rxjs";
import {Data} from "@angular/router";

describe('Component: App', () => {
  setUpTestBed(<TestModuleMetadata>{
    declarations: [AppComponent],
    providers: [{ provide: APP_BASE_HREF, useValue: '/' }],
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const comp = fixture.debugElement.componentInstance;
    expect(comp).toBeTruthy();
  });


  it('should test http request by HttpClient class', () => {
    // @ts-ignore
    verifyHttpIsWorking(HttpClient);
  });

  it('should test http request by _HttpClient class', () => {
    // @ts-ignore
    verifyHttpIsWorking(_HttpClient);
  });


  interface HttpGetAbility {
    get<T>(url: any, params?: any, options?: any): Observable<T>;
  }

  // This test case is copied from https://angular.io/guide/http#expecting-and-answering-requests
  function verifyHttpIsWorking<T extends HttpGetAbility>(type: T) {
    let http: T = TestBed.get(type);
    let mockHttp = mockedHttpBackend();

    const testUrl: string = "/data";
    const testData: Data = {name: 'Test Data'};

    http.get<Data>(testUrl)
      .subscribe(data =>
        expect(data).toEqual(testData)
      );

    const req = mockHttp.expectOne('/data');
    expect(req.request.method).toEqual('GET');
    req.flush(testData);

    mockHttp.verify();
  }
});
