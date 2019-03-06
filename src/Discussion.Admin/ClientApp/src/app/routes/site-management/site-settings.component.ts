import {Component, OnInit} from '@angular/core';
import { _HttpClient } from '@delon/theme';
import {ApiResponse} from "../../api-response";
import {NzMessageService} from "ng-zorro-antd";
import {SiteSettings} from "./site";


@Component({
  selector: 'app-site-settings',
  templateUrl: './site-settings.component.html'
})


export class SiteSettingsComponent implements OnInit {

  siteSettings: SiteSettings;

  constructor(private _httpClient: _HttpClient,
              private _msg: NzMessageService) { }

  getSiteSettings(){
    this._httpClient.get<ApiResponse>('api/settings')
      .subscribe(data => {
        if(data.code === 200){
          this.siteSettings = <SiteSettings>data.result;
          if(this.siteSettings === null){
            this.siteSettings = new SiteSettings();
          }
        }else{
          this._msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this._msg.error(err);
      });
  }

  updateSettings(){
    if(!window.confirm('确定要保存站点设置吗？\n将立即被应用到线上站点。')){
      return;
    }

    this._httpClient.put('api/settings', this.siteSettings)
      .subscribe((data: ApiResponse) => {
        if(data.code === 200){
          this._msg.success('已更新站点设置。');
          this.getSiteSettings();
        }else{
          this._msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this._msg.error(err);
      });
  }

  ngOnInit() {
    this.getSiteSettings();
  }

}
