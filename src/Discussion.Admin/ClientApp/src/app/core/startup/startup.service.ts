import { Injectable, Injector, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MenuService, SettingsService, TitleService, ALAIN_I18N_TOKEN } from '@delon/theme';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { ACLService } from '@delon/acl';

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
    private httpClient: HttpClient,
    private injector: Injector
  ) { }

  private setupBasicInformation(resolve: any, reject: any) {
    this.httpClient.get('api/account/user')
      .subscribe(res => {
        const user: any = {
          name: res['userName'],
          avatar: './assets/tmp/img/avatar.jpg',
          email: 'jijie.chen@someplace.com'
        };
        this.settingService.setUser(user);
      });

    const app: any = {
      name: `dotnetClub`,
      description: `.NET Core club 管理中心`
    };
    this.settingService.setApp(app);
    this.aclService.setFull(true);

    this.menuService.add([
      {
        text: '主导航',
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
          }
        ]
      },
      {
        text: '网站管理',
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
          }
        ]
      }
    ]);

    this.titleService.suffix = app.name;

    resolve({});
  }

  load(): Promise<any> {
    return new Promise((resolve, reject) => {
      this.setupBasicInformation(resolve, reject);
    });
  }
}
