import { Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpErrorResponse,
  HttpSentEvent,
  HttpHeaderResponse,
  HttpProgressEvent,
  HttpResponse,
  HttpUserEvent,
} from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { mergeMap, catchError } from 'rxjs/operators';
import { NzMessageService } from 'ng-zorro-antd';
import { _HttpClient } from '@delon/theme';
import { environment } from '@env/environment';
import {ApiResponse} from '../../api-response';

/**
 * 默认HTTP拦截器，其注册细节见 `app.module.ts`
 */
@Injectable()
export class DefaultInterceptor implements HttpInterceptor {
  constructor(private injector: Injector) { }

  get msg(): NzMessageService {
    return this.injector.get(NzMessageService);
  }

  private goTo(url: string) {
    setTimeout(() => this.injector.get(Router).navigateByUrl(url));
  }

  private handleData(
    event: HttpResponse<any> | HttpErrorResponse,
  ): Observable<any> {
    this.injector.get(_HttpClient).end();
    switch (event.status) {
      case 200:
        if (event instanceof HttpResponse) {
          const response: ApiResponse = event.body as ApiResponse;
          if (response.code === 401) {
            this.goTo('/passport/login');
          } else {
            return of(new HttpResponse(Object.assign(event, {body: response})));
          }
        }
        break;
      case 401:
        this.goTo('/passport/login');
        break;
      case 403:
      case 404:
      case 500:
        this.goTo(`/${event.status}`);
        break;
      default:
        if (event instanceof HttpErrorResponse) {
          console.warn(
            '未可知错误，大部分是由于后端不支持CORS或无效配置引起',
            event,
          );
          this.msg.error(event.message);
        }
        break;
    }
    return of(event);
  }

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler,
  ): Observable<
  | HttpSentEvent
  | HttpHeaderResponse
  | HttpProgressEvent
  | HttpResponse<any>
  | HttpUserEvent<any>
  > {
    // 统一加上服务端前缀
    let url = req.url;
    if (!url.startsWith('https://') && !url.startsWith('http://')) {
      url = environment.SERVER_URL + url;
    }

    const newReq = req.clone({
      url: url,
    });
    return next.handle(newReq).pipe(
      mergeMap((event: any) => {
        if (event instanceof HttpResponse && event.status === 200)
          return this.handleData(event);
        return of(event);
      }),
      catchError((err: HttpErrorResponse) => this.handleData(err)),
    );
  }
}
