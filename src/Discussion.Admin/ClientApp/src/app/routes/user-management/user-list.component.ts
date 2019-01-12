import {Component, OnInit} from '@angular/core';
import { _HttpClient } from '@delon/theme';
import {UserSummary} from './user';
import {ApiResponse} from "../../api-response";
import {STChange, STColumn, STPage} from "@delon/abc";
import {NzMessageService} from "ng-zorro-antd";
import {Router} from "@angular/router";
import {Paged, Paging} from "@core/pagination";


@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
})


export class UserListComponent implements OnInit {

  users: UserSummary[] = [];
  paging: Paging = new Paging();
  loading: boolean = false;
  error: any = null;

  stPagingOptions: STPage = {
    front: false
  };
  columns: STColumn[] = [
    { title: '编号', index: 'id', width: '10%' },
    { title: '头像', index: 'avatarUrl', width: '15%',
      format: (user: UserSummary)=>{
        return `<img src="${user.avatarUrl}" alt="用户头像" class="user-avatar"/>`
      }
    },
    { title: '用户名', index: 'loginName', width: '25%' },
    { title: '显示名称', index: 'displayName', width: '35%' }
  ];

  constructor(private httpClient: _HttpClient, private msg: NzMessageService, private router: Router) { }

  dispatchClick( event: STChange ){
    if(event.pi !== this.paging.currentPage){
      this.getUsers(event.pi);
    }
  }


  getUsers(page : number){
    this.error = null;
    this.loading = true;

    this.httpClient.get<ApiResponse>('api/discussion-users?page=' + page)
      .subscribe(data => {
        this.loading = false;

        if(data.code === 200){
          const topicResult = <Paged<UserSummary>>data.result;

          this.users = topicResult.items;
          this.paging = topicResult.paging;
        }else{
          this.msg.error(data.errorMessage);
        }
      }, (err: any) => {
        this.loading = false;
        this.msg.error(err);
      });
  }


  ngOnInit() {
    this.getUsers(1);
  }

}
