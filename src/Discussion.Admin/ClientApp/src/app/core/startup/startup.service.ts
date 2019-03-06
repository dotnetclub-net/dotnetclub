import { Injectable, Injector, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MenuService, SettingsService, TitleService, ALAIN_I18N_TOKEN } from '@delon/theme';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { ACLService } from '@delon/acl';
import {ApiResponse} from "../../api-response";
import {SiteSettings} from "../../routes/site-management/site";
import {Observable} from "rxjs";
import { zip } from 'rxjs';

/**
 * 用于应用启动时
 * 一般用来获取应用所需要的基础数据等
 */
@Injectable()
export class StartupService {
  constructor(
    private menuService: MenuService,
    private settingService: SettingsService,
    private aclService: ACLService,
    private titleService: TitleService,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
    private httpClient: HttpClient
  ) { }

  private setupBasicInformation(resolve: any, reject: any) {
    this.menuService.add([
      {
        text: '论坛管理',
        group: true,
        children: [
          {
            text: '管理面板',
            link: '/dashboard',
            icon: 'anticon anticon-line-chart'
          },
          {
            text: '话题管理',
            link: '/topics',
            icon: 'anticon anticon-solution'
          },
          {
            text: '用户管理',
            link: '/users',
            icon: 'anticon anticon-team'
          },
          {
            text: '站点设置',
            link: '/site-settings',
            icon: 'anticon anticon-setting'
          }
        ]
      }
    ]);


    zip(this.httpClient.get('api/account/user'),
      this.httpClient.get<ApiResponse>('api/settings'))
      .subscribe((results : any[]) =>{
        const userResponse = results[0];
        if(userResponse.code === 200) {
          const userInfo = results[0];
          const user: any = {
            name: userInfo.userName,
            id: userInfo.id,
            avatar: './assets/tmp/img/avatar.jpg',
            email: 'admin@dotnetclub.net'
          };
          this.settingService.setUser(user);
        }

        const app: any = {
          name: `dotnetClub`,
          description: `.NET Core club 管理中心`,
          clubHostName: ''
        };
        const settingsResponse = results[1];
        if(settingsResponse.code === 200){
          let siteSettings: SiteSettings = <SiteSettings>settingsResponse.result;
          if(siteSettings === null){
            siteSettings = new SiteSettings();
          }

          app.clubHostName = siteSettings.publicHostName;
        }

        this.settingService.setApp(app);
        this.titleService.suffix = app.name;
        this.aclService.setFull(true);
      });

    resolve({});
  }

  load(): Promise<any> {
    return new Promise((resolve, reject) => {
      this.setupBasicInformation(resolve, reject);
    });
  }
}
